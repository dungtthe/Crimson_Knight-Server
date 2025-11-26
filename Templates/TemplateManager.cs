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


        static string DataDirectory = "Resources";
        public static void LoadTemplate()
        {
            LoadMapTemplates();
        }

        private static void LoadMapTemplates()
        {
            string fileName = "MapTemplates.json";
            string filePath = Path.Combine(DataDirectory, fileName);
            string jsonString = File.ReadAllText(filePath);
            MapTemplates = JsonSerializer.Deserialize<List<MapTemplate>>(jsonString);
        }
    }
}
