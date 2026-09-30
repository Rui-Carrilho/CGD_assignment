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

        await using SqlCommand command = new(
            """
            INSERT INTO dbo.CreditRequest
                (Id, Nif, InputJson, EvaluationJson,
                 CurrentDecision, SubmittedAt)
            VALUES
                (@Id, @Nif, @InputJson, @EvaluationJson,
                 @CurrentDecision, @SubmittedAt);
            """,
            connection);

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;

        command.Parameters.Add("@Nif", SqlDbType.NVarChar, 9).Value =
            CreditEvaluator.IsValidNif(input.Nif)
                ? input.Nif!
                : DBNull.Value;

        command.Parameters.Add("@InputJson", SqlDbType.NVarChar, -1).Value =
            inputJson;

        command.Parameters.Add("@EvaluationJson", SqlDbType.NVarChar, -1).Value =
            evaluationJson;

        command.Parameters.Add("@CurrentDecision", SqlDbType.TinyInt).Value =
            (byte)evaluation.Decision;

        command.Parameters.Add("@SubmittedAt", SqlDbType.DateTimeOffset).Value =
            submittedAt;

        await command.ExecuteNonQueryAsync();
        return id;
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
}