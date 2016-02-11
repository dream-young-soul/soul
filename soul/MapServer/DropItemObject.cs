using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;
using GameBase.Network.Internal;

//掉落物品对象--
//2015.8.30
namespace MapServer
{
   public class DropItemObject : BaseObject
    {

       private int mTime;
       private int mCurTime;
       private uint nOwnerid; //道具爆出的所有者-
       private const int nOwnerTime = 60000; //所持者时间.,过了这个时间后,任何人都可以捡取该道具
       private GameStruct.RoleItemInfo mItemInfo;
       private RoleData_Eudemon mEudemonInfo;
       public GameStruct.RoleItemInfo GetRoleItemInfo() { return mItemInfo; }
       public RoleData_Eudemon GetRoleEudemonInfo() { return mEudemonInfo; }
       public DropItemObject(uint itemid/*基本物品数据库的id*/,short x,short y,uint ownerid,int time)//存活时间)
       {
           mTime = time;
           type = OBJECTTYPE.DROPITEM;
           typeid = itemid;
           mCurTime = System.Environment.TickCount;
           nOwnerid = ownerid;
           SetPoint(x, y);
           mItemInfo = null;
        
       }

       public void SetRoleItemInfo(GameStruct.RoleItemInfo info)
       {
           mItemInfo = info;
       }

       public void SetRoleEudemonInfo(RoleData_Eudemon eudemon)
       {
           mEudemonInfo = eudemon;
       }
       public override bool Run()
       {
           //道具存活时间已到，通知清除
           if (System.Environment.TickCount - mCurTime > mTime)
           {
               this.RefreshVisibleObject();
               this.BroadcastInfo(2);
               return false;
           }
           if (nOwnerid != 0)
           {
               if (System.Environment.TickCount - mCurTime > nOwnerTime)
               {
                   nOwnerid = 0;
               }
           }
           
           return true;
       }
       public override void RefreshVisibleObject()
        {
            base.RefreshVisibleObject();
            //只遍历玩家
            foreach (BaseObject o in mGameMap.GetAllObject().Values)
            {
                if (o.type != OBJECTTYPE.PLAYER)
                {
                    continue;
                }
                if (this.GetPoint().CheckVisualDistance(o.GetCurrentX(), o.GetCurrentY()))
                {
                    //RefreshObject refobj = new RefreshObject();
                    //refobj.bRefreshTag = false;
                    //refobj.obj = o;
                    //this.mVisibleList[o.GetGameID()] = refobj;
                    this.AddVisibleObject(o, false);
                }
                else
                {
                    if (this.mVisibleList.ContainsKey(o.GetGameID()))
                    {
                        this.mVisibleList.Remove(o.GetGameID());
                    }
                }
            }
        }
       //默认是刷新标记
       //广播道具信息
       public void BroadcastInfo(uint tag = 1)
       {
          // this.RefreshVisibleObject();

            NetMsg.MsgDropItem data = new NetMsg.MsgDropItem();
            data.tag = tag; 
            data.id = this.GetGameID();
            data.typeid = this.GetTypeId();
            data.x = this.GetCurrentX();
            data.y = this.GetCurrentY();
            byte[] msg = data.GetBuffer();
            this.GetGameMap().BroadcastBuffer(this,msg);
           
       }

       public bool IsOwner() { return nOwnerid != 0; } //该道具是否有持有者
       public uint GetOwnerId() { return nOwnerid; }        //取该道具的持有者
    }
}
