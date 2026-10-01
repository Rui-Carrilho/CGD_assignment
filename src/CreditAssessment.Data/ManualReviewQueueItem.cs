using CreditAssessment.Core;

namespace CreditAssessment.Data;

public sealed record ManualReviewQueueItem(
    Guid Id,
    string? Nif,
    string? RequestedAmount,
    DateTimeOffset SubmittedAt,
    IReadOnlyList<Finding> Findings);