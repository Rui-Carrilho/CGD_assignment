using System.Globalization;
using CreditAssessment.Core;
using Xunit;

namespace CreditAssessment.Tests;

public sealed class CreditEvaluatorTests
{
    private readonly CreditEvaluator evaluator = new();

    [Theory]
    [InlineData("A", Decision.Approved, "166.67", "14.67", 0)]
    [InlineData("B", Decision.ManualReview, "416.67", "45.83", 2)]
    [InlineData("C", Decision.Refused, "208.33", "16.94", 1)]
    [InlineData("D", Decision.Refused, "694.44", "82.87", 2)]
    public void Assignment_scenarios(
        string scenario,
        Decision expectedDecision,
        string expectedPayment,
        string expectedEffort,
        int expectedReasons)
    {
        var result = evaluator.Evaluate(Scenarios.All[scenario]);

        Assert.Equal(expectedDecision, result.Decision);
        Assert.Equal(expectedReasons, result.Findings.Count);

        Assert.NotNull(result.Indicators);

        Assert.Equal(
            decimal.Parse(expectedPayment, CultureInfo.InvariantCulture),
            decimal.Round(result.Indicators.EstimatedInstalment, 2));

        Assert.Equal(
            decimal.Parse(expectedEffort, CultureInfo.InvariantCulture),
            decimal.Round(result.Indicators.EffortRate, 2));
    }

    [Theory]
    [InlineData("4200", Decision.Approved)]       // Exactly 35%
    [InlineData("4200.01", Decision.ManualReview)] // Just above 35%
    [InlineData("6000", Decision.ManualReview)]   // Exactly 50%
    [InlineData("6000.01", Decision.Refused)]     // Just above 50%
    public void Effort_rate_boundaries(
        string amount,
        Decision expectedDecision)
    {
        var input = Scenarios.All["A"] with
        {
            MonthlyIncome = "1000",
            ExistingInstalments = "0",
            RequestedAmount = amount,
            TermMonths = "12"
        };

        var result = evaluator.Evaluate(input);

        Assert.Equal(expectedDecision, result.Decision);
    }

    [Fact]
    public void Zero_income_is_invalid_under_the_written_rules()
    {
        var input = Scenarios.All["A"] with
        {
            MonthlyIncome = "0",
            Employment = "Desempregado"
        };

        var result = evaluator.Evaluate(input);

        Assert.Equal(Decision.Invalid, result.Decision);
        Assert.Null(result.Indicators);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == "INCOME_INVALID");
    }

    [Fact]
    public void Refusal_takes_priority_over_manual_review()
    {
        var input = Scenarios.All["A"] with
        {
            CreditIncidents = "Sim",
            RequestedAmount = "60000"
        };

        var result = evaluator.Evaluate(input);

        Assert.Equal(Decision.Refused, result.Decision);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == "CREDIT_INCIDENTS");
        Assert.Contains(
            result.Findings,
            finding => finding.Code == "HIGH_AMOUNT");
    }

    [Fact]
    public void Nif_with_a_newline_is_invalid()
    {
        var input = Scenarios.All["A"] with
        {
            Nif = "123456789\n"
        };

        var result = evaluator.Evaluate(input);

        Assert.Equal(Decision.Invalid, result.Decision);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == "NIF_INVALID");
    }

    [Theory]
    [InlineData("12", Decision.Approved)]
    [InlineData("13", Decision.ManualReview)]
    public void Age_at_end_compares_months(
        string term,
        Decision expectedDecision)
    {
        var input = Scenarios.All["A"] with
        {
            Age = "74",
            TermMonths = term,
            RequestedAmount = "100"
        };

        var result = evaluator.Evaluate(input);

        Assert.Equal(expectedDecision, result.Decision);
    }
}