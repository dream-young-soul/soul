using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameBase.Config;
using MapServer;
//游戏结构--
namespace GameStruct
{

    //地图信息
    public class MapInfo
    {
        public uint id; //地图id
        public String name; //地图名称
        public String dmappath; //地图文件路径
        public uint recallid;   //回城地图id
        public ushort recallx;  //回城地图x
        public ushort recally;  //回城地图y
        public bool issnows;   //是否下雪[注意要修改客户端文件，否则也下不起来-。-]
        public MapInfo()
        {
            id = 0;
            name = dmappath = "";
            recallid = recallx = recally = 0;
        }
    }

    public class DropItemClass
    {
        public List<uint> list_itemid;
        public DropItemClass()
        {
            list_itemid = new List<uint>();
        }
    }
    //掉宝
    public class DropItemInfo
    {
        public uint groupid;    //组id
        public List<DropItemClass> listitem; //道具id
        public List<uint> listamount; //最多掉落数量
        public List<uint> listrate;   //概率
        public DropItemInfo()
        {
            groupid = 0;
            listitem = new List<DropItemClass>(); 
            listrate = new List<uint>();
            listamount = new List<uint>();
        }

    }

    public class TrackInfo
    {
        public uint id;
        public uint id_next;
        public byte direction;
        public byte step;
        public byte alt;
        public uint action;
        public int power;
        public int apply_ms;

        public TrackInfo()
        {
            id = id_next = 0;
            direction = step = alt = 0;
            power = apply_ms = 0;
            action = 0;
        }
    }

    public struct MapRegionInfo
    {
        public const int MAPREGIONINFO_TYPE_SAFE = 595849;//安全区
        public uint mapid;  //地图id
        public int type;    //地图类型
        public short bound_x;   //中心点x
        public short bound_y;   //中心点y
        public short bound_cx;  //范围x
        public short bound_cy; //范围y

    }
    //地图传送点
    public class MapGateInfo
    {
        public uint src_mapid;
        public short src_x;
        public short src_y;
        public uint target_mapid;
        public short target_x;
        public short target_y;
        public int dis; //距离差值
        public MapGateInfo()
        {
            src_mapid = 0;
            src_x = 0;
            src_y = 0;
            target_mapid = 0;
            target_x = 0;
            target_y = 0;
            dis = 0;
        }


    }
    //等级经验信息
    public class LevelExp
    {
        public const uint LEVELEXP_ROLE = 0; //角色的升级经验
        public const uint LEVELEXP_EUDEMON = 1; //幻兽的升级经验
        public byte level;
        public ulong exp;
        public LevelExp()
        {
            level = 0;
            exp = 0;
        }
    }
    //角色等级基础信息
    public class BaseAttributeInfo
    {
        public byte lv;
        public int force;       //攻击
        public int dexterity;   //闪避
        public int health;
        public int soul;

        public BaseAttributeInfo()
        {
            lv = 0;
            force = dexterity = health = soul;
        }
        //获取生命值
        public uint GetLife()
        {
            return (uint)(health * 10);
        }
        //获取魔法值
        public uint GetMana()
        {
            return (uint)(soul * 20);
        }

        //获取该等级的魔法攻击
        public uint GetMagicAttack()
        {
            return (uint)(force / 2);
        }
        //获取物理攻击
        public uint GetAttack()
        {
            return (uint)force;
        }
        //获取闪避
        public uint GetDoage()
        {
            return (uint)dexterity;
        }
    }
    //NPC信息
    public class NPCInfo
    {
        public uint id;
        public String name;
        public uint mapid;
        public short x;
        public short y;
        public int lookface;
      
        public String ScriptPath;
        public uint ScriptID;
        public NPCInfo()
        {
            id = 0;
            name = "";
            mapid = 0;
            x = 0;
            y = 0;
         
            ScriptPath = "";
            ScriptID = 0;

        }
    }

    //刷怪信息
    public class GeneratorInfo
    {
        public uint mapid;      //地图id
        public uint bound_x;    //刷怪x坐标
        public uint bound_y;    //刷怪y坐标
        public uint bound_cx;   //刷怪x随机范围
        public uint bound_cy;   //刷怪y随机范围
        public uint amount;     //数量
        public uint time;       //时间,秒为单位
        public uint monsterid;  //怪物id
        public byte dir;        //初始方向
        public GeneratorInfo()
        {
            mapid = bound_x = bound_y = bound_cx = bound_cy = amount = time = monsterid = 0;
            dir = 0;
        }
    }

    //怪物信息
    public class MonsterInfo
    {
        public uint id;                  //怪物id
        public String name;             //怪物名称
        public int ai;               //ai类型
        public uint lookface;           //外观
        public ushort level;            //等级
        public int life;             //生命
        public int mana;             //魔法
        public uint attack_min;          //最小攻击
        public uint attack_max;         //最大攻击
        public uint defense;            //防御
        public ushort dodge;            //躲避
        public ushort range;           //可视范围
        public ushort attack_speed;     //攻击速度
        public ushort move_speed;       //移动速度
        public uint drop_group;         //掉落道具组
        public int eudemon_type;        //幻兽类型
        public uint die_scripte_id;      //死亡触发脚本ID

        public MonsterInfo()
        {
            id = 0;
            name = "";
            ai = 0;
            lookface = 0;
            level = 0;
            life = mana = 0;
            attack_max = attack_min = defense = 0;
            dodge = range = attack_speed = move_speed = 0;
            drop_group = 0;
            eudemon_type = 0;
            die_scripte_id = 0;
        }
    }

    //道具信息
    public class ItemTypeInfo
    {
        public uint id;         //道具id
        public String name;     //道具名称
        public byte req_profession;  //所需职业
        public byte req_level;      //所需等级
        public byte req_sex;        //所需性别
        public uint attack_min;     //最小物理攻击
        public uint attack_max;     //最大物理攻击
        public uint defense;        //物理防御
        public uint magic_defense;    //魔法防御
        public uint magic_attack_min;   //最小魔法攻击
        public uint magic_attck_max;        //最大魔法攻击
        public uint dodge;              //躲避
        public uint hitrate;        //准确
        public ushort amount;         //当前叠加数量
        public ushort amount_limit;     //最大叠加数量
        public uint actionid;       //脚本id
        public uint monster_type;    //幻兽蛋对应怪物id
        public int price;           //价格
        public String info;             //说明
        public ItemTypeInfo()
        {
            id = 0;
            name = "";
            req_profession = 0;
            req_level = req_sex = 0;
            attack_min = attack_max = 0;
            defense = magic_defense = 0;
            magic_attack_min = magic_attck_max = 0;
            dodge = hitrate = 0;
            amount = 0;
            info = "";
            actionid = 0;
            price = 0;
            monster_type = 0;
        }
    }
    public class Size
    {
        public int nWidth;
        public int nHeight;
        public Size()
        {
            nWidth = nHeight = 0;
        }
    }

