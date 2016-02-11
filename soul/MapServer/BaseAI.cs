using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapServer;
using GameStruct;
using GameBase.Config;
namespace AI
{
    //AI基类
    //必须使用 Init方法初始化-  否则宕机--。- 2015.10.19
    public class BaseAI
    {
        protected static byte  STATE_IDLE = 1; //休闲状态
        protected static byte  INJURED =2;      //受伤状态
        protected static byte ATTACK = 3;       //攻击状态
        public static byte FOLLOW = 4;      //跟随目标

        protected BaseObject TargetObj;
        protected BaseObject SelObj;
        public BaseObject GetTargetObject() { return TargetObj; }
       
     
        private byte nState;
        private int mnLastAttackTick;   //最后攻击时间戳
        private int mnLastMoveTick; //最后移动时间戳
        protected GameStruct.AiInfo mAiInfo;
        List<MapServer.FindPoint> findlist; //自动寻路坐标列表
        private int mnActiveAttackTick;
        public  BaseAI()
        {

        }

        public virtual void Init(BaseObject obj = null, int nAi_Id = Define.AI_TYPE_MELEE)
        {
            TargetObj = null;
            SelObj = obj;
            nState = STATE_IDLE;
            mnLastAttackTick = System.Environment.TickCount;
            mnLastMoveTick = mnLastAttackTick;
            findlist = null;
            mAiInfo = ConfigManager.Instance().GetAIInfo(nAi_Id);
            mnActiveAttackTick = System.Environment.TickCount;
        }
        public virtual void Run()
        {
            if (SelObj.IsDie()) return; //自身实体已死亡
            if (SelObj.IsLock())  //自身被锁定
            {
                if (findlist != null)
                    findlist.Clear();
                return;
            }
            //攻击的对方被锁定，攻击不了
            if (TargetObj != null && TargetObj.IsLock()) return;
            //目标已死亡切换目标
            if (TargetObj != null && TargetObj.IsDie())
            {
                SetAttackTarget(null);
            }
            //目标是幻兽- 并且在休息状态
            if(TargetObj != null && TargetObj.type == OBJECTTYPE.EUDEMON &&
                (TargetObj as EudemonObject).GetState() != EUDEMONSTATE.BATTLE)
            {
                SetAttackTarget(null);
            }
            //空闲状态走路
            if (nState == BaseAI.STATE_IDLE && mAiInfo.bIdle_Move)
            {
                if (System.Environment.TickCount - SelObj.GetLastWalkTime() > SelObj.GetWalkTime() && !SelObj.IsDie())
                {
                    //二分之一的几率不走路
                    if (IRandom.Random(1, 4) > 2)
                    {
                        //增加判断- 优化服务端运行效率- 视野里有玩家才走
                        bool bWalk = false;
                        foreach (RefreshObject obj in SelObj.GetVisibleList().Values)
                        {
                            if (obj.obj.type == OBJECTTYPE.PLAYER)
                            {
                                bWalk = true;
                                break;
                            }
                        }
                        if (bWalk)
                        {
                            byte dir = 0; short x = 0; short y = 0;
                            if (DIR.Random_Walk(SelObj, ref dir, ref x, ref y))
                            {
                                SelObj.Walk(dir, x, y);
                            }
                        }
                      
                    }
                }
            }
            //玩家意外掉线
            if (TargetObj != null && TargetObj.type == OBJECTTYPE.PLAYER &&
                TargetObj.GetGameSession() == null)
            {
                SetAttackTarget(null);
            
            }
            //主动怪物- 遍历视野范围内的玩家- 找到最近的那一个开始攻击他
            ActiveAttackPlay();
           
            //受伤了..反击
            if ((TargetObj != null) && 
                (nState == BaseAI.INJURED || nState == BaseAI.ATTACK ||
                nState == BaseAI.FOLLOW))
            {
                if (!SelObj.CanPK(TargetObj))
                {
                    SetAttackTarget(null);
                    return;
                }
                //目标角色死亡了或者已经不在可视范围内
                if (TargetObj.IsDie() || !SelObj.GetPoint().CheckVisualDistance(TargetObj.GetCurrentX(),
                    TargetObj.GetCurrentY(),mAiInfo.nRange))
                {
                    TargetObj = null;
                    nState = BaseAI.STATE_IDLE; 
                    if (findlist != null) findlist.Clear();
                }
            
                
                if (TargetObj != null && 
                    System.Environment.TickCount - mnLastMoveTick > mAiInfo.nMove_Speed &&
                    mAiInfo.bMove)
                {
                    //与目标差距太大-- 就跟随
                    if (Math.Abs(TargetObj.GetCurrentX() - SelObj.GetCurrentX()) > mAiInfo.nAttack_Range ||
                        Math.Abs(TargetObj.GetCurrentY() - SelObj.GetCurrentY()) > mAiInfo.nAttack_Range)
                    {
                        if (findlist != null && findlist.Count > 0)
                        {
                            MapServer.FindPoint point = findlist[findlist.Count - 1];
                            findlist.RemoveAt(findlist.Count - 1);
                            if (point.x == SelObj.GetCurrentX() && point.y == SelObj.GetCurrentY()) return;
                            byte dir = DIR.GetDirByPos(SelObj.GetCurrentX(), SelObj.GetCurrentY(), point.x, point.y);
                         //   Console.WriteLine("当前怪物坐标:"+SelObj.GetCurrentX().ToString()+"y:"+SelObj.GetCurrentY().ToString() + "目的坐标:"+point.x.ToString() + "y:" + point.y.ToString() + "方向:" + dir.ToString()+"\n");
                            if (findlist.Count == 0)
                            {
                                nState = BaseAI.ATTACK;
                               
                            //    Console.Write("------------------------------------------------------------\n");
                            }
                            SelObj.Walk(dir,point.x,point.y);
                            
                        }else this.FollowTarget();

                        mnLastMoveTick = System.Environment.TickCount ;
                       // Console.WriteLine((System.Environment.TickCount - mnLastMoveTick).ToString());
                        return;
                    }
            
                  
                   
                }
                //攻击目标
                if(TargetObj != null &&
                    System.Environment.TickCount - mnLastAttackTick > mAiInfo.nAttack_Speed)
                {
                    //在攻击范围内
                    if(Math.Abs(TargetObj.GetCurrentX() - SelObj.GetCurrentX()) <= mAiInfo.nAttack_Range &&
                        Math.Abs(TargetObj.GetCurrentY() - SelObj.GetCurrentY()) <= mAiInfo.nAttack_Range)
                    {
                        NetMsg.MsgMonsterAttackInfo msg = new NetMsg.MsgMonsterAttackInfo();
                        msg.Create(null, null);
                        msg.monsterid = SelObj.GetTypeId();
                        msg.roleid = TargetObj.GetTypeId(); ;
                        msg.role_x = TargetObj.GetCurrentX();
                        msg.role_y = TargetObj.GetCurrentY();
                        msg.injuredvalue = BattleSystem.AdjustDamage(SelObj, TargetObj); ;
                        byte[] buff = msg.GetBuffer();
                        //广播
                        (SelObj as MonsterObject).BrocatBuffer(buff);
                        //GameStruct.Action act = new GameStruct.Action(GameStruct.Action.ATTACK, buff);
                        //SelObj.PushAction(act);

                        //反馈角色被攻击--减血3
                        NetMsg.MsgAttackInfo info = new NetMsg.MsgAttackInfo();
                        info.tag = 21;
                        TargetObj.Injured(SelObj, msg.injuredvalue, info);
                        mnLastAttackTick = System.Environment.TickCount;
                    }
                }

            }
        }

