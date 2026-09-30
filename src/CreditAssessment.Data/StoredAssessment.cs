using CreditAssessment.Core;

namespace CreditAssessment.Data;

public sealed record StoredAssessment(
    Guid Id,
    RequestInput Input,
    Evaluation Evaluation,
    Decision CurrentDecision,
    DateTimeOffset SubmittedAt);