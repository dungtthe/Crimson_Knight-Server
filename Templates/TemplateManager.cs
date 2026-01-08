using Crimson_Knight_Server.Stats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Templates
{
    public static class TemplateManager
    {
        public static List<MapTemplate> MapTemplates = new List<MapTemplate>();
        public static List<MonsterTemplate> MonsterTemplates = new List<MonsterTemplate>();
        public static List<DepartTemplate> DepartTemplates = new List<DepartTemplate>();
        public static List<NpcTemplate> NpcTemplates = new List<NpcTemplate>();

        private static Dictionary<StatId, StatDefinition> StatDefinitions;
        public static StatDefinition GetStatDefinition(StatId id)
            => StatDefinitions.TryGetValue(id, out var def) ? def : null;

        static string DataDirectory = "Resources";
        public static void LoadTemplate()
        {
            NpcTemplates = LoadTemplates<NpcTemplate>("NpcTemplates.json");
            MonsterTemplates = LoadTemplates<MonsterTemplate>("MonsterTemplates.json");
            MapTemplates = LoadTemplates<MapTemplate>("MapTemplates.json");
            DepartTemplates = LoadTemplates<DepartTemplate>("DepartTemplates.json");

            LoadStats();
        }

        private static void LoadStats()
        {
            string fileName = "StatDefinitions.json";
            string filePath = Path.Combine(DataDirectory, fileName);
            string jsonString = File.ReadAllText(filePath);

            var list = JsonSerializer.Deserialize<List<StatDefinition>>(jsonString,
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true
                });

            StatDefinitions = list.ToDictionary(s => s.StatId, s => s);
        }

        private static List<T> LoadTemplates<T>(string fileName)
        {
            string filePath = Path.Combine(DataDirectory, fileName);
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<T>>(jsonString);
        }
    }
}
