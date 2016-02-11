using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;

namespace MapServer
{
    //亡灵巫师- 深渊恶灵
    //2015.11.2
    public class ShenYuanELing : MonsterObject
    {
   
        public PlayerObject mPlay;
     

        public ShenYuanELing(PlayerObject _play,BaseObject _AttackTarget, short x, short y, byte dir, uint _id, int nAi_Id)
            : base(_id, nAi_Id,x,y, false)
        {
            type = OBJECTTYPE.CALLOBJECT;
            mPlay = _play;
            //1.5倍的血量
            this.attr.life = this.attr.life_max =  (int)(mPlay.GetBaseAttr().life * 1.5f);
            
            typeid = IDManager.CreateTypeId(OBJECTTYPE.GUARDKNIGHT); 
            SetPoint(x, y);
            mRebirthTime = 0;//不允许复活
           
            SetDir(dir);
            this.GetAi().SetAttackTarget(_AttackTarget);
        }

        public override bool Run()
        {
            bool ret = base.Run();
            //目标已死亡或者不再攻击范围内了..
            if (this.GetAi().GetTargetObject() == null)
            {
                this.ClearThis();
                if (mPlay.GetGameSession() != null)
                {
                    mPlay.SetZhaoHuanWuHuanObj(null);
                }
              //  mPlay.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_SHENYUANELING);
                ret = false;
            }
            //宿主已退出游戏
            if (mPlay.GetGameSession() == null)
            {
                this.ClearThis();
                ret = false;
            }
            
            return ret;
        }
        public override void ClearThis()
        {
            this.attr.life = 0;
            base.ClearThis();
            this.GetGameMap().RemoveObj(this);
            IDManager.RecoveryTypeID(this.GetTypeId(), this.type);
        }
    }
}
