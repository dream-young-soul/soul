using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;
using GameStruct;
//pk系统- 
//pk的减少，死亡根据pk规则掉落相应的装备
//2015.10.18
namespace MapServer
{
    public class PlayerPK
    {
        private PlayerObject play;
        private GameBase.Core.TimeOut mDecTime; //减少pk值的定时器
        private int mnNameType;  //玩家PK状态
       
      
        public PlayerPK(PlayerObject _play)
        {
            play = _play;

            mDecTime = new GameBase.Core.TimeOut();
            mDecTime.SetInterval(Define.PK_DEC_TIME);
          
         
            mnNameType = GetNameType();
          
        }



        public void Run()
        {

            //不在pk状态下才减少pk值
            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_CRIME) == null)
            {
                if (mDecTime.ToNextTime()&& play.GetBaseAttr().pk > 0)
                {
                    int decPK = -Define.PK_DEC_NORMAL;
                    //监狱地图额外减少pk值
                    if (play.GetGameMap().GetMapInfo().id == Define.PK_PRISON_MAPID)
                    {
                        decPK = -Define.PK_DEC_PRISON;
                    }

                    //else
                    //{
                    //    play.GetBaseAttr().pk -= decPK;
                    //}
                    mnNameType = GetNameType();
                    play.ChangeAttribute(GameStruct.UserAttribute.PK, decPK); //下发pk信息
                 
                }
            }
    
        }

        //死亡
        //target 击杀者 this 被击杀者
        public void Die(BaseObject target)
        {
            ushort nAddPk = 0; //击杀者需要增加pk值
            int nMinDropItem = 0; //最小掉落道具概率,百分比
            int nMaxDropItem = 0;   //最大掉落装备概率,百分比
            int nMinGold = 0;       //最小掉落金钱概率
            int nMaxGold = 0;           //最大掉落金钱概率
            int nExp = 0;           //减少的经验百分比
            switch (mnNameType)
            {
                case Define.PK_NAME_WHITE:
                    {
                        nAddPk = 20;
                        nMinDropItem = 10; //百分之十
                        nMaxDropItem = 50;//百分之五十
                        nMinGold = 10;
                        nMaxGold = 50;
                        nExp = 1; //百分之一
                        break;
                    }
                case Define.PK_NAME_RED:
                    {
                        nAddPk = 10;
                        nMinDropItem = 50; //百分之五十
                        nMaxDropItem = 100; //百分之一百
                        nMinGold = 50;
                        nMaxGold = 100;
                        nExp = 20;
                        break;
                    }
                case Define.PK_NAME_BLACK:
                    {
                        nMinDropItem = 100;
                        nMaxDropItem = 100;
                        nMinGold = 100;
                        nMaxGold = 100;
                        nExp = 30;
                        nAddPk = 0; //如果自身是黑名，击杀者无罪
                        break;
                    }
            }
            //自身是蓝名,击杀者无罪
            if (IsPKing()) { nAddPk = 0; }
            int nDropItem = IRandom.Random(nMinDropItem, nMaxDropItem);
            int nDropGold = IRandom.Random(nMinGold, nMaxGold);
         
            int nItemCount = (int)((float)play.GetItemSystem().GetBagCount() * ((float)nDropItem / 100f)); //掉落的道具数量
            if (nItemCount > 0)
            {
                while (nItemCount > 0)
                {
                    int nIndex = IRandom.Random(0, nItemCount);
                    int nAddIndex = 0;
                    foreach (GameStruct.RoleItemInfo itemobj in play.GetItemSystem().GetDicItem().Values)
                    {
                        if (itemobj.postion == NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK)
                        {
                            if (nAddIndex == nIndex)
                            {
                                play.GetItemSystem().DropItemBag(itemobj.id);
                                break;
                            }
                            nAddIndex++;
                        }
                    }
                    nItemCount--;
                }
            }
            //掉落金钱
            int nGoldCount = (int)((float)play.GetBaseAttr().gold * ((float)nDropGold / 100));
            if (nGoldCount > 0)
            {
                play.GetItemSystem().DropGold(nGoldCount);
            }
            //减少经验
            long nDecExp = play.GetBaseAttr().exp * (nExp / 100);
            if (nDecExp > 0)
            {
                play.ChangeAttribute(UserAttribute.EXP, (int)-nDecExp);
            }

            if (nAddPk > 0 && target.type == OBJECTTYPE.PLAYER)
            {
                (target as PlayerObject).ChangeAttribute(GameStruct.UserAttribute.PK, nAddPk);
            }

            //清除pk状态
            this.SetPKIng(false);

            //如果死亡时是黑名- 进监狱 并且身上穿戴装备随机掉一件-百分之百哦 2015.11.15
            if (mnNameType == Define.PK_NAME_BLACK)
            {
                int nIndex = 0;
                while(true)
                {
                    byte rand = IRandom.Random(NetMsg.MsgItemInfo.ITEMPOSITION_ARMOR, NetMsg.MsgItemInfo.ITEMPOSITION_SHOES);
                    if (rand == NetMsg.MsgItemInfo.ITEMPOSITION_WEAPONL) continue; //游戏里并没有左武器
                    GameStruct.RoleItemInfo role_item_info = play.GetItemSystem().GetEquipByPostion(rand);
                    if (role_item_info != null)
                    {
                        play.GetItemSystem().DropItemEquip(role_item_info.id);
                        break;
                    }
                    nIndex ++;
                    if (nIndex >= 8)
                    {
                        break;
                    }
                }
                play.ChangeMap(Define.PK_PRISON_MAPID, Define.PRISON_MAP_X, Define.PRISON_MAP_Y);
            }
        }

        //设置pk状态
        //v 是否是PK状态
        //bCrime 是否是犯罪状态[需要闪蓝]
        public void SetPKIng(bool v,bool bCrime = true)
        {
           
            if (v && bCrime)
            {
             
                play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_CRIME, Define.PKSTATE_TIME);
            }
            else
            {
                mnNameType = GetNameType();
                //play.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_CRIME);
                //ResetPKNameType();
            }
        }

        //是否正在pk中-- 蓝名
        public bool IsPKing() { return play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_CRIME) != null; }
        //计算玩家名称类型-
        public int GetNameType()
        {
           //if (mbPKing) return Define.PK_NAME_BLUE;
            short nPK = play.GetBaseAttr().pk;
            if (nPK < 20)   //白名
            {
                return Define.PK_NAME_WHITE;
            }
            else if (nPK < 100 && nPK >= 20)//红名
            {
                return Define.PK_NAME_RED;
            }
            return Define.PK_NAME_BLACK;    //黑名
        }

        //重新计算玩家颜色名字
        public void ResetPKNameType()
        {
            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_CRIME) != null) { return; }
            short nPK = play.GetBaseAttr().pk;
            if (nPK >= 20 && nPK < 100) //红名
            {
                play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_RED);
            }
            else if (nPK >= 100) //黑名
            {
                play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_BLOCK);
            }
        }
    }
}
