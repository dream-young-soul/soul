using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

//流写入 2015.8.5
namespace GameBase.Network
{
    public class PacketOut
    {
        private MemoryStream stream;
        private BinaryWriter write;
        private GamePacketKeyEx m_key;
      
        public PacketOut(GamePacketKeyEx key = null)
        {
            stream = new MemoryStream();
            write = new BinaryWriter(stream);
            m_key = key;
           
        }
        ~PacketOut()
        {
           
            stream.Dispose();
            stream = null;
            write = null;
        }
        public void WriteInt32(int v)
        {
            write.Write(v);
        }

        public void WriteUInt32(uint v)
        {
            write.Write(v);
        }

        public void WriteInt16(short v)
        {
            write.Write(v);
        }

        public void WriteUInt16(ushort v)
        {
            write.Write(v);
        }

        public void WriteLong(long v)
        {
            write.Write(v);
        }

        public void WriteULong(ulong v)
        {
            write.Write(v);
        }
        public void WriteBool(bool v)
        {
            write.Write(v);
        }
        public void WriteString(String v)
        {
            byte[] data = GameBase.Core.Coding.GetDefauleCoding().GetBytes(v);
            write.Write((byte)data.Length);
            write.Write(data);
        }

        public void WriteFloat(float v)
        {
            write.Write(v);
        }
        public int GetPostion()
        {
            return (int)write.BaseStream.Length;
        }
        public void WriteBuff(byte[] v)
        {
            write.Write(v);
            
        }
        public void WriteByte(byte v)
        {
            write.Write(v);
        }
        //取未加密的数据--
        public byte[] GetNormalBuff()
        {
            write.Flush();
            byte[] v1 = new byte[2];
            byte[] ret = stream.GetBuffer();
            v1[0] = ret[0]; v1[1] = ret[1];
            ushort nLen = BitConverter.ToUInt16(v1, 0);
            byte[] retdata = new byte[nLen];
            Buffer.BlockCopy(ret, 0, retdata, 0, nLen);
            return retdata;
        }
        //这个方法只适用于与客户端通信的封包组装
        public byte[] Flush()
        {
            write.Flush();
            byte[] ret = stream.GetBuffer();
            byte[] v1 = new byte[2];
            v1[0] = ret[0]; v1[1] = ret[1];
            ushort nLen = BitConverter.ToUInt16(v1, 0);
            if (m_key != null)
            {
            
                m_key.EncodePacket(ref ret, nLen);
            }
            byte[] retdata = new byte[nLen];
            Buffer.BlockCopy(ret, 0, retdata, 0, nLen);
            return retdata;
        }

        public byte[] GetBuffer()
        {
            byte[] ret = stream.GetBuffer();
            byte[] retdata = new byte[write.BaseStream.Length];
            Buffer.BlockCopy(ret, 0, retdata, 0, (int)write.BaseStream.Length);
            return retdata;
        }

        public void Clear(GamePacketKeyEx key = null)
        {
            m_key = key;
            stream.SetLength(0);
        }
    }
}
