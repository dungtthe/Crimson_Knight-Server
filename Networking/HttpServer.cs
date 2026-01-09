using Crimson_Knight_Server.Services;
using Crimson_Knight_Server.Services.Dtos;
using Crimson_Knight_Server.Templates;
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

                    statusCode = 200; 
                    break;
                case "/login":
                    if (request.HttpMethod != "POST")
                    {
                        statusCode = 405;
                        responseJson = JsonSerializer.Serialize(new LoginResponse
                        {
                            HttpStatusCode = 405,
                            Message = "a"
                        });
                        break;
                    }

                    // Đọc body JSON
                    string body;
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        body = reader.ReadToEnd();
                    }

                    LoginRequest loginRequest;
                    try
                    {
                        loginRequest = JsonSerializer.Deserialize<LoginRequest>(body);
                    }
                    catch
                    {
                        statusCode = 400;
                        responseJson = JsonSerializer.Serialize(new LoginResponse
                        {
                            HttpStatusCode = 400,
                            Message = "a"
                        });
                        break;
                    }

                    var loginResponse = PlayerService.Login(loginRequest);

                    statusCode = loginResponse.HttpStatusCode;
                    responseJson = JsonSerializer.Serialize(loginResponse);
                    break;
                case "/load-templates":
                    statusCode = 200;
                    responseJson = JsonSerializer.Serialize(
                    new {
                        MonsterTemplates = TemplateManager.MonsterTemplates,
                        NpcTemplates = TemplateManager.NpcTemplates,
                        SkillTemplates = TemplateManager.SkillTemplates,
                    });
                    break;
                default:
                    var errorInfo = new { error = "Not Found" };
                    responseJson = JsonSerializer.Serialize(errorInfo);

                   
                    statusCode = 404; 
                    break;
            }

            response.ContentType = "application/json";
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
