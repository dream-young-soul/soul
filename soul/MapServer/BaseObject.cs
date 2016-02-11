using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameBase.Config;
using System.Diagnostics;
using System.Threading;
using GameBase.Network;
using GameStruct;
using NetMsg;

namespace MapServer
{
    //回收ID结构
    struct RecoveryID
    {
        public uint nID;
        public byte bType;
    }
    public class IDManager
    {
       // private static IDManager m_Instance = null;

        private static uint player_start_id = 1000000;    //玩家起始id
        private static uint player_end_id = 1999999999;  //玩家结束id
        private static uint playser_id = player_start_id;


        private static uint monster_start_id = 400001; //怪物起始id
        private static uint monster_end_id = 599999;    //怪物结束id
        private static uint monster_id = monster_start_id;

        private static uint guardknight_start_id = 700001; //暗黑龙骑 守护骑士起始ID
        private static uint guardknight_end_id = 899999;
        private static uint guardknight_id = guardknight_start_id;
        public static uint eudemon_start_id = 2000000000; //幻兽的起始id
        private static uint eudemon_end_id = 3999999999;    //幻兽的结束id
        private static uint eudemon_id = eudemon_start_id;
        private static uint npc_start_id = 400001; //npc起始id
 
        private static uint npc_id = npc_start_id;

        private static uint effect_start_id = 50001;
        private static uint effect_end_id = 69999;
        private static uint effect_id = effect_start_id;

    
        private static uint _id = 0;

        //回收的id-
        private static List<RecoveryID> mListRecovery = new List<RecoveryID>();

        public static void RecoveryTypeID(uint id, byte type)
        {
            RecoveryID info;
            info.nID = id;
            info.bType = type;
            mListRecovery.Add(info);
        }
        public static uint CreateTypeId(byte type)
        {
            uint ret = 0;
            //优先从回收id列表中取出
            for (int i = 0; i < mListRecovery.Count; i++)
            {
                if (mListRecovery[i].bType == type)
                {
                    ret = mListRecovery[i].nID;
                    mListRecovery.RemoveAt(i);
                    return ret;
                }
            }
                switch (type)
                {
                    case OBJECTTYPE.MONSTER:
                        {
                            if (monster_id > monster_end_id)
                            {
                                Log.Instance().WriteLog("生成怪物类型id失败...,这么多怪物！ 我操！！");
                                return 0;
                            }
                            ret = monster_start_id;
                            monster_start_id++;
                            break;
                        }
                    case OBJECTTYPE.NPC:
                        {
                            ret = npc_id;
                            npc_id++;
                            break;
                        }
                    case OBJECTTYPE.PLAYER:
                        {
                            if (playser_id > player_end_id)
                            {
                                Log.Instance().WriteLog("生成玩家类型id失败...要重启机器了啊...");
                                return 0;
                            }
                            ret = playser_id;
                            playser_id++;
                            break;
                        }
                    case OBJECTTYPE.EUDEMON:
                        {
                            if (eudemon_id > eudemon_end_id)
                            {
                                Log.Instance().WriteLog("生成幻兽类型id失败...要重启机器了啊...");
                                return 0;
                            }
                            ret = eudemon_id;
                            eudemon_id++;

                            break;
                        }
                    case OBJECTTYPE.GUARDKNIGHT:
                        {
                            if (guardknight_id > guardknight_end_id)
                            {
                                Log.Instance().WriteLog("生成守护类型id失败...要重启机器了啊...");
                                return 0;
                            }
                            ret = guardknight_id;
                            guardknight_id++;
                            break;
                        }
                    case OBJECTTYPE.EFFECT:
                        {
                            if (effect_id > effect_end_id)
                            {
                                Log.Instance().WriteLog("生成特效类id失败...要重启机器了啊...");
                                return 0;
                            }
                            ret = effect_id;
                            effect_id++;
                            break;
                        }
                }
           
            return ret;
        }
        //取幻兽的身份牌号码
        //暂时先随机，可重复-
        //2015.9.28
        public static int CreateEudemonCard()
        {
            //创建的身份证号是五位数到十位数哒
            int count = IRandom.Random(5, 10);
            String str = IRandom.Random(1,9).ToString();
            for(int i = 1;i < count;i++)
            {
                str += IRandom.Random(0, 9).ToString();
            }
            return Convert.ToInt32(str);
        }

