using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//一些文本定义- 放到这里=
//2015.8.5
namespace GameBase.Config
{
    public class TextDefine
    {
        public const String GoldConfig = "../GlobalConfig.ini";  //全局服务端配置文件
        public const String DBServerSestion = "DBServer";           //数据库服务器ini节点 
        public const String ACCServerSection = "AccServer";         //帐号服务器ini节点
        public const String LogicServerSection = "LogicServer";      //登录服务器ini节点
        public const String GameServerSetion = "GameServer";            //游戏服务器ini节点
        public const String GlobalSection = "Global";                //全局ini节点
        public const String MysqlSection = "Mysql";                 //全局mysqlini节点
        public const String NormalIPKey = "IP";                       //ipkey
        public const string NormalPortKey = "Port";                    //端口
        public const string NormalIP = "0.0.0.0";                  //默认绑定所有ip
        public const int AccServerPort = 8000;                     //帐号服务器默认端口
        public const int LoginServerPort = 8001;                     //登录服务器默认端口
        public const int GameServerPort = 8002;                      //游戏服务器默认端口
        public const int DBServerPort = 1500;                           //数据库服务器默认端口
        public const String EncodeKey = "EncodeKey";                 //加密key
        public const String EncodeKey2 = "EncodeKey2";               //加密key2FilterName
        public const String CONFIG_FILTERNAME = "data/FilterName.txt";              //数据库服务器的名称过滤文件
        public const String CONFIG_FILE_MAP = "data/config/GameMap.csv";            //地图配置文件
        public const String CONFIG_FILE_GENERATOR = "data/config/Generator.csv";    //刷怪配置文件
        public const String CONFIG_FILE_ITEMTYPE = "data/config/Itemtype.csv";      //道具配置文件
        public const String CONFIG_FILE_MONSTER = "data/config/Monster.csv";       //怪物配置文件
        public const String CONFIG_FILE_NPC = "data/config/Npc.csv";       //NPC配置文件
        public const String CONFIG_FILE_GOLBALSCRIPT = "data/config/Script.csv";    //全局脚本文件
        public const String CONFIG_FILE_MAGICTYPE = "data/config/MagicType.csv";    //技能配置文件
        public const String CONFIG_FILE_ATTRIBUTE = "data/config/Attribute.csv";    //角色等级属性配置文件
        public const String CONFIG_FILE_LEVELEXP = "data/config/LevelExp.csv";      //等级经验配置文件
        public const String CONFIG_FILE_DROPITEM = "data/config/DropItem.csv";      //怪物掉宝配置文件
        public const String CONFIG_FILE_MAPGATE = "data/config/MapGate.csv";        //地图传送点
        public const String CONFIG_FILE_TRACK = "data/config/Track.csv";            //连招技能动作
        public const String CONFIG_FILE_EQUIPSTRONG = "data/config/EquipStrong.csv"; //提升魔魂等级配置文件
        public const String CONFIG_FILE_GEM = "data/config/GemInfo.csv";            //宝石配置文件
        public const String CONFIG_FILE_NPCSHOP = "data/config/NpcShop.csv";        //npc商店配置文件
        public const String CONFIG_FILE_ITEMADDITION = "data/config/ItemAddition.csv"; //装备强化属性加成文件
        public const String CONFIG_FILE_EUDEMON = "data/config/Eudemon.csv";        //幻兽配置文件
        public const String CONFIG_FILE_LOOKFACE = "data/config/LookFace.csv";      //头像配置文件
        public const String CONFIG_FILE_HAIR = "data/config/Hair.csv";              //发型配置文件
        public const String CONFIG_FILE_ROBOT = "data/config/Robot.csv";            //机器人配置文件
        public const String CONFIG_FILE_SCRIPTTIMER = "data/config/ScriptTimer.csv";    //脚本定时器配置文件
        public const String CONFIG_FILE_AI = "data/config/Ai.csv";                      //AI配置文件
        public const String CONFIG_EUDEMON_SOUL = "data/config/EudemonSoul.csv";        //幻兽幻化文件
        public const String CONFIG_REGION_FILE = "data/config/Region.csv";              //地图参数配置文件，目前仅仅用于安全区
         
    }

