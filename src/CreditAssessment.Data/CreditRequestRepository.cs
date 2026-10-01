using System.Data;
using System.Text.Json;
using CreditAssessment.Core;
using Microsoft.Data.SqlClient;

namespace CreditAssessment.Data;

public sealed class CreditRequestRepository
{
    public async Task<Guid> SaveAsync(
        RequestInput input,
        Evaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(evaluation);

        Guid id = Guid.NewGuid();
        DateTimeOffset submittedAt = DateTimeOffset.UtcNow;

        string inputJson = JsonSerializer.Serialize(input);
        string evaluationJson = JsonSerializer.Serialize(evaluation);

        await using SqlConnection connection =
            await SqlServerConnectionFactory.OpenAsync();

        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync();

        await using (SqlCommand requestCommand = new(
            """
            INSERT INTO dbo.CreditRequest
                (Id, Nif, InputJson, EvaluationJson,
                CurrentDecision, SubmittedAt)
            VALUES
                (@Id, @Nif, @InputJson, @EvaluationJson,
                @CurrentDecision, @SubmittedAt);
            """,
            connection,
            transaction))
        {
            requestCommand.Parameters.Add(
                "@Id", SqlDbType.UniqueIdentifier).Value = id;

            requestCommand.Parameters.Add(
                "@Nif", SqlDbType.NVarChar, 9).Value =
                CreditEvaluator.IsValidNif(input.Nif)
                    ? input.Nif!
                    : DBNull.Value;

            requestCommand.Parameters.Add(
                "@InputJson", SqlDbType.NVarChar, -1).Value =
                inputJson;

            requestCommand.Parameters.Add(
                "@EvaluationJson", SqlDbType.NVarChar, -1).Value =
                evaluationJson;

            requestCommand.Parameters.Add(
                "@CurrentDecision", SqlDbType.TinyInt).Value =
                (byte)evaluation.Decision;

            requestCommand.Parameters.Add(
                "@SubmittedAt", SqlDbType.DateTimeOffset).Value =
                submittedAt;

            await requestCommand.ExecuteNonQueryAsync();
        }

        for (int index = 0; index < evaluation.Findings.Count; index++)
        {
            Finding finding = evaluation.Findings[index];

            await using SqlCommand reasonCommand = new(
                """
                INSERT INTO dbo.EvaluationReason
                    (RequestId, SequenceNumber, Code, Severity, Message)
                VALUES
                    (@RequestId, @SequenceNumber, @Code,
                    @Severity, @Message);
                """,
                connection,
                transaction);

            reasonCommand.Parameters.Add(
                "@RequestId", SqlDbType.UniqueIdentifier).Value = id;

            reasonCommand.Parameters.Add(
                "@SequenceNumber", SqlDbType.Int).Value = index;

            reasonCommand.Parameters.Add(
                "@Code", SqlDbType.NVarChar, 100).Value = finding.Code;

            reasonCommand.Parameters.Add(
                "@Severity", SqlDbType.TinyInt).Value =
                (byte)finding.Severity;

            reasonCommand.Parameters.Add(
                "@Message", SqlDbType.NVarChar, 1000).Value =
                finding.Message;

            await reasonCommand.ExecuteNonQueryAsync();
        }

        await using (SqlCommand historyCommand = new(
            """
            INSERT INTO dbo.StatusHistory
                (RequestId, PreviousDecision, NewDecision,
                ChangedAt, Actor, Justification)
            VALUES
                (@RequestId, NULL, @NewDecision,
                @ChangedAt, @Actor, @Justification);
            """,
            connection,
            transaction))
        {
            historyCommand.Parameters.Add(
                "@RequestId", SqlDbType.UniqueIdentifier).Value = id;

            historyCommand.Parameters.Add(
                "@NewDecision", SqlDbType.TinyInt).Value =
                (byte)evaluation.Decision;

            historyCommand.Parameters.Add(
                "@ChangedAt", SqlDbType.DateTimeOffset).Value =
                submittedAt;

            historyCommand.Parameters.Add(
                "@Actor", SqlDbType.NVarChar, 100).Value =
                "System";

            historyCommand.Parameters.Add(
                "@Justification", SqlDbType.NVarChar, 500).Value =
                "Automatic evaluation";

            await historyCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        return id;
    }

    public async Task ResolveManualReviewAsync(
        Guid id,
        Decision newDecision,
        string actor,
        string justification)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "A request ID is required.",
                nameof(id));

