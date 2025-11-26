using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players
{
    public class Player
    {
        #region network
        private TcpClient tcpClient;
        private NetworkStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;
        private Thread receiveThread;
        private static Thread sendThread;
        private volatile bool isRunning;
        private MessageHandler messageHandler;

        public void InitNetwork(TcpClient client)
        {
            tcpClient = client;
            stream = client.GetStream();
            reader = new BinaryReader(stream, Encoding.UTF8, true);//đảm bảo stream là người close
            writer = new BinaryWriter(stream, Encoding.UTF8, true);//đảm bảo stream là người close
            messageHandler = new MessageHandler(this);
            Start();
        }

        private void Start()
        {
            isRunning = true;
            receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void ReceiveLoop()
        {
            try
            {
                while (isRunning)
                {
                    int length = reader.ReadInt32();//co blocking roi nen k phai lo
                    if (length <= 0) continue;
                    byte[] data = reader.ReadBytes(length);
                    Message msg = new Message(data);
                    messageHandler.HandleMessage(msg);
                }
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError($"[Client {PlayerId}] Lỗi ReceiveLoop: {ex.Message}");
            }
            finally
            {
                Close();
            }
        }


        private object lockSendMessage = new object();
        public void SendMessage(Message msg)
        {
            if (!isRunning) return;
            lock (lockSendMessage)
            {
                try
                {
                    byte[] data = msg.GetData();
                    writer.Write(data.Length);
                    writer.Write(data);
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    ConsoleLogging.LogError($"[Client {PlayerId}] Lỗi SendMessage: {ex.Message}");
                    Close();
                }
            }
        }


        private static object lockClose = new object();
        public void Close()
        {
            lock (lockClose)
            {
                ServerManager.GI().RemoveSession(this);
                if (!isRunning) return;
                isRunning = false;
                try { reader.Close(); } catch { }
                try { writer.Close(); } catch { }
                try { stream.Close(); } catch { }
                try { tcpClient.Close(); } catch { }
                ConsoleLogging.LogWarning($"[Client {PlayerId}] Đã đóng kết nối.");
            }
        }
        #endregion

        public int PlayerId;
        public Map MapCur;
        public Player(int playerId)
        {
            PlayerId = playerId;
        }
    }

}
