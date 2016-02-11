using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//封包协议号
//2015.8.5
namespace GameBase.Network
{
    public class PacketProtoco
    {
        public const ushort S_NOTICE = 1004;    //游戏公告
        public const ushort S_LEFTNOTICE = 1004;        //左上角公告
        public const ushort S_CHATNOTICE = 1004;        //聊天框系统公告
        public const ushort S_SELFROLEINFO = 1006;  //角色信息
        public const ushort S_OPERATEEQUIP = 1009; //装备操作信息
        public const ushort S_CLEARITEM = 1009;     //清除包裹道具
        public const ushort S_ITEMINFO = 1008;     //道具信息
        public const ushort S_MAPINFO = 1010;       //地图信息
        public const ushort S_CLEARMONSTER = 1010;  //清除地图怪物
        public const ushort S_SCROOLRANDOM = 1010; // 随机卷
        public const ushort S_LOCK = 1010;          //锁定
        public const ushort S_EUDEMONTAG = 1010;        //幻兽标记
        public const ushort S_SPINFO = 1011;        //sp更新
        public const ushort S_ROLEINFO = 1014;      //刷新玩家信息
        public const ushort S_COMBO = 1015;         //连击技能
        public const ushort S_HOTKEY = 1015;        //热键
        public const ushort S_LEGION_NAME = 1015;   //军团名称
        public const ushort S_UPDATESP = 1017;      //更新sp值
        public const ushort S_UPXP = 1017;          //更新xp值
        public const ushort S_USERATTRIBUTE = 1017;      //角色属性
        public const ushort S_FRIENDINFO = 1019;        //好友信息
        public const ushort S_TRAD = 1056;              //交易
        public const ushort S_ATTACK = 1022;        //攻击
        public const ushort S_GAMESERVERINFO = 1057; //发送key与游戏服务器信息
        public const ushort S_MAGICINFO = 1103;     //技能信息
        public const ushort S_SELFLEGIONINFO = 1106;    //自身行会信息
        public const ushort S_PTICH_ITEMINFO = 1108;    //摊位出售道具信息
        public const ushort S_KEY = 1059;           //发送key
        public const ushort S_DROPITEM = 1101;      //发送掉落道具
        public const ushort S_STRONGINFO = 1102;    //发送仓库信息
        public const ushort S_MAGICATTACK = 1105;       //魔法攻击
        public const ushort S_EUDEMONBALLTE = 1116;     //幻兽出征-
        public const ushort S_NPCINFO = 2030;       //刷新npc信息
        public const ushort S_NPCREPLY = 2032;      //返回npc信息
        public const ushort S_EQUIPOPERATION = 2036;    //装备操作
        public const ushort S_EUDEMONINFO = 2037;   //幻兽详细信息
        public const ushort S_GUANJUE = 2060;           //爵位操作
        public const ushort S_MONSTERINFO = 2069;   //刷新怪物


        public const ushort C_CREATEROLE = 1001;        //创建角色
        public const ushort C_CHANGEPKMODE = 1010;      //更改pk模式
        public const ushort C_MSGTALK = 1004;           //聊天
        public const ushort C_MSGIEM = 1009;          //道具操作/
        public const ushort C_HOTKEY = 1015;            //热键
        public const ushort C_ADDFRIEND = 1019;     //添加好友
        public const ushort C_ATTACK = 1022;        //攻击
        public const ushort C_GETFRIENDINFO = 1032; //获取好友信息
        public const ushort C_UPDATEKEY = 1052;     //更新密码本数据
        public const ushort C_TRAD = 1056;          //交易
        public const ushort C_LOGINUSER = 1083;     //验证帐号
        public const ushort C_LOGINGAME = 1095;     //登录游戏
        public const ushort C_PICKDROPITEM = 1101;  //拾取道具
        public const ushort C_STRONGPACK = 1102;       //仓库操作
        public const ushort C_QUERYCREATEROLENAME = 1158;//注册名称查询
        public const ushort C_OPENNPC = 2031;       //打开npc
        public const ushort C_NPCREPLY = 2032;
        public const ushort C_EQUIPOPERATION = 2036;    //装备操作
        public const ushort C_GUANJUE = 2060;           //爵位操作
        public const ushort C_DANCING = 1049;           //跳舞

        public const ushort C_MOVE = 3005;          //移动
       


    }
}
