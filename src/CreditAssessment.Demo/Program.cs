using System.Globalization;
using CreditAssessment.Core;
using CreditAssessment.Data;
using Microsoft.Data.SqlClient;

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

var repository = new CreditRequestRepository();

RequestInput sampleInput = Scenarios.All["A"];
Evaluation sampleEvaluation = evaluator.Evaluate(sampleInput);

Guid savedId = await repository.SaveAsync(
    sampleInput,
    sampleEvaluation);

StoredAssessment? saved = await repository.GetAsync(savedId);

Console.WriteLine($"Saved request: {savedId}");
Console.WriteLine($"Loaded NIF: {saved?.Input.Nif}");
Console.WriteLine($"Loaded decision: {saved?.CurrentDecision}");