using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;
using GameBase.Core;
//亡灵巫师 亡念巫灵
namespace MapServer
{
    class WangNianWuLing : MonsterObject
    {
        private static int DIS = 20; //与宿主的距离-
        private static int REFRESHTIME = 5000; //刷新对象时间
        public PlayerObject mPlay;
        private int mnRefreshTick;
        private TimeOut mAddHP_Time; 
        public WangNianWuLing(PlayerObject _play, short x, short y, byte dir, uint _id, int nAi_Id)
            : base(_id, nAi_Id,x,y, false)
        {
            type = OBJECTTYPE.CALLOBJECT;
            typeid = IDManager.CreateTypeId(OBJECTTYPE.GUARDKNIGHT); 
            SetPoint(x, y);
            mRebirthTime = 0;//不允许复活
       
            mPlay = _play;
            SetDir(dir);
            mnRefreshTick = System.Environment.TickCount;
            mAddHP_Time = new TimeOut();
            mAddHP_Time.SetInterval(2);
        }
        public override bool Run()
        {
            bool ret = base.Run();
            //距离超出-
            if (!this.GetPoint().CheckVisualDistance(mPlay.GetCurrentX(), mPlay.GetCurrentY(), DIS))
            {
                mPlay.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_WANGNIANWULING);
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

            if (mAddHP_Time.ToNextTime())
            {
                if (mPlay.GetBaseAttr().life < mPlay.GetBaseAttr().life_max)
                {
                    int nAddHP = (int)(mPlay.GetBaseAttr().life_max * 0.05);//百分之五的血量
                    mPlay.ChangeAttribute(GameStruct.UserAttribute.LIFE, nAddHP, true);
                    NetMsg.MsgMonsterMagicInjuredInfo magicattack = new NetMsg.MsgMonsterMagicInjuredInfo();
                    magicattack.roleid = this.GetTypeId();
                    magicattack.role_x = this.GetCurrentX();
                    magicattack.role_y = this.GetCurrentY();
                    magicattack.tag = 21;
                    magicattack.magicid = 6055;
                    magicattack.magiclv = 0;
                    magicattack.monsterid = mPlay.GetTypeId();
                    magicattack.injuredvalue = (uint)nAddHP;
                    this.BrocatBuffer(magicattack.GetBuffer());


                  
                    byte[] data2 = {  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    GameBase.Network.PacketOut outpack = new GameBase.Network.PacketOut();
                    outpack.WriteUInt16(88);
                    outpack.WriteUInt16(1105);
                    outpack.WriteUInt32(this.GetTypeId());
                    outpack.WriteUInt32(mPlay.GetTypeId());
                    outpack.WriteUInt16(6055);  //技能id
                    outpack.WriteUInt16(0);//技能等级
                    outpack.WriteByte(this.GetDir());
                    outpack.WriteByte(1); //类型
                    outpack.WriteUInt32(0);
                    outpack.WriteUInt32(0);
                    outpack.WriteUInt32(0);
                    outpack.WriteUInt16(0);
                    outpack.WriteUInt32(mPlay.GetTypeId());
                    outpack.WriteInt32(nAddHP);
                    outpack.WriteBuff(data2);
                    this.BrocatBuffer(outpack.Flush());
                        //NetMsg.MsgGroupM;agicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                    //magicattackex.nID = this.GetTypeId();
                    //magicattackex.nX = this.GetCurrentX();
                    //magicattackex.nY = this.GetCurrentX();
                    //magicattackex.nMagicID = 6055;
                    //magicattackex.nMagicLv = 0;
                    //magicattackex.bDir = this.GetDir();
                    //this.BrocatBuffer(magicattackex.GetBuffer());
                }
             
             
            }
            return ret;
        }
        protected override void ProcessAction_Die(GameStruct.Action act)
        {
            base.ProcessAction_Die(act);
            mPlay.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_WANGNIANWULING);
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