        public virtual void Injured(BaseObject attackobj)
        {
            //if (TargetObj == null)
            //{
            //    TargetObj = attackobj;
            //    mnLastAttackTick = System.Environment.TickCount ;
            //}
            SetAttackTarget(attackobj);
            nState = BaseAI.INJURED;
        }

        public virtual void Die()
        {
            TargetObj = null;
            if(findlist != null) findlist.Clear();
        }

        public virtual void SetAttackTarget(BaseObject obj)
        {

            if (obj != null && !SelObj.CanPK(obj))
            {
                //对象不可攻击
                TargetObj = null;
                nState = BaseAI.STATE_IDLE;
                return;
            }
            TargetObj = obj;
            if (TargetObj == null)
            {
                nState = BaseAI.STATE_IDLE;
            }
            else
            {
                nState = BaseAI.ATTACK;
            }
            //mnLastAttackTick = System.Environment.TickCount;
            //mnLastMoveTick = mnLastAttackTick;
            if (findlist != null) findlist.Clear();
        }
        public virtual void FollowTarget()
        {
            if (!SelObj.GetGameMap().CanMove(TargetObj.GetCurrentX(), TargetObj.GetCurrentY()))
            {
                return;
            }
            findlist = SelObj.GetGameMap().GetMapPath().FindPath(SelObj.GetCurrentX(),
                SelObj.GetCurrentY(), TargetObj.GetCurrentX(), TargetObj.GetCurrentY());
            if (findlist != null && findlist.Count > 0)
            {
                findlist.RemoveAt(0); //这个是角色目标点- 删掉
            }
            //short x = 0;
            //short y = 0;
            //byte dir = 0;
            //short targetx,targety;
            //targetx = TargetObj.GetCurrentX();
            //targety = TargetObj.GetCurrentY();
            //for (int i = 0; i < 8; i++)
            //{
            //   short tempx =(short) (targetx + DIR._DELTA_X[i]);
            //   short tempy = (short) (targety + DIR._DELTA_Y[i]);
            //   if (SelObj.GetGameMap().GetPointOfObj(SelObj, x, y))
            //   {
            //       if (x == 0 && y == 0) { x = tempx; y = tempy; }
            //       if(Math.Abs(tempx - targetx) < Math.Abs(x - targetx) &&
            //           Math.Abs(tempy - targety) < Math.Abs(y - targety))
            //       {
            //           x = tempx;
            //           y = tempy;
            //       }
            //   }

            //}
            //if (x != 0 && y != 0)
            //{
            //    dir = DIR.GetNextDir(SelObj.GetCurrentY(), SelObj.GetCurrentY(), x, y);
            //    SelObj.Walk(dir);
            //}
            
        }
        //主动攻击
        protected virtual void ActiveAttackPlay()
        {
            if (mAiInfo.nType != Define.MONSTER_TYPE_ACTIVE) return;//不是主动怪物- 
            //寻找目标需要时间-- 
            if (System.Environment.TickCount - mnActiveAttackTick < Define.MONSTER_ACTIVEATTACK_TIME * 1000)
            {
                return;
            }
            mnActiveAttackTick = System.Environment.TickCount;
            GameStruct.Point point = null;
            GameStruct.Point newPoint = null;
            BaseObject newObj = null;
            if (TargetObj == null && 
                System.Environment.TickCount - mnLastMoveTick > mAiInfo.nMove_Speed &&
                SelObj.GetVisibleList().Count > 0 
                )
            {
                foreach (RefreshObject refobj in SelObj.GetVisibleList().Values)
                {
                    if(refobj.obj.type != OBJECTTYPE.PLAYER && //只攻击角色与幻兽与暗黑龙骑的守护骑士
                        refobj.obj.type != OBJECTTYPE.EUDEMON &&
                        refobj.obj.type != OBJECTTYPE.GUARDKNIGHT &&
                        refobj.obj.type != OBJECTTYPE.MONSTER &&
                        refobj.obj.type != OBJECTTYPE.CALLOBJECT) 
                    {
                        continue;
                    }
                    //如果是暗杀邪龙. 不攻击主人

                    if (!SelObj.CanPK(refobj.obj))
                    {
                        continue;
                    }
                    point = new GameStruct.Point();
                    point.x = (short)Math.Abs(refobj.obj.GetCurrentX() - SelObj.GetCurrentX());
                    point.y = (short)Math.Abs(refobj.obj.GetCurrentY() - SelObj.GetCurrentY());
                    if (newPoint == null)
                    {
                        newPoint = point;
                        newObj = refobj.obj;
                    }
                    if(point.x < newPoint.x && 
                        point.y < newPoint.y)
                    {
                        newPoint = point;
                        newObj = refobj.obj;
                    }
                }
                if (newObj != null)
                {
                    //设置为攻击状态
                    TargetObj = newObj;
                    nState = BaseAI.ATTACK;
                    mnLastMoveTick = System.Environment.TickCount;
                }
                
            }
        }


      
    }
}
