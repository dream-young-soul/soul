using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using GameBase.Network;
using GameBase.Network.Internal;

namespace DBServer
{
    public class SessionManager
    {
        private static SessionManager mInstance = null;
        private  Dictionary<Socket, InternalSession> mDicSession;
        private static Object _lock = new Object();
        public static SessionManager Instance()
        {
            if (mInstance == null)
            {
                mInstance = new SessionManager();
            }
            return mInstance;
        }
        public SessionManager()
        {
            mDicSession = new Dictionary<Socket, InternalSession>();
        }
    
        public void AddSession(Socket s,TcpServer server)
        {
            lock (_lock)
            {
                InternalSession session = new InternalSession(server, s);
                mDicSession[s] = session;
            }
        }

        public void RemoveSession(Socket s)
        {
            lock (_lock)
            {
                if (mDicSession.ContainsKey(s))
                {
                    InternalSession session = mDicSession[s];
                    mDicSession.Remove(s);
                }
            }
        }


        public void ReceiveData(Socket s,byte[] data, int nSize)
        {
            lock (_lock)
            {
                if (mDicSession.ContainsKey(s))
                {
                    InternalSession session = mDicSession[s];
                    byte[] msg = new byte[nSize];
                    Buffer.BlockCopy(data, 0, msg, 0, nSize);
                    session.GetPacket().ProcessNetMsg(msg);
                    session.SetLastTime(System.Environment.TickCount);
                }
            }
        }
        public void Run()
        {
            lock (_lock)
            {
                foreach (InternalSession session in mDicSession.Values)
                {
                    session.Run();
                }
            }
        }

        public InternalSession FindSessionToSocket(Socket s)
        {
            InternalSession session = null;
            if (mDicSession.ContainsKey(s))
            {
                session = mDicSession[s];
            }
            return session;
        }

        //第一个参数是多地图服务器用的- 现在暂时不用
        public void SendMapServer(int mapid, byte[] data)
        {
            lock (_lock)
            {
                foreach (InternalSession session in mDicSession.Values)
                {
                    if (session.GetSessionType() == Define.TYPE_MAPSERVER)
                    {
                        session.GetTcpServer().SendData(session.GetSocket(), data);
                        break;
                    }
                }
            }

         }

        //只有一个登录服务器
        public void SendLoginServer(byte[] data)
        {
            lock (_lock)
            {
                foreach (InternalSession session in mDicSession.Values)
                {
                    if (session.GetSessionType() == Define.TYPE_LOGINSERVER)
                    {
                        session.GetTcpServer().SendData(session.GetSocket(), data);
                        break;
                    }
                }
            }

        }
        
    }
}
