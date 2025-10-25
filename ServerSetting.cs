using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Crimson_Knight_Server
{
    public static class ServerSetting
    {
        public static bool IsConsoleLoggingInfo;
        public static bool IsConsoleLoggingError;
        public static bool IsConsoleLoggingWarning;
        public static void SetUp()
        {
            JsonObject obj = (JsonObject)JsonNode.Parse(File.ReadAllText("ServerSetting.json"));

            //log
            JsonObject logsSection = (JsonObject)obj["Logs"];
            IsConsoleLoggingInfo = (bool)logsSection["IsConsoleLoggingInfo"];
            IsConsoleLoggingError = (bool)logsSection["IsConsoleLoggingError"];
            IsConsoleLoggingWarning = (bool)logsSection["IsConsoleLoggingWarning"];
        }
    }
}
