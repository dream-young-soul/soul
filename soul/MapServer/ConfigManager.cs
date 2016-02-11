using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GameBase.Config;

namespace MapServer
{
    //读取常用的配置文件
    public class ConfigManager
    {
        public static ConfigManager m_Instance = null;
        private VerPacket mPacket;
        private Dictionary<uint, GameStruct.NPCInfo> mDicNpc;
        private Dictionary<uint, GameStruct.MonsterInfo> mDicMonster;
        private Dictionary<uint, GameStruct.ItemTypeInfo> mDicItemType;
        private Dictionary<uint, GameStruct.MagicTypeInfo> mDicMagicType;
        private Dictionary<uint, Dictionary<byte, GameStruct.LevelExp>> mDicLevelExp;
        private Dictionary<byte, Dictionary<byte, GameStruct.BaseAttributeInfo>> mDicAttribute;
        private Dictionary<uint, GameStruct.DropItemInfo> mDicDropItem;
        private Dictionary<uint, List<GameStruct.MapGateInfo>> mDicMapGate;
        private Dictionary<uint, GameStruct.TrackInfo> mDicTrack;
        private Dictionary<uint, GameStruct.GemInfo> mDicGem;
        private Dictionary<uint, GameStruct.NpcShopInfo> mDicNpcShop;
        private Dictionary<byte, List<GameStruct.ItemAdditionInfo>> mDicItemAddition;
        private Dictionary<uint, GameStruct.EudemonInfo> mDicEudemonInfo;
        private Dictionary<uint, GameStruct.LookFaceInfo> mDicLookFace;
        private Dictionary<uint, GameStruct.HairInfo> mDicHair;
        private List<GameStruct.RobotInfo> mListRobotInfo;
        private Dictionary<int,GameStruct.EudemonSoulInfo> mDicEudemonSoul;
        private Dictionary<int, GameStruct.AiInfo> mDicAiInfo;
        public static ConfigManager Instance()
        {
            if (m_Instance == null)
            {
                m_Instance = new ConfigManager();
            }
            return m_Instance;
        }

        public ConfigManager()
        {
            mPacket = new VerPacket();
            mDicNpc = new  Dictionary<uint, GameStruct.NPCInfo>() ;
            mDicMonster = new Dictionary<uint, GameStruct.MonsterInfo>();
            mDicItemType = new Dictionary<uint, GameStruct.ItemTypeInfo>();
            mDicMagicType = new Dictionary<uint, GameStruct.MagicTypeInfo>();
            mDicAttribute = new Dictionary<byte, Dictionary<byte, GameStruct.BaseAttributeInfo>>();
            mDicLevelExp = new Dictionary<uint, Dictionary<byte, GameStruct.LevelExp>>();
            mDicDropItem = new Dictionary<uint, GameStruct.DropItemInfo>();
            mDicMapGate = new Dictionary<uint, List<GameStruct.MapGateInfo>>();
            mDicTrack = new Dictionary<uint, GameStruct.TrackInfo>();
            mDicGem = new  Dictionary<uint, GameStruct.GemInfo>() ;
            mDicNpcShop = new Dictionary<uint, GameStruct.NpcShopInfo>();
            mDicItemAddition = new Dictionary<byte, List<GameStruct.ItemAdditionInfo>>();
            mDicEudemonInfo = new Dictionary<uint, GameStruct.EudemonInfo>();
            mDicLookFace = new Dictionary<uint, GameStruct.LookFaceInfo>();
            mDicHair = new Dictionary<uint, GameStruct.HairInfo>();
            mListRobotInfo = new List<GameStruct.RobotInfo>();
            mDicAiInfo = new Dictionary<int, GameStruct.AiInfo>();
            mDicEudemonSoul = new Dictionary<int,GameStruct.EudemonSoulInfo>();
            mDicNpc.Clear();
        }

        public bool LoadConfig()
        {
            if (!LoadGameMapInfo())
            {
                Log.Instance().WriteLog("载入地图文件失败");
                return false;
            }
            if (!LoadAiInfo())
            {
                Log.Instance().WriteLog("载入AI配置文件失败");
                return false;
            }
            if (!LoadNpcInfo())
            {
                Log.Instance().WriteLog("载入NPC文件失败");
                return false;
            
           }
           if (!LoadMonsterInfo())
           {
                Log.Instance().WriteLog("载入怪物文件失败");
                return false;
           }
           if (!LoadMagicTypeInfo())
           {
               Log.Instance().WriteLog("载入技能文件失败");
               return false;
           }
           if (!LoadGeneratorInfo())
           {
               Log.Instance().WriteLog("载入刷怪文件失败");
               return false;
            }
           if (!LoadItemTypeInfo())
           {
               Log.Instance().WriteLog("载入道具文件失败");
               return false;
            }
           if (!LoadGolbalScript())
           {
               Log.Instance().WriteLog("载入全局脚本失败");
               return false;
            }
           if (!LoadAttributeInfo())
           {
               Log.Instance().WriteLog("载入等级属性文件失败");
               return false;
           }
           if (!LoadLevelExpInfo())
           {
               Log.Instance().WriteLog("载入等级经验文件失败");
               return false;
            }
           if (!LoadDropItemInfo())
           {
               Log.Instance().WriteLog("载入怪物掉落文件失败");
               return false;
            }
           if (!LoadMapGateInfo())
           {
               Log.Instance().WriteLog("载入地图传送点失败");
               return false;
            }
           if (!LoadRegionInfo())
           {
               Log.Instance().WriteLog("载入地图参数文件失败");
               return false;
            }
           if (!LoadMagicTrackInfo())
           {
               Log.Instance().WriteLog("载入连招动作失败");
               return false;
            }
           if (!EquipOperation.Instance().Load())
           {
               Log.Instance().WriteLog("载入装备操作信息文件失败");
               return false;
            }
           if (!LoadGemInfo())
           {
               Log.Instance().WriteLog("载入宝石配置文件失败");
               return false;
            }
           if (!LoadNpcShopInfo())
           {
               Log.Instance().WriteLog("载入npc商店文件失败");
               return false;
            }
           if (!LoadItemAdditionInfo())
           {
               Log.Instance().WriteLog("载入装备强化信息失败");
               return false;
            }
           if (!LoadEudemonInfo())
           {
               Log.Instance().WriteLog("载入幻兽属性失败");
               return false;
           }
           if (!LoadLookFaceInfo())
           {
               Log.Instance().WriteLog("载入头像文件失败!");
               return false;
            }
           if (!LoadHairInfo())
           {
               Log.Instance().WriteLog("载入发型文件失败!");
               return false;
            }
           if (!LoadRobotInfo())
           {
               Log.Instance().WriteLog("载入机器人失败");
               return false;
            }
           if (!this.LoadEudemonSoulInfo())
           {
               Log.Instance().WriteLog("载入幻兽幻化文件失败");
               return false;
            }
            //定时脚本管理器加载
           if (!ScriptTimerManager.Instance().Load())
           {
               Log.Instance().WriteLog("载入定时脚本管理文件失败");
               return false;
           }
        
            return true;
        }