    public class Define
    {
        public const int MAX_VISIBLE_DISTANCE = 15; //怪物 npc的可视距离
        public const int MAX_EUDEMON_PLAY_DISTANCE = 10;//幻兽与宿主玩家的距离，超出此距离会传送到玩家身边
        public const int MAX_PLAY_VISIBLE_DISTANCE = 18; //玩家的可视距离
        public const int MAX_EUDEMON_OTHER_PLAY_DISTANCE = 14;    //幻兽与其他玩家之间的可视距离
        public const uint FIRSTSCRIPTID = 1000;     //创建角色进游戏的执行脚本id


        //---------------npcid------------------------------------------------------
        public const uint GMAESHOPID = 1207;        //魔石商店的npcid
        public const uint LOOKFACEID = 1998;       // 头像npcid
        public const uint HAIRID = 1997;            //发型npcid
        //---------------npcid------------------------------------------------------
        //--------------金币道具id---------------------------------------------------

        public const uint GOLD_ITEM_1 = 1090000;  //几个金币
        public const uint GOLD_ITEM_2 = 1090010;  //一些金币
        public const uint GOLD_ITEM_3 = 1090020;  //许多金币
        public const uint GOLD_ITEM_4 = 1090030;  //一小堆金币
        public const uint GOLD_ITEM_5 = 1090040;  //一大堆金币
        //----------------------------------------------------------------


        public const int MAX_JUEWEICOUNT = 50;  //最高爵位数量
        public const int ALIVE_TIME = 20;       //死亡复活时间,单位[秒]

        public const int MAX_SIZEADD = 2;       // 最大的SizeAdd，用于魔法区域的放大搜索(注意该数据不要太大)
        //-------------PK相关---------begin---------------------------------------------
        public const int PK_PRISON_MAPID = 300;          //监狱地图id
        public const short PRISON_MAP_X = 76;           //监狱x坐标
        public const short PRISON_MAP_Y = 86;           //监狱y坐标
        public const int PKSTATE_TIME = 30  ;     //蓝名持续状态时间,单位[秒]
        public const int PK_DEC_NORMAL = 1;      //普通地图pk值减少值 。 一分钟减少一点
        public const int PK_DEC_PRISON = 3;     //监狱地图pk值减少值 一分钟减少三点
        public const int PK_DEC_TIME = 60;      //pk减少时间 单位[毫秒]


        public const int PK_NAME_WHITE = 0;			//玩家PK状态- 白名
        public const int PK_NAME_BLUE = 1;             //玩家PK状态- 蓝名
        public const int PK_NAME_RED = 2;              // 玩家PK状态- 红名
        public const int PK_NAME_BLACK = 3;		    // 玩家PK状态- 黑名
             //-------------------------end---------------------------------------------
        //-军团称谓相关--begin------------------------------------------------
        public const short LEGION_PLACE_JUNTUANZHANG = 1000; //军团长
        public const short LEGION_PLACE_FUTUANZHANG = 990;      //副团长
        public const short LEGION_PLACE_RONGYUFUTUANZHANG = 980;//荣誉副团长
        public const short LEGION_PLACE_JUNTUANZHANGBANLV = 920;//军团长伴侣
        public const short LEGION_PLACE_YUANBAO = 890;          //元老
        public const short LEGION_PLACE_RONGYUYUANBAO = 880;        //荣誉元老
        public const short LEGION_PLACE_YIYUAN = 850;           //议员
        public const short LEGION_PLACE_RONGYUYIYUAN = 840;     //荣誉议员
        public const short LEGION_PLACE_ZHIHUIGUAN = 690;       //指挥官
        public const short LEGION_PLACE_RONGYUZHIHUIGUAN = 680; //荣誉指挥官
        public const short LEGION_PLACE_FUTUANZHANGBANLV = 620;     //副团长伴侣
        public const short LEGION_PLACE_JUNTUANZHANGBANLVZHUSHOU = 610; //军团长伴侣助手
        public const short LEGION_PLACE_ZHIXINGGUAN = 590;      //执行官
        public const short LEGION_PLACE_YUANBAOBANLV = 520;     //元宝伴侣
        public const short LEGION_PLACE_YUANBAOZHUSHOU = 510;   //元宝助手
        public const short LEGION_PLACE_JINGYINGTUANYUAN = 490; //精英团员
        public const short LEGION_PLACE_ZHIHUIGUANBANLV = 420;  //指挥官伴侣
        public const short LEGION_PLACE_PUTONGTUANYUAN = 200; //普通团员
        //-军团称谓相关--end------------------------------------------------

