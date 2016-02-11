using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//内部通讯的消息--
namespace GameBase.Network.Internal
{

    public class Define
    {
        public const ushort OPENLOGINSERVER = 10;
        public const ushort QUERYROLE = 11;     //loginserver 发送给dbserver查询角色
        public const ushort QUERYROLE_RET = 12; //dbserver发送给loginserver查询角色

        public const ushort OPENMAPSERVER = 111;
        public const ushort ROLEINFO = 112;         //dbserver 发送给mapserver的角色信息
        public const ushort ROLEINFO_RET = 113;     //loginserver 发送给dbserver
        public const ushort QUERYROLENAME = 114;    //mapserver 发给dbserver 查询角色是否重名
        public const ushort QUERYROLENAME_RET = 115;    //db发给mapserver


        public const ushort CREATEROLE = 115;       //mapserver 发给 dbserver 创建角色
        public const ushort CREATEROLE_RET = 116; //dbserver 发给mapserver 创建角色
        public const ushort SAVEROLEDATA_ATTR = 117; //保存玩家基本数据
        public const ushort DELETEROLEDATA_ITEM = 119;  //删除角色道具
        public const ushort ADDROLEDATA_ITEM = 120;     //增加角色道具
        public const ushort ADDROLEDATA_ITEM_RET = 121; //增加角色道具返回

        public const ushort LOADROLEDATA_ITEM = 123;        //载入玩家物品
        public const ushort SAVEROLEDATA_ITEM = 124;        //保存玩家物品
        public const ushort LOADROLEDATA_MAGIC = 125;       //载入玩家技能
        public const ushort SAVEROLEDATA_MAGIC = 126;       //保存玩家技能
        public const ushort KICKGAMEPLAY = 127;             //DBserver发给mapserver 踢掉玩家
        public const ushort LOADROLEDATA_EUDEMON = 128;     //载入玩家幻兽信息
        public const ushort SAVEROLEDATA_EUDEMON = 129;     //保存玩家幻兽数据
        public const ushort LOADROLEDATA_FRIEND = 130;      //载入玩家好友数据
        public const ushort SAVEROLEDATA_FRIEND = 131;      //保存玩家好友数据
        public const ushort GUANJUEDATA = 132;              //爵位信息 dbserver 发给mapserver
        public const ushort UPDATEGUANJUEDATA = 133;        //更新爵位信息 mapserver发给dbserver
        public const ushort LOADLEGION = 134;               //载入军团信息
        public const ushort SAVELEGION = 135;               //保存单个军团信息
        public const ushort UPDATELEGION = 136;             //更新单个军团信息
        public const ushort CREATELEGION = 137;             //创建军团
        public const ushort CREATELEGION_RET = 138;         //创建军团返回
        public const ushort LOADPAYRECINFO = 139;           //载入充值信息
        public const ushort UPDATEPAYRECINFO = 140;         //更新充值信息
        
        public const byte TYPE_MAPSERVER = 5; //游戏服务器
        public const byte TYPE_LOGINSERVER = 2; //登录服务器
    }
    //mapserver 与 dbserver 通讯的包
    public class OpenMapSession
    {
        public ushort mParam;
        public byte mType;
        public String text = "MapServer";
        public OpenMapSession()
        {
            mParam = Define.OPENMAPSERVER;
            mType = Define.TYPE_MAPSERVER;
        }
        public byte[] GetBuff()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteByte(mType);
            outpack.WriteString(text);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    //loginserver 与 dbserver通讯的包
    public class OpenLoginSession
    {
        public ushort mParam;
        public byte mType;
        public String text = "LoginServer";
        public OpenLoginSession()
        {
            mParam = Define.OPENMAPSERVER;
            mType = Define.TYPE_LOGINSERVER;
        }
        public byte[] GetBuff()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteByte(mType);
            outpack.WriteString(text);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }


    //loginserver 发给dbserver 查询数据库角色的包
    public class QueryRole
    {
        public ushort mParam;
        public uint gameid;
        public int key;
        public int key2;
        public byte[] account;
        public QueryRole(uint _gameid = 0, int _key = 0, int _key2 = 0, byte[] _account = null)
        {
            mParam = Define.QUERYROLE;
            gameid = _gameid;
            key = _key;
            key2 = _key2;
            account = _account;
        }

        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            mParam = inpack.ReadUInt16();
            gameid = inpack.ReadUInt32();
            key = inpack.ReadInt32();
            key2 = inpack.ReadInt32();
            account = inpack.ReadBuff(16);

        }
        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(gameid);
            outpack.WriteInt32(key);
            outpack.WriteInt32(key2);
            outpack.WriteBuff(account);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }

