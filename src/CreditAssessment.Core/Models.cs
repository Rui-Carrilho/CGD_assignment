namespace CreditAssessment.Core;

public enum Decision
{
    Approved = 0,
    ManualReview = 1,
    Refused = 2,
    Invalid = 3
}

public sealed record RequestInput
{
    public string? Nif { get; init; }
    public string? Age { get; init; }
    public string? MonthlyIncome { get; init; }
    public string? ExistingInstalments { get; init; }
    public string? RequestedAmount { get; init; }
    public string? TermMonths { get; init; }
    public string? Employment { get; init; }
    public string? CreditIncidents { get; init; }
}

public sealed record Finding(
    string Code,
    Decision Severity,
    string Message);

public sealed record Indicators(
    decimal EstimatedInstalment,
    decimal EffortRate,
    decimal AgeAtMaturity,
    decimal RecommendedAmountLimit);

public sealed record Evaluation(
    Decision Decision,
    IReadOnlyList<Finding> Findings,
    Indicators? Indicators,
    string RulesVersion);
