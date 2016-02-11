using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using GameBase.Config;
using GameBase.Network;

//与客户端的socket 用队列存储，然后通过核心线程处理，保持同步-
//2015.9.1
//会发生-下断后就死锁的情况。。-有待解决
namespace AccServer
{
    public class SocketInfo
    {
        public byte type = 0;
        public Socket s = null;
        public byte[] data = null;
        public GameBase.Network.GameSession session = new GameBase.Network.GameSession(null);

    }

    public class SocketCallBack
    {
        public const byte TYPE_ONCONNECT = 0;   //连接
        public const byte TYPE_RECEIVE = 2;     //数据到达
        public const byte TYPE_CLOSE = 3;       //客户关闭


        private static SocketCallBack mInstance = null;
        private List<SocketInfo> mList;
        public static SocketCallBack Instance()
        {
            if (mInstance == null)
            {
                mInstance = new SocketCallBack();
            }
            return mInstance;
        }

        public SocketCallBack()
        {
            mList = new List<SocketInfo>();
        }

        public void AddData(SocketInfo info)
        {
            //有些时候调试的时候会发生死锁现象- 待release版本测试- 2015.9.18-
            //已解决- 不是这个原因...是其他bug。。。2015.10.10
            lock (mList)
            {
                mList.Add(info);
            }
        }

        public SocketInfo GetInfo()
        {
            SocketInfo info = null;
            lock (mList)
            {
                if (mList.Count > 0)
                {
                    info = mList[0];
                    mList.RemoveAt(0);
                }
            }
            return info;
        }

        public void Run()
        {
            int runtime = System.Environment.TickCount;
           
            while (true)
            {
                if (System.Environment.TickCount - runtime > 300) break;//超出三百毫秒下次再处理
                SocketInfo info = GetInfo();
                if (info == null) break;
                if (info.s == null) break;
                Socket s = info.s;
                switch (info.type)
                {
                    case TYPE_ONCONNECT:
                        {

                            //  SessionManager.Instance().AddSession(s, GameServer.GetTcpServer());
                            break;
                        }
                    case TYPE_CLOSE:
                        {
                            // PlayerObject play = UserEngine.Instance().FindPlayerObjectToSocket(s);
                            //   SessionManager.Instance().RemoveSession(s);
                            //  UserEngine.Instance().RemovePlayObject(play);
                            //if (play != null)
                            // {
                            // play.GetGameSession().Dispose();
                            // play.SetGameSession(null);
                            //}

                            break;
                        }
                    case TYPE_RECEIVE:
                        {
                              GameBase.Network.GameSession session = info.session;
                             byte[] bydata = new byte[info.data.Length];
                             Buffer.BlockCopy(info.data, 0, bydata, 0, info.data.Length);
                             session.m_GamePack.ProcessNetData(bydata);
                            //            session.m_nLastTime = System.Environment.TickCount;

                             byte[] retdata = session.m_GamePack.GetData();
                            if (retdata != null)
                            {
                                GameBase.Network.PackIn packin = new GameBase.Network.PackIn(retdata);

                                ushort tag = packin.ReadUInt16();
                                switch (tag)
                                {

                                    case PacketProtoco.C_LOGINUSER:
                                        {
                                          
                                            GameBase.Network.PacketOut packout = new GameBase.Network.PacketOut(session.GetGamePackKeyEx());
                                            byte[] sendbyte = { 0, 1, 59, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 255, 255, 255, 255 };
                                            packout.WriteBuff(sendbyte);
                                            sendbyte = packout.Flush();
                                            Program.server.SendData(info.s, sendbyte);
                                            break;
                                        }

                                }
                            }
                            //  SessionManager.Instance().AddNetData(s, info.data, info.data.Length);
                            break;
                        }
                }
                       
               
            }
        }
    }
}
