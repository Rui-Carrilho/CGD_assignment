namespace CreditAssessment.Core;

public static class Scenarios
{
    public static IReadOnlyDictionary<string, RequestInput> All { get; } =
        new Dictionary<string, RequestInput>
        {
            ["A"] = Make("35", "2500", "200", "10000",
                         "60", "Efetivo", "Não"),

            ["B"] = Make("42", "2000", "500", "20000",
                         "48", "ContratoPrazo", "Não"),

            ["C"] = Make("40", "3000", "300", "15000",
                         "72", "Efetivo", "Sim"),

            ["D"] = Make("30", "1200", "300", "25000",
                         "36", "Efetivo", "Não")
        };

    private static RequestInput Make(
        string age,
        string income,
        string existing,
        string amount,
        string term,
        string employment,
        string incidents) =>
        new()
        {
            Nif = "123456789",
            Age = age,
            MonthlyIncome = income,
            ExistingInstalments = existing,
            RequestedAmount = amount,
            TermMonths = term,
            Employment = employment,
            CreditIncidents = incidents
        };
}