        if (newDecision is not
            (Decision.Approved or Decision.Refused))
        {
            throw new ArgumentException(
                "A manual review may only be approved or refused.",
                nameof(newDecision));
        }

        if (string.IsNullOrWhiteSpace(actor) || actor.Length > 100)
        {
            throw new ArgumentException(
                "An actor of at most 100 characters is required.",
                nameof(actor));
        }

        if (string.IsNullOrWhiteSpace(justification) ||
            justification.Length > 500)
        {
            throw new ArgumentException(
                "A justification of at most 500 characters is required.",
                nameof(justification));
        }

        await using SqlConnection connection =
            await SqlServerConnectionFactory.OpenAsync();

        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync();

        await using (SqlCommand updateCommand = new(
            """
            UPDATE dbo.CreditRequest
            SET CurrentDecision = @NewDecision
            WHERE Id = @Id
            AND CurrentDecision = @ManualReview;
            """,
            connection,
            transaction))
        {
            updateCommand.Parameters.Add(
                "@Id", SqlDbType.UniqueIdentifier).Value = id;

            updateCommand.Parameters.Add(
                "@NewDecision", SqlDbType.TinyInt).Value =
                (byte)newDecision;

            updateCommand.Parameters.Add(
                "@ManualReview", SqlDbType.TinyInt).Value =
                (byte)Decision.ManualReview;

            int changedRows =
                await updateCommand.ExecuteNonQueryAsync();

            if (changedRows != 1)
            {
                await transaction.RollbackAsync();

                throw new InvalidOperationException(
                    "The request does not exist or is no longer awaiting manual review.");
            }
        }

