using System.Net;
using CreditAssessment.Core;
using CreditAssessment.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;

namespace CreditAssessment.Web.Pages;

public class ReviewModel : PageModel
{
    private readonly IWebHostEnvironment environment;

    public ReviewModel(IWebHostEnvironment environment) =>
        this.environment = environment;

    public StoredAssessment? Assessment { get; private set; }

    [BindProperty] public string? Actor { get; set; }
    [BindProperty] public string? Justification { get; set; }

    private bool IsLocalDevelopment() =>
        environment.IsDevelopment() &&
        HttpContext.Connection.RemoteIpAddress is { } address &&
        IPAddress.IsLoopback(address);

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (!IsLocalDevelopment())
            return NotFound();

        var repository = new CreditRequestRepository();
        Assessment = await repository.GetAsync(id);

        if (Assessment is null)
            return NotFound();

        if (Assessment.CurrentDecision != Decision.ManualReview)
            return RedirectToPage("/Result", new { id });

        return Page();
    }

    public Task<IActionResult> OnPostApproveAsync(Guid id) =>
        ResolveAsync(id, Decision.Approved);

    public Task<IActionResult> OnPostRefuseAsync(Guid id) =>
        ResolveAsync(id, Decision.Refused);

    private async Task<IActionResult> ResolveAsync(
        Guid id,
        Decision newDecision)
    {
        if (!IsLocalDevelopment())
            return NotFound();

        var repository = new CreditRequestRepository();
        Assessment = await repository.GetAsync(id);

        if (Assessment is null)
            return NotFound();

        if (Assessment.CurrentDecision != Decision.ManualReview)
            return RedirectToPage("/Result", new { id });

        if (string.IsNullOrWhiteSpace(Actor) || Actor.Length > 100)
        {
            ModelState.AddModelError(
                nameof(Actor),
                "Indique o nome do analista (até 100 caracteres).");
        }

        if (string.IsNullOrWhiteSpace(Justification) ||
            Justification.Length > 500)
        {
            ModelState.AddModelError(
                nameof(Justification),
                "Indique a fundamentação da decisão (até 500 caracteres).");
        }

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await repository.ResolveManualReviewAsync(
                id,
                newDecision,
                Actor!,
                Justification!);
        }
        catch (InvalidOperationException)
        {
            return RedirectToPage("/Result", new { id });
        }

        return RedirectToPage("/Result", new { id });
    }
}