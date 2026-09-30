-- 1. Requests whose current status is approved.
SELECT COUNT(*) AS ApprovedCount
FROM dbo.CreditRequest
WHERE CurrentDecision = 0;
GO

-- 2. Requests in each other current status.
SELECT
    CASE CurrentDecision
        WHEN 1 THEN N'ANÁLISE MANUAL'
        WHEN 2 THEN N'RECUSADO'
        WHEN 3 THEN N'PEDIDO INVÁLIDO'
    END AS Status,
    COUNT(*) AS RequestCount
FROM dbo.CreditRequest
WHERE CurrentDecision IN (1, 2, 3)
GROUP BY CurrentDecision
ORDER BY CurrentDecision;
GO

-- 3. Most frequent refusal reason(s), including ties.
WITH ReasonCounts AS
(
    SELECT
        e.Code,
        e.Message,
        COUNT(*) AS RefusalCount
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
    (SELECT MAX(RefusalCount) FROM ReasonCounts);
GO

-- 4. Customers with multiple requests in the preceding month.
-- Here "last month" means the rolling month ending now.
SELECT
    Nif,
    COUNT(*) AS RequestCount
FROM dbo.CreditRequest
WHERE Nif IS NOT NULL
  AND SubmittedAt >= DATEADD(MONTH, -1, SYSDATETIMEOFFSET())
GROUP BY Nif
HAVING COUNT(*) > 1
ORDER BY RequestCount DESC, Nif;
GO

-- 5. Initially manual requests subsequently approved.
SELECT COUNT(*) AS ManualToApprovedCount
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
GO