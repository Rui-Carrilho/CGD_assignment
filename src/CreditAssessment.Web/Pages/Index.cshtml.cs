using CreditAssessment.Core;
using CreditAssessment.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreditAssessment.Web.Pages;

public class IndexModel : PageModel
{
    [BindProperty] public string? Nif { get; set; }
    [BindProperty] public string? Age { get; set; }
    [BindProperty] public string? MonthlyIncome { get; set; }
    [BindProperty] public string? ExistingInstalments { get; set; }
    [BindProperty] public string? RequestedAmount { get; set; }
    [BindProperty] public string? TermMonths { get; set; }
    [BindProperty] public string? Employment { get; set; }
    [BindProperty] public string? CreditIncidents { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var input = new RequestInput
        {
            Nif = Nif,
            Age = Age,
            MonthlyIncome = MonthlyIncome,
            ExistingInstalments = ExistingInstalments,
            RequestedAmount = RequestedAmount,
            TermMonths = TermMonths,
            Employment = Employment,
            CreditIncidents = CreditIncidents
        };

        var evaluator = new CreditEvaluator();
        Evaluation evaluation = evaluator.Evaluate(input);

        var repository = new CreditRequestRepository();
        Guid id = await repository.SaveAsync(input, evaluation);

        return RedirectToPage("/Result", new { id });
    }
}