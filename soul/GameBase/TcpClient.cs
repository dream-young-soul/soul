using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using GameBase.Config;

//异步Tcp客户端-- 
//2015.8.24
namespace GameBase.Network
{

    public class TcpClientEvent
    {
        public delegate void OnConnectEventHandler(bool isSucceed);       //连接成功失败与否
        public delegate void OnReceiveEventHandler( byte[] data, int nSize); //数据到达
        public delegate void OnCloseEventHandler(Socket s); //客户断开
    }

    public class   ClientStateObject
    {
        public Socket s = null;
        private const int BUFFSIZE = 1024;
        public byte[] buffer = new byte[BUFFSIZE];
        public TcpClient c;
    }
    public class TcpClient
    {
        private Socket mSocket = null;
        public Socket GetSocket() { return mSocket; }
        public void SetSocket(Socket s) { mSocket = s; }
        private String msIP;
        private int mnPort;
        public event TcpClientEvent.OnConnectEventHandler onConnect;
        public event TcpClientEvent.OnReceiveEventHandler onReceive;
        public event TcpClientEvent.OnCloseEventHandler onClose;

        public String GetConnectIP() { return msIP; }
        public int GetConnectPort() { return mnPort; }
       
       
        public void Connect(String ip, int port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            msIP = ip;
            mnPort = port;
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
           
            mSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), this);
        }
        public void ReConnect()
        {
            Connect(msIP, mnPort);

        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            TcpClient t = (TcpClient)ar.AsyncState;
            try
            {
                Socket s = t.GetSocket();
                s.EndConnect(ar);
                t.onConnect(true);
                ClientStateObject state = new ClientStateObject();
                state.s = s;
                state.c = t;
                s.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReceiveCallback), state);
              

            }
            catch (System.Exception ex)
            {
                t.onConnect(false);
              
                //Log.Instance().WriteLog(ex.Message);
                //Log.Instance().WriteLog(ex.StackTrace);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            ClientStateObject state = (ClientStateObject)ar.AsyncState;
            TcpClient t = state.c;
            Socket s = t.GetSocket();
            if (s == null) return ;
            try
            {
                int read = s.EndReceive(ar);
                if (read > 0)
                {
                    t.onReceive(state.buffer, read);
                    s.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    t.onClose(s);
  
                }
            }
            catch (System.Exception ex)
            {
                t.onClose(s);
                Log.Instance().WriteLog(ex.Message);
                Log.Instance().WriteLog(ex.StackTrace);

            }
        }

        public void SendData(byte[] data)
        {
            if (!mSocket.Connected) return;
            ClientStateObject state = new ClientStateObject();
            state.c = this;
            state.s = this.mSocket;
            mSocket.BeginSend(data,0,data.Length,0,new AsyncCallback(SendCallback), state);
        }

        public static void SendCallback(IAsyncResult ar)
        {
            ClientStateObject state = (ClientStateObject)ar.AsyncState;
            try
            {
                int retlen = state.s.EndSend(ar);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                state.c.onClose(state.s);
            }
        }
    }
}
