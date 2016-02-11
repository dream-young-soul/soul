using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameBase;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Timers;
using System.Diagnostics;
using GameBase.Config;
using GameBase.Network;
namespace AccServer
{
    class Program
    {
        public static GameBase.Network.TcpServer server = new GameBase.Network.TcpServer();
        //public static Dictionary<Socket, GameBase.Network.GameSession> m_DicSession;
        public static System.Timers.Timer m_LogicTimer; //逻辑处理定时器
        static void Main(string[] args)
        {
            //日志路径
            Log.Instance().Init("./Accserver");
            //异常
            GameBase.Core.GlobalException.InitException();


            //读取全局配置
            MemIniFile ini = new MemIniFile();
            if (!ini.LoadFromFile(TextDefine.GoldConfig))
            {
                return;
            }
            String sIP = ini.ReadValue(TextDefine.ACCServerSection, TextDefine.NormalIPKey, TextDefine.NormalIP);
            int nPort = ini.ReadValue(TextDefine.ACCServerSection, TextDefine.NormalPortKey, TextDefine.AccServerPort);
            Console.Title = "AccServer";
           // m_DicSession = new Dictionary<Socket, GameBase.Network.GameSession>();
            //m_DicSession.Clear();
            server.onConnect += new GameBase.Network.TcpServerEvent.OnConnectEventHandler(OnConnect);
            server.onReceive += new GameBase.Network.TcpServerEvent.OnReceiveEventHandler(OnRecv);
            server.onClose += new GameBase.Network.TcpServerEvent.OnCloseEventHandler(OnClose);
            //启动服务器
            Log.Instance().WriteLog("bind ip:" + sIP + "bindport:" + nPort.ToString());
            if (!server.Start(sIP, nPort))
            {
                Log.Instance().WriteLog("start tcpserver error!");
            }
            Log.Instance().WriteLog("start server success!!");
            //数据包处理
            m_LogicTimer = new System.Timers.Timer(1);
            m_LogicTimer.Elapsed += new ElapsedEventHandler(LogicTimer);
            m_LogicTimer.Enabled = true;
            while (true)
            {
               String command =  Console.ReadLine();
               if(command == "quit" || command == "exit")
               {
                   server.Stop();
                   Log.Instance().Dispose();
                   break;
               }
            }

           
        }

        public static void OnConnect(Socket s)
        {
            SocketInfo info = new SocketInfo();
            info.type = SocketCallBack.TYPE_ONCONNECT;
            info.s = s;
            SocketCallBack.Instance().AddData(info);
            //try
            //{
            //    lock (m_DicSession)
            //    {
            //        GameBase.Network.GameSession session = new GameBase.Network.GameSession(s);
            //        m_DicSession[s] = session;
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    Log.Instance().WriteLog(ex.Message);
            //    Log.Instance().WriteLog(ex.StackTrace);
            //}
        

        }
        public static void OnClose(Socket s)
        {
            SocketInfo info = new SocketInfo();
            info.type = SocketCallBack.TYPE_CLOSE;
            info.s = s;
            SocketCallBack.Instance().AddData(info);
            //try
            //{
            //    lock (m_DicSession)
            //    {
            //        if (m_DicSession.ContainsKey(s))
            //        {
            //            GameBase.Network.GameSession session = m_DicSession[s];
            //            m_DicSession.Remove(s);
            //            session = null;
            //        }
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    Log.Instance().WriteLog(ex.Message);
            //    Log.Instance().WriteLog(ex.StackTrace);
            //}
          

        }

        public static void OnRecv(Socket s, byte[] data, int nSize)
        {

            SocketInfo info = new SocketInfo();
            info.type = SocketCallBack.TYPE_RECEIVE;
            info.s = s;
            info.data = new byte[nSize];
            Buffer.BlockCopy(data, 0, info.data, 0, nSize);
            SocketCallBack.Instance().AddData(info);
            //try
            //{
            //    lock (m_DicSession)
            //    {
            //        if (m_DicSession.ContainsKey(s))
            //        {
            //            GameBase.Network.GameSession session = m_DicSession[s];
            //            byte[] bydata = new byte[nSize];
            //            Buffer.BlockCopy(data, 0, bydata, 0, nSize);
           //            session.m_GamePack.ProcessNetData(bydata);
            //            session.m_nLastTime = System.Environment.TickCount;
            //        }
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    Log.Instance().WriteLog(ex.Message);
            //    Log.Instance().WriteLog(ex.StackTrace);
            //}
           

        }

        public static void LogicTimer(object source, ElapsedEventArgs e)
        {
            SocketCallBack.Instance().Run();
            //lock (m_DicSession)
            //{
            //    foreach (GameBase.Network.GameSession session in m_DicSession.Values)
            //    {
            //        byte[] retdata = session.m_GamePack.GetData();
            //        if (retdata != null)
            //        {
            //            GameBase.Network.PackIn packin = new GameBase.Network.PackIn(retdata);

            //            ushort tag = packin.ReadUInt16();
            //            switch (tag)
            //            {
            //                case PacketProtoco.C_LOGINUSER:
            //                    {
            //                        GameBase.Network.PacketOut packout = new GameBase.Network.PacketOut(session.GetGamePackKeyEx());
            //                        byte[] sendbyte = { 0, 1, 59, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            //                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 255, 255, 255, 255 };
            //                        packout.WriteBuff(sendbyte);
            //                        sendbyte = packout.Flush();
            //                        server.SendData(session.m_Socket, sendbyte);
            //                        break;
            //                    }

            //            }
            //        }

            //        //空连接判断-

            //    }
            //}
          
        }



    }
}
