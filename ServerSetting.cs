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
        //log
        public static bool IsConsoleLoggingInfo;
        public static bool IsConsoleLoggingError;
        public static bool IsConsoleLoggingWarning;


        //networking
        public static string URI;
        public static int PORT_HTTP;
        public static int PORT_TCP;
        public static void SetUp()
        {
            JsonObject obj = (JsonObject)JsonNode.Parse(File.ReadAllText("ServerSetting.json"));

            //log
            JsonObject logsSection = (JsonObject)obj["Logs"];
            IsConsoleLoggingInfo = (bool)logsSection["IsConsoleLoggingInfo"];
            IsConsoleLoggingError = (bool)logsSection["IsConsoleLoggingError"];
            IsConsoleLoggingWarning = (bool)logsSection["IsConsoleLoggingWarning"];

            //networking
            JsonObject networkingSection = (JsonObject)obj["Networking"];
            URI = (string)networkingSection["URI"];
            PORT_HTTP = (int)networkingSection["PORT_HTTP"];
            PORT_TCP = (int)networkingSection["PORT_TCP"];
        }
    }
}
