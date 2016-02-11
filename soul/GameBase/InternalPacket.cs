using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GameBase.Core;
using System.Diagnostics;

//服务端内部通讯封包处理--
//2015.8.24
namespace GameBase.Network
{
     
    public class InternalPacket
    {
        private MemoryStream m_stream;
        private List<byte[]> m_ListData;
        ////头部标志
        //public static  HEAD = Convert.ToByte('#');
        ////尾部标识
        //public static byte TAIL = Convert.ToByte('!');

        //public static String _HEAD = "###";
      //  public static String _TAIL = "!!!";

        public static byte[] HEAD = { 35, 35, 35 };
        public static byte[] TAIL = { 33, 33, 33 };
        public InternalPacket()
        {
            m_stream = new MemoryStream();
            m_ListData = new List<byte[]>();
        }
        public byte[] GetData()
        {
            //lock (this)
           // {
                byte[] ret = null;
                if (m_ListData.Count > 0)
                {
                    ret = m_ListData[0];
                    m_ListData.Remove(ret);
                    return ret;
                }
                return ret;
            //}
        }
        public void ClearPacket()
        {
           // lock (this)
           // {
                m_ListData.Clear();
           // }
        }

        private int FindTag(byte[] data,byte[] tag)
        {
            for (int i = 0; i < data.Length; i++)
            {
                int nStart = i;
                int j = 0;
                for (; j < tag.Length; j++)
                {
                   
                    if (tag[j] != data[nStart])
                    {
                        break;
                    }
                    nStart++;
                    //超出了长度
                    if (nStart == data.Length)
                    {
                        j++;
                        break;
                    }

                }
                if (j == tag.Length ) { return i; }
            }
            return -1;
        }
        public void ProcessNetMsg(byte[] data)
        {
            byte[] _msg = null;
            byte[] _desmsg = null;
            m_stream.Write(data, 0, data.Length);
            _msg = m_stream.GetBuffer();
            _desmsg = new byte[(int)m_stream.Length];
            Buffer.BlockCopy(_msg, 0, _desmsg, 0, _desmsg.Length);
            String str = Coding.GetDefauleCoding().GetString(_desmsg);
            int nHead = -1; int nTail = -1;
    
            while (true)
            {
                nHead = FindTag(_desmsg,HEAD);
                if (nHead >= 0)
                {
                    nTail = FindTag(_desmsg,TAIL);
                    if (nTail > 0)
                    {
                        int nlen = nTail - nHead - TAIL.Length;
                        byte[] _newmsg = new byte[nlen];
                        Buffer.BlockCopy(_desmsg, nHead + HEAD.Length, _newmsg, 0, nlen);                      
                        m_ListData.Add(_newmsg);
                        nlen = _desmsg.Length - (nTail + TAIL.Length);
                        if (nlen == 0)
                        {
                            _desmsg = null;
                            break;
                        }
                        _newmsg = new byte[nlen];
                        Buffer.BlockCopy(_desmsg, nTail + TAIL.Length, _newmsg, 0, nlen);
                        _desmsg = _newmsg;
                        nHead = nTail = -1;
                    }
                    else break;
                }
                else break;
            }
            m_stream.SetLength(0);
            if (_desmsg != null && _desmsg.Length > 0)
            {
                m_stream.Write(_desmsg, 0, _desmsg.Length);
            }
            //这段代码会有几率出现 有重复头标识与尾部标识的封包..
            //换掉
            //2015.9.1
           // lock (this)
          //  {
            //int nLen = 0;
            //byte[] _msg = null;
            //m_stream.Write(data, 0, data.Length);
            //byte[] msg = m_stream.GetBuffer();
            //int _head = -1, _tail = -1, nPos = 0;
            //for (int i = 0; i < m_stream.Length; i++)
            //{
            //    if (msg[i] == HEAD) _head = i;
            //    if (msg[i] == TAIL && _head != -1)
            //    {
            //        已经到末尾了—
            //        if (m_stream.Length == i + 1 || msg[i + 1] == HEAD)
            //        {
            //            _tail = i;
            //            nPos = i;
            //            nLen = _tail - _head - 1;
            //            _msg = new byte[nLen];
            //            Buffer.BlockCopy(msg, _head + 1, _msg, 0, nLen);
            //            m_ListData.Add(_msg);
            //            _tail = -1;
            //            _head = -1;
            //            _msg = null;

            //        }

            //    }
            //}
            //if (_head != -1)
            //{
            //    _tail = (int)m_stream.Length - 1;
            //    nLen = _tail - _head - 1;
            //    nPos = (int)m_stream.Length - 1;
            //    _msg = new byte[nLen];
            //    Buffer.BlockCopy(msg, _head + 1, _msg, 0, nLen);
            //    m_ListData.Add(_msg);
            //}
            //nLen = (int)m_stream.Length - nPos - 1;
            //if (nLen > 0)
            //{
            //    _msg = new byte[nLen];
            //    Buffer.BlockCopy(msg, nPos + 1, _msg, 0, nLen);
            //}
            //m_stream.SetLength(0);
            //if (_msg != null) m_stream.Write(_msg, 0, _msg.Length);
           // }
        }
    }

}
