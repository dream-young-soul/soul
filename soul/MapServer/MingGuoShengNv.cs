using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;
//亡灵巫师- 冥国圣女
namespace MapServer
{
    public class MingGuoShengNv : MonsterObject
    {
       private static int DIS = 20; //与宿主的距离-
        private static int REFRESHTIME = 5000; //刷新对象时间
        public PlayerObject mPlay;
        private int mnRefreshTick;
        private GameBase.Core.TimeOut mMagicAttackTime;

        public MingGuoShengNv(PlayerObject _play, short x, short y, byte dir, uint _id, int nAi_Id)
            : base(_id, nAi_Id,x,y, false)
        {
           
            type = OBJECTTYPE.CALLOBJECT;
            typeid = IDManager.CreateTypeId(OBJECTTYPE.GUARDKNIGHT); 
            SetPoint(x, y);
            mRebirthTime = 0;//不允许复活
       
            mPlay = _play;
            SetDir(dir);
            mnRefreshTick = System.Environment.TickCount;
            mMagicAttackTime = new GameBase.Core.TimeOut();
            mMagicAttackTime.SetInterval(5);
        }
        public override bool Run()
        {
            bool ret = base.Run();
            //距离超出-
            if (!this.GetPoint().CheckVisualDistance(mPlay.GetCurrentX(), mPlay.GetCurrentY(), DIS))
            {
                mPlay.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_MINGGUOSHENGNV);
                return false;
            }
            //刷新周围对象以便寻找目标
            if (this.GetAi().GetTargetObject() == null)
            {
                if (System.Environment.TickCount - mnRefreshTick > REFRESHTIME)
                {
                    this.RefreshVisibleObject();
                    mnRefreshTick = System.Environment.TickCount;
                }
            }
            if (mMagicAttackTime.ToNextTime())
            {
                this.RefreshVisibleObject();
              
                NetMsg.MsgMonsterMagicInjuredInfo magicattack = new NetMsg.MsgMonsterMagicInjuredInfo();
                magicattack.roleid = this.GetTypeId();
                magicattack.role_x = this.GetCurrentX();
                magicattack.role_y = this.GetCurrentY();
                magicattack.tag = 21;
                magicattack.magicid = 6051;
                magicattack.magiclv = 0;
                this.BrocatBuffer(magicattack.GetBuffer());
          


                NetMsg.MsgGroupMagicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                magicattackex.nID = this.GetTypeId();
                magicattackex.nX = this.GetCurrentX();
                magicattackex.nY = this.GetCurrentY();
                magicattackex.nMagicID = 6051;
                magicattackex.nMagicLv = 0;
                magicattackex.bDir = this.GetDir();
                foreach (RefreshObject refobj in this.GetVisibleList().Values)
                {
                    //只攻击怪物
                    if (refobj.obj.type == OBJECTTYPE.MONSTER)
                    {
                        BaseObject obj = refobj.obj;
                        if (this.GetPoint().CheckVisualDistance(obj.GetCurrentX(), obj.GetCurrentY(), 10))
                        {
                            uint injured = BattleSystem.AdjustDamage(mPlay, obj, true);
                            NetMsg.MsgAttackInfo info = new NetMsg.MsgAttackInfo();
                            info.tag = 21;
                            obj.Injured(this, injured, info);
                            magicattackex.AddObject(obj.GetTypeId(), (int)injured);
                        }
                    }
                }
                this.BrocatBuffer(magicattackex.GetBuffer());
              
            }
            return ret;
        }
        protected override void ProcessAction_Die(GameStruct.Action act)
        {
            base.ProcessAction_Die(act);
            mPlay.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_MINGGUOSHENGNV);
        }
        public override void ClearThis()
        {
            this.attr.life = 0;
            base.ClearThis();
            this.GetGameMap().RemoveObj(this);
            IDManager.RecoveryTypeID(this.GetTypeId(), this.type);
        }
        public override bool CanPK(BaseObject obj, bool bGoCrime = true)
        {
            bool ret = base.CanPK(obj);
            if (ret)
            {
                //不攻击主人
                if (obj.type == OBJECTTYPE.PLAYER)
                {
                    if (obj.GetTypeId() == mPlay.GetTypeId())
                    {
                        return false;
                    }
                }
            }
            return ret;
            // return base.CheckIsAttack(obj);
        }
    }
}
