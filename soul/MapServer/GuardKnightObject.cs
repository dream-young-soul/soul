using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;

//暗黑龙骑- 守护骑士对象
//2015.10.28
namespace MapServer
{
    public class GuardKnightObject : MonsterObject
    {
        
     
        private int mnSurvivalTick;
        private PlayerObject mPlay; //宿主
        public GuardKnightObject(PlayerObject _play, short x, short y,byte dir, uint _id, int nAi_Id)
            : base(_id, nAi_Id,x,y, false)
        {
            type = OBJECTTYPE.GUARDKNIGHT;
            typeid = IDManager.CreateTypeId(OBJECTTYPE.GUARDKNIGHT); ;
            SetPoint(x, y);
            mRebirthTime = 0;//不允许复活
            mnSurvivalTick = System.Environment.TickCount;
            mPlay = _play;
          
            SetDir(dir);
        }

        public override bool Run()
        {
            base.Run();
            //要消失了 移除其他的三个守护骑士与地效对象-
            if (System.Environment.TickCount - mnSurvivalTick >= Define.GUARDKNIGHT_TIME * 1000 && 
                mPlay != null)
            {
                mPlay.GetFightSystem().RemoveQiShiTuanGuardEffect(); 
                return false;
            }
            return true;
        }

        public override void ClearThis()
        {
            base.ClearThis();
            this.GetGameMap().RemoveObj(this);
            IDManager.RecoveryTypeID(this.GetTypeId(), this.type);
            mPlay = null;
        }
        public void SendInfo(PlayerObject play = null)
        {
            //  byte[] data2 = {  100, 0, 1, 4, 210, 193, 183, 227, 0, 0, 0 };
            GameStruct.MonsterInfo info = ConfigManager.Instance().GetMonsterInfo(id);
            if (info == null)
            {
                Log.Instance().WriteLog("获取守护骑士信息失败!!!" + id.ToString());
                return;
            }
            GameBase.Network.PacketOut outpack = null;
            if (play == null)
            {
                outpack = new GameBase.Network.PacketOut();
            }else
            {
                outpack = new GameBase.Network.PacketOut(play.GetGamePackKeyEx());
            }
            outpack.WriteUInt16(81);
            outpack.WriteUInt16(2069);
            outpack.WriteUInt32(GetTypeId());
            outpack.WriteUInt32(play.GetTypeId());
            byte[] data = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            outpack.WriteBuff(data);
            outpack.WriteUInt32(info.lookface);
            outpack.WriteInt16(GetCurrentX());
            outpack.WriteInt16(GetCurrentY());
            outpack.WriteInt16(0); //未知
            outpack.WriteUInt16(info.level);
            outpack.WriteUInt32(info.id);
            outpack.WriteInt32(attr.life);
            outpack.WriteInt32(info.life);
            outpack.WriteInt16(GetDir());
            byte[] data1 = { 100, 0, 1, 4, 210, 193, 183, 227, 0, 0, 0 };
            outpack.WriteBuff(data1);
           
            if (play != null)
            {
                play.SendData(outpack.Flush());
            }
            else
            {
                //广播
                data = outpack.Flush();
                this.BrocatBuffer(data);
            }

        }
    }
}
