using CreditAssessment.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreditAssessment.Web.Pages;

public class HistoryModel : PageModel
{
    public IReadOnlyList<RequestSummary> Requests { get; private set; } =
        Array.Empty<RequestSummary>();

    public async Task OnGetAsync()
    {
        var repository = new CreditRequestRepository();
        Requests = await repository.ListRecentAsync();
    }
}