        private bool LoadGameMapInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_MAP);
            CsvFile csv = new CsvFile(text);
            
            String v;
            if (text == "") return false;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                GameStruct.MapInfo info = new GameStruct.MapInfo();
                v = csv.GetFieldInfoToValue(i, "id");
                info.id = Convert.ToUInt32(v);
                info.name = csv.GetFieldInfoToValue(i, "name");
                info.dmappath = csv.GetFieldInfoToValue(i, "dmap");
                v = csv.GetFieldInfoToValue(i, "recallid");
                info.recallid = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "recallx");
                info.recallx = Convert.ToUInt16(v);
                v = csv.GetFieldInfoToValue(i, "recally");
                info.recally = Convert.ToUInt16(v);
                v = csv.GetFieldInfoToValue(i, "snows");
                info.issnows = Convert.ToBoolean(v);
                //加入到地图
                GameMap map = new GameMap(info);
                if (!map.Create())
                {
                    Log.Instance().WriteLog("加载地图失败.." + info.name);
                }
                MapManager.Instance().AddMap(map);
               
            }
            return true;
        }

        private bool LoadNpcInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_NPC);
            CsvFile csv = new CsvFile(text);
            GameStruct.NPCInfo info;
            String v;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                info = new GameStruct.NPCInfo();
                v = csv.GetFieldInfoToValue(i, "id");
                info.id = Convert.ToUInt32(v);
                info.name = csv.GetFieldInfoToValue(i, "name");
                v = csv.GetFieldInfoToValue(i, "mapid");
                info.mapid = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "x");
                info.x = Convert.ToInt16(v);
                v = csv.GetFieldInfoToValue(i, "y");
                info.y = Convert.ToInt16(v);
                v = csv.GetFieldInfoToValue(i, "lookface");
                info.lookface = Convert.ToInt32(v);
           
                info.ScriptPath = csv.GetFieldInfoToValue(i, "script");
                if (info.ScriptPath != "null")
                {
                    info.ScriptID = ScripteManager.Instance().LoadScripteFile(info.ScriptPath);
                }
                else
                {
                    info.ScriptID = 0;
                }
               
             
                NpcObject obj = new NpcObject(info);
                obj.SetID(info.id);
                
                obj.Name = info.name;
           
                //obj.SetDir(info.dir);
                obj.ScriptId = info.ScriptID;
               
                if (mDicNpc.ContainsKey(info.id))
                {
                    Log.Instance().WriteLog("检测到相同npcid:" + info.name + " 重复:" + mDicNpc[info.id].name);
                    continue;
                }
                GameMap map = MapManager.Instance().GetGameMapToID(info.mapid);
                if (map == null)
                {
                    continue;
                }
                map.AddObject(obj);
                obj.SetPoint(info.x, info.y);
                mDicNpc[info.id] = info;
            }
            return true;
        }

        
        private bool LoadMonsterInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_MONSTER);
            CsvFile csv = new CsvFile(text);
           
            String v;
            GameStruct.MonsterInfo info;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                info = new GameStruct.MonsterInfo();
                v = csv.GetFieldInfoToValue(i, "id");
                info.id = Convert.ToUInt32(v);
                info.name = csv.GetFieldInfoToValue(i, "name");
                v = csv.GetFieldInfoToValue(i, "ai");
                info.ai = Convert.ToUInt16(v);
                v = csv.GetFieldInfoToValue(i, "lookface");
                info.lookface = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "level");
                info.level = Convert.ToUInt16(v);
                v = csv.GetFieldInfoToValue(i, "life");
                info.life = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "mana");
                info.mana = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "attack_min");
                info.attack_min = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "attack_max");
                info.attack_max = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "defense");
                info.defense = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "dodge");
                info.dodge = Convert.ToUInt16(v);
                v = csv.GetFieldInfoToValue(i, "range");
                info.range = Convert.ToUInt16(v);
                v = csv.GetFieldInfoToValue(i, "attack_speed");
                info.attack_speed = Convert.ToUInt16(v);
                v = csv.GetFieldInfoToValue(i, "move_speed");
                info.move_speed = Convert.ToUInt16(v);
                v = csv.GetFieldInfoToValue(i, "drop_group");
                info.drop_group = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "eudemon_type");
                info.eudemon_type = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "die_scripte_id");
                info.die_scripte_id = Convert.ToUInt32(v);
                if (mDicMonster.ContainsKey(info.id))
                {
                    Log.Instance().WriteLog("检测到相同怪物id:" + info.name + " 重复:" + mDicNpc[info.id].name);
                    continue;
                }
                mDicMonster[info.id] = info;
            }
            return true;
        }

        //载入刷怪文件
        private bool LoadGeneratorInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_GENERATOR);
            CsvFile csv = new CsvFile(text);
            GameMap map;
            String v;
            GameStruct.GeneratorInfo info = new GameStruct.GeneratorInfo();
            for (int i = 0; i < csv.GetLine(); i++)
            {
                v = csv.GetFieldInfoToValue(i, "mapid");
                info.mapid = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i,"bound_x");
                info.bound_x = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "bound_y");
                info.bound_y = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "bound_cx");
                info.bound_cx = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "bound_cy");
                info.bound_cy = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "amount");
                info.amount = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "time");
                info.time = Convert.ToUInt32(v) * 1000;
                v = csv.GetFieldInfoToValue(i, "monsterid");
                info.monsterid = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "dir");
                info.dir = Convert.ToByte(v);
                map = MapManager.Instance().GetGameMapToID(info.mapid);
                if (map != null)
                {
                    map.CreateMonster(info);
                }
            }
            return true;
        }

        private bool LoadItemTypeInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_ITEMTYPE);
            CsvFile csv = new CsvFile(text);
            int i = 0;
            String v;

            try
            {
                
                for (; i < csv.GetLine(); i++)
                {
                    GameStruct.ItemTypeInfo info = new GameStruct.ItemTypeInfo();
                    v = csv.GetFieldInfoToValue(i, "id");
                    info.id = Convert.ToUInt32(v);
                    info.name = csv.GetFieldInfoToValue(i, "name");
                    v = csv.GetFieldInfoToValue(i, "req_profession");
                    info.req_profession = Convert.ToByte(v);
                    v = csv.GetFieldInfoToValue(i, "req_level");
                    info.req_level = Convert.ToByte(v);
                    v = csv.GetFieldInfoToValue(i, "req_sex");
                    info.req_sex = Convert.ToByte(v);
                    v = csv.GetFieldInfoToValue(i, "attack_min");
                    info.attack_min = Convert.ToUInt32(v);
                    v = csv.GetFieldInfoToValue(i, "attack_max");
                    info.attack_max = Convert.ToUInt32(v);
                    v = csv.GetFieldInfoToValue(i, "defense");
                    info.defense = Convert.ToUInt32(v);
                    v = csv.GetFieldInfoToValue(i, "magic_defense");
                    info.magic_defense = Convert.ToUInt32(v);
                    v = csv.GetFieldInfoToValue(i, "magic_attack_min");
                    info.magic_attack_min = Convert.ToUInt32(v);
                    v = csv.GetFieldInfoToValue(i, "magic_attck_max");
                    info.magic_attck_max = Convert.ToUInt32(v);
                    v = csv.GetFieldInfoToValue(i, "dodge");
                    info.dodge = Convert.ToUInt32(v);
                    v = csv.GetFieldInfoToValue(i, "hitrate");
                    info.hitrate = Convert.ToUInt32(v);
                    v = csv.GetFieldInfoToValue(i, "amount");
                    info.amount = Convert.ToUInt16(v);
                    v = csv.GetFieldInfoToValue(i, "amount_limit");
                    info.amount_limit = Convert.ToUInt16(v);
                    v = csv.GetFieldInfoToValue(i, "actionid");
                    info.actionid = Convert.ToUInt32(v);
                    v = csv.GetFieldInfoToValue(i, "price");
                    info.price = Convert.ToInt32(v);
                    v = csv.GetFieldInfoToValue(i, "monster_type");
                    info.monster_type = Convert.ToUInt32(v);
                    info.info = csv.GetFieldInfoToValue(i, "info");
                    mDicItemType[info.id] = info;
                }
            }
            catch (System.Exception ex)
            {

                Log.Instance().WriteLog(ex.Message);
                Log.Instance().WriteLog(ex.StackTrace);
                Log.Instance().WriteLog("载入物品数据库失败，行号:" + i.ToString());
                return false;
            }
         
            return true;
        }

        private bool LoadMagicTypeInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_MAGICTYPE);
            CsvFile csv = new CsvFile(text);
            String v;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                GameStruct.MagicTypeInfo info = new GameStruct.MagicTypeInfo();
                v = csv.GetFieldInfoToValue(i, "id");
                info.id = Convert.ToUInt32(v);

                v = csv.GetFieldInfoToValue(i, "typeid");
                info.typeid = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "sort");
                info.sort = Convert.ToByte(v); 
                v = csv.GetFieldInfoToValue(i, "name");
                info.name = v;
                v = csv.GetFieldInfoToValue(i, "crime");
                info.crime = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "ground");
                info.ground = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "multi");
                info.multi = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "target");
                info.target = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "level");
                info.level = Convert.ToByte(v); 
                v = csv.GetFieldInfoToValue(i, "use_mp");
                info.use_mp = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "use_potential");
                info.use_potential = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "power");
                info.power = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "intone_speed");
                info.intone_speed = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "percent");
                info.percent = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "step_secs");
                info.step_secs = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "range");
                info.range = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "distance");
                info.distance = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "status_chance");
                info.status_chance = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "status");
                info.status = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "need_prof");
                info.need_prof = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "need_exp");
                info.need_exp = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "need_level");
                info.need_level = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "need_gemtype");
                info.need_gemtype = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "use_xp");
                info.use_xp = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "weapon_subtype");
                info.weapon_subtype = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "active_times");
                info.active_times = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "auto_active");
                info.auto_active = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "floor_attr");
                info.floor_attr = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "auto_learn");
                info.auto_learn = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "learn_level");
                info.learn_level = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "drop_weapon");
                info.drop_weapon = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "use_ep");
                info.use_ep = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "weapon_hit");
                info.weapon_hit = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "use_item");
                info.use_item = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "next_magic");
                info.next_magic = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "delay_ms");
                info.delay_ms = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "use_item_num");
                info.use_item_num = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "width");
                info.width = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "durability");
                info.durability = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "apply_ms");
                info.apply_ms = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "track_id");
                info.track_id = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "track_id2");
                info.track_id2 = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "auto_learn_prob");
                info.auto_learn_prob = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "group_type");
                info.group_type = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "group_member1_pos");
                info.group_member1_pos = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "group_member2_pos");
                info.group_member2_pos = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "group_member3_pos");
                info.group_member3_pos = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "magic1");
                info.magic1 = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "magic2");
                info.magic2 = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "magic3");
                info.magic3 = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "magic4");
                info.magic4 = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "attack_combine");
                info.attack_combine = Convert.ToUInt32(v); 
                v = csv.GetFieldInfoToValue(i, "flag");
                info.flag = 0;
                mDicMagicType[info.id] = info;
            }

            return true;
        }
        private bool LoadGolbalScript()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_GOLBALSCRIPT);
            CsvFile csv = new CsvFile(text);
            
            for (int i = 0; i < csv.GetLine(); i++)
            {
                String sPath = csv.GetFieldInfoToValue(i, "script");
                ScripteManager.Instance().LoadScripteFile(sPath);
            }
            return true;
        }

        private bool LoadAttributeInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_ATTRIBUTE);
            CsvFile csv = new CsvFile(text);
            String data;
            Dictionary<byte, GameStruct.BaseAttributeInfo> dic;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                GameStruct.BaseAttributeInfo info = new GameStruct.BaseAttributeInfo();
                data = csv.GetFieldInfoToValue(i, "profession");
                byte profession = Convert.ToByte(data);
               
                if (!mDicAttribute.ContainsKey(profession))
                {
                    dic = new Dictionary<byte, GameStruct.BaseAttributeInfo>();
                    mDicAttribute[profession] = dic;
                }
                else dic = mDicAttribute[profession];

                data = csv.GetFieldInfoToValue(i, "level");
                info.lv = Convert.ToByte(data);
                data = csv.GetFieldInfoToValue(i, "force");
                info.force = Convert.ToInt32(data);
                data = csv.GetFieldInfoToValue(i, "dexterity");
                info.dexterity = Convert.ToInt32(data);
                data = csv.GetFieldInfoToValue(i, "health");
                info.health = Convert.ToInt32(data);
                data = csv.GetFieldInfoToValue(i, "soul");
                info.soul = Convert.ToInt32(data);
                dic[info.lv] = info;

            }
            return true;
        }

        private bool LoadLevelExpInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_LEVELEXP);
            CsvFile csv = new CsvFile(text);
            String data;
            Dictionary<byte, GameStruct.LevelExp> dic;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                GameStruct.LevelExp info = new GameStruct.LevelExp();
                data = csv.GetFieldInfoToValue(i, "type");
                uint type = Convert.ToUInt32(data);

                if (!mDicLevelExp.ContainsKey(type))
                {
                    dic = new Dictionary<byte, GameStruct.LevelExp>();
                    mDicLevelExp[type] = dic;
                }
                else dic = mDicLevelExp[type];

                data = csv.GetFieldInfoToValue(i, "level");
                info.level = Convert.ToByte(data);
                data = csv.GetFieldInfoToValue(i, "exp");
                info.exp = Convert.ToUInt64(data);
                dic[info.level] = info;

            }
            return true;
        }

        private bool LoadDropItemInfo()
        {
            
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_DROPITEM);
            CsvFile csv = new CsvFile(text);
            String data;
            GameStruct.DropItemInfo info;
            uint groupid, itemid, amount, rate;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                data = csv.GetFieldInfoToValue(i, "group");
                groupid = Convert.ToUInt32(data);
                if (mDicDropItem.ContainsKey(groupid))
                {
                    info = mDicDropItem[groupid];
                }
                else
                {
                    info = new GameStruct.DropItemInfo();
                    info.groupid = groupid;
                    mDicDropItem[groupid] = info;
                }
                GameStruct.DropItemClass dropclass = new GameStruct.DropItemClass();
                data = csv.GetFieldInfoToValue(i, "itemid");
                if (data.IndexOf('|') != -1)
                {
                    String[] split = data.Split('|');
                    for(int j = 0;j < split.Length;j++)
                    {
                        itemid = Convert.ToUInt32(split[j]);
                        if(ConfigManager.Instance().GetItemTypeInfo(itemid) == null)
                        {
                            Log.Instance().WriteLog("未找到的掉落道具ID"+itemid.ToString());
                        }
                         dropclass.list_itemid.Add(itemid);
                    }
                }
                else
                {
                    
                    itemid = Convert.ToUInt32(data);
                    if(ConfigManager.Instance().GetItemTypeInfo(itemid) == null)
                    {
                        Log.Instance().WriteLog("未找到的掉落道具ID"+itemid.ToString());
                    }
                    dropclass.list_itemid.Add(itemid);
                    
                }
                info.listitem.Add(dropclass);
                data = csv.GetFieldInfoToValue(i, "amount");
                amount = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "rate");
                rate = Convert.ToUInt32(data);
               
                info.listamount.Add(amount);
                info.listrate.Add(rate);
            }
            return true;
        }
        private bool LoadGemInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_GEM);
              CsvFile csv = new CsvFile(text);
            String data;
            GameStruct.GemInfo info;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                data = csv.GetFieldInfoToValue(i, "itemid");
                uint itemid = Convert.ToUInt32(data);
                if (ConfigManager.Instance().GetItemTypeInfo(itemid) == null)
                {
                    Log.Instance().WriteLog("载入宝石信息错误,不存在该道具！" + itemid.ToString());
                    continue; 
                }

                info = new GameStruct.GemInfo();
                info.itemid = itemid;
                data = csv.GetFieldInfoToValue(i, "type");
                info.type = Convert.ToByte(data);
                data = csv.GetFieldInfoToValue(i, "value");
                info.value = Convert.ToInt32(data);
                data = csv.GetFieldInfoToValue(i, "amount");
                info.amount = Convert.ToInt32(data);
                data = csv.GetFieldInfoToValue(i, "gemtype");
                info.gemtype = Convert.ToByte(data);
                mDicGem[info.itemid] = info;
            }
            return true;
        }

        private bool LoadNpcShopInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_NPCSHOP);
            CsvFile csv = new CsvFile(text);
            String data;
            GameStruct.NpcShopInfo info;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                data = csv.GetFieldInfoToValue(i, "npcid");
                uint npcid = Convert.ToUInt32(data);
                if (mDicNpcShop.ContainsKey(npcid))
                {
                    info = mDicNpcShop[npcid];
                }
                else
                {
                    info = new GameStruct.NpcShopInfo(npcid);
                    mDicNpcShop[npcid] = info;
                }
                data = csv.GetFieldInfoToValue(i, "itemid");
                uint itemid = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "price"); 
                int price = Convert.ToInt32(data);
                info.AddItem(itemid, price);
            }
            return true;
        }
        private bool LoadItemAdditionInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_ITEMADDITION);
            CsvFile csv = new CsvFile(text);
            String data;
            GameStruct.ItemAdditionInfo info;
            List<GameStruct.ItemAdditionInfo> list;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                data = csv.GetFieldInfoToValue(i, "type");
                byte type = Convert.ToByte(data);
                if (mDicItemAddition.ContainsKey(type))
                {
                    list = mDicItemAddition[type];
                }
                else
                {
                    list = new List<GameStruct.ItemAdditionInfo>();
                    mDicItemAddition[type] = list;
                }
                info = new GameStruct.ItemAdditionInfo();
                info.type = type;
                data = csv.GetFieldInfoToValue(i, "level");
                info.level = Convert.ToByte(data);
                data = csv.GetFieldInfoToValue(i, "life");
                info.life = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "max_attack");
                info.max_attack = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "min_attack");
                info.min_attack = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "defense");
                info.defense = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "max_magicack");
                info.max_magic_attack = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "min_magicack");
                info.min_attack = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "magic_defense");
                info.magic_defense = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "dodge");
                info.dodge = Convert.ToUInt32(data);
                list.Add(info);
            }
            return true;
        }

        public GameStruct.ItemAdditionInfo GetItemAdditionInfo(byte type, byte level)
        {
            if (mDicItemAddition.ContainsKey(type))
            {
                List<GameStruct.ItemAdditionInfo> list = mDicItemAddition[type];
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].level == level)
                    {
                        return list[i];
                    }
                }
            }
            return null;
        }
        public GameStruct.NpcShopInfo GetNpcShopInfo(uint npcid)
        {
            if (mDicNpcShop.ContainsKey(npcid))
            {
                return mDicNpcShop[npcid];
            }
            return null;
        }
        private bool LoadMagicTrackInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_TRACK);
              CsvFile csv = new CsvFile(text);
            String data;
          
            GameStruct.TrackInfo info;
         
            for (int i = 0; i < csv.GetLine(); i++)
            {
                info = new GameStruct.TrackInfo();
                data = csv.GetFieldInfoToValue(i, "id");
                info.id = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "id_next");
                info.id_next = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "direction");
                info.direction = Convert.ToByte(data);
                data = csv.GetFieldInfoToValue(i, "step");
                info.step = Convert.ToByte(data);
                data = csv.GetFieldInfoToValue(i, "alt");
                info.alt = Convert.ToByte(data);
                data = csv.GetFieldInfoToValue(i, "action");
                info.action = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "power");
                info.power = Convert.ToInt32(data);
                data = csv.GetFieldInfoToValue(i, "apply_ms");
                info.apply_ms = Convert.ToInt32(data);
                mDicTrack[info.id] = info;
            }
            return true;
        }

        private bool LoadRegionInfo()
        {

            String text = mPacket.LoadFileToText(TextDefine.CONFIG_REGION_FILE);
            CsvFile csv = new CsvFile(text);
            GameStruct.MapRegionInfo info;
            String data;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                data = csv.GetFieldInfoToValue(i, "mapid");
                info.mapid = Convert.ToUInt32(data);
               GameMap map =  MapManager.Instance().GetGameMapToID(info.mapid);
               if (map == null)
               {
                   Log.Instance().WriteLog("载入地图注册参数失败,没有找到该地图id" + info.mapid.ToString());
                   continue;
                }
               data = csv.GetFieldInfoToValue(i, "type");
               info.type = Convert.ToInt32(data);
               data = csv.GetFieldInfoToValue(i, "bound_x");
               info.bound_x = Convert.ToInt16(data);
               data = csv.GetFieldInfoToValue(i, "bound_y");
               info.bound_y = Convert.ToInt16(data);
               data = csv.GetFieldInfoToValue(i, "bound_cx");
               info.bound_cx = Convert.ToInt16(data);
               data = csv.GetFieldInfoToValue(i, "bound_cy");
               info.bound_cy = Convert.ToInt16(data);
               map.AddRegionInfo(info);
            }
            return true;

        }
        private bool LoadMapGateInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_MAPGATE);
            CsvFile csv = new CsvFile(text);
            String data;
            List<GameStruct.MapGateInfo> list_info;
            GameStruct.MapGateInfo info;
            uint src_mapid = 0;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                
                data = csv.GetFieldInfoToValue(i, "src_mapid");
                src_mapid = Convert.ToUInt32(data);
                if (mDicMapGate.ContainsKey(src_mapid))
                {
                    list_info = mDicMapGate[src_mapid];
                }
                else
                {
                    list_info = new List<GameStruct.MapGateInfo>();
                    mDicMapGate[src_mapid] = list_info;
                }
                info = new GameStruct.MapGateInfo();
                info.src_mapid = src_mapid;
                data = csv.GetFieldInfoToValue(i, "src_x");
                info.src_x = Convert.ToInt16(data);
                data = csv.GetFieldInfoToValue(i, "src_y");
                info.src_y = Convert.ToInt16(data);
                data = csv.GetFieldInfoToValue(i, "target_mapid");
                info.target_mapid = Convert.ToUInt32(data);
                data = csv.GetFieldInfoToValue(i, "target_x");
                info.target_x = Convert.ToInt16(data);
                data = csv.GetFieldInfoToValue(i, "target_y");
                info.target_y = Convert.ToInt16(data);
                data = csv.GetFieldInfoToValue(i, "dis");
                info.dis = Convert.ToInt32(data);
                list_info.Add(info);

            }
            return true;
        }
       private bool LoadEudemonInfo()
        {
           String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_EUDEMON);
            CsvFile csv = new CsvFile(text);
            

            GameStruct.EudemonInfo info;
            String v;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                info = new GameStruct.EudemonInfo();
                v = csv.GetFieldInfoToValue(i, "itemid");
                info.id = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i,"life_min");
                info.life_min = Convert.ToInt32(v);
               v = csv.GetFieldInfoToValue(i,"life_max");
                info.life_max = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i,"defense_min");
                info.defense_min = Convert.ToInt32(v);          ;
                 v = csv.GetFieldInfoToValue(i,"defense_max");
                info.defense_max = Convert.ToInt32(v);             ;
                v = csv.GetFieldInfoToValue(i,"magicdef_min");
                info.magicdef_min = Convert.ToInt32(v);          ;
                v = csv.GetFieldInfoToValue(i,"magicdef_max");
                info.magicdef_max = Convert.ToInt32(v);  
                v = csv.GetFieldInfoToValue(i,"atk_min_min");
                info.atk_min_min = Convert.ToInt32(v); ;
                v = csv.GetFieldInfoToValue(i, "atk_min_max");
                info.atk_min_max = Convert.ToInt32(v); ;
                v = csv.GetFieldInfoToValue(i,"atk_max_min");
                info.atk_max_min = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "atk_max_max");
                info.atk_max_max = Convert.ToInt32(v);     
                v = csv.GetFieldInfoToValue(i,"magicatk_min_min");
                info.magicatk_min_min = Convert.ToInt32(v); 
                v = csv.GetFieldInfoToValue(i,"magicatk_min_max");
                info.magicatk_min_max = Convert.ToInt32(v); 
                v = csv.GetFieldInfoToValue(i,"magicatk_max_min");
                info.magicatk_max_min = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "magicatk_max_max");
                info.magicatk_max_max = Convert.ToInt32(v);     
                v = csv.GetFieldInfoToValue(i,"life_grow_min");
                info.life_grow_min = Convert.ToSingle(v);     
                v = csv.GetFieldInfoToValue(i,"life_grow_max");
                info.life_grow_max = Convert.ToSingle(v);  
                v = csv.GetFieldInfoToValue(i,"defense_grow_min");
                info.defense_grow_min = Convert.ToSingle(v);     
                v = csv.GetFieldInfoToValue(i,"defense_grow_max");
                info.defense_grow_max = Convert.ToSingle(v);  
                v = csv.GetFieldInfoToValue(i,"magicdef_grow_min");
                info.magicdef_grow_min = Convert.ToSingle(v);     
                v = csv.GetFieldInfoToValue(i,"magicdef_grow_max");
                info.magicdef_grow_max = Convert.ToSingle(v);        
                v = csv.GetFieldInfoToValue(i,"atk_grow_min");
                info.atk_grow_min = Convert.ToSingle(v);     
                v = csv.GetFieldInfoToValue(i,"atk_grow_max");
                info.atk_grow_max = Convert.ToSingle(v);             
                v = csv.GetFieldInfoToValue(i,"magicatk_grow_min");
                info.magicatk_grow_min = Convert.ToSingle(v);
                v = csv.GetFieldInfoToValue(i, "magicatk_grow_max");
                info.magicatk_grow_max = Convert.ToSingle(v);
                v = csv.GetFieldInfoToValue(i, "quality_min");
                info.quality_min = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "quality_max");
                info.qulity_max = Convert.ToInt32(v);
                mDicEudemonInfo[info.id] = info;
            }
            return true;
          }
        
        public GameStruct.EudemonInfo GetEudemonInfo(uint id)
       {
           if (mDicEudemonInfo.ContainsKey(id))
           {
               return mDicEudemonInfo[id];
            }
           return null;
       }

        private bool LoadHairInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_HAIR);
            CsvFile csv = new CsvFile(text);

            GameStruct.HairInfo info;
            String v;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                info = new GameStruct.HairInfo();
                v = csv.GetFieldInfoToValue(i, "itemid");
                info.itemid = Convert.ToUInt32(v);
                info.name = csv.GetFieldInfoToValue(i, "name");
                v = csv.GetFieldInfoToValue(i, "hairid");
                info.hairid = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "price");
                info.price = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "sex");
                info.sex = Convert.ToByte(v);
                mDicHair[info.itemid] = info;
            }
            return true;
        }
        public GameStruct.HairInfo GetHairInfo(uint itemid)
        {
            if (mDicHair.ContainsKey(itemid))
            {
                return mDicHair[itemid];
            }
            return null;
        }
        private bool LoadLookFaceInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_LOOKFACE);
            CsvFile csv = new CsvFile(text);

            GameStruct.LookFaceInfo info;
            String v;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                info = new GameStruct.LookFaceInfo();
                v = csv.GetFieldInfoToValue(i, "itemid");
                info.itemid = Convert.ToUInt32(v);
                info.name = csv.GetFieldInfoToValue(i, "name");
                v = csv.GetFieldInfoToValue(i, "lookfaceid");
                info.lookfaceid = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "price");
                info.price = Convert.ToInt32(v);
                mDicLookFace[info.itemid] = info;
            }
            return true;
            
        }
        private bool LoadEudemonSoulInfo()
        {
            mDicEudemonSoul.Clear();
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_EUDEMON_SOUL);
            CsvFile csv = new CsvFile(text);
            GameStruct.EudemonSoulInfo info;
            String v;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                info = new GameStruct.EudemonSoulInfo();
                v = csv.GetFieldInfoToValue(i,"star");
                info.star = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i,"level");
                info.level = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i,"fu_level");
                info.fu_level = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i,"fu_star");
                info.fu_star = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i,"add_min");
                info.add_min = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i,"add_max");
                info.add_max = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i,"add_main");
                info.add_main = Convert.ToSingle(v);
                 v = csv.GetFieldInfoToValue(i,"add_fu");
                info.add_fu = Convert.ToSingle(v);
                v = csv.GetFieldInfoToValue(i,"add_init");
                info.add_init = Convert.ToInt32(v);
                 v = csv.GetFieldInfoToValue(i,"notice");
                info.bNotice = Convert.ToBoolean(v);
                mDicEudemonSoul[info.star] = info;
            }
            return true;
        }
        private bool LoadRobotInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_ROBOT);
            CsvFile csv = new CsvFile(text);

            GameStruct.RobotInfo info;
            String v;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                info = new GameStruct.RobotInfo();
                info.name = csv.GetFieldInfoToValue(i,"name");
                v = csv.GetFieldInfoToValue(i, "lookface");
                info.lookface = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "hair");
                info.hair = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "armor_id");
                info.armor_id = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "wepon_id");
                info.wepon_id = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "guanjue");
                info.guanjue = Convert.ToByte(v);
                v = csv.GetFieldInfoToValue(i, "rid_id");
                info.rid_id = Convert.ToUInt32(v);
                info.legion_name = csv.GetFieldInfoToValue(i, "legion_name");
                v = csv.GetFieldInfoToValue(i, "legion_place");
                info.legion_place = Convert.ToInt16(v);
                v = csv.GetFieldInfoToValue(i, "legion_title");
                info.legion_title = Convert.ToByte(v);
                v = csv.GetFieldInfoToValue(i, "map_id");
                info.map_id = Convert.ToUInt32(v);
                v = csv.GetFieldInfoToValue(i, "x");
                info.x = Convert.ToInt16(v);
                v = csv.GetFieldInfoToValue(i, "y");
                info.y = Convert.ToInt16(v);
                v = csv.GetFieldInfoToValue(i, "dir");
                info.dir = Convert.ToByte(v);
                mListRobotInfo.Add(info);

                //创建机器人军团
                if (info.legion_name.Length > 0)
                {
                    RobotLegionManager.GetInstance().CreateLegion(info.legion_name);
                }
                
            }
            //创建机器人
            for (int i = 0; i < mListRobotInfo.Count; i++)
            {
                info = mListRobotInfo[i];
                GameMap map = MapManager.Instance().GetGameMapToID(info.map_id);
                if (map != null)
                {
                    RobotObject obj = new RobotObject();
                    obj.SetRobotInfo(info);
                    map.AddObject(obj);
                }
            }
                return true;
        }

        public bool LoadAiInfo()
        {
            String text = mPacket.LoadFileToText(TextDefine.CONFIG_FILE_AI);
            CsvFile csv = new CsvFile(text);

            GameStruct.AiInfo info;
            String v;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                info = new GameStruct.AiInfo();
                v = csv.GetFieldInfoToValue(i, "id");
                info.nId = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "type");
                info.nType = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "range");
                info.nRange = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "attack_range");
                info.nAttack_Range = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "move_speed");
                info.nMove_Speed = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "attack_speed");
                info.nAttack_Speed = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "idle_move");
                info.bIdle_Move = Convert.ToBoolean(v);
                v = csv.GetFieldInfoToValue(i, "move");
                info.bMove = Convert.ToBoolean(v);
                if (mDicAiInfo.ContainsKey(info.nId))
                {
                    Log.Instance().WriteLog("存在相同的AIid" + info.nId.ToString());

                }
                if (info.nType != Define.MONSTER_TYPE_PASSIVE && info.nType != Define.MONSTER_TYPE_ACTIVE)
                {
                    Log.Instance().WriteLog("AI类型出错，已重置为被动类型,aiID:"+info.nId.ToString());
                    info.nType = Define.MONSTER_TYPE_PASSIVE;
                   
                }
                mDicAiInfo[info.nId] = info;
            }
            return true;
        }

        public GameStruct.EudemonSoulInfo GetEudemonSoulInfo(int nStar)
        {
            if(mDicEudemonSoul.ContainsKey(nStar))
            {
                return mDicEudemonSoul[nStar];
            }
            return null;
        }

        public GameStruct.AiInfo GetAIInfo(int nAi_id)
        {
            if (mDicAiInfo.ContainsKey(nAi_id))
            {
                return mDicAiInfo[nAi_id];
            }
            Log.Instance().WriteLog("未找到ai信息,重置为第一个AIid");
            return mDicAiInfo[1]; //默认第一个id
        }
        public GameStruct.LookFaceInfo GetLookFaceInfo(uint itemid)
        {
            if (mDicLookFace.ContainsKey(itemid))
            {
                return mDicLookFace[itemid];
            }
            return null;
        }

 
        //检测是否到达了传送点
        public bool CheckMapGate(uint mapid, short x, short y, 
            ref uint target_mapid, ref short target_x, ref short target_y)
        {
            if (mDicMapGate.ContainsKey(mapid))
            {
                List<GameStruct.MapGateInfo> list_info = mDicMapGate[mapid];
                for (int i = 0; i < list_info.Count; i++)
                {
                    if (list_info[i].src_mapid == mapid)
                    {
                        if (Math.Abs(list_info[i].src_x - x)<= list_info[i].dis &&
                            Math.Abs(list_info[i].src_y - y) <= list_info[i].dis)
                        {
                            target_x = list_info[i].target_x;
                            target_y = list_info[i].target_y;
                            target_mapid = list_info[i].target_mapid;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public GameStruct.TrackInfo GetTrackInfo(uint id)
        {
            if (mDicTrack.ContainsKey(id))
            {
                return mDicTrack[id];
            }
            return null;
        }
        //取连招动作数量
        public int GetTrackNumber(uint id)
        {
            int ret = 0;
            uint next_id = id;
            while (true)
            {
                GameStruct.TrackInfo info = GetTrackInfo(next_id);
                if (info == null) break;
                ret++;
                if (info.id_next == 0) break;
                next_id = info.id_next;
            }
            return ret;
        }
        //取连招锁定时间
        public int GetTrackTime(uint id)
        {
            int ret = 0;
            uint next_id = id;
            while (true)
            {
                GameStruct.TrackInfo info = GetTrackInfo(next_id);
                if (info == null) break;
                ret += info.apply_ms;
                if (info.id_next == 0) break;
                next_id = info.id_next;
            }
            return ret;
        }
        public GameStruct.LevelExp GetLevelExp(uint id, byte level)
        {
            if (mDicLevelExp.ContainsKey(id))
            {
                Dictionary<byte, GameStruct.LevelExp> info = mDicLevelExp[id];
                if (info.ContainsKey(level))
                {
                    return info[level];
                }
            }
            return null;
        }

        //取宝石信息
        public GameStruct.GemInfo GetGemInfo(uint itemid)
        {
            if (mDicGem.ContainsKey(itemid))
            {
                return mDicGem[itemid];
            }
            return null;
        }
        public GameStruct.BaseAttributeInfo GetAttributeInfo(byte profession, byte level)
        {
            if (mDicAttribute.ContainsKey(profession))
            {
                Dictionary<byte, GameStruct.BaseAttributeInfo> info = mDicAttribute[profession];
                if (info.ContainsKey(level))
                {
                    return info[level];
                    }
             }
            return null;
        }
        public VerPacket GetVerPacket()
        {
            return mPacket;
        }

        public GameStruct.NPCInfo GetNpcInfoToID(uint id)
        {
            if (mDicNpc.ContainsKey(id))
            {
                return mDicNpc[id];
            }
            return null;
        }

        public GameStruct.MonsterInfo GetMonsterInfo(uint id)
        {
            if (mDicMonster.ContainsKey(id))
            {
                return mDicMonster[id];
            }
            return null;
        }

        public GameStruct.ItemTypeInfo GetItemTypeInfo(uint id)
        {
            if (mDicItemType.ContainsKey(id))
            {
                return mDicItemType[id];
            }
            return null;
        }

        public GameStruct.MagicTypeInfo GetMagicTypeInfo(uint id, byte level = 0)
        {
            uint key = id * 10 + level;
            if (mDicMagicType.ContainsKey(key))
            {
                return mDicMagicType[key];
            }
            return null;
        }
        public GameStruct.ItemTypeInfo GetItemTypeInfo(String name)
        {
            foreach (GameStruct.ItemTypeInfo obj in mDicItemType.Values)
            {
                if (obj.name == name)
                {
                    return obj;
                }
            }
            return null;
        }

        public GameStruct.DropItemInfo GetDropItemInfo(uint groupid)
        {
            if (mDicDropItem.ContainsKey(groupid))
            {
                return mDicDropItem[groupid];
            }
            return null;
        }

        public void ReloadAllScripte()
        {
            ScripteManager.Instance().ClearAllScripte();
            //全局脚本
            LoadGolbalScript();
            //npc脚本
            foreach (GameStruct.NPCInfo info in mDicNpc.Values)
            {
                ScripteManager.Instance().LoadScripteFile(info.ScriptPath, true);
            }
        }
      
    }


  
    public class VerPacket
    {
        private const int VERSION = 3389;
        public int mnVer; //文件版本

        public List<String> mListName; //文件名
        public List<byte[]> mListData; //文件数据


        public String m_sPath;
        public bool isVer; //是否是版本包
      
        public VerPacket(String verpacketpath = "")
        {
            mListName = new List<String>();
            mListData = new List<byte[]>();
            isVer = false;
            m_sPath = verpacketpath;
            if (m_sPath.Length > 0)
            {
                isVer = true;
                if (!DownVerPack(m_sPath))
                {
                    Log.Instance().WriteLog("读取版本包出错...url:"+verpacketpath);
                }
            }
           
        }
        //读取文件包
        public void InitPacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader read = new BinaryReader(stream);
            int nVer = read.ReadInt32();
            if (nVer != VERSION)
            {
                Log.Instance().WriteLog("版本号不匹配!");
                return;
            }
            int nCount = read.ReadInt32();
            for (int i = 0; i < nCount; i++)
            {
                byte[] name_data = read.ReadBytes(128);
                byte[] name_data1 = null;
                for (int k = 0; k < name_data.Length; k++)
                {
                    if (name_data[k] == 0)
                    {
                        name_data1 = new byte[k];
                        Buffer.BlockCopy(name_data, 0, name_data1, 0, k);
                        break;
                    }
                }
                String sFileName = GameBase.Core.Coding.GetDefauleCoding().GetString(name_data1);
                int filesize = read.ReadInt32();
                byte[] filedata = read.ReadBytes(filesize);
                mListName.Add(sFileName);
                mListData.Add(filedata);
            }
        }
     
        public byte[] LoadFileToBytes(String file)
        {
            if (isVer)
            {
                for (int i = 0; i < mListName.Count; i++)
                {
                    if (mListName[i] == file)
                    {
                        return mListData[i];
                    }
                }
               
            }
            else
            {
                FileStream f = new FileStream(file, FileMode.Open);
                if(f.Length > 0)
                {
                    byte[] ret = new byte[f.Length];
                    f.Read(ret,0,(int)f.Length);
                    f.Close();
                    return ret;
                }
                
            }
            return null;
        }

        public String LoadFileToText(String file)
        {
            if (isVer)
            {
                for (int i = 0; i < mListName.Count; i++)
                {
                    if (mListName[i] == file)
                    {
                        return GameBase.Core.Coding.GetDefauleCoding().GetString(mListData[i]);
                    }
                }
            }
            else
            {
                if (!File.Exists(file))
                {
                    Log.Instance().WriteLog("载入文件失败" + file); 
                    return "";
                }
                FileStream f = new FileStream(file, FileMode.Open);
                if (f.Length > 0)
                {
                    byte[] ret = new byte[f.Length];
                    f.Read(ret, 0, (int)f.Length);
                    f.Close();
                    return GameBase.Core.Coding.GetDefauleCoding().GetString(ret);
                   
                }
                f.Dispose();
            }
            return "";
        }

        //下载游戏版本包
        public bool DownVerPack(String url)
        {
            return true;
        }

        
    }
}
