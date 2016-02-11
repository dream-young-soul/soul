using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameBase.Network;
using GameBase.Config;
using GameBase.Core;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Diagnostics;
using GameBase.Network.Internal;
namespace MapServer
{
    public class GameServer
    {
        private static TcpServer mTcpServer;
        public static TcpServer GetTcpServer(){return mTcpServer;}

      
        private static bool mbTestMode = false; //是否是测试模式
 
        public static bool IsTestMode() { return mbTestMode; }
      
        public static bool Start()
        {
            bool ret = true;
            Log.Instance().Init("./MapServer");
            GlobalException.InitException();
            //载入配置文件
            try
            {
                ConfigManager.Instance().LoadConfig();
                MemIniFile ini = new MemIniFile();
                if (!ini.LoadFromFile(TextDefine.GoldConfig))
                {
                    Log.Instance().WriteLog("load golbalconfig error!");
                    return false;
                }
                String sIP = ini.ReadValue(TextDefine.GameServerSetion, TextDefine.NormalIPKey, TextDefine.NormalIP);
                int nPort = ini.ReadValue(TextDefine.GameServerSetion, TextDefine.NormalPortKey, TextDefine.GameServerPort);

                mTcpServer = new TcpServer();
                mTcpServer.onConnect += new TcpServerEvent.OnConnectEventHandler(OnConnect);
                mTcpServer.onClose += new TcpServerEvent.OnCloseEventHandler(OnClose);
                mTcpServer.onReceive += new TcpServerEvent.OnReceiveEventHandler(OnReceive);
                if (!mTcpServer.Start(sIP, nPort))
                {
                    return false;
                }

            }
            catch (System.Exception ex)
            {
                Log.Instance().WriteLog("启动服务器失败");
                Log.Instance().WriteLog(ex.Message);
                Log.Instance().WriteLog(ex.StackTrace);
                return false;

            }
          
            

            return ret;
        }


        public static void Stop()
        {
            UserEngine.Instance().Stop();
            mTcpServer.Dispose();
            SessionManager.Instance().Dispose();
        }

        private static void OnConnect(Socket s)
        {
            SocketInfo info = new SocketInfo();
            info.type = SocketCallBack.TYPE_ONCONNECT;
            info.s = s;
            SocketCallBack.Instance().AddData(info);
        }

        private static void OnClose(Socket s)
        {
            SocketInfo info = new SocketInfo();
            info.type = SocketCallBack.TYPE_CLOSE;
            info.s = s;
            SocketCallBack.Instance().AddData(info);

        }

        private static void OnReceive(Socket s, byte[] data, int nSize)
        {
            SocketInfo info = new SocketInfo();
            info.type = SocketCallBack.TYPE_RECEIVE;
            info.s = s;
            info.data = new byte[nSize];
            Buffer.BlockCopy(data, 0, info.data, 0, nSize);
            SocketCallBack.Instance().AddData(info);
  
        }



        public static void LogicRun()
        {
            SocketCallBack.Instance().Run();        //玩家发过来的封包进行处理，加到数据队列
            DBServer.Instance().ProcessDBNetMsg();  //优先处理db数据库服务器发过来的消息
            SessionManager.Instance().ProcessNetMsg(); //处理玩家发过来的消息
            MapManager.Instance().Process();
            UserEngine.Instance().Run();
            ScriptTimerManager.Instance().Run();  //脚本定时器

            WorldPigeon.Instance().Run();   //魔法飞鸽
        }
        


     
    }
}
