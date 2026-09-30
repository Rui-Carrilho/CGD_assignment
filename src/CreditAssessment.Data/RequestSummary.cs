using CreditAssessment.Core;

namespace CreditAssessment.Data;

public sealed record RequestSummary(
    Guid Id,
    string? Nif,
    Decision CurrentDecision,
    DateTimeOffset SubmittedAt);