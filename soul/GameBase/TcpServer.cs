using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using GameBase.Config;

//异步tcp服务器
//2015.8.4
namespace GameBase.Network
{
    public class TcpServerEvent
    {
        public delegate void OnConnectEventHandler(Socket s);       //客户连接
        public delegate void OnReceiveEventHandler(Socket s, byte[] data,int nSize); //数据到达
        public delegate void OnCloseEventHandler(Socket s); //客户断开
    }

    public class StateObject
    {
        public Socket s = null;
        private const int BUFFSIZE = 1024;
        public byte[] buffer = new byte[BUFFSIZE];
        public TcpServer c;
    }
    public class TcpServer
    {
        public event TcpServerEvent.OnConnectEventHandler onConnect;
        public event TcpServerEvent.OnReceiveEventHandler onReceive;
        public event TcpServerEvent.OnCloseEventHandler onClose;
        private String m_sIP;
        private int m_nPort;
        public Socket m_Socket = null;
        private int BACKLOG = 100;
        public TcpServer()
        {

        }
 
        //启动服务器
        public bool Start(String sBindIP,int nPort)
       {
           m_sIP = sBindIP;
           m_nPort = nPort;
           IPAddress ipAddress = IPAddress.Parse(m_sIP);
           IPEndPoint localEndPoint = new IPEndPoint(ipAddress, m_nPort);
           try
           {
               m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
               m_Socket.Bind(localEndPoint);
               m_Socket.Listen(BACKLOG);
               m_Socket.BeginAccept(new AsyncCallback(AcceptCallback), this);
           }
           catch (System.Exception ex)
           {
               Console.WriteLine("启动服务器失败: 绑定ip:"+sBindIP.ToString() +"绑定端口:"+nPort.ToString()+ex.Message);
               return false;
           }
           return true;
       }

        public static void AcceptCallback(IAsyncResult ar)
        {
            StateObject state = null;
            TcpServer c = (TcpServer)ar.AsyncState;
            try
            {
                if (c == null) return;
                Socket listener = (Socket)c.m_Socket;
                Socket handler = listener.EndAccept(ar);
                c.onConnect(handler);
                state = new StateObject();
                state.s = handler;
                state.c = (TcpServer)c;
                handler.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReadCallBack), state);

                listener.BeginAccept(new AsyncCallback(AcceptCallback), c);
            }
            catch (System.Exception ex)
            {
                if (state != null)
                {
                    state.c.onClose(state.s);
                 
                    state.s.Dispose();
                }
            }
     
        }

        public static void ReadCallBack(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.s;
            if (!handler.Connected) return;
            try
            {
                SocketError error = SocketError.Disconnecting;
                int readsize = handler.EndReceive(ar,out error);
                if (error == SocketError.Success && readsize > 0)
                {
                    state.c.onReceive(state.s, state.buffer, readsize);
                    handler.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReadCallBack), state);
                }
                else //if(!handler.Connected )//客户离开
                {
                    state.c.onClose(handler);
                    //handler.Disconnect(true);
                    handler.Dispose();
                }
  
            }
            catch (System.Exception ex)
            {
                state.c.onClose(handler);
                handler.Dispose();
                Console.WriteLine(ex.Message);
            }
  
        }

        public static void SendCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            try
            {
               int retlen =  state.s.EndSend(ar);
            }
            
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                state.c.onClose(state.s);
                state.s.Close();
            }
           

        }
        public bool SendData(Socket s, byte[] data)
        {
            if (s == null) return false;
            //意外断线的异常处理
            try
            {
                StateObject state = new StateObject();
                state.c = this;
                state.s = s;

                s.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), state);
            }
            catch (System.Exception ex)
            {
                this.onClose(s);
               
               // Log.Instance().WriteLog(ex.Message);
               // Log.Instance().WriteLog(ex.StackTrace);
                return false;
            }
            return true;
        }

        public void Stop()
        {
            if (m_Socket != null)
            {
                m_Socket.Close();
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
