using System.Globalization;
using System.Text.RegularExpressions;

namespace CreditAssessment.Core;

public sealed partial class CreditEvaluator
{
    public const string Version = "1.0.0";
    public const decimal MaximumMoney = 1_000_000_000_000m;

    [GeneratedRegex(@"\A[0-9]{9}\z", RegexOptions.CultureInvariant)]
    private static partial Regex NifPattern();

    [GeneratedRegex(@"\A[0-9]+([.,][0-9]{1,2})?\z",
        RegexOptions.CultureInvariant)]
    private static partial Regex MoneyPattern();

    [GeneratedRegex(@"\A[0-9]+\z", RegexOptions.CultureInvariant)]
    private static partial Regex IntegerPattern();

    public static bool IsValidNif(string? value) =>
        value is not null && NifPattern().IsMatch(value);

    public Evaluation Evaluate(RequestInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var findings = new List<Finding>();

        void Invalid(string code, string message) =>
            findings.Add(new Finding(code, Decision.Invalid, message));

        // First validate every input. We collect all errors at once.
        if (!IsValidNif(input.Nif))
            Invalid("NIF_INVALID",
                "O NIF deve conter exatamente 9 dígitos (0–9).");

        var age = Integer(
            input.Age, 18, "AGE_INVALID",
            "A idade deve ser um número inteiro igual ou superior a 18.");

        var income = Money(
            input.MonthlyIncome, false, "INCOME_INVALID",
            "O rendimento mensal deve ser superior a zero.");

        var existing = Money(
            input.ExistingInstalments, true, "EXISTING_INVALID",
            "As prestações atuais devem ser iguais ou superiores a zero.");

        var amount = Money(
            input.RequestedAmount, false, "AMOUNT_INVALID",
            "O montante pretendido deve ser superior a zero.");

        var term = Integer(
            input.TermMonths, 1, "TERM_INVALID",
            "O prazo deve ser um número inteiro superior a zero.");

        if (input.Employment is not
            ("Efetivo" or "ContratoPrazo" or "Desempregado"))
        {
            Invalid("EMPLOYMENT_INVALID",
                "Selecione uma situação profissional válida.");
        }

        if (input.CreditIncidents is not ("Sim" or "Não"))
        {
            Invalid("INCIDENTS_INVALID",
                "Indique Sim ou Não para incidentes de crédito.");
        }

        // Do not divide by income or term if validation failed.
        if (findings.Count > 0)
            return new Evaluation(
                Decision.Invalid, findings.AsReadOnly(), null, Version);

        // All values are now valid.
        var payment = amount / term;
        var effort = (existing + payment) / income * 100m;
        var maturity = age + term / 12m;

        var indicators = new Indicators(
            payment,
            effort,
            maturity,
            income * 20m);

        void Add(string code, Decision severity, string message) =>
            findings.Add(new Finding(code, severity, message));

        // Age at the end: compare in months to avoid losing part-years.
        if ((long)age * 12 + term > 75L * 12)
        {
            Add("AGE_AT_MATURITY", Decision.ManualReview,
                "Idade no final do contrato superior a 75 anos.");
        }

        if (input.CreditIncidents == "Sim")
        {
            Add("CREDIT_INCIDENTS", Decision.Refused,
                "Existem incidentes de crédito registados.");
        }

        if (input.Employment == "ContratoPrazo")
        {
            Add("FIXED_TERM_EMPLOYMENT", Decision.ManualReview,
                "Cliente com contrato a prazo.");
        }

        if (input.Employment == "Desempregado")
        {
            Add("UNEMPLOYED", Decision.Refused,
                "Cliente desempregado.");
        }

        if (amount > income * 20m)
        {
            Add("AMOUNT_ABOVE_INCOME_LIMIT", Decision.ManualReview,
                "Montante superior a 20 vezes o rendimento mensal líquido.");
        }

        // Equivalent to comparing the effort rate with 35% and 50%.
        // This avoids rounding the estimated payment before the decision.
        var total = (existing * term + amount) * 100m;

        if (total > 50m * income * term)
        {
            Add("EFFORT_ABOVE_50", Decision.Refused,
                "Taxa de esforço superior a 50%.");
        }
        else if (total > 35m * income * term)
        {
            Add("EFFORT_ABOVE_35", Decision.ManualReview,
                "Taxa de esforço superior a 35% e até 50%.");
        }

        if (amount > 50_000m)
        {
            Add("HIGH_AMOUNT", Decision.ManualReview,
                "Montante pretendido superior a 50.000,00 €.");
        }

        // The enum values increase with severity.
        var decision = findings.Count == 0
            ? Decision.Approved
            : findings.Max(f => f.Severity);

        return new Evaluation(
            decision, findings.AsReadOnly(), indicators, Version);

        // These local functions belong to Evaluate. They can add findings
        // to the list declared above.
        int Integer(string? raw, int minimum, string code, string message)
        {
            if (raw is not null &&
                IntegerPattern().IsMatch(raw) &&
                int.TryParse(
                    raw,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var value) &&
                value >= minimum)
            {
                return value;
            }

            Invalid(code, message);
            return 0;
        }

        decimal Money(
            string? raw,
            bool allowZero,
            string code,
            string message)
        {
            if (raw is not null &&
                MoneyPattern().IsMatch(raw) &&
                decimal.TryParse(
                    raw.Replace(',', '.'),
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out var value) &&
                (allowZero ? value >= 0 : value > 0) &&
                value <= MaximumMoney)
            {
                return value;
            }

            Invalid(code, message);
            return 0;
        }
    }
}