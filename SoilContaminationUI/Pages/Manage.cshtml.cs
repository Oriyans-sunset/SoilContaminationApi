using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

public class ManageModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ManageModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public NewGuidelineInput NewGuideline { get; set; } = new();

    public List<ContaminantGuideline> Guidelines { get; set; } = new();

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var result = await client.GetAsync("/manage");
        if (result.IsSuccessStatusCode)
        {
            Guidelines = await result.Content.ReadFromJsonAsync<List<ContaminantGuideline>>() ?? new();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var payload = new
        {
            Contaminant = NewGuideline.Contaminant,
            SoilType = NewGuideline.SoilType,
            Pathways = string.IsNullOrWhiteSpace(NewGuideline.Pathways)
                ? new List<string>()
                : NewGuideline.Pathways.Split(',').Select(p => p.Trim()).ToList(),
            GuidelineValue = NewGuideline.GuidelineValue
        };

        var response = await client.PostAsJsonAsync("/manage", payload);
        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage();
        }

        ModelState.AddModelError(string.Empty, "Failed to add guideline.");
        await OnGetAsync(); // Reload the list
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.DeleteAsync($"/manage/{id}");

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, $"Failed to delete guideline with ID {id}.");
        }

        return RedirectToPage();
    }

    public class NewGuidelineInput
    {
        public string Contaminant { get; set; }
        public string SoilType { get; set; }
        public string Pathways { get; set; }
        public string GuidelineValue { get; set; }
    }

    public class ContaminantGuideline
    {
        public int Id { get; set; }
        public string Contaminant { get; set; }
        public string SoilType { get; set; }
        public List<string> Pathways { get; set; } = new();
        public string GuidelineValue { get; set; }
    }
}