        //军团称谓标识--begin----------------------------------------------
        public const byte LEGION_JUNTUAN = 1;   //军团
        public const byte LEGION_BANGZHU = 2;   //帮会
        public const byte LEGION_JIAOZHU = 3;   //教会
        public const byte LEGION_HUIZHANG = 4;  //行会.
        //军团称谓标识--end----------------------------------------------

        //角色动作编号--2015.10.16---------begin--------------------------
        public const uint _ACTION_STANDBY = 100;        //站立
        public const uint _ACTION_EXCITEMENT = 150;     //高兴
        public const uint _ACTION_ANGRY = 60;           //生气
        public const uint _ACTION_SADNESS = 170;        //悲哀
        public const uint _ACTION_CHEER = 180;          //欢呼
        public const uint _ACTION_SAYHELLO = 190;       //招呼
        public const uint _ACTION_SHOUT = 194;          //叫喊
        public const uint _ACTION_FLOWERS = 200;        //献花
        public const uint _ACTION_PRAY = 202;           //祈求
        public const uint _ACTION_GENUFLECT = 210;      //行礼
        public const uint _ACTION_APPLAUSE = 220;       //鼓掌
        public const uint _ACTION_CHAT = 230;           //交谈
        public const uint _ACTION_AGREE = 232;          //同意
        public const uint _ACTION_REFUSE = 234;         //否定
        public const uint _ACTION_DOUBT = 238;          //疑问
        public const uint _ACTION_FLY_KISS = 240;       //飞吻
        public const uint _ACTION_LAUGH = 242;          //嘲笑
        public const uint _ACTION_PROVOKE = 244;        //挑衅
        public const uint _ACTION_FIGHT = 248;          //开火
        public const uint _ACTION_SIT_DOWN = 254;       //坐下
        public const uint _ACTION_STAND_UP = 256;       //站起
        public const uint _ACTION_SQUAT_DOWN = 260;     //蹲下
        public const uint _ACTION_LIE_DOWN = 270;       //躺下
        public const uint _ACTION_BEFORE_LYING = 272;   //前躺
        public const uint _ACTION_LEFT_LYING = 274;     //左躺
        public const uint _ACTION_RIGHT_LYING = 276;    //右躺
        public const uint _ACTION_DEFENSE = 340;        //防御
        public const uint _ACTION_ATTACK = 350;         //攻击
        public const uint _ACTION_JUMP = 361;           //跳跃
        public const uint _ACTION_WIND = 926;           //防风
        //互动类动作
        public const uint _ACTION_INTERACT_SWEET_KISS = 100;        //甜蜜之吻
        public const uint _ACTION_INTERACT_THE_KISS = 100;          //世纪之吻
        public const uint _ACTION_INTERACT_WILD_KISS = 100;         //狂野之吻
        public const uint _ACTION_INTERACT_PURE_KISS = 100;         //清纯之吻
        public const uint _ACTION_INTERACT_HAND = 100;              //牵手
        public const uint _ACTION_INTERACT_DOUBLE_HORSE = 100;      //双人骑宠
        //角色动作编号------------------end--------------------------

        //-----------------------怪物类型--------------------------------
        public const int MONSTER_TYPE_PASSIVE = 0;       //被动怪
        public const int MONSTER_TYPE_ACTIVE = 1;        //主动怪
        //-------------------------------------------------------

        //-------------------AI类型---------------------------------
        public const int AI_TYPE_MELEE = 1;         //近战AI- 被动
        public const int AI_TYPE_MELEEEX = 2;       //近战AI- 主动


