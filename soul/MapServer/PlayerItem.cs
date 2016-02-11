using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;
using GameBase.Network.Internal;
using GameBase.Network;
using GameBase.Core;

namespace MapServer
{
    //角色道具类
    public class PlayerItem
    {
        private const int IETMSORT_FINERY = 1;  //服饰
        private const int ITEMSORT_MOUNT = 6;   //坐骑
        public const int MAX_STRONGITEM = 100;          //仓库最大存储道具数量
        public const long MAX_GOLD = 3000000000;         //包裹与仓库携带最多的金币数
        public const int MAXBAG_COUNT = 40;     //装备格子最大数量
     
        private uint mScriptItemId = 0;
        //当前使用道具的id
        public uint GetScriptItemId() { return mScriptItemId; }
        private PlayerObject play;
        private Dictionary<uint, GameStruct.RoleItemInfo> mDicItem;
        public Dictionary<uint, GameStruct.RoleItemInfo> GetDicItem(){return mDicItem;}
        private Dictionary<uint, GameStruct.RoleItemInfo> mDicAddItem; //临时的道具表,增加道具需要像dbserver发送消息- 返回之后才确定是否增加成功
      //  private int mBagCount;     //包裹里的道具数量

        //取物品栏道具数量
        public int GetBagCount() 
        {
            int nCount = 0;
            foreach (GameStruct.RoleItemInfo itemobj in mDicItem.Values)
            {
                if (itemobj.postion == NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK)
                {
                    nCount++;
                }
            }
            return nCount;
        }

        private uint mWeaponId; //当前穿戴的武器id
        private uint mArmorId; //穿戴的盔甲id
        private uint mFashionId; //当前穿戴的外套id
        //取当前穿戴的武器id
        public uint GetWeaponLook()
        {
            return mWeaponId;
        }

        public uint GetArmorLook()
        {
            if (mFashionId != 0) return mFashionId;
            return mArmorId;
        }
        public PlayerItem(PlayerObject _play)
        {
            //mBagCount = 0;
            mDicItem = new Dictionary<uint, GameStruct.RoleItemInfo>();
            mDicItem.Clear();
            play = _play;
            mDicAddItem = new Dictionary<uint, GameStruct.RoleItemInfo>();
            mDicAddItem.Clear();
            mScriptItemId = 0;
            mWeaponId = 0;
            mFashionId = 0;
            mArmorId = 0;
        }


        //这个函数用于玩家丢弃的道具，然后又捡起来应用..所以不叠加
        public GameStruct.RoleItemInfo AwardItem(GameStruct.RoleItemInfo info)
        {
            if (IsGold(info.itemid))
            {
                play.ChangeAttribute(GameStruct.UserAttribute.GOLD, info.property);
                play.LeftNotice(string.Format("获得{0}金币!", info.property));
                return null;
            }
            GameStruct.ItemTypeInfo baseitem = ConfigManager.Instance().GetItemTypeInfo(info.itemid);
            if (baseitem == null)
            {
                Log.Instance().WriteLog("创建道具失败,道具不存在 id:" + info.itemid.ToString());
                //  return null;
                if (!GameServer.IsTestMode()) return null;
                baseitem = new GameStruct.ItemTypeInfo();
                baseitem.id = info.itemid;
            }
            uint key = (uint)mDicAddItem.Count + 1;

            GameStruct.RoleItemInfo item = new GameStruct.RoleItemInfo();
            item.itemid = baseitem.id;
            item.postion = info.postion;
            item.stronglv = info.stronglv;
            item.gemcount = info.gemcount;
            item.amount = info.amount;
            item.gem1= info.gem1;
            item.gem2 = info.gem2;
            item.forgename = info.forgename;
            item.war_ghost_exp = info.war_ghost_exp;
            item.di_attack = info.di_attack;
            item.huo_attack = info.huo_attack;
            item.shui_attack = info.shui_attack;
            item.feng_attack = info.feng_attack;
            item.property = info.property;
            item.gem3 = info.gem3;
            item.god_strong = info.god_strong;
            item.god_exp = info.god_exp;

            item.typeid = info.typeid;
            mDicAddItem[key] = item;

            //发给dbserver 通知增加这个道具
            GameBase.Network.Internal.AddRoleData_Item dbitem = new GameBase.Network.Internal.AddRoleData_Item();
            dbitem.item.playerid = play.GetBaseAttr().player_id;
            dbitem.gameid = play.GetGameID();
            dbitem.item.postion = item.postion;
            dbitem.item.itemid = item.itemid;
            dbitem.item.stronglv = item.stronglv;
        
            dbitem.item.amount = item.amount;
            dbitem.item.gem1 = item.gem1;
            dbitem.item.gem2 = item.gem2;
            dbitem.item.forgename = item.forgename;
            dbitem.item.war_ghost_exp = item.war_ghost_exp;
            dbitem.item.di_attack = item.di_attack;
            dbitem.item.huo_attack = item.huo_attack;
            dbitem.item.shui_attack = item.shui_attack;
            dbitem.item.feng_attack = item.feng_attack;
            dbitem.item.property = item.property;
            dbitem.item.gem3 = item.gem3;
            dbitem.item.god_strong = item.god_strong;
            dbitem.item.god_exp = item.god_exp;
            dbitem.sortid = key;
            DBServer.Instance().GetDBClient().SendData(dbitem.GetBuffer());
            return item;
        }

        //叠加道具 2015.9.22
        public GameStruct.RoleItemInfo ItemLimit(uint itemid, byte amount)
        {
            GameStruct.ItemTypeInfo baseitem = ConfigManager.Instance().GetItemTypeInfo(itemid);
            foreach (GameStruct.RoleItemInfo info in mDicItem.Values)
            {
                if (info.itemid == itemid)
                {
                    if (info.amount + amount <= baseitem.amount_limit)
                    {
                        info.amount += amount;
                        //下发更新道具
                        this.UpdateItemInfo(info.id);
                        return info;
                    }
                }
            }
            return null;
        }
        //道具id是否为金币
        //itemid 道具id
        public bool IsGold(uint itemid)
        {
            if(itemid == GameBase.Config.Define.GOLD_ITEM_1 ||
               itemid == GameBase.Config.Define.GOLD_ITEM_2 ||
                itemid == GameBase.Config.Define.GOLD_ITEM_3 ||
                itemid == GameBase.Config.Define.GOLD_ITEM_4 ||
                itemid == GameBase.Config.Define.GOLD_ITEM_5)
            {
                return true;
            }
            return false;
           
        }


