using System.IO;
using System.Text.Json;
using LiteDB;
using SoilContaminationApi.Models;

namespace SoilContaminationApi.Services
{
    public class LiteDbService
    {
        private readonly ILiteDatabase _db;

        public LiteDbService()
        {
            _db = new LiteDatabase("Filename=soil.db;Connection=shared");
        }

        public ILiteCollection<ContaminantGuideline> Guidelines =>
            _db.GetCollection<ContaminantGuideline>("guidelines");

        public void SeedData(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var json = File.ReadAllText(filePath);
            var records = System.Text.Json.JsonSerializer.Deserialize<List<ContaminantGuideline>>(json);

            if (records != null && Guidelines.Count() == 0)
            {
                Guidelines.InsertBulk(records);
            }
        }
    }
}
