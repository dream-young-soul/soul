using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Core;

//魔法飞鸽
namespace MapServer
{

     struct PigeonInfo
    {
        public String name;
        public uint id;
        public String text;
    }
    public class WorldPigeon
    {

        private static WorldPigeon mInstance = null;
        private List<PigeonInfo> mListPeginInfo;
        private GameBase.Core.TimeOut mSendTime; //发送定时器

        private int mFuckTick;
        public static WorldPigeon Instance()
        {
            if (mInstance == null)
            {
                mInstance = new WorldPigeon();
            }
            return mInstance;
        }

        public WorldPigeon()
        {
            mListPeginInfo = new List<PigeonInfo>();
            mSendTime = new GameBase.Core.TimeOut();
            mSendTime.SetInterval(GameBase.Config.Define.WORDPIGEONSENDITME);
            mSendTime.Update();
           mFuckTick = System.Environment.TickCount;
           
        }

        public void Run()
        {
            if (mListPeginInfo.Count > 0)
            {
                if (mSendTime.ToNextTime())
                {
                    this.Send(mListPeginInfo[0]);
                    mListPeginInfo.RemoveAt(0);
                 
                }
            }
            //if (System.Environment.TickCount - mFuckTick > 5000)
            //{
            //    UserEngine.Instance().SceneNotice("圣战-仙剑修QQ群号: 306929937,小婊砸们，一起玩！！");
            //    mFuckTick = System.Environment.TickCount;
            //}
        }

        //添加魔法飞鸽信息
        //name 玩家昵称
        //id 玩家id
        //text 玩家说的话
        //返回排名位置
        //只能排一次话
        public int AddText(string name,uint id,String text)
        {
            for (int i = 0; i < mListPeginInfo.Count; i++)
            {
                if (mListPeginInfo[i].name.Length == name.Length &&
                    mListPeginInfo[i].name == name)
                {
                    return -1;
                }
            }
            PigeonInfo info;
            info.name = name;
            info.id = id;
            info.text = text;
            mListPeginInfo.Add(info);
            return mListPeginInfo.Count;
        }

        //发送飞鸽信息
        private void Send(PigeonInfo info)
        {
        
            short msgLen = 28;
            msgLen += (short)(Coding.GetDefauleCoding().GetBytes(info.name).Length + 1);
            msgLen += 17;
            msgLen += (short)(Coding.GetDefauleCoding().GetBytes(info.text).Length + 1);
            GameBase.Network.PacketOut outpack = new GameBase.Network.PacketOut();
            outpack.WriteInt16(msgLen);
            outpack.WriteInt16(1004);
            outpack.WriteInt32(0xffffff);
            outpack.WriteInt32(2017);
            outpack.WriteInt32(1419);
            outpack.WriteInt32(-1);
            outpack.WriteInt32(0);
            outpack.WriteByte(4); //四个字符串数组
            outpack.WriteString(info.name);
            outpack.WriteString("ALLUSERS");
            outpack.WriteString("1241350");
            outpack.WriteString(info.text);
            outpack.WriteByte(0);
            outpack.WriteByte(0);
            outpack.WriteByte(0);
            UserEngine.Instance().BrocatBuffer(outpack.Flush());
        }
    }

}
