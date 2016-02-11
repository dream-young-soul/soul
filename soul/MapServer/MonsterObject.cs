using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMsg;
using GameBase.Config;
using GameStruct;
using GameBase.Network;
using System.Diagnostics;

namespace MapServer
{
    //怪物对象
    public class MonsterObject : BaseObject
    {
        private GameStruct.MonsterInfo mInfo;
        protected GameStruct.MonterAttribute attr;
        private GameStruct.Point mInitPoint;    //玩家初始化坐标
        protected uint mRebirthTime;       //刷怪时间
        //设置怪物刷新间隔
        public void SetRebirthTime(uint nTime) 
        { 
            mRebirthTime = nTime;
            mAliveTime.SetInterval(mRebirthTime);
            
        }
        private int LastDieTime;        //最后死亡时间
       
        private AI.BaseAI m_Ai;
        private BaseObject mTarget; //被攻击的目标

        //单体魔法攻击延迟死亡
        GameStruct.Action mDieMagicInfo;
        int mnDieMagicTick;

        //复活定时器
        private GameBase.Core.TimeOut mAliveTime;
        public MonsterObject(uint _id,int nAi_Id,short x,short y,bool isCreateTypeId = true)
        {
            mTarget = null;
            id = _id;
            type = OBJECTTYPE.MONSTER;
            attr = new GameStruct.MonterAttribute();
            mRebirthTime = 1000; //默认复活时间
            if (isCreateTypeId)
            {
                typeid = IDManager.CreateTypeId(type);
            }
            mInitPoint = new GameStruct.Point();
            mInitPoint.x = x;
            mInitPoint.y = y;
            m_Ai = CreateAi(nAi_Id);

            mnDieMagicTick = System.Environment.TickCount;
            mDieMagicInfo = null;
            attr.life = attr.life_max = 0;
            mAliveTime = new GameBase.Core.TimeOut();
            mAliveTime.SetInterval(mRebirthTime);

            this.SetPoint(x, y);
           // ai = new AI.BaseAI(this);
            Alive();
            
        }

        public AI.BaseAI GetAi() { return m_Ai; }
        public void SetAi(AI.BaseAI _ai) { m_Ai = _ai; }

