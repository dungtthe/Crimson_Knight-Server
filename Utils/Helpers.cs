using Crimson_Knight_Server.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Utils
{
    public static class Helpers
    {
        public static float GetPercent(int value)
        {
            return value / 10000f;
        }

        public static Dictionary<StatId, Stat> DeserializeStats(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return new Dictionary<StatId, Stat>();

            return JsonSerializer.Deserialize<Dictionary<StatId, Stat>>(jsonString,
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true
                });
        }

        public static int GetStatValue(Dictionary<StatId, Stat> stats, StatId id)
        {
            if (stats == null || stats.Count == 0) return 0;
            return stats.TryGetValue(id, out Stat stat) ? stat.Value : 0;
        }

        public static string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
