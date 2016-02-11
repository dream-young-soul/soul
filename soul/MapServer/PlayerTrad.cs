using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Network;
using GameBase.Network.Internal;


//交易系统 10.4
namespace MapServer
{
    
    public class PlayerTrad
    {

        public PlayerObject play;
        private uint mTargetId = 0;
        private bool mIsTrad = false;
        private int mnGold; //交易金币
        private int mnGameGold; //交易魔石
        private bool mbSureTrad;
        private List<GameStruct.RoleItemInfo> mListItem;
        private List<GameBase.Network.Internal.RoleData_Eudemon> mListEudemon; //幻兽数据
        public PlayerTrad(PlayerObject _play)
        {
            mnGameGold = mnGold = 0;
            play = _play;
            mbSureTrad = false;
            mListItem = new List<GameStruct.RoleItemInfo>();
            mListEudemon = new List<GameBase.Network.Internal.RoleData_Eudemon>();
        }
        
        public void SetTrading(bool v) 
        { 
            mIsTrad = v;
            PacketOut outpack = null;
            if (mIsTrad)
            {
                PlayerObject target = UserEngine.Instance().FindPlayerObjectToTypeID(GetTradTarget());
                if (target == null)
                {
                    SetTrading(false);
                    return;
                }
                outpack = new PacketOut(target.GetGamePackKeyEx());
                outpack.WriteUInt16(16);
                outpack.WriteUInt16(1056);
                outpack.WriteUInt32(play.GetTypeId());
 
                outpack.WriteUInt32(3);
                outpack.WriteUInt32(1);
                target.SendData(outpack.Flush());

 
               
            }
            else
            {
                byte[] data = { 16, 0, 32, 4, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0 };
                outpack = new PacketOut(play.GetGamePackKeyEx());
                outpack.WriteBuff(data);
                play.SendData(outpack.Flush());
                play.LeftNotice("交易失败！");

                //返还魔石、金币 道具与装备
                play.ChangeAttribute(GameStruct.UserAttribute.GOLD, mnGold);
                if (mnGameGold > 0)
                {
                    play.ChangeAttribute(GameStruct.UserAttribute.GAMEGOLD, mnGameGold);
                }
                if (mnGold > 0)
                {
                    play.ChangeAttribute(GameStruct.UserAttribute.GOLD, mnGameGold);
                }
                //装备
                for (int i = 0; i < mListItem.Count; i++)
                {
                    play.GetItemSystem().AddTradItem(mListItem[i]);
                 
                }
                //返还幻兽
                for (int i = 0; i < mListEudemon.Count; i++)
                {
                    play.GetEudemonSystem().AddEudemon(mListEudemon[i]);   
                }
                mListItem.Clear();
                mListEudemon.Clear();
            }
        }
        public bool IsTrading() { return mIsTrad; }