        //public RoleData_Eudemon AwardEudemon(uint itemid)
        //{

        //}
        //创建物品参数说明
        //postion:道具位置
        //amount:数量
        //stronglv: 强化等级
        //gem1: 第一洞宝石 255为打孔
        //gem2: 第二洞镶嵌的宝石 255为打孔
        //gem3: 第三洞镶嵌的宝石 255为打孔
        //warghost_exp: 战魂等级 最高9级
        //di_attack : 地攻击 最高255级
        //shui_attack: 水攻击 最高255级
        //huo_attack: 火攻击 最高255级
        //feng_attack: 风攻击 最高255级
        //limit: 是否叠加
        public GameStruct.RoleItemInfo AwardItem(uint itemid, byte postion, byte amount = 1, byte stronglv = 0,byte gem1 = 0,byte gem2 = 0,byte gem3 = 0,byte warghost_exp = 0,
            byte di_attack = 0,byte shui_attack = 0,byte huo_attack = 0,byte feng_attack = 0,bool limit = true/*是否叠加*/)
        {
            //是金币
            if (IsGold(itemid))
            {
                return null;
            }
            GameStruct.ItemTypeInfo baseitem = ConfigManager.Instance().GetItemTypeInfo(itemid);
            if (baseitem == null)
            {
                Log.Instance().WriteLog("创建道具失败,道具不存在 id:" + itemid.ToString());
              //  return null;
                if (!GameServer.IsTestMode()) return null; 
                baseitem = new GameStruct.ItemTypeInfo();
                baseitem.id = itemid;
            }
          
            GameStruct.RoleItemInfo item = null;
            //可叠加
            if (!IsEquip(baseitem.id) && limit && baseitem.amount_limit > 1)
            {
                item = ItemLimit(itemid, (byte)baseitem.amount);
                if(item  != null)
                {
                    return item;
                }
            }
            uint key = (uint)mDicAddItem.Count + 1;

            item = new GameStruct.RoleItemInfo();
            item.itemid = baseitem.id;
            item.postion = postion;
            item.stronglv = stronglv;
            item.gem1 = gem1;
            item.gem2 = gem2;
            item.gem3 = gem3;
            item.war_ghost_exp = warghost_exp * 100;
            item.di_attack = di_attack;
            item.shui_attack = shui_attack;
            item.huo_attack = huo_attack;
            item.feng_attack = feng_attack;
            item.amount = baseitem.amount;
           // item.amount = IsEquip(baseitem.id) == true ? baseitem.amount_limit : amount;

            //宝石属性
            GameStruct.GemInfo gem = ConfigManager.Instance().GetGemInfo(item.itemid);
            if (gem != null)
            {
                item.gem1 = (uint)gem.type;
            }
            mDicAddItem[key] = item;

            //发给dbserver 通知增加这个道具
            GameBase.Network.Internal.AddRoleData_Item dbitem = new GameBase.Network.Internal.AddRoleData_Item();
            dbitem.item.playerid = play.GetBaseAttr().player_id;
            dbitem.gameid = play.GetGameID();
            dbitem.item.postion = postion;
            dbitem.item.itemid = item.itemid;
            dbitem.item.stronglv = stronglv;
         
            dbitem.item.amount = amount;
            dbitem.sortid = key;
            DBServer.Instance().GetDBClient().SendData(dbitem.GetBuffer());
            return item;
        }