    //技能基础信息
    public class MagicTypeInfo
    {
        public const byte MAGICSORT_ATTACK = 1; //普通攻击
        public const byte MAGICSORT_RECRUIT = 2;
        public const byte MAGICSORT_CROSS = 3;
        public const byte MAGICSORT_FAN = 4;                    //扇形攻击
        public const byte MAGICSORT_BOMB = 5;    //范围攻击
        public const byte MAGICSORT_ATTACHSTATUS = 6;    //引诱技能
        public const byte MAGICSORT_DETACHSTATUS = 7;
        public const byte MAGICSORT_SQUARE = 8;
        public const byte MAGICSORT_JUMPATTACK = 9;		// move, a-lock
        public const byte MAGICSORT_RANDOMTRANS = 10;			// move, a-lock
        public const byte MAGICSORT_DISPATCHXP = 11;
        public const byte MAGICSORT_COLLIDE = 12;			// move, a-lock & b-synchro
        public const byte MAGICSORT_SERIALCUT = 13;			// auto active only.
        public const byte MAGICSORT_LINE = 14;			// support auto active(random). 直线型攻击
        public const byte MAGICSORT_ATKRANGE = 15;			// auto active only, forever active.
        public const byte MAGICSORT_ATKSTATUS = 16;		// support auto active, random active.
        public const byte MAGICSORT_CALLTEAMMEMBER = 17;
        public const byte MAGICSORT_RECORDTRANSSPELL = 18;
        public const byte MAGICSORT_TRANSFORM = 19;
        public const byte MAGICSORT_ADDMANA = 20;			// support self target only.
        public const byte MAGICSORT_LAYTRAP = 21;
        public const byte MAGICSORT_DANCE = 22;			// 跳舞(only use for client)
        public const byte MAGICSORT_CALLPET = 23;			// 召唤兽
        public const byte MAGICSORT_VAMPIRE = 24;			// 吸血，power is percent award. use for call pet
        public const byte MAGICSORT_INSTEAD = 25;			// 替身. use for call pet
        public const byte MAGICSORT_DECLIFE = 26;			// 扣血(当前血的比例)
        public const byte MAGICSORT_GROUNDSTING = 27;		// 地刺
        public const byte MAGICSORT_REBORN = 28;			// 复活 -- zlong 2004.5.14
        public const byte MAGICSORT_TEAM_MAGIC = 29;			// 界结魔法—— 与MAGICSORT_ATTACHSTATUS相同处理，
        //				这里独立分类只是为了方便客户端识别
        public const byte MAGICSORT_BOMB_LOCKALL = 30;			// 与MAGICSORT_BOMB处理相同，只是锁定全部目标
        public const byte MAGICSORT_SORB_SOUL = 31;			// 吸魂魔法
        public const byte MAGICSORT_STEAL = 32;			// 偷盗，随机从目标身上偷取power个物品
        public const byte MAGICSORT_LINE_PENETRABLE = 33;			// 攻击者轨迹可以穿人的线性攻击
        public const byte MAGICSORT_DRAGON_MOLONGSHOUHU = 40;       //暗黑龙骑 魔龙守护- 
        public const byte MAGICSORT_POINTBOMB = 41;                 //鼠标指定范围技能 例:炽链陨灭
        public const byte MAGICSORT_DRAGON_QISHITUANSHOUHU = 42;    // 暗黑龙骑， 骑士团守护
        public const byte MAGICSORT_DRAGON_QISHITUANCHONGFENG = 43; //暗黑龙骑 骑士团冲锋
        public const byte MAGICSORT_JUMP_ATTACK = 81;   //跳斩单体攻击[ 战士 凌空神击]
        public const byte MAGICSORT_JUMPBOMB = 82;      //跳斩范围攻击-【战士 神之审判】
        public const byte MAGICSORT_STEALTH = 83;       //法师 潜行
        public const byte MAGICSORT_HIDEDEN = 84;            //法师 隐身
        public const byte MAGICSORT_YUANSUZHANGKONG = 85;   //法师 元素掌控
        public const byte MAGICSORT_LIUXINGYUNHUO = 86;     //法师 流星陨火
        public const byte MAGICSORT_JUYANSHENGDUN = 87;     //法师 巨岩圣盾
        public const byte MAGICSORT_YUANSUZHAOHUAN = 88;        //法师 元素召唤
        public const byte MAGICSORT_ZHAOHUANWUHUAN = 90;        //亡灵巫师 召唤巫环
        public const byte MAGICSORT_JIANGLINGZHOUYU =  92;       //亡灵巫师 降灵咒雨
        public const byte MAGICSORT_ANSHAXIELONG = 93;          //亡灵巫师 暗沙邪龙
        public const byte MAGICSORT_MINGGUOSHENGNV = 94;        //亡灵巫师 冥国圣女
        public const byte MAGICSORT_WANGNIANWULING = 95;        //亡灵巫师 亡念巫灵
        public const byte MAGICSORT_SHENYUANELING = 96;         //亡灵巫师 深渊恶灵
        public const byte MAGICSORT_DIYUXIEFU = 97;         //亡灵巫师 地狱邪蝠
        public const byte MAGICSORT_SHIHUNWULING = 98;      //亡灵巫师 蚀魂巫灵
        public const byte MAGICSORT_GULINGQIYUE = 99;       //亡灵巫师 骨灵契约
        public const byte MAGICSORT_MIXINSHU = 100;     //血族 迷心术
        public const byte MAGICSORT_SINGLE_DANCING = 101;   //单人舞
        public const byte MAGICSORT_DOUBLE_DANCING = 102;       //双人舞
        public const uint SILIANZHAN = 1005;     //四连斩

        public const uint FEITIANZHAN = 1007;   //飞天斩
        public const uint LIULIANZHAN = 1009;   //六连斩
        public const uint FEITIANLIANZHAN = 1010; //飞天连斩
        public const uint LONGHUNFENGBAO = 1021;        //龙魂风暴
        public const uint LEITINGWANJUN = 3021;     //雷霆万钧
        public const uint LONGQIANGLIEHUN = 5212;   //龙枪猎魂
        public const uint LONGQIANGZANGHUN = 5213;  //龙枪葬魂
        public const uint MOLONGSHOUHU = 5225;      //魔龙守护
        public const uint LONGQIANGSUIHUN = 5242;    //龙枪碎魂
        public const uint YANHUNQIANG_LIEDI = 5214; //焰魂枪·裂地
        public const uint YANHUNQIANG_LIUYAN = 5217;    //焰魂枪·流焰
        public const uint LIUXINGYUNHUO = 5302;         //流星陨火
        public const uint HEILONGWU = 6008;             //黑龙舞
        public const uint LIEHUNSHAN = 6009;            //裂魂闪
        public const uint WUNUSHIHUN = 6017;            //巫怒噬魂
        public const uint ZHENSHIDAJI = 7003;        //血族 真视打击
        public const uint XUEXI = 7007;                 //血族 血袭
        public const uint SHUNYINGJI = 7009;            //血族 瞬影击
        public const uint XUEYINGLUNHUI = 7011;     //血族 血影轮回
        public const uint XUEYINGQIANHUAN = 7010;   //血族 血影千幻
        public const uint XUEYINGXINGMANG = 7016;   //血族 血影星芒
        public const uint XUEYUXUANWO = 7014;   //血族 血雨旋涡
        public uint id;
        public uint typeid;     //技能id
        public byte sort;   //技能类型
        public String name; //技能名称
        public uint crime;
        public uint ground;
        public uint multi;
        public uint target;
        public byte level;
        public uint use_mp;
        public uint use_potential;
        public uint power;
        public uint intone_speed;
        public uint percent;
        public uint step_secs;
        public uint range;
        public uint distance;
        public uint status_chance;
        public uint status;
        public uint need_prof;
        public uint need_exp;
        public uint need_level;     //升到下一级所需的等级
        public uint need_gemtype;
        public uint use_xp;
        public uint weapon_subtype;
        public uint active_times;
        public uint auto_active;
        public uint floor_attr;
        public uint auto_learn;
        public uint learn_level;
        public uint drop_weapon;
        public uint use_ep;
        public uint weapon_hit;
        public uint use_item;
        public uint next_magic;
        public uint delay_ms;
        public uint use_item_num;
        public uint width;
        public uint durability;
        public uint apply_ms;
        public uint track_id;
        public uint track_id2;
        public uint auto_learn_prob;
        public uint group_type;
        public uint group_member1_pos;
        public uint group_member2_pos;
        public uint group_member3_pos;
        public uint magic1;
        public uint magic2;
        public uint magic3;
        public uint magic4;
        public uint attack_combine;
        public uint flag;
        public MagicTypeInfo()
        {
            id = 0;
            typeid = 0;     //技能id
            sort = 0;   //技能类型
            name = ""; //技能名称
            crime = 0;
            ground = 0;
            multi = 0;
            target = 0;
            level = 0;
            use_mp = 0;
            use_potential = 0;
            power = 0;
            intone_speed = 0;
            percent = 0;
            step_secs = 0;
            range = 0;
            distance = 0;
            status_chance = 0;
            status = 0;
            need_prof = 0;
            need_exp = 0;
            need_level = 0;
            need_gemtype = 0;
            use_xp = 0;
            weapon_subtype = 0;
            active_times = 0;
            auto_active = 0;
            floor_attr = 0;
            auto_learn = 0;
            learn_level = 0;
            drop_weapon = 0;
            use_ep = 0;
            weapon_hit = 0;
            use_item = 0;
            next_magic = 0;
            delay_ms = 0;
            use_item_num = 0;
            width = 0;
            durability = 0;
            apply_ms = 0;
            track_id = 0;
            track_id2 = 0;
            auto_learn_prob = 0;
            group_type = 0;
            group_member1_pos = 0;
            group_member2_pos = 0;
            group_member3_pos = 0;
            magic1 = 0;
            magic2 = 0;
            magic3 = 0;
            magic4 = 0;
            attack_combine = 0;
            flag = 0;
        }
    }