        //---------------------------------------------------------
        //---------------------PK模式------------------------------
        public const byte PK_MODE_FREE = 0;  //自由pk模式
        public const byte PK_MODE_SAFE = 1;  //安全PK模式
        public const byte PK_MODE_TEAM = 2;//组队pk模式
        public const byte PK_MODE_GUARD = 3; //捕杀pk模式
        public const byte PK_MODE_LEGEND = 4; //军团pk模式
        public const byte PK_MODE_FREND = 5;//同盟pk模式
        //---------------------------------------------------------
        //---------------------技能目标------------------------------
        public const byte MAGIC_TARGET_COMBO = 8; //被动技能
        public const byte MAGIC_TARGET_XP = 4;      //XP技能
        public const int  XP_MULTIPLE = 10;          //XP技能伤害倍数
        //---------------------------------------------------------
   
        //---------------------XP技能相关------------------------------
        public const int XP_MAX_USER = 100;			// 最大XP值
        public const int XP_ADD_SECS = 30;			// 每隔30秒增加一次XP[秒]
        public const int XP_ADD_VALUE = 15;			// 每次增加15点XP
        public const int XP_MAX_FULL_SECS = 60;			// XP满最多持续的时间[秒]
        public const int XP_DROP_FULL_SECS = 30;        //xp爆气后没点技能的下降时间[秒]
        //---------------------------------------------------------
        //-------------------sp体力值相关----------------------------------
        public const int SP_ADD_SECS = 5;           //没隔3秒增加一次体力
        public const int SP_ADD_VALUE = 10;         //每次增加10点体力
        //---------------------------------------------------------


        //////////////////////////////////////////////////////////////////////
        // 外部状态，需要通过MsgPlayer通知其它玩家的状态。与客户端同步
        public const ulong KEEPEFFECT_NORMAL = 0x00000000; 	// 无特殊状态

        public const ulong KEEPEFFECT_TEAMLEADER = 0x00000001;    // 队长
        public const ulong KEEPEFFECT_DIE = 0x00000002;	// 死亡
        public const ulong KEEPEFFECT_GHOST = 0x00000004;    // 灵魂状态
        public const ulong KEEPEFFECT_DISAPPEARING = 0x00000008;    // 尸体消失状态
        public const ulong KEEPEFFECT_CRIME = 0x00000010;	// 犯罪 // 闪蓝色	
        public const ulong KEEPEFFECT_RED = 0x00000020;	// 红名
        public const ulong KEEPEFFECT_DEEPRED = 0x00000040;	// 黑名
        public const ulong KEEPEFFECT_SYNCRIME = 0x00000080;	// 帮派犯罪
        public const ulong KEEPEFFECT_POISON = 0x00000100;    // 中毒
        public const ulong KEEPEFFECT_HIDDEN = 0x00000200;    // 隐身
        public const ulong KEEPEFFECT_FREEZE = 0x00000400;    // 冰冻
        public const ulong KEEPEFFECT_SUPERSOLDIER = 0x00000800;    // 无双
        public const ulong KEEPEFFECT_LURKER = 0x00001000;    // 潜伏 潜行
      
