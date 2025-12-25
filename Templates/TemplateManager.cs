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


        static string DataDirectory = "Resources";
        public static void LoadTemplate()
        {
            LoadMapTemplates();
            LoadMonsterTemplate();
            LoadDepartTemplates();
        }

        private static void LoadMonsterTemplate()
        {
            List<MonsterTemplate> items = new List<MonsterTemplate>()
            {
                new MonsterTemplate(){Id = 0, Name = "Slime",    ImageId = 1000},
                new MonsterTemplate(){Id = 1, Name = "Snail",    ImageId = 1001 },
                new MonsterTemplate(){Id = 2, Name = "Scorpion", ImageId = 1103 },
                new MonsterTemplate(){Id = 3, Name = "Bunny",    ImageId = 1173},
                new MonsterTemplate(){Id = 4, Name = "Frog",     ImageId = 1215},
            };
            MonsterTemplates.AddRange(items);
        }

        private static void LoadMapTemplates()
        {
            string fileName = "MapTemplates.json";
            string filePath = Path.Combine(DataDirectory, fileName);
            string jsonString = File.ReadAllText(filePath);
            MapTemplates = JsonSerializer.Deserialize<List<MapTemplate>>(jsonString);
        }


        private static void LoadDepartTemplates()
        {
            string fileName = "DepartTemplates.json";
            string filePath = Path.Combine(DataDirectory, fileName);
            string jsonString = File.ReadAllText(filePath);
            DepartTemplates = JsonSerializer.Deserialize<List<DepartTemplate>>(jsonString);
        }
    }
}
