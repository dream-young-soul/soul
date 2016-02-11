using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
//游戏会话类
//2015.8.5
namespace GameBase.Network
{
    public class IDManager
    {
        private static uint _id = 0;
        public static uint CreateGameId()
        {
            _id++;
            return _id;
        }
    }
    
    public class GameSession
    {
       
        public TcpServer server;
        public Socket m_Socket;
        public GamePack m_GamePack;
        public int m_nLastTime; //最后会话时间
        public uint gameid; //有玩家对象才会赋值
        public GameSession(Socket s,TcpServer tcpserver = null)
        {
            server = tcpserver;
            m_Socket = s;
            m_GamePack = new GamePack();
            m_nLastTime = System.Environment.TickCount;
            gameid = IDManager.CreateGameId();
        }

        ~GameSession()
        {

            
        }

         public GamePacketKeyEx GetGamePackKeyEx()
        {
            if (m_GamePack == null) return null;
            return m_GamePack.m_Key;
        }
        public TcpServer GetTcpServer()
        {
            return server;
        }

        public void SendData(byte[] data)
        {
            if (server == null || m_Socket == null)
            {
                return;
            }
            if (!server.SendData(m_Socket, data))
            {
              //  m_Socket.Dispose();
              //  m_Socket = null;
           }
        }

        public void Dispose()
        {
            server = null;
            if (m_Socket != null && m_Socket.Connected)
            {
                m_Socket.Close();
                m_Socket.Dispose();
            }
            m_Socket = null;
            m_GamePack = null;
        }
    }
}
