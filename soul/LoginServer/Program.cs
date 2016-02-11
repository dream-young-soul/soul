using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameBase.Network;
using GameBase.Config;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Timers;
using System.Threading;
using GameBase;
using GameBase.Core;
namespace LoginServer
{
    class Program
    {
        public static TcpServer server = null;
        public static GameBase.Network.TcpClient mTcpClient = null;
        public static InternalPacket mDBPacket;
        public static int m_Key;
        public static int m_Key2;
        public static Dictionary<Socket, GameBase.Network.GameSession> m_DicSession;
        private static object _lock_session = new object();
        public static string m_GameServerIP;
        public static int m_GameServerPort;
        static void Main(string[] args)
        {
            //日志路径
            Log.Instance().Init("./LogicServer");
            //异常
            GameBase.Core.GlobalException.InitException();
            MemIniFile ini = new MemIniFile();
            if (!ini.LoadFromFile(TextDefine.GoldConfig))
            {
                return;
            }
            m_DicSession = new Dictionary<Socket, GameBase.Network.GameSession>();
            m_DicSession.Clear();

            String sIP = ini.ReadValue(TextDefine.LogicServerSection, TextDefine.NormalIPKey, TextDefine.NormalIP);
            int nPort = ini.ReadValue(TextDefine.LogicServerSection, TextDefine.NormalPortKey, TextDefine.LoginServerPort);
            m_Key = ini.ReadValue(TextDefine.GlobalSection, TextDefine.EncodeKey, System.Environment.TickCount);
            m_Key2 = ini.ReadValue(TextDefine.GlobalSection, TextDefine.EncodeKey2, System.Environment.TickCount);
            m_GameServerIP = ini.ReadValue(TextDefine.GameServerSetion,TextDefine.NormalIPKey, TextDefine.NormalIP);
            m_GameServerPort = ini.ReadValue(TextDefine.GameServerSetion, TextDefine.NormalPortKey, TextDefine.GameServerPort);
            server = new TcpServer();
            server.onConnect += new TcpServerEvent.OnConnectEventHandler(OnConnect);
            server.onReceive += new TcpServerEvent.OnReceiveEventHandler(OnReceive);
            server.onClose += new TcpServerEvent.OnCloseEventHandler(OnClose);
            if (!server.Start(sIP, nPort))
            {
                Console.WriteLine("start server error!");
                return;
            }
           
            //连接dbserver
            mDBPacket = new InternalPacket();
            GenerateKey.Init(m_Key, m_Key2);
            sIP = ini.ReadValue(TextDefine.DBServerSestion, TextDefine.NormalIPKey, TextDefine.NormalIP);
            nPort = ini.ReadValue(TextDefine.DBServerSestion, TextDefine.NormalPortKey, TextDefine.DBServerPort);
            mTcpClient = new GameBase.Network.TcpClient();
            mTcpClient.onConnect += new TcpClientEvent.OnConnectEventHandler(OnDBConnect);
            mTcpClient.onReceive += new TcpClientEvent.OnReceiveEventHandler(OnDBReceive);
            mTcpClient.onClose += new TcpClientEvent.OnCloseEventHandler(OnDBClose);
            mTcpClient.Connect(sIP, nPort);
            //逻辑处理
            Thread logicThread = new Thread(new ThreadStart(LogicTimer));
            logicThread.IsBackground = true;
            logicThread.Start();
           
  
            while (true)
            {
               String command = Console.ReadLine();
               if (command == "exit")
               {
                   break;
               }
            }
        }
        public static void OnConnect(Socket s)
        {
            GameSession session;
            lock (_lock_session)
            {
                session = new GameSession(s);
                m_DicSession[s] = session;
            }

            //发送key
            PacketOut packout = new PacketOut(session.GetGamePackKeyEx());
            packout.WriteInt16(8); //长度
            packout.WriteUInt16(PacketProtoco.S_KEY);
            packout.WriteInt32(m_Key);
            byte[] data = packout.Flush();
            server.SendData(s, data);
        }

        public static void OnReceive(Socket s, byte[] data, int nSize)
        {
            lock (_lock_session)
            {
                if (m_DicSession.ContainsKey(s))
                {
                    GameSession session = m_DicSession[s];
                    byte[] reData = new byte[nSize];
                    Buffer.BlockCopy(data, 0, reData, 0, nSize);
                    session.m_GamePack.ProcessNetData(reData);
                }
            }

        }
        public static void OnClose(Socket s)
        {
            lock (_lock_session)
            {
                if (m_DicSession.ContainsKey(s))
                {
                    GameSession session = m_DicSession[s];
                    session = null;
                    m_DicSession.Remove(s);
                }
            }

        }

        public static void LogicTimer()
        {

            int processtime = 4;
            int sleeptime = 4;
            int nlastProcessTime = System.Environment.TickCount;
            while (true)
            {

                if (System.Environment.TickCount - nlastProcessTime > processtime)
                {
                    ProcessDBNetMsg();
                    Run();
                    nlastProcessTime = System.Environment.TickCount;
                }

                System.Threading.Thread.Sleep(sleeptime);
            }

        }


