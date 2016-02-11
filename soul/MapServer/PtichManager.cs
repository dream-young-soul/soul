using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStruct;
using GameBase.Network;
using GameBase.Network.Internal;
using GameBase.Core;

//摆摊全局管理器 2016.1.12
namespace MapServer
{

    //摊位出售道具信息
    class PtichSellItemInfo
    {
        public uint item_id;
        public byte sell_type; //出售类型  22.金币 52.魔石
        public int price;     //价格
    }
    //摆摊信息
    class PtichInfo
    {
        public int Id;
        public PlayerObject play;
        public PtichObject PtichObj;
        public List<PtichSellItemInfo> mSellItemList;

    }

    public class PtichManager
    {

        private static PtichManager mInstance = null;
        private List<PtichInfo> mListPtichInfo;
        public static PtichManager Instance()
        {
            if (mInstance == null)
            {
                mInstance = new PtichManager();
            }
            return mInstance;
        }

        public PtichManager()
        {
            mListPtichInfo = new List<PtichInfo>();
            for (int i = 0; i < GameBase.Config.Define.PTICH_MAX_COUNT; i++)
            {
                PtichInfo info = new PtichInfo();
                info.Id = i;
                info.play = null;
                info.PtichObj = null;

                info.mSellItemList = new List<PtichSellItemInfo>();
                mListPtichInfo.Add(info);
            }
        }
        //角色摆摊
        //nPtichId 摊位ID
        //play   摆摊对象
        public bool AddPlayPtich(int nPtichId, PlayerObject play)
        {
            if (nPtichId < 0 || nPtichId >= GameBase.Config.Define.PTICH_MAX_COUNT) return false;
            if (PtichHasPlay(nPtichId)) return false;

            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) != null) return false; //正在摆摊中
            play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_PTICH);


            mListPtichInfo[nPtichId].play = play;

            //设置方向
            play.SetDir(DIR.LEFT_DOWN);


            PacketOut outpack = new PacketOut();

            //网络连接堵塞提示
            //outpack.WriteInt16(16);
            //outpack.WriteInt16(1012);
            //outpack.WriteUInt32(play.GetTypeId());
            //outpack.WriteInt32(0);
            //outpack.WriteInt32(0);
            //play.SendData(outpack.Flush(), true);

            //这个不知道是什么鬼
            //outpack = new PacketOut();
            //outpack.WriteInt16(20);
            //outpack.WriteInt16(1017);
            //outpack.WriteUInt32(play.GetTypeId());
            //outpack.WriteInt32(1);
            //outpack.WriteInt32(4);
            //outpack.WriteInt32(60317); //157, 235, 0, 0
            //play.SendData(outpack.Flush(),true);

            PtichObject obj = new PtichObject(play);
            obj.SetPoint((short)(play.GetCurrentX() + 1), (short)(play.GetCurrentY() + 1));

            play.GetGameMap().AddObject(obj);
            obj.Refresh();
            mListPtichInfo[nPtichId].PtichObj = obj;
            //收到网络协议:长度：28协议号:1010
            outpack = new PacketOut();
            outpack.WriteInt16(28);
            outpack.WriteInt16(1010);
            outpack.WriteInt32(101088);
            outpack.WriteUInt32(play.GetTypeId());
            outpack.WriteInt16(obj.GetCurrentX());
            outpack.WriteInt16(obj.GetCurrentY());
            outpack.WriteInt32(0);
            outpack.WriteUInt32(obj.GetTypeId());
            outpack.WriteInt32(9570);
            play.SendData(outpack.Flush(), true);
            //byte[] data4 = { 28, 0, 242, 3, 224, 138, 1, 0, 174, 66, 15, 0, 91, 1, 27, 2, 0, 0, 0, 0, 28, 162, 1, 0, 98, 37, 0, 0 };
            //this.SendData(data4, true);

            return true;
        }

        //摊位是否被占用
        public bool PtichHasPlay(int nPtichId)
        {
            if (mListPtichInfo[nPtichId].play == null)
            {
                return false;
            }
            PlayerObject play = mListPtichInfo[nPtichId].play;
            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) != null)
            {
                return true;
            }
            return false;
        }

        //收摊
        public void DeletePlayPtich(PlayerObject play)
        {
            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) == null) return;

        }
        //获取摊位对象ID
        public uint GetPtichObjectTypeID(int nPtichId)
        {
            if (nPtichId < 0 || nPtichId >= mListPtichInfo.Count) return 0;
            if (mListPtichInfo[nPtichId].PtichObj == null) return 0;
            return mListPtichInfo[nPtichId].PtichObj.GetTypeId();
        }


        //摊位出售道具
        public void SellItem(PlayerObject play, uint item_id, byte type, int price)
        {

            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) == null) return;
            uint ptich_obj_id = GetPtichObjectTypeID(play.GetCurrentPtichID());
            GameStruct.RoleItemInfo item = null;
            if (ptich_obj_id == 0) return;
            if (item_id >= IDManager.eudemon_start_id)
            {

                RoleData_Eudemon eudemon = play.GetEudemonSystem().FindEudemon(item_id);
                if (eudemon == null) return;
                item = play.GetItemSystem().FindItem(eudemon.itemid);
                if (item == null) return;
            }
            else
            {
                item = play.GetItemSystem().FindItem(item_id);
                if (item == null) return;
            }

            if (ptich_obj_id == 0) return;
            //判断是否已经在出售摊位列表中- 反作弊
            int nPtichId = play.GetCurrentPtichID();
            for (int i = 0; i < mListPtichInfo[nPtichId].mSellItemList.Count; i++)
            {
                if (mListPtichInfo[nPtichId].mSellItemList[i].item_id == item_id)
                {
                    return;
                }
            }
            //摆摊出售道具已满
            if (mListPtichInfo[nPtichId].mSellItemList.Count >= GameBase.Config.Define.PTICH_SELL_MAX_COUNT)
            {
                return;
            }
            PacketOut outpack = new PacketOut();
            outpack.WriteInt16(28);
            outpack.WriteInt16(1009);
            outpack.WriteUInt32(item_id);
            outpack.WriteInt32(price);
            outpack.WriteInt32(type);
            outpack.WriteInt32(0);
            outpack.WriteInt32(0);
            outpack.WriteInt32(0);
            play.SendData(outpack.Flush(), true);
            //移到摊位状态
            item.postion = NetMsg.MsgItemInfo.ITEMPOSTION_PTICH_PACK;


            PtichSellItemInfo info = new PtichSellItemInfo();
            info.item_id = item_id;
            info.price = price;
            info.sell_type = type;
            mListPtichInfo[nPtichId].mSellItemList.Add(info);
            // {28,0,241,3,8,127,205,7,111,0,0,0,52,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
        }

        //摊位取回道具
        public void GetBackItem(PlayerObject play, uint item_id)
        {
            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) == null) return;
            uint ptich_obj_id = GetPtichObjectTypeID(play.GetCurrentPtichID());
            GameStruct.RoleItemInfo item = null;
            if (ptich_obj_id == 0) return;
            if (item_id >= IDManager.eudemon_start_id)
            {

                RoleData_Eudemon eudemon = play.GetEudemonSystem().FindEudemon(item_id);
                if (eudemon == null) return;
                item = play.GetItemSystem().FindItem(eudemon.itemid);
                if (item == null) return;
            }
            else
            {
                item = play.GetItemSystem().FindItem(item_id);
                if (item == null) return;
            }

            int nPtichId = play.GetCurrentPtichID();
            for (int i = 0; i < mListPtichInfo[nPtichId].mSellItemList.Count; i++)
            {
                if (mListPtichInfo[nPtichId].mSellItemList[i].item_id == item_id)
                {
                    mListPtichInfo[nPtichId].mSellItemList.RemoveAt(i);
                    break;
                }
            }
            //放回到包裹或幻兽背包
            if (item_id >= IDManager.eudemon_start_id)
            {
                item.postion = NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK;

            }
            else
            {
                item.postion = NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK;
            }

            PacketOut outpack = new PacketOut();
            outpack.WriteInt16(28);
            outpack.WriteInt16(1009);
            outpack.WriteUInt32(item_id);
            outpack.WriteUInt32(ptich_obj_id);
            outpack.WriteInt32((byte)NetMsg.MsgOperateItem.PTICH_GETBACK_SELLITEM);
            outpack.WriteInt32(0);
            outpack.WriteInt32(0);
            outpack.WriteInt32(0);
            play.SendData(outpack.Flush(), true);



        }
        //收摊
        //bSendData 是否回发收摊数据- 玩家下线就不发
        public void ShutPtich(PlayerObject play, bool bSendData = true)
        {
            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) == null) return;
            uint ptich_obj_id = GetPtichObjectTypeID(play.GetCurrentPtichID());
            if (ptich_obj_id == 0) return;

            for (int i = 0; i < mListPtichInfo[play.GetCurrentPtichID()].mSellItemList.Count; i++)
            {
                 GameStruct.RoleItemInfo item = null;
                 if (mListPtichInfo[play.GetCurrentPtichID()].mSellItemList[i].item_id >= IDManager.eudemon_start_id)
                 {
                     RoleData_Eudemon eudemon = play.GetEudemonSystem().FindEudemon(
                         mListPtichInfo[play.GetCurrentPtichID()].mSellItemList[i].item_id);
                     if (eudemon == null) continue;
                     item = play.GetItemSystem().FindItem(eudemon.itemid);

                 }
                 else
                 {
                     item = play.GetItemSystem().FindItem(
                  mListPtichInfo[play.GetCurrentPtichID()].mSellItemList[i].item_id);
                 }
                
                if (item != null)
                {
                    //放回到包裹
                    //放回到包裹或幻兽背包
                    if (item.typeid >= IDManager.eudemon_start_id)
                    {
                        item.postion = NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK;

                    }
                    else
                    {
                        item.postion = NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK;
                    }

                    if (bSendData)
                    {
                        PacketOut outpack = new PacketOut();
                        outpack.WriteInt16(28);
                        outpack.WriteInt16(1009);
                        outpack.WriteUInt32(item.id);
                        outpack.WriteUInt32(ptich_obj_id);
                        outpack.WriteInt32((byte)NetMsg.MsgOperateItem.PTICH_GETBACK_SELLITEM);
                        outpack.WriteInt32(0);
                        outpack.WriteInt32(0);
                        outpack.WriteInt32(0);
                        play.SendData(outpack.Flush(), true);
                    }
                }
            }
            mListPtichInfo[play.GetCurrentPtichID()].play = null;
            //移除地图对象
            play.GetGameMap().RemoveObj(mListPtichInfo[play.GetCurrentPtichID()].PtichObj);
            mListPtichInfo[play.GetCurrentPtichID()].PtichObj = null;
            mListPtichInfo[play.GetCurrentPtichID()].mSellItemList.Clear();
            if (bSendData)
            {
                PacketOut outpack = new PacketOut();
                outpack.WriteInt16(16);
                outpack.WriteInt16(2031);
                outpack.WriteUInt32(ptich_obj_id);
                outpack.WriteUInt32(play.GetTypeId());
                outpack.WriteInt32(2);
                play.SendData(outpack.Flush(), true);
            }
            //移除摆摊状态
            play.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_PTICH);
        }
        //查看摊位
        //ptich_obj_id 摊位对象id
        //page 页码
        public void LookPtich(PlayerObject play, uint ptich_obj_id)
        {
            int ptich_id = -1;
            for (int i = 0; i < mListPtichInfo.Count; i++)
            {
                if (mListPtichInfo[i].PtichObj == null) continue;
                if (mListPtichInfo[i].PtichObj.GetTypeId() == ptich_obj_id)
                {
                    ptich_id = i;
                    break;
                }
            }
            if (ptich_id == -1) return;
            // 摊位信息
            byte[] senddata = { 42, 0, 105, 4, 244, 1, 0, 0, 64, 66, 15, 0, 36, 52, 156, 8, 3, 0, 0, 0, 30, 214, 44, 135, 2, 0, 0, 0, 164, 3, 178, 5, 1, 0 };
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(senddata);
            outpack.WriteInt16((short)(ptich_id + 1));
            byte[] data2 = { 1, 4, 202, 165, 213, 189 };
            outpack.WriteBuff(data2);
            play.SendData(outpack.Flush(), true);
            //出售的道具信息
            for (int i = 0; i < mListPtichInfo[ptich_id].mSellItemList.Count; i++)
            {
                RoleItemInfo item_info = null;
                RoleData_Eudemon eudemon = null;
                if (mListPtichInfo[ptich_id].mSellItemList[i].item_id >= IDManager.eudemon_start_id)
                {
                    eudemon = mListPtichInfo[ptich_id].play.GetEudemonSystem().FindEudemon(
                        mListPtichInfo[ptich_id].mSellItemList[i].item_id);
                    if (eudemon == null) continue;
                    item_info = mListPtichInfo[ptich_id].play.GetItemSystem().FindItem( eudemon.itemid);
                }
                else
                {
                    item_info = mListPtichInfo[ptich_id].play.GetItemSystem().FindItem(
                    mListPtichInfo[ptich_id].mSellItemList[i].item_id);
                }
                
                if (item_info != null)
                {
                    NetMsg.MsgPtichItemInfo msg = new NetMsg.MsgPtichItemInfo(item_info,
                        ptich_obj_id, mListPtichInfo[ptich_id].mSellItemList[i].price,mListPtichInfo[ptich_id].mSellItemList[i].sell_type);
                    play.SendData(msg.GetBuffer(), true);
                    //发送幻兽信息
                    if (item_info.typeid >= IDManager.eudemon_start_id)
                    {
                        mListPtichInfo[ptich_id].play.GetEudemonSystem().SendLookPtichEudemonInfo(play, eudemon);
                       // play.GetEudemonSystem().SendEudemonInfo(eudemon, false, true);
                    }
                }


            }
            //道具id
            //摊位对象id
            //魔石价格
            //道具基础id
            //最大损耗  172, 38,
            //当前损耗  172, 38,
            //摊位栏 3
            //是否鉴定 0.已鉴定 1.未鉴定
            //未知 50
            //第一个宝石 30
            //第二个宝石 30
            //未知 0,0
            //强化等级 12
            //32.未知  
            //33.未知
            //34.未知
            //35.未知
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
            //    play.SendData(GameServer.PtichData, true);
            //107075

          //  收到网络协议:长度：98协议号:1108
            //byte[] data1 = { 98, 0, 84, 4, 110, 134, 61, 138, 67, 162, 1, 0, 14, 0, 0, 0, 118, 91, 16, 0, 0, 0, 0, 0, 3, 0, 53, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 198, 230, 210, 236, 202, 222, 79, 208, 205, 0, 0, 0 };
            //play.SendData(data1,true);
//收到网络协议:长度：24协议号:2037
//byte[] data3 = {24,0,245,7,3,0,0,0,110,134,61,138,1,0,0,0,24,0,0,0,138,2,0,0};
//            play.SendData(data3,true);
////收到网络协议:长度：496协议号:2037
//            byte[] data4 = { 240, 1, 245, 7, 3, 0, 0, 0, 110, 134, 61, 138, 60, 0, 0, 0, 6, 0, 0, 0, 159, 8, 0, 0, 7, 0, 0, 0, 159, 8, 0, 0, 10, 0, 0, 0, 81, 0, 0, 0, 8, 0, 0, 0, 150, 0, 0, 0, 9, 0, 0, 0, 83, 94, 121, 19, 55, 0, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0, 2, 0, 0, 0, 14, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 2, 0, 0, 0, 21, 0, 0, 0, 5, 0, 0, 0, 23, 0, 0, 0, 1, 0, 0, 0, 25, 0, 0, 0, 244, 5, 0, 0, 26, 0, 0, 0, 33, 5, 0, 0, 27, 0, 0, 0, 165, 3, 0, 0, 28, 0, 0, 0, 19, 0, 0, 0, 50, 0, 0, 0, 0, 0, 0, 0, 73, 0, 0, 0, 0, 0, 0, 0, 51, 0, 0, 0, 0, 0, 0, 0, 59, 0, 0, 0, 0, 0, 0, 0, 60, 0, 0, 0, 0, 0, 0, 0, 61, 0, 0, 0, 0, 0, 0, 0, 62, 0, 0, 0, 0, 0, 0, 0, 63, 0, 0, 0, 11, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 66, 0, 0, 0, 0, 0, 0, 0, 74, 0, 0, 0, 0, 0, 0, 0, 75, 0, 0, 0, 0, 0, 0, 0, 76, 0, 0, 0, 0, 0, 0, 0, 77, 0, 0, 0, 0, 0, 0, 0, 78, 0, 0, 0, 0, 0, 0, 0, 79, 0, 0, 0, 0, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 81, 0, 0, 0, 0, 0, 0, 0, 84, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 1, 0, 0, 0, 24, 0, 0, 0, 138, 2, 0, 0, 13, 0, 0, 0, 50, 0, 0, 0, 1, 0, 0, 0, 13, 4, 0, 0, 0, 0, 0, 0, 204, 5, 0, 0, 3, 0, 0, 0, 231, 2, 0, 0, 2, 0, 0, 0, 11, 4, 0, 0, 4, 0, 0, 0, 47, 2, 0, 0, 5, 0, 0, 0, 190, 2, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 66, 0, 0, 0, 0, 0, 0, 0, 82, 0, 0, 0, 0, 0, 0, 0, 36, 0, 0, 0, 176, 10, 0, 0, 37, 0, 0, 0, 235, 49, 0, 0, 38, 0, 0, 0, 79, 71, 0, 0, 39, 0, 0, 0, 168, 35, 0, 0, 40, 0, 0, 0, 235, 49, 0, 0, 41, 0, 0, 0, 225, 26, 0, 0, 42, 0, 0, 0, 179, 32, 0, 0, 83, 0, 0, 0, 50, 0, 0, 0 };
//            play.SendData(data4, true);
        }

        public void BuyItem(PlayerObject play, uint ptich_obj_id, uint item_id)
        {
            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) != null)
            {
                play.MsgBox("摆摊中不允许购买!");
                return;
            }
            int ptich_id = -1;
            for (int i = 0; i < mListPtichInfo.Count; i++)
            {
                if (mListPtichInfo[i].PtichObj == null) continue;
                if (mListPtichInfo[i].PtichObj.GetTypeId() == ptich_obj_id)
                {
                    ptich_id = i;
                    break;
                }
            }
            if (ptich_id == -1) return;

            //检测是否有道具
            int price = 0;
            byte sell_type = 0;

            RoleItemInfo item = null;
             RoleData_Eudemon eudemon  = null;
             bool bFind = false;
            for (int i = 0; i < mListPtichInfo[ptich_id].mSellItemList.Count; i++)
            {
                
                if (mListPtichInfo[ptich_id].mSellItemList[i].item_id == item_id)
                {
                    bFind = true;
                    if (item_id >= IDManager.eudemon_start_id)
                    {
                        eudemon = mListPtichInfo[ptich_id].play.GetEudemonSystem().FindEudemon(item_id);
                        if (eudemon == null) return;
                        item = mListPtichInfo[ptich_id].play.GetItemSystem().FindItem(eudemon.itemid);
                     }else
                    {
                         item = mListPtichInfo[ptich_id].play.GetItemSystem().FindItem(item_id);
                    }

                    if (item == null)
                    {
                        play.MsgBox("购买失败！");
                        return;
                    }
                    price = mListPtichInfo[ptich_id].mSellItemList[i].price;
                    sell_type = mListPtichInfo[ptich_id].mSellItemList[i].sell_type;
                    if (sell_type == NetMsg.MsgOperateItem.PTICH_SELL_ITEM_GAMEGOLD)
                    {
                        if (price > play.GetMoneyCount(MONEYTYPE.GOLD))
                        {
                            play.MsgBox("购买失败,魔石不足!");
                            return;
                        }
                        play.ChangeMoney(MONEYTYPE.GAMEGOLD, -price);
                        mListPtichInfo[ptich_id].play.ChangeMoney(MONEYTYPE.GAMEGOLD, price);
                    }
                    else if (sell_type == NetMsg.MsgOperateItem.PTICH_SELL_ITEM_GOLD)
                    {
                        if (price > play.GetMoneyCount(MONEYTYPE.GOLD))
                        {
                            play.MsgBox("购买失败,金币不足!");
                            return;
                        }
                        play.ChangeMoney(MONEYTYPE.GOLD, -price);
                        mListPtichInfo[ptich_id].play.ChangeMoney(MONEYTYPE.GOLD, price);
                    }
                    else return;
                    mListPtichInfo[ptich_id].mSellItemList.RemoveAt(i);
                    break;
                }
            }
            if (!bFind)
            {
                play.MsgBox("购买失败,该道具已下架!");
                return;
            }

            if (item_id >= IDManager.eudemon_start_id)
            {
                item.postion = NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK;
                play.GetEudemonSystem().AddTempEudemon(eudemon);//加到临时表
               
            }
            else
            {
                //添加道具
                item.postion = NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK;
               
            }
            play.GetItemSystem().AwardItem(item);
            //删除卖方道具
            //刷新摊位道具栏 卖方
            GetBackItem(mListPtichInfo[ptich_id].play, item_id);
            mListPtichInfo[ptich_id].play.GetItemSystem().DeleteItemByID(item_id);

            //买方 
            //{28,0,241,3,140,87,212,7,70,160,1,0,23,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0}
            PacketOut outpack = new PacketOut();
            outpack.WriteInt16(28);
            outpack.WriteInt16(1009);
            outpack.WriteUInt32(item_id);
            outpack.WriteUInt32(ptich_obj_id);
            outpack.WriteInt32(23); //购买完的标识
            outpack.WriteInt16(0);
            outpack.WriteInt32(1);
            outpack.WriteInt32(0);
            outpack.WriteInt16(0);
            play.SendData(outpack.Flush(), true);
           // this.LookPtich(play, ptich_obj_id);
          //  this.LookPtich(mListPtichInfo[ptich_id].play, ptich_obj_id);
        }

        //获取远程摊位
        //id 为从指定序号摊位 -1为自动顺序
        public void GetRemotePtich(PlayerObject play,int id = -1)
        {
            int ptich_id = -1;
            if (id != -1 && id >= 0 && id < mListPtichInfo.Count)
            {
                if (mListPtichInfo[id].play == null)
                {
                    play.MsgBox("该摊位未开放");
                    return;
               }
                ptich_id = id;
            }else
            {
                    ptich_id = GetRemotePtichId(play.GetCurrentRemotePtichId());
            }
          
            if (ptich_id == -1) return; //无摊位 返回
            play.SetCurrentRemotePtichId(ptich_id);

            //远程摊位信息
            String sName = mListPtichInfo[ptich_id].play.GetName();
            int nLen = 13 + Coding.GetDefauleCoding().GetBytes(sName).Length;
            PacketOut outpack = new PacketOut();
            outpack.WriteInt16((short)nLen);
            outpack.WriteInt16(1015);
           
           // {19,0,247,3,14,0,0,0,125,0,1,6,203,167,208,161,187,239,0}
            outpack.WriteInt32(ptich_id + 1);
            outpack.WriteInt16(125);
            outpack.WriteByte(1);
            outpack.WriteString(sName);
            outpack.WriteByte(0);
            play.SendData(outpack.Flush(), true);
            for (int i = 0; i < mListPtichInfo[ptich_id].mSellItemList.Count
                ; i++)
            {
                RoleItemInfo item_info = null;
                RoleData_Eudemon eudemon = null;
                if (mListPtichInfo[ptich_id].mSellItemList[i].item_id >= IDManager.eudemon_start_id)
                {
                    eudemon = mListPtichInfo[ptich_id].play.GetEudemonSystem().FindEudemon(
                        mListPtichInfo[ptich_id].mSellItemList[i].item_id);
                    if (eudemon == null) continue;
                    item_info = mListPtichInfo[ptich_id].play.GetItemSystem().FindItem(eudemon.itemid);
                }
                else
                {
                    item_info = mListPtichInfo[ptich_id].play.GetItemSystem().FindItem(
                    mListPtichInfo[ptich_id].mSellItemList[i].item_id);
                }

                if (item_info != null)
                {
                    NetMsg.MsgPtichItemInfo msg = new NetMsg.MsgPtichItemInfo(item_info,
                        (uint)(ptich_id + 1), mListPtichInfo[ptich_id].mSellItemList[i].price, mListPtichInfo[ptich_id].mSellItemList[i].sell_type,true);
                    play.SendData(msg.GetBuffer(), true);
                    //发送幻兽信息
                    if (item_info.typeid >= IDManager.eudemon_start_id)
                    {
                        mListPtichInfo[ptich_id].play.GetEudemonSystem().SendLookPtichEudemonInfo(play, eudemon);
                    
                    }
                }
            }

         
        }
        //获取远程摊位编号id
        private int GetRemotePtichId(int ptich_id)
        {
            int nIndex = -1;
            for (int i = ptich_id; i < mListPtichInfo.Count; i++)
            {
                if (mListPtichInfo[i].play != null)
                {
                    nIndex = i;
                    break;
                }
            }
            if (nIndex == -1)
            {
                for (int i = 0; i < mListPtichInfo.Count; i++)
                {
                    if (mListPtichInfo[i].play != null)
                    {
                        nIndex = i;
                        break;
                    }
                }
            }
            return nIndex;
        }

        public void BuyRemotePtichItem(PlayerObject play, uint item_id)
        {
            int ptich_id = play.GetCurrentRemotePtichId();
            if(ptich_id < 0 || ptich_id >= mListPtichInfo.Count)return;
            if(mListPtichInfo[ptich_id].PtichObj == null)return;
            this.BuyItem(play, mListPtichInfo[ptich_id].PtichObj.GetTypeId(), item_id);
        
        }
    }
}