        //怪物复活
        //init 是否是第一次初始化的怪物
        public void Alive(bool init = true)
        {
            mInfo = ConfigManager.Instance().GetMonsterInfo(id);
            Name = mInfo.name;
            attr.life =attr.life_max = mInfo.life;
            if (!init)
            {
                this.SetPoint(mInitPoint.x, mInitPoint.y);
            }
            SetLastWalkTime(System.Environment.TickCount);
            LastDieTime = System.Environment.TickCount;

            SetWalkTime(IRandom.Random(1000, 60 * 1000));
            
            
            if (!init)
            {
                Walk(DIR.MAX_DIRSIZE);
            }


        }
        public override void RefreshVisibleObject()
        {
            base.RefreshVisibleObject();
            //只遍历玩家
            foreach (BaseObject o in mGameMap.GetAllObject().Values)
            {
                if (o.GetGameID() == this.GetGameID()) continue;
                if (o.type != OBJECTTYPE.PLAYER &&
                    o.type != OBJECTTYPE.DROPITEM &&//掉落物品也加到可视列表..因为怪物掉落装备需要判断该位置是否被道具占有
                    o.type != OBJECTTYPE.GUARDKNIGHT &&
                     o.type != OBJECTTYPE.MONSTER &&
                     o.type != OBJECTTYPE.CALLOBJECT )
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
            //mRefreshList.Clear();
            //foreach (BaseObject o in mGameMap.mDicObject.Values)
            //{
            //    if (o.GetGameID() == this.GetGameID()) continue;
            //    //怪物视野只有角色与怪物
            //    if (o.type != OBJECTTYPE.PLAYER && 
            //        o.type != OBJECTTYPE.MONSTER &&
            //        o.type != OBJECTTYPE.DROPITEM) continue;
            //    if (GetPoint().CheckVisualDistance(o.GetCurrentX(), o.GetCurrentY())
            //        /* && this.mVisibleList.ContainsKey(o.GetGameID())*/)
            //    {
            //        this.mVisibleList[o.GetGameID()] = o;
            //        if (o.type == OBJECTTYPE.PLAYER) //只发给角色
            //        {
            //            mRefreshList[o.GetGameID()] = o;
            //        }
                    
            //    }
            //    else
            //    {
            //        if (!GetPoint().CheckVisualDistance(o.GetCurrentX(), o.GetCurrentY())
            //            && this.mVisibleList.ContainsKey(o.GetGameID()))
            //        {
            //            this.mVisibleList.Remove(o.GetGameID());
            //        }
            //    }
            //}
        }

        public override bool Run()
        {
            if (this.GetAi() == null) return true;
            base.Run();
            //单体魔法攻击延迟死亡
            if (mDieMagicInfo != null &&
                System.Environment.TickCount - mnDieMagicTick >= 500)
            {
                GameStruct.Action action = new GameStruct.Action(GameStruct.Action.DIE, null);
                action.AddObject(mDieMagicInfo.GetObject(0));
                action.AddObject(mDieMagicInfo.GetObject(1));

                this.PushAction(action);
                mDieMagicInfo = null;
                return true;
            }
            else if (mDieMagicInfo != null)
            {
                return true;
            }
            this.GetAi().Run();
           

            if(this.IsLock())
            {
                if (!this.CheckLockTime())
                {
                    this.UnLock(false);
                    if (IsDie())
                    {
                        GameStruct.Action action;
                        //死亡
                        action = new GameStruct.Action(GameStruct.Action.DIE, null);
                        action.AddObject(mTarget);
                        action.AddObject((uint)mTarget.GetMinAck()); //取最小攻击为经验值
                        this.PushAction(action);
                        LastDieTime = System.Environment.TickCount;
                    }

                }
            }
          
            //死亡后三秒后发送清除怪物消息
            if (IsDie() && !this.IsLock())
            {
                if (!IsClear() && System.Environment.TickCount - LastDieTime > 3000 )
                {
                    this.ClearThis();
                }
               
            }

            ////复活
            //if (IsClear() && IsDie() && mRebirthTime > 0)
            //{
            //    if (System.Environment.TickCount - LastDieTime > mRebirthTime)
            //    {
            //        Alive(false);
            //    }
            //}
            if (IsClear() && IsDie() && mAliveTime.ToNextTime())
            {
                Alive(false);
            }
            return true;
        }

        //是否死亡
        public override bool IsDie() { return attr.life == 0 ? true : false; }
        //是否已经在地图上消失
        public bool IsClear() { return LastDieTime == -1 ? true : false; }


        public override void Walk(byte dir, short x, short y)
        {
            base.Walk(dir, x, y);
            GameStruct.Action action = new GameStruct.Action(GameStruct.Action.MOVE);
            SetLastWalkTime(System.Environment.TickCount);
            SetWalkTime(IRandom.Random(1000, 60 * 1000));

            PushAction(action);
        }

        public override void Walk(byte dir)
        {
            base.Walk(dir);
            GameStruct.Action action = new GameStruct.Action(GameStruct.Action.MOVE);
            SetLastWalkTime(System.Environment.TickCount);
            SetWalkTime(IRandom.Random(1000, 60 * 1000));

            PushAction(action);
        }
        protected override void ProcessAction_Move(GameStruct.Action act)
        {
            this.RefreshVisibleObject();
            List<BaseObject> list_add = null;
            foreach(RefreshObject refobj in this.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                //    //角色没这个怪物的要刷新该怪物视野
                //    //2015.9.5
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
                        MsgMoveInfo moveinfo = new MsgMoveInfo();
                        moveinfo.Create(null, null);
                        moveinfo.id = this.GetTypeId();
                        moveinfo.x = GetCurrentX();
                        moveinfo.y = GetCurrentY();
                        moveinfo.ucMode = 1;
                        moveinfo.dir = this.GetDir();
                        byte[] msg = moveinfo.GetBuffer();
                        this.GetGameMap().BroadcastBuffer(this, msg);
                    }
                }
            }
            if (list_add != null)
            {
                for (int i = 0; i < list_add.Count; i++)
                {
                    //RefreshObject _addobj = new RefreshObject();
                    //_addobj.bRefreshTag = true;
                    //_addobj.obj = this;
                    //list_add[i].GetVisibleList()[this.GetGameID()] = _addobj;
                    list_add[i].AddVisibleObject(this, true);
                    (list_add[i] as PlayerObject).SendMonsterInfo(this);
                }
            }
            //if (mRefreshList.Count > 0)
            //{
            //    //角色没这个怪物的要刷新该怪物视野
            //    //2015.9.5
            //    foreach (BaseObject obj in mRefreshList.Values)
            //    {
            //        if (obj.type == OBJECTTYPE.PLAYER)
            //        {
            //            if (!obj.GetVisibleList().ContainsKey(this.GetGameID()))
            //            {
            //                (obj as PlayerObject).SendMonsterInfo(this);
            //                obj.GetVisibleList().Add(this.GetGameID(), this);
            //            }
            //        }
            //        mRefreshList.Clear();
            //    }
            //    MsgMoveInfo moveinfo = new MsgMoveInfo();
            //    moveinfo.Create(null, null);
            //    moveinfo.id = this.GetTypeId();
            //    moveinfo.x = GetCurrentX();
            //    moveinfo.y = GetCurrentY();
            //    moveinfo.ucMode = 1;
            //    moveinfo.dir = this.GetDir();
            //    byte[] msg = moveinfo.GetBuffer();
            //    this.GetGameMap().BroadcastBuffer(this,msg);
              