        //请求交易
        public void RequstTrad(NetMsg.MsgTradInfo info)
        {
            PlayerObject obj = UserEngine.Instance().FindPlayerObjectToTypeID(info.typeid);
            if (obj == null)
            {
                play.LeftNotice("对方已离线,无法交易!");
                return;
            }
            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) != null)
            {
                play.MsgBox("摆摊中无法交易!");
                return;
            }
            if (obj.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) != null)
            {
                play.MsgBox("对方摆摊中，无法交易!");
                return;
            }
            if (this.GetTradTarget() == info.typeid) //同意交易
            {
                obj.GetTradSystem().SetTrading(true);
                play.GetTradSystem().SetTrading(true);
                return;
            }
            else if(this.GetTradTarget() != 0 )//正在交易中- 无法再次交易
            {
                play.LeftNotice("正在交易中,无法再次交易");
                return;
            }

            //请求交易
            //距离判断
            int x = Math.Abs(play.GetCurrentX() - obj.GetCurrentX());
            int y = Math.Abs(play.GetCurrentY() - obj.GetCurrentY());
            if (x >  GameBase.Config.Define.MAX_PLAY_VISIBLE_DISTANCE || 
                y >  GameBase.Config.Define.MAX_PLAY_VISIBLE_DISTANCE)
            {
                play.LeftNotice("距离太远,无法交易");
                return;
            }
            NetMsg.MsgTradInfo data = new NetMsg.MsgTradInfo();
            data.Create(null, obj.GetGamePackKeyEx());
            data.typeid = play.GetTypeId();
            data.type = NetMsg.MsgTradInfo.REQUEST_TRAD;
            data.level = play.GetBaseAttr().level;
            data.fightpower = (short)play.GetFightSoul();
            obj.SendData(data.GetBuffer());
            obj.GetTradSystem().SetTradTarget(play.GetTypeId());
            play.GetTradSystem().SetTradTarget(obj.GetTypeId());
            play.LeftNotice("[交易]已经发出交易请求。");
            
        }

        //取消交易
        public void QuitTrad(NetMsg.MsgTradInfo info)
        {
            if (GetTradTarget() == 0) return;
            this.SetTrading(false);
            this.SetSureTradTag(false);
            PlayerObject obj = UserEngine.Instance().FindPlayerObjectToTypeID(GetTradTarget());
            if (obj == null)
            {
                return;
            }
            this.SetTradTarget(0);
            obj.GetTradSystem().SetTradTarget(0);
            obj.GetTradSystem().SetTrading(false);
            obj.GetTradSystem().SetSureTradTag(false);
        }
        public void SetTradTarget(uint typeid)
        {
            mTargetId = typeid;
        }
        public uint GetTradTarget() { return mTargetId; }

        public void SetTradGold(int gold)
        {
            if (gold <= 0) return;
            PlayerObject obj = UserEngine.Instance().FindPlayerObjectToTypeID(GetTradTarget());
            if (obj == null)
            {
                return;
            }
            mnGold = gold;
            //发给对方
            PacketOut outpack = new PacketOut(obj.GetGamePackKeyEx()) ;
            outpack.WriteUInt16(16);
            outpack.WriteUInt16(1056);
            outpack.WriteInt32(mnGold);
            outpack.WriteInt32(8);
            outpack.WriteInt32(0);

            obj.SendData(outpack.Flush());

        
        }
        public int GetTradGold() { return mnGold; }

        public void SetTradGameGold(int gamegold)
        {
            if (gamegold <= 0) return;
            PlayerObject obj = UserEngine.Instance().FindPlayerObjectToTypeID(GetTradTarget());
            if (obj == null)
            {
                return;
            }
            mnGameGold = gamegold;

            //发给对方
            PacketOut outpack = new PacketOut(obj.GetGamePackKeyEx());
            outpack.WriteUInt16(16);
            outpack.WriteUInt16(1056);
            outpack.WriteInt32(mnGameGold);
            outpack.WriteInt32(12);
            outpack.WriteInt32(0);
            obj.SendData(outpack.Flush());

          
        }
        public int GetTradGameGold() { return mnGameGold; }

        //确定交易标记
        public void SetSureTradTag(bool v)
        {
            mbSureTrad = v;
   
        }
        public bool GetSureTradTag()
        {
            return mbSureTrad;
        }
        

        //添加交易道具
        public void AddTradItem(uint itemid)
        {
            PlayerObject obj = UserEngine.Instance().FindPlayerObjectToTypeID(GetTradTarget());
            RoleData_Eudemon eudemon = null;
            if (obj == null)
            {
                return;
            }
            GameStruct.RoleItemInfo info = null;
            if (itemid >= IDManager.eudemon_start_id)
            {
                 eudemon = play.GetEudemonSystem().FindEudemon(itemid);
                if (eudemon == null) return;
                info = play.GetItemSystem().FindItem(eudemon.itemid);
            }
            else
            {
                info = play.GetItemSystem().FindItem(itemid);
            }
            
            if (info == null) return;
            //最多二十个道具
            if (mListItem.Count >= 20) return;
            //对面包裹满了-
            if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK)
            {
                if (obj.GetItemSystem().IsItemFull())
                {
                    play.LeftNotice("对方物品栏已满,无法放置更多道具");
                    return;
                }
            }//幻兽栏已满
            else if (info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK)
            {
                if (obj.GetEudemonSystem().IsEudemonFull())
                {
                    play.LeftNotice("对方幻兽栏已满,无法放置更多道具");
                    return;
                }
            }
           
 
            mListItem.Add(info);
            //幻兽数据
            //发给对方道具数据
            obj.GetItemSystem().SendItemInfo(info, NetMsg.MsgItemInfo.TAG_TRADITEM);
          
            if(info.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK)
            {
                eudemon = play.GetEudemonSystem().FindEudemon(itemid);
                if (eudemon != null)
                {
                    mListEudemon.Add(eudemon);
                    play.GetEudemonSystem().SendLookTradEudemonInfo(obj,eudemon);
                }
               
            }
        
          
            

        }
        public void ClearTradItem() { mListItem.Clear(); }

        public List<GameStruct.RoleItemInfo> GetTradItem() { return mListItem; }

        //确定交易
        public void SureTrad()
        {
            PlayerObject obj = UserEngine.Instance().FindPlayerObjectToTypeID(GetTradTarget());
            if (obj == null)
            {
                return;
            }
            play.GetTradSystem().SetSureTradTag(true);
            //通知对方已经确定交易了-- 
            byte[] data = { 16, 0, 32, 4, 0, 0, 0, 0, 10, 0, 0, 0, 0, 0, 0, 0 };
            obj.GetGamePackKeyEx().EncodePacket(ref data, data.Length);
            obj.SendData(data);

            if (!obj.GetTradSystem().GetSureTradTag()) //对方还没确定交易
            {
                return;
            }
            obj.GetTradSystem().Trad(play);
            play.GetTradSystem().Trad(obj);
            
        }
        //交易
        public void Trad(PlayerObject obj)
        {
            //互换魔石
            int nGold = obj.GetTradSystem().GetTradGold();
            if (nGold > 0)
            {
                play.ChangeAttribute(GameStruct.UserAttribute.GOLD, nGold);
            }
            //魔石
            int nGameGold = obj.GetTradSystem().GetTradGameGold();
            if (nGameGold > 0)
            {
                play.ChangeAttribute(GameStruct.UserAttribute.GAMEGOLD, nGameGold);
            }
            obj.GetTradSystem().SetTradGameGold(0);
            obj.GetTradSystem().SetTradGold(0);
            //道具
            List<GameStruct.RoleItemInfo> list = obj.GetTradSystem().GetTradItem();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK)
                {
                    RoleData_Eudemon eudemon = obj.GetEudemonSystem().FindEudemon(list[i].typeid);
                    if (eudemon != null)
                    {
                        play.GetEudemonSystem().AddTempEudemon(eudemon);
                    }
                  
                    
                }
                play.GetItemSystem().AwardItem(list[i]);
                obj.GetItemSystem().DeleteItemByID(list[i].id);
                
            }
            obj.GetTradSystem().ClearTradItem();
            //成功
            play.LeftNotice("交易成功");
            //关闭对话框
            SetSureTradTag(false);
            SetTradTarget(0);
            mIsTrad = false;
            byte[] data = { 16, 0, 32, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0 };
            play.GetGamePackKeyEx().EncodePacket(ref data, data.Length);
            play.SendData(data);

        }
    }

    
}
