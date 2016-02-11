using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Network.Internal;
using GameStruct;
using GameBase.Config;
using GameBase.Network;
using GameBase.Core;

//幻兽系统--
//2015.9.27 今天中秋节哦

namespace MapServer
{
    public class PlayerEudemon
    {
        public const int MAX_EUDEMON_COUNT = 12;//最多幻兽数量
        PlayerObject play;
        private Dictionary<uint, RoleData_Eudemon> mDicEudemon;
        private List<RoleData_Eudemon> mTempDicEudemon; //临时的幻兽信息表- 用于存储地图丢弃的幻兽
        private List<EudemonObject> mBattleObj; //出征或者合体的幻兽
        private List<uint> mListRecordEudemon;
        private List<EudemonObject> mListObj; //背包幻兽的实例对象

        private uint mCurEudemonSoulId; //当前幻化主幻兽id
        public void SetSoulEudemon(uint id) { mCurEudemonSoulId = id; }
        public PlayerEudemon(PlayerObject _play)
        {
            play = _play;
            mDicEudemon = new Dictionary<uint, RoleData_Eudemon>();
            mTempDicEudemon = new List<RoleData_Eudemon>();
            mBattleObj = new List<EudemonObject>();
            mListObj = new List<EudemonObject>();
            mListRecordEudemon = new List<uint>();
        }

        public bool IsEudemonFull()
        {
            return mDicEudemon.Count >= MAX_EUDEMON_COUNT;
        }
        public RoleData_Eudemon FindEudemon(uint eudemon_id)
        {
            if (mDicEudemon.ContainsKey(eudemon_id))
            {
                return mDicEudemon[eudemon_id];
            }
            return null;
        }

        public void DeleteEudemon(uint eudemon_id)
        {
            RoleData_Eudemon eudemon = null;
            if (mDicEudemon.ContainsKey(eudemon_id))
            {
                eudemon = mDicEudemon[eudemon_id];
               
                //PacketOut outpack = null;
                mDicEudemon.Remove(eudemon_id);
                //实例对象也移除
                for (int i = 0; i < mListObj.Count; i++)
                {
                    if (mListObj[i].GetTypeId() == eudemon_id)
                    {
                        mListObj.RemoveAt(i);
                        break;
                    }
                }

       
            }

            // 收到网络协议:长度：12协议号:1015
            //{12,0,247,3,117,251,72,119,89,2,0,0}
            PacketOut outpack = new PacketOut();
            outpack.WriteUInt16(12);
            outpack.WriteUInt16(1015);
            outpack.WriteUInt32(eudemon_id);
            outpack.WriteInt32(601);
            play.SendData(outpack.Flush(), true);
            //收到网络协议:长度：76协议号:1040
            byte[] data1 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            outpack = new PacketOut();
            outpack.WriteInt16(76);
            outpack.WriteInt16(1040);
            outpack.WriteInt32(0);
            outpack.WriteInt16(5);
            outpack.WriteInt16(1);
            outpack.WriteUInt32(eudemon_id);
            outpack.WriteBuff(data1);
            play.SendData(outpack.Flush(), true);

            //刷新幻兽界面...经过测试 这些封包并没有什么卵用，幻兽界面还是有这个幻兽，后面再说吧。。2015.11.6
            //// 收到网络协议:长度：76协议号:1040
            //2015.11.9 已解决..是因为这里删掉了幻兽..后面调用playeritem的clearitem 找不到id了。。。
            //就在这里发清除道具的信息
            NetMsg.MsgClearItem clear = new NetMsg.MsgClearItem();
            clear.id = eudemon_id;
            clear.roleid = play.GetTypeId();
            play.SendData(clear.GetBuffer(), true);
        }
        public void AddTempEudemon(RoleData_Eudemon eudemon)
        {
            mTempDicEudemon.Add(eudemon);
        }
        public RoleData_Eudemon FindTempEudemon(uint eudemon_typeid)
        {
            for (int i = 0; i < mTempDicEudemon.Count; i++)
            {
                if (mTempDicEudemon[i].typeid == eudemon_typeid)
                {
                    return mTempDicEudemon[i];
                }
            }
            return null;
        }
        public void DeleteTempEudemon(uint eudemon_typeid)
        {
            for (int i = 0; i < mTempDicEudemon.Count; i++)
            {
                if (mTempDicEudemon[i].typeid == eudemon_typeid)
                {
                    mTempDicEudemon.RemoveAt(i);
                    break;
                }
            }
        }
        public void AddEudemon(RoleData_Eudemon eudemon)
        {
            mDicEudemon[eudemon.GetTypeID()] = eudemon;
            EudemonObject obj = new EudemonObject(eudemon, play);
            obj.CalcAttribute();
            mListObj.Add(obj);
            SendEudemonInfo(eudemon);
        }
        //增加幻兽
        public void AddEudemon(GameStruct.RoleItemInfo item, byte level = 1, int quality = 0,byte wuxing = 0)
        {
            //创建进化后的幻兽处理-- 因为幻兽进化配置表只配置了初始幻兽的id
            String sItemID = item.itemid.ToString();
            uint nItemId = item.itemid;
            if (sItemID.Substring(sItemID.Length-1, 1) != "0")
            {
                sItemID = sItemID.Substring(0, sItemID.Length - 1) + "0";
                nItemId = Convert.ToUInt32(sItemID);
            }

            GameStruct.EudemonInfo info = ConfigManager.Instance().GetEudemonInfo(nItemId);
            
            if (info == null)
            {
                Log.Instance().WriteLog("创建幻兽失败,不存在的幻兽id:" + item.id.ToString());
                return;
            }
            if (mDicEudemon.ContainsKey(item.itemid))
            {
                Log.Instance().WriteLog("创建幻兽失败,重复的幻兽id:" + item.id.ToString());
                return;
            }
            GameStruct.ItemTypeInfo itembaseinfo = ConfigManager.Instance().GetItemTypeInfo(item.itemid);
            if (itembaseinfo == null)
            {
                Log.Instance().WriteLog("创建幻兽失败,找不到基础物品id:" + item.itemid.ToString());
                return;
            }
            RoleData_Eudemon data = new RoleData_Eudemon();
            data.id = 0;
            data.itemid = item.id;
            data.phyatk_grow_rate = IRandom.Random(0.5f, info.atk_grow_min);
            data.phyatk_grow_rate_max = IRandom.Random(info.atk_grow_min, info.atk_grow_max);
            data.magicatk_grow_rate = IRandom.Random(0.5f, info.magicatk_grow_min);
            data.magicatk_grow_rate_max = IRandom.Random(info.magicatk_grow_min, info.magicatk_grow_max);
            data.life_grow_rate = IRandom.Random(info.life_grow_min, info.life_grow_max);
            data.defense_grow_rate = IRandom.Random(info.defense_grow_min, info.defense_grow_max);
            data.magicdef_grow_rate = IRandom.Random(info.magicdef_grow_min, info.magicdef_grow_max);
            data.init_life = IRandom.Random(info.life_min, info.life_max);
            data.init_atk_min = IRandom.Random(info.atk_min_min, info.atk_min_max);
            data.init_atk_max = IRandom.Random(info.atk_max_min, info.atk_max_max);
            data.init_defense = IRandom.Random(info.defense_min, info.defense_max);
            data.init_magicdef = IRandom.Random(info.magicdef_min, info.magicdef_max);
            data.init_magicatk_min = IRandom.Random(info.magicatk_min_min, info.magicatk_min_max);
            data.init_magicatk_max = IRandom.Random(info.magicatk_max_min, info.magicatk_max_max);
            data.luck = IRandom.Random(1, 100); //幸运值
            data.intimacy = 150; //亲密度
            data.level = level; //等级
            //data.card = IDManager.CreateEudemonCard(); //创建身份牌号码
            data.card = 0;              //身份牌是二次进化的时候才获得-
            data.exp = 0;
            data.quality = quality; //品质在第一次进化的时候才出现
            if (wuxing == 0)
            {
                data.wuxing = IDManager.GetEudemonWuxing();
            }
            else
            {
                data.wuxing = wuxing;
            }
           
            data.name = itembaseinfo.name;
            data.typeid = item.typeid;
            mDicEudemon[data.GetTypeID()] = data;
            EudemonObject obj = new EudemonObject(data, play);
            obj.CalcAttribute();
            mListObj.Add(obj);
            SendEudemonInfo(data);


        }