    public class Point
    {
        public short x;
        public short y;
        public Point()
        {
            x = y = 0;
        }
        //检测可视距离
        public bool CheckVisualDistance(short xx, short yy, int distance = Define.MAX_VISIBLE_DISTANCE)
        {
            int dis_x = Math.Abs(xx - x);
            int dis_y = Math.Abs(yy - y);
            if (dis_x <= distance &&
                dis_y <= distance)
            {
                return true;
            }
            return false;
        }



        public bool CheckFanDistance(Point pos, Point magicPos, int distance = Define.MAX_VISIBLE_DISTANCE)
        {

            if (!CheckVisualDistance(pos.x, pos.y, distance))
                return false;
            byte dir = DIR.GetDirByPos(x, y, magicPos.x, magicPos.y);
            byte[] dirArr = new byte[3];
            switch (dir)
            {
                case DIR.LEFT_DOWN: //左下
                    {
                        dirArr[0] = DIR.DOWN;
                        dirArr[1] = DIR.LEFT_DOWN;
                        dirArr[2] = DIR.LEFT;
                        break;
                    }
                case DIR.LEFT:      //左
                    {
                        dirArr[0] = DIR.LEFT_DOWN;
                        dirArr[1] = DIR.LEFT;
                        dirArr[2] = DIR.LEFT_UP;
                        break;
                    }
                case DIR.LEFT_UP:   //左上
                    {
                        dirArr[0] = DIR.LEFT;
                        dirArr[1] = DIR.LEFT_UP;
                        dirArr[2] = DIR.UP;
                        break;
                    }
                case DIR.UP:    //上
                    {
                        dirArr[0] = DIR.LEFT_UP;
                        dirArr[1] = DIR.UP;
                        dirArr[2] = DIR.RIGHT_UP;
                        break;
                    }
                case DIR.RIGHT_UP:  //右上
                    {
                        dirArr[0] = DIR.UP;
                        dirArr[1] = DIR.RIGHT_UP;
                        dirArr[2] = DIR.RIGHT;
                        break;
                    }
                case DIR.RIGHT: //右
                    {
                        dirArr[0] = DIR.RIGHT_UP;
                        dirArr[1] = DIR.RIGHT;
                        dirArr[2] = DIR.RIGHT_DOWN;
                        break;
                    }
                case DIR.RIGHT_DOWN:    //右下
                    {
                        dirArr[0] = DIR.RIGHT;
                        dirArr[1] = DIR.RIGHT_DOWN;
                        dirArr[2] = DIR.DOWN;
                        break;
                    }
                case DIR.DOWN: //下
                    {
                        dirArr[0] = DIR.RIGHT_DOWN;
                        dirArr[1] = DIR.DOWN;
                        dirArr[2] = DIR.LEFT_DOWN;
                        break;
                    }
            }
            byte toDir = DIR.GetDirByPos(x, y, pos.x, pos.y);
            for (int i = 0; i < dirArr.Length; i++)
            {
                if (dirArr[i] == toDir)
                {
                    return true;
                }
            }
            return false;
        }
        //检测扇形距离
        //pos = 未知-
        //posSource 目标对象坐标
        //nRange 范围
        //nWidth 技能宽度
        //posCenter 技能施放坐标
        //public bool CheckFanDistance(Point pos, Point posSource, int nRange, int nWidth, Point posCenter)
        //{
        //    //CHECKF(nWidth > 0 && nWidth < 360);

        //    if (posCenter.x == x && posCenter.y == y)
        //        return false;
        //    if (pos.x == posSource.x && pos.y == posSource.y)
        //        return false;

        //    if (!CheckVisualDistance(posSource.x, posSource.y, nRange))
        //        return false;

        //    const float PI =3.1415926535f;
        //    float fRadianDelta = (PI * nWidth / 180) / 2;
        //    float fCenterLine = GetRadian(x, y, posCenter.x, posCenter.y);
        //    float fTargetLine = GetRadian(posSource.x, posSource.y, pos.x, pos.y);
        //    float fDelta = Math.Abs(fCenterLine - fTargetLine);
        //    if (fDelta <= fRadianDelta || fDelta >= 2 * PI - fRadianDelta)
        //        return true;

        //    return false;
        //}
        //public static float  GetRadian(float posSourX, float posSourY, float posTargetX, float posTargetY)
        //{
        //        //CHECKF(posSourX != posTargetX || posSourY != posTargetY);

        //        const float PI = 3.1415926535f;
        //        float fDeltaX = posTargetX - posSourX;
        //        float fDeltaY = posTargetY - posSourY;

        //        float fDistance	= (float) Math.Sqrt((double)(fDeltaX*fDeltaX + fDeltaY*fDeltaY));
        //      //  CHECKF(fDeltaX <= fDistance && fDistance > 0);
        //        float fRadian = (float)Math.Asin((double)(fDeltaX / fDistance));
        //        return fDeltaY > 0 ? (PI/2 - fRadian) : (PI + fRadian + PI/2);
        //}



    }

    //地图格子信息
    public struct MapGridInfo
    {

        //2015.11.14 优化内存用的-
        public byte Mask;

        //
     //   public ushort Mask; //掩码
     //   public ushort Terrain; //地形
     //   public short Altitude; //高度
       
    }

    //玩家属性
    public class PlayerAttribute
    {
        public int account_id;                     //帐号id  
        public int player_id;                       //角色id
        public uint attack;                         //最小物理攻击
        public uint attack_max;                     //最大物理攻击
        public uint magic_attack;                   //最小魔法攻击
        public uint magic_attack_max;               //最大魔法攻击
        public uint lookface;                       //外观
        public byte profession;                      //职业
        public uint hair;                            //发型
        public byte level;                        //等级
        public int exp;                          //当前经验
        public ulong exp_max;                        //最大经验
        public uint life;                       //当前生命
        public uint life_max;                   //最大生命
        public uint mana;                       //当前魔法
        public uint mana_max;                   //最大魔法
        public uint doage;                      //躲避
        public uint hitrate;                    //准确
        public uint defense;                    //防御
        public uint magic_defense;              //魔法防御
        public int sp;                         //体力
        public int sp_max;                     //最大体力
        public int gold;                       //金币
        public int gamegold;                   //魔石
        public long stronggold;                 //仓库金币
        public short pk;                           //当前pk值
        public byte pk_mode;                    //pk模式
        public uint mapid;
        public ulong guanjue;                //爵位积分
        public byte godlevel;                //神等级
        public byte maxeudemon;             //最大召唤幻兽商量
        public String sAccount;     //游戏帐号

        public PlayerAttribute()
        {
            resetAttr();
            account_id = 0;
            lookface = 0;
            profession = 0;
            hair = 0;
            level = 0;
            exp = 0;
            life = mana = 0;
            exp_max = 0;
            pk = 0;
            gold = gamegold = 0;
            sp =0;
            sp_max = 100;
            pk_mode = Define.PK_MODE_SAFE; //默认是安全pk模式
            mapid = 0;
            guanjue = 0;
            sAccount = "";
            godlevel = 0;
            maxeudemon = 2;
        }
        public void resetAttr()
        {
            doage = hitrate = 0;
            attack = attack_max = magic_attack = magic_attack_max = 0;
            life_max = mana_max = 0;
            defense = magic_defense = 0;
        }
    }

    //怪物属性
    public class MonterAttribute
    {
        public int life; //当前血量
        public int life_max; //最大血量
    }
    
