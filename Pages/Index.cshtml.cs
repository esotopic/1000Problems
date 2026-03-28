using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _1000Problems.Data;
using _1000Problems.Models;

namespace _1000Problems.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationRepository _repository;

    // Static fallback for instant page load when no search is active
    private static readonly List<Application> _staticApps = new()
    {
        new Application
        {
            Id = 2,
            Name = "RubberJointsAI",
            Description = "Because your joints shouldn't sound like a bowl of Rice Krispies when you stand up. AI-powered mobility coaching that keeps you moving like you're 25 — even if your knees disagree.",
            Url = "https://rubberjoints-ai-app-aycpgve8acbvfpfj.canadacentral-01.azurewebsites.net",
            ImageUrl = "/images/rubberjoints-logo.svg",
            IsActive = true,
            CreatedDate = new DateTime(2026, 3, 27)
        }
    };

    public IndexModel(ApplicationRepository repository)
    {
        _repository = repository;
    }

    public List<Application> Applications { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? search)
    {
        SearchTerm = search;

        if (string.IsNullOrWhiteSpace(search))
        {
            // No search — use static data for instant rendering
            Applications = _staticApps;
            return;
        }

        // Search requested — hit the database for fresh results
        try
        {
            Applications = await _repository.GetActiveApplicationsAsync(search);
        }
        catch (Exception)
        {
            ErrorMessage = "Unable to search right now. Please try again in a moment.";
            // Fall back to filtering static list
            Applications = _staticApps
                .Where(a => a.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || a.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