        public String GetAccount()
        {
            int nPos = 0;
            for (int i = 0; i < account.Length; i++)
            {
                if (account[i] == 0)
                {
                    nPos = i;
                    break;
                }
            }
            byte[] buf = new byte[nPos];
            Buffer.BlockCopy(account, 0, buf, 0, nPos);
            return GameBase.Core.Coding.GetDefauleCoding().GetString(buf);
        }
    }
    //查询角色返回
    public class QueryRole_Ret
    {
        public ushort mParam;
        public uint gameid;
        public int key;
        public int key2;
        public byte ret;
        public QueryRole_Ret()
        {
            mParam = Define.QUERYROLE_RET;
            gameid = 0;
            key = key2 = 0;
            ret = 0;
        }
        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            gameid = inpack.ReadUInt32();
            key = inpack.ReadInt32();
            key2 = inpack.ReadInt32();
            ret = inpack.ReadByte();
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(gameid);
            outpack.WriteInt32(key);
            outpack.WriteInt32(key2);
            outpack.WriteByte(ret);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }
    //dbserver 发给mapserver 数据库角色的详细信息包-
    public class RoleInfo
    {
        public ushort mParam;
        public bool isRole;   //是否有角色
        public uint gameid;
        public int mKey;
        public int mKey1;
        public int accountid; //帐号id
        public String sAccount; //游戏帐号
        public int playerid;
        public String name;   //角色名称
        public uint lookface;   //角色性别与头像
        public uint hair;       //发型
        public byte lv;     //角色等级
        public uint exp;       //当前经验值
        public uint life;       //当前生命
        public uint mana;       //当前魔法
        public byte profession; //职业
        public short pk;       //pk值
        public int gold;      //金币
        public int gamegold;  //魔石
        public int stronggold; //仓库金币
        public int mapid;       //所在地图id
        public short x;         //所在地图坐标x
        public short y;         //所在地图坐标y
        public String hotkey;   //热键信息
        public ulong guanjue;
        public int godlevel;        //神等级
        public byte maxeudemon; //最大召唤幻兽商量
        public RoleInfo(byte[] msg = null)
        {

            mParam = Define.ROLEINFO;
            gameid = 0;
            isRole = false;
            mKey = 0;
            mKey1 = 0;
            accountid = 0;
            sAccount = "";
            name = "";
            lookface = 0;
            hair = 0;
            lv = 0;
            exp = 0;
            life = 0;
            mana = 0;
            profession = 0;
            pk = 0;
            gold = 0;
            gamegold = 0;
            mapid = 0;
            x = 0;
            y = 0;
            playerid = 0;
            hotkey = "";
            guanjue = 0;
            godlevel = 0;
            maxeudemon = 2;
            if (msg != null)
            {
                PackIn inpack = new PackIn(msg);
                mParam = inpack.ReadUInt16();
                gameid = inpack.ReadUInt32();
                
                mKey = inpack.ReadInt32();
                mKey1 = inpack.ReadInt32();
                accountid = inpack.ReadInt32();
                sAccount = inpack.ReadString();
                playerid = inpack.ReadInt32();
                isRole = inpack.ReadBool();
                if (isRole)
                {
                    name = inpack.ReadString();
                    lookface = inpack.ReadUInt32();
                    hair = inpack.ReadUInt32();
                    lv = inpack.ReadByte();
                    exp = inpack.ReadUInt32();
                    life = inpack.ReadUInt32();
                    mana = inpack.ReadUInt32();
                    profession = inpack.ReadByte();
                    pk = inpack.ReadInt16();
                    gold = inpack.ReadInt32();
                    gamegold = inpack.ReadInt32();
                    stronggold = inpack.ReadInt32();
                    mapid = inpack.ReadInt32();
                    x = inpack.ReadInt16();
                    y = inpack.ReadInt16();
                    hotkey = inpack.ReadString();
                    guanjue = inpack.ReadULong();
                    godlevel = inpack.ReadInt32();
                    maxeudemon = inpack.ReadByte();
                }

            }
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(gameid);
            outpack.WriteInt32(mKey);
            outpack.WriteInt32(mKey1);
            outpack.WriteInt32(accountid);
            outpack.WriteString(sAccount);
            outpack.WriteInt32(playerid);
            outpack.WriteBool(isRole);
            outpack.WriteString(name);
            outpack.WriteUInt32(lookface);
            outpack.WriteUInt32(hair);
            outpack.WriteByte(lv);
            outpack.WriteUInt32(exp);
            outpack.WriteUInt32(life);
            outpack.WriteUInt32(mana);
            outpack.WriteByte(profession);
            outpack.WriteInt16(pk);
            outpack.WriteInt32(gold);
            outpack.WriteInt32(gamegold);
            outpack.WriteInt32(stronggold);
            outpack.WriteInt32(mapid);
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteString(hotkey);
            outpack.WriteULong(guanjue);
            outpack.WriteInt32(godlevel);
            outpack.WriteByte(maxeudemon);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    public class RoleInfo_Ret
    {
        public ushort mParam;
        public uint gameid;
        public int key;
        public int key2;
        public int accountid;
        public RoleInfo_Ret()
        {
            mParam = Define.ROLEINFO_RET;
            gameid = 0;
            key = key2 = 0;
            accountid = 0;
        }

        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            gameid = inpack.ReadUInt32();
            key = inpack.ReadInt32();
            key2 = inpack.ReadInt32();
            accountid = inpack.ReadInt32();
        }
        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(gameid);
            outpack.WriteInt32(key);
            outpack.WriteInt32(key2);
            outpack.WriteInt32(accountid);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    public class QueryRoleName
    {
        public ushort mParam;
        public uint gameid;
        public String name;
        public QueryRoleName()
        {
            mParam = Define.QUERYROLENAME;
            gameid = 0;
            name = "";
        }

        public void Create(byte[] data)
        {
            PackIn inpack = new PackIn(data);
            inpack.ReadUInt16();
            gameid = inpack.ReadUInt32();
            name = inpack.ReadString();
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(gameid);
            outpack.WriteString(name);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    public class QueryRoleName_Ret
    {
        public ushort mParam;
        public uint gameid;
        public bool tag;
        public QueryRoleName_Ret()
        {
            mParam = Define.QUERYROLENAME_RET;
            gameid = 0;
            tag = false;
        }

        public void Create(byte[] data)
        {
            PackIn inpack = new PackIn(data);
            inpack.ReadUInt16();
            gameid = inpack.ReadUInt32();
            tag = inpack.ReadBool();
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(gameid);
            outpack.WriteBool(tag);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    public class CreateRole
    {
        public ushort mParam;
        public uint gameid;
        public int accountid;
        public String name;     //角色名称
        public uint lookface; //头像与性别
        public byte profession; //职业
        public CreateRole()
        {
            mParam = Internal.Define.CREATEROLE;
            gameid = 0;
            accountid = 0;
            name = "";
            lookface = 0;
            profession = 0;
        }

        public void Create(byte[] data)
        {
            PackIn inpack = new PackIn(data);
            inpack.ReadUInt16();
            gameid = inpack.ReadUInt32();
            accountid = inpack.ReadInt32();
            name = inpack.ReadString();
            lookface = inpack.ReadUInt32();
            profession = inpack.ReadByte();
        }
        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(gameid);
            outpack.WriteInt32(accountid);
            outpack.WriteString(name);
            outpack.WriteUInt32(lookface);
            outpack.WriteByte(profession);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    public class CreateRole_Ret
    {
        public ushort mParam;
        public int playerid;
        public uint gameid;
        public bool tag;    //true 创建成 false 创建失败
        public CreateRole_Ret()
        {
            mParam = Define.CREATEROLE_RET;
            playerid = 0;
            gameid = 0;
            tag = false;

        }
        public void Create(byte[] data)
        {
            PackIn inpack = new PackIn(data);
            inpack.ReadUInt16();
            playerid = inpack.ReadInt32();
            gameid = inpack.ReadUInt32();
            tag = inpack.ReadBool();

        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(playerid);
            outpack.WriteUInt32(gameid);
            outpack.WriteBool(tag);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    //保存玩家基本数据
    public class SaveRoleData_Attr
    {
        public ushort mParam;
        public int accountid;
        public bool IsExit;
        public String name;
        public uint lookface;
        public uint hair;
        public byte level;
        public int exp;
        public uint life;
        public uint mana;
        public byte profession;
        public short pk;
        public long gold;
        public long gamegold;
        public long stronggold;
        public uint mapid;
        public short x;
        public short y;
        public ulong guanjue;   //爵位
        public byte godlevel;   //神等级
        public byte maxeudemon; //最大可召唤幻兽数量
        public String hotkey;
      
        public SaveRoleData_Attr()
        {
            mParam = Define.SAVEROLEDATA_ATTR;
            accountid = 0;
            name = "";
            lookface = 0;
            hair = 0;
            level = 0;
            exp = 0;
            life = 0;
            mana = 0;
            profession = 0;
            pk = 0;
            gold = 0;
            gamegold = 0;
            mapid = 0;
            x = 0;
            y = 0;
            hotkey = "";
            guanjue = 0;
            godlevel = 0;
            maxeudemon = 2;
        }

        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);

            inpack.ReadUInt16();
            accountid = inpack.ReadInt32();
            IsExit = inpack.ReadBool();
            name = inpack.ReadString();
            lookface = inpack.ReadUInt32();
            hair = inpack.ReadUInt32();
            level = inpack.ReadByte();
            exp = inpack.ReadInt32();
            life = inpack.ReadUInt32();
            mana = inpack.ReadUInt32();
            profession = inpack.ReadByte();
            pk = inpack.ReadInt16();
            gold = inpack.ReadLong();
            gamegold = inpack.ReadLong();
            stronggold = inpack.ReadLong();
            mapid = inpack.ReadUInt32();
            x = inpack.ReadInt16();
            y = inpack.ReadInt16();
            hotkey = inpack.ReadString();
            guanjue = inpack.ReadULong();
            godlevel = inpack.ReadByte();
            maxeudemon = inpack.ReadByte();
        }
        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(accountid);
            outpack.WriteBool(IsExit);
            outpack.WriteString(name);
            outpack.WriteUInt32(lookface);
            outpack.WriteUInt32(hair);
            outpack.WriteByte(level);
            outpack.WriteInt32(exp);
            outpack.WriteUInt32(life);
            outpack.WriteUInt32(mana);
            outpack.WriteByte(profession);
            outpack.WriteInt16(pk);
            outpack.WriteLong(gold);
            outpack.WriteLong(gamegold);
            outpack.WriteLong(stronggold);
            outpack.WriteUInt32(mapid);
            outpack.WriteInt16(x);
            outpack.WriteInt16(y);
            outpack.WriteString(hotkey);
            outpack.WriteULong(guanjue);
            outpack.WriteByte(godlevel);
            outpack.WriteByte(maxeudemon);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    public class AddRoleData_Item_Ret
    {
        public ushort mParam;
        public uint gameid;
        public uint sordid;
        public uint id; //数据库道具索引id
        public AddRoleData_Item_Ret()
        {
            mParam = Define.ADDROLEDATA_ITEM_RET;
            gameid = sordid = id = 0;
        }
        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            gameid = inpack.ReadUInt32();
            sordid = inpack.ReadUInt32();
            id = inpack.ReadUInt32();
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(gameid);
            outpack.WriteUInt32(sordid);
            outpack.WriteUInt32(id);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();

        }


    }
    //增加玩家道具
    public class AddRoleData_Item
    {
        public ushort mParam;
        public uint gameid;
        public uint sortid;             //临时表的id
        public RoleData_Item item;
      
        public AddRoleData_Item()
        {
            mParam = Define.ADDROLEDATA_ITEM;
            item = new RoleData_Item();
         
        }
        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            gameid = inpack.ReadUInt32();
            sortid = inpack.ReadUInt32();
            item.Create(null, inpack);
            //playerid = inpack.ReadInt32();
            //itemid = inpack.ReadUInt32();
            //postion = inpack.ReadUInt16();
            //stronglv = inpack.ReadByte();
            //gemcount = inpack.ReadByte();
            //gem1 = inpack.ReadUInt32();
            //gem2 = inpack.ReadUInt32();
            //forgename = inpack.ReadString();
            //amount = inpack.ReadUInt16();
            //war_ghost_exp = inpack.ReadInt32();
            //di_attack = inpack.ReadByte();
            //shui_attack = inpack.ReadByte();
            //huo_attack = inpack.ReadByte();
            //feng_attack = inpack.ReadByte();
            //property = inpack.ReadInt32();
            //gem3 = inpack.ReadByte();
            //god_strong = inpack.ReadInt32();
            //god_exp = inpack.ReadInt32();


        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(gameid);
            outpack.WriteUInt32(sortid);
            outpack.WriteBuff(item.GetBuffer());
            //outpack.WriteInt32(playerid);
            //outpack.WriteUInt32(itemid);
            //outpack.WriteUInt16(postion);
            //outpack.WriteByte(stronglv);
            //outpack.WriteByte(gemcount);
            //outpack.WriteUInt32(gem1);
            //outpack.WriteUInt32(gem2);
            //outpack.WriteString(forgename);
            //outpack.WriteUInt16(amount);
            //outpack.WriteInt32(war_ghost_exp);
            //outpack.WriteByte(di_attack);
            //outpack.WriteByte(shui_attack);
            //outpack.WriteByte(huo_attack);
            //outpack.WriteByte(feng_attack);
            //outpack.WriteInt32(property);
            //outpack.WriteByte(gem3);
            //outpack.WriteInt32(god_strong);
            //outpack.WriteInt32(god_exp);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }

    }

    public class ROLEDATA_ITEM
    {
        public ushort mParam;
        public int playerid;

        public int key;
        public int key2;
        public List<RoleData_Item> mListItem;
        public ROLEDATA_ITEM()
        {


            key = 0;
            key2 = 0;
            mListItem = new List<RoleData_Item>();
        }
        public void SetLoadTag()
        {
            mParam = Define.LOADROLEDATA_ITEM;
        }
        public void SetSaveTag()
        {
            mParam = Define.SAVEROLEDATA_ITEM;
        }

        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();

            playerid = inpack.ReadInt32();
            key = inpack.ReadInt32();
            key2 = inpack.ReadInt32();
            int count = inpack.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                RoleData_Item item = new RoleData_Item();
                item.Create(null, inpack);
                //-----------------------------------------------
                //item.id = inpack.ReadUInt32();
                //item.itemid = inpack.ReadUInt32();
                //item.postion = inpack.ReadUInt16();
                //item.stronglv = inpack.ReadByte();
                //item.gemcount = inpack.ReadByte();
                //item.gem1 = inpack.ReadUInt32();
                //item.gem2 = inpack.ReadUInt32();
                //item.forgename = inpack.ReadString();
                //item.amount = inpack.ReadUInt16();

                //item.war_ghost_exp = inpack.ReadInt32();
                //item.di_attack = inpack.ReadByte();
                //item.shui_attack = inpack.ReadByte();
                //item.huo_attack = inpack.ReadByte();
                //item.feng_attack = inpack.ReadByte();
                //item.property = inpack.ReadInt32();
                //item.gem3 = inpack.ReadByte();
                //item.god_strong = inpack.ReadInt32();
                //item.god_exp = inpack.ReadInt32();
                //------------------------------------------------

                mListItem.Add(item);
            }
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();

            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);

            outpack.WriteInt32(playerid);
            outpack.WriteInt32(key);
            outpack.WriteInt32(key2);
            outpack.WriteInt32(mListItem.Count);
            for (int i = 0; i < mListItem.Count; i++)
            {
                RoleData_Item item = mListItem[i];
                outpack.WriteBuff(item.GetBuffer());
                //outpack.WriteUInt32(item.id);
                //outpack.WriteUInt32(item.itemid);
                //outpack.WriteUInt16(item.postion);
                //outpack.WriteByte(item.stronglv);
                //outpack.WriteByte(item.gemcount);
                //outpack.WriteUInt32(item.gem1);
                //outpack.WriteUInt32(item.gem2);
                //outpack.WriteString(item.forgename);
                //outpack.WriteUInt16(item.amount);
                //outpack.WriteInt32(item.war_ghost_exp);
                //outpack.WriteByte(item.di_attack);
                //outpack.WriteByte(item.shui_attack);
                //outpack.WriteByte(item.huo_attack);
                //outpack.WriteByte(item.feng_attack);
                //outpack.WriteInt32(item.property);
                //outpack.WriteByte(item.gem3);
                //outpack.WriteInt32(item.god_strong);
                //outpack.WriteInt32(item.god_exp);

            }
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
        //取幻兽列表
        public List<RoleData_Item> GetEudemonItemList()
        {
            List<RoleData_Item> list = null;
            const ushort EUDEMON_POSTION = 53;
            for (int i = 0; i < mListItem.Count; i++)
            {
                if (mListItem[i].postion == EUDEMON_POSTION) //幻兽背包
                {
                    if (list == null) list = new List<RoleData_Item>();
                    list.Add(mListItem[i]);
                }
            }
            return list;
        }

    }

    public class RoleData_Magic
    {
        public ushort mParam;
        public int ownerid;
        public int key;
        public int key2;
        public List<MagicInfo> mListMagic;
        public RoleData_Magic()
        {
            mListMagic = new List<MagicInfo>();

            key = key2 = ownerid = 0;
        }
        public void SetLoadTag()
        {
            mParam = Define.LOADROLEDATA_MAGIC;
        }

        public void SetSaveTag()
        {
            mParam = Define.SAVEROLEDATA_MAGIC;
        }
        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            ownerid = inpack.ReadInt32();
            key = inpack.ReadInt32();
            key2 = inpack.ReadInt32();
            int count = inpack.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                MagicInfo item = new MagicInfo();
                item.id = inpack.ReadInt32();
                item.magicid = inpack.ReadUInt32();
                item.level = inpack.ReadByte();
                item.exp = inpack.ReadUInt32();
                mListMagic.Add(item);
            }
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(ownerid);
            outpack.WriteInt32(key);
            outpack.WriteInt32(key2);
            outpack.WriteInt32(mListMagic.Count);
            for (int i = 0; i < mListMagic.Count; i++)
            {
                MagicInfo item = mListMagic[i];
                outpack.WriteInt32(item.id);
                outpack.WriteUInt32(item.magicid);
                outpack.WriteByte(item.level);
                outpack.WriteUInt32(item.exp);
            }
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    public class MagicInfo
    {
        public int id;
        public int ownerid;
        public uint magicid;
        public byte level;
        public uint exp;
        public MagicInfo()
        {
            id = ownerid = 0;
            magicid = 0;
            level = 0;
            exp = 0;
        }

    }
    //保存玩家道具信息
    public class RoleData_Item
    {

        //  public ushort mParam;
        public uint id;             //数据库的道具索引id
        public int playerid;        //玩家id
        public uint itemid;             //道具id
        public ushort postion;              //道具位置
        public byte stronglv;           //强化等级
      
        public uint gem1;               //镶嵌的第一个宝石id
        public uint gem2;               //镶嵌的第二个宝石id
        public String forgename;        //锻造者名称
        public ushort amount;           //数量
        public int war_ghost_exp;//战魂经验
        public byte di_attack;  //地攻击
        public byte shui_attack; //水攻击
        public byte huo_attack; //火攻击
        public byte feng_attack; //风攻击
        public int property;
        public uint gem3;   //第三个宝石
        public int god_strong;  //神炼强度
        public int god_exp; //神佑经验


        public RoleData_Item()
        {
            //mParam = Define.SAVEROLEDATA_ITEM;
            forgename = "";
            playerid = 0;
            itemid = gem1 = gem2 = 0;
           stronglv = 0;
            amount = 0;
            postion = 0;
            id = 0;
            war_ghost_exp = 0;
            di_attack = 0;
            shui_attack = 0;
            huo_attack = 0;
            feng_attack = 0;
            property = 0;
            gem3 = 0;
            god_strong = 0;
            god_exp = 0;
        }



        public void Create(byte[] msg = null, PackIn _inpack = null)
        {
            PackIn inpack = null;
            if (_inpack != null)
            {
                inpack = _inpack;
            }
            else inpack = new PackIn(msg); ;

            id = inpack.ReadUInt32();
            playerid = inpack.ReadInt32();
            itemid = inpack.ReadUInt32();
            postion = inpack.ReadUInt16();
            stronglv = inpack.ReadByte();
          
            gem1 = inpack.ReadUInt32();
            gem2 = inpack.ReadUInt32();
            forgename = inpack.ReadString();
            amount = inpack.ReadUInt16();
            war_ghost_exp = inpack.ReadInt32();
            di_attack = inpack.ReadByte();
            shui_attack = inpack.ReadByte();
            huo_attack = inpack.ReadByte();
            feng_attack = inpack.ReadByte();
            property = inpack.ReadInt32();
            gem3 = inpack.ReadUInt32();
            god_strong = inpack.ReadInt32();
            god_exp = inpack.ReadInt32();
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            //  outpack.WriteBuff(InternalPacket.HEAD);
            //  outpack.WriteUInt16(mParam);
            outpack.WriteUInt32(id);
            outpack.WriteInt32(playerid);
            outpack.WriteUInt32(itemid);
            outpack.WriteUInt16(postion);
            outpack.WriteByte(stronglv);
            
            outpack.WriteUInt32(gem1);
            outpack.WriteUInt32(gem2);
            outpack.WriteString(forgename);
            outpack.WriteUInt16(amount);
            outpack.WriteInt32(war_ghost_exp);
            outpack.WriteByte(di_attack);
            outpack.WriteByte(shui_attack);
            outpack.WriteByte(huo_attack);
            outpack.WriteByte(feng_attack);
            outpack.WriteInt32(property);
            outpack.WriteUInt32(gem3);
            outpack.WriteInt32(god_strong);
            outpack.WriteInt32(god_exp);

            //  outpack.WriteBuff(InternalPacket.TAIL);


            return outpack.GetBuffer();
        }


    }


    //通知数据库服务器删除道具
    public class DeleteItemByID
    {
        public ushort mParam;
        public int playerid;
        public uint id;
        public ushort postion;
        public DeleteItemByID()
        {
            mParam = Define.DELETEROLEDATA_ITEM;
            playerid = 0;
            id = 0;
        }

        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            playerid = inpack.ReadInt32();
            id = inpack.ReadUInt32();
            postion = inpack.ReadUInt16();
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(playerid);
            outpack.WriteUInt32(id);
            outpack.WriteUInt16(postion);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }
    //踢掉玩家
    public class KickGamePlay
    {
        public ushort mParam;
        public int accountid;
        public KickGamePlay()
        {
            mParam = Define.KICKGAMEPLAY;
            accountid = 0;
        }
        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            accountid = inpack.ReadInt32();

        }
        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(accountid);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    public class RoleData_Eudemon
    {   
        public uint id;            //数据库里的cq_eudemon 主键，仅仅只是拿来判断是否是添加幻兽数据与更新数据用的
        public uint itemid;     //道具id-数据库的主键 cq_item 的id,不是基础道具id
        public String name; //幻兽名称
        public float phyatk_grow_rate;
        public float phyatk_grow_rate_max;
        public float magicatk_grow_rate;
        public float magicatk_grow_rate_max;
        public float life_grow_rate;
        public float defense_grow_rate;
        public float magicdef_grow_rate;
        public int init_life;
        public int init_atk_min;
        public int init_atk_max;
        public int init_magicatk_min;
        public int init_magicatk_max;
        public int init_defense;
        public int init_magicdef;
        public int luck;
        public int intimacy;
        public short level;
        public int card;
        public int exp;
        public int quality;
        public int wuxing; //五行属性
        public int recall_count; //转世次数
        public List<GameBase.Network.Internal.MagicInfo> mListMagicInfo;
        //不存储到数据库- 幻兽的动态id
        public uint typeid;
        public int life_max;    //最大血量
        public int life;        //当前血量
        public int atk_min;     //最小攻击
        public int atk_max;     //最大攻击
        public int magicatk_max;    //最大魔法攻击
        public int magicatk_min;    //最小魔法攻击
        public int defense;        //物理防御
        public int magicdef;   //魔法防御
        public bool bDie;
        public uint GetTypeID()
        {
            return typeid;
        }
        public RoleData_Eudemon()
        {
          
            id = 0;
            itemid = 0;
            name = "";
            phyatk_grow_rate = 0; ;
            phyatk_grow_rate_max = 0; ;
            magicatk_grow_rate = 0; ;
            magicatk_grow_rate_max = 0; ;
            life_grow_rate = 0; ;
            defense_grow_rate = 0; ;
            magicdef_grow_rate = 0; ;
            init_life = init_life = 0;
            init_atk_min = 0;
            init_atk_max = 0;
            init_magicatk_min = 0;
            init_magicatk_max = 0;
            init_defense = 0;
            init_magicdef = 0;
            luck = 0;
            intimacy = 0;
            level = 0;
            card = 0;
            exp = 0;
            quality = 0;
            wuxing = 0;
            recall_count = 0;
            bDie = false;
            mListMagicInfo = new List<GameBase.Network.Internal.MagicInfo>();
        }

        public void Create(byte[] msg = null, PackIn _inpack = null)
        {
            PackIn inpack = null;
            if (msg != null)
            {
                inpack = new PackIn(msg);
            }
            else inpack = _inpack;
            id = inpack.ReadUInt32();
            itemid = inpack.ReadUInt32();
            name = inpack.ReadString();
            phyatk_grow_rate = inpack.ReadFloat();
            phyatk_grow_rate_max = inpack.ReadFloat();
            magicatk_grow_rate = inpack.ReadFloat();
            magicatk_grow_rate_max = inpack.ReadFloat();
            life_grow_rate = inpack.ReadFloat();
            defense_grow_rate = inpack.ReadFloat();
            magicdef_grow_rate = inpack.ReadFloat();
            init_life = inpack.ReadInt32();
            init_atk_min = inpack.ReadInt32();
            init_atk_max = inpack.ReadInt32();
            init_magicatk_min = inpack.ReadInt32();
            init_magicatk_max = inpack.ReadInt32();
            init_defense = inpack.ReadInt32();
            init_magicdef = inpack.ReadInt32();
            luck = inpack.ReadInt32();
            intimacy = inpack.ReadInt32();
            level = inpack.ReadInt16();
            card = inpack.ReadInt32();
            exp = inpack.ReadInt32();
            quality = inpack.ReadInt32();
            recall_count = inpack.ReadInt32();
            wuxing = inpack.ReadInt32();
            int nMagicCount = inpack.ReadInt32();
            for (int i = 0; i < nMagicCount; i++)
            {
                GameBase.Network.Internal.MagicInfo info = new GameBase.Network.Internal.MagicInfo();
                //这里的owernid 宿主在上面一个包含类里面，所以不用读
                info.id = inpack.ReadInt32();
                info.magicid = inpack.ReadUInt32();
                info.exp = inpack.ReadUInt32();
                mListMagicInfo.Add(info);
            }
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteUInt32(id);            
            outpack.WriteUInt32(itemid);     
            outpack.WriteString(name);     
            outpack.WriteFloat(phyatk_grow_rate); 
            outpack.WriteFloat(phyatk_grow_rate_max);
            outpack.WriteFloat(magicatk_grow_rate);
            outpack.WriteFloat(magicatk_grow_rate_max);
            outpack.WriteFloat(life_grow_rate); 
            outpack.WriteFloat(defense_grow_rate); 
            outpack.WriteFloat(magicdef_grow_rate); 
            outpack.WriteInt32(init_life); 
            outpack.WriteInt32(init_atk_min); 
            outpack.WriteInt32(init_atk_max); 
            outpack.WriteInt32(init_magicatk_min); 
            outpack.WriteInt32(init_magicatk_max); 
            outpack.WriteInt32(init_defense); 
            outpack.WriteInt32(init_magicdef); 
            outpack.WriteInt32(luck); 
            outpack.WriteInt32(intimacy); 
            outpack.WriteInt16(level); 
            outpack.WriteInt32(card);
            outpack.WriteInt32(exp); 
            outpack.WriteInt32(quality);
            outpack.WriteInt32(recall_count);
            outpack.WriteInt32(wuxing);
            outpack.WriteInt32(mListMagicInfo.Count);
            for (int i = 0; i < mListMagicInfo.Count; i++)
            {
                outpack.WriteInt32(mListMagicInfo[i].id);
                //这里的owernid 宿主在上面一个包含类里面，所以不用写
                outpack.WriteUInt32(mListMagicInfo[i].magicid);
                outpack.WriteUInt32(mListMagicInfo[i].exp);
            }
                return outpack.GetBuffer();
        }

        public int GetInitAtk()
        {
            String s = init_atk_min.ToString() + init_atk_max.ToString();
            return Convert.ToInt32(s);
        }

        public int GetInitMagicAtk()
        {
            String s = init_magicatk_min.ToString() + init_magicatk_max.ToString();
            return Convert.ToInt32(s);
        }

        public int GetInitDefense()
        {
            String s = init_defense.ToString() + init_magicdef.ToString();
            return Convert.ToInt32(s);
        }
    }
    public class ROLEDATE_EUDEMON
    {
        public ushort mParam;
        public int playerid;
        public int key;
        public int key2;
        public List<RoleData_Eudemon> list_item;
        public ROLEDATE_EUDEMON()
        {

            list_item = new List<RoleData_Eudemon>();
        }

        public void SetLoadTag() { mParam = Define.LOADROLEDATA_EUDEMON; }
        public void SetSaveTag() { mParam = Define.SAVEROLEDATA_EUDEMON; }
        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            playerid = inpack.ReadInt32();
            key = inpack.ReadInt32();
            key2 = inpack.ReadInt32();
            int count = inpack.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                RoleData_Eudemon eudemon = new RoleData_Eudemon();
                eudemon.Create(null, inpack);
                list_item.Add(eudemon);
            }
        }


        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(playerid);
            outpack.WriteInt32(key);
            outpack.WriteInt32(key2);
            outpack.WriteInt32(list_item.Count);
            for (int i = 0; i < list_item.Count; i++)
            {
                outpack.WriteBuff(list_item[i].GetBuffer());
            }
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }

    }

    public class RoleData_Friend
    {
        public int id;//0 为保存 -1为删除
        public byte friendtype; //好友类型
        public uint friendid;
        public String friendname;
        public RoleData_Friend()
        {
            friendid = 0;
            friendname = "";
            id = 0;
        }
        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteInt32(id);
            outpack.WriteByte(friendtype);
            outpack.WriteUInt32(friendid);
            outpack.WriteString(friendname);
            return outpack.GetBuffer();
        }
        public void Create(byte[] msg, PackIn inpack)
        {
            id = inpack.ReadInt32();
            friendtype = inpack.ReadByte();
            friendid = inpack.ReadUInt32();
            friendname = inpack.ReadString();
        }
   
    }
    public class ROLEDATA_FRIEND
    {
        public ushort mParam;
        public int playerid;
        public int key;
        public int key2;
        public List<RoleData_Friend> list_item;
        public ROLEDATA_FRIEND()
        {
            list_item = new List<RoleData_Friend>();
        }

        public void SetLoadTag() { mParam = Define.LOADROLEDATA_FRIEND; }
        public void SetSaveTag() { mParam = Define.SAVEROLEDATA_FRIEND; }

        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            playerid = inpack.ReadInt32();
            key = inpack.ReadInt32();
            key2 = inpack.ReadInt32();
            int count = inpack.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                RoleData_Friend item = new RoleData_Friend();
                item.Create(null, inpack);
                list_item.Add(item);
            }
        }
        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(playerid);
            outpack.WriteInt32(key);
            outpack.WriteInt32(key2);
            outpack.WriteInt32(list_item.Count);
            for (int i = 0; i < list_item.Count; i++)
            {
                
                outpack.WriteBuff(list_item[i].GetBuffer());
            }
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    public class GuanJueInfo
    {
        public uint id;
        public String name;
        public ulong guanjue;
        public GuanJueInfo()
        {
            id = 0;
            guanjue = 0;
            name = "";
        }
        public void Create(PackIn inpack)
        {
            id = inpack.ReadUInt32();
            name = inpack.ReadString();
            guanjue = inpack.ReadULong();
        }
        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteUInt32(id);
            outpack.WriteString(name);
            outpack.WriteULong(guanjue);
            return outpack.GetBuffer();
        }
    }

    //更新充值信息
    public class PackUpdatePayRecInfo
    {
        public ushort mparam;
        public String account;
        public PackUpdatePayRecInfo()
        {
            mparam = Define.UPDATEPAYRECINFO; 
        }

        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadInt16();
            account = inpack.ReadString();
        }
        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mparam);
            outpack.WriteString(account);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
        
    }
    //充值信息封包
    public class PackPayRecInfo
    {
        public ushort mparam;
        public int id;
        public String order;
        public String account;
        public int money;

        public PackPayRecInfo()
        {
            mparam = Define.LOADPAYRECINFO;

        }
        public void Creaet(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadInt16();
            id = inpack.ReadInt32();
           
            account = inpack.ReadString();
            order = inpack.ReadString();
            money = inpack.ReadInt32();
         
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mparam);
            outpack.WriteInt32(id);
            outpack.WriteString(account);
            outpack.WriteString(order);
            outpack.WriteInt32(money);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }

    }
    //官爵信息封包
    public class GUANJUEINFO
    {
        public ushort mparam;
        public List<GuanJueInfo> list_item;
        public GUANJUEINFO()
        {
            mparam = Define.GUANJUEDATA;
            list_item = new List<GuanJueInfo>();
        }
        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            int count = inpack.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                GuanJueInfo info = new GuanJueInfo();
                info.Create(inpack);
                list_item.Add(info);
            }
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mparam);
            outpack.WriteInt32(list_item.Count);
            for (int i = 0; i < list_item.Count; i++)
            {
                outpack.WriteBuff(list_item[i].GetBuffer());
            }
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }

    }

    public class UPDATEGUANJUEDATA
    {
        public ushort mparam;
        public GuanJueInfo info;
        public UPDATEGUANJUEDATA()
        {
            mparam = Define.UPDATEGUANJUEDATA;
            info = new GuanJueInfo();
        }

        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            info.Create(inpack);
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mparam);
            outpack.WriteBuff(info.GetBuffer());
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }


    //行会成员信息
    public class LegionMember
    {
       // public uint legion_id;  //军团id
        public uint id;
        public String members_name; //
        public long money; //贡献度
        public short rank; //军团职位
        public bool boChange; //是否要更新数据库标志
        public LegionMember()
        {
            id = 0;
            boChange = false;
        }

        public void Create(PackIn inpack)
        {
            members_name = inpack.ReadString();
            money = inpack.ReadLong();
            rank = inpack.ReadInt16();
            boChange = inpack.ReadBool();
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteString(members_name);
            outpack.WriteLong(money);
            outpack.WriteInt16(rank);
            outpack.WriteBool(boChange);
            return outpack.GetBuffer();
        }
    }
    public class LegionInfo
    {
        public uint id; //行会id
        public String name; //行会名称
        public byte title; //军团称谓
        public int leader_id; //军团长id
        public String leader_name; //军团长名称
        public long money;  //行会资金
        public String notice; //行会公告
        public List<LegionMember> list_member; //行会成员信息
        public LegionInfo()
        {
            list_member = new List<LegionMember>();
            name = "";
        }
        public void Create(PackIn inpack)
        {
            id = inpack.ReadUInt32();
            name = inpack.ReadString();
            title = inpack.ReadByte();
            leader_id = inpack.ReadInt32();
            leader_name = inpack.ReadString();
            money = inpack.ReadLong();
            notice = inpack.ReadString();
            int count = inpack.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                LegionMember member = new LegionMember();
                member.Create(inpack);
                list_member.Add(member);
            }
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteUInt32(id);
            outpack.WriteString(name);
            outpack.WriteByte(title);
            outpack.WriteInt32(leader_id);
            outpack.WriteString(leader_name);
            outpack.WriteLong(money);
            outpack.WriteString(notice);
            outpack.WriteInt32(list_member.Count);
            for (int i = 0; i < list_member.Count; i++)
            {
                outpack.WriteBuff(list_member[i].GetBuffer());
            }
             return outpack.GetBuffer();
           
        }
    }
    public class LEGIONINFO
    {
        public ushort mParam;
        public List<LegionInfo> list_item;
        public LEGIONINFO()
        {
            mParam = Define.LOADLEGION;
            list_item = new List<LegionInfo>();
        }

        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadInt16();
            int count = inpack.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                LegionInfo info = new LegionInfo();
                info.Create(inpack);
                list_item.Add(info);
            }
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
           
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(list_item.Count);
            for (int i = 0; i < list_item.Count; i++)
            {
                outpack.WriteBuff(list_item[i].GetBuffer());
            }
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
           
        }
       
    }

    public class LegionOption
    {
        public ushort mParam;
        public int player_id;
        public LegionInfo mInfo;
        public LegionOption()
        {
           
            mInfo = new LegionInfo();
        }

        public void SetCreateTag()
        {
            mParam = Define.CREATELEGION;
        }
        public void SetUpdateTag()
        {
            mParam = Define.UPDATELEGION;
        }
        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            
            inpack.ReadUInt16();
            player_id = inpack.ReadInt32();
            mInfo.Create(inpack);
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteInt32(player_id);
            outpack.WriteBuff(mInfo.GetBuffer());
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    public class CreateLegion_Ret
    {
        public ushort mParam;
        public byte ret; //0.失败 1.成功
        public int play_id; //玩家id
        public int legion_id; //军团id
        public long money; //贡献度
        public uint boss_id; //cq_legion_members
        public CreateLegion_Ret()
        {
            mParam = Define.CREATELEGION_RET;
        }

        public void Create(byte[] msg)
        {
            PackIn inpack = new PackIn(msg);
            inpack.ReadUInt16();
            ret = inpack.ReadByte();
            play_id = inpack.ReadInt32();
            legion_id = inpack.ReadInt32();
            money = inpack.ReadInt32();
            boss_id = inpack.ReadUInt32();
        
        }

        public byte[] GetBuffer()
        {
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(InternalPacket.HEAD);
            outpack.WriteUInt16(mParam);
            outpack.WriteByte(ret);
            outpack.WriteInt32(play_id);
            outpack.WriteInt32(legion_id);
            outpack.WriteLong(money);
            outpack.WriteUInt32(boss_id);
            outpack.WriteBuff(InternalPacket.TAIL);
            return outpack.GetBuffer();
        }
    }

    
   
}