        await using (SqlCommand historyCommand = new(
            """
            INSERT INTO dbo.StatusHistory
                (RequestId, PreviousDecision, NewDecision,
                ChangedAt, Actor, Justification)
            VALUES
                (@RequestId, @PreviousDecision, @NewDecision,
                @ChangedAt, @Actor, @Justification);
            """,
            connection,
            transaction))
        {
            historyCommand.Parameters.Add(
                "@RequestId", SqlDbType.UniqueIdentifier).Value = id;

            historyCommand.Parameters.Add(
                "@PreviousDecision", SqlDbType.TinyInt).Value =
                (byte)Decision.ManualReview;

            historyCommand.Parameters.Add(
                "@NewDecision", SqlDbType.TinyInt).Value =
                (byte)newDecision;

            historyCommand.Parameters.Add(
                "@ChangedAt", SqlDbType.DateTimeOffset).Value =
                DateTimeOffset.UtcNow;

            historyCommand.Parameters.Add(
                "@Actor", SqlDbType.NVarChar, 100).Value =
                actor.Trim();

            historyCommand.Parameters.Add(
                "@Justification", SqlDbType.NVarChar, 500).Value =
                justification.Trim();

            await historyCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task<StoredAssessment?> GetAsync(Guid id)
    {
        await using SqlConnection connection =
            await SqlServerConnectionFactory.OpenAsync();

        await using SqlCommand command = new(
            """
            SELECT InputJson, EvaluationJson,
                   CurrentDecision, SubmittedAt
            FROM dbo.CreditRequest
            WHERE Id = @Id;
            """,
            connection);

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;

        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        RequestInput input =
            JsonSerializer.Deserialize<RequestInput>(reader.GetString(0))
            ?? throw new InvalidOperationException("Stored input is empty.");

        Evaluation evaluation =
            JsonSerializer.Deserialize<Evaluation>(reader.GetString(1))
            ?? throw new InvalidOperationException("Stored evaluation is empty.");

        Decision currentDecision = (Decision)reader.GetByte(2);
        DateTimeOffset submittedAt = reader.GetDateTimeOffset(3);

        return new StoredAssessment(
            id, input, evaluation, currentDecision, submittedAt);
    }

    public async Task<IReadOnlyList<RequestSummary>> ListRecentAsync(
        int take = 50)
    {
        if (take < 1 || take > 100)
            throw new ArgumentOutOfRangeException(nameof(take));

        await using SqlConnection connection =
            await SqlServerConnectionFactory.OpenAsync();

        await using SqlCommand command = new(
            """
            SELECT TOP (@Take)
                Id, Nif, CurrentDecision, SubmittedAt
            FROM dbo.CreditRequest
            ORDER BY SubmittedAt DESC, Id DESC;
            """,
            connection);

        command.Parameters.Add("@Take", SqlDbType.Int).Value = take;

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        var requests = new List<RequestSummary>();

        while (await reader.ReadAsync())
        {
            requests.Add(new RequestSummary(
                reader.GetGuid(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                (Decision)reader.GetByte(2),
                reader.GetDateTimeOffset(3)));
        }

        return requests;
    }

    public async Task<IReadOnlyList<ManualReviewQueueItem>>
        ListManualReviewQueueAsync(int take = 100)
    {
        if (take < 1 || take > 100)
            throw new ArgumentOutOfRangeException(nameof(take));

        await using SqlConnection connection =
            await SqlServerConnectionFactory.OpenAsync();

        await using SqlCommand command = new(
            """
            SELECT TOP (@Take)
                Id,
                Nif,
                InputJson,
                EvaluationJson,
                SubmittedAt
            FROM dbo.CreditRequest
            WHERE CurrentDecision = @ManualReview
            ORDER BY SubmittedAt, Id;
            """,
            connection);

        command.Parameters.Add("@Take", SqlDbType.Int).Value = take;

        command.Parameters.Add(
            "@ManualReview",
            SqlDbType.TinyInt).Value = (byte)Decision.ManualReview;

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        var requests = new List<ManualReviewQueueItem>();

        while (await reader.ReadAsync())
        {
            RequestInput input =
                JsonSerializer.Deserialize<RequestInput>(reader.GetString(2))
                ?? throw new InvalidOperationException(
                    "Stored request input is empty.");

            Evaluation evaluation =
                JsonSerializer.Deserialize<Evaluation>(reader.GetString(3))
                ?? throw new InvalidOperationException(
                    "Stored evaluation is empty.");

            requests.Add(new ManualReviewQueueItem(
                reader.GetGuid(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                input.RequestedAmount,
                reader.GetDateTimeOffset(4),
                evaluation.Findings));
        }

        return requests;
    }

    public async Task<IReadOnlyList<StatusHistoryEntry>> GetHistoryAsync(
        Guid requestId)
    {
        await using SqlConnection connection =
            await SqlServerConnectionFactory.OpenAsync();

        await using SqlCommand command = new(
            """
            SELECT
                Id, PreviousDecision, NewDecision,
                ChangedAt, Actor, Justification
            FROM dbo.StatusHistory
            WHERE RequestId = @RequestId
            ORDER BY Id;
            """,
            connection);

        command.Parameters.Add(
            "@RequestId", SqlDbType.UniqueIdentifier).Value = requestId;

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        var entries = new List<StatusHistoryEntry>();

        while (await reader.ReadAsync())
        {
            entries.Add(new StatusHistoryEntry(
                reader.GetInt64(0),
                reader.IsDBNull(1)
                    ? null
                    : (Decision)reader.GetByte(1),
                (Decision)reader.GetByte(2),
                reader.GetDateTimeOffset(3),
                reader.GetString(4),
                reader.GetString(5)));
        }

        return entries;
    }
}