            //}
        
        }
        protected override void ProcessAction_Injured(GameStruct.Action act)
        {
            BaseObject attack_obj = act.GetObject(0) as BaseObject;
            NetMsg.MsgAttackInfo info = act.GetObject(2) as NetMsg.MsgAttackInfo;
            if (attack_obj == null) return;
            uint injured = (uint)act.GetObject(1) ; //受伤害的值
            mTarget = attack_obj;

            this.GetAi().Injured(attack_obj);
     
            //死亡-- 锁定后不允许死亡
            if (IsDie() && !this.IsLock() && info.tag == 2/*单体攻击*/)
            {
                GameStruct.Action action;
                //死亡
                action = new GameStruct.Action(GameStruct.Action.DIE, null);
                action.AddObject(attack_obj);
                action.AddObject(injured);
                this.PushAction(action);
            }
            //魔法攻击要延迟一秒下发死亡消息

            if (info.tag == 21 && IsDie() && !this.IsLock())
            {
                mnDieMagicTick = System.Environment.TickCount;
                mDieMagicInfo = act;
            }
        }

        protected override void ProcessAction_Die(GameStruct.Action act)
        {
            PlayerObject play = act.GetObject(0) as PlayerObject;
            BaseObject baseobj = act.GetObject(0) as BaseObject;
            if (play == null && baseobj.type == OBJECTTYPE.EUDEMON)
            {
                play = (baseobj as EudemonObject).GetOwnerPlay();
            }
            //根据打出的伤害获得经验值
            uint injured = (uint)act.GetObject(1);

            NetMsg.MsgMonsterDieInfo info = new NetMsg.MsgMonsterDieInfo();
            info.roleid = baseobj.GetTypeId();
            info.role_x = baseobj.GetCurrentX();
            info.role_y = baseobj.GetCurrentY();
            info.injuredvalue = 0;
            info.monsterid = this.GetTypeId();
            byte[] msg = info.GetBuffer();

           
             //掉落道具
            this.DropItem(baseobj);
            
            //RefreshVisibleObject();
            //if (mRefreshList.Count > 0)
            //{
                

            //    this.GetGameMap().BroadcastBuffer(this,msg);
            //    //掉落道具
            //    this.DropItem(play);
              
            //}
            this.BrocatBuffer(msg);
            LastDieTime = System.Environment.TickCount;
            if (play == null && baseobj.type != OBJECTTYPE.EUDEMON) return;
            //计算经验
            play.AddExp((int)injured, play.GetLevel(), this.GetLevel());
            //死亡的幻兽加灵气值复活
            play.GetEudemonSystem().Eudemon_Alive(this);

            this.GetAi().Die();

            this.GetAi().SetAttackTarget(null);

            mAliveTime.Update();
            //执行死亡脚本- 最后一击的击杀者执行该脚本
            if (mInfo.die_scripte_id > 0 && play != null)
            {
                ScripteManager.Instance().ExecuteAction(mInfo.die_scripte_id, play);
            }
            
        }

 

        //怪物复活
        //protected override void ProcessAction_Alive(GameStruct.Action act)
        //{
        //    RefreshVisibleObject();
        //    this.BrocatBuffer(act.GetBuff());
        //    //this.GetGameMap().BroadcastMonsterInfo(this, act.GetBuff());
        //}

        //protected override void ProcessAction_Attack(GameStruct.Action act)
        //{
        //    this.BrocatBuffer(act.GetBuff());
        //   // this.GetGameMap().BroadcastBuffer(this, act.GetBuff());
        //}
        public GameStruct.MonterAttribute GetAttribute() { return attr; }
        //获得基础属性
        public GameStruct.MonsterInfo GetBasicAttribute() { return mInfo; }

        public override void Injured(BaseObject obj, uint value,NetMsg.MsgAttackInfo info)
        {
            if (value > attr.life) attr.life = 0;
            else attr.life -= (int)value;
            GameStruct.Action action;


            action = new GameStruct.Action(GameStruct.Action.INJURED, null);
            action.AddObject(obj);
            action.AddObject(value);
            action.AddObject(info);
            this.PushAction(action);
        }


