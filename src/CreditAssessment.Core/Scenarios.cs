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

    public static IReadOnlyDictionary<string, RequestInput> Additional { get; } =
    new Dictionary<string, RequestInput>
    {
        ["ZERO_INCOME"] = All["A"] with
        {
            MonthlyIncome = "0"
        },

        ["AGE_LIMIT"] = All["A"] with
        {
            Age = "74",
            TermMonths = "13",
            RequestedAmount = "100"
        },

        ["EFFORT_35"] = All["A"] with
        {
            MonthlyIncome = "1000",
            ExistingInstalments = "0",
            RequestedAmount = "4200",
            TermMonths = "12"
        },

        ["EFFORT_OVER_50"] = All["A"] with
        {
            MonthlyIncome = "1000",
            ExistingInstalments = "0",
            RequestedAmount = "6000.01",
            TermMonths = "12"
        },

        ["HIGH_AMOUNT"] = All["A"] with
        {
            MonthlyIncome = "10000",
            ExistingInstalments = "0",
            RequestedAmount = "50000.01",
            TermMonths = "120"
        },

        ["MULTIPLE_RULES"] = All["A"] with
        {
            CreditIncidents = "Sim",
            RequestedAmount = "60000"
        },

        ["UNEMPLOYED"] = All["A"] with
        {
            Employment = "Desempregado"
        },

        ["INCOMPLETE"] = All["A"] with
        {
            Nif = "",
            Age = "",
            MonthlyIncome = "",
            RequestedAmount = "",
            TermMonths = "",
            Employment = "",
            CreditIncidents = ""
        },

        ["AGE_EXACT_75"] = All["A"] with
        {
            Age = "74",
            TermMonths = "12",
            RequestedAmount = "100"
        },

        ["EFFORT_50"] = All["A"] with
        {
            MonthlyIncome = "1000",
            ExistingInstalments = "0",
            RequestedAmount = "6000",
            TermMonths = "12"
        },

        ["AMOUNT_LIMIT"] = All["A"] with
        {
            MonthlyIncome = "1000",
            ExistingInstalments = "0",
            RequestedAmount = "20000.01",
            TermMonths = "120"
        }
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