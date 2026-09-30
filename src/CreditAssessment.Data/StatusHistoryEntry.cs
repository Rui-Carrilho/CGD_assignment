using CreditAssessment.Core;

namespace CreditAssessment.Data;

public sealed record StatusHistoryEntry(
    long Id,
    Decision? PreviousDecision,
    Decision NewDecision,
    DateTimeOffset ChangedAt,
    string Actor,
    string Justification);