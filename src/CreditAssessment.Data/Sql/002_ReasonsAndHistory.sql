IF OBJECT_ID(N'dbo.EvaluationReason', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EvaluationReason
    (
        RequestId UNIQUEIDENTIFIER NOT NULL,
        SequenceNumber INT NOT NULL,
        Code NVARCHAR(100) NOT NULL,
        Severity TINYINT NOT NULL,
        Message NVARCHAR(1000) NOT NULL,

        CONSTRAINT PK_EvaluationReason
            PRIMARY KEY (RequestId, SequenceNumber),

        CONSTRAINT FK_EvaluationReason_Request
            FOREIGN KEY (RequestId)
            REFERENCES dbo.CreditRequest(Id),

        CONSTRAINT CK_EvaluationReason_Severity
            CHECK (Severity BETWEEN 1 AND 3)
    );
END;

IF OBJECT_ID(N'dbo.StatusHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StatusHistory
    (
        Id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_StatusHistory PRIMARY KEY,

        RequestId UNIQUEIDENTIFIER NOT NULL,
        PreviousDecision TINYINT NULL,
        NewDecision TINYINT NOT NULL,
        ChangedAt DATETIMEOFFSET(0) NOT NULL,
        Actor NVARCHAR(100) NOT NULL,
        Justification NVARCHAR(500) NOT NULL,

        CONSTRAINT FK_StatusHistory_Request
            FOREIGN KEY (RequestId)
            REFERENCES dbo.CreditRequest(Id),

        CONSTRAINT CK_StatusHistory_Previous
            CHECK (PreviousDecision IS NULL
                   OR PreviousDecision BETWEEN 0 AND 3),

        CONSTRAINT CK_StatusHistory_New
            CHECK (NewDecision BETWEEN 0 AND 3)
    );

    CREATE INDEX IX_StatusHistory_Request_Id
        ON dbo.StatusHistory (RequestId, Id);
END;

-- Include requests saved during the previous sprint.
INSERT INTO dbo.EvaluationReason
    (RequestId, SequenceNumber, Code, Severity, Message)
SELECT
    r.Id,
    CONVERT(INT, reason.[key]),
    JSON_VALUE(reason.value, '$.Code'),
    CONVERT(TINYINT, JSON_VALUE(reason.value, '$.Severity')),
    JSON_VALUE(reason.value, '$.Message')
FROM dbo.CreditRequest AS r
CROSS APPLY OPENJSON(r.EvaluationJson, '$.Findings') AS reason
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.EvaluationReason AS existing
    WHERE existing.RequestId = r.Id
);

INSERT INTO dbo.StatusHistory
    (RequestId, PreviousDecision, NewDecision,
     ChangedAt, Actor, Justification)
SELECT
    r.Id,
    NULL,
    r.CurrentDecision,
    r.SubmittedAt,
    N'System',
    N'Automatic evaluation'
FROM dbo.CreditRequest AS r
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.StatusHistory AS existing
    WHERE existing.RequestId = r.Id
);