        public const ulong KEEPEFFECT_FLY = 0x00800000;         //飞行
        public const ulong KEEPEFFECT_DEFENCE1 = 0x00002000;    // 防御提高1
        public const ulong KEEPEFFECT_DEFENCE2 = 0x00004000;    // 防御提高2
        public const ulong KEEPEFFECT_DEFENCE3 = 0x00008000;    // 防御提高3
        public const ulong KEEPEFFECT_ATTACK = 0x00010000;    // 攻击提升
        public const ulong KEEPEFFECT_ATKSPEED = 0x00020000;    // 攻击速度提高
        public const ulong KEEPEFFECT_MAGICDAMAGE = 0x00040000;    // 魔法伤害提高
        public const ulong KEEPEFFECT_MAGICDEFENCE = 0x00080000;    // 魔法防御提高
        public const ulong KEEPEFFECT_REFLECT = 0x00100000;	// 攻击反射
        public const ulong KEEPEFFECT_REFLECTMAGIC = 0x00200000;    // 魔法反射
        public const ulong KEEPEFFECT_SLOWDOWN1 = 0x00400000;    // 减速状态 50%
        public const ulong KEEPEFFECT_SLOWDOWN2 = 0x00800000;	// 减速状态 // 血少于一半时开始减速 // 50%
        public const ulong KEEPEFFECT_TEAM_HEALTH = 0x01000000;	// 医疗结界状态
        public const ulong KEEPEFFECT_TEAM_ATTACK = 0x02000000;	// 攻击结界状态
        public const ulong KEEPEFFECT_TEAM_DEFENCE = 0x04000000;	// 护体结界状态
        public const ulong KEEPEFFECT_TEAM_SPEED = 0x08000000;	// 速度结界状态
        public const ulong KEEPEFFECT_TEAM_EXP = 0x10000000;	// 修炼结界状态
        public const ulong KEEPEFFECT_TEAM_SPIRIT = 0x20000000;	// 心灵结界状态
        public const ulong KEEPEFFECT_TEAM_CLEAN = 0x40000000;	// 净化结界状态
        public const ulong KEEPEFFECT_SMOKE = 0x80000000;			// 烟雾效果
        public const ulong KEEPEFFECT_DARKNESS = 0x0000000100000000;	// 黑暗效果
        public const ulong KEEPEFFECT_PALSY = 0x0000000200000000;	// 麻痹效果
        public const ulong KEEPEFFECT_MAXLIFE = 0x0000000400000000;	// 最大生命增加/减少
        public const ulong KEEPEFFECT_MAXENERGY = 0x0000000800000000;	// 最大体力增加/减少
        public const ulong KEEPEFFECT_ADD_EXP = 0x0000001000000000;	// 战斗经验增加
        public const ulong KEEPEFFECT_ATTRACT_MONSTER = 0x0000002000000000;	// 吸引怪物
        public const ulong KEEPEFFECT_XPFULL = 0x0000004000000000;	// XP满
        public const ulong KEEPEFFECT_HEILONGWU = 0x0000200000000000;   //黑龙舞-特效
        public const ulong KEEPEFFECT_ADJUST_DODGE = 0x0000008000000000;	// 调节总的躲避值
        public const ulong KEEPEFFECT_ADJUST_XP = 0x0000010000000000;	// 调节每次增加XP值
        public const ulong KEEPEFFECT_ADJUST_DROPMONEY = 0x0000020000000000;	// 调节怪物每次掉钱
        public const ulong KEEPEFFECT_PK_PROTECT = 0x0000040000000000;	// pk保护状态
        public const ulong KEEPEFFECT_PELT = 0x0000080000000000;	// 疾行状态
        public const ulong KEEPEFFECT_ADJUST_EXP = 0x0000100000000000;	// 战斗获得经验调整
        public const ulong KEEPEFFECT_HEAL = 0x0000200000000000;	// 治愈状态
        public const ulong KEEPEFFECT_FAINT = 0x0000400000000000;	// 晕
        public const ulong KEEPEFFECT_TRUCULENCE = 0x0000800000000000;	// 野蛮
        public const ulong KEEPEFFECT_DAMAGE = 0x0001000000000000;	// 调整受到的伤害
        public const ulong KEEPEFFECT_ATKER_DAMAGE = 0x0002000000000000;	// 调整对目标造成的伤害
        public const ulong KEEPEFFECT_CONFUSION = 0x0004000000000000;	// 混乱
        public const ulong KEEPEFFECT_FRENZY = 0x0008000000000000;	// 狂暴
        public const ulong KEEPEFFECT_EXTRA_POWER = 0x0010000000000000;	// 神力
        public const ulong KEEPEFFECT_TRANSFER_SHIELD = 0x0020000000000000;	// 护盾
        public const ulong KEEPEFFECT_SORB_REFLECT = 0x0040000000000000;	// 吸收反射
        public const ulong KEEPEFFECT_FRENZY2 = 0X0080000000000000;	// 另一种狂暴状态

