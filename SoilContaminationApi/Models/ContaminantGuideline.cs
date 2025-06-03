using Swashbuckle.AspNetCore.Annotations;
using LiteDB;

namespace SoilContaminationApi.Models
{
    [SwaggerSchema(Description = "Represents a guideline record for a specific chemical, soil type, and exposure pathways.")]
    public class ContaminantGuideline
    {
        [BsonId]
        [SwaggerSchema("Unique identifier for the guideline entry.")]
        public int Id { get; set; }

        [SwaggerSchema("The name of the chemical/contaminant.")]
        public string Contaminant { get; set; }

        [SwaggerSchema("The list of exposure pathways associated with this guideline.")]
        public List<string> Pathways { get; set; } = new List<string>();

        [SwaggerSchema("The type of soil this guideline applies to (e.g., Fine or Coarse).")]
        public string SoilType { get; set; }

        [SwaggerSchema("The guideline value threshold for this contaminant-pathway-soilType combination.")]
        public string GuidelineValue { get; set; }
    }
}