        //数据库读取幻兽
        public void DB_Load(ROLEDATE_EUDEMON data)
        {
            for (int i = 0; i < data.list_item.Count; i++)
            {
                RoleData_Eudemon info = data.list_item[i];

                GameStruct.RoleItemInfo itemdata = play.GetItemSystem().FindItem(info.itemid);
                if (itemdata != null)
                {
                    info.typeid = itemdata.typeid;
                   
                    mDicEudemon[info.GetTypeID()] = info;
                    //创建幻兽实例对象-
                    EudemonObject obj = new EudemonObject(info, play);
                    obj.CalcAttribute();


               
                    mListObj.Add(obj);
                }

            }
            //死亡幻兽删掉
           
            play.GetItemSystem().Process_DieEudemon();
   
        }

      
        //保存幻兽数据
        public void DB_Save()
        {
            GameBase.Network.Internal.ROLEDATE_EUDEMON info = new GameBase.Network.Internal.ROLEDATE_EUDEMON();
            info.SetSaveTag();
            info.playerid = play.GetBaseAttr().player_id;
            foreach (RoleData_Eudemon obj in mDicEudemon.Values)
            {
                info.list_item.Add(obj);
            }
            DBServer.Instance().GetDBClient().SendData(info.GetBuffer());
        }

        //下发的客户端成长率要转换成整数型
        public static int ConvertGrowRate(float fValue)
        {
            int nValue = (int)(fValue * 1000);
            String s = nValue.ToString();
            if (s.Length > 4)
            {
                s.Substring(0, 3);
                nValue = Convert.ToInt32(s);
            }
            return nValue;
            
        }
        //发送幻兽信息到交易栏
         public void SendLookTradEudemonInfo(PlayerObject _play, RoleData_Eudemon info)
        {
            NetMsg.MsgEudemonInfo msg = new NetMsg.MsgEudemonInfo();

            msg.id = info.GetTypeID();
            msg.tag = 4;
            msg.AddAttribute(GameStruct.EudemonAttribute.Atk_Max, info.atk_max);
            msg.AddAttribute(GameStruct.EudemonAttribute.Atk_Min, info.atk_min);
            msg.AddAttribute(GameStruct.EudemonAttribute.MagicAtk_Max, info.magicatk_max);
            msg.AddAttribute(GameStruct.EudemonAttribute.MagicAtk_Min, info.magicatk_min);
            msg.AddAttribute(GameStruct.EudemonAttribute.Defense, info.defense);
            msg.AddAttribute(GameStruct.EudemonAttribute.Magic_Defense, info.magicdef);
            msg.AddAttribute(GameStruct.EudemonAttribute.Life, info.life);
            msg.AddAttribute(GameStruct.EudemonAttribute.Life_Max, info.life_max);
            msg.AddAttribute(GameStruct.EudemonAttribute.Intimacy, info.intimacy);
            msg.AddAttribute(GameStruct.EudemonAttribute.Level, info.level);
            msg.AddAttribute(GameStruct.EudemonAttribute.WuXing, info.wuxing);
            msg.AddAttribute(GameStruct.EudemonAttribute.Luck, info.luck);
            msg.AddAttribute(GameStruct.EudemonAttribute.Recall_Count, info.recall_count);
            msg.AddAttribute(EudemonAttribute.Card, info.card);
            msg.AddAttribute(EudemonAttribute.Exp, info.exp);
            msg.AddAttribute(EudemonAttribute.Quality, info.quality);
            msg.AddAttribute(EudemonAttribute.Init_Atk, info.GetInitAtk());
            msg.AddAttribute(EudemonAttribute.Init_Magic_Atk, info.GetInitMagicAtk());
            msg.AddAttribute(EudemonAttribute.Init_Defense, info.GetInitDefense());
            msg.AddAttribute(EudemonAttribute.Init_Life, info.init_life);
            msg.AddAttribute(EudemonAttribute.Life_Grow_Rate, ConvertGrowRate(info.life_grow_rate));
            msg.AddAttribute(EudemonAttribute.Atk_Min_Grow_Rate, ConvertGrowRate(info.phyatk_grow_rate));
            msg.AddAttribute(EudemonAttribute.Atk_Max_Grow_Rate, ConvertGrowRate(info.phyatk_grow_rate_max));
            msg.AddAttribute(EudemonAttribute.MagicAtk_Min_Grow_Rate, ConvertGrowRate(info.magicatk_grow_rate));
            msg.AddAttribute(EudemonAttribute.MagicAtk_Max_Grow_Rate, ConvertGrowRate(info.magicatk_grow_rate_max));
            msg.AddAttribute(EudemonAttribute.Defense_Grow_Rate, ConvertGrowRate(info.defense_grow_rate));
            msg.AddAttribute(EudemonAttribute.MagicDefense_Grow_Rate, ConvertGrowRate(info.magicdef_grow_rate));

            GameStruct.MonsterInfo _info = EudemonObject.GetMonsterInfo(play, info.itemid);
            if (_info != null)
            {
                msg.AddAttribute(EudemonAttribute.Riding, _info.eudemon_type);
            }

            _play.SendData(msg.GetBuffer(), true);
        }
        //发送幻兽信息到摊位
        public void SendLookPtichEudemonInfo(PlayerObject _play, RoleData_Eudemon info)
        {
            NetMsg.MsgEudemonInfo msg = new NetMsg.MsgEudemonInfo();

            msg.id = info.GetTypeID();
            msg.tag = 3;
            msg.AddAttribute(GameStruct.EudemonAttribute.Atk_Max, info.atk_max);
            msg.AddAttribute(GameStruct.EudemonAttribute.Atk_Min, info.atk_min);
            msg.AddAttribute(GameStruct.EudemonAttribute.MagicAtk_Max, info.magicatk_max);
            msg.AddAttribute(GameStruct.EudemonAttribute.MagicAtk_Min, info.magicatk_min);
            msg.AddAttribute(GameStruct.EudemonAttribute.Defense, info.defense);
            msg.AddAttribute(GameStruct.EudemonAttribute.Magic_Defense, info.magicdef);
            msg.AddAttribute(GameStruct.EudemonAttribute.Life, info.life);
            msg.AddAttribute(GameStruct.EudemonAttribute.Life_Max, info.life_max);
            msg.AddAttribute(GameStruct.EudemonAttribute.Intimacy, info.intimacy);
            msg.AddAttribute(GameStruct.EudemonAttribute.Level, info.level);
            msg.AddAttribute(GameStruct.EudemonAttribute.WuXing, info.wuxing);
            msg.AddAttribute(GameStruct.EudemonAttribute.Luck, info.luck);
            msg.AddAttribute(GameStruct.EudemonAttribute.Recall_Count, info.recall_count);
            msg.AddAttribute(EudemonAttribute.Card, info.card);
            msg.AddAttribute(EudemonAttribute.Exp, info.exp);
            msg.AddAttribute(EudemonAttribute.Quality, info.quality);
            msg.AddAttribute(EudemonAttribute.Init_Atk, info.GetInitAtk());
            msg.AddAttribute(EudemonAttribute.Init_Magic_Atk, info.GetInitMagicAtk());
            msg.AddAttribute(EudemonAttribute.Init_Defense, info.GetInitDefense());
            msg.AddAttribute(EudemonAttribute.Init_Life, info.init_life);
            msg.AddAttribute(EudemonAttribute.Life_Grow_Rate, ConvertGrowRate(info.life_grow_rate));
            msg.AddAttribute(EudemonAttribute.Atk_Min_Grow_Rate, ConvertGrowRate(info.phyatk_grow_rate));
            msg.AddAttribute(EudemonAttribute.Atk_Max_Grow_Rate, ConvertGrowRate(info.phyatk_grow_rate_max));
            msg.AddAttribute(EudemonAttribute.MagicAtk_Min_Grow_Rate, ConvertGrowRate(info.magicatk_grow_rate));
            msg.AddAttribute(EudemonAttribute.MagicAtk_Max_Grow_Rate, ConvertGrowRate(info.magicatk_grow_rate_max));
            msg.AddAttribute(EudemonAttribute.Defense_Grow_Rate, ConvertGrowRate(info.defense_grow_rate));
            msg.AddAttribute(EudemonAttribute.MagicDefense_Grow_Rate, ConvertGrowRate(info.magicdef_grow_rate));

            GameStruct.MonsterInfo _info = EudemonObject.GetMonsterInfo(play, info.itemid);
            if (_info != null)
            {
                msg.AddAttribute(EudemonAttribute.Riding, _info.eudemon_type);
            }

            _play.SendData(msg.GetBuffer(), true);
        }
        //发送幻兽信息
        //info 幻兽信息
        //tag 是否发送休息标记
        //brank 是否发送排行榜信息
        public void SendEudemonInfo(RoleData_Eudemon info, bool tag = true,bool bRank = true)
        {
           

            if (tag)
            {
                //要设置一个标记才可以出征，，目前不知道这个消息是干什么用的为何 应该是状态之类的吧。。。(*^__^*) 嘻2015.9.28
                NetMsg.MsgEudemonTag eudemontag = new NetMsg.MsgEudemonTag();
              
                eudemontag.playerid = play.GetTypeId();
                eudemontag.eudemonid = info.GetTypeID();
                eudemontag.SetBreakTag();
                play.SendData(eudemontag.GetBuffer(), true);
            }

            //所有幻兽都为至尊圣兽
            if (bRank && info.quality > 0)
            {
                byte[] data1 = { 12, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 37, 38, 0, 0 };
                PacketOut outpack = new PacketOut();
                outpack.WriteInt16(28);
                outpack.WriteInt16(1010);
                outpack.WriteUInt32(info.typeid);
                outpack.WriteUInt32(play.GetTypeId());
                outpack.WriteBuff(data1);
                play.SendData(outpack.Flush(),true);
            }
            NetMsg.MsgEudemonInfo msg = new NetMsg.MsgEudemonInfo();
          
            msg.id = info.GetTypeID();
            msg.AddAttribute(GameStruct.EudemonAttribute.Atk_Max, info.atk_max);
            msg.AddAttribute(GameStruct.EudemonAttribute.Atk_Min, info.atk_min);
            msg.AddAttribute(GameStruct.EudemonAttribute.MagicAtk_Max, info.magicatk_max);
            msg.AddAttribute(GameStruct.EudemonAttribute.MagicAtk_Min, info.magicatk_min);
            msg.AddAttribute(GameStruct.EudemonAttribute.Defense, info.defense);
            msg.AddAttribute(GameStruct.EudemonAttribute.Magic_Defense, info.magicdef);
            msg.AddAttribute(GameStruct.EudemonAttribute.Life, info.life);
            msg.AddAttribute(GameStruct.EudemonAttribute.Life_Max, info.life_max);
            msg.AddAttribute(GameStruct.EudemonAttribute.Intimacy, info.intimacy);
            msg.AddAttribute(GameStruct.EudemonAttribute.Level, info.level);
            msg.AddAttribute(GameStruct.EudemonAttribute.WuXing, info.wuxing);
            msg.AddAttribute(GameStruct.EudemonAttribute.Luck, info.luck);
            msg.AddAttribute(GameStruct.EudemonAttribute.Recall_Count, info.recall_count);
            msg.AddAttribute(EudemonAttribute.Card, info.card);
            msg.AddAttribute(EudemonAttribute.Exp, info.exp);
            msg.AddAttribute(EudemonAttribute.Quality, info.quality);
            msg.AddAttribute(EudemonAttribute.Init_Atk, info.GetInitAtk());
            msg.AddAttribute(EudemonAttribute.Init_Magic_Atk, info.GetInitMagicAtk());
            msg.AddAttribute(EudemonAttribute.Init_Defense, info.GetInitDefense());
            msg.AddAttribute(EudemonAttribute.Init_Life, info.init_life);
            msg.AddAttribute(EudemonAttribute.Life_Grow_Rate, ConvertGrowRate(info.life_grow_rate));
            msg.AddAttribute(EudemonAttribute.Atk_Min_Grow_Rate, ConvertGrowRate(info.phyatk_grow_rate));
            msg.AddAttribute(EudemonAttribute.Atk_Max_Grow_Rate, ConvertGrowRate(info.phyatk_grow_rate_max ));
            msg.AddAttribute(EudemonAttribute.MagicAtk_Min_Grow_Rate, ConvertGrowRate(info.magicatk_grow_rate));
            msg.AddAttribute(EudemonAttribute.MagicAtk_Max_Grow_Rate, ConvertGrowRate(info.magicatk_grow_rate_max));
            msg.AddAttribute(EudemonAttribute.Defense_Grow_Rate, ConvertGrowRate(info.defense_grow_rate ));
            msg.AddAttribute(EudemonAttribute.MagicDefense_Grow_Rate, ConvertGrowRate(info.magicdef_grow_rate));
  
            GameStruct.MonsterInfo _info = EudemonObject.GetMonsterInfo(play, info.itemid);
            if (_info != null)
            {
                msg.AddAttribute(EudemonAttribute.Riding, _info.eudemon_type);
            }
           
            play.SendData(msg.GetBuffer(),true);
       
        
  
        }
        //发送所有幻兽信息
        public void SendAllEudemonInfo()
        {
            foreach (RoleData_Eudemon obj in mDicEudemon.Values)
            {
                SendEudemonInfo(obj);
                EudemonObject eudemon_obj = GetEudmeonObject(obj.GetTypeID());
                if (eudemon_obj != null)
                {
                    eudemon_obj.SendMagicInfo();
                }
            }
        }
        public EudemonObject GetEudmeonObject(uint eudemon_id)
        {
            for (int i = 0; i < mListObj.Count; i++)
            {
                if (mListObj[i].GetTypeId() == eudemon_id)
                {
                    return mListObj[i];
                }
            }
            return null;
        }
        
