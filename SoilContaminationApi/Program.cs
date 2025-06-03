using SoilContaminationApi.Models;
using SoilContaminationApi.Services;
using LiteDB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<LiteDbService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

var app = builder.Build();

// seed data
var liteDbService = app.Services.GetRequiredService<LiteDbService>();
liteDbService.SeedData("Data/seed.json");

app.UseSwagger();
app.UseSwaggerUI();

// Minimal CRUD API for /manage
app.MapGet("/manage", (LiteDbService db) =>
{
    return Results.Ok(db.Guidelines.FindAll());
});

app.MapPost("/manage", (LiteDbService db, ContaminantGuideline guideline) =>
{
    // Validate the guideline object
    if (string.IsNullOrWhiteSpace(guideline.Contaminant) ||
        guideline.Pathways == null || !guideline.Pathways.Any() ||
        string.IsNullOrWhiteSpace(guideline.SoilType) ||
        string.IsNullOrWhiteSpace(guideline.GuidelineValue))
    {
        return Results.BadRequest("Contaminant, Pathways, SoilType, or GuidelineValue cannot be null or empty.");
    }

    // Check for duplicates
    var duplicate = db.Guidelines.FindAll()
        .FirstOrDefault(x =>
            x.Contaminant == guideline.Contaminant &&
            x.SoilType == guideline.SoilType &&
            x.Pathways.OrderBy(p => p).SequenceEqual(guideline.Pathways.OrderBy(p => p))
        );

    if (duplicate != null)
    {
        return Results.Conflict("A guideline with the same Contaminant, Pathways, and SoilType already exists.");
    }

    db.Guidelines.Insert(guideline);
    return Results.Created($"/manage/{guideline.Id}", guideline);
});

app.MapPut("/manage/{id}", (int id, ContaminantGuideline updated, LiteDbService db) =>
{
    var existing = db.Guidelines.FindById(id);
    if (existing == null) return Results.NotFound();

    // only update non-null fields
    existing.Contaminant = string.IsNullOrWhiteSpace(updated.Contaminant) ? existing.Contaminant : updated.Contaminant;
    existing.Pathways = (updated.Pathways == null || !updated.Pathways.Any()) ? existing.Pathways : updated.Pathways;    existing.SoilType = string.IsNullOrWhiteSpace(updated.SoilType) ? existing.SoilType : updated.SoilType;
    existing.GuidelineValue = string.IsNullOrWhiteSpace(updated.GuidelineValue) ? existing.GuidelineValue : updated.GuidelineValue;

    db.Guidelines.Update(existing);
    return Results.Ok(existing);
});

app.MapDelete("/manage/{id}", (int id, LiteDbService db) =>
{
    var success = db.Guidelines.Delete(id);
    return success ? Results.NoContent() : Results.NotFound();
});

app.MapPost("/analyze", (AnalyzeRequest request, LiteDbService db) =>
{
    if (string.IsNullOrWhiteSpace(request.Contaminant) || string.IsNullOrWhiteSpace(request.SoilType))
        return Results.BadRequest("Contaminant and SoilType are required.");

    var matchingRecords = db.Guidelines.FindAll()
        .Where(g =>
            g.Contaminant.Equals(request.Contaminant, StringComparison.OrdinalIgnoreCase) &&
            g.SoilType.Equals(request.SoilType, StringComparison.OrdinalIgnoreCase) &&
            (request.Pathways == null || !request.Pathways.Any() || g.Pathways.Any(p => request.Pathways.Contains(p)))
        )
        .ToList();

    if (!matchingRecords.Any())
        return Results.NotFound("No matching guidelines found.");

    var parsedRecords = matchingRecords
        .Select(g =>
        {
            double value;
            try
            {
                value = double.Parse(g.GuidelineValue);
            }
            catch
            {
                value = double.MaxValue; // if the value is "NGR" (not parsable) automatically set to a very high value
            }

            return new
            {
                Pathway = g.Pathways.FirstOrDefault() ?? "Unknown",
                Value = value
            };
        }).ToList();

    var lowestValue = parsedRecords.Min(p => p.Value);

    var exceedingPathways = parsedRecords
        .Where(p => request.MeasuredValue > p.Value)
        .Select(p => p.Pathway)
        .Distinct()
        .ToList();

    return Results.Ok(new
    {
        IsCompliant = request.MeasuredValue <= lowestValue,
        GuidelineValue = lowestValue,
        ExceedingPathways = exceedingPathways
    });
});

app.Run();
