using System.Data;
using Microsoft.Data.SqlClient;

namespace CreditAssessment.Data;

public sealed class ReportRepository
{
    public async Task<ReportSnapshot> GetAsync()
    {
        await using SqlConnection connection =
            await SqlServerConnectionFactory.OpenAsync();

        long approved;
        long manualReview;
        long refused;
        long invalid;

        await using (SqlCommand countsCommand = new(
            """
            SELECT
                COUNT_BIG(CASE WHEN CurrentDecision = 0 THEN 1 END),
                COUNT_BIG(CASE WHEN CurrentDecision = 1 THEN 1 END),
                COUNT_BIG(CASE WHEN CurrentDecision = 2 THEN 1 END),
                COUNT_BIG(CASE WHEN CurrentDecision = 3 THEN 1 END)
            FROM dbo.CreditRequest;
            """,
            connection))
        {
            await using SqlDataReader reader =
                await countsCommand.ExecuteReaderAsync();

            await reader.ReadAsync();

            approved = reader.GetInt64(0);
            manualReview = reader.GetInt64(1);
            refused = reader.GetInt64(2);
            invalid = reader.GetInt64(3);
        }

        var topReasons = new List<RefusalReasonCount>();

        await using (SqlCommand reasonsCommand = new(
            """
            WITH ReasonCounts AS
            (
                SELECT
                    e.Code,
                    e.Message,
                    COUNT_BIG(*) AS RefusalCount
                FROM dbo.CreditRequest AS r
                JOIN dbo.EvaluationReason AS e
                    ON e.RequestId = r.Id
                WHERE r.CurrentDecision = 2
                  AND e.Severity = 2
                GROUP BY e.Code, e.Message
            )
            SELECT Code, Message, RefusalCount
            FROM ReasonCounts
            WHERE RefusalCount =
                (SELECT MAX(RefusalCount) FROM ReasonCounts)
            ORDER BY Code;
            """,
            connection))
        {
            await using SqlDataReader reader =
                await reasonsCommand.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                topReasons.Add(new RefusalReasonCount(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetInt64(2)));
            }
        }

        var repeatCustomers = new List<RepeatCustomerCount>();

        await using (SqlCommand customersCommand = new(
            """
            SELECT
                Nif,
                COUNT_BIG(*) AS RequestCount
            FROM dbo.CreditRequest
            WHERE Nif IS NOT NULL
              AND SubmittedAt >= @Since
            GROUP BY Nif
            HAVING COUNT_BIG(*) > 1
            ORDER BY RequestCount DESC, Nif;
            """,
            connection))
        {
            customersCommand.Parameters.Add(
                "@Since", SqlDbType.DateTimeOffset).Value =
                DateTimeOffset.UtcNow.AddMonths(-1);

            await using SqlDataReader reader =
                await customersCommand.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                repeatCustomers.Add(new RepeatCustomerCount(
                    reader.GetString(0),
                    reader.GetInt64(1)));
            }
        }

        await using SqlCommand transitionsCommand = new(
            """
            SELECT COUNT_BIG(*)
            FROM dbo.CreditRequest AS r
            WHERE r.CurrentDecision = 0
              AND EXISTS
              (
                  SELECT 1
                  FROM dbo.StatusHistory AS initialHistory
                  WHERE initialHistory.RequestId = r.Id
                    AND initialHistory.PreviousDecision IS NULL
                    AND initialHistory.NewDecision = 1
              )
              AND EXISTS
              (
                  SELECT 1
                  FROM dbo.StatusHistory AS approvalHistory
                  WHERE approvalHistory.RequestId = r.Id
                    AND approvalHistory.PreviousDecision = 1
                    AND approvalHistory.NewDecision = 0
              );
            """,
            connection);

        long manualToApproved =
            (long)(await transitionsCommand.ExecuteScalarAsync() ?? 0L);

        return new ReportSnapshot(
            approved,
            manualReview,
            refused,
            invalid,
            topReasons,
            repeatCustomers,
            manualToApproved);
    }
}