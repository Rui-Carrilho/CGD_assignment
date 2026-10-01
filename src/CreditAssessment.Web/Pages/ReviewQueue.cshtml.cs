using System.Net;
using CreditAssessment.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;

namespace CreditAssessment.Web.Pages;

public class ReviewQueueModel : PageModel
{
    private readonly IWebHostEnvironment environment;

    public ReviewQueueModel(IWebHostEnvironment environment) =>
        this.environment = environment;

    public IReadOnlyList<ManualReviewQueueItem> Requests
        { get; private set; } =
        Array.Empty<ManualReviewQueueItem>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!environment.IsDevelopment() ||
            HttpContext.Connection.RemoteIpAddress is not { } address ||
            !IPAddress.IsLoopback(address))
        {
            return NotFound();
        }

        var repository = new CreditRequestRepository();
        Requests = await repository.ListManualReviewQueueAsync();

        return Page();
    }
}