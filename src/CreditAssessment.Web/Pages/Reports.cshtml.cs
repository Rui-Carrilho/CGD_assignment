using CreditAssessment.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreditAssessment.Web.Pages;

public class ReportsModel : PageModel
{
    public ReportSnapshot? Snapshot { get; private set; }

    public async Task OnGetAsync()
    {
        var repository = new ReportRepository();
        Snapshot = await repository.GetAsync();
    }
}