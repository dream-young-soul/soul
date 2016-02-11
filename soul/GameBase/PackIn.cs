using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using GameBase.Config;
//流读入
//2015.8.5
namespace GameBase.Network
{
    public class PackIn
    {
        private MemoryStream stream;
        private BinaryReader read;
        public PackIn(byte[] data)
        {
            stream = new MemoryStream(data);
            read = new BinaryReader(stream);
        }
        ~PackIn()
        {
            stream.Close();
            read = null;
        }
        public int ReadInt32()
        {
            //超出流的读取范围
            if (read.BaseStream.Position + sizeof(int) > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadInt32 error!");
                read.BaseStream.Position = read.BaseStream.Length;
                return 0;
            }
            return read.ReadInt32();
          
        }

        public uint ReadUInt32()
        {
            //超出流的读取范围
            if (read.BaseStream.Position + sizeof(uint) > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin readuint32 error!");
                read.BaseStream.Position = read.BaseStream.Length;
                return 0;
            }
            return read.ReadUInt32();
        }

        public short ReadInt16()
        {
            //超出流的读取范围
            if (read.BaseStream.Position + sizeof(short) > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadInt16 error!");
                read.BaseStream.Position = read.BaseStream.Length;
                return 0;
            }
            return read.ReadInt16();
        }

        public ushort ReadUInt16()
        {
            //超出流的读取范围
            if (read.BaseStream.Position + sizeof(ushort) > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadUInt16 error!");
                read.BaseStream.Position = read.BaseStream.Length;
                return 0;
            }
            return read.ReadUInt16();
        }

        public long ReadLong()
        {
            if (read.BaseStream.Position + sizeof(long) > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadLong error!");
                read.BaseStream.Position = read.BaseStream.Length;
                return 0;
            }
            return read.ReadInt64();
        }

        public ulong ReadULong()
        {
            if (read.BaseStream.Position + sizeof(ulong) > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadULong error!");
                read.BaseStream.Position = read.BaseStream.Length;
                return 0;
            }
            return read.ReadUInt64();
        }
        public bool ReadBool()
        {
            if (read.BaseStream.Position + sizeof(bool) > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadBool error!");
                read.BaseStream.Position = read.BaseStream.Length;
                return false;
            }
            return read.ReadBoolean();
        }
        public byte[] ReadBuff(int len)
        {
            byte[] buf = null;
            if (read.BaseStream.Position + len > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadBool error!");
                buf = new byte[len];
                read.BaseStream.Position = read.BaseStream.Length;
                return buf;
            }
            buf = read.ReadBytes(len);
            return buf;
        }
        public float ReadFloat()
        {
            if (read.BaseStream.Position + sizeof(float) > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadFloat error!");
                read.BaseStream.Position = read.BaseStream.Length;
                return 0;
            }
            return read.ReadSingle();
        }
        public String ReadString()
        {
            if (read.BaseStream.Position + sizeof(byte) > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadString error1!");
                read.BaseStream.Position = read.BaseStream.Length;
                return "";
            }
            byte nLen = read.ReadByte();
            if (read.BaseStream.Position + nLen > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadString error2!");
                read.BaseStream.Position = read.BaseStream.Length;
                return "";
            }
            byte[] buf = read.ReadBytes(nLen);
            return GameBase.Core.Coding.GetDefauleCoding().GetString(buf);
        }
        public String ReadString(int len)
        {
            if (read.BaseStream.Position + len > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadString error3!");
                read.BaseStream.Position = read.BaseStream.Length;
                return "";
            }
            byte[] buf = read.ReadBytes(len);
            return GameBase.Core.Coding.GetDefauleCoding().GetString(buf);
        }

        public byte ReadByte()
        {
            if (read.BaseStream.Position + sizeof(byte) > read.BaseStream.Length)
            {
                Log.Instance().WriteLog("packin ReadByte error3!");
                read.BaseStream.Position = read.BaseStream.Length;
                return 0;
            }
            return read.ReadByte();
           
        }

        public bool IsComplete()
        {
            return read.BaseStream.Position == stream.Length;
        }
    }
}
