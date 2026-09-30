IF OBJECT_ID(N'dbo.CreditRequest', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CreditRequest
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_CreditRequest PRIMARY KEY,

        Nif NVARCHAR(9) NULL,

        InputJson NVARCHAR(MAX) NOT NULL,
        EvaluationJson NVARCHAR(MAX) NOT NULL,

        CurrentDecision TINYINT NOT NULL,
        SubmittedAt DATETIMEOFFSET(0) NOT NULL,

        CONSTRAINT CK_CreditRequest_InputJson
            CHECK (ISJSON(InputJson) = 1),

        CONSTRAINT CK_CreditRequest_EvaluationJson
            CHECK (ISJSON(EvaluationJson) = 1),

        CONSTRAINT CK_CreditRequest_Decision
            CHECK (CurrentDecision BETWEEN 0 AND 3)
    );

    CREATE INDEX IX_CreditRequest_Nif_SubmittedAt
        ON dbo.CreditRequest (Nif, SubmittedAt);
END;