        public override void ClearThis()
        {
            base.ClearThis();
            LastDieTime = -1;
        }
        public void DropItem(BaseObject attack)
        {
          //计算爆的值
            uint ownerid = attack.GetTypeId();
            if (attack.type == OBJECTTYPE.EUDEMON)
            {
                if((attack as EudemonObject).GetOwnerPlay() != null)
                {
                    ownerid = (attack as EudemonObject).GetOwnerPlay().GetTypeId();
                }
               
            }
            byte droptype = BattleSystem.AdjustDrop(attack, this);
            if(droptype == BattleSystem.EXPLODE_ITEM_CHANCE1)return;
            GameStruct.DropItemInfo info = ConfigManager.Instance().GetDropItemInfo(mInfo.drop_group);
            if (info == null) return;
            int num = 0;
            switch(droptype)
            {
                case BattleSystem.EXPLODE_ITEM_CHANCE2:
                    {
                        num = IRandom.Random(2,5);
                        break;   
                    }
                case BattleSystem.EXPLODE_ITEM_CHANCE3:
                    {
                        num = IRandom.Random(6,9);
                        break;
                    }
                case BattleSystem.EXPLODE_ITEM_CHANCE4:
                    {
                        num = IRandom.Random(10,15);
                        break;
                    }
            }
            //如果不是百分之百的爆率..爆不出那么多的啦
            int nCurnum = 0;
            short x = 0;
            short y = 0;
            //要爆的数量
            for (int i = 0; i < num; i++)
            {
               int nNum = IRandom.Random(0, info.listamount.Count);
               for (int j = 0; j < nNum; j++)
               {
                     int index = IRandom.Random(0, info.listamount.Count);
                     if (IRandom.Random(1, 100) < info.listrate[index])
                   {
                       this.GetDropItemPoint(ref x, ref y);
                       DropItemClass dropclass = info.listitem[index];
                       if (dropclass.list_itemid.Count == 1)
                       {
                           this.GetGameMap().AddDropItemObj(dropclass.list_itemid[0], x, y, ownerid);
                       }
                       else continue; //掉落的道具组多个道具在下面的函数掉落。
                       //else
                       //{
                       //    this.GetGameMap().AddDropItemObj(dropclass.list_itemid[IRandom.Random(0,dropclass.list_itemid.Count - 1)], x, y, attack.GetTypeId());
                       //}
                       
                       nCurnum++;
                       if (nCurnum == num) break;
                    }
                }
            }
            //必掉的道具
            for (int i = 0; i < info.listitem.Count; i++)
            {
                DropItemClass dropclass = info.listitem[i];
                if (dropclass.list_itemid.Count > 1)
                {
                    for (int j = 0; j < info.listamount[i]; j++)
                    {
                        if (IRandom.Random(1, 100) < info.listrate[i])
                        {
                            this.GetDropItemPoint(ref x, ref y);
                            this.GetGameMap().AddDropItemObj(dropclass.list_itemid[IRandom.Random(0, dropclass.list_itemid.Count - 1)], x, y, ownerid);
                        }
                    }
               
                  
                }
            }
        }



        private bool GetDropItemPoint(ref short x, ref short y)
        {
            //由目标中心点衍生周围坐标点遍历--
            x = this.GetCurrentX();
            y = this.GetCurrentY();
            short[] _DELTA_X = { 0, -1, -1, -1, 0, 1, 1, 1, 0 };
            short[] _DELTA_Y = { 1, 1, 0, -1, -1, -1, 0, 1, 0 };
            //最多绕怪物四圈。。4*8 =32 爆32件道具
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int tempx = x + (_DELTA_X[j] * i + 1);
                    int tempy = y + (_DELTA_Y[j] * i + 1);
                    if (!this.GetGameMap().GetPointOfObj(this, (short)tempx, (short)tempy))
                    {
                        x = (short)tempx;
                        y = (short)tempy;
                        return true;
                    }
                }
            }
         
            return false;
        }

        public override byte GetLevel()
        {
            return (byte)mInfo.level;
        }
        public override int GetMinAck()
        {
            return (int)mInfo.attack_min;
        }

        public override int GetMaxAck()
        {
            return (int)mInfo.attack_max;
        }

        public override int GetDefense()
        {
            return (int)mInfo.defense;
        }

        public override int GetMagicAck()
        {
            return (int)mInfo.attack_max;
        }

        public override int GetMagicDefense()
        {
            return (int)mInfo.defense;
        }

        public AI.BaseAI CreateAi(int nAi_Id)
        {
            AI.BaseAI Ai = null;
            switch (nAi_Id)
            {
                case Define.AI_TYPE_MELEE:
                case Define.AI_TYPE_MELEEEX:
                    {
                        Ai = new AI.BaseAI();
                        break;
                    }
                
            }
            if (Ai == null)
            {
                Ai = new AI.BaseAI();
            }
            Ai.Init(this, nAi_Id);
            return Ai;

        }

        public override bool CanPK(BaseObject obj, bool bGoCrime = true)
        {
            if (obj.type == OBJECTTYPE.PLAYER)
            {
                PlayerObject play = obj as PlayerObject;
                //潜行 隐身 飞行状态攻击不到
                if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_HIDDEN) != null ||
                    play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_FLY) != null ||
                    play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_STEALTH) != null)
                {
                    return false;
                }
            }
            return base.CanPK(obj);
        }
       

    }
}