        private static void OnDBConnect(bool isSucceed)
        {
            if (isSucceed)
            {
                Log.Instance().WriteLog("dbserver connect success!");
                //认证


                GameBase.Network.Internal.OpenLoginSession pack = new GameBase.Network.Internal.OpenLoginSession();
                mTcpClient.SendData(pack.GetBuff());
            }
            else
            {
                Log.Instance().WriteLog("dbserver connect error!");
                Log.Instance().WriteLog("Reconnect  dbserver ip:" + mTcpClient.GetConnectIP() +
                    " port:" + mTcpClient.GetConnectPort().ToString());
                System.Threading.Thread.Sleep(5000);
                mTcpClient.ReConnect();
            }
        }


        private static void OnDBReceive(byte[] data, int nSize)
        {
            byte[] buff = new byte[nSize];
            Buffer.BlockCopy(data, 0, buff, 0, nSize);
            mDBPacket.ProcessNetMsg(buff);
        }

        private static void OnDBClose(Socket s)
        {
            mDBPacket.ClearPacket();
            Log.Instance().WriteLog("dbserver close!!!reconnect ");
            mTcpClient.ReConnect(); //重新连接
        }

        public static void ProcessDBNetMsg()
        {
            byte[] data = mDBPacket.GetData();
            if (data == null) return;
            PackIn inpack = new PackIn(data);
            ushort param = inpack.ReadUInt16();
            switch (param)
            {
                case GameBase.Network.Internal.Define.QUERYROLE_RET:
                {
                    uint gameid = inpack.ReadUInt32();
                    int key = inpack.ReadInt32();
                    int key2 = inpack.ReadInt32();
                    byte ret = inpack.ReadByte();
                    if (ret == 1) //有这个帐号--
                    {
                        GameSession session  = FindGameSessionToGameID(gameid);
                        if (session != null)
                        {
                            //发送数据给客户端，连接mapserver
                            Log.Instance().WriteLog("通知客户端登录MapServer");
                            SendConnectMapServer(session,key,key2);
                        }
                    }
                    else if (ret == 2) //重复登录，踢下线
                    {
                        GameSession session = FindGameSessionToGameID(gameid);
                        if (session != null)
                        {
                            lock (_lock_session)
                            {
                                m_DicSession.Remove(session.m_Socket);
                            }
                          
                            session.Dispose();
                        }
                    }

                    break;
                }
            }
        }
        public static void Run()
        {

            lock (_lock_session)
            {
                foreach (GameSession session in m_DicSession.Values)
                {
                    byte[] retData = session.m_GamePack.GetData();
                    if (retData != null)
                    {
                        PackIn packin = new PackIn(retData);
                        ushort tag = packin.ReadUInt16();
                        switch (tag)
                        {
                            case PacketProtoco.C_LOGINGAME:
                                {
                                    packin.ReadUInt32();

                                    int _key = 0, _key2 = 0;
                                    GenerateKey.GenerateKey_(ref _key, ref _key2);
                                    //取封包帐号- 发给dbserver 
                                    byte[] bAccount = packin.ReadBuff(16);

                                    String account = Coding.GetDefauleCoding().GetString(bAccount);
                                    GameBase.Network.Internal.QueryRole query = new GameBase.Network.Internal.QueryRole(session.gameid, _key, _key2, bAccount);
                                    mTcpClient.SendData(query.GetBuffer());

                                    Log.Instance().WriteLog("帐号登录!" + account);
                                    break;
                                }
                        }
                    }
                }
            }
          
        }

        public static GameSession FindGameSessionToGameID(uint gameid)
        {
            foreach (GameSession session in m_DicSession.Values)
            {
                if (session.gameid == gameid)
                {
                    return session;
                }
            }
            return null;
        }

        public static void SendConnectMapServer(GameSession session, int key, int key2)
        {
            byte[] defdata = { 232, 16, 67, 3 };
            byte[] defdata2 = { 121, 39, 0, 0, 49, 50, 48, 46, 49, 51, 50, 46, 54, 
                                          57, 46, 49, 52, 55, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] ipdata = Coding.GetDefauleCoding().GetBytes(m_GameServerIP);

            PacketOut packout = new PacketOut(session.GetGamePackKeyEx());
            packout.WriteUInt16(84);
            packout.WriteUInt16(PacketProtoco.S_GAMESERVERINFO);
            packout.WriteInt32(key);
            packout.WriteInt32(key2);
            packout.WriteInt32(m_GameServerPort);
            packout.WriteBuff(defdata);
            packout.WriteBuff(ipdata);
            for (int i = 0; i < 28 - ipdata.Length; i++)
            {
                packout.WriteByte(0);
            }
            packout.WriteBuff(defdata2);

            defdata = packout.Flush();
            server.SendData(session.m_Socket, defdata);
        }
    }
}
