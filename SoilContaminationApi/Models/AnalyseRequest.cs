using Swashbuckle.AspNetCore.Annotations;

namespace SoilContaminationApi.Models
{
    [SwaggerSchema(Description = "Payload to evaluate a chemical's compliance against guideline values.")]
    public class AnalyzeRequest
    {
        [SwaggerSchema("The name of the chemical/contaminant to analyze.")]
        public string Contaminant { get; set; }

        [SwaggerSchema("The type of soil: Fine or Coarse.")]
        public string SoilType { get; set; }

        [SwaggerSchema("The measured concentration of the chemical.")]
        public double MeasuredValue { get; set; }

        [SwaggerSchema("Optional list of pathway names to evaluate against.")]
        public List<string>? Pathways { get; set; }
    }
}