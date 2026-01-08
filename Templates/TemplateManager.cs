using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Utils;
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


        public static Dictionary<ClassType, List<SkillTemplate>> SkillTemplates;

        static string DataDirectory = "Resources";
        public static void LoadTemplate()
        {
            NpcTemplates = LoadTemplates<NpcTemplate>("NpcTemplates.json");
            MonsterTemplates = LoadTemplates<MonsterTemplate>("MonsterTemplates.json");
            MapTemplates = LoadTemplates<MapTemplate>("MapTemplates.json");
            DepartTemplates = LoadTemplates<DepartTemplate>("DepartTemplates.json");

            LoadStats();
            LoadSkillTemplates();
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
        private static void LoadSkillTemplates()
        {
            string fileName = "SkillTemplates.json";
            string filePath = Path.Combine(DataDirectory, fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Không tìm thấy file {filePath}");

            string jsonString = File.ReadAllText(filePath);

            var dictRaw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            SkillTemplates = new Dictionary<ClassType, List<SkillTemplate>>();

            foreach (var kvp in dictRaw)
            {
                if (!Enum.TryParse<ClassType>(kvp.Key, out var classType))
                    throw new Exception($"Lỗi khi chuyển đổi ClassType từ chuỗi: {kvp.Key}");

                var skillList = new List<SkillTemplate>();

                foreach (var skillElem in kvp.Value.EnumerateArray())
                {
                    var skill = JsonSerializer.Deserialize<SkillTemplate>(skillElem.GetRawText(),
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (skillElem.TryGetProperty("Stats", out var statsElem))
                        skill.Stats = Helpers.DeserializeStats(statsElem.GetRawText());
                    else
                        skill.Stats = new Dictionary<StatId, Stat>();

                    skillList.Add(skill);
                }
                SkillTemplates[classType] = skillList;
            }
        }


        private static List<T> LoadTemplates<T>(string fileName)
        {
            string filePath = Path.Combine(DataDirectory, fileName);
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<T>>(jsonString);
        }
    }
}
