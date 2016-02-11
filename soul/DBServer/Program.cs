using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameBase.Config;
using MySql.Data.MySqlClient;
using GameBase.Network;
using System.Net.Sockets;
using System.Threading;
using GameBase.Core;

namespace DBServer
{

    public class Global
    {
        public static bool mbTestMode = false; //测试模式，随意输入帐号密码
    }
    class Program
    {
        private static TcpServer mTcpServer = null;
        
        static void Main(string[] args)
        {


            //日志路径
            Log.Instance().Init("./DBServer");
            //异常
            GameBase.Core.GlobalException.InitException();
            MemIniFile ini = new MemIniFile();
            
            if (!ini.LoadFromFile(TextDefine.GoldConfig))
            {
                return;
            }
            //载入名称过滤文件
            if (!Filter.Instance().LoadFilterNameFile(TextDefine.CONFIG_FILTERNAME))
            {
                Log.Instance().WriteLog("载入名称过滤文件失败");
            }
            //连接mysql
            String sip = ini.ReadValue(TextDefine.MysqlSection, "IP", "127.0.0.1");
            int nPort = ini.ReadValue(TextDefine.MysqlSection, "Port", 3306);
            String sUser = ini.ReadValue(TextDefine.MysqlSection, "User", "root");
            String sPaswd = ini.ReadValue(TextDefine.MysqlSection, "Passwd", "test");
            String sDataBase = ini.ReadValue(TextDefine.MysqlSection, "database", "soul");
            if (!MysqlConn.Connect(sip, nPort, sUser, sPaswd, sDataBase))
            {
                Log.Instance().WriteLog("connect mysql error!");
                return;
            }
          
            LoadGameKernel();
            //启动tcp服务器=
            sip = ini.ReadValue(TextDefine.DBServerSestion, "IP", "0.0.0.0");
            nPort = ini.ReadValue(TextDefine.DBServerSestion, "Port", 1500);
            mTcpServer = new TcpServer();
            mTcpServer.onConnect += new TcpServerEvent.OnConnectEventHandler(OnConnect);
            mTcpServer.onReceive += new TcpServerEvent.OnReceiveEventHandler(OnReceive);
            mTcpServer.onClose += new TcpServerEvent.OnCloseEventHandler(OnClose);
            if (!mTcpServer.Start(sip, nPort))
            {
                Console.WriteLine("start server error!");
                MysqlConn.Dispose();
                return;
            }
            //启动工作线程-
 
            Thread logicThread = new Thread(new ThreadStart(LogicRun));
            logicThread.IsBackground = true;
            logicThread.Start();

            while (true)
            {
                String sCommand = Console.ReadLine();
                if(sCommand == "quit" ||
                    sCommand == "exit")
                {
                    break;
                }
            }
            MysqlConn.Dispose();
        }


        public static void OnConnect(Socket s)
        {

            SessionManager.Instance().AddSession(s, mTcpServer);

        }

        public static void OnReceive(Socket s, byte[] data, int nSize)
        {
            SessionManager.Instance().ReceiveData(s,data, nSize);

        }

        public static void OnClose(Socket s)
        {

            SessionManager.Instance().RemoveSession(s);
        }

        private static void LogicRun()
        {

                int processtime = 4;
                int sleeptime = 4;
                int nlastProcessTime = System.Environment.TickCount;
                while (true)
                {

                    if (System.Environment.TickCount - nlastProcessTime > processtime)
                    {
                        SessionManager.Instance().Run();
                        //充值信息读取
                        PayManager.Instance().Run();
                        nlastProcessTime = System.Environment.TickCount;
                    }

                    System.Threading.Thread.Sleep(sleeptime);
                }

            
        }

        private static void LoadGameKernel()
        {
            //载入爵位数据
            GuanJue.GetInstance().DB_Load();
            //载入军团数据
            Legion.GetInstance().DB_Load();
            //载入充值信息
            PayManager.Instance().DB_Load();
        }
    }
}
