using System;
using System.Collections.Generic;
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


        static string DataDirectory = "Resources";
        public static void LoadTemplate()
        {
            NpcTemplates = LoadTemplates<NpcTemplate>("NpcTemplates.json");
            MonsterTemplates = LoadTemplates<MonsterTemplate>("MonsterTemplates.json");
            MapTemplates = LoadTemplates<MapTemplate>("MapTemplates.json");
            DepartTemplates = LoadTemplates<DepartTemplate>("DepartTemplates.json");
        }

        private static List<T> LoadTemplates<T>(string fileName)
        {
            string filePath = Path.Combine(DataDirectory, fileName);
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<T>>(jsonString);
        }
    }
}
