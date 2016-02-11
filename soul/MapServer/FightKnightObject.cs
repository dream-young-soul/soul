using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStruct;
using GameBase.Config;

//暗黑龙骑- 冲锋骑士
//2015.10.28
namespace MapServer
{
    public class FightKnightObject : BaseObject
    {
       
        private int mnTime; //生存时间
        private int mnTick;
        private int mnLastMoveTick;
        public FightKnightObject( short x, short y, byte dir, uint _id,int nTime)
        {
            
            this.SetPoint(x, y);
            this.SetDir(dir);
            this.id = _id;
            mnTime = nTime * 1000;
            mnLastMoveTick = mnTick = System.Environment.TickCount;
            typeid = IDManager.CreateTypeId(OBJECTTYPE.GUARDKNIGHT);
            
        }
        public override bool Run()
        {
             base.Run();
             if (System.Environment.TickCount - mnTick > mnTime)
             {
                 this.ClearThis();
                 return false;
             }
             if (System.Environment.TickCount - mnLastMoveTick > Define.FIGHT_KNIGHT_MOVE_TIME)
             {
                 //冲锋骑士无视阻挡,他是不可控制的...(/ □ \)
                 //short x = 0; short y = 0;
                 //if (DIR.GetNexPoint(this,ref x, ref y))
                 //{
                 this.Run(this.GetDir(), Define.FIGHTKNIGHT_RUN_SPEED);
                 //}
                 mnLastMoveTick = System.Environment.TickCount;
                 
             }
             return true;
        }
        public override void ClearThis()
        {
            base.ClearThis();
            this.GetGameMap().RemoveObj(this);
            IDManager.RecoveryTypeID(this.GetTypeId(), this.type);
        }
        public override void Walk(byte dir)
        {
            base.Walk(dir);
            GameStruct.Action action = new GameStruct.Action(GameStruct.Action.MOVE);
            PushAction(action);
        }
        public override void Run(byte dir, int ucMode)
        {
            base.Run(dir, ucMode);
            GameStruct.Action action = new GameStruct.Action(GameStruct.Action.MOVE);
            PushAction(action);
        }
        protected override void ProcessAction_Move(GameStruct.Action act)
        {
            this.RefreshVisibleObject();
            List<BaseObject> list_add = null;
            foreach (RefreshObject refobj in this.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                //    //角色没这个怪物的要刷新该怪物视野
                if (obj.type == OBJECTTYPE.PLAYER)
                {
                    if (!obj.GetVisibleList().ContainsKey(this.GetGameID())) //刷新该角色视野
                    {
                        //集合会被修改 与foreach冲突. 放到外面添加
                        if (list_add == null) list_add = new List<BaseObject>();
                        list_add.Add(obj);
                    }
                    else
                    {
                        NetMsg.MsgMoveInfo moveinfo = new NetMsg.MsgMoveInfo();
                        moveinfo.Create(null, null);
                        moveinfo.id = this.GetTypeId();
                        moveinfo.x = GetCurrentX();
                        moveinfo.y = GetCurrentY();
                        moveinfo.ucMode = DIR.MOVEMODE_RUN_DIR0 + Define.FIGHTKNIGHT_RUN_SPEED;
                        moveinfo.dir = this.GetDir();
                        byte[] msg = moveinfo.GetBuffer();
                        this.BrocatBuffer(msg);
                    }
                }
            }
            if (list_add != null)
            {
                for (int i = 0; i < list_add.Count; i++)
                {
                    list_add[i].AddVisibleObject(this, true);
                    this.SendInfo(list_add[i] as PlayerObject);
                    //(list_add[i] as PlayerObject).SendMonsterInfo(this);
                }
            }
        }

        public void SendInfo(PlayerObject play = null)
        {
            //  byte[] data2 = {  100, 0, 1, 4, 210, 193, 183, 227, 0, 0, 0 };
            GameStruct.MonsterInfo info = ConfigManager.Instance().GetMonsterInfo(id);
            if (info == null)
            {
                Log.Instance().WriteLog("获取黑暗骑士信息失败!!!" + id.ToString());
                return;
            }
            GameBase.Network.PacketOut outpack = null;
            if (play == null)
            {
                outpack = new GameBase.Network.PacketOut();
            }
            else
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
            outpack.WriteInt32(0);
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

        public override void RefreshVisibleObject()
        {
            base.RefreshVisibleObject();
            //只遍历玩家
            foreach (BaseObject o in mGameMap.GetAllObject().Values)
            {
                if (o.type != OBJECTTYPE.PLAYER &&
                    o.type != OBJECTTYPE.MONSTER )
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
                        if (o.type == OBJECTTYPE.PLAYER)
                        {
                            this.ClearThis(o as PlayerObject);
                        }
                       
                    }
                }
            }

        }
        public void ClearThis(PlayerObject play)
        {
            NetMsg.MsgClearObjectInfo info = new NetMsg.MsgClearObjectInfo();
            info.Create(null, play.GetGamePackKeyEx());
            info.id = GetTypeId();
            play.SendData(info.GetBuffer());
        }
        
    }
}
