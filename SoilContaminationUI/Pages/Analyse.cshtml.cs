using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class AnalyzeModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AnalyzeModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public AnalyzeInput Input { get; set; } = new AnalyzeInput();

    public AnalyzeResult? Result { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var payload = new
        {
            Contaminant = Input.Contaminant,
            MeasuredValue = Input.MeasuredValue,
            SoilType = Input.SoilType,
            Pathways = string.IsNullOrWhiteSpace(Input.Pathways)
                ? null
                : Input.Pathways.Split(',').Select(p => p.Trim()).ToList()
        };

        var response = await client.PostAsJsonAsync("/analyze", payload);

        if (response.IsSuccessStatusCode)
        {
            Result = await response.Content.ReadFromJsonAsync<AnalyzeResult>();
        }
        else
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                ModelState.AddModelError(string.Empty, "Contaminant not found.");
            }
            else if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "An error occurred during analysis.");
            }
        }

        return Page();
    }

    public class AnalyzeInput
    {
        public string Contaminant { get; set; }
        public double MeasuredValue { get; set; }
        public string SoilType { get; set; }
        public string? Pathways { get; set; }
    }

    public class AnalyzeResult
    {
        public bool IsCompliant { get; set; }
        public double GuidelineValue { get; set; }
        public List<string> ExceedingPathways { get; set; } = new();
    }
}