        public EudemonObject GetBattleEudemonSystem(byte nIndex)
        {
            if (nIndex >= mBattleObj.Count) return null;
            return mBattleObj[nIndex];
        }
        //获取出战的幻兽对象
        public EudemonObject GetBattleEudemon(uint eudemon_id)
        {
            for (int i = 0; i < mBattleObj.Count; i++)
            {
                if (mBattleObj[i].GetEudemonInfo().GetTypeID() == eudemon_id)
                {
                    return mBattleObj[i];
                }
            }
            return null;
        }
        public uint GetEudemonTypeID(uint itemid)
        {
            foreach (RoleData_Eudemon obj in mDicEudemon.Values)
            {
                if (obj.itemid == itemid)
                {
                    return obj.GetTypeID();
                }
            }
            return 0;
        }


        public void Move(NetMsg.MsgMoveInfo moveinfo)
        {
            EudemonObject obj = GetBattleEudemon(moveinfo.id);
            if (obj == null) return;
            obj.Move(moveinfo);
        }

        //玩家退出游戏后，也要销毁幻兽对象
        public void ExitGame()
        {
            for (int i = 0; i < mBattleObj.Count; i++)
            {
                if (mBattleObj[i].GetState() == EUDEMONSTATE.BATTLE)
                {
                    play.GetGameMap().RemoveObj(mBattleObj[i]);
                }
            }
            mBattleObj.Clear();
        }

