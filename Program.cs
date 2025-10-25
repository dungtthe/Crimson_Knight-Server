using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Utils.Loggings;

namespace Crimson_Knight_Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Start server");
            ServerSetting.SetUp();
            HttpServer.Start();
            Console.WriteLine("Set up ok");
            Console.ReadLine();
        }
    }
}
