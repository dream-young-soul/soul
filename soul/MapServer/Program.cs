using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameBase.Core;
using GameBase.Config;
using GameBase.Network;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using GameStruct;
using System.Threading;
using System.IO;
namespace MapServer
{
    class Program
    {
        public static byte _Head = 0;
        public static byte _Tail = 0;
        public static TcpServer server = null;


        static void Main(string[] args)
        {
            
           //FileStream f = new FileStream("E:\\soul\\trunk\\MapServer\\verpack.dat",FileMode.Open);
           //byte[] ret = new byte[f.Length];
           //f.Read(ret, 0, (int)f.Length);
           //f.Close();
           //ConfigManager.Instance().GetVerPacket().InitPacket(ret);
            if (!GameServer.Start())
            {
                return;
            }
            //初始化连接dbserver   
            DBServer.Instance().Init();
            //工作逻辑线程
            Thread logicThread = new Thread(new ThreadStart(ServerRun));
            logicThread.IsBackground = true;
            logicThread.Start();
            while (true)
            {
                String sCommand = Console.ReadLine();
                String[] sArr = sCommand.Split(' ');
                if (sArr.Length <= 0) continue;
                sCommand = sArr[0];
                try
                {
                    if (sCommand == "quit" || sCommand == "exit")
                    {
                        break;
                    }
                    if (sCommand == "test")
                    {
                        PlayerObject play = MapManager.Instance().GetGameMapToID(1000).GetObject(3988) as PlayerObject;
                        NetMsg.MsgUpdateSP data = new NetMsg.MsgUpdateSP();
                        data.Create(null, play.GetGamePackKeyEx());
                        data.role_id = play.GetTypeId();
                        data.value = Convert.ToUInt32(sArr[1]);
                        data.sp = Convert.ToUInt32(sArr[2]);
                        play.SendData(data.GetBuffer());
                    }
   
  
                    
                }
                catch (System.Exception ex)
                {
                    Log.Instance().WriteLog(ex.Message);
                }
              
               
            }
            GameServer.Stop();
            Log.Instance().WriteLog("exit server!");
            
            Console.ReadLine();
           
            
        }

        private static void ServerRun()
        {
           int processtime = 4;
           int sleeptime = 4;
           int nlastProcessTime = System.Environment.TickCount;
            while (true)
            {
               
               if (System.Environment.TickCount - nlastProcessTime > processtime)
               {
                    GameServer.LogicRun();
                    nlastProcessTime = System.Environment.TickCount;  
               }
               
              System.Threading.Thread.Sleep(sleeptime);
           }
        }
    }
}