        public void AwardItem_Ret(uint sortid, uint id)
        {
            if (mDicAddItem.ContainsKey(sortid))
            {
                GameStruct.RoleItemInfo info = mDicAddItem[sortid];
                info.id = id;
               
                //如果是从地图上捡到别人扔的幻兽- 先取出typeid 好findtempeudemon
                uint nTypeID = 0;
                if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK && info.typeid > 0)
                {
                    nTypeID = info.typeid;
                }
                mDicAddItem.Remove(sortid);
                mDicItem[info.id] = info;
                 //如果是穿戴在身上之类的..就计算外观
                if (info.postion == NetMsg.MsgItemInfo.ITEMPOSTION_STRONG_PACK) return; //仓库的不下发
                if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK) //是幻兽- 增加幻兽
                {
                    //幻兽初始名称为道具名称
                    GameStruct.ItemTypeInfo baseinfo = ConfigManager.Instance().GetItemTypeInfo(info.itemid);
                    if (baseinfo != null)
                    {
                        info.forgename = baseinfo.name;
                    }
                    info.typeid = IDManager.CreateTypeId(OBJECTTYPE.EUDEMON);

                }
                else if (info.postion != NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK)
                {
                    CalcEquipLook(info);
                  
                }
                //通知客户端生成道具
                SendItemInfo(info);
                if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK)
                {
                 
                    RoleData_Eudemon tempeudemon = play.GetEudemonSystem().FindTempEudemon(nTypeID);
                    // tempeudemon.itemid 为0时表示是用脚本创建的幻兽- 否则就是地图捡取或者交易所得的
                    if (tempeudemon != null && tempeudemon.itemid != 0)
                    {
                        tempeudemon.itemid = info.id;
                        tempeudemon.typeid = info.typeid;
                        play.GetEudemonSystem().AddEudemon(tempeudemon);
                        play.GetEudemonSystem().DeleteTempEudemon(nTypeID);
                        //-回收该幻兽id
                        IDManager.RecoveryTypeID(nTypeID, OBJECTTYPE.EUDEMON);
                    }
                    else
                    {
                        //增加幻兽的扩展接口，可以增加预设属性的幻兽2016.1.31
                        if (tempeudemon != null && tempeudemon.itemid == 0)
                        {
                            play.GetEudemonSystem().AddEudemon(info, (byte)tempeudemon.level, tempeudemon.quality, (byte)tempeudemon.wuxing);
                        }
                        else
                        {
                            play.GetEudemonSystem().AddEudemon(info);
                        }

                    }
                }
                //if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK)
                //{
                //    mBagCount++;
                //}
            }


        }

        
        //如果是装备- 要更新角色属性
        private void  CalcEquipLook(GameStruct.RoleItemInfo _item = null)           
        {
            bool isChangeLook = false;
            bool isChangeEquip = false;
            mFashionId = mArmorId = mWeaponId = 0;
            if(_item != null)
            {
                if (_item.postion == NetMsg.MsgItemInfo.ITEMPOSITION_FASHION ||
                _item.postion == NetMsg.MsgItemInfo.ITEMPOSITION_WEAPONR ||
                _item.postion == NetMsg.MsgItemInfo.ITEMPOSITION_ARMOR)
                {
                    isChangeLook = true;
                }
                
            }
            foreach (GameStruct.RoleItemInfo info in mDicItem.Values)
            {
                if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK) continue;
                if (IsEquip(info.itemid))
                {
                    isChangeEquip = true;
                    switch (info.postion)
                    {
                        case NetMsg.MsgItemInfo.ITEMPOSITION_FASHION:   //时装
                            {
                                if (info.itemid != mFashionId) isChangeLook = true;
                                mFashionId = info.itemid;
                                
                                break;
                            }
                        case NetMsg.MsgItemInfo.ITEMPOSITION_WEAPONR:       //武器
                            {
                                if (info.itemid != mWeaponId) isChangeLook = true;
                                mWeaponId = info.itemid;
                              
                                break;
                            }
                        case NetMsg.MsgItemInfo.ITEMPOSITION_ARMOR:     //盔甲
                            {
                                if (info.itemid != mArmorId) isChangeLook = true;
                                mArmorId = info.itemid;
                               
                                break;
                            }
                        
                    }
                    //武器幻魂-
                    if (info.postion == NetMsg.MsgItemInfo.ITEMPOSTION_WEPON_SOUL)
                    {
                        mWeaponId = info.itemid;
                    }
                }
            }
            if (isChangeEquip)
                play.CalcAttribute();
            //更新附近的玩家刷新
            if (isChangeLook && play.GetGameMap() != null)
            {
                foreach (RefreshObject refobj in play.GetVisibleList().Values)
                {
                    BaseObject obj = refobj.obj;
                    if (obj.type == OBJECTTYPE.PLAYER)
                    {
                        (obj as PlayerObject).SendRoleInfo(play);
                    }
                }
                //NetMsg.MsgRoleInfo role = new NetMsg.MsgRoleInfo();
                //role.role_id = play.GetTypeId();
                //role.x = play.GetCurrentX();
                //role.y = play.GetCurrentY();
                //role.armor_id = this.GetArmorLook();
                //role.wepon_id = this.GetWeaponLook();
                //role.face_sex = play.GetFace();
                //role.dir = play.GetDir();
                //role.guanjue = (byte)play.GetGuanJue();
                //role.str.Add(play.GetName());
                //play.BroadcastBuffer(role.GetBuffer());
            }
        }

        //删除当前脚本道具
        public void DeleteScripteItem()
        {
            if (mScriptItemId == 0) return;
            if (mDicItem.ContainsKey(mScriptItemId))
            {
                DeleteItemByID(mScriptItemId);
            }
            mScriptItemId = 0;
        }
        public void SendItemInfo(GameStruct.RoleItemInfo info,byte tag = NetMsg.MsgItemInfo.TAG_ROLEITEM)
        {
            NetMsg.MsgItemInfo item = new NetMsg.MsgItemInfo();
            item.Create(null, play.GetGamePackKeyEx());
            item.postion = (byte)info.postion;
            if (item.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK)
            {
                item.id = info.typeid;
                if (tag == NetMsg.MsgItemInfo.TAG_ROLEITEM)
                {
                    item.tag = NetMsg.MsgItemInfo.TAG_ROLEEUDEMONPACK;
                }
               
            }
            else
            {
                item.id = info.id;
            }



            item.item_id = info.itemid;
            item.amount = info.amount;
            item.amount_limit = info.amount;
            item.magic3 = info.stronglv;
            item.gem = (byte)info.gem1;
            item.gem2 =(byte) info.gem2;
            item.warghost_exp = info.war_ghost_exp;
            item.di_attack = info.di_attack;
            item.shui_attack = info.shui_attack;
            item.huo_attack = info.huo_attack;
            item.feng_attack = info.feng_attack;
            item.properties = info.property;
            item.gem3 = (byte)info.gem3;
            item.god_exp = info.god_exp;
            item.god_strong = info.god_strong;
        
            item.tag = tag;
            item.name = info.forgename;
            //特制经验值球与经验值球只有满的..
            if (item.item_id == GameBase.Config.Define.EXP_BALL_ID ||
                item.item_id == GameBase.Config.Define.SUPER_EXP_BALL_ID)
            {
                item.param3 = GameBase.Config.Define.EXP_BALL_MAX;
            }
            //法宝为星级经验
            if (item.item_id == GameBase.Config.Define.ITEM_SHUGUANGZHANHUN_ID ||
                item.item_id == GameBase.Config.Define.ITEM_DILONGZHILEI_ID ||
                item.item_id == GameBase.Config.Define.ITEM_SHENGYAOFUWEN_ID)
            {
                item.param3 = info.god_strong;
            }
          
            GameStruct.ItemTypeInfo _info = ConfigManager.Instance().GetItemTypeInfo(item.item_id);
            if (_info != null)
            {
                item.amount_limit = _info.amount_limit;
            }
           
      
            play.SendData(item.GetBuffer());
        }

        public void UpdateItemInfo(uint id)
        {
            if (mDicItem.ContainsKey(id))
            {
                GameStruct.RoleItemInfo info = mDicItem[id];
                SendItemInfo(info);
                //如果不在背包就重新计算属性
                if (info.postion >= NetMsg.MsgItemInfo.ITEMPOSITION_HELMET && 
                    info.postion < NetMsg.MsgItemInfo.ITEMPOSITION_FASHION)
                {
                    play.CalcAttribute();
                }
            }
        }
        public void SendAllItemInfo()
        {
            foreach (GameStruct.RoleItemInfo info in mDicItem.Values)
            {
                SendItemInfo(info);
            }
        }

        //从dbserver接收到的道具信息
        public void AddItemInfo(GameBase.Network.Internal.RoleData_Item item)
        {
            GameStruct.RoleItemInfo info = new GameStruct.RoleItemInfo();
            info.id = item.id;
            info.itemid = item.itemid;
            info.postion = item.postion;
            info.stronglv = item.stronglv;
       
            info.gem1 = item.gem1;
            info.gem2 = item.gem2;
            info.forgename = item.forgename;
            info.amount = item.amount;
            info.war_ghost_exp = item.war_ghost_exp;
            info.di_attack = item.di_attack;
            info.shui_attack = item.shui_attack;
            info.huo_attack = item.huo_attack;
            info.feng_attack = item.feng_attack;
            info.property = item.property;
            info.gem3 = item.gem3;
            info.god_exp = item.god_exp;
            info.god_strong = item.god_strong;
            mDicItem[info.id] = info;
            if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK)
            {
                info.typeid = IDManager.CreateTypeId(OBJECTTYPE.EUDEMON);
            }
            //if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK)
            //{
            //    mBagCount++;
            //}
            CalcEquipLook();
        }

        //通过物品id删除一件道具
        public bool DeleteItemByID(uint id)
        {
            //幻兽id特殊处理
            uint _id = id;
            if (id >= IDManager.eudemon_start_id)
            {
                _id = GetEudemonItemId(id);

            }
            if (mDicItem.ContainsKey(_id))
            {
                GameStruct.RoleItemInfo info = mDicItem[_id];
                mDicItem.Remove(_id);
                //幻兽
                if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK)
                {
                    play.GetEudemonSystem().DeleteEudemon(id);
                }
                else
                {
                    this.ClearItem(id);
                }
                
                //身上装备- 重新计算属性
                if (info.postion >= NetMsg.MsgItemInfo.ITEMPOSITION_ARMOR && info.postion <= NetMsg.MsgItemInfo.ITEMPOSTION_RUB_SHENGYAOFUWEN)
                {
                    if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_ARMOR ||
                        info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_WEAPONR)
                    {
                        CalcEquipLook(info);
                    }
                    play.CalcAttribute();

                }
      
      

                //发给数据库服务器，通知删除道具
                GameBase.Network.Internal.DeleteItemByID dbdata = new GameBase.Network.Internal.DeleteItemByID();
                dbdata.id = _id;
                dbdata.playerid = play.GetBaseAttr().player_id;
                dbdata.postion = info.postion;
                DBServer.Instance().GetDBClient().SendData(dbdata.GetBuffer());
                return true;
            }
            return false;
        }

        public int GetItemCount(uint itemid)
        {
            int ret = 0;
            foreach (GameStruct.RoleItemInfo info in mDicItem.Values)
            {
                if (info.itemid == itemid)
                {
                    ret++;
                }
            }
            return ret;
        }

   
        //删除道具-从基本物品id 
        public bool DeleteItemByItemID(uint itemid, int count = 1)
        {
            if (GetItemCount(itemid) < count) return false;
            List<uint> dellist = null;
            int amount = 0;
            foreach (GameStruct.RoleItemInfo info in mDicItem.Values)
            {
                if (info.itemid == itemid)
                {
                    if (dellist == null) dellist = new List<uint>();
                    dellist.Add(info.id);
                    amount++;
                    if (amount == count) break;
                }
            }
            if (dellist.Count > 0)
            {
                for (int i = 0; i < dellist.Count; i++)
                {
                    this.DeleteItemByID(dellist[i]);
                }
            }
            return true;
        }
        //保存
        public void DB_Save()
        {
            if (mDicItem.Count <= 0) return;
            GameBase.Network.Internal.ROLEDATA_ITEM item = new GameBase.Network.Internal.ROLEDATA_ITEM();
            item.SetSaveTag();
            item.playerid = play.GetBaseAttr().player_id;
            foreach (GameStruct.RoleItemInfo info in mDicItem.Values)
            {
                GameBase.Network.Internal.RoleData_Item _item = new GameBase.Network.Internal.RoleData_Item();
                _item.id = info.id;
                _item.itemid = info.itemid;
                _item.postion = info.postion;
                _item.stronglv = info.stronglv;
              
                _item.gem1 = info.gem1;
                _item.gem2 = info.gem2;
                _item.forgename = info.forgename;
                _item.amount = info.amount;
                _item.war_ghost_exp = info.war_ghost_exp;
                _item.di_attack = info.di_attack;
                _item.shui_attack = info.shui_attack;
                _item.huo_attack = info.huo_attack;
                _item.feng_attack = info.feng_attack;
                _item.property = info.property;
                _item.gem3 = info.gem3;
                _item.god_exp = info.god_exp;
                _item.god_strong = info.god_strong;
  
                item.mListItem.Add(_item);
            }

            DBServer.Instance().GetDBClient().SendData(item.GetBuffer());
        }

        //穿戴装备
        public void Equip(uint id, uint postion)
        {
            if (!mDicItem.ContainsKey(id)) return;
          
            //等级不够啊--
            GameStruct.RoleItemInfo newEquip = mDicItem[id];
            //如果该未知有装备..就发回到包裹
            GameStruct.RoleItemInfo oldequip = GetEquipByPostion((byte)postion);
            if (oldequip != null)
            {
                UnEquip(oldequip.id, 0, false);
            }
         
            newEquip.postion = (ushort)postion;
            NetMsg.MsgOperateEquip send = new NetMsg.MsgOperateEquip();
            send.SetTagEquip();
            send.Create(null, play.GetGamePackKeyEx());
            send.equipid = newEquip.id;
            send.postion = (int)postion;
            play.SendData(send.GetBuffer());
            newEquip.postion = (ushort)postion;
            CalcEquipLook(newEquip);
        }
        //脱下装备
        //参数: id: 要脱下的装备id
        // oldpostion :要脱下的装备位置
        public void UnEquip(uint id, uint oldpostion,bool isChangeLook = true)
        {
            if (!mDicItem.ContainsKey(id)) return;
            if (IsItemFull()) return; //包裹已满。。脱不下了
            GameStruct.RoleItemInfo info = mDicItem[id];
           // if (info.postion != oldpostion) return;
            //如果是时装- 就放到衣柜 2015.10.3
            if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_FASHION
                )
            {
                info.postion = NetMsg.MsgItemInfo.ITEMPOSITION_CHEST;
            }
            //幻魂武器放到衣柜 2015.10.14
            else if(info.postion == NetMsg.MsgItemInfo.ITEMPOSTION_WEPON_SOUL)
            {
                info.postion = NetMsg.MsgItemInfo.ITEMPOSITION_CHEST_SOUL;
            }
            else
            {
                info.postion = NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK;
            }
         
            NetMsg.MsgOperateEquip send = new NetMsg.MsgOperateEquip();
            send.SetTagUnEquip();
            send.Create(null, play.GetGamePackKeyEx());
            send.equipid = info.id;
            send.postion = (int)oldpostion;
            play.SendData(send.GetBuffer());
            if(isChangeLook)CalcEquipLook(info);
        }

        public void UseItem(uint id, uint dwdata,short param,short param1)
        {
            if (!mDicItem.ContainsKey(id)) return;
            GameStruct.RoleItemInfo info = mDicItem[id];
            GameStruct.ItemTypeInfo baseitem = ConfigManager.Instance().GetItemTypeInfo(info.itemid);
            if (baseitem == null)
            {
                Log.Instance().WriteLog("玩家使用道具:,道具不存在: id:" + id.ToString() + " 基本id:" + info.itemid.ToString());
                if (!GameServer.IsTestMode()) return;
                baseitem = new GameStruct.ItemTypeInfo();
                baseitem.id = info.itemid;
            }
            //等级不够
            if (play.GetBaseAttr().level < baseitem.req_level)
            {
                play.ChatNotice("等级不够,无法使用.");
                return;
            }
            //职业不符
            if (baseitem.req_profession != 0 && baseitem.req_profession != play.GetBaseAttr().profession)
            {
                play.ChatNotice("职业不符,无法使用.");
                return;
            }
            //是装备就穿戴装备
            if (IsEquip(baseitem.id))
            {
                //再次校验一下装备穿戴未知 防止非法封包
                if (GetEquipPostion(baseitem) != dwdata)
                {

                    return;
                }
                Equip(id, dwdata);
                return;
            }

            //特殊道具的处理
            play.SetUseItemEudemonId(0);
            if (baseitem.id == GameBase.Config.Define.ITEM_DIANJIANGYAOSHUI_ID)
            {
                uint nUseItemEudemonId = (uint)BaseFunc.MakeLong(param, param1);
                play.SetUseItemEudemonId(nUseItemEudemonId);
            }
            //执行脚本
            if (baseitem.actionid > 0)
            {
                mScriptItemId = id;
                ScripteManager.Instance().ExecuteAction(baseitem.actionid, play);
            }

        }

        //从身上掉落的装备
        public void DropItemEquip(uint id)
        {
            uint _id = id;
            if (!mDicItem.ContainsKey(_id)) return;
            GameStruct.RoleItemInfo info = mDicItem[_id];
            short x = 0; short y = 0;
            if (!this.GetDropItemPoint(ref x, ref y)) return;
            DeleteItemByID(id);

            //加到地图上
            this.play.GetGameMap().AddDropItemObj(info.itemid, x, y, 0, 120000, info, null);
        }
        //从包裹掉落的装备
        public void DropItemBag(uint id)
        {
            //摆摊状态下不允许丢弃道具
            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) != null)
            {
                play.MsgBox("摆摊状态下不允许丢弃道具!");
                return;
            }
            //丢弃幻兽特殊处理
        
            uint _id = id;
            if (id >= IDManager.eudemon_start_id)
            {
                _id = GetEudemonItemId(id);
                
            }
            if (!mDicItem.ContainsKey(_id)) return;
            GameStruct.RoleItemInfo info = mDicItem[_id];
            //优先判断角色所在位置能不能扔道具--不能扔就不删除了
            short x = 0; short y = 0;
            if (!this.GetDropItemPoint(ref x, ref y)) return;
            //幻兽也要处理
            RoleData_Eudemon eudemon = null;
            if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK)
            {
                eudemon = play.GetEudemonSystem().FindEudemon(id);
                //出征合体的幻兽不允许丢弃
                if (eudemon == null || (eudemon != null && play.GetEudemonSystem().GetBattleEudemon(eudemon.GetTypeID()) != null))
                {
                    return;
                }
            }
         
            DeleteItemByID(id);
          
            //加到地图上
            this.play.GetGameMap().AddDropItemObj(info.itemid, x, y, 0, 120000, info, eudemon);
        }

        //丢弃金钱
        //gold 金钱数量
        public void DropGold(int gold)
        {
            play.ChangeAttribute(GameStruct.UserAttribute.GOLD, -gold);
            play.LeftNotice(string.Format("您遗失了{0}金币!", gold));
            GameStruct.RoleItemInfo info = new GameStruct.RoleItemInfo();
            info.property = gold;
            short x = 0; short y = 0;
            if (!this.GetDropItemPoint(ref x, ref y)) return;
            if (gold < 10)
            {
                info.itemid = GameBase.Config.Define.GOLD_ITEM_1; //几个金币
            }
            else if (gold > 10 && gold < 100)
            {
                info.itemid = GameBase.Config.Define.GOLD_ITEM_2; //一些金币
            }
            else if (gold > 100 && gold < 500)
            {
                info.itemid = GameBase.Config.Define.GOLD_ITEM_3; //许多金币
            }
            else if (gold > 500 && gold < 1500)
            {
                info.itemid = GameBase.Config.Define.GOLD_ITEM_4; //一小堆金币
            }
            else info.itemid = GameBase.Config.Define.GOLD_ITEM_5; //一大堆金币
            this.play.GetGameMap().AddDropItemObj(info.itemid, x, y, 0, 120000,info);
        }
        public bool IsItemFull()
        {
            if (GameServer.IsTestMode()) return false;
            return GetBagCount() >= MAXBAG_COUNT;
        }

        //根据装备位置取道具信息
        public GameStruct.RoleItemInfo GetEquipByPostion(byte postion)
        {
            foreach (GameStruct.RoleItemInfo info in mDicItem.Values)
            {
                if (info.postion == postion)
                {
                    return info;
                }
            }
            return null;
        }


        public bool IsEquip(uint itemid)//是否是装备
        {
            GameStruct.ItemTypeInfo baseinfo = ConfigManager.Instance().GetItemTypeInfo(itemid);
            if (baseinfo == null)
            {
                if (!GameServer.IsTestMode()) return false;
                else return true;

            }
            if (GetEquipPostion(baseinfo) != 0)
            {
                return true;
            }
            return false;
        }

        public GameStruct.RoleItemInfo FindItem(uint id)
        {
            if (mDicItem.ContainsKey(id))
            {
                return mDicItem[id];
            }
            return null;
        }

        //根据道具基本数据库查找道具，成功返回一个找到的道具
        //name 道具基本id
        //nCount 数量
        public GameStruct.RoleItemInfo FindItem(uint itemid, ref int nCount)
        {
            GameStruct.RoleItemInfo ret = null;
            foreach (GameStruct.RoleItemInfo info in mDicItem.Values)
            {
                GameStruct.ItemTypeInfo item = ConfigManager.Instance().GetItemTypeInfo(info.itemid);
                if (item != null && item.id == itemid)
                {
                    if (ret == null) ret = info;
                    nCount++;

                }
            }
            return ret;
        }
        //根据道具名称查找道具,成功返回第一个找到的道具
        //name 道具名称
        //nCount 数量
        public GameStruct.RoleItemInfo FindItem(string name,ref int nCount)
        {
            GameStruct.RoleItemInfo ret = null;
            foreach (GameStruct.RoleItemInfo info in mDicItem.Values)
            {
                GameStruct.ItemTypeInfo item = ConfigManager.Instance().GetItemTypeInfo(info.itemid);
                if (item != null && item.name == name)
                {
                    if (ret == null) ret = info;
                    nCount++;
                   
                }
            }
            return ret;
        }

        //删除道具- 从道具名称
        public void DeleteItemByItemName(string name, int count = 1)
        {
           
            List<GameStruct.RoleItemInfo> list = new List<GameStruct.RoleItemInfo>();
            foreach (GameStruct.RoleItemInfo info in mDicItem.Values)
            {
                GameStruct.ItemTypeInfo item = ConfigManager.Instance().GetItemTypeInfo(info.itemid);
                if (item != null && item.name == name)
                {
                    this.DeleteItemByID(info.id);
                    list.Add(info);
                    
                    if (list.Count == count) break;

                }
            }
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    this.DeleteItemByID(list[i].id);
                }
            }
            
        }


        private bool GetDropItemPoint(ref short x, ref short y)
        {
            //由目标中心点衍生周围坐标点遍历--
            x = play.GetCurrentX();
            y = play.GetCurrentY();
            short[] _DELTA_X = { 0, -1, -1, -1, 0, 1, 1, 1, 0 };
            short[] _DELTA_Y = { 1, 1, 0, -1, -1, -1, 0, 1, 0 };
            //最多绕怪物四圈。。4*8 =32 爆32件道具
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int tempx = x + (_DELTA_X[j] * i + 1);
                    int tempy = y + (_DELTA_Y[j] * i + 1);
                    if (!play.GetGameMap().GetPointOfObj(play, (short)tempx, (short)tempy))
                    {
                        x = (short)tempx;
                        y = (short)tempy;
                        return true;
                    }
                }
            }

            //丢弃的道具可以重叠坐标点
            //    //2015.10.19
            return false;
        }
        //购买魔石商店道具
        public void BuyGameShopItem(uint itemid, int nAmount)
        {
            if (this.GetBagCount() + nAmount > MAXBAG_COUNT) return; //防止溢出
            GameStruct.ItemTypeInfo baseitem = ConfigManager.Instance().GetItemTypeInfo(itemid);
            if (baseitem == null) return;
           
            GameStruct.NpcShopInfo info = ConfigManager.Instance().GetNpcShopInfo(GameBase.Config.Define.GMAESHOPID);
            if (info == null) return;
            int money = info.GetItemPrice(itemid) * nAmount;
            if (money <= 0) return;
            if (play.GetBaseAttr().gamegold < money) return;
            play.ChangeAttribute(GameStruct.UserAttribute.GAMEGOLD, -money);
            for (int i = 0; i < nAmount; i++)
            {
                this.AwardItem(itemid, NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK);
            }

              
        }
        //购买道具
        public void BuyItem(uint npcid, uint itemid)
        {
            if (play.GetCurrentNpcInfo() == null) return;
            if (play.GetCurrentNpcInfo().id != npcid) return;
            GameStruct.ItemTypeInfo baseitem = ConfigManager.Instance().GetItemTypeInfo(itemid);
            if (baseitem == null) return;
            GameStruct.NpcShopInfo info = ConfigManager.Instance().GetNpcShopInfo(npcid);
            if (info == null) return;
            int price = info.GetItemPrice(itemid);
            if (price == -1) return;
            if (play.GetMoneyCount(GameStruct.MONEYTYPE.GOLD) < price) return;
            play.ChangeMoney(GameStruct.MONEYTYPE.GOLD, -price);
            this.AwardItem(itemid, NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK);
        }

        //卖出道具- 
        public void SellItem(uint npcid, uint itemid)
        {
            if (play.GetCurrentNpcInfo() == null) return;
            if (play.GetCurrentNpcInfo().id != npcid) return;
            GameStruct.RoleItemInfo roleitem = FindItem(itemid);
            if (roleitem == null) return;
            GameStruct.ItemTypeInfo baseitem = ConfigManager.Instance().GetItemTypeInfo(roleitem.itemid);
            if (baseitem == null) return;
            //八折-
            int price = (int)(baseitem.price * 0.8);
            play.ChangeAttribute(GameStruct.UserAttribute.GOLD, price);
            DeleteItemByID(itemid);
        }
        //修理装备- 目前设置装备不损耗- 暂时不做2015.9.21
        public void RepairEquip(uint npcid, uint itemid)
        {
            if (play.GetCurrentNpcInfo() == null) return;
            if (play.GetCurrentNpcInfo().id != npcid) return;
            GameStruct.RoleItemInfo info = FindItem(itemid);
            if (info == null) return;
            
        }

        public void ClearItem(uint id)
        {
            //幻兽id特殊处理
            uint _id = id;
            if (id >= IDManager.eudemon_start_id)
            {
                _id = GetEudemonItemId(id);

            }
            NetMsg.MsgClearItem clear = new NetMsg.MsgClearItem();
          
            clear.id = _id;
            clear.roleid = play.GetTypeId();
            play.SendData(clear.GetBuffer(),true);
         
        }
        //获取仓库道具列表信息
        public void GetItemStrongInfo(List<GameStruct.RoleItemInfo> list)
        {
            foreach (GameStruct.RoleItemInfo obj in mDicItem.Values)
            {
                if (obj.postion == NetMsg.MsgItemInfo.ITEMPOSTION_STRONG_PACK)
                {
                    list.Add(obj);
                }
            }
        }

        //移动道具- 比如 从仓库移动到包裹 从包裹移动到仓库
        public void MoveItem(uint id, ushort dest_postion)
        {
            if (!mDicItem.ContainsKey(id)) return;
            GameStruct.RoleItemInfo info = mDicItem[id];
            ushort src_postion = info.postion;
            //如果源道具在包裹- 通知删除这个道具
            switch (src_postion)
            {
                case NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK:
                    {
                        this.ClearItem(id);
                        break;
                    }
            }

            info.postion = dest_postion;

            switch(dest_postion)
            {
                case NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK:
                    {
                        this.UpdateItemInfo(id);
                        break;
                    }
  
            }

            if(src_postion == NetMsg.MsgItemInfo.ITEMPOSTION_STRONG_PACK ||
                dest_postion == NetMsg.MsgItemInfo.ITEMPOSTION_STRONG_PACK)
            {
                play.OpenDialog(NetMsg.MsgOpenDialog.OPENDIALOGTYPE_STRONG);
            }
           
        }

        //仓库存钱小心溢出！
        public void SaveStrongMoney(int gold)
        {
            if (play.GetMoneyCount(GameStruct.MONEYTYPE.GOLD) < gold) return;
            play.ChangeAttribute(GameStruct.UserAttribute.GOLD, -gold);
            NetMsg.MsgStrongInfo stronginfo = new NetMsg.MsgStrongInfo();
            stronginfo.Create(null, play.GetGamePackKeyEx());
            play.ChangeMoney(GameStruct.MONEYTYPE.STRONGGOLD, gold);
            byte[] data = NetMsg.MsgStrongInfo.GetStrongMoneyBuffer(play.GetTypeId(), play.GetMoneyCount(GameStruct.MONEYTYPE.STRONGGOLD));
            play.SendData(data,true);
        }
        //仓库取钱 小心溢出！
        public void GiveStrongMoney(int gold)
        {
            if (play.GetMoneyCount(GameStruct.MONEYTYPE.STRONGGOLD) < gold) return;
            play.ChangeAttribute(GameStruct.UserAttribute.GOLD, gold);
            play.ChangeMoney(GameStruct.MONEYTYPE.STRONGGOLD, -gold);
            //发送仓库剩余道具
            NetMsg.MsgStrongInfo stronginfo = new NetMsg.MsgStrongInfo();
            stronginfo.Create(null, play.GetGamePackKeyEx());
            byte[] data = NetMsg.MsgStrongInfo.GetStrongMoneyBuffer(play.GetTypeId(), play.GetMoneyCount(GameStruct.MONEYTYPE.STRONGGOLD));
            play.SendData(data,true);
        }

        //取仓库道具数量
        public int GetStrongItemCount()
        {
            int amount = 0;
            foreach (GameStruct.RoleItemInfo obj in mDicItem.Values)
            {
                if (obj.postion == NetMsg.MsgItemInfo.ITEMPOSTION_STRONG_PACK)
                {
                    amount++;
                }
            }
            return amount;
        }
        //取幻兽背包幻兽数量
        public int GetEudemonCount()
        {
            int amount = 0;
            foreach (GameStruct.RoleItemInfo obj in mDicItem.Values)
            {
                if (obj.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK)
                {
                    amount++;
                }
            }
            return amount;
            
        }

        public uint GetEudemonItemId(uint eudemon_id)
        {
            foreach (GameStruct.RoleItemInfo obj in mDicItem.Values)
            {
                if (obj.typeid == eudemon_id)
                {
                    return obj.id;
                }
            }
            return 0;
        }

        //更改头像
        public void ChangeLookFace(uint itemid)
        {
            GameStruct.LookFaceInfo info = ConfigManager.Instance().GetLookFaceInfo(itemid);
            if (info == null) return;
            if (play.GetMoneyCount(GameStruct.MONEYTYPE.GOLD) < info.price)
            {
                play.LeftNotice("金币不足,无法购买!");
                return;
            }
            if ((play.GetSex() % 2) != (info.lookfaceid % 2))
            {
                play.LeftNotice("性别不符,无法购买!");
                    return;
            }
            play.ChangeMoney(GameStruct.MONEYTYPE.GOLD, -info.price);
            play.ChangeAttribute(GameStruct.UserAttribute.LOOKFACE, info.lookfaceid);
        }
        //更改发型
        public void ChangeHair(uint itemid)
        {
            GameStruct.HairInfo info = ConfigManager.Instance().GetHairInfo(itemid);
            if (info == null) return;
            if (play.GetMoneyCount(GameStruct.MONEYTYPE.GOLD) < info.price)
            {
                play.LeftNotice("金币不足,无法购买!");
                return;
            }
            if (play.GetSex() != info.sex)
            {
                play.LeftNotice("性别不符,无法购买!");
                return;

            }
            play.ChangeMoney(GameStruct.MONEYTYPE.GOLD, info.hairid);
            play.ChangeAttribute(GameStruct.UserAttribute.HAIR, info.hairid);
        }
        //时装取回到包裹
        public void Give_FashionChest(uint itemid)
        {
            if (!mDicItem.ContainsKey(itemid)) return;
            GameStruct.RoleItemInfo info = mDicItem[itemid];
            if (this.IsItemFull()) return; //包裹已满
            info.postion = NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK;
            this.UpdateItemInfo(info.id);
        }

        //返还交易道具
        public void AddTradItem(GameStruct.RoleItemInfo info)
        {
            mDicItem[info.id] = info;
            SendItemInfo(info);
        }

        public byte GetEquipPostion(GameStruct.ItemTypeInfo info)
        {
            String sID = info.id.ToString();
            //法宝特殊
            const uint FABAO_SHUGUANGZHANHUN = 1110010;
            const uint FABAO_DILONGZHILEI = 1110110;
            const uint FABAO_SHENGYAOFUWEN = 1110210;
            if (info.id == FABAO_SHUGUANGZHANHUN)
            {
                return NetMsg.MsgItemInfo.ITEMPOSTION_RUB_SHUGUANGZHANHUN;
            }
            else if (info.id == FABAO_DILONGZHILEI)
            {
                return NetMsg.MsgItemInfo.ITEMPOSTION_RUB_DILONGZHILEI;
            }
            else if (info.id == FABAO_SHENGYAOFUWEN)
            {
                return NetMsg.MsgItemInfo.ITEMPOSTION_RUB_SHENGYAOFUWEN; 
            }
            //武器幻魂-
            if (sID[0] == '4' && sID[2] == '5')
            {
                return NetMsg.MsgItemInfo.ITEMPOSTION_WEPON_SOUL;
            }
            if (sID[0] == '4')  //武器
            {
                return NetMsg.MsgItemInfo.ITEMPOSITION_WEAPONR;
            }
            else if (sID[0] == '1' && sID[1] == '1') //头盔
            {
                return NetMsg.MsgItemInfo.ITEMPOSITION_HELMET;
            }else if(sID[0] == '1' && sID[1] =='2') //项链
            {
                return NetMsg.MsgItemInfo.ITEMPOSITION_NECKLACE;
            }
            else if (sID[0] == '1' && sID[1] == '3') //盔甲
            {
                return NetMsg.MsgItemInfo.ITEMPOSITION_ARMOR;
            }
            else if (sID[0] == '1' && sID[1] == '4') //手镯
            {
                return NetMsg.MsgItemInfo.ITEMPOSITION_TREASURE;
            }
            else if (sID[0] == '1' && sID[1] == '6') //靴子
            {
                return NetMsg.MsgItemInfo.ITEMPOSITION_SHOES;
            }
            else if (sID[0] == '1' && sID[1] == '9')//时装
            {
                return NetMsg.MsgItemInfo.ITEMPOSITION_FASHION;
            }
            return 0;
        }

        //查看装备- 把自身装备信息发给对方
        public void SendLookRoleInfo(PlayerObject target)
        {
             //装备属性--
            for (int i = NetMsg.MsgItemInfo.ITEMPOSITION_HELMET; i < NetMsg.MsgItemInfo.ITEMPOSTION_RUB_SHENGYAOFUWEN + 1; i++)
            {
                GameStruct.RoleItemInfo info = this.GetEquipByPostion((byte)i);
                if (info != null)
                {
                    target.GetItemSystem().SendItemInfo(info, NetMsg.MsgItemInfo.TAG_LOOKROLEINFO);
                }
            }
            //战斗力---
            //248,0,0,0 为战斗力
//            //   收到网络协议:长度：40协议号:2036
            //248000 
            byte[] data = {  1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 69, 0, 0, 0 };
            PacketOut outpack = new PacketOut();
            outpack.WriteInt16(40);
            outpack.WriteInt16(2036);
            outpack.WriteInt32(524359);
            outpack.WriteUInt32(play.GetTypeId());
            outpack.WriteInt32(play.GetFightSoul()); //战斗力
            outpack.WriteBuff(data);
            target.SendData(outpack.Flush(), true);

////收到网络协议:长度：17协议号:1015
//            byte[] data1 = { 17, 0, 247, 3, 144, 177, 177, 5, 0, 1, 1, 4, 176, 161, 193, 200, 0 };
//            target.SendData(data1, true);
////收到网络协议:长度：15协议号:1015
//            outpack = new PacketOut();
//            outpack.WriteInt16(15);
//            outpack.WriteInt16(1015);
//            outpack.WriteUInt32(play.GetTypeId());
//            outpack.WriteInt16(16);
//            outpack.WriteByte(1);
//            outpack.WriteByte(2);
//            outpack.WriteByte(206);
//            outpack.WriteByte(222);
//            outpack.WriteByte(0);
//            target.SendData(outpack.Flush(), true);


        }

        public void Process_DieEudemon()
        {
            List<uint> del_list = new List<uint>();
            foreach (GameStruct.RoleItemInfo info in mDicItem.Values)
            {
                if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK)
                {
                    EudemonObject obj = play.GetEudemonSystem().GetEudmeonObject(info.typeid);
                    if (obj == null)
                    {
                        del_list.Add(info.id);
                      
                    }
                }
            }
            if (del_list.Count > 0)
            {
                for (int i = 0; i < del_list.Count; i++) { this.DeleteItemByID(del_list[i]); }
            }
        }
    }
}