        //取幻兽的五行属性
        public static int GetEudemonWuxing()
        {
            int wuxing = IRandom.Random(1, 5);
            if (wuxing == (int)GameStruct.EudemonWuXing.LEI) //雷属性再加百分之五十概率
            {
                if (IRandom.Random(1, 100) < 50)
                {
                    wuxing = IRandom.Random(1, 5);
                }
            }
            return wuxing;
        }
        public static uint CreateId()
        {
            _id++;
            return _id;
        }
    }

    //npc对象
    public class NpcObject : BaseObject
    {
        public uint ScriptId; //脚本id
        public GameStruct.NPCInfo mInfo;
        public NpcObject(GameStruct.NPCInfo info)
        {
            type = OBJECTTYPE.NPC;
            mInfo = info;
        }
    }
    //摊位对象
    public class PtichObject : BaseObject
    {
        private PlayerObject mPlay;
        public PtichObject(PlayerObject Play)
        {
            type = OBJECTTYPE.PTICH;
            mPlay = Play;
            typeid = (uint)(107000 + mPlay.GetCurrentPtichID());
        }

        public override void RefreshVisibleObject()
        {
            base.RefreshVisibleObject();
            //只遍历玩家
            foreach (BaseObject o in mGameMap.GetAllObject().Values)
            {
                if (o.GetGameID() == this.GetGameID()) continue;
                if (o.type != OBJECTTYPE.PLAYER )
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

        //刷新
        public void Refresh()
        {
            this.RefreshVisibleObject();
            foreach (RefreshObject o in this.GetVisibleList().Values)
            {
                BaseObject obj = o.obj;
                this.SendInfo(obj as PlayerObject);
            }
        }
        public void SendInfo(PlayerObject play)
        {
            byte[] data3 = { 41, 0, 238, 7, 28, 162, 1, 0, 91, 1, 27, 2, 144, 1, 0, 0, 34, 0, 0, 0, 0, 0, 0, 0, 76, 0, 0, 0, 1, 8, 211, 249, 193, 250, 207, 201, 181, 192, 0, 0, 0 };
            //play.SendData(data3, true);
            PacketOut outpack = new PacketOut();
            String sName = mPlay.GetName();
            int nLen = 33 + GameBase.Core.Coding.GetDefauleCoding().GetBytes(sName).Length;
            outpack.WriteInt16((short)nLen);
            outpack.WriteInt16(2030);
            outpack.WriteUInt32(this.GetTypeId());
            outpack.WriteInt16(this.GetCurrentX());
            outpack.WriteInt16(this.GetCurrentY());
            outpack.WriteInt32(400);//144, 1, 0, 0
            outpack.WriteInt32(34);
            outpack.WriteInt32(0);
            outpack.WriteInt32(76);
            outpack.WriteByte(1);
            outpack.WriteString(sName);
            outpack.WriteByte(0);
            outpack.WriteByte(0);
            outpack.WriteByte(0);
          //  Log.Instance().WriteLog(GamePacketKeyEx.byteToText(outpack.GetNormalBuff()));
            play.SendData(outpack.Flush(), true);
         //   byte[] data3 = { 41, 0, 238, 7, 28, 162, 1, 0, 91, 1, 27, 2, 144, 1, 0, 0, 34, 0, 0, 0, 0, 0, 0, 0, 76, 0, 0, 0, 1, 8, 211, 249, 193, 250, 207, 201, 181, 192, 0, 0, 0 };
           // this.SendData(data3, true);
        }
        
    }
    public class RefreshObject
    {
        public bool bRefreshTag; //是否是刷新标记
        public BaseObject obj;
        public RefreshObject()
        {
            Reset();
        }
        public void Reset()
        {
            bRefreshTag = false;
            obj = null;
        }
    }
    //游戏对象基类
    //2015.8.12
    public class BaseObject
    {
        protected uint gameid; //动态内存id- 玩家索引用的-
        protected uint typeid;//类型id
        protected uint id;     //id 

        private GameStruct.Point mPoint;//当前坐标
     
        private byte bDir;              //当前方向
        public String Name;     //名字
        public byte type;       //对象类型
        public GameMap mGameMap; //地图对象
        //public Dictionary<uint, BaseObject> mVisibleList; //玩家可视对象
        public Dictionary<uint, RefreshObject> mVisibleList; //玩家可视对象
       
        private List<GameStruct.Action> mActionList;
        public GameBase.Network.GameSession session;
        //public Dictionary<uint, BaseObject> GetVisibleList() { return mVisibleList; }
        public Dictionary<uint, RefreshObject> GetVisibleList() { return mVisibleList; }
        private int walkTime; //下次走路时间戳
        private int lastwalktime; //最后一次走路时间戳

        public GameStruct.Point GetPoint() { return mPoint; }
        public int GetLastWalkTime() { return lastwalktime; }
        public void SetLastWalkTime(int _lasttime) { lastwalktime = _lasttime; }

        public int GetWalkTime() { return walkTime; }
        public void SetWalkTime(int _time) { walkTime = _time; }

        //锁定该角色与怪物--用于连击技能
        private bool mIsLock;
        private int locktime = 0;
        private int lastlocktime = System.Environment.TickCount;
        public void Lock(int time,bool isSendData = true) 
        { 
            locktime = time; 
            lastlocktime = System.Environment.TickCount;
            if (isSendData)
            {
                NetMsg.MsgLock msglock = new NetMsg.MsgLock();
                msglock.Lock();
                msglock.id = this.GetTypeId();
                msglock.x = GetCurrentX();
                msglock.y = GetCurrentY();
                this.GetGameMap().BroadcastBuffer(this, msglock.GetBuffer());
            }

            mIsLock = true;
        }
        //是否是被锁定
        public bool IsLock() { return mIsLock; }
        //检测是否超出锁定时间
        public bool CheckLockTime()
        {
            if (locktime == 0) return false;
            if (System.Environment.TickCount - lastlocktime > locktime)
            {
                locktime = 0;
                return false;
            }
            return true;
        }
        //解锁
        public void UnLock(bool isSendData = true) 
        {
            mIsLock = false;
            locktime = 0;
            lastlocktime = System.Environment.TickCount;
            if (isSendData)
            {
                NetMsg.MsgLock msglock = new NetMsg.MsgLock();
                msglock.UnLock();
                msglock.id = this.GetTypeId();
                msglock.x = GetCurrentX();
                msglock.y = GetCurrentY();
                this.RefreshVisibleObject();
                this.GetGameMap().BroadcastBuffer(this, msglock.GetBuffer());
            }
          
        }
        public BaseObject()
        {
           // mVisibleList = new Dictionary<uint, BaseObject>();
            mVisibleList = new Dictionary<uint, RefreshObject>();
           
            mActionList = new List<GameStruct.Action>();
            mPoint = new GameStruct.Point();
            type = OBJECTTYPE.NORMAL;
            gameid = IDManager.CreateId();
            session = null;
        }
        public uint GetID() { return id; }
        public void SetID(uint __id) { id = __id; }
        public uint GetGameID() { return gameid; }
        public uint GetTypeId() { return typeid; }
       
        public GameBase.Network.GameSession GetGameSession() { return session; }
        public void SetGameSession(GameBase.Network.GameSession _session) { session = _session; }
        public String GetName() { return Name; }
        public void SetName(String _name) { Name = _name; }

        public virtual bool Run()
        {
            while (true)
            {
                GameStruct.Action act = PopAction();
                if (act == null) break;
                ProcessAction(act);
            }
            return true;
        }

        //刷新可视对象列表
        public virtual void RefreshVisibleObject()
        {
            //在地图被销毁的怪物啊-- 掉落物品啊..刷新的时候找不到了就删除掉！
            uint id = 0;
            List<uint> templist = null;
            foreach (RefreshObject refobj in mVisibleList.Values)
            {
                BaseObject obj = refobj.obj;
                if(obj.type == OBJECTTYPE.MONSTER ||
                    obj.type == OBJECTTYPE.GUARDKNIGHT) id =obj.GetTypeId();
                else id = obj.GetGameID();
                if(GetGameMap().GetObject(id) == null)
                {
                    if (templist == null) templist = new List<uint>();
                    templist.Add(id);
                }
            }
            if (templist != null)
            {
                for (int i = 0; i < templist.Count; i++)
                {
                    mVisibleList.Remove(templist[i]);
                }
            }
        }


        public void SetDir(byte dir){bDir = dir;}
        public byte GetDir(){return bDir;}


        public virtual void SetPoint(short x, short y)
        {
            //暂时允许怪物与人物重叠
            //2015.10.17
            //把原先的那个位置设置为寻路开放
            //if (this.GetGameMap() != null)
            //{
            //    this.GetGameMap().GetMapPath().SetPointMask(mPoint.x, mPoint.y, MapPath.MASK_OPEN);
            //}
           
            mPoint.x = x;
            mPoint.y = y;
            //if (this.GetGameMap() != null)
            //{
            //    //把现在的设置为寻路关闭
            //    this.GetGameMap().GetMapPath().SetPointMask(x, y, MapPath.MASK_CLOSE);
            //}

        }

        public short GetCurrentX() { return mPoint.x; }
        public short GetCurrentY() { return mPoint.y; }
        public GameMap GetGameMap() { return mGameMap; }
        public virtual void Walk(byte dir, short x, short y)
        {
            bDir = dir;
            SetPoint(x, y);
        }

        public virtual void Run(byte dir, int ucMode)
        {
            if (dir == DIR.MAX_DIRSIZE) return;
            Walk(dir);
            for (int i = 0; i < ucMode; i++)
            {
                Walk(dir);
            }
           // mPoint.x += DIR._DELTA_X[dir];
           // mPoint.y += DIR._DELTA_Y[dir];
        }
        public virtual void Walk(byte dir)
        {
            if (dir == DIR.MAX_DIRSIZE) return;
            bDir = dir;
           // mPoint.x += DIR._DELTA_X[dir];
          //  mPoint.y += DIR._DELTA_Y[dir];
            switch (dir)
            {
                case DIR.LEFT_DOWN:
                    {
                        mPoint.x -= 1;
                        mPoint.y += 1;
                        break;
                    }
                case DIR.LEFT:
                    {
                        mPoint.x -= 1;
                        break;
                    }
                case DIR.LEFT_UP:
                    {
                        mPoint.x -= 1;
                        mPoint.y -= 1;
                        break;
                    }
                case DIR.UP:
                    {
                        mPoint.y -= 1;
                        break;
                    }
                case DIR.RIGHT_UP:
                    {
                        mPoint.x += 1;
                        mPoint.y -= 1;
                        break;
                    }
                case DIR.RIGHT:
                    {
                        mPoint.x += 1;
                        break;
                    }
                case DIR.RIGHT_DOWN:
                    {
                        mPoint.x += 1;
                        mPoint.y += 1;
                        break;
                    }
                case DIR.DOWN:
                    {
                        mPoint.y += 1;
                        break;
                    }
            }
        }

        public virtual void PushAction(GameStruct.Action act)
        {
            mActionList.Add(act);
        }

        public virtual GameStruct.Action PopAction()
        {
           
            if (mActionList.Count > 0)
            {
                GameStruct.Action ret = mActionList[0];
                mActionList.Remove(ret);
                return ret;
            }
            return null;
        }

        public virtual void ProcessAction(GameStruct.Action act)
        {
            if (act == null) return;
            switch (act.GetAction())
            {
                case GameStruct.Action.MOVE:
                    {
                        ProcessAction_Move(act);
                        break;
                    }
                case GameStruct.Action.ATTACK:
                    {
                        ProcessAction_Attack(act);
                        break;
                    }
                case GameStruct.Action.DIE:
                    {
                        ProcessAction_Die(act);
                        break;
                    }
                case GameStruct.Action.ALIVE:
                    {
                        ProcessAction_Alive(act);
                        break;
                    }
                case GameStruct.Action.INJURED:
                    {
                        ProcessAction_Injured(act);
                        break;
                    }
            }
        }

        protected virtual void ProcessAction_Move(GameStruct.Action act) { }
        protected virtual void ProcessAction_Attack(GameStruct.Action act) { }
        protected virtual void ProcessAction_Die(GameStruct.Action act) { }
        protected virtual void ProcessAction_Alive(GameStruct.Action act) { }
        protected virtual void ProcessAction_Injured(GameStruct.Action act) { }
        public virtual void Injured(BaseObject obj, uint value, NetMsg.MsgAttackInfo info) { }


        public virtual void Dispose()
        {
            mVisibleList.Clear();
         
            mActionList.Clear();
            if (session != null)
            {
                session.Dispose();
                session = null;
            }
        }

        public virtual bool IsDie() { return false; }


        //计算属性
        public virtual void CalcAttribute() { }
     
        //取最小攻击
        public virtual int GetMinAck() { return 0; }
        //取最大攻击
        public virtual int GetMaxAck() { return 0; }
        //取防御
        public virtual int GetDefense() { return 0; }
        //取魔法防御
        public virtual int GetMagicDefense() { return 0; }
        //取等级
        public virtual byte GetLevel() { return 0; }
        //取魔法攻击
        public virtual int GetMagicAck() { return 0; }
        //取最大魔法攻击
        public virtual int GetMaxMagixAck() { return 0; }
        //取幸运
        public virtual int GetLuck() { return 0; }
        //计算经验
        public virtual int AdjustExp(int exp) { return exp; }

        //广播
        public virtual void BrocatBuffer(byte[] msg)
        {
            foreach (RefreshObject refobj in this.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                if (obj.type == OBJECTTYPE.PLAYER && obj.GetGameSession() != null)
                {
                    BaseMsg data = new BaseMsg();
                    data.Create(msg, obj.GetGamePackKeyEx());
                    obj.SendData(data.GetBuffer());

                }
            }
        }

        //从地图中删除自己
        public virtual void ClearThis()
        {
            
            NetMsg.MsgClearObjectInfo info = new NetMsg.MsgClearObjectInfo();
            info.id = GetTypeId();
            byte[] msg = info.GetBuffer();
            this.BrocatBuffer(msg);
            //移除范围内的对象..以便下次刷新怪物的时候刷新出来2015.10.16
            foreach (RefreshObject refobj in this.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                if (obj.type != OBJECTTYPE.PLAYER) continue;
                if (obj.GetVisibleList().ContainsKey(this.GetGameID()))
                {
                    obj.GetVisibleList().Remove(this.GetGameID());
                }
            }
            this.GetVisibleList().Clear();
            //  this.GetGameMap().BroadcastBuffer(this, msg);

        }

        //添加可视对象
        //
        public void AddVisibleObject(BaseObject obj,bool bRefreshTag = true)
        {
              RefreshObject refobj = null;
            if (this.GetVisibleList().ContainsKey(obj.GetGameID()))
            {
                refobj = mVisibleList[obj.GetGameID()];
                refobj.bRefreshTag = bRefreshTag;
                return;
            }
            refobj = new RefreshObject();
            refobj.bRefreshTag = bRefreshTag;
            refobj.obj = obj;
            mVisibleList[obj.GetGameID()] = refobj;
        }

        //检测是否可以攻击
        public virtual bool CanPK(BaseObject obj, bool bGoCrime = true)
        {
          
            return true;
        }

        //发送数据
        //data 要发送的数据
        //isEncode 是否加密
        public void SendData(byte[] data,bool isEncode = false)
        {
            if (this.GetGameSession() != null)
            {
                if (isEncode)
                {
                    if (GetGamePackKeyEx() == null)
                    {
                        Log.Instance().WriteLog("发送数据失败..玩家已掉线?");
                        return;
                    }
                    byte[] encode = new byte[data.Length];
                    Buffer.BlockCopy(data, 0, encode, 0, data.Length);
                    this.GetGamePackKeyEx().EncodePacket(ref encode, encode.Length);
                    this.GetGameSession().SendData(encode);
                }
                else
                {
                    this.GetGameSession().SendData(data);
                }
              
            }
        }

        public GameBase.Network.GamePacketKeyEx GetGamePackKeyEx()
        {
            if (this.GetGameSession() != null)
            {
                return this.GetGameSession().GetGamePackKeyEx();
            }
            return null;
        }
    }


   
}
