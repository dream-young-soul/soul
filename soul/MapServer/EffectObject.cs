using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;

//游戏特效对象--
//由技能或者其他东西创建在地图上的特效
namespace MapServer
{
    public class EffectObject : BaseObject
    {
        private PlayerObject mPlay;
        private int mnEffID;
        private int mnParam;
        private int mnParam1;
        private int mnTime;
        private int mnTick;

        private int mnAttackTick;
        //参数说明
        //_play 创建该特效的宿主
        //nEffid 特效id
        //param 应该是面特效标记,比如是道具 地面特效等等 //标记 10 地面特效 12.清除地面特效
        //param1 未知  15.守护骑士 14.降灵咒雨
        //nTime 对象存活时间[秒]
        public EffectObject(PlayerObject _play, int nEffId, int nParam, int nParam1, int nTime, short nX, short nY)
        {
            mnEffID = nEffId;
            mPlay = _play;
            mnTime = nTime * 1000;
            mnTick = System.Environment.TickCount;
            mnParam = nParam;
            mnParam1 = nParam1;
            type = OBJECTTYPE.EFFECT;
            typeid = IDManager.CreateTypeId(OBJECTTYPE.EFFECT);
            mnAttackTick = System.Environment.TickCount;
            this.SetPoint(nX, nY);
        }
        public override bool Run()
        {
            base.Run();
            if (System.Environment.TickCount - mnTick > mnTime)
            {
                this.ClearThis();
                return false;
            }

            //会攻击的特效-- 降灵咒雨
            if (mnEffID == Define.JIANGLINGZHOUYU)
            {
                if (System.Environment.TickCount - mnAttackTick > 1000)
                {
                    mnAttackTick = System.Environment.TickCount;

                    NetMsg.MsgMonsterMagicInjuredInfo magicattack = new NetMsg.MsgMonsterMagicInjuredInfo();
                    magicattack.roleid = (uint)mnEffID;
                    magicattack.role_x = this.GetCurrentX();
                    magicattack.role_y = this.GetCurrentY();
                    magicattack.tag = 21;
                    magicattack.magicid = (ushort)Define.JIANGLINGZHOUYU_MAGICID;
                    magicattack.magiclv = 0;

                    this.BrocatBuffer(magicattack.GetBuffer());

                    NetMsg.MsgGroupMagicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                    magicattackex.nID = mPlay.GetTypeId();
                    magicattackex.nX = this.GetCurrentX();
                    magicattackex.nY = this.GetCurrentY();
                    magicattackex.nMagicID = (ushort)Define.JIANGLINGZHOUYU_MAGICID;
                    magicattackex.nMagicLv = 0;
                    magicattackex.bDir = this.GetDir();
                    NetMsg.MsgAttackInfo info = new NetMsg.MsgAttackInfo();
                    info.tag = 21;
                    //---攻击 暂时只攻击怪物
                    foreach (RefreshObject refobj in this.GetVisibleList().Values)
                    {
                        BaseObject obj = refobj.obj;
                        if (obj.type == OBJECTTYPE.MONSTER)
                        {

                            if (this.GetPoint().CheckVisualDistance(obj.GetCurrentX(), obj.GetCurrentY(), Define.JIANGLINGZHOUYU_DIS))
                            {
                                uint nValue = BattleSystem.AdjustDamage(mPlay, obj, true);
                                magicattackex.AddObject(obj.GetTypeId(), (int)nValue);

                                obj.Injured(mPlay, nValue, info);

                            }
                            
                        }
                    }
                    this.BrocatBuffer(magicattackex.GetBuffer());
                }
            }
            return true;
        }

        public override void ClearThis()
        {
            this.SendInfo(null, true);
            //这里会清除可视列表..所以发送广播要在前面发
            base.ClearThis();
            this.GetGameMap().RemoveObj(this);
            IDManager.RecoveryTypeID(this.GetTypeId(), OBJECTTYPE.EFFECT);
        }

        //发送对象信息
        //play 要发送给对象的信息
        //bClear 是否是清除信息
        public void SendInfo(PlayerObject play = null, bool bClear = false)
        {
            GameBase.Network.PacketOut outpack = null;
            if (play != null)
            {
                outpack = new GameBase.Network.PacketOut(play.GetGamePackKeyEx());
            }
            else outpack = new GameBase.Network.PacketOut();
            outpack.WriteUInt16(32);
            outpack.WriteUInt16(1101);
            outpack.WriteUInt32(this.GetTypeId());
            outpack.WriteInt32(mnEffID);
            outpack.WriteInt16(this.GetCurrentX());
            outpack.WriteInt16(this.GetCurrentY());
            outpack.WriteInt32(0);
            if (!bClear)
            {
                outpack.WriteInt32(mnParam);
                outpack.WriteInt32(0);
                outpack.WriteInt32(mnParam1);
            }
            else
            {
                outpack.WriteInt32(12); //清除特效标识
                outpack.WriteInt32(0);
                outpack.WriteInt32(mnParam1);
            }

            if (play != null)
            {
                play.SendData(outpack.Flush());

            }
            else
            {
                byte[] data = outpack.Flush();
                this.BrocatBuffer(data);
            }
        }
        public override void RefreshVisibleObject()
        {
            base.RefreshVisibleObject();
            //只遍历玩家
            foreach (BaseObject o in mGameMap.GetAllObject().Values)
            {
                if (o.type != OBJECTTYPE.PLAYER &&
                    o.type != OBJECTTYPE.MONSTER &&
                    o.type != OBJECTTYPE.EUDEMON)
                {
                    continue;
                }
                if (this.GetPoint().CheckVisualDistance(o.GetCurrentX(), o.GetCurrentY()))
                {
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

    }
}