        //解体所有合体幻兽
        public void Eudemon_BreakUpAll()
        {
            int nCount = mBattleObj.Count;
            while (nCount > 0)
            {
                nCount--;
                if (mBattleObj[nCount].GetState() == EUDEMONSTATE.FIT)
                {

                    Eudemon_BreakUp(mBattleObj[nCount].GetTypeId());

                }
            }
        }
        //召回所有出征幻兽-  
        //isRecord 是否记录- 用于传送记录下ID 然后再出征
        public void Eudemon_ReCallAll(bool isRecord= false)
        {
            if (isRecord)
            {
                mListRecordEudemon.Clear();
            }
            int nCount = mBattleObj.Count;
            while (nCount > 0)
            {
                nCount--;
                if (mBattleObj[nCount].GetState() == EUDEMONSTATE.BATTLE)
                {
                    if (isRecord)
                    {
                        mListRecordEudemon.Add(mBattleObj[nCount].GetTypeId());
                    }
                    Eudemon_ReCall(mBattleObj[nCount].GetTypeId());
                    
                }
            }
           
        }
        //出征之前召回的记录幻兽
        public void Eudemon_BattleAll()
        {
            for (int i = 0; i < mListRecordEudemon.Count; i++)
            {
                play.GetEudemonSystem().Eudemon_Battle(mListRecordEudemon[i]);
            }
        }
        //幻兽召回- 
        public void Eudemon_ReCall(uint eudemon_id)
        {
          
          EudemonObject obj = GetBattleEudemon(eudemon_id);
          if (obj == null)
          {
              return;
          }
            mBattleObj.Remove(obj);
            obj.GetGameMap().RemoveObj(obj);
        }
        //幻兽合体
        public void Eudemon_Fit(uint eudemon_id)
        {
            EudemonObject obj = GetBattleEudemon(eudemon_id);
            if (obj == null) return;
            obj.SetState(EUDEMONSTATE.FIT);
           
          //  mBattleObj.Remove(obj);
            //长度：28协议号:1009
            byte[] data1 = { 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            PacketOut outpack = new PacketOut();
            outpack.WriteUInt16(28);
            outpack.WriteUInt16(1009);
            outpack.WriteUInt32(obj.GetTypeId());
            outpack.WriteUInt32(obj.GetTypeId());
            outpack.WriteBuff(data1);
            play.BroadcastBuffer(outpack.Flush(), true);
            
            //收到网络协议:长度：28协议号:1010
            byte[] data2 = {  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 73, 37, 0, 0 };
            outpack = new PacketOut();
            outpack.WriteUInt16(28);
            outpack.WriteUInt16(1010);
            outpack.WriteInt32(System.Environment.TickCount);
            outpack.WriteUInt32(obj.GetTypeId());
            outpack.WriteBuff(data2);
            play.BroadcastBuffer(outpack.Flush(), true);
            //收到网络协议:长度：28协议号:1010
            //byte[] data3 = { 28, 0, 242, 3, 143, 246, 58, 86, 0, 148, 53, 119, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 73, 37, 0, 0 };
            //play.SendData(data3, true);
            //收到网络协议:长度：32协议号:2037
            //幻兽最大血量- 与最小血量

            //byte[] data4 = { 2, 0, 0, 0, 6, 0, 0, 0, 228, 0, 0, 0, 7, 0, 0, 0, 228, 0, 0, 0 };
            outpack = new PacketOut();
            outpack.WriteUInt16(32);
            outpack.WriteUInt16(2037);
            outpack.WriteUInt32(1);
            outpack.WriteUInt32(obj.GetTypeId());
            outpack.WriteInt32(2);
            outpack.WriteInt32(6);
            outpack.WriteInt32(obj.GetAttr().life);
            outpack.WriteInt32(7);
            outpack.WriteInt32(obj.GetAttr().life_max);
            play.SendData(outpack.Flush(), true);
           //play.BroadcastBuffer(outpack.Flush(), true);
            //收到网络协议:长度：28协议号:1009
            byte[] data5 = { 255, 0, 0, 0, 35, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            outpack = new PacketOut();
            outpack.WriteUInt16(28);
            outpack.WriteUInt16(1009);
            outpack.WriteUInt32(obj.GetTypeId());
            outpack.WriteBuff(data5);
            play.BroadcastBuffer(outpack.Flush(), true);



            //下面这个好像无用
            //收到网络协议:长度：32协议号:2037
            //byte[] data6 = { 32, 0, 245, 7, 1, 0, 0, 0, 0, 148, 53, 119, 2, 0, 0, 0, 6, 0, 0, 0, 228, 0, 0, 0, 7, 0, 0, 0, 228, 0, 0, 0 };
            //play.SendData(data6, true);
            ////收到网络协议:长度：28协议号:1010
            //byte[] data7 = { 28, 0, 242, 30, 148, 53, 119, 86, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 60, 38, 0, 0 };
            //play.SendData(data7, true);
            ////收到网络协议:长度：16协议号:1012
            //byte[] data8 = { 16, 0, 244, 3, 86, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0 };
            //play.SendData(data8, true);

        }
        //幻兽解体
        public void Eudemon_BreakUp(uint eudemon_id)
        {
            EudemonObject obj = GetBattleEudemon(eudemon_id);
            if (obj == null) return;
            if (obj.GetState() != EUDEMONSTATE.FIT) return;
            mBattleObj.Remove(obj);
            //this.Eudemon_ReCall(eudemon_id);
            //  收到网络协议:长度：28协议号:1009
            byte[] data1 = { 255, 0, 0, 0, 36, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            PacketOut outpack = new PacketOut();
            outpack.WriteUInt16(28);
            outpack.WriteUInt16(1009);
            outpack.WriteUInt32(obj.GetTypeId());
            outpack.WriteBuff(data1);
            play.SendData(outpack.Flush(), true);
            //收到网络协议:长度：32协议号:2037
            byte[] data2 = {  2, 0, 0, 0, 6, 0, 0, 0, 228, 0, 0, 0, 7, 0, 0, 0, 228, 0, 0, 0 };
            outpack = new PacketOut();
            outpack.WriteUInt16(32);
            outpack.WriteUInt16(2037);
            outpack.WriteUInt32(1);
            outpack.WriteUInt32(obj.GetTypeId());
            outpack.WriteBuff(data2);
            play.SendData(outpack.Flush(), true);
            //收到网络协议:长度：16协议号:1012
           

            outpack = new PacketOut();
            outpack.WriteUInt16(16);
            outpack.WriteUInt16(1012);
            outpack.WriteUInt32(play.GetTypeId());
            outpack.WriteInt32(0);
            outpack.WriteInt32(0);
            play.SendData(outpack.Flush(), true);

            obj.SetState(EUDEMONSTATE.NROMAL);
        }
        //进化
        public void Eudemon_Evolution(uint eudemon_id)
        {
            EudemonObject obj = GetBattleEudemon(eudemon_id);
            //要出征才能进化
            if (obj == null) return;
           
            if (obj.GetState() != EUDEMONSTATE.BATTLE) return;
            GameStruct.RoleItemInfo role_item = play.GetItemSystem().FindItem(obj.GetEudemonInfo().itemid);
            RoleData_Eudemon eudemon_info = this.FindEudemon(eudemon_id);
            if (role_item == null || eudemon_info == null) return;
      
            //最多进化二次哦
            String sEvolution = role_item.itemid.ToString();
            PacketOut outpack = null;
            int nEvolutionCount = Convert.ToInt32(sEvolution.Substring(sEvolution.Length - 1));
            if (nEvolutionCount >= GameBase.Config.Define.EUDEMON_EVOLUTION_COUNT) return;
           GameStruct.ItemTypeInfo follow_info = ConfigManager.Instance().GetItemTypeInfo(role_item.itemid+1);
           if (follow_info == null)
           {
               play.MsgBox("进化失败.1");
               uint normal_id = role_item.itemid + 1;
               Log.Instance().WriteLog("进化失败，未找到道具ID:" + normal_id.ToString());
               return;
            }
           GameStruct.MonsterInfo monster_info = ConfigManager.Instance().GetMonsterInfo(follow_info.monster_type);
            if (monster_info == null)
            {
                play.MsgBox("进化失败2.");
                Log.Instance().WriteLog("进化失败，未找到怪物类型:" + follow_info.monster_type.ToString());
                return;
            }
            if (nEvolutionCount == 0) //第一次进化是二十级
            {
                if (obj.GetEudemonInfo().level < GameBase.Config.Define.EUDEMON_EVOLUTION_ONE) return;
              
                if (obj.GetEudemonInfo().quality == 0)
                {
                    GameStruct.EudemonInfo base_eudemon_info = ConfigManager.Instance().GetEudemonInfo(role_item.itemid);
                    int nQuality = GameBase.Config.Define.EUDEMON_NORMAL_QUALITY; //默认极品十星
                    if (base_eudemon_info != null)
                    {
                        nQuality = IRandom.Random(base_eudemon_info.quality_min, base_eudemon_info.qulity_max);
                    }
                    eudemon_info.quality = nQuality;
                   // obj.GetEudemonInfo().quality = nQuality;

                }
                role_item.itemid++; //增加id
            }
            else //第二次进化
            {
                if (obj.GetEudemonInfo().level < GameBase.Config.Define.EUDEMON_EVOLUTION_TWO) return;
                role_item.itemid++; //增加id
                eudemon_info.card = IDManager.CreateEudemonCard(); //创建身份牌号码
             //   obj.GetEudemonInfo().card =IDManager.CreateEudemonCard(); //创建身份牌号码
            }
            obj.SetEudemonInfo(eudemon_info);
            //更新道具与幻兽信息
            play.GetItemSystem().UpdateItemInfo(role_item.id);
            obj.SetMosterInfo(monster_info) ;
           
            //---更改幻兽模型----
            outpack = new PacketOut();
            outpack.WriteInt16(24);
            outpack.WriteInt16(2035);
            outpack.WriteUInt32(obj.GetTypeId());
            outpack.WriteUInt32(obj.GetMonsterInfo().lookface);
            outpack.WriteInt32(2);
            outpack.WriteInt16(obj.GetCurrentX());
            outpack.WriteInt16(obj.GetCurrentY());
            outpack.WriteUInt32(obj.GetTypeId());
            obj.BrocatBuffer(outpack.Flush());


            this.SendEudemonInfo(obj.GetEudemonInfo(),false);
            //幻兽排行榜
            //收到网络协议:长度：28协议号:1010

            //幻兽总排行榜          12
            //幻兽/神兽星级榜       13 
            //初始生命排行          14
            //生命成长排行          15
            //初始最小物攻排行      16
            //初始最小物攻排行      17
            //最小物攻成长排行      18
            //初始最大物攻排行      19
            //最大物攻成长排行      20
            //初始物防              21
            //物防成长排行          22   
            //初始最小魔攻排行      23
            //最小魔攻成长排行      24
            //初始最大魔攻排行      25
            //最大魔攻成长排行      26
            //初始魔防排行          27
            //魔防成长排行          28
            //雷霆骑士排行          29
            //超杀                  30
            //灵性                  31
            //智慧                  32

            byte[] data1 = { 12, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 37, 38, 0, 0 };
            outpack = new PacketOut();
            outpack.WriteInt16(28);
            outpack.WriteInt16(1010);
            outpack.WriteUInt32(obj.GetTypeId());
            outpack.WriteUInt32(play.GetTypeId());
            outpack.WriteBuff(data1);
            obj.BrocatBuffer(outpack.Flush());
//收到网络协议:长度：28协议号:1009
                //幻兽进化特效
            byte[] data2 = { 1, 0, 0, 0, 28, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            outpack = new PacketOut();
            outpack.WriteInt16(28);
            outpack.WriteInt16(1009);
            outpack.WriteUInt32(obj.GetTypeId());
            outpack.WriteBuff(data2);
            obj.BrocatBuffer(outpack.Flush());

           // obj.SendEudemonInfo();

            
          
        }
        //幻兽攻击
        public void Eudemon_Attack(NetMsg.MsgAttackInfo info)
        {
            EudemonObject obj = GetBattleEudemon(info.roleId);
            if (obj == null) return;
            switch (info.tag)
            {
                case 2:
                    {
                        obj.Attack(info);
                        break;
                    }
                case 21:
                    {
                        obj.MagicAttack(info);
                        break;
                    }
           }
           
        }
        //幻兽出征
        public void Eudemon_Battle(uint eudemon_id)
        {
           
            //正在出征或者合体
            if (mBattleObj.Count >= GameBase.Config.Define.MAX_CALL_EUDEMON) return; //出征的幻兽已满
            if (GetBattleEudemon(eudemon_id) != null) return;
            if (mDicEudemon.ContainsKey(eudemon_id))
            {
               //从实例幻兽对象取出--
                 EudemonObject obj  = null;
                for (int i = 0; i < mListObj.Count; i++)
                {
                    if (mListObj[i].GetTypeId() == eudemon_id)
                    {
                        obj = mListObj[i];
                        break;
                    }
                }
                if (obj == null) return;
                //检测出征条件
                if (obj.GetEudemonInfo().level > play.GetBaseAttr().level &&
                    obj.GetEudemonInfo().level - play.GetBaseAttr().level > GameBase.Config.Define.EUDEMON_MAX_BATTLE) { return; }
                //如果正在骑乘- 就先下马
                if (obj.IsRiding())
                {
                    play.TakeOffMount(obj.GetTypeId());
                }
                //出战
                obj.Battle();
                mBattleObj.Add(obj);

            }

        }
        //玩家随机或者回城- 出征幻兽也要跟随上去
        public void FlyPlay()
        {
            for(int i = 0;i < mBattleObj.Count;i++)
            {
                EudemonObject obj = mBattleObj[i];
                if(obj.GetState() == EUDEMONSTATE.BATTLE)
                {
                    obj.FlyPlay();
                }
            }
        }
        //复活幻兽
        //target 死亡的怪物- 只有打怪才会加幻兽灵气
        public void Eudemon_Alive(MonsterObject taget)
        {
            for (int i = 0; i < mBattleObj.Count; i++)
            {
                EudemonObject obj = mBattleObj[i];
                if (obj.GetState() == EUDEMONSTATE.BATTLE && obj.GetAttr().bDie)
                {
                    obj.GetAttr().life += (int)(obj.GetAttr().life_max * 0.1) ; //杀死一个怪物加5点
                    NetMsg.MsgEudemonInfo msg = new NetMsg.MsgEudemonInfo();
                    msg.id = obj.GetTypeId();
                    msg.AddAttribute(EudemonAttribute.Life, obj.GetAttr().life);
                  
                     PacketOut outpack = new PacketOut();
                    if (obj.GetAttr().life >= obj.GetAttr().life_max )
                    {
                        //      收到网络协议:长度：24协议号:2037
                        //{24,0,245,7,1,0,0,0,252,159,138,131,1,0,0,0,6,0,0,0,65,0,0,0}
                        obj.GetAttr().life = obj.GetAttr().life_max;
                        outpack.WriteInt16(24);
                        outpack.WriteInt16(2037);
                        outpack.WriteInt32(1);
                        outpack.WriteUInt32(obj.GetTypeId());
                        outpack.WriteInt32(1);
                        outpack.WriteInt32(6);
                        outpack.WriteInt32(obj.GetAttr().life);
                        obj.BrocatBuffer(outpack.Flush());
//收到网络协议:长度：20协议号:1017
//{20,0,249,3,252,159,138,131,1,0,0,0,0,0,0,0,65,0,0,0}
                        outpack = new PacketOut();
                        outpack.WriteInt16(20);
                        outpack.WriteInt16(1017);
                        outpack.WriteUInt32(obj.GetTypeId());
                        outpack.WriteInt32(1);
                        outpack.WriteInt32(0);
                        outpack.WriteInt32(obj.GetAttr().life);
                        obj.BrocatBuffer(outpack.Flush());
//收到网络协议:长度：20协议号:1017
//{20,0,249,3,252,159,138,131,1,0,0,0,26,0,0,0,4,0,0,0}
                        outpack = new PacketOut();
                        outpack.WriteInt16(20);
                        outpack.WriteInt16(1017);
                        outpack.WriteUInt32(obj.GetTypeId());
                        outpack.WriteInt32(1);
                        outpack.WriteInt32(26);
                        outpack.WriteInt32(4);
                        obj.BrocatBuffer(outpack.Flush());
//收到网络协议:长度：20协议号:1017
//{20,0,249,3,252,159,138,131,1,0,0,0,26,0,0,0,0,0,0,0}
                        outpack = new PacketOut();
                        outpack.WriteInt16(20);
                        outpack.WriteInt16(1017);
                        outpack.WriteUInt32(obj.GetTypeId());
                        outpack.WriteInt32(1);
                        outpack.WriteInt32(26);
                        outpack.WriteInt32(0);
                        obj.BrocatBuffer(outpack.Flush());

                        obj.GetAttr().bDie = false;
                        obj.SendEudemonInfo();
                    }
                    else
                    {
                    //    收到网络协议:长度：40协议号:1022

                       outpack.WriteInt16(40);
                        outpack.WriteInt16(1022);
                        outpack.WriteInt32(System.Environment.TickCount);
                        outpack.WriteUInt32(obj.GetTypeId());
                        outpack.WriteUInt32(taget.GetTypeId());
                        outpack.WriteInt16(obj.GetCurrentX());
                        outpack.WriteInt16(obj.GetCurrentY());
                       
                        outpack.WriteInt32(32);
                        outpack.WriteInt16(4);
                        outpack.WriteInt32(obj.GetAttr().life);
                        outpack.WriteInt32(0);
                        outpack.WriteInt32(0);
                        outpack.WriteInt16(0);
                        obj.BrocatBuffer(outpack.Flush());
                        
                    }
                    break;
                }
            }
        }

        public bool Eudemon_Injured(BaseObject obj, uint value, NetMsg.MsgAttackInfo info)
        {
            int i = mBattleObj.Count;
            bool bRet = false;
            //从上到下是因为最上面的是最外面的合体幻兽
            while (i > 0)
            {
                i--;
                EudemonObject eudemon_obj = mBattleObj[i];
                if (eudemon_obj.GetState() == EUDEMONSTATE.FIT)
                {
                  
                    eudemon_obj.Injured(obj, value, info);
                    bRet = true;
                    break;
                }
                
            }
            return bRet;
        
        }
        //取合体幻兽的外面的受伤害的幻兽
        public EudemonObject GetInjuredEudemon()
        {
            int i = mBattleObj.Count;
            while (i > 0)
            {
                i--;
                EudemonObject eudemon_obj = mBattleObj[i];
                if (eudemon_obj.GetState() == EUDEMONSTATE.FIT)
                {
                    return eudemon_obj;
                }

            }
            return null;
        }

        //幻兽遗忘技能
        public void Eudemon_DeleteMagic(uint eudemon_id, ushort magicid)
        {
            EudemonObject eudemon_obj = GetEudmeonObject(eudemon_id);
            if (eudemon_obj == null) return;
            eudemon_obj.DeleteMagicInfo(magicid);
        }

        //幻兽增加经验
        public void AddExp(int nExp)
        {
            for (int i = 0; i < mBattleObj.Count; i++)
            {
                EudemonObject obj = mBattleObj[i];
                if (obj.IsDie()) continue;
                obj.AddExp(nExp);
            }
        }
        //幻兽幻化- 
        //_id 副幻兽id
        public void Eudemon_Soul(uint _id)
        {
            EudemonObject fu_eudemon = GetEudmeonObject(_id);
            EudemonObject zhu_eudemon = GetEudmeonObject(mCurEudemonSoulId);
            if (zhu_eudemon == null)
            {
                play.LeftNotice("幻化失败,未找到主幻兽！");
                return;
            }
            if (fu_eudemon == null)
            {
                play.LeftNotice("幻化失败,未找到副幻兽！");
                return;
            }
            if (zhu_eudemon.GetState() != EUDEMONSTATE.BATTLE)
            {
                play.LeftNotice("幻化失败,主幻兽必须出征!");
                return;
            }
            if (fu_eudemon.GetState() != EUDEMONSTATE.BATTLE)
            {
                play.LeftNotice("幻化失败,副幻兽必须出征!");
                return;
            }

            GameStruct.RoleItemInfo role_item = play.GetItemSystem().FindItem(zhu_eudemon.GetEudemonInfo().itemid);
            if (role_item == null) return;
            GameStruct.EudemonSoulInfo soulinfo = ConfigManager.Instance().GetEudemonSoulInfo((int)(zhu_eudemon.GetEudemonInfo().quality / 100));
            if (soulinfo == null)
            {
                play.LeftNotice("幻化失败,错误!001");
                return;
            }
            //等级需求
            if (zhu_eudemon.GetEudemonInfo().level < soulinfo.level)
            {
                play.MsgBox("主幻兽等级需求:" + soulinfo.level.ToString() + "级");
                return;
            }
            //副幻兽等级需求
            if (fu_eudemon.GetEudemonInfo().level < soulinfo.level)
            {
                play.MsgBox("副幻兽等级需求:" + soulinfo.fu_level.ToString() + "级");
                return;
            }
            //副幻兽星级需求
            if (fu_eudemon.GetEudemonInfo().quality < soulinfo.fu_star)
            {
                play.MsgBox("副幻兽星级需求:极品" + soulinfo.fu_star.ToString());
                return;
            }
            RoleData_Eudemon attr = play.GetEudemonSystem().FindEudemon(zhu_eudemon.GetEudemonInfo().GetTypeID());
            if (attr == null)
            {
                play.MsgBox("幻化失败!aaa");
                return;
            }
            //幻兽id更改
            String sItemID = role_item.itemid.ToString();
            sItemID = sItemID.Substring(0,sItemID.Length-1) + "0";
            role_item.itemid = Convert.ToUInt32(sItemID);
            if (EudemonObject.GetMonsterInfo(play, role_item.id) == null)
            {
                play.MsgBox("幻化失败");
                return;
            }
            //更新道具与幻兽信息
            play.GetItemSystem().UpdateItemInfo(role_item.id);
            zhu_eudemon.SetMosterInfo(EudemonObject.GetMonsterInfo(play, role_item.id));
            zhu_eudemon.SendEudemonInfo();

            //----------------
            this.Eudemon_ReCall(fu_eudemon.GetTypeId()); //召回副幻兽
            play.GetItemSystem().DeleteItemByID(fu_eudemon.GetTypeId()); //删除副幻兽

            attr.level = 1; //幻化变为1级
            attr.recall_count++;//转世次数+1 
            //zhu_eudemon.GetEudemonInfo().level = 1;
            //zhu_eudemon.GetEudemonInfo().recall_count++; 
            GameStruct.MonsterInfo monster_info = zhu_eudemon.GetMonsterInfo();
            //幻兽品质
            attr.quality += IRandom.Random(soulinfo.add_min, soulinfo.add_max);
          //  zhu_eudemon.GetEudemonInfo().quality += IRandom.Random(soulinfo.add_min,soulinfo.add_max);
          //主属性-目前 战士幻兽加物攻 物防 法师幻兽加魔攻魔防
            if (soulinfo.add_main > 0)
            {
                switch (monster_info.eudemon_type)
                {
                        //战士
                    case GameBase.Config.Define.EUDEMON_TYPE_WARRIOR:
                    case GameBase.Config.Define.EUDEMON_TYPE_WARRIOR_RIG:
                        {
                            attr.phyatk_grow_rate += soulinfo.add_main;
                            attr.phyatk_grow_rate_max += soulinfo.add_main;
                            attr.defense_grow_rate += soulinfo.add_main;
                            //zhu_eudemon.GetEudemonInfo().phyatk_grow_rate += soulinfo.add_main;
                            //zhu_eudemon.GetEudemonInfo().phyatk_grow_rate_max += soulinfo.add_main;
                            //zhu_eudemon.GetEudemonInfo().defense_grow_rate += soulinfo.add_main;
                            break;
                        }
                        //法师
                    case GameBase.Config.Define.EUDEMON_TYPE_MAGE:
                    case GameBase.Config.Define.EUDEMON_TYPE_MAGE_RID:
                        {

                            attr.magicatk_grow_rate += soulinfo.add_main;
                            attr.magicatk_grow_rate_max += soulinfo.add_main;
                            attr.magicdef_grow_rate += soulinfo.add_main;
                            //zhu_eudemon.GetEudemonInfo().magicatk_grow_rate += soulinfo.add_main;
                            //zhu_eudemon.GetEudemonInfo().magicatk_grow_rate_max += soulinfo.add_main;
                            //zhu_eudemon.GetEudemonInfo().magicdef_grow_rate += soulinfo.add_main;
                            break;
                        }
                }
            }
            //副属性-
            if (soulinfo.add_fu > 0)
            {
                switch (monster_info.eudemon_type)
                {
                    //战士
                    case GameBase.Config.Define.EUDEMON_TYPE_WARRIOR:
                    case GameBase.Config.Define.EUDEMON_TYPE_WARRIOR_RIG:
                        {
                            attr.magicatk_grow_rate += soulinfo.add_fu;
                            attr.magicatk_grow_rate_max += soulinfo.add_fu;
                            attr.magicdef_grow_rate += soulinfo.add_fu;
                            //zhu_eudemon.GetEudemonInfo().magicatk_grow_rate += soulinfo.add_fu;
                            //zhu_eudemon.GetEudemonInfo().magicatk_grow_rate_max += soulinfo.add_fu;
                            //zhu_eudemon.GetEudemonInfo().magicdef_grow_rate += soulinfo.add_fu;
                            break;
                        }
                    //法师
                    case GameBase.Config.Define.EUDEMON_TYPE_MAGE:
                    case GameBase.Config.Define.EUDEMON_TYPE_MAGE_RID:
                        {
                            attr.phyatk_grow_rate += soulinfo.add_fu;
                            attr.phyatk_grow_rate_max += soulinfo.add_fu;
                            attr.defense_grow_rate += soulinfo.add_fu;
                            attr.phyatk_grow_rate += soulinfo.add_fu;
                            //zhu_eudemon.GetEudemonInfo().phyatk_grow_rate_max += soulinfo.add_fu;
                            //zhu_eudemon.GetEudemonInfo().defense_grow_rate += soulinfo.add_fu;
                            break;
                        }
                }
                attr.life_grow_rate += soulinfo.add_fu;
                //生命成长
               // zhu_eudemon.GetEudemonInfo().life_grow_rate += soulinfo.add_fu;
            }

            //初始属性
            if (soulinfo.add_init > 0)
            {

                attr.init_life += soulinfo.add_init;
                attr.init_atk_min += soulinfo.add_init;
                attr.init_atk_max += soulinfo.add_init;
                attr.init_magicatk_min += soulinfo.add_init;
                attr.init_magicatk_max += soulinfo.add_init;
                attr.init_defense += soulinfo.add_init;
                attr.init_magicdef += soulinfo.add_init;


                //zhu_eudemon.GetEudemonInfo().init_life += soulinfo.add_init;
                //zhu_eudemon.GetEudemonInfo().init_atk_min += soulinfo.add_init;
                //zhu_eudemon.GetEudemonInfo().init_atk_max += soulinfo.add_init;
                //zhu_eudemon.GetEudemonInfo().init_magicatk_min += soulinfo.add_init;
                //zhu_eudemon.GetEudemonInfo().init_magicatk_max += soulinfo.add_init;
                //zhu_eudemon.GetEudemonInfo().init_defense += soulinfo.add_init;
                //zhu_eudemon.GetEudemonInfo().init_magicdef += soulinfo.add_init;

            }
            //zhu_eudemon.GetEudemonInfo().luck++;
            //zhu_eudemon.GetEudemonInfo().magicatk_grow_rate += 0.5f;
            //zhu_eudemon.GetEudemonInfo().magicatk_grow_rate_max += 1.5f;
           // zhu_eudemon.GetEudemonInfo().magicdef_grow_rate += 0.5f;
            //    收到网络协议:长度：28协议号:1010
            zhu_eudemon.SetEudemonInfo(attr);
            NetMsg.MsgEudemonInfo msg = new NetMsg.MsgEudemonInfo();

    

            PacketOut outpack = new PacketOut();
            outpack.WriteInt16(28);
            outpack.WriteInt16(1010);
            outpack.WriteUInt32(zhu_eudemon.GetTypeId());
            outpack.WriteUInt32(play.GetTypeId());
            outpack.WriteInt16(21);
            outpack.WriteInt16(69);
            outpack.WriteInt32(0);
            outpack.WriteInt32(0);
            outpack.WriteInt32(9765);
            play.SendData(outpack.Flush(), true);

//收到网络协议:长度：24协议号:2037
            this.SendEudemonInfo(zhu_eudemon.GetEudemonInfo());

            //弹出幻化成功对话框
         //   收到网络协议:长度：28协议号:1010
            outpack = new PacketOut();
            outpack.WriteInt16(28);
            outpack.WriteInt16(1010);
            outpack.WriteInt32(System.Environment.TickCount);
            outpack.WriteUInt32(zhu_eudemon.GetTypeId());
            outpack.WriteInt32(50);
            outpack.WriteInt32(0);
            outpack.WriteInt32(1);
            outpack.WriteInt32(9742);
            play.SendData(outpack.Flush(), true);

        }

        //计算幻兽叠加到人物身上战斗力
        public int CalcFightSoul()
        {
            int nSoul = 0;
            for (int i = 0; i < mBattleObj.Count; i++)
            {
                EudemonObject obj = mBattleObj[i];
                if (obj.GetEudemonInfo().quality > 0)
                {
                    nSoul += 20;//一进化就是至尊圣兽
                    nSoul += (int)(obj.GetEudemonInfo().quality / 100);//星级
                    //幻兽死亡-2点战斗力
                    if (obj.IsDie())
                    {
                        nSoul -= 2;
                    }
                }
                
            }
            return nSoul;
        }
        //获取合体的幻兽的最小攻击
        public int GetFitEudemonMinAtk()
        {
            int nMinAtk = 0;
            for (int i = 0; i < mBattleObj.Count; i++)
            {
                EudemonObject obj = mBattleObj[i];
                if (obj.GetState() == EUDEMONSTATE.FIT)
                {
                    nMinAtk += obj.GetMinAck();
                }
            }
            return nMinAtk;
        }

        //获取合体的幻兽的最大攻击
        public int GetFitEudemonMaxAtk()
        {
            int nAtk = 0;
            for (int i = 0; i < mBattleObj.Count; i++)
            {
                EudemonObject obj = mBattleObj[i];
                if (obj.GetState() == EUDEMONSTATE.FIT)
                {
                    nAtk += obj.GetMaxAck();
                }
            }
            return nAtk;
        }

        public void TakeMount(uint eudemon_id)
        {
            
            EudemonObject obj = null;
            for (int i = 0; i < mBattleObj.Count; i++)
            {
                obj = mBattleObj[i];
                if (obj.GetTypeId() == eudemon_id)
                {
                    //出战的幻兽召回
                    if (obj.GetState() == EUDEMONSTATE.BATTLE)
                    {
                        this.Eudemon_ReCall(eudemon_id);
                    }
                        //合体的幻兽解体
                    else if (obj.GetState() == EUDEMONSTATE.FIT)
                    {
                        this.Eudemon_BreakUp(eudemon_id);
                    }
                    break;
                }
            }
            obj = this.GetEudmeonObject(eudemon_id);
            if (obj == null) return;
            obj.SetRiding(true);
        }
        //下马
        public void TakeOffMount(uint eudemon_id)
        {
            EudemonObject obj = GetEudmeonObject(eudemon_id);
            if (obj == null) return;
            obj.SetRiding(false);
        }
      public void Process_DieEudemon()
        {
            for (int i = 0; i < mListObj.Count; i++)
            {
                EudemonObject obj = mListObj[i];
                RoleItemInfo item = play.GetItemSystem().FindItem(obj.GetAttr().itemid);
                if (item == null)
                {
                    DeleteEudemon(obj.GetTypeId());
                }
           }
        }
        //查看幻兽- 把自身装备信息发给对方
        public void SendLookEudemonInfo(PlayerObject target)
        {
            uint play_id = play.GetTypeId();
        
            PacketOut outpack = new PacketOut();

            EudemonObject obj = null;
            for (int i = 0; i < mListObj.Count; i++)
            {
                obj = mListObj[i];
                uint itemid = obj.GetEudemonInfo().itemid;
                RoleItemInfo item_info = play.GetItemSystem().FindItem(itemid);
                RoleData_Eudemon info = obj.GetEudemonInfo();
                if (item_info == null) continue;
                //发送道具信息
                outpack = new PacketOut();
                int nLen = 84 + Coding.GetDefauleCoding().GetBytes(item_info.forgename).Length;
                outpack.WriteInt16((short)nLen);
                outpack.WriteInt16(1008); //道具信息
                outpack.WriteUInt32(play_id);
                outpack.WriteUInt32(info.GetTypeID());
                outpack.WriteUInt32(item_info.itemid);
                outpack.WriteInt32(0);
                outpack.WriteByte(NetMsg.MsgItemInfo.TAG_LOOKROLEEUDEMONINFO); //幻兽背包
                outpack.WriteByte(0);
                outpack.WriteByte(NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK);//幻兽背包
                byte[] _data = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
                outpack.WriteBuff(_data);
                outpack.WriteString(item_info.forgename);
                outpack.WriteByte(0);
                outpack.WriteByte(0);
                outpack.WriteByte(0);
                target.SendData(outpack.Flush(), true);
                //target.GetItemSystem().SendItemInfo(item_info, NetMsg.MsgItemInfo.TAG_LOOKROLEEUDEMONINFO);

                //发送幻兽详细信息
                NetMsg.MsgEudemonInfo msg = new NetMsg.MsgEudemonInfo();
              
                msg.id = info.GetTypeID();
                msg.tag = 2;
                msg.AddAttribute(GameStruct.EudemonAttribute.Atk_Max, info.atk_max);
                msg.AddAttribute(GameStruct.EudemonAttribute.Atk_Min, info.atk_min);
                msg.AddAttribute(GameStruct.EudemonAttribute.MagicAtk_Max, info.magicatk_max);
                msg.AddAttribute(GameStruct.EudemonAttribute.MagicAtk_Min, info.magicatk_min);
                msg.AddAttribute(GameStruct.EudemonAttribute.Defense, info.defense);
                msg.AddAttribute(GameStruct.EudemonAttribute.Magic_Defense, info.magicdef);
                msg.AddAttribute(GameStruct.EudemonAttribute.Life, info.life);
                msg.AddAttribute(GameStruct.EudemonAttribute.Life_Max, info.life_max);
                msg.AddAttribute(GameStruct.EudemonAttribute.Intimacy, info.intimacy);
                msg.AddAttribute(GameStruct.EudemonAttribute.Level, info.level);
                msg.AddAttribute(GameStruct.EudemonAttribute.WuXing, info.wuxing);
                msg.AddAttribute(GameStruct.EudemonAttribute.Luck, info.luck);
                msg.AddAttribute(GameStruct.EudemonAttribute.Recall_Count, info.recall_count);
                msg.AddAttribute(EudemonAttribute.Card, info.card);
                msg.AddAttribute(EudemonAttribute.Exp, info.exp);
                msg.AddAttribute(EudemonAttribute.Quality, info.quality);
                msg.AddAttribute(EudemonAttribute.Init_Atk, info.GetInitAtk());
                msg.AddAttribute(EudemonAttribute.Init_Magic_Atk, info.GetInitMagicAtk());
                msg.AddAttribute(EudemonAttribute.Init_Defense, info.GetInitDefense());
                msg.AddAttribute(EudemonAttribute.Init_Life, info.init_life);
                msg.AddAttribute(EudemonAttribute.Life_Grow_Rate, ConvertGrowRate(info.life_grow_rate));
                msg.AddAttribute(EudemonAttribute.Atk_Min_Grow_Rate, ConvertGrowRate(info.phyatk_grow_rate));
                msg.AddAttribute(EudemonAttribute.Atk_Max_Grow_Rate, ConvertGrowRate(info.phyatk_grow_rate_max));
                msg.AddAttribute(EudemonAttribute.MagicAtk_Min_Grow_Rate, ConvertGrowRate(info.magicatk_grow_rate));
                msg.AddAttribute(EudemonAttribute.MagicAtk_Max_Grow_Rate, ConvertGrowRate(info.magicatk_grow_rate_max));
                msg.AddAttribute(EudemonAttribute.Defense_Grow_Rate, ConvertGrowRate(info.defense_grow_rate));
                msg.AddAttribute(EudemonAttribute.MagicDefense_Grow_Rate, ConvertGrowRate(info.magicdef_grow_rate));
                GameStruct.MonsterInfo _info = EudemonObject.GetMonsterInfo(play, info.itemid);
                if (_info != null)
                {
                    msg.AddAttribute(EudemonAttribute.Riding, _info.eudemon_type);
                }
                target.SendData(msg.GetBuffer(), true);

            }
         //   收到网络协议:长度：99协议号:1008
//byte[] data1 = {99,0,240,3,73,48,96,5,253,159,138,131,93,92,16,0,0,0,0,0,7,0,53,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,14,218,164,186,211,189,228,193,233,161,164,206,172,193,208,0,0,0};
//            target.SendData(data1,true);
//            //收到网络协议:长度：143协议号:1116
//byte[] data2 = {143,0,92,4,253,159,138,131,183,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,73,48,96,5,220,3,0,0,220,3,0,0,199,1,52,2,6,0,3,0,0,0,0,0,29,26,1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,93,92,16,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,14,218,164,186,211,189,228,193,233,161,164,206,172,193,208,0,0};
//              target.SendData(data2,true);
//            //收到网络协议:长度：24协议号:2037
//              byte[] data3 = { 24, 0, 245, 7, 2, 0, 0, 0, 253, 159, 138, 131, 1, 0, 0, 0, 24, 0, 0, 0, 7, 0, 0, 0 };
//              target.SendData(data3, true);
////收到网络协议:长度：496协议号:2037
//byte[] data4 = {240,1,245,7,2,0,0,0,253,159,138,131,60,0,0,0,6,0,0,0,220,3,0,0,7,0,0,0,220,3,0,0,10,0,0,0,37,0,0,0,8,0,0,0,149,0,0,0,9,0,0,0,51,235,16,0,55,0,0,0,0,0,0,0,12,0,0,0,3,0,0,0,14,0,0,0,0,0,0,0,15,0,0,0,0,0,0,0,16,0,0,0,0,0,0,0,17,0,0,0,0,0,0,0,18,0,0,0,0,0,0,0,19,0,0,0,3,0,0,0,21,0,0,0,5,0,0,0,23,0,0,0,1,0,0,0,25,0,0,0,202,0,0,0,26,0,0,0,188,4,0,0,27,0,0,0,142,3,0,0,28,0,0,0,9,0,0,0,50,0,0,0,1,0,32,0,73,0,0,0,0,0,0,0,51,0,0,0,0,0,0,0,59,0,0,0,0,0,0,0,60,0,0,0,0,0,0,0,61,0,0,0,0,0,0,0,62,0,0,0,0,0,0,0,63,0,0,0,11,0,0,0,64,0,0,0,0,0,0,0,65,0,0,0,0,0,0,0,66,0,0,0,0,0,0,0,74,0,0,0,0,0,0,0,75,0,0,0,0,0,0,0,76,0,0,0,0,0,0,0,77,0,0,0,0,0,0,0,78,0,0,0,0,0,0,0,79,0,0,0,0,0,0,0,80,0,0,0,0,0,0,0,81,0,0,0,0,0,0,0,84,0,0,0,0,0,0,0,20,0,0,0,0,0,0,0,24,0,0,0,7,0,0,0,13,0,0,0,48,0,0,0,1,0,0,0,38,0,0,0,0,0,0,0,83,0,0,0,3,0,0,0,250,0,0,0,2,0,0,0,221,1,0,0,4,0,0,0,182,1,0,0,5,0,0,0,196,0,0,0,64,0,0,0,0,0,0,0,65,0,0,0,0,0,0,0,66,0,0,0,0,0,0,0,82,0,0,0,0,0,0,0,36,0,0,0,162,10,0,0,37,0,0,0,233,3,0,0,38,0,0,0,208,8,0,0,39,0,0,0,234,25,0,0,40,0,0,0,144,50,0,0,41,0,0,0,164,46,0,0,42,0,0,0,72,20,0,0,83,0,0,0,50,0,0,0};
//target.SendData(data4, true);
            //收到网络协议:长度：20协议号:1103
//{20,0,79,4,253,159,138,131,0,0,0,0,213,7,0,0,0,0,0,0}
//收到网络协议:长度：20协议号:1103
//{20,0,79,4,253,159,138,131,0,0,0,0,233,3,0,0,0,0,0,0}
//收到网络协议:长度：20协议号:1103
//{20,0,79,4,253,159,138,131,0,0,0,0,185,11,0,0,0,0,0,0}
//收到网络协议:长度：20协议号:1103
//{20,0,79,4,253,159,138,131,0,0,0,0,230,7,0,0,0,0,0,0}
//收到网络协议:长度：172协议号:1117
//byte[] data5 = { 172, 0, 93, 4 };
//    byte[] data6 = {0,0,0,0,0,70,0,0,0,0,1,0,12,228,48,138,92,92,16,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,15,0,0,0,15,0,0,0,100,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,4,0,0,0,0,0,15,0,26,18,104,0,107,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,32,0,11,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,218,164,186,211,189,228,193,233,161,164,206,172,193,208,0,0,0,0,0,0,0,0,0,0,0,0};
//    PacketOut outpack = new PacketOut();
//    outpack.WriteBuff(data5);
//    outpack.WriteUInt32(play.GetTypeId());
//    outpack.WriteBuff(data6);
//    target.SendData(outpack.Flush(), true);
            //for (int i = 0; i < mListObj.Count; i++);
            //{
            //    EudemonObject obj = mListObj[i];
            //    GameStruct.RoleItemInfo item = play.GetItemSystem().FindItem(obj.GetEudemonInfo().itemid);
            //    if (item == null) continue;
            //    target.GetItemSystem().SendItemInfo(item, NetMsg.MsgItemInfo.TAG_LOOKROLEEUDEMONINFO);
            //    target.GetEudemonSystem().SendEudemonInfo(obj.GetEudemonInfo(),false,true);
            //}
        }
    }
}