        public const ulong KEEPEFFECT_HUASHENWULING = 0x00040000;   //化身巫灵
        public const ulong KEEPEFFECT_ZHAOHUANWUHUAN = 0x0000800000000000; //召唤巫环
        public const ulong KEEPEFFECT_MIXINSHU = 0x0000000000000020;    //迷心术
        public const ulong KEEPEFFECT_XUEXI = 0x0000000000000200;                      //血袭
        //---------------------------------------------------------------------------------------

        //-----------------——-------------------------------------

        public const int MAX_CALL_EUDEMON = 3;          //最大宠物召唤数量


        public const int STATUS_MOLONGSHOUHU_TIME = 1800; ////魔龙守护BUFF持续时间

        public const uint GUARDKNIGHTID = 10795;        //暗黑龙骑守护骑士怪物id
        public const int GUARDKNIGHT_TIME = 120;        //骑士团守护特效时间
        public const int GUARDKNIGHT_EFFID = 6064;  //骑士团守护特效ID
        public const int JIANGLINGZHOUYU = 5678;            //降灵咒雨 特效id
        public const int JIANGLINGZHOUYU_DIS = 7;      //降灵咒雨的攻击范围
        public const int JIANGLINGZHOUYU_MAGICID = 6030;//降灵咒雨技能id

        public const int FIGHT_KNIGHT_MOVE_TIME = 700;  //冲锋骑士移动的速度- [毫秒]
        public const int FIGHTKNIGHTID = 10836;     //暗黑龙骑黑暗骑士怪物ID
        public const int FIGHTKNIGHT_TIME = 10;     //黑暗骑士冲锋持续时间
        public const int FIGHTKNIGHT_RUN_SPEED = 3; //黑暗骑士冲锋速度
        public const int FIGHTKNIGHT_AMOUNT = 4;        //黑暗骑士的数量
        public const int STEALTH_TIME = 60;         //潜行时间[秒]
        public const int HIDEDEM_TIME = 30;         //隐身时间

        public const int LIUXINGYUNHUO_TIME = 5;    //凝聚流行陨火时间[秒]
        public const int LIUXINGYUNHUO_MAX_COUNT = 7;   //最大凝聚流星陨火数量
        public const uint YUANSUZHAOHUAN_MOUNTID = 1072540;    //元素召唤的坐骑ID
        public const uint GULINGQIYUE_MOUNTID = 1072240; //骨灵契约的坐骑ID
        public const int HEILONGWU_ATTACK_TIME = 1; //黑龙舞攻击时间[秒]
        public const int LIEHUNSHAN_ATTACK_COUNT = 4;   //裂魂闪攻击次数
        public const int JIANGLINGZHOUYU_TIME = 5;      //降灵咒雨 持续时间[秒]

        public const uint ANSHAXIELONG_MONSTER_ID = 1430;  //暗杀邪龙ID
        public const uint WANGNIANWULONG_MONSTER_ID = 1431; //亡念巫灵ID
        public const uint MINGGUOSHENGNV_MONSTER_ID = 1429; //冥国圣女怪物ID
        public const uint SHENYUANELING_MONSTER_ID = 1434;  //深渊恶灵怪物ID
        public const uint DIYUXIEFU_MONSTER_ID = 1433;       //地狱邪蝠怪物id
        public const uint SHIHUNWULING_MONSTER_ID = 1432;              //蚀魂巫灵怪物id
        public const uint MONSTER_ACTIVEATTACK_TIME = 5; //主动攻击怪物的搜寻目标时间，不要设太短。。[秒]

        public const int MIXINSHU_TIME = 120;    //迷心术持续时间
        public const int XUEXI_TIME = 60;      //血袭持续时间

        //---------幻兽类型
        public const int EUDEMON_TYPE_WARRIOR = 1; //战士
        public const int EUDEMON_TYPE_MAGE = 2;     //法师
        public const int EUDEMON_TYPE_MAGE_RID = 4; //法师+骑宠
        public const int EUDEMON_TYPE_WARRIOR_RIG = 5;  //战士+骑宠

