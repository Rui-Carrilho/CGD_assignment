namespace CreditAssessment.Data;

public sealed record RefusalReasonCount(
    string Code,
    string Message,
    long Count);

public sealed record RepeatCustomerCount(
    string Nif,
    long Count);

public sealed record ReportSnapshot(
    long Approved,
    long ManualReview,
    long Refused,
    long Invalid,
    IReadOnlyList<RefusalReasonCount> TopRefusalReasons,
    IReadOnlyList<RepeatCustomerCount> RepeatCustomers,
    long ManualToApproved);