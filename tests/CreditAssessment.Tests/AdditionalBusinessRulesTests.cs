using CreditAssessment.Core;
using Xunit;

namespace CreditAssessment.Tests;

public sealed class AdditionalBusinessRulesTests
{
    private readonly CreditEvaluator evaluator = new();

    [Fact]
    public void Invalid_fields_take_priority_over_refusal()
    {
        var input = Scenarios.All["A"] with
        {
            Age = "17",
            MonthlyIncome = "0",
            RequestedAmount = "0",
            TermMonths = "0",
            CreditIncidents = "Sim"
        };

        Evaluation result = evaluator.Evaluate(input);

        Assert.Equal(Decision.Invalid, result.Decision);
        Assert.Null(result.Indicators);

        string[] codes = result.Findings
            .Select(finding => finding.Code)
            .ToArray();

        Assert.Contains("AGE_INVALID", codes);
        Assert.Contains("INCOME_INVALID", codes);
        Assert.Contains("AMOUNT_INVALID", codes);
        Assert.Contains("TERM_INVALID", codes);

        // Financial and risk rules do not run on invalid input.
        Assert.DoesNotContain("CREDIT_INCIDENTS", codes);
    }

    [Fact]
    public void Unemployed_person_with_valid_income_is_refused()
    {
        var input = Scenarios.All["A"] with
        {
            Employment = "Desempregado"
        };

        Evaluation result = evaluator.Evaluate(input);

        Assert.Equal(Decision.Refused, result.Decision);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == "UNEMPLOYED");
    }

    [Theory]
    [InlineData("50000", Decision.Approved, false)]
    [InlineData("50000.01", Decision.ManualReview, true)]
    public void High_amount_rule_starts_above_50000(
        string amount,
        Decision expectedDecision,
        bool hasHighAmountReason)
    {
        var input = Scenarios.All["A"] with
        {
            MonthlyIncome = "10000",
            ExistingInstalments = "0",
            RequestedAmount = amount,
            TermMonths = "120"
        };

        Evaluation result = evaluator.Evaluate(input);

        Assert.Equal(expectedDecision, result.Decision);
        Assert.Equal(
            hasHighAmountReason,
            result.Findings.Any(
                finding => finding.Code == "HIGH_AMOUNT"));
    }

    [Theory]
    [InlineData("200000", false)]
    [InlineData("200000.01", true)]
    public void Recommended_limit_starts_above_twenty_times_income(
        string amount,
        bool hasLimitReason)
    {
        var input = Scenarios.All["A"] with
        {
            MonthlyIncome = "10000",
            ExistingInstalments = "0",
            RequestedAmount = amount,
            TermMonths = "120"
        };

        Evaluation result = evaluator.Evaluate(input);

        Assert.Equal(
            hasLimitReason,
            result.Findings.Any(
                finding => finding.Code == "AMOUNT_ABOVE_INCOME_LIMIT"));
    }

    [Fact]
    public void Dot_decimal_input_is_accepted()
    {
        var input = Scenarios.All["A"] with
        {
            MonthlyIncome = "2500.50",
            ExistingInstalments = "200.25",
            RequestedAmount = "10000.25"
        };

        Evaluation result = evaluator.Evaluate(input);

        Assert.NotEqual(Decision.Invalid, result.Decision);
        Assert.NotNull(result.Indicators);
    }
}