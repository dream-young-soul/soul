using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameBase.Config;
using GameBase.Core;
using GameBase.Network;
using System.Collections;
using GameStruct;
using System.IO;
//消息基类
namespace NetMsg
{
    public class BaseMsg
    {
        protected byte[] m_Data; //封包数据
        protected ushort mMsgLen; //封包长度
        protected ushort mParam; //协议号
        protected GamePacketKeyEx mKey; 
        public BaseMsg()
        {
            m_Data = null;
            mKey = null;
            mMsgLen = 0;
            mParam = 0;
        }

        public virtual void Create(byte[] msg = null,GamePacketKeyEx key = null)
        {
            m_Data = msg;
            mKey = key;
        }

        public virtual void Process()
        {

        }
         public virtual void Reset()
        {

        }
        public virtual byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteBuff(m_Data);
            return outpack.Flush();
        }
    }


    //公告消息包-- 包括聊天信息啊 之类的
    public class MsgNotice : BaseMsg
    {

        public const byte TAG_ISROLE_TRUE = 0; //已有角色
        public const byte TAG_ISROLE_FALSE = 1; //没有角色
        public int rgba;     //颜色
        public ushort type; //类型
        public ushort tag; //标记
        public int param;   //未知参数
        public int param1;  //未知参数
        public int param2;  //未知参数
        public byte btype;  //类型
        public ArrayList strlist;
        public String str1;
        public String str2;
        public String str3;
        public String str4;
        public ushort param3;
        public MsgNotice()
        {
            mParam = PacketProtoco.S_NOTICE;
            mMsgLen = 27;
            strlist = new ArrayList();
            rgba = param = param1 = param2 = 0;
            type = tag = param3 = btype = 0;
        }

      
        public override void Create(byte[] msg = null,GamePacketKeyEx key = null)
        {
           
            base.Create(msg,key);
            if (msg != null)
            {
                PackIn packin = new PackIn(msg);
                rgba = packin.ReadInt32();
                type = packin.ReadUInt16();
                tag = packin.ReadUInt16();
                param = packin.ReadInt32();
                param1 = packin.ReadInt32();
                param2 = packin.ReadInt32();
                btype = packin.ReadByte();

                byte nLen = packin.ReadByte();
                str1 = packin.ReadString(nLen);
                nLen = packin.ReadByte();
                str2 = packin.ReadString(nLen);
                packin.ReadByte();
                nLen = packin.ReadByte();
                str3 = packin.ReadString(nLen);
                packin.ReadByte();
                param3 = packin.ReadUInt16();
            }
        }

        public override void Process()
        {

        }

        //构造注册名查询封包
        public byte[] GetQueryNameBuff(bool isSuccess)
        {
            rgba = 0xFFFFFF;
            type = 2100;
            tag = TAG_ISROLE_TRUE;
            param = 834;
            param1 = -1;
            param2 = 0;
            btype = 4;
            str1 = "SYSTEM";
            str2 = "ALLUSERS";
            if (isSuccess)
            {
                str3 = "REGIST_NAME_CHECK_SUC";
            }
            else str3 = "该昵称已经存在,请重新输入一个昵称!";
           
            param3 = 0;
            str4 = "";
            return GetBuffer();
        }
        //构造进入游戏的封包
        public byte[] GetStartGameBuff()
        {
            rgba = 0xFFFFFF;
            type = 2101;
            tag = TAG_ISROLE_TRUE;
            param = 834;
            param1 = -1;
            param2 = 0;
            btype = 4;
            str1 = "SYSTEM";
            str2 = "ALLUSERS";
            str3 = "ANSWER_OK";
            param3 = 0;
            str4 = "";
            return GetBuffer();
        }
        public byte[] GetChatNoticeBuff(String text)
        {
            rgba = 0xFFFFFF;
            type = 2000;
            tag = 0;
            param = 834;
            param1 = -1;
            param2 = 0;
            btype = 4;
            str1 = "SYSTEM";
            str2 = "ALLUSERS";
            str3 = text;
            str4 = "";
            param3 = 0;
            return GetBuffer();
        }
        //取信息框
        public byte[] GetMsgBoxBuff(String text)
        {
            rgba = 0xFFFFFF;
            type = 2112;
            tag = 0;
            param = 2325;
            param1 = -1;
            param2 = 0;
            btype = 4;
            str1 = "SYSTEM";
            str2 = "ALLUSERS";
            str3 = text;
            str4 = "";
            param3 = 0;
            return GetBuffer();
        }
        //构造中间公告包
        public byte[] GetSceneNoticeBuff(String text)
        {
            rgba = 0xFFFFFF;
            type = 2011;
            tag = 0;
            param = 834;
            param1 = -1;
            param2 = 0;
            btype = 4;
            str1 = "SYSTEM";
            str2 = "ALLUSERS";
            str3 = text;
            str4 = "";
            param3 = 0;
            return GetBuffer();
        }
        //构造创建角色的封包
        public byte[] GetCreateRoleBuff()
        {
            rgba = 0xFFFFFF;
            type = 2101;
            tag = TAG_ISROLE_FALSE;
            param = 834;
            param1 = -1;
            param2 = 0;
            btype = 4;
            str1 = "SYSTEM";
            str2 = "ALLUSERS";
            str3 = "NEW_ROLE";
            str4 = "";
            param3 = 0;
            return GetBuffer();

        }
        public override byte[] GetBuffer()
        {
            strlist.Add(str1);
            strlist.Add(str2);
            strlist.Add(str3);
            strlist.Add(str4);
            PacketOut pout = new PacketOut(mKey);
            ushort nLen = 0;
            String str;
          
            for (int i = 0; i < strlist.Count; i++)
            {
                str = (String)strlist[i];
                nLen += (ushort)(Coding.GetDefauleCoding().GetBytes(str).Length + sizeof(byte));
            }
            mMsgLen += 1;
            mMsgLen += nLen;
            pout.WriteUInt16(mMsgLen);
            pout.WriteUInt16(mParam);
            pout.WriteInt32(rgba);
            pout.WriteUInt16(type);
            pout.WriteUInt16(tag);
            pout.WriteInt32(param);
            pout.WriteInt32(param1);
            pout.WriteInt32(param2);
            pout.WriteByte(btype);
          
            pout.WriteString(str1);
           
            pout.WriteString(str2);

            pout.WriteByte(0);
            pout.WriteString(str3);
       

            //pout.WriteString(str4);

            
            pout.WriteByte(0);
            pout.WriteUInt16(param3);
            return pout.Flush();
        }
    }
    //登录消息包
    public class MsgLogin : BaseMsg
    {
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg,key);
        }

        public override void Process()
        {

        }
    }

    
    //下发npc消息
    public class MsgNpcInfo : BaseMsg
    {
        public uint mnID;         //NPC ID
        public short mnX;       //X坐标
        public short mnY;      //Y坐标
        public int lookface;        //lookface
       
        public MsgNpcInfo()
        {
            mMsgLen = 32;
            mParam = PacketProtoco.S_NPCINFO;
           mnX = mnY =  0;
        
           mnID = 0;
         
            lookface = 0;
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }
        public void Init(uint id, short x, short y, int _lookface )
        {
            mnID = id;
            mnX = x;
            mnY = y;
            lookface = _lookface;
        
        }
        public override byte[] GetBuffer()
        {
            PacketOut packout = new PacketOut(mKey);
            packout.WriteUInt16(mMsgLen);
            packout.WriteUInt16(mParam);
            packout.WriteUInt32(mnID);
            packout.WriteInt16(mnX);
            packout.WriteInt16(mnY);
            packout.WriteInt32(lookface);
      
            byte[] data = { 2, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            packout.WriteBuff(data);
            //packout.WriteUInt16(mnState);
            //packout.WriteUInt16(mnParam2);
            //packout.WriteUInt16(mnParam3);
            //packout.WriteUInt16(mnParam4);
            //packout.WriteUInt16(mnParam5);
            return packout.Flush();
        }
        
    }

    //地图信息
    public class MsgMapInfo : BaseMsg
    {

        public const int ENTERMAP = 9541;   //第一次进游戏进入地图
        public int ID;
        public uint MapID;
        public short x;
        public short y;
        public int Param;
        public uint MapID2;
        public int LoadTag;
        public MsgMapInfo()
        {
            mParam = PacketProtoco.S_MAPINFO;
            mMsgLen = 28;
            ID = Param = 0;
            MapID = MapID2 = 0;
            x = y = 0;
            LoadTag = 0;
            ID = (int)System.Environment.TickCount;
        }

        public void Init(uint id, short xx, short yy,int _tag)
        {
            MapID = id;
            MapID2 = MapID;
            x = xx;
            y = yy;
           // LoadTag = 9541;
            LoadTag = _tag;
        }
        public override byte[] GetBuffer()
        {
            PacketOut packout = new PacketOut(mKey);
            packout.WriteUInt16(mMsgLen);
            packout.WriteUInt16(mParam);
            packout.WriteInt32(ID);
            packout.WriteUInt32(MapID);
            packout.WriteInt16(x);
            packout.WriteInt16(y);
            packout.WriteInt32(Param);
            packout.WriteUInt32(MapID2);
            packout.WriteInt32(LoadTag);
            return packout.Flush();
        }
    }
    //角色自身信息
    public class MsgSelfRoleInfo : BaseMsg
    {
        public String name;
        public uint roleid; //角色id
        public uint lookface;   //外观与头像
        public uint hair;   //头发
        public uint gold;   //金币
        public uint gamegold;   //魔石
        public uint exp;        //当前经验值
        public uint expparam;   //置当前经验值参数
        public uint mentorexp;  //导师经验
        public uint mercenarexp;
        public uint potential;
        public ushort attackpower; //攻击力
        public ushort constitution;
        public ushort doage;    //闪避
        public ushort decdoage; //减少闪避率
        public ushort health;
        public ushort magic_attack;//魔法攻击
        public ushort addpoint;
        public ushort life;       //当前生命
        public ushort maxlife;    //总生命
        public ushort manna;        //当前魔法值
        public uint param;
        public uint param1;
        public ushort pk;       //pk值
        public byte level;      //等级
        public byte profession; //职业
        public byte param2;
        public byte param3 = 1;
        public byte param4  =1;
        public byte mentorlevel = 5; //导师等级
        public byte param14 = 1;
        public byte guanjue = 1;
        public ushort maxpetcall = 2; //最大召唤宠物数量
        public int exploit;
        public int bonuspoint;
        public byte edubroodpacksize;
        public byte winglevel;
        public byte godpetpackagelimit;
        public byte demonlev;
        public int demonexp;
        public int demonexpparam;
        public int param5 = 262164;
        public int godlevel;//神等级
        public byte param9;
        public byte param11;
        public ushort param10;

        public int[] param6 = new int[21];
        public int originalserverid;
        public ushort wordtreeareaid;
        public int[] param7 = new int[12];
        public ushort param8 = 0;

        public byte[] param13;

        public MsgSelfRoleInfo()
        {
     
            mMsgLen = 259;
            mParam = PacketProtoco.S_SELFROLEINFO;
            name = "";
          
            param13 = new byte[3];
            for (int i = 0; i < param13.Length; i++) { param13[i] = 0; }
        

        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null )
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadInt16();
  

                roleid = inpack.ReadUInt32();
                lookface = inpack.ReadUInt32();
                hair = inpack.ReadUInt32();
                gold = inpack.ReadUInt32();
                gamegold = inpack.ReadUInt32();
                exp = inpack.ReadUInt32();
                expparam = inpack.ReadUInt32();
                mentorexp = inpack.ReadUInt32();
                mercenarexp = inpack.ReadUInt32();
                potential = inpack.ReadUInt32();
                attackpower = inpack.ReadUInt16();
                constitution = inpack.ReadUInt16();
                doage = inpack.ReadUInt16();
                decdoage = inpack.ReadUInt16();
                health = inpack.ReadUInt16();
                magic_attack = inpack.ReadUInt16();
                addpoint = inpack.ReadUInt16();
                life = inpack.ReadUInt16();
                maxlife = inpack.ReadUInt16();
                manna = inpack.ReadUInt16();
                param = inpack.ReadUInt32();
                param1 = inpack.ReadUInt32();
                pk = inpack.ReadUInt16();
                level = inpack.ReadByte();
                profession = inpack.ReadByte();
                param2 = inpack.ReadByte();
                param3 = inpack.ReadByte();
                param4 = inpack.ReadByte();
                mentorlevel = inpack.ReadByte();
                param14 = inpack.ReadByte();
                guanjue = inpack.ReadByte();
       
              //  Mercenarylevel = inpack.ReadUInt16();
                maxpetcall = inpack.ReadUInt16();
                exploit = inpack.ReadInt32();
                bonuspoint = inpack.ReadInt32();
                edubroodpacksize = inpack.ReadByte();
                winglevel = inpack.ReadByte();
                godpetpackagelimit = inpack.ReadByte();
                demonlev = inpack.ReadByte();
                demonexp = inpack.ReadInt32();
                demonexpparam = inpack.ReadInt32();
                param5 = inpack.ReadInt32();
                godlevel = inpack.ReadInt32();
                param9 = inpack.ReadByte();
                param10 = inpack.ReadUInt16();
                param11 = inpack.ReadByte();
    
                for (int i = 0; i < param6.Length; i++)
                {
                    param6[i]= inpack.ReadInt32();
                }
                originalserverid = inpack.ReadInt32();
                wordtreeareaid = inpack.ReadUInt16();
                for (int i = 0; i < param7.Length; i++)
                {
                    param7[i] = inpack.ReadInt32();
                }
                param8 = inpack.ReadUInt16();
       

            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            byte nNameLen = (byte)GameBase.Core.Coding.GetDefauleCoding().GetBytes(name).Length;
            mMsgLen += nNameLen;
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(roleid);
            outpack.WriteUInt32(lookface);
            outpack.WriteUInt32(hair);
            outpack.WriteUInt32(gold);
            outpack.WriteUInt32(gamegold);
            outpack.WriteUInt32(exp);
            outpack.WriteUInt32(expparam);
            outpack.WriteUInt32(mentorexp);
            outpack.WriteUInt32(mercenarexp);
            outpack.WriteUInt32(potential);
            outpack.WriteUInt16(attackpower);
            outpack.WriteUInt16(constitution);
            outpack.WriteUInt16(doage);
            outpack.WriteUInt16(decdoage);
            outpack.WriteUInt16(health);
           // outpack.WriteUInt16(soul);
            outpack.WriteUInt16(magic_attack);
            outpack.WriteUInt16(addpoint);
            outpack.WriteUInt16(life);
            outpack.WriteUInt16(maxlife);
            outpack.WriteUInt16(manna);
            outpack.WriteUInt32(param);
            outpack.WriteUInt32(param1);
            outpack.WriteUInt16(pk);
            outpack.WriteByte(level);
            outpack.WriteByte(profession);
            outpack.WriteByte(param2);
            outpack.WriteByte(param3);
            outpack.WriteByte(param4);
            outpack.WriteByte(mentorlevel);
            outpack.WriteByte(param14);
            outpack.WriteByte(guanjue);
           // outpack.WriteUInt16(Mercenarylevel);
            outpack.WriteUInt16(maxpetcall);
            outpack.WriteInt32(exploit);
            outpack.WriteInt32(bonuspoint);
            outpack.WriteByte(edubroodpacksize);
            outpack.WriteByte(winglevel);
            outpack.WriteByte(godpetpackagelimit);
            outpack.WriteByte(demonlev);
            outpack.WriteInt32(demonexp);
            outpack.WriteInt32(demonexpparam);
            outpack.WriteInt32(param5);
            outpack.WriteInt32(godlevel);
            outpack.WriteByte(param9);
            outpack.WriteByte(param11);
            outpack.WriteUInt16(param10);
    
            for (int i = 0; i < param6.Length; i++)
            {
                outpack.WriteInt32(param6[i]);
            }
            outpack.WriteInt32(originalserverid);
            outpack.WriteUInt16(wordtreeareaid);
            for (int i = 0; i < param7.Length; i++)
            {
                outpack.WriteInt32(param7[i]);
            }
            outpack.WriteUInt16(param8);
            outpack.WriteByte(2);
            outpack.WriteString(name);
            for (int i = 0; i < param13.Length; i++)
            {
                outpack.WriteByte(param13[i]);
            }
         
      
            return outpack.Flush();
        }
    }
    //public class MsgSelfRoleInfo : BaseMsg
    //{

    //    public uint roleid;     //角色id
    //    public uint roletype;   //角色类型      可能是角色外观
    //    public int param;
    //    public uint gold;       //金币
    //    public uint gamegold;   //魔石
    //    public uint exp;        //当前经验值
    //    public int[] param1;       //未知参数
    //    public int logincount;  //登录计次
    //    public ushort attack;     //攻击力-
    //    public ushort param2;       //未知参数
    //    public ushort dodge;        //闪避
    //    public ushort state;        //被攻击状态
    //    public ushort param3;
    //    public ushort magic_attack; //魔法攻击
    //    public ushort param4;
    //    public ushort hp;           //当前生命
    //    public ushort hp_max;       //最大生命
    //    public ushort mp;           //当前魔法
    //    public int param5;
    //    public int param6;
    //    public byte lv;             //等级
    //    public byte param7 = 10;         //未知
    //    public byte param8;
    //    public int param9 = 1;
    //    public int param10 = 2;
    //    public byte[] param11;
    //    public String name;         //名称
    //    // public String param12;
    //    public byte[] param13;

    //    public MsgSelfRoleInfo()
    //    {
    //        mMsgLen = 252;
    //        mParam = PacketProtoco.S_SELFROLEINFO;
    //        param1 = new int[3];
    //        for (int i = 0; i < param1.Length; i++) { param1[i] = 0; }
    //        param11 = new byte[160];
    //        for (int i = 0; i < param11.Length; i++) { param11[i] = 0; }
    //        param11[159] = 2;
    //        param13 = new byte[6];
    //        for (int i = 0; i < param13.Length; i++) { param13[i] = 0; }
    //        param13[0] = 2;
    //        roleid = 1325068;
    //        roletype = 150001;
    //        param = 101;
    //        gold = 2741;
    //        gamegold = 0;
    //        exp = 1000;
    //        logincount = 227;
    //        attack = 65;
    //        param2 = 0;
    //        dodge = 27;
    //        state = 100;
    //        param3 = 89;
    //        magic_attack = 91;
    //        param4 = 0;
    //        hp = 890;
    //        hp_max = 890;
    //        mp = 1820;
    //        param5 = param6 = 0;
    //        lv = 149;
    //        param7 = 10;
    //        param8 = 0;
    //        param9 = 1;
    //        param10 = 512;
    //        name = "伊枫[PM]";

    //    }
    //    public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
    //    {
    //        base.Create(msg, key);
    //        if (msg != null)
    //        {
    //            PackIn inpack = new PackIn(msg);
    //            inpack.ReadUInt16();

    //            //roleid = inpack.ReadUInt32();
    //            inpack.ReadUInt32();
    //            roletype = inpack.ReadUInt32();
    //            param = inpack.ReadInt32();
    //            gold = inpack.ReadUInt32();
    //            gamegold = inpack.ReadUInt32();
    //            exp = inpack.ReadUInt32();
    //            for (int i = 0; i < param1.Length; i++)
    //            {
    //                param1[i] = inpack.ReadInt32();
    //            }
    //            logincount = inpack.ReadInt32();
    //            attack = inpack.ReadUInt16();
    //            param2 = inpack.ReadUInt16();
    //            dodge = inpack.ReadUInt16();
    //            state = inpack.ReadUInt16();
    //            param3 = inpack.ReadUInt16();
    //            magic_attack = inpack.ReadUInt16();
    //            param4 = inpack.ReadUInt16();
    //            hp = inpack.ReadUInt16();
    //            hp_max = inpack.ReadUInt16();
    //            mp = inpack.ReadUInt16();
    //            param5 = inpack.ReadInt32();
    //            param6 = inpack.ReadInt32();
    //            inpack.ReadUInt16();
    //            lv = inpack.ReadByte();
    //            param7 = inpack.ReadByte();
    //            param8 = inpack.ReadByte();
    //            param9 = inpack.ReadInt32();
    //            param10 = inpack.ReadInt32();
    //            for (int i = 0; i < param11.Length; i++)
    //            {
    //                param11[i] = inpack.ReadByte();
    //            }
    //            byte nlen = inpack.ReadByte();
    //            name = inpack.ReadString(nlen);

    //            for (int i = 0; i < param13.Length; i++)
    //            {
    //                param13[i] = inpack.ReadByte();
    //            }
    //        }
    //    }

    //    public override byte[] GetBuffer()
    //    {
    //        PacketOut outpack = new PacketOut(mKey);
    //        byte nNameLen = (byte)GameBase.Core.Coding.GetDefauleCoding().GetBytes(name).Length;
    //        mMsgLen += nNameLen;
    //        outpack.WriteUInt16(mMsgLen);
    //        outpack.WriteUInt16(mParam);
    //        outpack.WriteUInt32(roleid);
    //        outpack.WriteUInt32(roletype);
    //        outpack.WriteInt32(param);
    //        outpack.WriteUInt32(gold);
    //        outpack.WriteUInt32(gamegold);
    //        outpack.WriteUInt32(exp);
    //        for (int i = 0; i < param1.Length; i++)
    //        {
    //            outpack.WriteInt32(param1[i]);
    //        }
    //        outpack.WriteInt32(logincount);
    //        outpack.WriteUInt16(attack);
    //        outpack.WriteUInt16(param2);
    //        outpack.WriteUInt16(dodge);
    //        outpack.WriteUInt16(state);
    //        outpack.WriteUInt16(param3);
    //        outpack.WriteUInt16(magic_attack);
    //        outpack.WriteUInt16(param4);
    //        outpack.WriteUInt16(hp);
    //        outpack.WriteUInt16(hp_max);
    //        outpack.WriteUInt16(mp);
    //        outpack.WriteInt32(param5);
    //        outpack.WriteInt32(param6);
    //        outpack.WriteUInt16(0);
    //        outpack.WriteByte(lv);
    //        outpack.WriteByte(param7);
    //        outpack.WriteByte(param8);
    //        outpack.WriteInt32(param9);
    //        outpack.WriteInt32(param10);
    //        //for (int i = 0; i < param11.Length; i++)
    //        //{
    //        //    outpack.WriteByte(param11[i]);
    //        //}

    //        outpack.WriteString(name);
    //        for (int i = 0; i < param13.Length; i++)
    //        {
    //            outpack.WriteByte(param13[i]);
    //        }
    //        return outpack.Flush();
    //    }
    //}

    public class MsgMoveInfo : BaseMsg
    {
        public int time; //时间戳
        public uint id; //角色id
        public short x; //x坐标
        public short y; //y坐标
        public byte dir; //方向
        public byte ucMode; //是否是跑步模式
       
        public ushort param; //参数 0
        public int param2;//参数1 0
        public MsgMoveInfo()
        {
            mMsgLen = 24;
            mParam = PacketProtoco.C_MOVE;
            time = (int)System.Environment.TickCount;
            id = 0;
            x = y =  0;
            ucMode = dir = 0;
            param = 0;
            param2 = 0;
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null && msg.Length == mMsgLen - 2 )
            {
                PackIn packin = new PackIn(msg);
              
                packin.ReadUInt16(); //协议号
                time = packin.ReadInt32();
                id = packin.ReadUInt32();
                x = packin.ReadInt16();
                y = packin.ReadInt16();
                dir = packin.ReadByte();
                ucMode = packin.ReadByte();
                param = packin.ReadUInt16();
                param2 = packin.ReadInt32();
                
            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut packout = new PacketOut(mKey);
            packout.WriteUInt16(mMsgLen);
            packout.WriteUInt16(mParam);
            packout.WriteInt32(time);
            packout.WriteUInt32(id);
            packout.WriteInt16(x);
            packout.WriteInt16(y);
            packout.WriteByte(dir);
            packout.WriteByte(ucMode);
            packout.WriteUInt16(param);
            packout.WriteInt32(param2);
            return packout.Flush();
        }
    }

    //刷新怪物
    public class MsgMonsterInfo :BaseMsg
    {
        public uint id;     //怪物id
        public int param;   //参数
        public int param1;  //携带物品?
        public int[] param2; //未知
        public uint lookface;    //外观
        public short x;    //x
        public short y;    //y
        public ushort hp_;   //血量?
        public ushort level;    //等级
        public uint typeid;      //怪物id
        public int maxhp;  //未知参数 血量 
        public int hp;  //未知参数 最大血量
        public int dir;
        public MsgMonsterInfo()
        {
            mMsgLen = 72;
            mParam = PacketProtoco.S_MONSTERINFO;
            param2 = new int[7];
            for (int i = 0; i < param2.Length; i++)
            {
                param2[i] = 0;
            }
            lookface = 0;
            id = typeid = 0;
            param =  param1 =  dir = 0;
            maxhp = hp = 0;
            level = hp_ = 0;
            x = y = 0;

        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null && msg.Length == mMsgLen - 2)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                id = inpack.ReadUInt32();
                param = inpack.ReadInt32();
                param = inpack.ReadInt32();
                for (int i = 0; i < param2.Length; i++)
                {
                    param2[i] = inpack.ReadInt32();
                }
                lookface = inpack.ReadUInt32();
                x = inpack.ReadInt16();
                y = inpack.ReadInt16();
                hp_ = inpack.ReadUInt16();
                level = inpack.ReadUInt16();
                typeid = inpack.ReadUInt32();
                maxhp = inpack.ReadInt32();
                hp = inpack.ReadInt32();
                dir = inpack.ReadInt32();
            }
        }
        public override byte[] GetBuffer()
        {
            PacketOut packout = new PacketOut(mKey);
            packout.WriteUInt16(mMsgLen);
            packout.WriteUInt16(mParam);
            packout.WriteUInt32(id);
            packout.WriteInt32(param);
            packout.WriteInt32(param1);
            for(int i = 0;i < param2.Length;i++)
            {
                packout.WriteInt32(param2[i]);
            }
            packout.WriteUInt32(lookface);
            packout.WriteInt16(x);
            packout.WriteInt16(y);
            packout.WriteUInt16(hp_);
            packout.WriteUInt16(level);
            packout.WriteUInt32(typeid);
            packout.WriteInt32(maxhp);
            packout.WriteInt32(hp);
            packout.WriteInt32(dir);
            return packout.Flush();
        }
    }

    //打开npc
    public class MsgOpenNpc : BaseMsg
    {
        public uint id; //npcid
        public int param;
        public int param1;
        public MsgOpenNpc()
        {
            mMsgLen = 16;
            mParam = PacketProtoco.C_OPENNPC;
        }
        
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            
            base.Create(msg, key);
            if (msg.Length == mMsgLen - 2)
            {
                PackIn packin = new PackIn(msg);
                packin.ReadUInt16();
                id = packin.ReadUInt32();
                param = packin.ReadInt32();
                param1 = packin.ReadInt32();
            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut packout = new PacketOut(mKey);
            packout.WriteUInt16(mMsgLen);
            packout.WriteUInt16(mParam);
            packout.WriteUInt32(id);
            packout.WriteInt32(param);
            packout.WriteInt32(param1);
            return base.GetBuffer();
        }
    }

    //体力值更新
    public class MsgSPInfo : BaseMsg
    {
        public uint id; //角色id
        public int type; //类型
        public int param; //未知
        public int sp; //sp值
        public MsgSPInfo()
        {
            mMsgLen = 20;
            mParam = PacketProtoco.S_SPINFO;
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg.Length == mMsgLen - 2)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                id = inpack.ReadUInt32();
                type = inpack.ReadInt32();
                param = inpack.ReadInt32();
                sp = inpack.ReadInt32();
            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(id);
            outpack.WriteInt32(type);
            outpack.WriteInt32(param);
            outpack.WriteInt32(sp);
            return outpack.Flush();
        }
    }

    //npc对话框回复
    public class MsgNpcReply : BaseMsg
    {
        public int param; //参数1
        public ushort param2; //参数2
        public byte optionid; //页面索引
        public ushort interactType; //
        public String text;
        public byte[] param3; //预留字节

        //npc对话结尾标记
        private static byte[] flushdata = {16,0,240,7,0,0,0,0,0,0,255,100,0,0,0,0};
        public MsgNpcReply()
        {
            Reset();

        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                param = inpack.ReadInt32();
                param2 = inpack.ReadUInt16();
                optionid = inpack.ReadByte();
                interactType = inpack.ReadUInt16();
                byte nlen = inpack.ReadByte();
                text = inpack.ReadString(nlen);
                for (int i = 0; i < param3.Length; i++)
                {
                    param3[i] = inpack.ReadByte();
                }
            }
        }

        public override void Reset()
        {
            mMsgLen = 14;
            mParam = PacketProtoco.S_NPCREPLY;
            param3 = new byte[3];
        }
        public override byte[] GetBuffer()
        {

            mMsgLen += (ushort)GameBase.Core.Coding.GetDefauleCoding().GetBytes(text).Length;
            mMsgLen += (ushort)param3.Length;
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(param);
            outpack.WriteUInt16(param2);
            outpack.WriteByte(optionid);
            outpack.WriteUInt16(interactType);
           
            outpack.WriteString(text);
            for(int i = 0;i < param3.Length;i++)
            {
                outpack.WriteByte(param3[i]);
            }

            return outpack.Flush();
        }

        //结尾标记
        public  byte[] Flush()
        {
            byte[] b = new byte[flushdata.Length];
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteBuff(flushdata);
            return outpack.Flush();
        }

        //npc头像
        public byte[] NpcImage(ushort imageid)
        {
            byte[] b = { 16, 0, 240, 7, 0, 0, 0, 0};
            byte[] end = { 255, 4, 0, 0, 0, 0 };
            PacketOut outpack = new PacketOut(mKey);

            outpack.WriteBuff(b);
            outpack.WriteUInt16(imageid);
            outpack.WriteBuff(end);

            return outpack.Flush();
        }
    }


    public class MsgAttackInfo : BaseMsg
    {
        public int time;
        public uint roleId;//角色id
        public uint idTarget; //怪物id
        public ushort usPosX;
        public ushort usPosY;
        public uint tag; //2、普通攻击  21、技能攻击
        public ushort skillid;//技能id
        public uint usType;
        public byte[] param;
        public MsgAttackInfo()
        {
            mMsgLen = 40;
            mParam = PacketProtoco.C_ATTACK;
            param = new byte[12];
            roleId = idTarget = 0;
            usPosX = usPosY = 0;
            tag = 0;
            skillid = 0;
            usType = 0;
            time = 0;
            for (int i = 0; i < param.Length; i++)
            {
                param[i] = 0;
            }
            
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null )
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                time = inpack.ReadInt32();
                roleId = inpack.ReadUInt32();
                idTarget = inpack.ReadUInt32();
                usPosX = inpack.ReadUInt16();
                usPosY = inpack.ReadUInt16();
                tag = inpack.ReadUInt32();
                usType = inpack.ReadUInt32();
                int j = 12;
                if (tag == 2) j = 11;
                for (int i = 0; i < j; i++)
                {
                    param[i] = inpack.ReadByte();
                }
            }
        }
        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(time);
            outpack.WriteUInt32(roleId);
            outpack.WriteUInt32(idTarget);
            outpack.WriteUInt16(usPosX);
            outpack.WriteUInt16(usPosY);
            outpack.WriteUInt32(tag);
            outpack.WriteUInt16(skillid);
            outpack.WriteUInt32(usType);
 
            for (int i = 0; i < param.Length; i++)
            {
                outpack.WriteByte(param[i]);
            }
            return outpack.Flush();
        }
    }

    //怪物受伤害值
    public class MsgMonsterInjuredInfo : BaseMsg
    {
        public int time;                //时间
        public uint roleid;             //角色id
        public uint monsterid;          //怪物id
        public short role_x;           //角色x
        public short role_y;           //角色y
        public uint tag;                 //标记
        public uint injuredvalue;       //攻击伤害值
        public int[] param;           //未知参数
        public MsgMonsterInjuredInfo()
        {
            mMsgLen = 40;
            mParam = PacketProtoco.S_ATTACK;
            time = 0;
            roleid = monsterid = injuredvalue = 0;
            role_x = role_y = 0;
            tag = 2;
            param = new int[3];
            for (int i = 0; i < param.Length; i++)
            {
                param[i] = 0;
            }
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null && msg.Length == mMsgLen - 2)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                time = inpack.ReadInt32();
                roleid = inpack.ReadUInt32();
                monsterid = inpack.ReadUInt32();
                role_x = inpack.ReadInt16();
                role_y = inpack.ReadInt16();
                tag = inpack.ReadUInt32();
                injuredvalue = inpack.ReadUInt32();
                for (int i = 0; i < param.Length; i++)
                {
                    param[i] = inpack.ReadInt32();
                }

            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(time);
            outpack.WriteUInt32(roleid);
            outpack.WriteUInt32(monsterid);
            outpack.WriteInt16(role_x);
            outpack.WriteInt16(role_y);
            outpack.WriteUInt32(tag);
            outpack.WriteUInt32(injuredvalue);
            for (int i = 0; i < param.Length; i++)
            {
                outpack.WriteInt32(param[i]);
            }
            return outpack.Flush();
        }
         
    }
    //怪物魔法攻击受伤害值
 
    public class MsgMonsterMagicInjuredInfo : BaseMsg
    {
        public int time;                //时间
        public uint roleid;             //角色id
        public uint monsterid;          //怪物id
        public short role_x;           //角色x
        public short role_y;           //角色y
        public uint tag;                 //标记
        public ushort magicid;          //技能id
        public ushort magiclv;      //技能等级
        public uint injuredvalue;       //攻击伤害值
        public int[] param;           //未知参数
        public MsgMonsterMagicInjuredInfo()
        {
            mMsgLen = 44;
            mParam = PacketProtoco.S_ATTACK;
            time = 0;
            roleid = monsterid = injuredvalue = 0;
            role_x = role_y = 0;
            tag = 2;
            param = new int[3];
            for (int i = 0; i < param.Length; i++)
            {
                param[i] = 0;
            }
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null && msg.Length == mMsgLen - 2)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                time = inpack.ReadInt32();
                roleid = inpack.ReadUInt32();
                monsterid = inpack.ReadUInt32();
                role_x = inpack.ReadInt16();
                role_y = inpack.ReadInt16();
                tag = inpack.ReadUInt32();
                magicid = inpack.ReadUInt16();
                magiclv = inpack.ReadUInt16();
                injuredvalue = inpack.ReadUInt32();
                for (int i = 0; i < param.Length; i++)
                {
                    param[i] = inpack.ReadInt32();
                }

            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            if (injuredvalue == 0)
            {
                mMsgLen = 40;
            }
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(time);
            outpack.WriteUInt32(roleid);
            outpack.WriteUInt32(monsterid);
            outpack.WriteInt16(role_x);
            outpack.WriteInt16(role_y);
            outpack.WriteUInt32(tag);
            outpack.WriteUInt16(magicid);
            outpack.WriteUInt16(magiclv);
            if (injuredvalue > 0)
            {
                outpack.WriteUInt32(injuredvalue);
            }
           
            for (int i = 0; i < param.Length; i++)
            {
                outpack.WriteInt32(param[i]);
            }
           // Log.Instance().WriteLog(GameBase.Network.GamePacketKeyEx.byteToText(outpack.GetNormalBuff()));
            return outpack.Flush();
        }

    }
    //怪物死亡--
    //对象死亡 也发这个
    public class MsgMonsterDieInfo : BaseMsg
    {
        public int time;                //时间
        public uint roleid;             //角色id
        public uint monsterid;          //怪物id
        public short  role_x;           //角色x
        public short role_y;           //角色y
        public uint tag;                 //标记
        public uint injuredvalue;       //攻击伤害值
        public int[] param;           //未知参数
        public MsgMonsterDieInfo()
        {
            mMsgLen = 40;
            mParam = PacketProtoco.S_ATTACK;
            time = 0;
            roleid = monsterid = injuredvalue = 0;
            role_x = role_y = 0;
            tag = 14;
            param = new int[3];
            for (int i = 0; i < param.Length; i++)
            {
                param[i] = 0;
            }
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null && msg.Length == mMsgLen - 2)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                time = inpack.ReadInt32();
                roleid = inpack.ReadUInt32();
                monsterid = inpack.ReadUInt32();
                role_x = inpack.ReadInt16();
                role_y = inpack.ReadInt16();
                tag = inpack.ReadUInt32();
                injuredvalue = inpack.ReadUInt32();
                for (int i = 0; i < param.Length; i++)
                {
                    param[i] = inpack.ReadInt32();
                }

            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(time);
            outpack.WriteUInt32(roleid);
            outpack.WriteUInt32(monsterid);
            outpack.WriteInt16(role_x);
            outpack.WriteInt16(role_y);
            outpack.WriteUInt32(tag);
            outpack.WriteUInt32(injuredvalue);
            for (int i = 0; i < param.Length; i++)
            {
                outpack.WriteInt32(param[i]);
            }
            return outpack.Flush();
        }

    }


    //清除怪物信息
    public class MsgClearObjectInfo : BaseMsg
    {
        public int time;
        public uint id;
        public ushort x;
        public ushort y;
        public int param;
        public uint mapid;
        public uint tag;
        public MsgClearObjectInfo()
        {
            mMsgLen = 28;
            mParam = PacketProtoco.S_CLEARMONSTER;
            time = 0;
            id = 0;
            x = y = 0;
            param = 0;
            mapid = 0;
            tag = 9545;
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null && msg.Length == mMsgLen  - 2)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                time = inpack.ReadInt32();
                id = inpack.ReadUInt32();
                x = inpack.ReadUInt16();
                y = inpack.ReadUInt16();
                param = inpack.ReadInt32();
                mapid = inpack.ReadUInt32();
                tag = inpack.ReadUInt32();
                
            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(time);
            outpack.WriteUInt32(id);
            outpack.WriteUInt16(x);
            outpack.WriteUInt16(y);
            outpack.WriteInt32(param);
            outpack.WriteUInt32(mapid);
            outpack.WriteUInt32(tag);
            return outpack.Flush();
        }
    }

   
    //怪物攻击信息
    public class MsgMonsterAttackInfo : BaseMsg
    {
        public int time;                //时间
        public uint monsterid;             //怪物id
        public uint roleid;          //被攻击角色id
        public short role_x;           //角色x
        public short role_y;           //角色y
        public uint tag;                 //标记
        public uint injuredvalue;       //攻击伤害值
        public int[] param;           //未知参数
        public MsgMonsterAttackInfo()
        {
            mMsgLen = 40;
            tag = 2;
            mParam = PacketProtoco.S_ATTACK;
            param = new int[3];
            for (int i = 0; i < param.Length; i++)
            {
                param[i] = 0;
            }
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(time);
            outpack.WriteUInt32(monsterid);
            outpack.WriteUInt32(roleid);
            outpack.WriteInt16(role_x);
            outpack.WriteInt16(role_y);
            outpack.WriteUInt32(tag);
            outpack.WriteUInt32(injuredvalue);
            for (int i = 0; i < param.Length; i++)
            {
                outpack.WriteInt32(param[i]);
            }
            return outpack.Flush();
        }
    }

    //刷新玩家信息
    public class MsgRoleInfo : BaseMsg
    {
        
        public uint role_id; //玩家id
        public uint face_sex; //外型性别
        public uint face_sex1; //外形性别
        public int[] param = new int[8];
        public uint legion_id;//军团id
        public uint armor_id;   //护甲id
        public uint wepon_id;      //武器id
        public int param1 = 0;
        public uint rid_id;      //骑乘id
        public short x;
        public short y;
        public uint hair_id;  //发型
        public byte dir;        //方向
        public byte TodayGuideCountByOther; //什么鬼
        public ushort param2;
        public uint action;   //动作
        public byte level;      //玩家等级
        public byte job;        //玩家职业
        public short param6;
        public byte param7;
        public short param8;
        public byte guanjue;    //爵位
        public byte[] param9 = new byte[9];
        public byte legion_title; //军团称谓
        public byte[] param10 = new byte[12];
        public short legion_place; //军团职位
        public byte[] param11 = new byte[35];
        public uint legion_id1; //军团id?
        //2016.1.21 扩展了8个字节- 
        //最新版本扩展了8个字节- 测试用的版本用老版本
        public byte[] param5 = new byte[29];    //未知参数
        //public byte[] param5 = new byte[21];    //未知参数
        public List<String> str;
        public byte[] param3 = new byte[3];

        public MsgRoleInfo()
        {
            mMsgLen = 188;
        
            TodayGuideCountByOther = 5;
           
            armor_id = wepon_id = 0;
            rid_id = 0;
            dir = DIR.RIGHT;
    
            mParam = PacketProtoco.S_ROLEINFO;
           
            for (int i = 0; i < param.Length; i++) { param[i] = 0; }

            param3 = new byte[3];
            for (int i = 0; i < param3.Length; i++) { param3[i] = 0; }

          
            for (int i = 0; i < param5.Length; i++) { param5[i] = 0; }

            str = new List<String>();
           
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            mMsgLen++; //字符串长度
            for (int i = 0; i < str.Count; i++)
            {
                mMsgLen += (ushort)(1 + Coding.GetDefauleCoding().GetBytes(str[i]).Length);
            }
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(role_id);
            outpack.WriteUInt32(face_sex);
            outpack.WriteUInt32(face_sex1);
            for (int i = 0; i < param.Length; i++)
            {
                outpack.WriteInt32(param[i]);
            }
            outpack.WriteUInt32(legion_id);
            outpack.WriteUInt32(armor_id);
            outpack.WriteUInt32(wepon_id);
            outpack.WriteInt32(param1);
            outpack.WriteUInt32(rid_id);
            //如果是坐骑有一个标识。。目前暂时不太清楚这个参数有什么用
            if (rid_id > 0)
            {
                //2015.10.15  20下标是角色缩放。。。
                param11[22] = 75; //这个是幻兽星级，根据幻兽星级显示坐骑的外观
            }
        
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteUInt32(hair_id);
            outpack.WriteByte(dir);
            outpack.WriteByte(TodayGuideCountByOther);
            outpack.WriteUInt16(param2);
            outpack.WriteUInt32(action);
            outpack.WriteByte(level);
            outpack.WriteByte(job);

            outpack.WriteInt16(param6);
            outpack.WriteByte(param7);
            outpack.WriteInt16(param8);
            outpack.WriteByte(guanjue);
 
            outpack.WriteBuff(param9);
            outpack.WriteByte(legion_title);
            outpack.WriteBuff(param10);
            outpack.WriteInt16(legion_place);
            //public byte[] param11 = new byte[35];
            //public uint legion_id1; //军团id?
            outpack.WriteBuff(param11);
            outpack.WriteUInt32(legion_id1);
            for (int i = 0; i < param5.Length; i++)
            {
                outpack.WriteByte(param5[i]);
            }
            outpack.WriteByte((byte)str.Count);
            for (int i = 0; i < str.Count; i++)
            {
                outpack.WriteString(str[i]);
              //  outpack.WriteByte(0);
            }
            for (int i = 0; i < param3.Length; i++)
            {
                outpack.WriteByte(param3[i]);
            }
           // Log.Instance().WriteLog(GamePacketKeyEx.byteToText(outpack.GetNormalBuff()));
            return outpack.Flush();
        }
    }

    //更新sp值
    public class MsgUpdateSP : BaseMsg
    {
        public uint role_id;
        public uint amount;   //数量
        public uint value;  //类型
        public uint sp; //值
        public MsgUpdateSP()
        {
            mMsgLen = 20;
            mParam = PacketProtoco.S_UPDATESP;
            amount = 1;
            value = 9;
            sp = 100;
        }

        
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                role_id = inpack.ReadUInt32();
                amount = inpack.ReadUInt32();
                value = inpack.ReadUInt32();
                sp = inpack.ReadUInt32();
            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(role_id);
            outpack.WriteUInt32(amount);
            outpack.WriteUInt32(value);
            outpack.WriteUInt32(sp);
            return outpack.Flush();
        }
    }

    //摊位道具信息
    public class MsgPtichItemInfo : BaseMsg
    {
        public static byte _tag = 5;
        private uint id;
        private uint ptich_obj_id;
        private int price;           //魔石价格
        private uint base_item_id;   //道具基础id
        private short max_dura;      //最大损耗
        private short cur_dura;  //当前损耗
        private byte tag ;  //1.金币 3.魔石
        private byte status = 0;    //是否鉴定 0.已鉴定 1.未鉴定
        private byte postion ;     //未知
        private byte gem1;      //第一个宝石
        private byte gem2;      //第二个宝石
        private short param1 = 0;
        private byte strong_lv; //强化等级
        private int  param2 = 0;
        private int param3 = 0;
        private short soul_lv; //战魂等级
        private byte[] param4 = new byte[10];
        private byte di_attack; //地攻击
        private byte shui_attack; //水攻击
        private byte huo_attack;    //火攻击
        private byte feng_attack;   //风攻击
        private byte effect = 0; //特效
        private byte gem3;      //第三个宝石
        private byte[] param5 = new byte[20];
        private String forgetname;
        public MsgPtichItemInfo(RoleItemInfo item,uint _ptich_obj_id,int _price,byte sell_byte,bool isRemote = false/*是否是远程摊位*/)
        {
            mMsgLen = 88;
            mParam = PacketProtoco.S_PTICH_ITEMINFO;

            //幻兽起始id
            if (item.typeid >= 2000000000)
            {
                postion = NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK;
                id = item.typeid;
                max_dura = cur_dura = 0;
            }
            else
            {
                id = item.id;
                postion = NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK;
                max_dura = cur_dura = (short)item.amount;//游戏当前损耗度无用
            }
            forgetname = item.forgename;
           
            ptich_obj_id = _ptich_obj_id;
            price = _price;
            base_item_id = item.itemid;

          //  { 98, 0, 84, 4, 110, 134, 61, 138, 67, 162, 1, 0, 14, 0, 0, 0, 118, 91, 16, 0, 0, 0, 0, 0, 3, 0, 53, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 198, 230, 210, 236, 202, 222, 79, 208, 205, 0, 0, 0 };
         
            gem1 = item.GetGemType(0);
            gem2 = item.GetGemType(1);
            gem3 = item.GetGemType(2);
            strong_lv = item.GetStrongLevel();
            soul_lv = (short)item.war_ghost_exp;
            di_attack = item.di_attack;
            shui_attack = item.shui_attack;
            huo_attack = item.huo_attack;
            feng_attack = item.feng_attack;

            if (isRemote)
            {
                if (sell_byte == NetMsg.MsgOperateItem.PTICH_SELL_ITEM_GAMEGOLD)
                {
                    tag = 5;
                }
                else
                {
                    tag = 4;
                }
                
            }
            else
            {
                if (sell_byte == NetMsg.MsgOperateItem.PTICH_SELL_ITEM_GAMEGOLD)
                {
                    tag = 3;
                }
                else tag = 1;
            }

        }

        public override byte[] GetBuffer()
        {
            if (forgetname.Length > 0)
            {
                mMsgLen = (ushort)(mMsgLen + Coding.GetDefauleCoding().GetBytes(forgetname).Length + 1);
            }
            PacketOut outpack = new PacketOut();
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(id);
            outpack.WriteUInt32(ptich_obj_id);
            outpack.WriteInt32(price);
            outpack.WriteUInt32(base_item_id);
            outpack.WriteInt16(max_dura);
            outpack.WriteInt16(cur_dura);
            outpack.WriteByte(tag);
            outpack.WriteByte(status);
            outpack.WriteByte(postion);
            outpack.WriteByte(gem1);
            outpack.WriteByte(gem2);
            outpack.WriteInt16(param1);
            outpack.WriteByte(strong_lv);
            outpack.WriteInt32(param2);
            outpack.WriteInt32(param3);
            outpack.WriteInt16(soul_lv);
            outpack.WriteBuff(param4);
            outpack.WriteByte(di_attack);
            outpack.WriteByte(shui_attack);
            outpack.WriteByte(huo_attack);
            outpack.WriteByte(feng_attack);
            outpack.WriteByte(effect);
            outpack.WriteByte(gem3);
            outpack.WriteBuff(param5);
            if (forgetname.Length > 0)
            {
                //六个空字节
                outpack.WriteInt32(0);
                outpack.WriteInt16(0);

                outpack.WriteByte(1);
                outpack.WriteString(forgetname);
                outpack.WriteByte(0);
                outpack.WriteByte(0);
                outpack.WriteByte(0);
            }
            else
            {
                outpack.WriteInt32(0);
            }
         
            return outpack.Flush();
        }

        //36/37.38.39 未知
        //40.41. 战魂等级
        //42-51 未知
        //52 地攻击
        //53 水攻击
        //54 火攻击
        //55 风攻击
        //56 特效
        //64 第三个宝石
        // byte[] data = { 101, 0, 84, 4, 13, 11, 150, 7, 67,162,1,0, 172, 13, 0, 0, 132, 70, 2, 0, 172, 38, 172, 38, 3, 0, 50, 30, 30, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 10, 209, 215, 253, 136, 133, 101, 159, 111, 235, 112, 0, 0, 0, 0, 0 };
    }
    //道具信息
    public class MsgItemInfo : BaseMsg
    {

        public const byte ITEMPOSITION_HELMET = 1;	// 头盔
        public const byte ITEMPOSITION_NECKLACE = 2;	// 项链
        public const byte ITEMPOSITION_ARMOR = 3;	// 盔甲
        public const byte ITEMPOSITION_WEAPONR = 4;	// 右武器
        public const byte ITEMPOSITION_WEAPONL = 5;	// 左武器【作废】
        public const byte ITEMPOSITION_RINGR = 6;	// 戒指
        public const byte ITEMPOSITION_TREASURE = 7;	// 手镯
        public const byte ITEMPOSITION_SHOES = 8;	// 鞋子
        public const byte ITEMPOSITION_MOUNT = 9;	// 结婚戒指
        public const byte ITEMPOSITION_SPRITE = 10;	// 精灵
        public const byte ITEMPOSITION_FASHION = 12; // 是时装
        public const byte ITEMPOSTION_RUB_SHUGUANGZHANHUN = 13; //法宝- 曙光战魂
        public const byte ITEMPOSTION_RUB_DILONGZHILEI = 14;        //法宝- 帝龙之泪
        public const byte ITEMPOSTION_RUB_SHENGYAOFUWEN = 15;   //法宝- 圣耀符文
        public const byte ITEMPOSTION_WEPON_SOUL = 26;     //武器幻魂
        public const byte ITEMPOSITION_CHEST = 44;      //衣柜
        public const byte ITEMPOSITION_CHEST_SOUL = 49; //幻魂衣柜
        public const byte ITEMPOSITION_BACKPACK = 50;           //人物背包
        public const byte ITEMPOSITION_EUDEMONEGG_PACK = 52;	// 幻兽蛋背包
        public const byte ITEMPOSITION_EUDEMON_PACK = 53;	    // 幻兽背包
        public const byte ITEMPOSTION_STRONG_PACK = 100;        //角色道具仓库
        public const byte ITEMPOSTION_PTICH_PACK = 111;         //摊位


        //标记
        public const byte TAG_ROLEITEM = 1; //人物包裹
        public const byte TAG_TRADITEM = 2;//交易栏
        public const byte TAG_ROLEEUDEMONPACK = 3; //人物幻兽包裹
        public const byte TAG_LOOKROLEINFO = 4; //查看别人装备
        public const byte TAG_LOOKROLEEUDEMONINFO = 7;  //查看别人幻兽
        public int time;
        public uint id;
        public uint item_id;
        public ushort amount;     //数量
        public ushort amount_limit;    //最大叠加数量
        public byte tag;        //标记 1.为人物包裹 2.交易栏
        public byte status;     //状态 1.未鉴定 0.已鉴定
        public byte postion;        //穿戴位置
        public byte gem;        //第一个宝石
        public byte gem2;       //第二个宝石
        public byte magic;      //技能
        public byte magic2;
        public byte magic3;     //强化等级
        public int param3;       //如果是特制经验值球 这个值为1000000 为满的 
        public int lock_time;    //装备锁住时间
        public int warghost_exp; //战魂经验
        public int param4;
        public int param5;      //2015.9.28 与幻兽有关  115, 81, 157, 0 不知道是个啥
        public byte di_attack; //地攻击
        public byte shui_attack; //水攻击
        public byte huo_attack; //火攻击
        public byte feng_attack; //风攻击
        public byte add_eff;        //特效
        public byte param6;
        public byte param7;
        public int properties; //道具属性 什么封印道具 系统赠送道具 魂契武器的标识
        public short param10;
        public byte gem3;       //镶嵌的第三个宝石属性
        public int god_strong;  //神炼强度 2016.1.24未知  //如果是法宝为星级经验
        public short param12; //未知
        public int god_exp; //神佑等级 10000 = 1级
       
        
        public int param8;
      //  public byte[] param;
        public int param1;      //未知
        public byte[]  param2 = new byte[3];
        public byte pram9;
        public String name; //锻造者名称- 或者幻兽名称
        public MsgItemInfo()
        {
            mMsgLen = 87;
           
            mParam = PacketProtoco.S_ITEMINFO;
            tag = 1;
            param1 = 3;
            time = System.Environment.TickCount;
            param3 = 0;
            param10 = 0;
            lock_time = 0;    
            warghost_exp = 0;
            param4 = 0;
            param5 = 0;
            di_attack = 0; 
            shui_attack = 0; 
            huo_attack = 0;
            feng_attack = 0;
            add_eff = 0;       
            param6 = 0;
            param7 = 0;
            properties = 0;
            gem3 = 0;       
            god_strong = 0; 
            god_exp = 0; 
           
            param8 = 0;
           
            pram9 = 1;
            name = "";
            for (int i = 0; i < param2.Length; i++) param2[i] = 0;
        }
        //设置交易标记
        public void SetTradTag()
        {
            tag = 2;
        }

        //设置查看装备的标记
        public void SetLookEquipTag()
        {
            tag = 4;
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }
        //设置查看幻兽标记-查看别人装备
        public void SetLookEudemonTag()
        {
            tag = 7;
        }
        public override byte[] GetBuffer()
        {


            byte nNameLen = (byte)GameBase.Core.Coding.GetDefauleCoding().GetBytes(name).Length;
            mMsgLen += nNameLen;
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(time);
            outpack.WriteUInt32(id);
            outpack.WriteUInt32(item_id);
            outpack.WriteUInt16(amount);
            outpack.WriteUInt16(amount_limit);
            outpack.WriteByte(tag);
            outpack.WriteByte(status);
            outpack.WriteByte(postion);
            outpack.WriteByte(gem);
            outpack.WriteByte(gem2);
            outpack.WriteByte(magic);
            outpack.WriteByte(magic2);
            outpack.WriteByte(magic3);
            outpack.WriteInt32(param3);
            outpack.WriteInt32(lock_time);
            outpack.WriteInt32(warghost_exp);
            outpack.WriteInt32(param4);
            outpack.WriteInt32(param5);
            outpack.WriteByte(di_attack);
            outpack.WriteByte(shui_attack);
            outpack.WriteByte(huo_attack);
            outpack.WriteByte(feng_attack);
            outpack.WriteByte(add_eff);
            outpack.WriteByte(param6);
            outpack.WriteByte(param7);
            outpack.WriteInt32(properties);
            outpack.WriteInt16(param10);
           
            outpack.WriteByte(gem3);
            outpack.WriteInt32(god_strong);
            outpack.WriteInt16(param12);
            outpack.WriteInt32(god_exp);
            outpack.WriteInt32(param8);
            outpack.WriteInt32(param1);
            outpack.WriteByte(pram9);
            outpack.WriteString(name);
     
            for (int i = 0; i < param2.Length; i++) outpack.WriteByte(param2[i]);
            return outpack.Flush();
        }
    }

    //下发已学技能
    public class MsgMagicInfo : BaseMsg
    {
        public uint id;     //角色id
        public uint exp;    //技能经验
        public ushort magicid;   //技能id
        public ushort level;      //技能等级
        public MsgMagicInfo()
        {
           // mMsgLen = 20;
            mMsgLen = 16;
            mParam = PacketProtoco.S_MAGICINFO;
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(id);
            outpack.WriteUInt32(exp);
            outpack.WriteUInt16(magicid);
            outpack.WriteUInt16(level);
            return outpack.Flush();
        }
    }

    //创建角色注册名查询
    public class MsgQueryCreateRoleName : BaseMsg
    {
        public byte[] Name; //角色名
        public int version; //版本号
        public MsgQueryCreateRoleName()
        {
            Name = null;
            version = 0;
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                Name = inpack.ReadBuff(16);
                version = inpack.ReadInt32();
            }

        }

        public String GetName()
        {
            String sName = "";
            if (Name == null) return "";
            int nPos = 0;
            for (int i = 0; i < Name.Length; i++)
            {
                if (Name[i] == 0)
                {
                    nPos = i;
                    break;
                }
            }
            byte[] msg = new byte[nPos];
            Buffer.BlockCopy(Name, 0, msg, 0, nPos);
            sName = GameBase.Core.Coding.GetDefauleCoding().GetString(msg);
            return sName;
        }

    }
    //创建角色
    public class MsgCreateRoleInfo : BaseMsg
    {
        public String tag;      //注册标识
        public String name;     //角色名称
        public String tag1;     //注册标识
        public String hardwaretag;  //硬件标识
        public int version;     //版本号
        public uint lookface;   //性别+头像
        public ushort profession;   //职业
        public ushort param;        //未知参数
        public int param1;
        public int param2;
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                tag = inpack.ReadString(16);
                name = inpack.ReadString(16);
                tag1 = inpack.ReadString(16);
                hardwaretag = inpack.ReadString(44);
                version = inpack.ReadInt32();
                lookface = inpack.ReadUInt32();
                profession = inpack.ReadUInt16();
                param = inpack.ReadUInt16();
                param1 = inpack.ReadInt32();
                param2 = inpack.ReadInt32();
            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
          //  outpack.WriteUInt16(mMsgLen);
           
            return null;
        }

        public String GetName()
        {
            if (name == null) return "";
            byte[] bName = GameBase.Core.Coding.GetDefauleCoding().GetBytes(name);
            String sName = "";
            
            int nPos = 0;
            for (int i = 0; i < bName.Length; i++)
            {
                if (bName[i] == 0)
                {
                    nPos = i;
                    break;
                }
            }
            byte[] msg = new byte[nPos];
            Buffer.BlockCopy(bName, 0, msg, 0, nPos);
            sName = GameBase.Core.Coding.GetDefauleCoding().GetString(msg);
            return sName;
        }
    }

    //群攻技能伤害--
    public class MsgGroupMagicAttackInfo : BaseMsg
    {
        public uint nID;
        public short nX;
        public short nY;
        public uint nTargetID;
        public ushort nMagicID;
        public ushort nMagicLv;
        public byte bDir;
        List<uint> List_Obj; //群攻id
        List<int> List_Value; //群攻值

        private bool bSigle = false;
        public MsgGroupMagicAttackInfo()
        {
            List_Obj = new List<uint>();
            List_Value = new List<int>();
            mParam = PacketProtoco.S_MAGICATTACK;
            mMsgLen = 32; 
        }
        /*
        加入群攻伤害对象
        nTypeId 伤害对象id
        nInJured 伤害值
        */
        public void AddObject(uint nTypeId, int nInjured)
        {
            List_Obj.Add(nTypeId);
            List_Value.Add(nInjured);
        }
        //设置单体攻击
        public void SetSigleAttack(uint id)
        {
            nTargetID = id;
            bSigle = true;
        }
        public override byte[] GetBuffer()
        {
            byte[] bBuff = new byte[18];
            PacketOut outpack = new PacketOut(mKey);
            mMsgLen += (ushort)(List_Obj.Count * (28 + 1)+bBuff.Length);
            if (bSigle)
            {
                mMsgLen += 13;
            }
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(nID);
            //单体魔法攻击是对方的id,这样才会有轨迹特效
            if (bSigle)
            {
                outpack.WriteUInt32(nTargetID);
            }
            else
            {
                outpack.WriteInt16(nX);
                outpack.WriteInt16(nY);
            }
            outpack.WriteUInt16(nMagicID);
            outpack.WriteUInt16(nMagicLv);
            outpack.WriteByte(bDir);
            //int nCount = List_Obj.Count == 0 ? 1 : List_Obj.Count;
            outpack.WriteByte((byte)List_Obj.Count);
            outpack.WriteBuff(bBuff);
            if (bSigle)
            {
                bBuff = new byte[43];
            }
            else
            {
                bBuff = new byte[20];
            }
           
            for (int i = 0; i < List_Obj.Count; i++)
            {
                outpack.WriteUInt32(List_Obj[i]);
                outpack.WriteInt32(List_Value[i]);
                outpack.WriteBuff(bBuff);
            }
            outpack.WriteInt32(0);
            outpack.WriteInt32(0);

            outpack.WriteBuff(bBuff);
          //  Log.Instance().WriteLog(GameBase.Network.GamePacketKeyEx.byteToText(outpack.GetNormalBuff()));
            return outpack.Flush();
        } 
    }
    //群攻技能的怪物伤害值显示
  
    public class MsgMagicAttackInjured : BaseMsg
    {
        public uint id;
        public short x;
        public short y;
        public ushort magicid;
        public ushort magiclv;
        public byte dir;
        public byte param;
        public short param1;
        public int[] param2 = new int[3];
        public uint  targetid;
        public uint injured;
        public int[] param3 = new int[10];
        public MsgMagicAttackInjured()
        {
            mMsgLen = 80;
            mParam = PacketProtoco.S_MAGICATTACK;
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(id);
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteUInt16(magicid);
            outpack.WriteUInt16(magiclv);
            outpack.WriteByte(dir);
            outpack.WriteByte(param);
            outpack.WriteInt16(param1);
            for (int i = 0; i < param2.Length; i++)
            { outpack.WriteInt32(param2[i]); }
            outpack.WriteUInt32(targetid);
            outpack.WriteUInt32(injured);
            for (int i = 0; i < param3.Length; i++)
            { outpack.WriteInt32(param3[i]); }
             return outpack.Flush();
        }
             
    }
    //群攻技能的消息
    public class MsgMagicAttackInfoEx : BaseMsg
    {
        public uint roleid; //角色id
        public short x;     //角色x坐标
        public short y;     //角色y坐标
        public ushort magicid; // 技能id
        public ushort magiclv;  //技能等级
        public byte dir;        //方向
        public byte param;
        public int[] param1 = new int[9];

        public MsgMagicAttackInfoEx()
        {
            mMsgLen = 54;
            mParam = PacketProtoco.S_MAGICATTACK;
            roleid = 0;
            x = y = 0;
            magicid = magiclv = 0;
            dir = param = 0;
            for (int i = 0; i < param1.Length; i++) param1[i] = 0;
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(roleid);
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteUInt16(magicid);
            outpack.WriteUInt16(magiclv);
            outpack.WriteByte(dir);
            outpack.WriteByte(param);
            for (int i = 0; i < param1.Length; i++)
            {
                outpack.WriteInt32(param1[i]);
            }
                return outpack.Flush();
        }
    }   
    //魔法攻击单体
    public class MsgMagicAttackInfo : BaseMsg
    {
        public uint id; //角色id
        public uint targetid;
        //public short x;    //x
        //public short y;     //y
        public ushort magicid; //技能id
        public ushort level;    //等级
        public byte dir;        //方向
        public byte type;//1
        public short param;
        public int[] param1;
        //这里也有一个怪物id
        public uint value;  //被伤害值
        public int[] param2;
        public MsgMagicAttackInfo()
        {
            mMsgLen = 84;
            mParam = PacketProtoco.S_MAGICATTACK;
            type = 1;
            param = 0;
            param1 = new int[4];
            for (int i = 0; i < param1.Length; i++) { param1[i] = 0; }
            param2 = new int[10];
            for (int i = 0; i < param2.Length; i++) { param2[i] = 0; }
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
          
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(id);
            outpack.WriteUInt32(targetid);
            //outpack.WriteInt16(x);
            //outpack.WriteInt16(y);
            outpack.WriteUInt16(magicid);
            outpack.WriteUInt16(level);
            outpack.WriteByte(dir);
            outpack.WriteByte(type);
            outpack.WriteInt16(param);
            for (int i = 0; i < param1.Length; i++) { outpack.WriteInt32(param1[i]); }
            outpack.WriteUInt32(targetid);
            outpack.WriteUInt32(value);
            for (int i = 0; i < param2.Length; i++) { outpack.WriteInt32(param2[i]); }
            return outpack.Flush();
        }
    }

    public class MsgOperateItem : BaseMsg
    {
        public const ushort ITEMACT_BUY = 1;        //购买道具
        public const ushort ITEMACT_SELL = 2;       //卖出道具
        public const ushort ITEMACT_USE = 4;      //使用道具
        public const ushort ITEMACT_EQUIP = 5;    //穿戴装备
        public const ushort ITEMACT_UNEQUIP = 6;  //卸下装备
        public const ushort ITEMACT_DROP = 3;     //丢弃道具[从道具栏]
        public const ushort ITEMACT_REPAIREQUIP = 14;   //修理装备
        public const ushort STRONGACT_SAVEMONEY = 10;   //仓库存钱
        public const ushort STRONGACT_GIVEMONEY = 11;   //仓库取钱
        public const ushort ITEMACT_DROPEQUIPMENT = 18;   //丢弃装备[从身上]
        public const ushort PTICH_SELL_ITEM_GOLD = 22;      //摊位以金币方式出售道具- 
        public const ushort PTICH_GETBACK_SELLITEM = 23;        //摊位取回道具
        public const ushort PTICH_BUY_ITEM = 24;            //购买摊位道具
        public const ushort EUDEMON_EVOLUTION = 28;     //幻兽进化
        public const ushort ITEMACT_OPENGEM = 59;       //打洞
        public const ushort EUDEMONACT_RECALL = 32; //召回幻兽
        public const ushort EUDEMONACT_FIT = 35;         //幻兽合体
        public const ushort EUDEMONACT_BREAK_UP = 36;       //幻兽解体
        public const ushort EUDEMON_DELETE_MAGIC = 41;      //删除幻兽技能
        public const ushort GET_EXPBALL_EXP = 50;           //获取使用经验券得到的经验值

        public const ushort PTICH_SELL_ITEM_GAMEGOLD = 52;      //摊位以魔石方式出售道具- 
        public const ushort USE_EXPBALL_EXP = 63;           //使用经验球
        public const ushort EUDEMON_FOOD = 101;             //幻兽喂食圣兽魔晶
        public const ushort TAKEMOUNT = 110;                //骑乘幻兽
        public const ushort TAKEOFFMOUNT = 111;             //下马
        public const ushort GET_REMOTE_PTICH_ID = 114;      //获取远程摊位 从指定摊位号
        public const ushort GET_REMOTE_PTICH = 115;     //获取远程摊位 自动顺序
        public const ushort BUY_REMOTE_PTICH_ITEM = 116;    //购买远程摊位道具
        public uint id;
  
        public uint dwData;
        public ushort usAction;
        public ushort param;
        public ushort amount;//数量
        public ushort param1;
        public uint param2;
        public int param3;
        public MsgOperateItem()
        {
            mParam = PacketProtoco.C_MSGIEM;
            id = 0;
            usAction = 0;
            dwData = 0;
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                id = inpack.ReadUInt32();
                dwData = inpack.ReadUInt32();
                usAction = inpack.ReadUInt16();
                param = inpack.ReadUInt16();
                amount = inpack.ReadUInt16();
                param1 = inpack.ReadUInt16();
                param2 = inpack.ReadUInt32();
                param3 = inpack.ReadInt32();
            }
        }
    }

    public class MsgTalkInfo : BaseMsg
    {
        //频道-----------------------------------------------------------
        public const ushort _TXTATR_PRIVATE = 2001; //私聊
        public const ushort _TXTATR_ACTION = 2002;  //动作
        public const ushort _TXTATR_TEAM = 2003;     //队伍
        public const ushort _TXTATR_SYNDICATE = 2004;   //帮派
        public const ushort _TXTATR_SYSTEM = 2005;      //系统
        public const ushort _TXTATR_FAMILY = 2006;      //家族
        public const ushort _TXTATR_TALK = 2007;        //公聊
        public const ushort _TXTATR_YELP = 2008;        //叫喊
        public const ushort _TXTATR_FRIEND = 2009;  //朋友
        public const ushort _TXTATR_GLOBAL = 2010;      //飞鸽传书
        public const ushort _TXTATR_GM = 2011;      //GM频道
        public const ushort _TXTATR_WHISPER = 2022; //耳语
        public const ushort _TXTATR_GHOST = 2023;   //幽灵
        public const ushort _TXTATR_SERVE = 2024;       //服务
        public const ushort _TXTATR_REJECT = 2113;      //驳回
        //-----------------------------------------------------------
        public int param;
        public ushort unTxtAttribute; //聊天频道
        public ushort tag; //标记
        public int param1;
        public int param2;
        public int param3;
        public byte strcount; //文本数量
        public List<String> liststr;
        public MsgTalkInfo()
        {
            mMsgLen = 24;
            mParam = PacketProtoco.S_CHATNOTICE;
            param = 0;
            unTxtAttribute = 0;
            tag = 0;
            param = param1 = param2 = param3 = 0;
            strcount = 0;
            liststr = new List<String>();
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                param = inpack.ReadInt32();
                unTxtAttribute = inpack.ReadUInt16();
                tag = inpack.ReadUInt16();
                param = inpack.ReadInt32();
                param1 = inpack.ReadInt32();
                param2 = inpack.ReadInt32();
              //  param3 = inpack.ReadInt32();
                byte count = inpack.ReadByte();
                for (int i = 0; i < count; i++)
                {
                    String str = inpack.ReadString();
                    liststr.Add(str);
                }

                inpack.ReadByte();
                inpack.ReadByte();
                inpack.ReadByte();
            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            for (int i = 0; i < strcount; i++)
            {
                mMsgLen += (ushort)(1 + Coding.GetDefauleCoding().GetBytes(liststr[i]).Length);
            }
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt16(unTxtAttribute);
            outpack.WriteUInt16(tag);
            outpack.WriteInt32(param);
            outpack.WriteInt32(param1);
            outpack.WriteInt32(param2);
            outpack.WriteByte(strcount);
            for (int i = 0; i < strcount; i++)
            {
                outpack.WriteString(liststr[i]);
            }
            outpack.WriteByte(0);
            outpack.WriteByte(0);
            outpack.WriteByte(0);
            return outpack.Flush();
        }
        //获取说话角色
        public String GetTalkRoleText()
        {
            if (liststr.Count != 4) return "";
            return liststr[0];
        }
        //获取说话对象
        public String GetTalkTargetText()
        {
            if (liststr.Count != 4) return "";
            return liststr[1];
        }

        //获取说话表情
        public String GetEmtionText()
        {
            if (liststr.Count != 4) return "";
            return liststr[2];
        }
        //获取说明内容
        public String GetTalkText()
        {
            if (liststr.Count != 4) return "";
            return liststr[3];
        }

    }

    public class MsgChangeMapInfo : BaseMsg
    {
        public uint roleid;
        public uint mapid;
        public short x;
        public short y;
        public byte dir;
        public MsgChangeMapInfo(uint _roleid, uint _mapid, short _x, short _y,byte _dir)
        {
            roleid = _roleid;
            mapid = _mapid;
            x = _x;
            y = _y;
            dir = _dir;
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }
        //       长度,包识,地图ID,角色ID,短整数 x,短整数 y,2,地图ID,9535
        //   长度,包识,TIME,角色ID,短整数 x,短整数 y,0,-1,9567
       public byte[] GetMap1Info()
        {
            PacketOut outpack = new PacketOut(mKey);
            mMsgLen = 28;
            mParam = 1010;
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(mapid);
            outpack.WriteUInt32(roleid);
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteInt32(dir);
            outpack.WriteUInt32(mapid);
            outpack.WriteInt32(9535);
            return outpack.Flush() ; 
        }

        public byte[] GetMap2Info()
       {
           PacketOut outpack = new PacketOut(mKey);
           mMsgLen = 24;
           mParam = 1010;
           outpack.WriteUInt16(mMsgLen);
           outpack.WriteUInt16(mParam);
           outpack.WriteUInt32(0);
           outpack.WriteUInt32(roleid);
           outpack.WriteInt16(x);
           outpack.WriteInt16(y);
           outpack.WriteInt32(dir);
           outpack.WriteInt32(-1);
           outpack.WriteInt32(9567);
           return null; 
       }
    }

    //装备操作信息
    public class MsgOperateEquip : BaseMsg
    { 
        public uint equipid;
        public int postion;
        public int tag;
        public int param;
        public int param1;
        public int param2;
  
        public MsgOperateEquip()
        {
            mMsgLen = 28;
            mParam = PacketProtoco.S_OPERATEEQUIP;
            equipid = 0;
            postion = 0;
            tag = param = param1 = param2  = 0;
        }
        //设置穿戴装备标记
        public void SetTagEquip()
        {
            tag = 5;
        }
        //设置卸载装备标记
        public void SetTagUnEquip()
        {
            tag = 6;
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(equipid);
            outpack.WriteInt32(postion);
            outpack.WriteInt32(tag);
            outpack.WriteInt32(param);
            outpack.WriteInt32(param1);
            outpack.WriteInt32(param2);
            return outpack.Flush();
        }
    }

    //掉落物品
    public class MsgDropItem : BaseMsg
    {
        public uint id;
        public uint typeid;
        public short x;
        public short y;
        public int param;
        public uint tag;
        public int param1;
        public MsgDropItem()
        {
            mMsgLen = 28;
            mParam = PacketProtoco.S_DROPITEM;
        }
        //刷新物品标记
        public void SetRefreshTag()
        {
            tag = 1;
        }
        //拾取物品标记
        public void SetPickTag()
        {
            tag = 2;
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                id = inpack.ReadUInt32();
                id = id ^ 9527;
                typeid = inpack.ReadUInt32();
                x = inpack.ReadInt16();
                y = inpack.ReadInt16();
                param = inpack.ReadInt32();
                tag = inpack.ReadUInt32();
                param1 = inpack.ReadInt32();
            }
        }
        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(id);
            outpack.WriteUInt32(typeid);
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteInt32(param);
            outpack.WriteUInt32(tag);
            outpack.WriteInt32(param1);
            return outpack.Flush();
        }
    }

    public class MsgClearItem : BaseMsg
    {
        public uint id;
        public uint param1;
        public uint tag = 3;
        public uint roleid;
        public uint param2;
        public uint param3;
        public MsgClearItem()
        {
            mMsgLen = 28;
            mParam = PacketProtoco.S_CLEARITEM;
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(id);
            outpack.WriteUInt32(param1);
            outpack.WriteUInt32(tag);
            outpack.WriteUInt32(roleid);
            outpack.WriteUInt32(param2);
            outpack.WriteUInt32(param3);
            return outpack.Flush();
        }
    }
    public class MsgAction : BaseMsg
    {
        public const int TYPE_ALIVE = 9552;     //复活
        public const int TYPE_CHANGEPKMODE = 9556;//更改PK模式

        public const int TYPE_XPFULL = 9622;            //xp技能
        public const int TYPE_FACEACTION = 9530;        //表情动作
        public const int TYPE_CONTINUEGAME = 9630;      //继续游戏
        public const int TYPE_FLY_DOWN = 9632;          //雷霆万钧下降
        public const int TYPE_FRIENDINFO = 9560;        //获取好友详细信息
        public const int TYPE_PTICH = 9570;             //摆摊
        public const int TYPE_SHUT_PTICH = 9573;        //收摊
        public const int TYPE_LOOKROLEINFO = 9576;      //查看装备
        public const int TYPE_LOOK_PTICH = 9707;        //查看摊位
        public const int TYPE_EUDEMON_SOUL = 9742;      //幻兽幻化
        public const int TYPE_LOOKEUDEMONINFO = 9743;       //查看别人的幻兽
        public const int TYPE_EUDEMON_SOULINFO = 9756;  //获取主幻兽幻化信息
        public const int TYPE_EUDEMON_RANK = 9764;      //查看幻兽排行榜排名信息
        public const int TYPE_EUDEMON_BATTLE = 9788;    //幻兽出征
        public const int TYPE_FINDPATH = 9894;          //自动寻路-
    }
    //更改pk模式
    public class MsgChangePkMode : BaseMsg
    {
        

 
        public int time;
        public uint roleid;
        public int type;
        public int param;
        public int value;
        public int tag;
        public MsgChangePkMode()
        {
            mParam = PacketProtoco.C_CHANGEPKMODE;
            mMsgLen = 28;
            time = System.Environment.TickCount;
            roleid = 0;
            type = value = 0;
            param = 0;
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
 	         base.Create(msg, key);
             if (msg != null)
             {
                 PackIn inpack = new PackIn(msg);
                 inpack.ReadUInt16();
                 time = inpack.ReadInt32();
                 roleid = inpack.ReadUInt32();
                 type = inpack.ReadInt32();
                 param = inpack.ReadInt32();
                 value = inpack.ReadInt32();
                 tag = inpack.ReadInt32();
                 
            }
        }

      
        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(time);
            outpack.WriteUInt32(roleid);
            outpack.WriteInt32(type);
            outpack.WriteInt32(param);
            outpack.WriteInt32(value);
            outpack.WriteInt32(tag);
            return outpack.Flush();
        }

        public void SetKey(GamePacketKeyEx key) { mKey = key; }
    }
    //随机卷
    public class MsgScroolRandom : BaseMsg
    {
        public int time;
        public uint roleid;
        public short x;
        public short y;
        public int type;
        public short _x;
        public short _y;
        public int tag;
        public MsgScroolRandom()
        {
            mParam = PacketProtoco.S_SCROOLRANDOM;
            mMsgLen = 28;
            type = 2;
            tag = 9623;
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(time);
            outpack.WriteUInt32(roleid);
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteInt32(type);
            outpack.WriteInt16(_x);
            outpack.WriteInt16(_y);
            outpack.WriteInt32(tag);
           
            return outpack.Flush();
        }
    }
    //回城卷--
    public class MsgReCall1 : BaseMsg
    {
        public int mapid;
        public uint roleid;
        public short x;
        public short y;
        public int type;
    
        public int tag;
        public MsgReCall1()
        {
            mParam = PacketProtoco.S_SCROOLRANDOM;
            mMsgLen = 28;
            type = 2;
            tag = 9535;
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(mapid);
            outpack.WriteUInt32(roleid);
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteInt32(type);
            outpack.WriteInt32(mapid);
            outpack.WriteInt32(tag);
            return outpack.Flush();
        }
    }

    public class MsgReCall2 : BaseMsg
    {
        public int time;
        public uint roleid;
        public short x;
        public short y;
        public int type;
        public int param;
        public int tag;
        public MsgReCall2()
        {
            mParam = PacketProtoco.S_SCROOLRANDOM;
            param = -1;
            type = 0;
            tag = 9567;
            time = System.Environment.TickCount;
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(time);
            outpack.WriteUInt32(roleid);
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteInt32(type);
            outpack.WriteInt32(param);
            outpack.WriteInt32(tag);
            return outpack.Flush();
        }
    }

    //锁定目标
    public class MsgLock : BaseMsg
    {
        public int time;
        public uint id;
        public short x;
        public short y;
        public int param = 0;
        public int param1 = 1;
        public int tag;
        public MsgLock()
        {
            time = System.Environment.TickCount;
            mMsgLen = 28;
            mParam = PacketProtoco.S_LOCK;
        }

        public void Lock()
        {
            tag = 9618;
        }
        public void UnLock()
        {
            tag = 9619;
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(time);
            outpack.WriteUInt32(id);
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteInt32(param);
            outpack.WriteInt32(param1);
            outpack.WriteInt32(tag);
            return outpack.Flush();
        }
    }

    
    //连击
    public class MsgCombo : BaseMsg
    {

        public uint count; //连击数量
        public short type = 642;
        public PacketOut combo;

        private byte head = 0;
        private byte tail = 0;
        public MsgCombo()
        {
            mMsgLen = 11;
            mParam = PacketProtoco.S_COMBO;
            combo = new PacketOut();
            count = 0;

        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            combo.WriteByte(0);
            byte[] combodata = combo.GetBuffer();
           // Log.Instance().WriteLog(Coding.GetDefauleCoding().GetString(combodata));
           
            mMsgLen += (ushort)combodata.Length;
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(count);
            outpack.WriteInt16(type);
            outpack.WriteByte((byte)count);
           
            outpack.WriteBuff(combodata);
            byte[] ret = outpack.Flush();
           // Log.Instance().WriteLog(GamePacketKeyEx.byteToText(ret));
            return ret;
        }

        //根据双飞类型与坐标计算出标记
        public void CalcTag(uint magicid, MapServer.BaseObject attack, MapServer.BaseObject target)
        {
            if ((attack.GetCurrentX() < 999 && attack.GetCurrentY() < 999) ||
              (target.GetCurrentX() < 999 && target.GetCurrentY() < 999))
            {
                if (target.type == OBJECTTYPE.MONSTER)
                {
                    switch (magicid)
                    {
                        case GameStruct.MagicTypeInfo.FEITIANZHAN:
                        case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                        //  case GameStruct.MagicTypeInfo.XUEYINGLUNHUI:
                        case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                        case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                        case GameStruct.MagicTypeInfo.XUEXI:
                        case GameStruct.MagicTypeInfo.SHUNYINGJI:
                            {
                                head = 25;
                                tail = 26;
                                break;
                            }
                        case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                        case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                        case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                        case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                        case GameStruct.MagicTypeInfo.SILIANZHAN:
                        case GameStruct.MagicTypeInfo.LIULIANZHAN:
                            {
                                head = 26;
                                tail = 27;
                                break;
                            }
                        case GameStruct.MagicTypeInfo.XUEYINGLUNHUI:
                            {
                                head = 25;
                                tail = 28;
                                break;
                            }
                        case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                            {
                                head = 27;
                                tail = 28;
                                break;
                            }
                    }
                }
                //针对玩家对玩家 不同的符号分隔符
                else if (target.type == OBJECTTYPE.PLAYER)
                {


                    {
                        switch (magicid)
                        {
                            case GameStruct.MagicTypeInfo.FEITIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                            case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                            case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                            case GameStruct.MagicTypeInfo.XUEXI:
                            case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                {
                                    head = 26;
                                    tail = 26;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                            case GameStruct.MagicTypeInfo.SILIANZHAN:
                            case GameStruct.MagicTypeInfo.LIULIANZHAN:
                                {
                                    head = 27;
                                    tail = 27;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 28;
                                    tail = 28;
                                    break;
                                }
                        }
                    }
                }
                else if (target.type == OBJECTTYPE.EUDEMON)
                {
                    switch (magicid)
                    {
                        case GameStruct.MagicTypeInfo.FEITIANZHAN:
                        case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                        case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                        case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                        case GameStruct.MagicTypeInfo.XUEXI:
                        case GameStruct.MagicTypeInfo.SHUNYINGJI:
                            {
                                head = 29;
                                tail = 26;
                                break;
                            }
                        case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                        case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                        case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                        case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                        case GameStruct.MagicTypeInfo.SILIANZHAN:
                        case GameStruct.MagicTypeInfo.LIULIANZHAN:
                            {
                                head = 30;
                                tail = 27;
                                break;
                            }
                        case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                            {
                                head = 31;
                                tail = 31;
                                break;
                            }

                    }
                }
            }
            else
                //坐标大于四位数或者被攻击对象是玩家- 分隔符不一样
                //遇到单个坐标为四位数还有问题--需要反汇编调试 2015.9.14
                //
                //x y坐标都大于四位
                if (
                    ((target.GetCurrentX() > 999 && target.GetCurrentY() > 999) ||
                    (attack.GetCurrentX() > 999 && attack.GetCurrentY() > 999)))
                {
                    if (target.type == OBJECTTYPE.MONSTER)
                    {
                        switch (magicid)
                        {
                            case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                            case GameStruct.MagicTypeInfo.SILIANZHAN:
                            case GameStruct.MagicTypeInfo.LIULIANZHAN:
                                {
                                    head = 28;
                                    tail = 29;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.FEITIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                            case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                            case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                            case GameStruct.MagicTypeInfo.XUEXI:
                            case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                //   case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 27;
                                    tail = 28;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 29;
                                    tail = 30;
                                    break;
                                }
                        }
                    }
                    else if (target.type == OBJECTTYPE.PLAYER)
                    {
                        switch (magicid)
                        {
                            case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                            case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                            case GameStruct.MagicTypeInfo.XUEXI:
                            case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                //  case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 27;
                                    tail = 27;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.FEITIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                                {
                                    head = 28;
                                    tail = 28;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                            case GameStruct.MagicTypeInfo.SILIANZHAN:
                            case GameStruct.MagicTypeInfo.LIULIANZHAN:
                            case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 29;
                                    tail = 29;
                                    break;
                                }

                        }
                    }
                    else if (target.type == OBJECTTYPE.EUDEMON)
                    {
                        switch (magicid)
                        {
                            case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                            case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                            case GameStruct.MagicTypeInfo.XUEXI:
                            case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                //  case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 31;
                                    tail = 28;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.FEITIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                                {
                                    head = 31;
                                    tail = 28;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                            case GameStruct.MagicTypeInfo.SILIANZHAN:
                            case GameStruct.MagicTypeInfo.LIULIANZHAN:
                            case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 32;
                                    tail = 29;
                                    break;
                                }

                        }
                    }

                    //一个四位坐标 一个三位坐标
                }
                else if (target.GetCurrentX() > 999 || target.GetCurrentY() > 999 ||
                   attack.GetCurrentX() > 999 || attack.GetCurrentY() > 999)
                {
                    if (target.type == OBJECTTYPE.MONSTER)
                    {
                        switch (magicid)
                        {
                            case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                            case GameStruct.MagicTypeInfo.FEITIANZHAN:
                            case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                            case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                            case GameStruct.MagicTypeInfo.XUEXI:
                            case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                // case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 26;
                                    tail = 27;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                            case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                            case GameStruct.MagicTypeInfo.SILIANZHAN:
                            case GameStruct.MagicTypeInfo.LIULIANZHAN:
                                {
                                    head = 27;
                                    tail = 28;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 28;
                                    tail = 29;
                                    break;
                                }
                        }
                    }
                    else if (target.type == OBJECTTYPE.PLAYER)
                    {
                        switch (magicid)
                        {
                            case GameStruct.MagicTypeInfo.FEITIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                            case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                            case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                            case GameStruct.MagicTypeInfo.XUEXI:
                            case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                //    case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 27;
                                    tail = 27;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                            case GameStruct.MagicTypeInfo.SILIANZHAN:
                            case GameStruct.MagicTypeInfo.LIULIANZHAN:
                                {
                                    head = 28;
                                    tail = 28;

                                    break;
                                }
                            case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 30;
                                    tail = 30;
                                    break;
                                }
                        }
                    }
                    else if (target.type == OBJECTTYPE.EUDEMON)
                    {
                        switch (magicid)
                        {
                            case GameStruct.MagicTypeInfo.FEITIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                            case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                            case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                            case GameStruct.MagicTypeInfo.XUEXI:
                            case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                //    case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 20;
                                    tail = 27;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                            case GameStruct.MagicTypeInfo.SILIANZHAN:
                            case GameStruct.MagicTypeInfo.LIULIANZHAN:
                                {
                                    head = 31;
                                    tail = 28;

                                    break;
                                }
                            case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 30;
                                    tail = 30;
                                    break;
                                }
                        }
                    }


                    //一个三位数 一个二位数
                }
                else if (target.GetCurrentX() > 99 && target.GetCurrentY() < 99 ||
                   attack.GetCurrentX() < 99 && attack.GetCurrentY() > 99)
                {

                    if (target.type == OBJECTTYPE.MONSTER)
                    {
                        switch (magicid)
                        {
                            case GameStruct.MagicTypeInfo.FEITIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                            case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                            case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                            case GameStruct.MagicTypeInfo.XUEXI:
                            case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                //    case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 24;
                                    tail = 25;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                            case GameStruct.MagicTypeInfo.SILIANZHAN:
                            case GameStruct.MagicTypeInfo.LIULIANZHAN:
                                {
                                    head = 25;
                                    tail = 26;

                                    break;
                                }
                            case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 26;
                                    tail = 27;
                                    break;
                                }
                        }
                    }
                    else if (target.type == OBJECTTYPE.PLAYER)
                    {
                        switch (magicid)
                        {
                            case GameStruct.MagicTypeInfo.FEITIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                            case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                            case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                            case GameStruct.MagicTypeInfo.XUEXI:
                            case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                //    case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 25;
                                    tail = 25;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                            case GameStruct.MagicTypeInfo.SILIANZHAN:
                            case GameStruct.MagicTypeInfo.LIULIANZHAN:
                                {
                                    head = 26;
                                    tail = 26;

                                    break;
                                }
                            case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 27;
                                    tail = 27;
                                    break;
                                }
                        }
                    }
                    else if (target.type == OBJECTTYPE.EUDEMON)
                    {
                        switch (magicid)
                        {
                            case GameStruct.MagicTypeInfo.FEITIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                            case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                            case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                            case GameStruct.MagicTypeInfo.XUEXI:
                            case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                //    case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 28;
                                    tail = 25;
                                    break;
                                }
                            case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                            case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                            case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                            case GameStruct.MagicTypeInfo.SILIANZHAN:
                            case GameStruct.MagicTypeInfo.LIULIANZHAN:
                                {
                                    head = 29;
                                    tail = 26;

                                    break;
                                }
                            case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                {
                                    head = 30;
                                    tail = 27;
                                    break;
                                }
                        }
                    }

                }
                else
                    //二个二位数的
                    if (target.GetCurrentX() < 99 && target.GetCurrentY() < 99 ||
                    attack.GetCurrentX() < 99 && attack.GetCurrentY() < 99)
                    {
                        if (target.type == OBJECTTYPE.MONSTER)
                        {
                            switch (magicid)
                            {
                                case GameStruct.MagicTypeInfo.FEITIANZHAN:
                                case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                                case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                                case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                                case GameStruct.MagicTypeInfo.XUEXI:
                                case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                    //    case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                    {
                                        head = 23;
                                        tail = 24;
                                        break;
                                    }
                                case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                                case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                                case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                                case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                                case GameStruct.MagicTypeInfo.SILIANZHAN:
                                case GameStruct.MagicTypeInfo.LIULIANZHAN:
                                    {
                                        head = 24;
                                        tail = 25;

                                        break;
                                    }
                                case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                    {
                                        head = 25;
                                        tail = 26;
                                        break;
                                    }
                            }
                        }
                        else if (target.type == OBJECTTYPE.PLAYER)
                        {
                            switch (magicid)
                            {
                                case GameStruct.MagicTypeInfo.FEITIANZHAN:
                                case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                                case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                                case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                                case GameStruct.MagicTypeInfo.XUEXI:
                                case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                    //    case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                    {
                                        head = 24;
                                        tail = 24;
                                        break;
                                    }
                                case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                                case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                                case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                                case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                                case GameStruct.MagicTypeInfo.SILIANZHAN:
                                case GameStruct.MagicTypeInfo.LIULIANZHAN:
                                    {
                                        head = 25;
                                        tail = 25;

                                        break;
                                    }
                                case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                    {
                                        head = 26;
                                        tail = 26;
                                        break;
                                    }
                            }
                        }
                        else if (target.type == OBJECTTYPE.EUDEMON)
                        {
                            switch (magicid)
                            {
                                case GameStruct.MagicTypeInfo.FEITIANZHAN:
                                case GameStruct.MagicTypeInfo.LONGHUNFENGBAO:
                                case GameStruct.MagicTypeInfo.XUEYINGQIANHUAN:
                                case GameStruct.MagicTypeInfo.XUEYINGXINGMANG:
                                case GameStruct.MagicTypeInfo.XUEXI:
                                case GameStruct.MagicTypeInfo.SHUNYINGJI:
                                    //    case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                    {
                                        head = 27;
                                        tail = 24;
                                        break;
                                    }
                                case GameStruct.MagicTypeInfo.FEITIANLIANZHAN:
                                case GameStruct.MagicTypeInfo.LONGQIANGLIEHUN:
                                case GameStruct.MagicTypeInfo.LONGQIANGZANGHUN:
                                case GameStruct.MagicTypeInfo.LONGQIANGSUIHUN:
                                case GameStruct.MagicTypeInfo.SILIANZHAN:
                                case GameStruct.MagicTypeInfo.LIULIANZHAN:
                                    {
                                        head = 28;
                                        tail = 25;

                                        break;
                                    }
                                case GameStruct.MagicTypeInfo.LIEHUNSHAN:
                                    {
                                        head = 29;
                                        tail = 26;
                                        break;
                                    }
                            }
                        }
                    }
              
        }
        public void AddComboInfo(uint magicid,MapServer.BaseObject attack, MapServer.BaseObject target, uint track_id, uint track_id2)
        {
           // byte head = 0;
          //  byte tail = 0;
            //三位数的 针对怪物的..
          
              
            //测试连击状态
            if (MapServer.Program._Head > 0)
            {
                head = MapServer.Program._Head;
                tail = MapServer.Program._Tail;
            }
        
            byte dir = DIR.GetAgainstDir(target.GetDir());
            String str = "";
            byte[] data = null;
            count += 2;
            combo.WriteByte(head);
            str = Convert.ToString(target.GetTypeId());
            data = Coding.GetUtf8Coding().GetBytes(str);
            combo.WriteBuff(data);
            combo.WriteByte(32);
            str = Convert.ToString(target.GetCurrentX());
            data = Coding.GetUtf8Coding().GetBytes(str);
            combo.WriteBuff(data);
            combo.WriteByte(32);

            str = Convert.ToString(target.GetCurrentY());
            data = Coding.GetUtf8Coding().GetBytes(str);
            combo.WriteBuff(data);
            combo.WriteByte(32);

            str = Convert.ToString(dir);
            data = Coding.GetUtf8Coding().GetBytes(str);
            combo.WriteBuff(data);
            combo.WriteByte(32);

            str = Convert.ToString(track_id2);
            data = Coding.GetUtf8Coding().GetBytes(str);
            combo.WriteBuff(data);
            combo.WriteByte(tail);
            //if (magicid != GameStruct.MagicTypeInfo.XUEYINGLUNHUI)
            //{
            //   
            //}
            //else if (count <= 20)
            //{
            //    combo.WriteByte(tail);
            //}
            //else
            //{
            //    combo.WriteByte(28);
            //}
            
            

            str = Convert.ToString(attack.GetTypeId());
            data = Coding.GetUtf8Coding().GetBytes(str);
            combo.WriteBuff(data);
            combo.WriteByte(32);

            str = Convert.ToString(attack.GetCurrentX());
            data = Coding.GetUtf8Coding().GetBytes(str);
            combo.WriteBuff(data);
            combo.WriteByte(32);

            str = Convert.ToString(attack.GetCurrentY());
            data = Coding.GetUtf8Coding().GetBytes(str);
            combo.WriteBuff(data);
            combo.WriteByte(32);

            str = Convert.ToString(attack.GetDir());
            data = Coding.GetUtf8Coding().GetBytes(str);
            combo.WriteBuff(data);
            combo.WriteByte(32);

            str = Convert.ToString(track_id);
            data = Coding.GetUtf8Coding().GetBytes(str);
            combo.WriteBuff(data);;
           
        }
    }


    //左上角的公告
    public class MsgLeftNotice : BaseMsg
    {
        public int color;
        public short type;
        public short tag;
        public int param;
        public int param1 = -1;
        public int param2 = 0;
        public byte amount = 4;
        public String[] str;
        public MsgLeftNotice()
        {
            mParam = PacketProtoco.S_LEFTNOTICE;
            color = 0xFFFFFF;
            type = 2005;
            str = new String[4];
            for (int i = 0; i < str.Length; i++) str[i] = "";
            str[0] = "SYSTEM";
            mMsgLen = 28;
        }

        public void SetRoleName(String name)
        {
            str[1] = name;
        }
        public void SetText(String text)
        {
            str[3] = text;
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            for (int i = 0; i < str.Length; i++)
            {
                byte[] data = Coding.GetDefauleCoding().GetBytes(str[i]);
                mMsgLen += /*第一个字节为字符串长度*/(ushort)(data.Length + 1);
            }
           
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(color);
            outpack.WriteInt16(type);
            outpack.WriteInt16(tag);
            outpack.WriteInt32(param);
            outpack.WriteInt32(param1);
            outpack.WriteInt32(param2);
            outpack.WriteByte(amount);
            for (int i = 0; i < str.Length; i++)
            {
                outpack.WriteString(str[i]);
            }
            outpack.WriteByte(0);
            outpack.WriteByte(0);
            outpack.WriteByte(0);
            return outpack.Flush();
        }
    }
    public class MsgUserAttribute : BaseMsg
    {
        public uint role_id;
        public int amount;   //数量
        public List<UserAttribute> list_type; //值
        public List<uint> list_value; //类型
        public MsgUserAttribute()
        {
            amount = 0;
            list_type = new List<UserAttribute>();
            list_value = new List<uint>();
            mParam = PacketProtoco.S_USERATTRIBUTE;
            mMsgLen = 12;
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            mMsgLen += (ushort)(amount * 8);
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(role_id);
            outpack.WriteInt32(amount);
            for (int i = 0; i < amount; i++)
            {
                outpack.WriteInt32((int)list_type[i]);
                outpack.WriteUInt32(list_value[i]);
            }
            list_type.Clear();
            list_value.Clear();
           
            return outpack.Flush();
        }

        public void AddAttribute(UserAttribute Attribute, uint value)
        {
            amount++;
            list_type.Add(Attribute);
            list_value.Add(value);
        }
    }

    //热键信息
    public class MsgHotKey : BaseMsg
    {
        public const int TAG_SAVEHOTKEY = 215;  //保存热键信息
        public const int TAG_WANGLING_STATE = 477;//切换亡灵与巫师形态
        public const int WORLD_CHAT = 28;       //魔法飞鸽
        public const int CHANGE_EUDEMON_NAME = 24;  //更改幻兽名字
        public int type; //客发: 1、数字键 1-9  2、 F1-右键    服发=0
        public short tag;// 客发、215  服发、214
        public byte tag2;   //客发、1  服发、4
        //public byte len;
        public String str;
        public byte[] param1 = new byte[3];
        public MsgHotKey()
        {
            mParam = PacketProtoco.S_HOTKEY;
            mMsgLen = 14;
            str = "";
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                type = inpack.ReadInt32();
                tag = inpack.ReadInt16();
                tag2 = inpack.ReadByte();
             //   len = inpack.ReadByte();
                if (inpack.IsComplete()) return;
                str = inpack.ReadString();
                //for (int i = 0; i < param1.Length; i++)
                //{
                //    param1[i] = inpack.ReadByte();
                // }
            }
        }

        public override byte[] GetBuffer()
        {
            byte[] data = Coding.GetUtf8Coding().GetBytes(str);
            mMsgLen += (ushort)(1 + data.Length);
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(type);
            outpack.WriteInt16(tag);
            outpack.WriteByte(tag2);
            outpack.WriteString(str);
            for (int i = 0; i < param1.Length; i++)
            {
                outpack.WriteByte(param1[i]);
            }
            return outpack.Flush();
        }

        public String[] GetHotKeyArr()
        {
            if (str.Length <= 0) return null;
            return str.Split('-');
        }
        //取热键组
        public byte GetGroup()
        {
            return (byte)type;
        }
    }
    //装备操作信息3
    public class MsgEquipOperation : BaseMsg
    {
        public const uint EQUIPSTRONG = 131075; //使用魔魂晶石提升等级 {3,0,2,0}
        public const uint EQUIPSTRONGEX = 131078;   //使用魔魂晶石提升等级 与131075相同
        public const uint EQUIP_GODEXP = 131079;  //提升神佑
        public const uint EQUIPLEVEL = 131076; //使用幻魔晶石提升等级 {4,0,2,0}
        public const uint EQUIPQUALITY = 131074;//使用灵魂晶石提升品质 {2,0,2,0}
        public const uint MAMIC_ADD_GOD = 131081;  //法宝追加
        public const uint GEMSET = 262220;       //宝石镶嵌
        public const uint GEMFUSION = 65826;     //宝石融合
        public const uint GEMREPLACE = 458838;  //宝石替换
        public const uint GUANJUE_GOLD = 65747;//捐献爵位 itemid=为金币数额
        public const uint GUANJUE_GAMEGOLD = 65750;//捐献爵位-itemid=为魔石数额
        public const uint EXIT_GAME = 65753;        //退出游戏 or 重新登录
        public uint type;//操作类型
        public uint itemid;  
        public uint materialid; //材料id
        public uint param;
        public uint param1;
        public MsgEquipOperation()
        {

        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                type = inpack.ReadUInt32();
                itemid = inpack.ReadUInt32();
                materialid = inpack.ReadUInt32();
                if (inpack.IsComplete()) return;
                param = inpack.ReadUInt32();
                if (inpack.IsComplete()) return;
                param1 = inpack.ReadUInt32();
            }
        }
    }
   //打开对话框
    public class MsgOpenDialog : BaseMsg
    {

        public const int OPENDIALOGTYPE_STRONG = 3; //仓库
        public uint playid;
        public uint npcid;
        public short npc_x;
        public short npc_y;
        public int dialog_type;
       
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }


        public  void SetDialogType(int dwData)
        {

        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(28);
            outpack.WriteUInt16(1010);
            outpack.WriteUInt32(playid);
            outpack.WriteUInt32(npcid);
            outpack.WriteInt16(npc_x);
            outpack.WriteInt16(npc_y);
            outpack.WriteInt32(0);
            outpack.WriteInt32(dialog_type);
            outpack.WriteInt32(9596);
            return outpack.Flush();
        }
    }

    //仓库存取- 
    public class MsgStrongPack : BaseMsg
    {
        public const byte STRONGPACK_TYPE = 10;    //仓库
        public const byte CHEST_TYPE = 145;            //衣柜

        public const byte CHEST_TYPE_GIVE = 1;   //取回衣柜时装
        public const byte STRONGPACK_TYPE_SAVE = 1;//存道具
        public const byte STRONGPACK_TYPE_GIVE = 2;//取道具
        public int tick;
        public byte type;
        public byte param;
        public short param3;
        public int param1;
        public int param2;
        public uint itemid;

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                tick = inpack.ReadInt32();
                type = inpack.ReadByte();
                param = inpack.ReadByte();
                param3 = inpack.ReadInt16();
                param1 = inpack.ReadInt32();
                param2 = inpack.ReadInt32();
                itemid = inpack.ReadUInt32();
            }
        }
     
    }

    
    //仓库数据
    public class MsgStrongInfo : BaseMsg
    {
        public int tag;
        public byte param1;
        public byte type;
        public short action;
        public int param2;  
        public uint playid; //玩家id
        
        public List<GameStruct.RoleItemInfo> list_item;
        public MsgStrongInfo()
        {
            tag = 1005;
            type = 10;
            param2 = 100;
            list_item = new List<GameStruct.RoleItemInfo>();
            mMsgLen = 24;
            mParam = PacketProtoco.S_STRONGINFO;
    

        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public static byte[] GetStrongMoneyBuffer(uint playid,int strong_gold)
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteInt16(28);
            outpack.WriteInt16(1009);
            outpack.WriteUInt32(playid);
            outpack.WriteInt32(strong_gold);
            outpack.WriteInt32(9);
            outpack.WriteInt32(0);
            outpack.WriteInt32(0);
            outpack.WriteInt32(0);
            return outpack.Flush();
        }
        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            mMsgLen += (ushort)(152/*道具数据结构信息*/* list_item.Count);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(tag);
            outpack.WriteByte(param1);
            outpack.WriteByte(type);
            outpack.WriteInt16(action);
            outpack.WriteInt32(param2);
            outpack.WriteUInt32(playid);
            outpack.WriteInt32((int)list_item.Count);
            byte[] data = null;
            for (int i = 0; i < list_item.Count; i++)
            {
                GameStruct.RoleItemInfo item = list_item[i];
                GameStruct.ItemTypeInfo baseitem = MapServer.ConfigManager.Instance().GetItemTypeInfo(list_item[i].itemid);
                if (baseitem == null)
                {
                    data = new byte[152];
                    outpack.WriteBuff(data);
                    continue;
                }

               outpack.WriteUInt32(item.id);
         
                outpack.WriteUInt32(item.itemid);
              //当前耐久度
                outpack.WriteUInt16(item.amount);
                //最大耐久度
                outpack.WriteUInt16(baseitem.amount_limit);

                outpack.WriteByte(0);  //状态 1.未鉴定 0.已鉴定
                outpack.WriteByte((byte)item.gem1);
                outpack.WriteByte((byte)item.gem2);
                outpack.WriteByte((byte)0); //技能
                outpack.WriteByte((byte)0); //技能
               
                outpack.WriteByte(item.GetStrongLevel());
                outpack.WriteByte((byte)0); //技能
                outpack.WriteInt32(0);
                outpack.WriteInt32(0);//装备锁住时间
                outpack.WriteInt32(item.war_ghost_exp);
                outpack.WriteInt32(0);
                outpack.WriteInt32(0);
                outpack.WriteByte(item.di_attack);//地攻击
                outpack.WriteByte(item.shui_attack);//水攻击
                outpack.WriteByte(item.huo_attack);//火攻击
                outpack.WriteByte(item.feng_attack);//风攻击
                outpack.WriteByte(0);//特效
                outpack.WriteByte(0);
                outpack.WriteByte(0);
                outpack.WriteInt16(0);
                outpack.WriteInt32(0);//道具属性 什么封印道具 系统赠送道具 魂契武器的标识
                outpack.WriteByte((byte)item.gem3);//第三个宝石
                outpack.WriteInt32(item.god_strong); //神炼强度
                outpack.WriteInt16((short)item.god_exp); //神佑经验
                outpack.WriteInt32(0); //未激活时间 
                data = new byte[21];
                outpack.WriteBuff(data);
  


                //写名称-
                 byte[] namebyte = Coding.GetDefauleCoding().GetBytes(baseitem.name);
                 outpack.WriteBuff(namebyte);
                 data = new byte[68 - namebyte.Length];
                 outpack.WriteBuff(data);
 
            }

           // Log.Instance().WriteLog(GamePacketKeyEx.byteToText(outpack.GetNormalBuff()));
            return outpack.Flush();
        }

 
    }
 
    //幻兽信息
    public class MsgEudemonInfo : BaseMsg
    {
        private List<GameStruct.EudemonAttribute> list_item;
        private List<int> list_value;
        public int tag = 1;
        public uint id;
        public MsgEudemonInfo()
        {
            mParam = PacketProtoco.S_EUDEMONINFO;
            mMsgLen = 16;
            list_item = new List<GameStruct.EudemonAttribute>();
            list_value = new List<int>();
        }

        public void AddAttribute(GameStruct.EudemonAttribute attr, int value)
        {
            list_item.Add(attr);
            list_value.Add(value);
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            mMsgLen += (ushort)(list_value.Count * 8);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(tag);
            outpack.WriteUInt32(id);
            outpack.WriteInt32(list_item.Count);
            for (int i = 0; i < list_item.Count; i++)
            {
                outpack.WriteInt32((int)list_item[i]);
                outpack.WriteInt32(list_value[i]);
            }

            return outpack.Flush();
        }
    }

  
    public class MsgEudemonTag : BaseMsg
    {
        public uint playerid;   //玩家id
        public uint eudemonid;  //幻兽id
        public int param1;
        public int param2;
        public uint param3;
        public int action;
        public MsgEudemonTag()
        {
            mMsgLen = 28;
            mParam = PacketProtoco.S_EUDEMONTAG;
            action = 0;
        }
        
        public override byte[] GetBuffer()
        {
            param3 = eudemonid;
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(playerid);
            outpack.WriteUInt32(eudemonid);
            outpack.WriteInt32(param1);
            outpack.WriteInt32(param2);
            outpack.WriteUInt32(param3);
            outpack.WriteInt32(action);
            return outpack.Flush();
        }
        public void SetReCallTag()
        {
            action = 9545;
        }
        //设置休息标记
        public void SetBreakTag()
        {
            action = 9737;
        }
        //设置出征标记
        public void SetBattleTag()
        {
            action = 9788;
        }
    }

    //幻兽出征信息
 
   //                           /0,0,0,0, 0, 18, 30/*幻兽排行榜排名 罡星兽*/, 0, /*117, 1*/0,0, 0, 0, 
   //                            /*50*/0, 0, 0, 0, /*88, 2, 88, 2*/0,0,0,0, /*1*/0, 0, 0, 0, /*16*/0, 0, 0, 0, /*42, 119, 16*/0,0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
   //                            0, 0, 0, 0, 0, 0, 0, 0, 0, 1/*文本数量*/, 10, 202, 165, 204, 236, 202, 185, 192, 215, 203, 185, 0, 0 };
    public class MsgEudemonBattleInfo : BaseMsg
    {
        public uint id; //幻兽id
        public uint lookface; //幻兽外观
        public byte[] param = new byte[32];
        public uint play_id; //宿主id
        public int life;    //当前血量
        public int life_max; //最大血量
        public short x;     //x坐标
        public short y;     //y坐标
        public short dir;   //方向
        public byte wuxing; //五行属性
        public byte[] param1 = new byte[5];
        public uint monsterid; //怪物id
        public int param3;
        public int param4;
        public int param5;
        public int star;     //幻兽星级
        public byte[] param2 = new byte[37];
        public byte count = 1; //字符串数量为1
        public String name; //幻兽名称
        public MsgEudemonBattleInfo()
        {
            mMsgLen = 128;
            mParam = PacketProtoco.S_EUDEMONBALLTE;
            name = "";
        }

        public override byte[] GetBuffer()
        {
          
            PacketOut outpack = new PacketOut(mKey);
            byte[] data = Coding.GetDefauleCoding().GetBytes(name);
            mMsgLen += (ushort)data.Length;
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(id);
            outpack.WriteUInt32(lookface);
            outpack.WriteBuff(param);
            outpack.WriteUInt32(play_id);
            outpack.WriteInt32(life);
            outpack.WriteInt32(life_max);
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteInt16(dir);
            outpack.WriteByte(wuxing);
          
            outpack.WriteBuff(param1);
            outpack.WriteUInt32(monsterid);
            outpack.WriteInt32(param3);
            outpack.WriteInt32(param4);
    outpack.WriteInt32(param5);
      outpack.WriteInt32(star);
     
            outpack.WriteBuff(param2);
            outpack.WriteByte(count);
            outpack.WriteString(name);
            outpack.WriteInt16(0);
            return outpack.Flush();
        }
    }

    //装备操作返回信息

    public class MsgEquipOperationRet : BaseMsg
    {   
        public uint type;   //装备的操作类型
        public uint srcid; //装备id
        public uint destid; //材料id
        public uint ret; //1为成功 0为失败
        public MsgEquipOperationRet()
        {
            mMsgLen = 20;
            mParam = PacketProtoco.S_EQUIPOPERATION;
            ret = 0;
        }
        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(type);
            outpack.WriteUInt32(srcid);
            outpack.WriteUInt32(destid);
            outpack.WriteUInt32(ret);
            return outpack.Flush();
   
        }
    }

    //好友信息
    public class MsgFriendInfo : BaseMsg
    {
        public const byte TYPE_ONLINE = 12; //上线了
        public const byte TYPE_OFFLIE = 13; //离线了
        public const byte TYPE_KILL = 14;        //绝交
        public const byte TYPE_FRIEND = 15;//好友标记
        public const byte TYPE_ADDFRIEND = 10;  //添加好友标记
        public const byte TYPE_AGREED = 11;     //同意添加好友
        public const byte TYPE_REFUSE = 21;     //拒绝添加好友
        public uint playerid;
        public uint fightpower;      //战斗力
        public byte type; //好友仇人标识
        public byte Online; //是否是在线状态标记  1.在线 0.离线
        public byte level; //等级
        public byte param = 0; //未知
        public String name; //32
        public MsgFriendInfo()
        {
            mMsgLen = 48;
            mParam = PacketProtoco.S_FRIENDINFO;
            name = "";
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                playerid = inpack.ReadUInt32();
                fightpower = inpack.ReadUInt32();
                type = inpack.ReadByte();
                Online = inpack.ReadByte();
                level = inpack.ReadByte();
                param = inpack.ReadByte();
            }
        }
        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(playerid);
            outpack.WriteUInt32(fightpower);
            outpack.WriteByte(type);
            outpack.WriteByte(Online);
            outpack.WriteByte(level);
            outpack.WriteByte(param);
            byte[] namebyte = Coding.GetDefauleCoding().GetBytes(name);
            outpack.WriteBuff(namebyte);
            byte[] end = new byte[32 - namebyte.Length];
            outpack.WriteBuff(end);
            return outpack.Flush();
        }
    }

    //交易
    public class MsgTradInfo : BaseMsg
    {

        public const byte REQUEST_TRAD = 1;//1.请求交易
        public const byte QUIT_TRAD = 2;    //取消交易
        public const byte ITEM_TRAD = 6;    //道具
        public const byte GOLD_TRAD = 7;    //金币交易
        public const byte SURE_TRAD = 10;   //确定
        public const byte GAMEGOLD_TRAD = 13;//魔石
        public uint typeid;
       
        public short type;
        public short fightpower;    //战斗力
        public short level;     //等级
        public short param;
        public MsgTradInfo()
        {
            mMsgLen = 16;
            mParam = PacketProtoco.S_TRAD;
        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                inpack.ReadUInt16();
                typeid = inpack.ReadUInt32();
                type = inpack.ReadInt16();
                fightpower = inpack.ReadInt16();
                level = inpack.ReadInt16();
                param = inpack.ReadInt16();
                  
               
            }
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(typeid);
            outpack.WriteInt16(type);
            outpack.WriteInt16(fightpower);
            outpack.WriteInt16(level);
            outpack.WriteInt16(param);
            return outpack.Flush();
        }
    }

    public class MsgGuanJueItem
    {
        public String name;
        public ulong guanjue;
        public int pos; //排行
    }

    //爵位信息
    public class MsgGuanJueInfo : BaseMsg
    {
        public short param = 2;
        public int page;        //当前页面
        public int param1 = 5;
        public short param2 = 0;
        public int param3 = 0;
        public List<MsgGuanJueItem> list_item;
        public MsgGuanJueInfo()
        {
            mParam = PacketProtoco.S_GUANJUE;
            mMsgLen = 22;
            list_item = new List<MsgGuanJueItem>();
        }

        public override byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut(mKey);
            mMsgLen += (ushort)(list_item.Count * 40);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt16(param);
            outpack.WriteInt32(page);
            outpack.WriteInt32(param1);
            outpack.WriteInt16(param2);
            outpack.WriteInt16((short)list_item.Count);
            outpack.WriteInt32(param3);
            for (int i = 0; i < list_item.Count; i++)
            {
                byte[] namebyte = Coding.GetDefauleCoding().GetBytes(list_item[i].name);
                byte[] namebyteex = null;
                //防止溢出- 2016.1.23
                if (namebyte.Length > 15)
                {
                    namebyteex = new byte[15];
                    Buffer.BlockCopy(namebyte, 0, namebyteex, 0, 15);
                }
                else
                {
                    namebyteex = new byte[namebyte.Length];
                    Buffer.BlockCopy(namebyte, 0, namebyteex, 0, namebyte.Length);
                }
                outpack.WriteBuff(namebyteex);
                byte[] byte_ = new byte[16 - namebyteex.Length];
                outpack.WriteBuff(byte_);
                outpack.WriteInt32(0);
                outpack.WriteULong(list_item[i].guanjue);
                outpack.WriteInt32(1);
                outpack.WriteInt32(list_item[i].pos);
                outpack.WriteInt32(0);
              
            }
            return outpack.Flush();
        }
    }
    //自身行会信息
    public class MsgSelfLegionInfo : BaseMsg
    {
        //0, 0, 0, 0, 0, 0, 0, 0, 0, 232, 3,
        public uint legion_id;  //军团id
        public int money;       //军团资金
        public int devote;  //贡献值
        public byte[] param = new byte[12];
        public int param1 = 1;
        public int param2;
        public short param3;
        public short place; //军团职位
        public short param4;
        public byte title = 1; //军团称谓
        public byte[] param5 = new byte[23];
        public String legion_name; //军团名称 18字节
        public uint legion_id1;
        public byte[] param6 = new byte[20];
        public MsgSelfLegionInfo()
        {
            mMsgLen = 108;
            mParam = PacketProtoco.S_SELFLEGIONINFO;
            legion_name = "";

        }

        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
    
            if (legion_id1 == 0)
            {
                legion_id1 = legion_id;
            }
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(legion_id);
            outpack.WriteInt32(money);
            outpack.WriteInt32(devote);
            outpack.WriteBuff(param);
            outpack.WriteInt32(param1);
            outpack.WriteInt32(param2);
            outpack.WriteInt16(param3);
            outpack.WriteInt16(place);
            outpack.WriteInt16(param4);
            outpack.WriteByte(title);
            outpack.WriteBuff(param5);
            byte[] byte_name = Coding.GetDefauleCoding().GetBytes(legion_name);
            outpack.WriteBuff(byte_name);
            int nLen = 18 - byte_name.Length;
            if (nLen > 0)
            {
                byte[] byte_len = new byte[nLen];
                byte_len = new byte[nLen];
                outpack.WriteBuff(byte_len);
            }
            outpack.WriteUInt32(legion_id1);
            outpack.WriteBuff(param6);
            return outpack.Flush();
        }
    }
    //军团名称
    public class MsgLegionName : BaseMsg
    {
        //           byte[] legion_name = Coding.GetDefauleCoding().GetBytes(legion.GetBaseInfo().name);
       //     PacketOut outpack = new PacketOut(play.GetGamePackKeyEx());
      //      outpack.WriteUInt16((ushort)(12 + legion_name.Length));
      //      outpack.WriteUInt16(1015);
      //      outpack.WriteUInt32(legion.GetBaseInfo().id);
      //      outpack.WriteUInt16(3);
      //      outpack.WriteByte(1);
      //      outpack.WriteString(legion.GetBaseInfo().name);
      //      outpack.WriteByte(0);

     //       play.SendData(outpack.Flush());
        public uint legion_id;
        public String legion_name;
        public MsgLegionName()
        {
            mParam = PacketProtoco.S_LEGION_NAME;
            legion_name = "";
            mMsgLen = 12;
        }
        public override void Create(byte[] msg = null, GamePacketKeyEx key = null)
        {
            base.Create(msg, key);
        }

        public override byte[] GetBuffer()
        {
            mMsgLen += (ushort)Coding.GetDefauleCoding().GetBytes(legion_name).Length;
            PacketOut outpack = new PacketOut(mKey);
            outpack.WriteUInt16(mMsgLen);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(legion_id);
            outpack.WriteUInt16(3);
            outpack.WriteByte(1);
            outpack.WriteString(legion_name);
            outpack.WriteByte(0);
            return outpack.Flush();
        }
    }

 
}
