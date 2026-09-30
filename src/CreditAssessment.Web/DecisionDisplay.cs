using CreditAssessment.Core;

namespace CreditAssessment.Web;

public static class DecisionDisplay
{
    public static string Label(Decision decision) => decision switch
    {
        Decision.Approved => "APROVADO",
        Decision.ManualReview => "ANÁLISE MANUAL",
        Decision.Refused => "RECUSADO",
        Decision.Invalid => "PEDIDO INVÁLIDO",
        _ => "DESCONHECIDO"
    };
}