    public class CRect
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public CRect(int xx = 0, int yy = 0, int w = 0, int h = 0)
        {
            x = xx;
            y = yy;
            width = w + x;
            height = h + x;
        }
        //检测是否在某个点上
        public bool Check(int xx, int yy)
        {
            if (xx >= x && xx <= width && yy >= y && yy <= height) return true;
            return false;
        }

    }



    //脚本信息
    public class ActionInfo
    {
        public uint id;
        public uint id_next;
        public uint id_nextfail;
        public uint type;
        public uint data;
        public String param;
        public ActionInfo()
        {
            id = id_next = id_nextfail = type = data = 0;
            param = "";
        }
    }


    public class ActionID
    {
        public const uint ACTION_MEMU_FRIST = 100;
        public const uint ACTION_MENU_TEXT = 101; //npc对话内容
        public const uint ACTION_MENU_LINK = 102; //npc选项
        public const uint ACTION_MENU_EDIT = 103;   //输入框
        public const uint ACTION_MENU_PIC = 104; //显示图片
        public const uint ACTION_MESSAGEBOX = 126;  //对话框
        public const uint ACTION_MENU_END = 499;


        ///-----物品操作
        public const uint ACTION_ITEM_FIRST = 500;
        public const uint ACTION_ITEM_ADD = 501;        //添加一个道具 物品id,位置,数量,强化等级,宝石1,宝石2,宝石3,战魂经验,地攻击,水攻击,火攻击,风攻击
        // public const uint ACTION_ITEM_ADD_EQUIP = 502;  //添加一个道具并穿戴起来
        public const uint ACTION_ITEM_DELETE = 503;     //删除一个道具  参数:道具id[0为当前执行脚本的道具id] 数量:
       
        public const uint ACTION_ITEM_LEVEL = 504;      //检测道具等级- 参数: 操作符[>,<,=] 等级
        public const uint ACTION_ITEM_DELETE_NAME = 505;    //删除道具从道具名称 参数: 道具名称，数量
        public const uint ACTION_ITEM_DELETE_ITEMID = 506;  //删除道具从基本物品id 参数:道具基本id 数量
        public const uint ACTION_EQUIP_OPERATION = 507;     //身上道具操作
        public const uint ACTION_CHECK_BAG_SIZE = 508;      //检测是否可容纳包裹 参数: 数量
        public const uint ACTION_ITEM_END = 999;

        //---地图操作
        public const uint ACTION_MAP_FIRST = 1000;
        public const uint ACTION_MAP_ENTERMAP = 1001;  //进入地图 参数:地图id x y 方向
        public const uint ACTION_MAP_CHANGE = 1002;     //改变玩家地图 参数: 地图id x y 方向
        public const uint ACTION_MAP_RECALL = 1003;     //回城
        public const uint ACTION_MAP_RANDOM = 1004;     //随机卷传送
        public const uint ACTION_MAP_CREATENPC = 1005;  //创建动态npc 参数: 地图id npcid x坐标 y坐标
        public const uint ACTION_MAP_CLEARNPC = 1006;   //删除动态npc 参数: 地图id npcid
        public const uint ACTION_MAP_END = 1499;


        //---角色属性操作--profession
        public const uint ACTION_ROLE_FIRST = 1500;
        public const uint ACTION_CHECK_PROFESSION = 1501;   //检测玩家职业 参数：职业 10.战士 20.法师 30.异能者 50.血族 60.亡灵巫师 70.暗黑龙骑
        public const uint ACTION_CHECK_LEVEL = 1502;         //检测玩家等级  参数:操作符(< > = ) 等级
       
        public const uint ACTION_SET_ROLE_PRO = 1503;       //设置玩家属性 参数:1.字符串[level等级] 2.值
        public const uint ACTION_ADDMAGIC = 1504;           //增加角色技能- 参数:技能id, 等级 ,经验
        public const uint ACTION_GET_ROLE_PRO = 1505;       //检测玩家属性 参数:1.字符串[level 等级] 2.操作符 3.值
        
 
        //---玩家定时器操作- 
        public const uint ACTION_TIMEOUT_CREATE = 1506;        //创建定时器-参数: 1.定时器id 2.定时时间[秒] 3.回调脚本id
        public const uint ACTION_TIMEOUT_CHECK = 1507;         //检查定时器是否过期- 参数: 1.定时器id
        public const uint ACTION_TIMEOUT_DELETE = 1508;         //删除定时器 参数: 1.定时器ID
        /*
         * ACTION_MAGIC_OPERATION
         * 玩家技能操作
         * 函数
         * {
         *  bool LearnMaigic(职业(0为无限制),技能ID,技能等级=0,技能经验=0)  
         * }
        */
        public const uint ACTION_MAGIC_OPERATION = 1510;        


        public const uint ACTION_ROLE_END = 1999;

        //杂类--
        public const uint ACTION_FUCK_FIRST = 2000;
        public const uint ACTION_OPENDIALOG = 2001;    //打开仓库 1.npc商店 3.仓库

        public const uint ACTION_LEARNMAGIC = 2002;     //学习技能 参数:技能id 技能等级[默认0] 经验值[默认0]
        public const uint ACTION_CHECKMAGIC = 2003;     //检测玩家是否有该技能
        public const uint ACTION_LEFTNOTICE = 2004;     //左上角公告 参数:公告信息
        public const uint ACTION_CHATNOTICE = 2005;     //聊天窗口系统公告参数:公告信息
        public const uint ACTION_SCREENNOTICE = 2006;   //屏幕中间系统公告参数:公告信息
      
        public const uint ACTION_RANDOM_INIT = 2007;    //初始化随机数 参数: 1.最小值 2.最大值

        public const uint ACTION_RANDOM_COMPARE = 2008; //比较随机数 参数: 1.操作符(>, =, <, >=, <=) 2.值
        public const uint ACTION_MSGBOX = 2009;         //信息框 参数：提示文本
        public const uint ACTION_PITCH = 2010;      //摊位操作 1.摆摊 2.取消摆摊
        public const uint ACTION_GETPAYGAMEGOLD = 2011; //充值取出魔石
        public const uint ACTION_FUCK_NIAN = 2012;  //打年成功 参数: 层数 11-18层 
     
        public const uint ACTION_GET_EUDEMON_PRO = 2014;        //检测幻兽属性 参数：类型:0.使用道具指向的幻兽 属性[quality:品质 wuxing:五行] 操作符[>,>=,=,<=,!=] 值
        public const uint ACTION_SET_EUDEMON_PRO = 2015;        //设置幻兽属性  参数：类型:0.使用道具指向的幻兽 属性[quality:品质 wuxing:五行] 操作符[-,=,+] 值
       
      
        public const uint ACTION_FUCK_END = 2499;

        //幻兽操作
        public const uint ACTION_EUDEMON_FIRST = 2500;
        public const uint ACTION_EUDEMON_CREATE = 2501; //创建幻兽 参数: 道具id[0.为当前使用道具的id] 
        public const uint ACTION_RECALL_EUDEMON = 2502; //召回幻兽- 参数: 类型: 0.出征的与合体的幻兽 1.出征的幻兽 2.合体的幻兽
        public const uint ACTION_EUDEMON_CREATEEX = 2503;   //创建幻兽扩展- 可以定义幻兽属性 参数: 1.道具id 2.等级 3.属性 4.五行
        public const uint ACTION_EUDEMON_END = 2599;
       

        //军团操作
        public const uint ACTION_LEGION_FIRST = 2600;
        public const uint ACTION_LEGION_CREATE = 2601; //创建军团 参数:等级 消耗金币数量 起始军团资金
        public const uint ACTION_LEGION_CHANGE_TITLE = 2602; //更改军团称谓- 参数:称谓   
        public const uint ACTION_LEGION_END = 2999;

        //副本操作
        public const uint ACTION_FUBEN_FIRST = 3000;
        public const uint ACTION_FUBEN_CREATE = 3001;//创建并且进入副本-参数: 1.地图id 2.副本类型[1.个人 2.组队[会把队伍都拉进去]] 3.X坐标 4.Y坐标
        public const uint ACTION_FUBEN_END = 3999;
    }


    //方向
    public class DIR
    {
        public const byte LEFT_DOWN = 0;    //左下
        public const byte LEFT = 1;         //左
        public const byte LEFT_UP = 2;      //左上
        public const byte UP = 3;           //上
        public const byte RIGHT_UP = 4;     //右上
        public const byte RIGHT = 5;        //右
        public const byte RIGHT_DOWN = 6;   //右下
        public const byte DOWN = 7;         //下

        //动作模式
        public const byte MOVEMODE_WALK = 0;				// PathMove()的模式
        public const byte MOVEMODE_RUN = 1;
        public const byte MOVEMODE_SHIFT = 2;						// to server only
        public const byte MOVEMODE_JUMP = 3;
        public const byte MOVEMODE_TRANS = 4;
        public const byte MOVEMODE_CHGMAP = 5;
        public const byte MOVEMODE_JUMPMAGICATTCK = 6;
        public const byte MOVEMODE_COLLIDE = 7;
        public const byte MOVEMODE_SYNCHRO = 8;					// to server only
        public const byte MOVEMODE_TRACK = 9;

        public const byte MOVEMODE_RUN_DIR0 = 20;
        public const byte MAX_DIRSIZE = 8;
        public const byte MOVEMODE_RUN_DIR7 = 27;
        public static short[] _DELTA_X = { 0, -1, -1, -1, 0, 1, 1, 1, 0 };
        public static short[] _DELTA_Y = { 1, 1, 0, -1, -1, -1, 0, 1, 0 };
        private static Random rd = new Random();
        //随机方向
        public static byte Random_Dir()
        {
            return (byte)rd.Next(LEFT_DOWN, DOWN);
        }
        public static bool Random_Walk(MapServer.BaseObject obj, ref byte dir, ref short x, ref short y)
        {
            byte index = 0;
            x = obj.GetCurrentX();
            y = obj.GetCurrentY();
            while (true)
            {

                if (index >= 10) break;
                dir = Random_Dir();
                switch (dir)
                {
                    case LEFT_DOWN:
                        {
                            x -= 1;
                            y += 1;
                            break;
                        }
                    case LEFT:
                        {
                            x -= 1;
                            break;
                        }
                    case LEFT_UP:
                        {
                            x -= 1;
                            y -= 1;
                            break;
                        }
                    case UP:
                        {
                            y -= 1;
                            break;
                        }
                    case RIGHT_UP:
                        {
                            x += 1;
                            y -= 1;
                            break;
                        }
                    case RIGHT:
                        {
                            x += 1;
                            break;
                        }
                    case RIGHT_DOWN:
                        {
                            x += 1;
                            y += 1;
                            break;
                        }
                    case DOWN:
                        {
                            y += 1;
                            break;
                        }

                }
                if (obj.GetGameMap().CanMove(x, y))
                {
                    return true;
                }
                index++;
            }
            return false;

        }
        //public static byte GetNextDir(ushort srcx, ushort srcy, ushort destx, ushort desty)
        //{
        //    for (int i = 0; i < _DELTA_X.Length; i++)
        //    {

        //    }
        //}

        public static byte GetDirByPos(short nFromX, short nFromY, short nToX, short nToY)
        {
            if (nFromX < nToX)
            {
                if (nFromY < nToY)
                    return DIR.DOWN;
                else if (nFromY > nToY)
                    return DIR.RIGHT;
                else
                    return DIR.RIGHT_DOWN;
            }
            else if (nFromX > nToX)
            {
                if (nFromY < nToY)
                    return DIR.LEFT;
                else if (nFromY > nToY)
                    return DIR.UP;
                else
                    return DIR.LEFT_UP;
            }
            else // if(nFromX == nToX)
            {
                if (nFromY < nToY)
                    return DIR.LEFT_DOWN;
                else if (nFromY > nToY)
                    return DIR.RIGHT_UP;
            }
            return MAX_DIRSIZE;
        }
        public static byte GetNextDir(short srcx, short srcy, short destx, short desty)
        {
            if (destx - srcx < 0 && desty - srcy > 0)
            {
                return DIR.LEFT_DOWN;
            }
            else if (destx - srcx < 0 && desty == srcy)
            {
                return DIR.LEFT;
            }
            else if (destx - srcx < 0 && desty - srcy < 0)
            {
                return DIR.LEFT_UP;
            }
            else if (destx == srcx && desty - srcy < 0)
            {
                return DIR.UP;
            }
            else if (destx - srcx > 0 && desty - srcy < 0)
            {
                return DIR.RIGHT_UP;
            }
            else if (destx - srcx > 0 && desty == srcy)
            {
                return DIR.RIGHT;
            }
            else if (destx - srcx > 0 && desty - srcy > 0)
            {
                return DIR.RIGHT_DOWN;
            }
            return DIR.DOWN;

        }

        //取下一个坐标点--
        public static bool GetNexPoint(MapServer.BaseObject obj, ref short x, ref short y)
        {
            byte dir = obj.GetDir();
            short srcx = obj.GetCurrentX();
            short srcy = obj.GetCurrentY();

            x = (short)(srcx + _DELTA_X[dir]);
            y = (short)(srcy + _DELTA_Y[dir]);

            if (!obj.GetGameMap().CanMove(x, y)) return false; //不能行走
            if (x == srcx && y == srcy) return false;
            return true;
        }

        //       //取反方向的下一个坐标--
        public static byte GetAgainstDir(byte dir)
        {

            switch (dir)
            {
                case DIR.LEFT_DOWN:
                    {
                        return DIR.RIGHT_UP;

                    }
                case DIR.LEFT:
                    {
                        return DIR.RIGHT_UP;

                    }
                case DIR.LEFT_UP:
                    {
                        return DIR.RIGHT_DOWN;

                    }
                case DIR.UP:
                    {
                        return DIR.DOWN;
                    }
                case DIR.RIGHT_UP:
                    {
                        return DIR.LEFT_DOWN;
                    }
                case DIR.RIGHT:
                    {
                        return DIR.LEFT;
                    }
                case DIR.RIGHT_DOWN:
                    {
                        return DIR.LEFT_UP;
                    }
                case DIR.DOWN:
                    {
                        return DIR.UP;
                    }
            }
            return MAX_DIRSIZE;
        }

    }

    //对象动作
    public class Action
    {
        public const byte NORMAL = 0;   //无动作
        public const byte IDLE = 1;     //待机
        public const byte MOVE = 2;     //移动
        public const byte ATTACK = 3;   //普通攻击
        public const byte DIE = 4;      //死亡 
        public const byte ALIVE = 5;     //复活
        public const byte INJURED = 6;  //受击
        private byte action;
        private byte[] data; //附加数据

        private List<object> param;
        public Action(byte _action, byte[] _data = null)
        {

            action = _action;
            data = null;

            if (_data != null)
            {
                data = new byte[_data.Length];
                Buffer.BlockCopy(_data, 0, data, 0, _data.Length);
            }

        }
        public byte GetAction() { return action; }
        public byte[] GetBuff() { return data; }
        public int GetObjectCount()
        {
            if (param == null) return 0;
            return param.Count;
        }
        public object GetObject(int index)
        {
            if (param == null) return null;
            if (index >= param.Count) return null;
            return param[index];
        }
        public void AddObject(object obj)
        {
            if (param == null)
            {
                param = new List<object>();
            }
            param.Add(obj);
        }
    }

    //取随机数二次封装
    public class IRandom
    {

        private static Random rd = new Random();
        public static int Random(int min, int max)
        {
            if (max <= min) return 0;
            return rd.Next(min, max);
        }
        public static byte Random(byte min, byte max)
        {

            if (max <= min) return 0;
            return (byte)rd.Next(min, max);
        }

        public static float Random(float min, float max, int len = 1)
        {
            if (max <= min) return 0;
            return (float)Math.Round(rd.NextDouble() * (max - min) + min, len);

        }
    }


    public class RoleMagicInfo
    {
        public int id;
        public uint magicid;
        public byte level;
        public uint exp;
        public RoleMagicInfo()
        {
            magicid = 0;
            level = 0;
            exp = 0;
        }
    }


    public class RoleItemInfo
    {

        private const int ITEMSORT_INVALID = -1;
        private const int ITEMSORT_EXPEND = 10;	// 易耗品
        private const int IETMSORT_FINERY = 1;	// 服饰
        private const int ITEMSORT_WEAPON1 = 4;	// 单手武器（武器）
        private const int ITEMSORT_MOUNT = 6;	// 坐骑
        private const int ITEMSORT_OTHER = 7;	// 其他, 不能直接使用


        // ITEMSORT_FINERY 类别物品
        public const int ITEMTYPE_HELMET = 10000;	// 头盔
        public const int ITEMTYPE_NECKLACE = 20000;	// 项链
        public const int ITEMTYPE_ARMOR = 30000;	// 盔甲
        public const int ITEMTYPE_BANGLE = 40000;	// 手镯
        public const int ITEMTYPE_MANTLE = 50000;	// 披风
        public const int ITEMTYPE_SHOES = 60000;	// 鞋子


        public uint id; //数据库索引id
        public uint itemid;      //道具id
        public ushort postion;
        public byte stronglv;
        public byte gemcount;
        public uint gem1;
        public uint gem2;
        public String forgename;
        public ushort amount;
        public int war_ghost_exp;//战魂经验
        public byte di_attack;  //地攻击
        public byte shui_attack; //水攻击
        public byte huo_attack; //火攻击
        public byte feng_attack; //风攻击
        public int property;        //如果是掉落金币的话，这个值是掉落的金币值2015.10.19  
        public uint gem3;   //第三个宝石
        public int god_strong;  //神炼强度  1016.1.24如果是法宝这个值为星级经验
        public int god_exp; //神佑经验

        //动态id 不存在数据库、、目前用于幻兽的id
        public uint typeid;
        public RoleItemInfo()
        {
            id = 0;
            itemid = 0;
            postion = 0;
            stronglv = 0;
            gemcount = 0;
            gem1 = 0;
            gem2 = 0;
            forgename = "";
            amount = 0;
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
        public byte GetStrongLevel()
        {
            return stronglv;
        }

        public void UpStrongLevel(byte lv)
        {
            stronglv += lv;
        }

        public bool DecStrongLevel()
        {
            if (stronglv == 0) return false;
            stronglv = (byte)(stronglv - 1);
            return true;
        }

        //提升品质
        public void UpQuality()
        {
            itemid++;
        }
        public int GetQuality()
        {
            String s = itemid.ToString();
            return Convert.ToInt32(s.Substring(s.Length - 1));
        }

        public int GetLevel()
        {
            if (this.IsShield() || this.IsArmor() || this.IsHelmet())
                return ((int)itemid % 100) / 10;
            else
                return ((int)itemid % 1000) / 10;
        }

        public void UpLevel()
        {
            uint id = itemid + 10;
            GameStruct.ItemTypeInfo info = ConfigManager.Instance().GetItemTypeInfo(id);
            if (info != null)
            {
                itemid = id;
            }
        }
        public bool IsEquip()//是否是装备
        {

            GameStruct.ItemTypeInfo baseinfo = ConfigManager.Instance().GetItemTypeInfo(itemid);
            uint nType = baseinfo.id;
            return !IsArrowSort(nType) && ((GetItemSort() >= IETMSORT_FINERY && GetItemSort() <= ITEMSORT_MOUNT) || IsShield());
        }

        public bool IsArrowSort(uint type)
        {
            if (type == 170001 || type == 1710001)
            {
                return true;
            }
            return false;
        }

        public int GetItemSort()
        {
            return ((int)itemid % 10000000) / 100000;
        }

        public bool IsShield()
        {
            return GetItemSort() == -1;
        }

        public int GetItemType()
        {
            if (GetItemSort() == ITEMSORT_WEAPON1) // || GetItemSort(idType) == ITEMSORT_EXPEND)
                return (((int)itemid % 100000) / 1000) * 1000;		// 返回（万位，千位）*千
            else
                return (((int)itemid % 100000) / 10000) * 10000;	// 返回万位*万
        }

        public bool IsArmor()
        {

            return IsFinery() && GetItemType() == ITEMTYPE_ARMOR;

        }

        public bool IsFinery()
        {
            return GetItemSort() == IETMSORT_FINERY;
        }

        public bool IsHelmet()
        {
            return IsFinery() && GetItemType() == ITEMTYPE_HELMET;
        }
        //获取装备打洞数量
        public int GetGemCount()
        {
            int ret = 0;
            if (gem1 != 0) ret++;
            if (gem2 != 0) ret++;
            if (gem3 != 0) ret++;
            return ret;
        }
        public byte GetGemType(byte index)
        {
            switch (index)
            {
                case 0:
                    {
                        return (byte)gem1;
                    }
                case 1:
                    {
                        return (byte)gem2;
                    }
                case 2:
                    {
                        return (byte)gem3;
                    }
            }
            return 0;
        }

        public void SetGemType(byte index, byte value)
        {
            switch (index)
            {
                case 0:
                    {
                        gem1 = value;
                        break;
                    }
                case 1:
                    {
                        gem2 = value;
                        break;
                    }
                case 2:
                    {
                        gem3 = value;
                        break;
                    }
            }
        }
        //打洞
        public void OpenGem(byte index)
        {
            switch (index)
            {
                case 0:
                    {
                        gem1 = 255;
                        break;
                    }
                case 1:
                    {
                        gem2 = 255;
                        break;
                    }
                case 2:
                    {
                        gem3 = 255;
                        break;
                    }
            }
        }
        //是否是宝石
        public bool IsGem()
        {
            return ConfigManager.Instance().GetGemInfo(itemid) == null ? false : true;
        }
        //返回宝石类型
        public byte GetGemType()
        {
            GemInfo info = ConfigManager.Instance().GetGemInfo(itemid);
            if (info != null)
            {
                return info.gemtype;
            }
            return 0;
        }

    }

    //角色属性
    public enum UserAttribute
    {
        LIFE = 0,               //当前血量
        LIFE_MAX = 1,           //最大血量
        MANA = 2,               //当前魔法
        MANA_MAX = 3,           //最大魔法
        GOLD = 4,               //金币
        EXP = 5,               //未知
        PK = 6,                 //PK值
        PORFESSION = 7,                //职业
        SIZEADD = 8,                 //角色体形增大?
        SP = 9,                 //当前体力
        MONEYSAVED = 10,            //未知
        ADDPOINT = 11,            //未知
        LOOKFACE = 12,            //头像+变身？ 15000 35000 0为还原 lookface?
        LEVEL = 13,             //等级
        MAGIC_ATTACK = 14,      //基础魔法攻击
        MAGIC_ATTACK_MAX = 15,  //最大魔法攻击
        ATTACK = 16,           //基础物理攻击
        SPEED = 17,        //最大物理攻击
        STATUS = 26,              //低32位状态
        HAIR = 27,              //发型改变
        XP = 28,                //XP值
        MAXEUDEMON = 39,        //最大可召唤幻兽数量
        GAMEGOLD = 46,          //魔石
        GUANJUE = 51,           //捐献爵位金额
        STATUS1 = 36,           //高32位状态
        STATUSEX = 71,              //低三十二位扩展状态
        STATUSEX1= 72,          //值不确定- 先补上
        MOLONGSHOUHU_STATUS = 99,   //魔龙守护状态 1.开启  0.关闭
        YUANSUZHANGKONG = 101,  //元素掌控状态
        LIUXINGYUNHUO = 107,        //流星陨火数量

    }

    //幻兽五行属性
    public enum EudemonWuXing
    {
        TU = 1,     //土
        SHUI = 2,  //水
        HUO = 3,   //火
        FENG = 4,  //风
        LEI = 5,  //雷
    }

    //幻兽属性
    public enum EudemonAttribute
    {
        Atk_Max = 0,        //最大物理攻击
        Atk_Min = 1,        //最小物理攻击
        MagicAtk_Max = 2,    //最大魔法攻击
        MagicAtk_Min = 3,   //最小魔法攻击
        Defense = 4,        //物理防御
        Magic_Defense = 5,  //魔法防御
        Life = 6,           //当前血量
        Life_Max = 7,       //最大血量
        Intimacy = 8,       //亲密度
        Exp = 9,            //经验 
        Level = 10,         //等级
        WuXing = 12,        //五行属性[1.土 2.水 3.火 4.风 5.雷]
        Luck = 13,          //幸运值
        Talent1 = 14,       //天赋槽1
        Talent2 = 15,       //天赋槽2
        Talent3 = 16,       //天赋槽3
        Talent4 = 17,       //天赋槽4
        Tablent5 = 18,      //天赋槽5
        Riding = 19,        //是否是骑乘幻兽2015.11.5 幻兽的类型 1.战士 2.法师 3.未知 4.法师+骑宠 5.战士+骑宠 6
        Recall_Count = 20,  //转世次数
        Param = 21,         //未知
        Card = 22,          //身份牌
      
        Quality = 24,       //品质
        Init_Atk = 25,      //初始攻击[四位数 前二位为最小攻击  后二位为最大攻击]
        Init_Magic_Atk = 26,//初始魔法攻击[四位数 前二位为最小攻击  后二位为最大攻击]
        Init_Defense = 27,  //初始防御[四位数 前二位为初始防御  后二位为初始魔防]
        Init_Life = 28,     //初始生命
        Life_Grow_Rate = 36,    //生命成长率
        Atk_Min_Grow_Rate = 37, //最小物攻成长率
        Atk_Max_Grow_Rate = 38, //最大物攻成长率
        MagicAtk_Min_Grow_Rate = 39,    //最小魔攻成长率
        MagicAtk_Max_Grow_Rate = 40,    //最大物攻成长率
        Defense_Grow_Rate  = 41,        //物防成长率
        MagicDefense_Grow_Rate =  42,   //魔法防御成长率
        Param50 = 50,
        Param63 = 63,

    }
    //怪物类型
    public class MonsterNameType
    {
        public const int NAME_GREEN = 0; //绿名怪
        public const int NAME_WHITE = 1;			// 白名怪
        public const int NAME_RED = 2;// 红名怪
        public const int NAME_BLACK = 3;		// 黑名怪
        public static int GetNameType(int nAtkerLev, int nMonsterLev)
        {
            int nDeltaLev = nAtkerLev - nMonsterLev;

            if (nDeltaLev >= 3)
                return NAME_GREEN;
            else if (nDeltaLev >= 0)
                return NAME_WHITE;
            else if (nDeltaLev >= -5)
                return NAME_RED;
            else
                return NAME_BLACK;
        }
    }
    //热键信息
    public class HotkeyInfo
    {
        public const byte GROUP_F1 = 1; //f1-f9
        public const byte GROUP_KEY1 = 2; //数字键1-9

        public const byte TYPE_ITEM = 0;//道具类型
        public const byte TYPE_MAGIC = 2;//技能类型
        public byte group;   //
        public byte index;  //序号 0
        public byte count; //索引 1
        public int id;      //id
        public int type; //
        public int baseid; //如果是道具- 这个就是道具的数据库id
        public byte amount; //数量
        public HotkeyInfo(byte _group, String text)
        {
            //加个异常处理— 防止非法封包让服务器崩溃
            try
            {
                group = _group;
                String[] str = text.Split('|');
                if (str.Length == 6)
                {
                    index = Convert.ToByte(str[0]);
                    count = Convert.ToByte(str[1]);
                    id = Convert.ToInt32(str[2]);
                    type = Convert.ToInt32(str[3]);
                    baseid = Convert.ToInt32(str[4]);
                    amount = Convert.ToByte(str[5]);
                }
            }
            catch (System.Exception ex)
            {
                Log.Instance().WriteLog(ex.Message);
                Log.Instance().WriteLog(ex.StackTrace);
                Log.Instance().WriteLog("非法保存热键结构！！！");
            }

        }
        public byte GetGroup()
        {
            return group;
        }
        public String GetString(bool isGroup = false)
        {
            String str = "";
            if (isGroup)
            {
                str = group.ToString() + "|";
            }
            str += index.ToString() + "|" + count.ToString() + "|" + baseid.ToString()
                + "|" + type.ToString() + "|" + id.ToString() + "|" + amount.ToString();
            return str;
        }
    }

    //装备强化配置信息
    public class EquipStrongInfo
    {
        public byte level; //等级
        public int chance;  //概率
        public EquipStrongInfo()
        {
            level = 0;
            chance = 0;
        }
    }


    //宝石配置信息
    public class GemInfo
    {
        public const byte GEMTYPE_ADDATTACK = 50;   //加攻击的宝石类型
        public const byte GEMTYPE_FIGHTPOWER = 54; //加战斗力的宝石类型
        public const byte GEMTYPE_DECLIFE = 9;      //减伤的宝石类型
        public const byte GEMTYPE_DURA = 63;        //加耐久的宝石类型
        public const byte GEMTYPE_ADDEXP = 52;      //加升级经验的宝石类型
        public uint itemid;
        public byte type; //宝石类型
        public int value;
        public int amount;//合成这个宝石所需要的数量
        public byte gemtype; //镶嵌到装备的宝石类型
        public GemInfo()
        {
            itemid = 0;
            type = 0;
            value = 0;
        }
    }

    //npc卖的道具配置信息
    public class NpcShopInfo
    {
        public uint id; //npcid
        public List<uint> mListItem;    //卖的物品列表
        public List<int> mListPrice;     //价格列表
        public NpcShopInfo(uint _npcid)
        {
            id = _npcid;
            mListItem = new List<uint>();
            mListPrice = new List<int>();
        }

        public int GetItemPrice(uint itemid)
        {
            for (int i = 0; i < mListItem.Count; i++)
            {
                if (mListItem[i] == itemid)
                {
                    return mListPrice[i];
                }
            }
            return -1;
        }
        public void AddItem(uint itemid, int price)
        {
            mListItem.Add(itemid);
            mListPrice.Add(price);
        }
    }

    //装备强化属性加成配置
    public class ItemAdditionInfo
    {
        public byte level; //等级
        public byte type;   //强化类型
        public uint life;        //生命
        public uint max_attack;  //最大物理攻击
        public uint min_attack;      //最小物理攻击
        public uint defense;     //物理防御
        public uint max_magic_attack;    //最大魔法攻击
        public uint min_magic_attack;        //最小魔法攻击
        public uint magic_defense;       //魔法防御
        public uint dodge;           //闪避
        public ItemAdditionInfo()
        {
            level = 0;
            type = 0;
            life = 0;
            max_attack = 0;
            min_attack = 0;
            defense = 0;
            max_magic_attack = 0;
            min_magic_attack = 0;
            magic_defense = 0;
            dodge = 0;
        }
    }
    //幻兽属性配置文件
    public class EudemonInfo
    {
        public uint id; //幻兽蛋
        public int life_min;  //最小初始生命
        public int life_max;//最大初始生命
        public int defense_min; //最小防御
        public int defense_max;//最大防御
        public int magicdef_min;//最小魔防
        public int magicdef_max;//最大魔防
        public int atk_min_min;//最小物攻下限
        public int atk_min_max;//最大物攻上限
        public int atk_max_min;//最大物攻下限
        public int atk_max_max;//最大物攻上限
        public int magicatk_min_min;//最小魔攻下限
        public int magicatk_min_max;    //最小魔攻上限
        public int magicatk_max_min;//最大魔攻下限
        public int magicatk_max_max;//最大魔攻上限
        public float life_grow_min;//生命成长下限
        public float life_grow_max;//生命成长上限
        public float defense_grow_min;//物防成长下限
        public float defense_grow_max;//物防成长上限
        public float magicdef_grow_min;//魔防成长下限
        public float magicdef_grow_max;//魔防成长上限
        public float atk_grow_min;//物攻成长下限
        public float atk_grow_max;//物攻成长上限
        public float magicatk_grow_min;//魔攻成长下限
        public float magicatk_grow_max;//魔攻成长上限
        public int quality_min;             //默认第一次进化品质
        public int qulity_max;          //d第一次进化品质
        public EudemonInfo()
        {
           
        }
    }
    public enum MONEYTYPE
    {
        GOLD = 1,       //金币
        GAMEGOLD = 2,   //魔石
        STRONGGOLD = 3,  //仓库金币
    }

    public enum EUDEMONSTATE
    {
        NROMAL = 0,     //休息
        BATTLE = 1,     //出战状态
        FIT = 2,        //合体

    }

    //头像配置文件
    public class LookFaceInfo
    {
        public uint itemid;     //道具id
        public int lookfaceid; //头像id
        public String name;     //头像名称
        public int price;       //价格
        public LookFaceInfo()
        {
            name = "";
            price = 0;
            itemid = 0;
            lookfaceid = 0;
        }
    }
    //发型配置文件
    public class HairInfo
    {
        public uint itemid;     //道具id
        public int hairid; //头像id
        public String name;     //发型名称
        public byte sex;        //性别
        public int price;       //价格
        public HairInfo()
        {
            name = "";
            itemid = 0;
            hairid = 0;
            sex = 0;
            price = 0;
        }
    }

    public enum GUANGJUELEVEL
    {
        NORMAL = 0, //无爵位
        KING = 1,   //王
        QUEEN = 2,  //女王
        DUKE = 3,   //公爵 
        MARQUIS = 4, //侯爵
        EARL = 5,       //伯爵
        VISCOUNT = 6,   //子爵
        LORD = 7,       //勋爵
    }

    //幻兽幻化配置文件
    public class EudemonSoulInfo
    {
        public int star;            //星级
        public int level;           //主幻兽需要等级
        public int fu_level;        //副幻兽需要等级
        public int fu_star;         //副幻兽需要星级
        public int add_min;         //最小加分
        public int add_max;         //最大加分
        public float add_main;      //主属性成长率加的值
        public float add_fu;        //副属性成长率加的值
        public int add_init;        //初始属性添加的值
        public bool bNotice;        //是否公告
    }
    //机器人配置文件
    public class RobotInfo
    {

        public String name;     //机器人名称
        public uint lookface;        //性别
        public uint hair;       //发型id
        public uint armor_id;   //护甲id
        public uint wepon_id;   //武器id
        public byte guanjue;        //官爵名称
        public uint rid_id;         //坐骑id
        public String legion_name;  //军团名称
        public short legion_place;  //军团职位
        public byte legion_title;  //军团称谓
        public uint map_id;         //地图id
        public short x;             //x坐标
        public short y;             //y坐标
        public byte dir;            //方向
        public RobotInfo()
        {
            name = "";
            legion_name = "";
        }
    }
    //ai配置文件
    public class AiInfo
    {
        public int nId;             //ai id
        public int nType;           //类型
        public int nRange;          //可视范围
        public int nAttack_Range;   //攻击范围
        public int nMove_Speed;     //移动速度
        public int nAttack_Speed;   //攻击速度
        public bool bIdle_Move;     //空闲状态是否可以移动
        public bool bMove;          //对象是否可以移动
        public AiInfo()
        {
            nId = 0;
            nType = 0;
            nRange = 0;
            nAttack_Range = 0;
            nMove_Speed = 0;
            nAttack_Speed = 0;
            bIdle_Move = false;
            bMove = false;
        }
    }
    //玩家buff
    public class RoleStatus
    {

        public const int STATUS_NORMAL = 0;
        public const int STATUS_DIE = 1;										// 死亡
        public const int STATUS_CRIME = 2;									// 犯罪闪蓝状态
        public const int STATUS_POISON = 3;										// 中毒
        public const int STATUS_TEAMLEADER = 4;										// 队长
        public const int STATUS_PKVALUE = 5;									// PK状态
        public const int STATUS_DETACH_BADLY = 6;									// 清除所有不良状态
        public const int STATUS_DETACH_ALL = 7;										// 清除所有魔法状态
        public const int STATUS_VAMPIRE = 8;									// ATKSTATUS中吸血
        public const int STATUS_DISAPPEARING = 9;									// 尸体消失状态
        public const int STATUS_MAGICDEFENCE = 10;									// 魔法防御提升/下降
        public const int STATUS_SUPER_MDEF = 11;									// 超级魔防
        public const int STATUS_ATTACK = 12;									// 攻击提升/下降
        public const int STATUS_REFLECT = 13;									// 攻击反射
        public const int STATUS_HIDDEN = 14;									// 隐身
        public const int STATUS_MAGICDAMAGE = 15;									// 魔法伤害提升/下降
        public const int STATUS_ATKSPEED = 16;									// 攻击速度提升/下降
        public const int STATUS_LURKER = 17;									// user only			// 潜行，此状态下不受NPC攻击，对玩家无效
        public const int STATUS_SYNCRIME = 18;									// 帮派犯罪
        public const int STATUS_REFLECTMAGIC = 19;									// 魔法反射
        public const int STATUS_SUPER_DEF = 20;									// 超级防御
        public const int STATUS_SUPER_ATK = 21;									// self only	// 超级攻击
        public const int STATUS_SUPER_MATK = 22;		 							// self only	// 超级魔攻
        public const int STATUS_STOP = 23;
        public const int STATUS_DEFENCE1 = 24;									// 防御提高/降低1
        public const int STATUS_DEFENCE2 = 25;									// 防御提高/降低2
        public const int STATUS_DEFENCE3 = 26;									// 防御提高/降低3
        public const int STATUS_FREEZE = 27;									// 冰冻状态
        public const int STATUS_SMOKE = 28;									// 烟雾效果
        public const int STATUS_DARKNESS = 29;									// 黑暗效果
        public const int STATUS_PALSY = 30;								// 麻痹效果

        public const int STATUS_TEAM_BEGIN = 31;
        public const int STATUS_TEAMHEALTH = 31;									// 医疗结界
        public const int STATUS_TEAMATTACK = 32;									// 攻击结界
        public const int STATUS_TEAMDEFENCE = 33;									// 护体结界
        public const int STATUS_TEAMSPEED = 34;									// 速度结界
        public const int STATUS_TEAMEXP = 35;									// 修炼结界
        public const int STATUS_TEAMSPIRIT = 36;									// 心灵结界
        public const int STATUS_TEAMCLEAN = 37;									// 净化结界
        public const int STATUS_TEAM_END = 37;

        public const int STATUS_SLOWDOWN1 = 38;									// 移动速度提升/下降
        public const int STATUS_SLOWDOWN2 = 39;									// 降低速度（仅在生命低于一半的时候。客户端表现）
        public const int STATUS_MAXLIFE = 40;									// 最大生命增加/降低
        public const int STATUS_MAXENERGY = 41;									// 最大体力增加/降低
        public const int STATUS_DEF2ATK = 42;									// 防御转换为攻击(power=被转换的防御百分比)
        public const int STATUS_ADD_EXP = 43;						// 战斗经验增加 -- 只能对队长使用，类似修炼结界效果
        public const int STATUS_DMG2LIFE = 44;									// 每次攻击伤害部分转换为自己的生命(power=被转换的百分比)
        public const int STATUS_ATTRACT_MONSTER = 45;								// 吸引怪物
        public const int STATUS_XPFULL = 46;									// XP满
        public const int STATUS_XPFULL_ATTACK = 47;                             //XP满，被单击

        public const int STATUS_MOLONGSHOUHU = 99;                  //暗黑龙骑 魔龙守护状态
        public const int STATUS_STEALTH = 100;                       //潜行
        public const int STATUS_FLY = 101;                          //雷霆万钧 飞行
        public const int STATUS_YUANSUZHANGKONG = 102;              //法师 元素掌控
        public const int STATUS_JUYANSHENGDUN = 103;                //法师 巨岩圣盾
        public const int STATUS_HEILONGWU = 104;                    //黑龙舞
        public const int STATUS_ANSHAXIELONG = 105;                 //暗杀邪龙
        public const int STATUS_MINGGUOSHENGNV = 106;               //冥国圣女
        public const int STATUS_WANGNIANWULING = 107;               //亡念巫灵
        public const int STATUS_MIXINSHU = 120;                     //迷心术
        public const int STATUS_WUDI = 1000;                        //无敌
        public const int STATUS_RED = 1001;                         //红名
        public const int STATUS_BLOCK = 1002;                       //黑名状态

        public const int STATUS_HUASHENWANGLING = 1003;             //化身亡灵形态
        public const int STATUS_HUASHENWUSHI = 1004;                //化身巫师形态
        public const int STATUS_SHENYUANELING = 1005;               //亡灵巫师 深渊恶灵
        public const int STATUS_DIYUXIEFU = 1006;                   //亡灵巫师 地狱邪蝠
        public const int STATUS_SHIHUNWULING = 1007;                //亡灵巫师 蚀魂巫灵
        public const int STATUS_ZHAOHUANWUHUAN = 1008;              //亡灵巫师 召唤巫环
        public const int STATUS_XUEXI = 1009;                       //血袭
        public const int STATUS_PTICH = 1010;                       //摆摊状态
        public const int STATUS_LIMIT = 65535;						// 角色状态不能超过这个值


        public int nStatus; //状态id
        public int nTime;   //持续时间- 秒
        public int nLastTick; 
        public RoleStatus()
        {
            nStatus = nTime = 0;
            nLastTick = System.Environment.TickCount;
        }
    }

}