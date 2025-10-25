using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Networking
{
    public static class HttpServer
    {
        private static readonly HttpListener _listener = new HttpListener();
        public static void Start()
        {
            string prefix = $"{ServerSetting.URI}:{ServerSetting.PORT_HTTP}/";
            _listener.Prefixes.Add(prefix);
            try
            {
                _listener.Start();
                ConsoleLogging.LogInfor($"HTTP Server đang chạy trên {prefix}");
                Task.Run(() => ListenLoop());
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError($"Không thể khởi động HTTP server: {ex.Message}");
            }
        }

        private static async void ListenLoop()
        {
            try
            {
                while (_listener.IsListening)
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    ProcessRequest(context);
                }
            }
            catch (HttpListenerException)
            {
                ConsoleLogging.LogWarning("HTTP Listener đã dừng.");
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError($"Lỗi trong vòng lặp HTTP listener: {ex.Message}");
            }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseJson;
            int statusCode = 200; 

            switch (request.Url.AbsolutePath)
            {
                case "/check-endian":
                    bool isLittle = BitConverter.IsLittleEndian;
                    var endianInfo = new { isLittleEndian = isLittle };
                    responseJson = JsonSerializer.Serialize(endianInfo);

                    response.ContentType = "application/json";
                    statusCode = 200; 
                    break;

                default:
                    var errorInfo = new { error = "Not Found" };
                    responseJson = JsonSerializer.Serialize(errorInfo);

                    response.ContentType = "application/json";
                    statusCode = 404; 
                    break;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
            response.StatusCode = statusCode;
            response.ContentLength64 = buffer.Length;

            using (Stream output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
        }

        public static void Stop()
        {
            if (_listener.IsListening)
            {
                _listener.Stop();
                _listener.Close();
                ConsoleLogging.LogInfor("HTTP Server đã dừng.");
            }
        }
    }
}
