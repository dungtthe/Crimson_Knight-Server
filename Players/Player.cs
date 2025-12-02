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
    public class Player:IPlayerBroadcaster, IPlayerSelfSender
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
            if (!isRunning) return;
            isRunning = false;

            lock (lockClose)
            {
                if (this.PlayerId != -1)
                {
                    ServerManager.GI().RemoveSession(this);
                    ConsoleLogging.LogWarning($"[Client {PlayerId}] Đã đóng kết nối.");
                }
                else
                {

                }
                try { reader.Close(); } catch { }
                try { writer.Close(); } catch { }
                try { stream.Close(); } catch { }
                try { tcpClient.Close(); } catch { }
            }
        }

       
        #endregion

        public int PlayerId;
        public string Name;
        public Map MapCur;
        public short X;
        public short Y;
        public Player(int playerId)
        {
            PlayerId = playerId;
        }


        public void BroadcastEnterMap()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.OTHER_PLAYER_ENTER_MAP);
                msg.WriteInt(PlayerId);
                msg.WriteString(Name);
                msg.WriteShort(X);
                msg.WriteShort(Y);
                ServerManager.GI().SendOthersInMap(msg, this);
                msg.Close();
            }
        }

        public void BroadcastMove()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.OTHER_PLAYER_MOVE);
                msg.WriteInt(PlayerId);
                msg.WriteShort(X);
                msg.WriteShort(Y);
                ServerManager.GI().SendOthersInMap(msg, this);
                msg.Close();
            }
        }

        public void SendEnterMap()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.PLAYER_ENTER_MAP);
                msg.WriteInt(PlayerId);
                msg.WriteString(Name);
                msg.WriteShort(X);
                msg.WriteShort(Y);
                msg.WriteShort(MapCur.Id);
                SendMessage(msg);
                msg.Close();
            }
        }
    }

}
