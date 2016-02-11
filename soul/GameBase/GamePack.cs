using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

//游戏封包类-[与客户端通信的封包] 处理游戏粘包，封包队列
//2015.8.5
//2015.9.1 切记不能异步操作该类
namespace GameBase.Network
{
    public class GamePack
    {
        public GamePacketKeyEx m_Key;
        private MemoryStream m_stream;
        private List<byte[]> m_ListData;
        public GamePack()
        {
            m_Key = new GamePacketKeyEx();
            m_Key.InitKey();
            m_stream = new MemoryStream();
            m_ListData = new List<byte[]>();        
        }

        public byte[] GetData()
        {
            //lock (this)
            //{
                if (m_ListData.Count > 0)
                {
                    byte[] ret =  m_ListData[0];
                    m_ListData.RemoveAt(0);
                    return ret;
                }
            //}
            return null;
        }
        public void ProcessNetData(byte[] data)
        {
           // lock (this)
            //{
                byte[] dedata = new byte[data.Length];
                Buffer.BlockCopy(data, 0, dedata, 0, data.Length);
                m_Key.DecodePacket(ref dedata, data.Length);

                m_stream.Write(dedata, 0, dedata.Length);
                PackIn packin = new PackIn(m_stream.GetBuffer());
                int nCurPos = 0; //记录当前流位置
                while (true)
                {
                    int nLen = packin.ReadUInt16();
                    if (nLen > m_stream.Length - nCurPos) //封包不是完整的
                    {
                        m_stream.SetLength(0);
                        nLen = 0;
                        break;
                    }
                    if (nLen <= 0) break;
                    nCurPos += nLen;
                    //2016.1.25 non-negative number required.
                    if (nLen - sizeof(ushort) <= 0)
                    {
                        m_stream.SetLength(0);
                        nLen = 0;
                        break;
                    }
                    byte[] reData = packin.ReadBuff(nLen - sizeof(ushort));
                    m_ListData.Add(reData);
                    if (nCurPos == m_stream.Length) break;
                }
                int rema_Len = (int)m_stream.Length - nCurPos;
                if (rema_Len > 0)
                {
                    dedata = new byte[rema_Len];
                    Buffer.BlockCopy(m_stream.GetBuffer(), nCurPos, dedata, 0, rema_Len);
                }
                m_stream.SetLength(0);
                if (rema_Len > 0) m_stream.Write(dedata, 0, dedata.Length);
           // }
      
        }

        public void SunUpdateKey(int key, int key2)
        {
            m_Key.SunUpdateKey(key, key2);
        }

        public void ResetKey()
        {
            m_Key.InitKey();
        }


    }
}
