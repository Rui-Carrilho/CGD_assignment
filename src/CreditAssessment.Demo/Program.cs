using System.Globalization;
using CreditAssessment.Core;

var evaluator = new CreditEvaluator();

foreach (var scenario in Scenarios.All)
{
    var result = evaluator.Evaluate(scenario.Value);

    Console.WriteLine($"Scenario {scenario.Key}: {result.Decision}");

    foreach (var finding in result.Findings)
    {
        Console.WriteLine(
            $"  Reason: {finding.Code} — {finding.Message}");
    }

    if (result.Indicators is { } indicators)
    {
        Console.WriteLine(
            $"  Payment: {indicators.EstimatedInstalment.ToString("F2", CultureInfo.InvariantCulture)} €");

        Console.WriteLine(
            $"  Effort: {indicators.EffortRate.ToString("F2", CultureInfo.InvariantCulture)}%");
    }

    Console.WriteLine();
}