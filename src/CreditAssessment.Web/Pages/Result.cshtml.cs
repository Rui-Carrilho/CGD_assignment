using CreditAssessment.Core;
using CreditAssessment.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreditAssessment.Web.Pages;

public class ResultModel : PageModel
{
    public StoredAssessment? Assessment { get; private set; }

    public string DecisionLabel => Assessment?.CurrentDecision switch
    {
        Decision.Approved => "APROVADO",
        Decision.ManualReview => "ANÁLISE MANUAL",
        Decision.Refused => "RECUSADO",
        Decision.Invalid => "PEDIDO INVÁLIDO",
        _ => "DESCONHECIDO"
    };

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var repository = new CreditRequestRepository();
        Assessment = await repository.GetAsync(id);

        if (Assessment is null)
            return NotFound();

        History = await repository.GetHistoryAsync(id);
        return Page();
    }

    public IReadOnlyList<StatusHistoryEntry> History { get; private set; } =
    Array.Empty<StatusHistoryEntry>();
}