        public const int EUDEMON_EVOLUTION_ONE = 20;    //第一次进化需求多少级
        public const int EUDEMON_EVOLUTION_COUNT = 2;   //最多进化次数
        public const int EUDEMON_EVOLUTION_TWO = 40;        //第二次进化需求多少级
        public const int EUDEMON_MAX_BATTLE = 9;        //幻兽最多高于玩家多少级可出征
        public const int EUDEMON_NORMAL_QUALITY = 1000; //幻兽第一次进化品质- 极品十星


        public const float ROLE_MOVE_SPEED = 250f;      //角色移动速度
        public const float ROLE_RIG_MOVE_SPEED = 200f;  //角色骑乘移动速度
        public const float ROLE_ATTACK_SPEED = 1000f;  //角色普通攻击速度[毫秒]

        public const int SAVEROLE_TIME = 600;   //定时保存玩家数据,十分钟[秒]
    

     
        public const uint EXP_BALL_ID = 500001;     //经验值球
        public const uint SUPER_EXP_BALL_ID = 830000;   //特制经验值球
        public const int EXP_BALL_MAX = 1000000;       //经验球满的最大经验 
        public const int EXPBALL_EUDEMON_MAXLEVEL = 3;  //使用经验球 幻兽的等级最大超过角色多少级,例：角色10级别，使用经验球后 角色等级+3=13

        public const int WORDPIGEONSENDITME = 60;  //魔法飞鸽每隔一条发送时间 [秒]

        public const int PTICH_MAX_COUNT = 100; //摊位最大数量
        public const int PTICH_SELL_MAX_COUNT = 18;//摊位最大出售道具数量
        public const int PTICH_START_ID = 9101; //摊位起始ID
        public const uint ITEM_DIANJIANGYAOSHUI_ID = 831002; //电浆药水数据库ID
        public const int EUDEMON_LEI_ADD_QUALITY = 100;    //加一星
        public const int MINGRENTANG_OBJ_START_ID = 11000;  //把名人堂的雕像视为NPC对象//名人堂的NPC对象起始ID 
        public const uint ITEM_SHUGUANGZHANHUN_ID = 1110010;//曙光战魂道具ID
       public const uint ITEM_DILONGZHILEI_ID = 1110110; //帝龙之泪道具ID
       public const uint ITEM_SHENGYAOFUWEN_ID = 1110210; //圣耀符文道具ID

       public const int MAX_GOLD = 2000000000;      //最高二十亿金币
       public const int MAX_GAMEGOLD = 2000000000;  //最高二十亿魔石

       public const int MAX_FUBEN_CLONE_COUNT = 100;//每张地图只提供100格副本克隆，
       public const int LAST_FUBEN_NULL_DELETE_TIME = 60000;    //副本没人的时候 超过多久时间删除副本
    }

    //对象类型
    public class OBJECTTYPE
    {
        public const byte NORMAL = 0; 
        public const byte NPC = 1;          //npc
        public const byte PLAYER = 2;       //玩家
        public const byte MONSTER = 3;      //怪物
        public const byte EUDEMON = 4;      //幻兽
        public const byte DROPITEM = 5;     //地图物品对象
        public const byte ROBOT = 6;        //机器人
        public const byte GUARDKNIGHT = 7;  //暗黑龙骑守护骑士
        public const byte EFFECT = 8;       //地面特效
        public const byte CALLOBJECT = 9;   //亡灵巫师召唤的对象- 与怪物一致
        public const byte PTICH = 10;       //摊位
    }

    //职业
    public class JOB
    {
        public const byte MAGE = 10;        //法师
        public const byte WARRIOR = 20;     //战士
        public const byte POWER = 30;       //异能者
        public const byte BLOODCLAN = 50;   //血族
        public const byte UNDEAD_MAGE = 60; //亡灵巫师
        public const byte DRAGONRIDE = 70;  //暗黑龙骑

    }

    //性别
    public class Sex
    {
        public const byte NORMAL = 0;    //人妖？ (/ □ \)
        public const byte MAN = 1;      //男
        public const byte WOMAN = 2;    //女
    }

    public enum BROADCASTMSGTYPE
    {
        LEFT = 1,//右上角
        CHAT = 2,//聊天系统频道
        SCREEN = 3,//屏幕中间
    }
}
