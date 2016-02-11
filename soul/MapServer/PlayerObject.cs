using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameBase.Config;
using GameBase.Network;
using System.Diagnostics;
using GameStruct;
using GameBase.Core;
using GameBase.Network.Internal;

namespace MapServer
{
    //玩家对象
    public class PlayerObject : BaseObject
    {
        public GameStruct.PlayerAttribute mAttribute; //角色属性
      
        private int lastattacktime;
        private uint face = 150001;  //头像
        private byte sex = Sex.MAN;   //性别

        public byte job = JOB.MAGE;    //职业
        private Dictionary<byte, uint> mMenuLink;  //脚本选项索引id 
        private bool bIsExit; //该玩家对象是否已经退出游戏  
        private bool m_bGhost; //是否是鬼魂状态
        private int mnGhostTick; //鬼魂状态时间戳- 角色播放死亡动作之后才变为鬼魂
        public bool IsGhost() { return m_bGhost; }

        private uint mnMountID = 0;//当前骑乘的坐骑id
        public uint GetMountID() { return mnMountID; }

        private int mnFightSoul = 0; //人物战斗力
        public int GetFightSoul() { return mnFightSoul; }
        //用户道具系统
        private PlayerItem mItemSystem;
        public PlayerItem GetItemSystem() { return mItemSystem; }
        //技能系统
        private PlayerMagic mMagicSystem;
        public PlayerMagic GetMagicSystem() { return mMagicSystem; }
        //幻兽系统
        private PlayerEudemon mEudemonSystem;
        public PlayerEudemon GetEudemonSystem() { return mEudemonSystem; }
        //战斗系统
        private PlayerFight mFightSystem;
        public PlayerFight GetFightSystem() { return mFightSystem; }
        //好友系统
        private PlayerFriend mFriendSystem;
        public PlayerFriend GetFriendSystem() { return mFriendSystem; }

        //交易系统
        private PlayerTrad mTradSystem;
        public PlayerTrad GetTradSystem() { return mTradSystem; }
        //定时系统
        private PlayerTimer mTimerSystem;
        public PlayerTimer GetTimerSystem() { return mTimerSystem; }
        //军团系统
        private PlayerLegion mLegionSystem;
        public PlayerLegion GetLegionSystem() { return mLegionSystem; }

        //pk系统
        private PlayerPK mPKSystem;
        public PlayerPK GetPKSystem() { return mPKSystem; }
        public uint GetFace() { return GetBaseAttr().lookface; }
        public void SetFace(uint _face) { face = _face; }
        public byte GetSex() { return sex; }
        //队伍
        private Team mTeam;
        public void SetTeam(Team _team) { mTeam = _team; }
        public Team GetTeam() { return mTeam; }

        //脚本回调id
        private uint mTaskID = 0; //暂时不用队列
        public void SetTaskID(uint _id) { mTaskID = _id; }
        public uint GetTaskID() { return mTaskID; }


        private GameStruct.GUANGJUELEVEL mGuanJue = GameStruct.GUANGJUELEVEL.NORMAL;
        //设置官爵等级
        public GameStruct.GUANGJUELEVEL GetGuanJue() { return mGuanJue; }
        //设置官爵等级
        public void SetGuanJue(GameStruct.GUANGJUELEVEL info) { mGuanJue = info; }

        //------------------------------------------ 2015.10.17
        //目前用于角色播放了静态表情动作，停在了动作最后一帧，在其他角色可视后，需要把动作值传下去。。
        private uint mnCurAction = GameBase.Config.Define._ACTION_STANDBY; //初始化动作为站立

        //被连击死亡后要设置的被攻击角色对象
        private BaseObject mTarget;

        //定时保存角色数据定时器
        public GameBase.Core.TimeOut mSaveTime;
        //设置角色当前动作
        public void SetCurrentAction(uint action)
        {
            mnCurAction = action;
        }
        //获取角色当前动作
        public uint GetCurrentAction() { return mnCurAction; }

        //当前召唤巫环的攻击目标
        private BaseObject mZhaoHuanWuHuanObj;
        public void SetZhaoHuanWuHuanObj(BaseObject obj) { mZhaoHuanWuHuanObj = obj; }


        //脚本随机到的数值
        private int mnCurrentRandom;
        public void SetCurrentRandom(int nValue) { mnCurrentRandom = nValue; }
        public int GetCurrentRandom() { return mnCurrentRandom; }

        //单击当前摊位的记录NPC
        private int mPtichId = -1;
        public void SetCurrentPtichID(int PtichId) { mPtichId = PtichId; }
        public int GetCurrentPtichID() { return mPtichId; }
        //远程摊位编号
        private int mRemotePtichId = 0;
        public void SetCurrentRemotePtichId(int RemotePtichId) { mRemotePtichId = RemotePtichId; }
        public int GetCurrentRemotePtichId() { return mRemotePtichId; }
        //使用道具指向的幻兽ID记录
        private uint mUseItemEudemonId;
        public void SetUseItemEudemonId(uint eudemon_id) { mUseItemEudemonId = eudemon_id; }
        public uint GetUseItemEudemonId() { return mUseItemEudemonId; }
        //设置传送状态
        private GameBase.Core.TimeOut mTransmitTimeOut = null;
        private bool mbTransmit = false;
        public void SetTransmitIng(bool v)
        {
            mbTransmit = v;
            if (mTransmitTimeOut == null)
            {
                mTransmitTimeOut = new GameBase.Core.TimeOut();
            }
            mTransmitTimeOut.SetInterval(1);
            mTransmitTimeOut.Update();
        }
        //------------------------------------------ 
        public void CalcSex()
        {
            int v = (int)GetBaseAttr().lookface % 2;
            if (v == 0) sex = Sex.WOMAN;
            else sex = Sex.MAN;
        }
        public byte GetJob() { return GetBaseAttr().profession; }
        public void SetJob(byte _job) { GetBaseAttr().profession = _job; }

        //双人舞------------------------------------------
        private short mnDancingId = 0;
        public void SetDancing(short nDancingId) { 
            mnDancingId = nDancingId;
            if (mnDancingId == 0)
            {
                //    收到网络协议:长度：16协议号:1012
                PacketOut outpack = new PacketOut();
                outpack.WriteInt16(16);
                outpack.WriteInt16(1012);
                outpack.WriteUInt32(this.GetTypeId());
                outpack.WriteInt32(0);
                outpack.WriteInt32(0);
                this.SendData(outpack.Flush(), true);
                //收到网络协议:长度：12协议号:1015
                outpack = new PacketOut();
                outpack.WriteInt16(12);
                outpack.WriteInt16(1015);
                outpack.WriteUInt32(this.GetTypeId());
                outpack.WriteInt32(340);
                this.SendData(outpack.Flush(), true);
            }
        }
        public bool IsDancing() { return mnDancingId != 0 ? true : false; }
        private int mnDancingTick = System.Environment.TickCount;//跳舞的时候会发一个停止跳舞过来。。这个拿来判断是否立即停止跳舞的。。不让他立即停止
        //------------------------------------------
        public int GetLookFace()
        {
            int ret = (int)GetBaseAttr().lookface;
            const int LOOK_MALEGHOST = 98;
            const int LOOK_FEMALEGHOST = 99;
            const int MASK_CHANGELOOK = 10000000;						// 变身时LOCKFACE的掩码，face=face + new_face*10000000
            if (IsGhost())
            {
                if (this.GetSex() == Sex.MAN)
                {
                    ret = (int)GetBaseAttr().lookface + MASK_CHANGELOOK * LOOK_MALEGHOST;
                    return ret;
                }
                else
                {
                    ret = (int)GetBaseAttr().lookface + MASK_CHANGELOOK * LOOK_FEMALEGHOST;
                    return ret;
                }
            }
            return ret;
        }
        public bool IsExit() { return bIsExit; }
        public void SetExit(bool _isExit) { bIsExit = _isExit; }

        //public BaseObject GetTargetObj() { return targetObject; }
        //public void SetTargetObj(BaseObject obj) { targetObject = obj; }

        public Dictionary<byte, uint> GetMenuLink() { return mMenuLink; }   //获取脚本选项索引id
        public void ClearScriptMenuLink() { mMenuLink.Clear(); }           //清除脚本选项索引id

        //设置当前说话的npc
        private GameStruct.NPCInfo mNpcInfo = null;
        public void SetCurrentNpcInfo(GameStruct.NPCInfo info) { mNpcInfo = info; }
        public GameStruct.NPCInfo GetCurrentNpcInfo() { return mNpcInfo; }
        public PlayerAttribute GetBaseAttr() { return mAttribute; }

      
        private List<GameStruct.HotkeyInfo> mListHotKey; //热键信息
        public PlayerObject()
        {
            mAttribute = new GameStruct.PlayerAttribute();
            type = OBJECTTYPE.PLAYER;
            typeid = IDManager.CreateTypeId(type);
            Name = "";
            lastattacktime = System.Environment.TickCount;

            mMenuLink = new Dictionary<byte, uint>();

            mItemSystem = new PlayerItem(this);
            mMagicSystem = new PlayerMagic(this);
            mTimerSystem = new PlayerTimer(this);
            mFightSystem = new PlayerFight(this);
            mEudemonSystem = new PlayerEudemon(this);
            mFriendSystem = new PlayerFriend(this);
            mTradSystem = new PlayerTrad(this);
            mLegionSystem = new PlayerLegion(this);
            mPKSystem = new PlayerPK(this);
            mListHotKey = new List<GameStruct.HotkeyInfo>();
            mZhaoHuanWuHuanObj = null;
            mTeam = null;
            m_bGhost = false;
            mnGhostTick = System.Environment.TickCount;
            mTarget = null;

            mSaveTime = new GameBase.Core.TimeOut();
            mSaveTime.SetInterval(GameBase.Config.Define.SAVEROLE_TIME);
            mnCurrentRandom = 0;
        }

        //public List<BaseObject> RefreshMagicVisibleObject(uint magicid)
        //{
        //    MagicTypeInfo info = ConfigManager.Instance().GetMagicTypeInfo(magicid);
        //    List<BaseObject> list = new List<BaseObject>();
        //    list.Clear();
        //    if(info == null)return list;
        //    short x = 0; short y = 0; int dis = 0;
        //     x = GetCurrentX();
        //     y = GetCurrentY();
        //    switch (info.sort)
        //    {
        //        case GameStruct.MagicTypeInfo.MAGICSORT_BOMB: //范围，以自身为原点-
        //            {

        //                dis = (int)info.distance;
        //                if (dis == 0) return list;
        //                this.RefreshVisibleObject();
        //                //foreach (BaseObject o in GetVisibleList().Values)
        //                //{
        //                //    if (o.GetGameID() == this.GetGameID()) continue;
        //                //    if (o.type != OBJECTTYPE.PLAYER && o.type != OBJECTTYPE.MONSTER) continue;
        //                //    if (o.GetPoint().CheckVisualDistance(x, y, dis))
        //                //    {
        //                //        list.Add(o);
        //                //    }
        //                //}
        //                break;
        //            }
        //        case GameStruct.MagicTypeInfo.MAGICSORT_FAN: //扇形攻击
        //            {

        //                this.RefreshVisibleObject();
        //               // this.GetFightSystem().ProcessFan();
        //                //foreach(BaseObject o in GetVisibleList().Values)
        //                //{
        //                //     if (o.GetGameID() == this.GetGameID()) continue;
        //                //    if (o.type != OBJECTTYPE.PLAYER && o.type != OBJECTTYPE.MONSTER) continue;
        //                //    if (o.GetPoint().CheckFanDistance(x, y, GetDir(), dis))
        //                //    {
        //                //        list.Add(o);
        //                //    }
        //                //}
        //                break;
        //            }
        //    }


        //    return list;
        //}

        public override void RefreshVisibleObject() //刷新可视对象
        {
            base.RefreshVisibleObject();
            foreach (BaseObject o in mGameMap.GetAllObject().Values)
            {
                if (o.GetGameID() == this.GetGameID()) continue;
                int dis = GameBase.Config.Define.MAX_VISIBLE_DISTANCE;
                //已经不在视野范围..清除该对象
                if (this.mVisibleList.ContainsKey(o.GetGameID()))
                {
                    if (!GetPoint().CheckVisualDistance(o.GetCurrentX(), o.GetCurrentY(), dis))
                    {
                        mVisibleList.Remove(o.GetGameID());

                        o.GetVisibleList().Remove(this.GetGameID());
                        //只清除玩家,怪物与npc客户端会自动清除
                        if (o.type == OBJECTTYPE.PLAYER)
                        {
                            // (o as PlayerObject).ClearThis();
                            this.ClearThis(o as PlayerObject);
                            (o as PlayerObject).ClearThis(this);
                        }
                    }
                    continue;
                }
                //新加入的对象
                if (this.GetPoint().CheckVisualDistance(o.GetCurrentX(), o.GetCurrentY(), dis))
                {
                    //- 清除并且死亡的怪物对象不加到列表中
                    if (o.type == OBJECTTYPE.MONSTER)
                    {
                        if ((o as MonsterObject).IsClear()) continue;
                    }
                    //RefreshObject refobj = new RefreshObject();
                    //refobj.obj = o;
                    //refobj.bRefreshTag = false;
                    //mVisibleList[o.GetGameID()] = refobj;
                    this.AddVisibleObject(o, false);
                }
            }
        }
        //public override void RefreshVisibleObject() //刷新可视对象
        //{
        //    base.RefreshVisibleObject();
        //    mRefreshList.Clear();
        //    foreach (BaseObject o in mGameMap.mDicObject.Values)
        //    {
        //        if (o.GetGameID() == this.GetGameID()) continue;
        //        //玩家和玩家的视野范围扩大- 
        //       // int dis = o.type == OBJECTTYPE.PLAYER ? Define.MAX_PLAY_VISIBLE_DISTANCE : Define.MAX_VISIBLE_DISTANCE;
        //        int dis = Define.MAX_VISIBLE_DISTANCE;
        //        //如果是怪物和npc 掉落物品对象就不发消息了-- 已经在视野范围了嘛
        //        if ((o.type == OBJECTTYPE.NPC || o.type == OBJECTTYPE.MONSTER ||
        //            o.type == OBJECTTYPE.EUDEMON  || o.type == OBJECTTYPE.DROPITEM || 
        //            o.type == OBJECTTYPE.PLAYER || o.type == OBJECTTYPE.ROBOT)  &&
        //            this.mVisibleList.ContainsKey(o.GetGameID()))
        //        {
        //            if (!GetPoint().CheckVisualDistance(o.GetCurrentX(), o.GetCurrentY(), dis)
        //                 && this.mVisibleList.ContainsKey(o.GetGameID()))
        //            {
        //                this.mVisibleList.Remove(o.GetGameID());
        //                if (this.mPlayObject.ContainsKey(o.GetGameID()))
        //                {
        //                    this.mPlayObject.Remove(o.GetGameID());
        //                }
        //            }
        //            continue;
        //        }
        //        if (GetPoint().CheckVisualDistance(o.GetCurrentX(), o.GetCurrentY(), dis))
        //        {
        //            this.mVisibleList[o.GetGameID()] = o;
        //             if (o.type == OBJECTTYPE.MONSTER)
        //             {
        //                    MonsterObject mobj = o as MonsterObject;
        //                    if (mobj.IsDie()) continue;
        //             }
        //             if (o.type == OBJECTTYPE.NPC ||
        //                 o.type == OBJECTTYPE.MONSTER ||
        //                 o.type == OBJECTTYPE.PLAYER ||
        //                 o.type == OBJECTTYPE.DROPITEM ||
        //                 o.type == OBJECTTYPE.EUDEMON ||
        //                 o.type == OBJECTTYPE.ROBOT) 

        //            {
        //                mRefreshList[o.GetGameID()] = o;

        //            }
        //        }
        //        else
        //        {
        //            if (!GetPoint().CheckVisualDistance(o.GetCurrentX(), o.GetCurrentY(), dis)
        //                && this.mVisibleList.ContainsKey(o.GetGameID()))
        //            {
        //                this.mVisibleList.Remove(o.GetGameID());
        //                if ((o.type == OBJECTTYPE.PLAYER || o.type == OBJECTTYPE.EUDEMON || 
        //                    o.type == OBJECTTYPE.ROBOT) &&
        //                    mPlayObject.ContainsKey(o.GetGameID()))
        //                {
        //                    mPlayObject.Remove(o.GetGameID());

        //                    //清除目标对象
        //                    NetMsg.MsgClearObjectInfo clearobj = new NetMsg.MsgClearObjectInfo();
        //                    clearobj.Create(null, this.GetGamePackKeyEx());
        //                    clearobj.id = o.GetTypeId();
        //                    this.SendData(clearobj.GetBuffer());
        //                    if (o.type == OBJECTTYPE.PLAYER)
        //                    {
        //                        clearobj = new NetMsg.MsgClearObjectInfo();
        //                        clearobj.Create(null, o.GetGamePackKeyEx());
        //                        clearobj.id = this.GetTypeId();
        //                        o.SendData(clearobj.GetBuffer());
        //                        (o as PlayerObject).GetPlayObjectList().Remove(this.GetGameID());
        //                    }
        //                    if (o.type == OBJECTTYPE.EUDEMON)
        //                    {
        //                        (o as EudemonObject).GetPlayObjectList().Remove(this.GetGameID());
        //                    }

        //                }
        //            }

        //        }
        //    }
        //}

        public bool Move(NetMsg.MsgMoveInfo move)
        {

            if (!this.GetMagicSystem().CheckMoveSpeed())
            {
                this.ScroolRandom(this.GetCurrentX(), this.GetCurrentY());

                return false;
            }
            // testeudemon();
            byte dir = (byte)((int)move.dir % 8);
            this.SetDir(dir);

            short nNewX = GetCurrentX();
            short nNewY = GetCurrentY();
            nNewX += DIR._DELTA_X[dir];
            nNewY += DIR._DELTA_Y[dir];
            if (!mGameMap.CanMove(nNewX, nNewY))
            {
                nNewX = this.GetCurrentX();
                nNewY = this.GetCurrentY();
                if (!mGameMap.CanMove(nNewX, nNewY))
                {
                    nNewX = (short)mGameMap.GetMapInfo().recallx;
                    nNewY = (short)mGameMap.GetMapInfo().recally;
                }
                //暂时使用随机卷的方式重置玩家坐标
                this.ScroolRandom(nNewX, nNewY);
               // Log.Instance().WriteLog("非法封包..禁止走路！！x:" + nNewX.ToString() + "y:" + nNewY.ToString());
                return false;
            }
            // 跑步模式的阻挡判断
            bool IsRun = false;
            if (move.ucMode >= DIR.MOVEMODE_RUN_DIR0 && move.ucMode <= DIR.MOVEMODE_RUN_DIR7 && GetBaseAttr().sp > 0 /*没有体力不让跑*/)
            {

              
                nNewX += DIR._DELTA_X[move.ucMode - DIR.MOVEMODE_RUN_DIR0];
                nNewY += DIR._DELTA_Y[move.ucMode - DIR.MOVEMODE_RUN_DIR0];
                IsRun = true;
                if (!mGameMap.CanMove(nNewX, nNewY))
                {
                    nNewX = this.GetCurrentX();
                    nNewY = this.GetCurrentY();
                    if (!mGameMap.CanMove(nNewX, nNewY))
                    {
                        nNewX = (short)mGameMap.GetMapInfo().recallx;
                        nNewY = (short)mGameMap.GetMapInfo().recally;
                    }
                    //暂时使用随机卷的方式重置玩家坐标
                    this.ScroolRandom(nNewX, nNewY);
                    //Log.Instance().WriteLog("非法封包..禁止走路！！x:" + nNewX.ToString() + "y:" + nNewY.ToString());
                    //Log.Instance().WriteLog("这个家伙肯定用了外挂,不如我们把他封号吧,角色名称:" + this.GetName() +
                    //    "如果下次他还开外挂,就把他硬盘里的种子全删掉...");
                    return false;
                }
            }
            //传送点判断
            uint mapid = 0; short x = 0; short y = 0;
            if (ConfigManager.Instance().CheckMapGate(this.GetGameMap().GetMapInfo().id, nNewX, nNewY, ref mapid, ref x, ref y))
            {
                this.ChangeMap(mapid, x, y);
                return false;
            }
            if (GetBaseAttr().sp <= 0) move.ucMode = 0;
            SetPoint(nNewX, nNewY);

            GameStruct.Action action = new GameStruct.Action(GameStruct.Action.MOVE);
            if (IsRun) action.AddObject(move.ucMode);

            //解除锁定目标
            this.GetFightSystem().SetAutoAttackTarget(null);
            PushAction(action);
            return true;
        }

        public override bool Run()
        {

            //意外断开处理
            if (GetGameSession() == null)
            {
                //广播出去,清除该玩家
                if (GetVisibleList().Count > 0)
                {
                    NetMsg.MsgClearObjectInfo clear = new NetMsg.MsgClearObjectInfo();
                    clear.id = this.GetTypeId();
                    GetGameMap().BroadcastBuffer(this, clear.GetBuffer());
                }

                return false;
            }
            base.Run();
            //连招解锁
            if (this.IsLock())
            {
                if (!this.CheckLockTime())
                {
                    this.UnLock();
                }
            }
            this.GetTimerSystem().Run();
            //战斗系统
            this.GetFightSystem().Run();
            this.GetPKSystem().Run();
            //角色死亡变为鬼魂状态,
            if (this.IsDie() && m_bGhost && mnGhostTick != -1)
            {
                if (System.Environment.TickCount - mnGhostTick >= 3000)
                {
                    this.ChangeAttribute(UserAttribute.STATUS, 6);
                    this.ChangeAttribute(UserAttribute.LOOKFACE, GetLookFace(), true);

                    mnGhostTick = -1;

                }
            }

            //死亡--因为会被连击锁定-- 所以放到run方法
            if (IsDie() && !this.IsLock() && mTarget != null && m_bGhost == false)
            {
                GameStruct.Action action;
                //死亡
                action = new GameStruct.Action(GameStruct.Action.DIE, null);
                action.AddObject(mTarget);

                this.PushAction(action);
            }

            //定时保存玩家数据
            if (mSaveTime.ToNextTime())
            {
                UserEngine.Instance().AddSaveRole(this);
            }

            //传送状态
            if (mbTransmit && mTransmitTimeOut.ToNextTime())
            {
                //幻兽出征
                this.GetEudemonSystem().Eudemon_BattleAll();
                //发送天气信息
                this.GetGameMap().SendWeatherInfo(this);
                this.SetTransmitIng(false);
            }
            return true;

        }

        //npc
        public void SendNpcInfo(BaseObject obj)
        {
            if (GetGameSession() == null) return;
            NetMsg.MsgNpcInfo info = new NetMsg.MsgNpcInfo();
            info.Create(null, session.GetGamePackKeyEx());
            int lookface = (obj as NpcObject).mInfo.lookface;
            info.Init(obj.GetID(), obj.GetCurrentX(), obj.GetCurrentY(), lookface);
          
            
            this.SendData(info.GetBuffer());
        }

        public void SendDropItemInfo(BaseObject obj)
        {
            DropItemObject itemobj = obj as DropItemObject;
            NetMsg.MsgDropItem data = new NetMsg.MsgDropItem();
            data.Create(null, GetGamePackKeyEx());
            data.SetRefreshTag();
            data.id = itemobj.GetGameID();
            data.typeid = itemobj.GetTypeId();
            data.x = itemobj.GetCurrentX();
            data.y = itemobj.GetCurrentY();

            this.SendData(data.GetBuffer());

        }

        public void SendRoleInfo(PlayerObject play)
        {
            NetMsg.MsgRoleInfo role = new NetMsg.MsgRoleInfo();
            role.Create(null, this.GetGamePackKeyEx());
            role.role_id = play.GetTypeId();
            role.x = play.GetCurrentX();
            role.y = play.GetCurrentY();
            role.armor_id = play.GetItemSystem().GetArmorLook();
            role.wepon_id = play.GetItemSystem().GetWeaponLook();
            // role.face_sex = play.GetFace();
            role.face_sex = (uint)play.GetLookFace();
            role.face_sex1 = play.GetBaseAttr().lookface;
       
            role.dir = play.GetDir();
            role.action = play.GetCurrentAction();
            role.guanjue = (byte)play.GetGuanJue();
            role.hair_id = play.GetBaseAttr().hair;
            role.str.Add(play.GetName());
            role.rid_id = play.GetMountID();

            //军团
            if (play.GetLegionSystem().IsHaveLegion() && play.GetLegionSystem().GetLegion() != null)
            {
                role.legion_id = play.GetLegionSystem().GetLegion().GetBaseInfo().id;
                role.legion_title = play.GetLegionSystem().GetLegion().GetBaseInfo().title;
                role.legion_place = play.GetLegionSystem().GetPlace();
                role.legion_id1 = role.legion_id;
            }


            this.SendData(role.GetBuffer());
            //发送状态
            play.GetTimerSystem().SendState(this);
            //军团名称-
            if (role.legion_id > 0)
            {
                NetMsg.MsgLegionName legion = new NetMsg.MsgLegionName();
                legion.Create(null, this.GetGamePackKeyEx());
                legion.legion_id = role.legion_id;
                legion.legion_name = play.GetLegionSystem().GetLegion().GetBaseInfo().name;
                this.SendData(legion.GetBuffer());

            }
            //加到对方玩家可视列表
            //if (!this.GetVisibleList().ContainsKey(play.GetGameID()))
            //{
            //    RefreshObject refobj = new RefreshObject();
            //    refobj.bRefreshTag = true;
            //    refobj.obj = play;
            //    this.GetVisibleList()[play.GetGameID()] = refobj;
            //}
            this.AddVisibleObject(play, true);
            //前面发送了角色的lookface 却并没有变为鬼魂状态,用这个协议号再改变一次..偷个懒 省的去分析封包结构了。
            if (play.IsDie() && play.IsGhost())
            {
                play.ChangeAttribute(UserAttribute.LOOKFACE, play.GetLookFace(), true);
            }

        }
        public void SendRoleMoveInfo(BaseObject obj, byte runValue, RefreshObject _refobj)
        {

            if (obj.type != OBJECTTYPE.PLAYER) return;
            PlayerObject play = obj as PlayerObject;
            // if (!play.GetPlayObjectList().ContainsKey(this.GetGameID()) ||
            //     !this.GetPlayObjectList().ContainsKey(play.GetGameID()))
            if (_refobj.bRefreshTag == false)
            {
                this.SendRoleInfo(play);
                play.SendRoleInfo(this);
                _refobj.bRefreshTag = true; //设置为已刷新标记
                //NetMsg.MsgRoleInfo role = new NetMsg.MsgRoleInfo();
                //role.Create(null, obj.GetGamePackKeyEx());

                //role.role_id = this.GetTypeId();
                //role.x = this.GetCurrentX();
                //role.y = this.GetCurrentY();
                //role.armor_id = this.GetItemSystem().GetArmorLook();
                //role.wepon_id = this.GetItemSystem().GetWeaponLook();
                //role.face_sex = this.GetFace();
                //role.dir = this.GetDir();
                //role.guanjue = (byte)this.GetGuanJue();
                //role.str.Add(this.GetName());
                //obj.SendData(role.GetBuffer());

                ////这个玩家也刷新给自己---
                //role = new NetMsg.MsgRoleInfo();
                //role.Create(null, this.GetGamePackKeyEx());
                //role.role_id = play.GetTypeId();
                //role.x = play.GetCurrentX();
                //role.y = play.GetCurrentY();
                //role.armor_id = play.GetItemSystem().GetArmorLook();
                //role.wepon_id = play.GetItemSystem().GetWeaponLook();
                //role.face_sex = play.GetFace();
                //role.dir = play.GetDir();
                //role.guanjue = (byte)play.GetGuanJue();
                //role.str.Add(play.GetName());

                //this.SendData(role.GetBuffer());
                //play.GetPlayObjectList()[this.GetGameID()] = this;
                //this.GetPlayObjectList()[play.GetGameID()] = play;

                return;
            }
            //存在可视列表就发移动消息
            NetMsg.MsgMoveInfo move = new NetMsg.MsgMoveInfo();

            move.id = this.GetTypeId();
            move.x = this.GetCurrentX();
            move.y = this.GetCurrentY();
            move.dir = this.GetDir();
            move.ucMode = runValue;
            obj.SendData(move.GetBuffer(), true);
        }
        public void SendMonsterInfo(BaseObject obj)
        {
            if (this.GetGameSession() == null) return;

            NetMsg.MsgMonsterInfo info = new NetMsg.MsgMonsterInfo();
            info.Create(null, GetGamePackKeyEx());
            MonsterObject o = obj as MonsterObject;
            info.id = o.GetTypeId();
            info.typeid = o.GetBasicAttribute().id;
            info.lookface = o.GetBasicAttribute().lookface;
            info.x = o.GetCurrentX();
            info.y = o.GetCurrentY();
            info.level = o.GetBasicAttribute().level;
            info.maxhp = o.GetAttribute().life_max;
            info.hp = o.GetAttribute().life;
            info.dir = o.GetDir();

            if (obj.GetType().FullName == "MapServer.AnShaXieLongObject")
            {
                info.param = (int)(obj as AnShaXieLongObject).mPlay.GetTypeId();
            }
            this.SendData(info.GetBuffer());

            //加到怪物可视列表
            if (!obj.GetVisibleList().ContainsKey(this.GetGameID()))
            {
                //RefreshObject refobj = new RefreshObject();
                //refobj.obj = this;
                //obj.GetVisibleList()[this.GetGameID()] = refobj;
                obj.AddVisibleObject(this, false);
            }
            //自己的可视列表 //人物不走动的情况下,怪物进入了视野范围
            if (!this.GetVisibleList().ContainsKey(obj.GetGameID()))
            {
                //RefreshObject refobj = new RefreshObject();
                //refobj.bRefreshTag = true;
                //refobj.obj = obj;
                //this.GetVisibleList()[this.GetGameID()] = refobj;
                this.AddVisibleObject(obj, true);
            }
        }
        public void ProcessNetMsg(ushort tag, byte[] netdata)
        {

            PlayerObject play = this;
            // Log.Instance().WriteLog("协议号:" + tag.ToString());
            // Log.Instance().WriteLog("data:"+GamePacketKeyEx.byteToText(netdata));
            switch (tag)
            {
                case PacketProtoco.C_MOVE:
                    {
                        
                        //被锁定之后无法进行操作
                        NetMsg.MsgMoveInfo moveinfo = new NetMsg.MsgMoveInfo();
                        moveinfo.Create(netdata, session.GetGamePackKeyEx());
                        byte[] movebuf = null;
                        //角色移动
                        if (moveinfo.id == this.GetTypeId())
                        {
                            if (this.IsLock()) return;
                            //摆摊状态
                            if (this.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) != null) return;
                            if (this.Move(moveinfo))
                            {
                                moveinfo.id = this.GetTypeId();
                                moveinfo.x = this.GetCurrentX();
                                moveinfo.y = this.GetCurrentY();
                                moveinfo.dir = this.GetDir();
                                movebuf = moveinfo.GetBuffer();
                                //发给自己
                                this.SendData(movebuf);
                           
                            }
                            break;
                        }
                        //幻兽移动
                        play.GetEudemonSystem().Move(moveinfo);

                        //正在跳舞中- 就设置停止跳舞标记
                        if (this.IsDancing())
                        {
                            this.SetDancing(0);
                        }
                        // this.GetGameMap().BroadcastMove(play, movebuf);
                        break;
                    }
                case PacketProtoco.C_OPENNPC: //单击npc
                    {
                        PackIn inpack = new PackIn(netdata);
                        inpack.ReadUInt16();
                        uint npcid = inpack.ReadUInt32();
                        ScripteManager.Instance().ExecuteActionForNpc(npcid, this);
                        break;
                    }
                case PacketProtoco.C_NPCREPLY: //点击npc选项
                    {

                        PackIn inpack = new PackIn(netdata);
                        inpack.ReadUInt16();
                        inpack.ReadUInt32();
                        inpack.ReadUInt16();
                        byte selectindex = inpack.ReadByte();
                        inpack.ReadInt16();
                        String szStr = inpack.ReadString();
                        if (selectindex == 255) //退出该npc对话脚本
                        {
                            this.SetTaskID(0);
                            break;
                        }
                        if ((selectindex > 0))
                        {
                            ScripteManager.Instance().ExecuteOptionId(selectindex, this, szStr);

                        }
                        else if (this.GetTaskID() != 0) //回调-
                        {
                            ScripteManager.Instance().ExecuteOptionId(play.GetTaskID(), this, szStr);
                            this.SetTaskID(0);
                        }
                        break;
                    }
                case PacketProtoco.C_ATTACK:        //攻击
                    {
                        //检测是否是安全区- 防作弊
                        if (this.GetGameMap().IsSafeArea(this.GetCurrentX(), this.GetCurrentY()))
                        {
                            this.LeftNotice("该区域内禁止PK！");
                            break;
                        }
                        NetMsg.MsgAttackInfo info = new NetMsg.MsgAttackInfo();
                        info.Create(netdata, this.GetGamePackKeyEx());
                        if (info.tag == 21)//魔法攻击 要解密
                        {
                            uint nData = ((info.usPosX) ^ (info.roleId) ^ 0x2ED6);
                            info.usPosX = (ushort)(0xFFFF & (BaseFunc.ExchangeShortBits(nData, 16 - 1) + 0xDD12));
                            nData = ((info.usPosY) ^ (info.roleId) ^ 0xB99B);
                            info.usPosY = (ushort)(0xFFFF & (BaseFunc.ExchangeShortBits(nData, 16 - 5) + 0x76DE));
                            info.idTarget = (BaseFunc.ExchangeLongBits((info.idTarget), 13) ^ (info.roleId) ^ 0x5F2D2463) + 0x8B90B51A;
                            info.usType = 0xFFFF & (BaseFunc.ExchangeShortBits(((info.usType) ^ (info.roleId) ^ 0x915D), 16 - 3) + 0x14BE);
                        }
                        
                        if (info.roleId == this.GetTypeId()) //角色攻击
                        {
                            //被锁定之后无法进行操作
                            this.GetFightSystem().SetFighting();
                            //摆摊状态
                            if (this.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_PTICH) != null) return;
                            if (this.IsLock() || this.IsDie()) return;
                            //潜行状态就移除
                            if (this.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_STEALTH) != null)
                            {
                                this.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_STEALTH);
                            }
                            switch (info.tag)
                            {
                                case 2:     //普通攻击
                                    {
                                        // this.Attack(info);
                                        GetFightSystem().Attack(info);
                                        break;
                                    }
                                case 21:     //魔法攻击
                                    {

                                        //this.MagicAttack(info); 
                                        GetFightSystem().MagicAttack(info);
                                        break;
                                    }
                            }
                            //正在跳舞中- 就设置停止跳舞标记
                            if (this.IsDancing())
                            {
                                this.SetDancing(0);
                            }
                            break;
                        }
                        //幻兽攻击
                        this.GetEudemonSystem().Eudemon_Attack(info);
                        break;
                    }
                case PacketProtoco.C_MSGIEM:    //道具操作信息
                    {
                        //被锁定后是无法使用道具的
                        if (this.IsLock()) break;
                        NetMsg.MsgOperateItem info = new NetMsg.MsgOperateItem();
                        info.Create(netdata);
                        switch (info.usAction)
                        {
                            //case NetMsg.MsgOperateItem.ITEMACT_EQUIP:   //穿戴装备
                            //    {
                            //        this.GetItemSystem().Equip(info.id, info.dwData);
                            //        break;
                            //    }
                            case NetMsg.MsgOperateItem.ITEMACT_BUY: //购买道具
                                {
                                    //魔石商店 1207为魔石商店的id
                                    switch (info.id)
                                    {
                                        case GameBase.Config.Define.GMAESHOPID: //魔石商店
                                            {
                                                this.GetItemSystem().BuyGameShopItem(info.dwData, info.amount);
                                                break;
                                            }
                                        case GameBase.Config.Define.LOOKFACEID: //头像商店
                                            {
                                                this.GetItemSystem().ChangeLookFace(info.dwData);
                                                break;
                                            }
                                        case GameBase.Config.Define.HAIRID:     //发型商店
                                            {
                                                this.GetItemSystem().ChangeHair(info.dwData);
                                                break;
                                            }

                                        default:
                                            {
                                                //普通的npc商店
                                                this.GetItemSystem().BuyItem(info.id, info.dwData);
                                                break;
                                            }
                                    }


                                    break;
                                }
                            case NetMsg.MsgOperateItem.ITEMACT_SELL: //卖出道具
                                {
                                    this.GetItemSystem().SellItem(info.id, info.dwData);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.ITEMACT_UNEQUIP: //卸下装备
                                {
                                    this.GetItemSystem().UnEquip(info.id, info.dwData);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.ITEMACT_USE: //使用道具
                                {
                                    //
                                    this.GetItemSystem().UseItem(info.id, info.dwData, (short)info.amount, (short)info.param1);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.ITEMACT_REPAIREQUIP:
                                {
                                    this.GetItemSystem().RepairEquip(info.param1, info.id);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.ITEMACT_DROP:    //丢弃道具,从道具栏
                                {
                                    this.GetItemSystem().DropItemBag(info.id);
                                    break;
                                }
                    
                            case NetMsg.MsgOperateItem.ITEMACT_OPENGEM: //装备打洞
                                {
                                    //Log.Instance().WriteLog("装备打洞:" + play.GetName() + " " + GamePacketKeyEx.byteToText(netdata));
                                    EquipOperation.Instance().OpenGem(this, info.id, (uint)BaseFunc.MakeLong(info.amount,info.param1));
                                    break;
                                }
                            case NetMsg.MsgOperateItem.STRONGACT_SAVEMONEY: //仓库存钱
                                {
                                    this.GetItemSystem().SaveStrongMoney((int)info.dwData);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.STRONGACT_GIVEMONEY: //仓库取钱
                                {
                                    this.GetItemSystem().GiveStrongMoney((int)info.dwData);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.EUDEMONACT_RECALL: //幻兽召回
                                {
                                    this.GetEudemonSystem().Eudemon_ReCall(info.id);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.EUDEMONACT_FIT://幻兽合体
                                {
                                    this.GetEudemonSystem().Eudemon_Fit(info.id);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.EUDEMONACT_BREAK_UP: //幻兽解体
                                {
                                    this.GetEudemonSystem().Eudemon_BreakUp(info.id);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.EUDEMON_EVOLUTION:  //幻兽进化
                                {
                                    this.GetEudemonSystem().Eudemon_Evolution(info.id);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.EUDEMON_DELETE_MAGIC: //删除幻兽技能
                                {
                                    this.GetEudemonSystem().Eudemon_DeleteMagic(info.id, (ushort)info.amount);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.TAKEMOUNT: //骑乘
                                {
                                    //如果幻兽出征了，先召回
                                    EudemonObject obj = this.GetEudemonSystem().GetEudmeonObject(info.id);
                                    if (obj == null) break;
                                    GameStruct.RoleItemInfo item = this.GetItemSystem().FindItem(obj.GetEudemonInfo().itemid);
                                    if (item == null) break;
                                    this.TakeMount(obj.GetTypeId(), item.itemid);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.TAKEOFFMOUNT: //下马
                                {
                                    this.TakeOffMount(info.id);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.GET_EXPBALL_EXP: //获取经验球的等级
                                {
                                    int id = BaseFunc.MakeLong(info.amount, info.param1);
                                    if (id == this.GetTypeId())
                                    {
                                        this.LeftNotice("经验球不允许给人物使用");
                                        break;
                                    }
                                  //  收到网络协议:长度：28协议号:1009
                                    //{28,0,241,3,122,243,157,131,32,147,105,18,50,0,0,0,122,243,157,131,0,0,0,0,0,0,0,0}
                                    break;
                                }
                            case NetMsg.MsgOperateItem.USE_EXPBALL_EXP: //使用经验球
                                {
                                    int id = BaseFunc.MakeLong(info.amount, info.param1);
                                    if (id == this.GetTypeId())
                                    {
                                        this.LeftNotice("经验球不允许给人物使用");
                                        break;
                                    }
                                    //给幻兽使用
                                    RoleData_Eudemon eudemon_info = this.GetEudemonSystem().FindEudemon((uint)id);
                                    EudemonObject _obj = this.GetEudemonSystem().GetEudmeonObject((uint)id);
                                    if (_obj == null || eudemon_info == null) break;
                                    eudemon_info.level = 0;
                                  //  _obj.GetAttr().level = 0;
                                    _obj.ChangeAttribute(EudemonAttribute.Level, this.GetBaseAttr().level + GameBase.Config.Define.EXPBALL_EUDEMON_MAXLEVEL, false);
                                    //删除经验球
                                    this.GetItemSystem().DeleteItemByID(info.id);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.PTICH_SELL_ITEM_GOLD: //摊位以金币方式出售道具
                                {
                                    PtichManager.Instance().SellItem(this, info.id, (byte)NetMsg.MsgOperateItem.PTICH_SELL_ITEM_GOLD, (int)info.dwData);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.PTICH_SELL_ITEM_GAMEGOLD: //摊位以魔石方式出售道具
                                {
                                    PtichManager.Instance().SellItem(this, info.id, (byte)NetMsg.MsgOperateItem.PTICH_SELL_ITEM_GAMEGOLD, (int)info.dwData);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.PTICH_GETBACK_SELLITEM: //摊位取回道具
                                {
                                    PtichManager.Instance().GetBackItem(this, info.id);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.PTICH_BUY_ITEM: //购买摊位道具
                                {
                                    PtichManager.Instance().BuyItem(this, info.dwData, info.id);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.GET_REMOTE_PTICH://获取远程摊位
                                {
                                    PtichManager.Instance().GetRemotePtich(this);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.BUY_REMOTE_PTICH_ITEM:   //购买远程摊位道具
                                {
                                   
                                    PtichManager.Instance().BuyRemotePtichItem(this,info.id);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.GET_REMOTE_PTICH_ID: //获取远程摊位- 从指定摊位号
                                {
                                    PtichManager.Instance().GetRemotePtich(this,(int) (info.dwData - 1)/*客户端传上来的是以1为下标*/);
                                    break;
                                }
                            case NetMsg.MsgOperateItem.EUDEMON_FOOD: //幻兽喂食圣兽魔晶
                                {
                                    if (this.GetMoneyCount(MONEYTYPE.GAMEGOLD) < 19) return;
                                    uint eudemon_id = (uint)BaseFunc.MakeLong(info.amount, info.param1);
                                    EudemonObject eudemon = this.GetEudemonSystem().GetBattleEudemon(eudemon_id);
                                   
                                    if (eudemon == null ) break;
                                    //幻兽等级不能超过角色九级以上
                                    if (eudemon.GetAttr().level + 50 > this.GetBaseAttr().level + 8)
                                    {
                                        eudemon.ChangeAttribute(EudemonAttribute.Level, this.GetBaseAttr().level + 8 - eudemon.GetAttr().level);
                                    }
                                    //最高255级
                                    else if (eudemon.GetAttr().level + 50 > 255)
                                    {
                                        eudemon.ChangeAttribute(EudemonAttribute.Level, 255 - eudemon.GetAttr().level);
                                  
                                    }
                                    else eudemon.ChangeAttribute(EudemonAttribute.Level, 50);
                                    eudemon.CalcAttribute();
                                    this.ChangeMoney(MONEYTYPE.GAMEGOLD, -19);
                                    break;
                                }
                            default: { 
                                //Log.Instance().WriteLog("未知操作封包:" + play.GetName() + " " + GamePacketKeyEx.byteToText(netdata)); 
                                break; 
                            }
                        }

                        break;
                    }
                case PacketProtoco.C_MSGTALK:
                    {
                        NetMsg.MsgTalkInfo talkinfo = new NetMsg.MsgTalkInfo();
                        talkinfo.Create(netdata);
                        String talk = talkinfo.GetTalkText();
                        if (talk.Length > 0)
                        {
                            if (talk[0] == '\\' && this.IsGM())
                            {
                                //GM命令--
                                GMCommand.ExecuteGMCommand(talk, this);
                            }
                            if (talk[0] == '\\') //普通命令-- 
                            {
                                GMCommand.ExecuteNormalCommand(talk, this);
                            }
                        }

                        PacketOut outpack;
                        switch (talkinfo.unTxtAttribute)
                        {
                            case NetMsg.MsgTalkInfo._TXTATR_TALK: //公聊
                                {
                                    outpack = new PacketOut();
                                    outpack.WriteUInt16((ushort)(netdata.Length + 2));
                                    outpack.WriteBuff(netdata);
                                    this.BroadcastBuffer(outpack.Flush(), false);
                                    break;
                                }
                            case NetMsg.MsgTalkInfo._TXTATR_PRIVATE: //私聊
                                {
                                    String targetname = talkinfo.GetTalkTargetText();
                                    PlayerObject targetobj = UserEngine.Instance().FindPlayerObjectToName(targetname);
                                    if (targetobj != null)
                                    {
                                        outpack = new PacketOut(targetobj.GetGamePackKeyEx());
                                        outpack.WriteUInt16((ushort)(netdata.Length + 2));
                                        outpack.WriteBuff(netdata);
                                        targetobj.SendData(outpack.Flush());
                                    }
                                    break;
                                }
                            case NetMsg.MsgTalkInfo._TXTATR_REJECT: //驳回
                                {
                                    String targetname = talkinfo.GetTalkTargetText();
                                    PlayerObject targetobj = UserEngine.Instance().FindPlayerObjectToName(targetname);
                                    if (targetobj != null)
                                    {
                                        switch (talkinfo.GetTalkText())
                                        {
                                            case "a": //好友请求驳回
                                                {
                                                    break;
                                                }
                                            case "b": //交易请求驳回
                                                {
                                                    this.GetTradSystem().SetTradTarget(0);
                                                    targetobj.GetTradSystem().SetTradTarget(0);
                                                    targetobj.LeftNotice("对方拒绝你的交易请求");
                                                    break;
                                                }
                                            case "c": //队伍请求 
                                                {
                                                    break;
                                                }
                                        }
                                    }
                                    break;
                                }
                        }


                        break;
                    }
                case PacketProtoco.C_PICKDROPITEM: //拾取道具
                    {
                        NetMsg.MsgDropItem item = new NetMsg.MsgDropItem();
                        item.Create(netdata, null);
                        BaseObject obj = GetGameMap().GetObject(item.id);
                        if (obj == null) break;
                        if (this.IsDie() || this.IsLock()) break;//死亡或者被锁定
                        if (this.GetCurrentX() != obj.GetCurrentX() || this.GetCurrentY() != obj.GetCurrentY())
                        {
                            //连击技能或者xp技能没校验过坐标。。 就校验一下坐标 2015.11.21
                            //该道具暂时不可拾取-
                            this.ScroolRandom(this.GetCurrentX(), this.GetCurrentY());
                           
                        }
                        if ((obj as DropItemObject).IsOwner() && (obj as DropItemObject).GetOwnerId() != this.GetTypeId())
                        {
                            this.LeftNotice("该道具暂时无法拾取！");
                            return;
                        }
  
                        //在地图
                        GameStruct.RoleItemInfo roleitem = (obj as DropItemObject).GetRoleItemInfo();
                        (obj as DropItemObject).BroadcastInfo(2);
                        this.GetGameMap().RemoveObj(obj);
                        //捡起来放到背包
                        if (roleitem == null) //怪物爆的
                        {
                            this.GetItemSystem().AwardItem(obj.GetTypeId(), NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK);
                        }
                        else
                        {
                            //玩家丢的
                            //如果是幻兽蛋，就得有幻兽数据
                            if (roleitem.postion == NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK)
                            {
                                this.GetEudemonSystem().AddTempEudemon((obj as DropItemObject).GetRoleEudemonInfo());
                            }
                            this.GetItemSystem().AwardItem(roleitem);

                        }
                      
                        break;
                    }
                case PacketProtoco.C_CHANGEPKMODE: //更改pk模式
                    {
                        bool isrecall = false;
                        NetMsg.MsgChangePkMode action = new NetMsg.MsgChangePkMode();
                        action.Create(netdata, GetGamePackKeyEx());
                        switch (action.tag)
                        {
                            case NetMsg.MsgAction.TYPE_CHANGEPKMODE:
                                {
                                    if (action.value >= 0 && action.value <= 5)
                                    {
                                        this.SetPkMode((byte)action.value);
                                        isrecall = true;

                                    }
                                    break;
                                }
                            //xp技能单击
                            case NetMsg.MsgAction.TYPE_XPFULL:
                                {
                                    GetTimerSystem().XPFull((short)action.value);
                                    //测试--
                                    //PacketOut outpack = new PacketOut(this.GetGamePackKeyEx());
                                    //outpack.WriteUInt16(28);
                                    //outpack.WriteUInt16(1017);
                                    //outpack.WriteUInt32(this.GetTypeId());
                                    //outpack.WriteUInt32(2);
                                    //outpack.WriteInt32(36);
                                    //outpack.WriteInt32(64);
                                    //outpack.WriteInt32(71);
                                    //outpack.WriteInt32(2048);
                                    //byte[] data = outpack.Flush();
                                    //this.SendData(data);

                                    break;
                                }
                            //幻兽出征
                            case NetMsg.MsgAction.TYPE_EUDEMON_BATTLE:
                                {
                                    //value = id;
                                    this.GetEudemonSystem().Eudemon_Battle((uint)action.value);
                                    break;
                                }
                            //表情动作
                            case NetMsg.MsgAction.TYPE_FACEACTION:
                                {
                                    if (this.IsLock()) break;

                                    uint nAction = BitConverter.ToUInt32(netdata, 18);
                                    this.SetCurrentAction(nAction);
                                    action.SetKey(null);
                                    this.BroadcastBuffer(action.GetBuffer());
                                    //迷心术
                                    if (this.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_MIXINSHU) != null)
                                    {
                                        foreach (RefreshObject refobj in this.GetVisibleList().Values)
                                        {
                                            BaseObject obj = refobj.obj;
                                            if (obj.type == OBJECTTYPE.PLAYER)
                                            {
                                                (obj as PlayerObject).PlayAction(nAction);
                                            }
                                        }
                                    }
                                    break;
                                }
                            //获取角色详细信息
                            case NetMsg.MsgAction.TYPE_FRIENDINFO:
                                {
                                    this.GetFriendSystem().GetFriendInfo(action.value);

                                    break;
                                }
                            //复活角色
                            case NetMsg.MsgAction.TYPE_ALIVE:
                                {
                                    //要是鬼魂状态 并且死亡超过等于二十秒
                                    if (IsGhost() && System.Environment.TickCount - mnGhostTick >= GameBase.Config.Define.ALIVE_TIME)
                                    {

                                        Alive();

                                    }

                                    break;
                                }
                            case NetMsg.MsgAction.TYPE_FLY_DOWN:    //雷霆万钧 下降
                                {
                                    play.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_FLY);
                                    play.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_XPFULL_ATTACK);
                                    if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_HEILONGWU) != null)
                                    {
                                        play.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_HEILONGWU);
                                    }
                                    break;

                                }
                            case NetMsg.MsgAction.TYPE_EUDEMON_RANK: //查看幻兽排行榜
                                {
                                    //data1 从总星排行开始 超出100名为0
                                    byte[] data1 = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                                    //{36,0,102,4,114,189,89,134,39,31,97,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,83,85,0,0,0,0}
                                    PacketOut outpack = new PacketOut();
                                    outpack.WriteInt16(36);
                                    outpack.WriteInt16(1126);
                                    outpack.WriteUInt32((uint)action.value);
                                    outpack.WriteUInt32(this.GetTypeId());
                                    outpack.WriteInt16(0);
                                    outpack.WriteBuff(data1);
                                    this.SendData(outpack.Flush(), true);
                                    break;
                                }
                            case NetMsg.MsgAction.TYPE_EUDEMON_SOULINFO: //主幻兽增加分数信息
                                {

                                    EudemonObject obj = this.GetEudemonSystem().GetEudmeonObject((uint)action.time);
                                    if (obj == null) break;
                                    this.GetEudemonSystem().SetSoulEudemon((uint)action.time);
                                    GameStruct.EudemonSoulInfo soulinfo = ConfigManager.Instance().GetEudemonSoulInfo((int)(obj.GetEudemonInfo().quality / 100));
                                    if (soulinfo == null)
                                    {
                                        this.MsgBox("获取幻化信息错误!!");
                                        Log.Instance().WriteLog("获取幻化信息错误:名称:" + play.GetName() + " 品质:" + obj.GetEudemonInfo().quality.ToString()
                                            + "道具id:" + obj.GetEudemonInfo().itemid.ToString());
                                        break;
                                    }
                                    //125为等级需求
                                  //  byte[] data1 = { 32, 0, 244, 7, 73, 0, 6, 0, 125, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0/*, 9, 0, 0, 0, 1, 0, 0, 0*/ };
                                    PacketOut outpack = new PacketOut();
                                    outpack.WriteInt16(32);
                                    outpack.WriteInt16(2036);
                                    outpack.WriteInt16(73);
                                    outpack.WriteInt16(6);
                                    outpack.WriteInt32(soulinfo.level);
                                    outpack.WriteInt32(soulinfo.fu_star);
                                    outpack.WriteInt32(0);//亲密度
                                    outpack.WriteInt32(0);
                                    outpack.WriteInt32(0);
                                  //  outpack.WriteBuff(data1);
                                    //这个问成长率总分
                                    outpack.WriteInt32(obj.GetEudemonInfo().quality);
                                   // outpack.WriteInt32(9);
                                    outpack.WriteInt32(1);
                                    play.SendData(outpack.Flush(), true);
                                    break;
                                }
                            case NetMsg.MsgAction.TYPE_EUDEMON_SOUL: //幻兽幻化
                                {
                                    this.GetEudemonSystem().Eudemon_Soul((uint)action.value);
                                    break;
                                }
                            case NetMsg.MsgAction.TYPE_LOOKROLEINFO: //查看装备
                                {
                                    PlayerObject obj = UserEngine.Instance().FindPlayerObjectToTypeID((uint)action.value);
                                    if (obj == null) break;
                                
                                    obj.GetItemSystem().SendLookRoleInfo(this);
                                    break;
                                }
                            case NetMsg.MsgAction.TYPE_LOOKEUDEMONINFO: //查看幻兽
                                {
                                    PlayerObject obj = UserEngine.Instance().FindPlayerObjectToTypeID((uint)action.value);
                                    if (obj == null) break;

                                    obj.GetEudemonSystem().SendLookEudemonInfo(this);
                                    break;
                                }
                            case NetMsg.MsgAction.TYPE_FINDPATH: //单击地图自动寻路
                                {
                                    short x = BaseFunc.LoWord(action.type);
                                    short y = BaseFunc.HiWord(action.type);
                                    if (!this.GetGameMap().CanMove(x, y)) break;

                                    //{28,0,242,3,1,0,0,0,73,48,96,5,195,1,159,1,0,0,0,0,232,3,0,0,166,38,0,0}
                                    PacketOut outpack = new PacketOut();
                                    outpack.WriteInt16(28);
                                    outpack.WriteInt16(1010);
                                    outpack.WriteInt32(1);
                                    outpack.WriteUInt32(this.GetTypeId());
                                    outpack.WriteInt16(x);
                                    outpack.WriteInt16(y);
                                    outpack.WriteInt32(0);
                                    outpack.WriteUInt32(this.GetGameMap().GetMapInfo().id);
                                    outpack.WriteInt32(9894);
                                    this.SendData(outpack.Flush(), true);
                                    break;
                                }
                            case NetMsg.MsgAction.TYPE_CONTINUEGAME:    //继续游戏
                                {
                                   
                                    PacketOut outpack = new PacketOut();
                                    outpack.WriteInt16(28);
                                    outpack.WriteInt16(1010);
                                    outpack.WriteInt32(System.Environment.TickCount);
                                    outpack.WriteUInt32(this.GetTypeId());
                                    outpack.WriteInt32(0);
                                    outpack.WriteInt32(0);
                                    outpack.WriteInt32(0);
                                    outpack.WriteInt32(9630);
                                    this.SendData(outpack.Flush(), true);
                                    break;
                                }
                            case NetMsg.MsgAction.TYPE_PTICH: //摆摊有关
                                {
                                    if (this.GetCurrentPtichID() == -1) break;
                                    PtichManager.Instance().AddPlayPtich(this.GetCurrentPtichID(), this);
                                    break;
                                }
                            case  NetMsg.MsgAction.TYPE_SHUT_PTICH: //收摊
                                {
                               
                                    PtichManager.Instance().ShutPtich(this,true);
           
                                    break;
                                }
                            case 9528:
                                {
                                    //{242,3,180,167,114,0,174,66,15,0,70,1,211,1,0,0,0,0,0,0,0,0,56,37,0,0,}
                                 //   Log.Instance().WriteLog(GamePacketKeyEx.byteToText(netdata));
                                    break;
                                }
                            case 9855: //开启远程浏览
                                {
                                    break;
                                }
                            case NetMsg.MsgAction.TYPE_LOOK_PTICH: //查看摊位
                                {
                                    uint ptich_obj_id = (uint)action.time;
                                    PtichManager.Instance().LookPtich(this, ptich_obj_id);
                                    //Log.Instance().WriteLog(GamePacketKeyEx.byteToText(netdata));
                                    break;
                                }
                        }
                        //是否回发-
                        if (isrecall)
                        {
                            this.SendData(action.GetBuffer());
                        }
                        break; ;
                    }
                //保存热键信息
                case PacketProtoco.C_HOTKEY:
                    {
                        // Log.Instance().WriteLog("data:" + GamePacketKeyEx.byteToText(netdata));
                        NetMsg.MsgHotKey hotkey = new NetMsg.MsgHotKey();
                        hotkey.Create(netdata, GetGamePackKeyEx());
                        switch (hotkey.tag)
                        {
                            case NetMsg.MsgHotKey.TAG_SAVEHOTKEY: //保存热键
                                {
                                    String[] hotkeyarr = hotkey.GetHotKeyArr();
                                    if (hotkeyarr != null)
                                    {
                                        this.ClearHotKey(hotkey.GetGroup());
                                        for (int i = 0; i < hotkeyarr.Length; i++)
                                        {
                                            GameStruct.HotkeyInfo info = new GameStruct.HotkeyInfo(hotkey.GetGroup(), hotkeyarr[i]);
                                            if (info.index == 0 && info.id == 0) continue;
                                            this.AddHotKeyInfo(info);
                                        }
                                    }
                                    break;
                                }
                            case NetMsg.MsgHotKey.TAG_WANGLING_STATE: //切换亡灵形态
                                {
                                    if (this.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_HUASHENWANGLING) != null)
                                    {
                                        this.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_HUASHENWANGLING);
                                    }
                                    else
                                    {
                                        this.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_HUASHENWANGLING);
                                    }

                                    break;
                                }
                            case NetMsg.MsgHotKey.WORLD_CHAT: //魔法飞鸽
                                {
                                    //this.MsgBox("游戏当前禁止发送飞鸽!");
                                    //break;
                                   // hotkey.str;
                                    int Index = WorldPigeon.Instance().AddText(this.GetName(), this.GetTypeId(), hotkey.str);
                                    if (Index > 1)
                                    {
                                        this.MsgBox("发布飞鸽成功,目前排在:" + Index.ToString() + "位");
                                    }
                                    else if (Index == -1)
                                    {
                                        this.MsgBox("你已经发过一次飞鸽了，请等上一次的飞鸽发完。");
                                    }
                                   
                                   
                                    break;
                                }
                            case NetMsg.MsgHotKey.CHANGE_EUDEMON_NAME:  //更改幻兽名字
                                {
                                   // hotkey.type 幻兽id
                                  //  hotkey.str
                                    if (hotkey.str.Length > 8) return;
                                    EudemonObject obj = this.GetEudemonSystem().GetEudmeonObject((uint)hotkey.type);
                                    if (obj == null) return;
                                    GameStruct.RoleItemInfo role_item = this.GetItemSystem().FindItem(obj.GetEudemonInfo().itemid);
                                    if (role_item == null) return;
                                    role_item.forgename = hotkey.str;
                                   // {21,0,247,3,114,189,89,134,24,0,1,8,210,187,184,246,201,181,177,198,0}
                                    PacketOut outpack = new PacketOut();
                                    outpack.WriteInt16((short)(11 + Coding.GetDefauleCoding().GetBytes(role_item.forgename).Length + 2));
                                    outpack.WriteInt16(1015);
                                    outpack.WriteUInt32((uint)hotkey.type);
                                    outpack.WriteInt16(24);
                                    outpack.WriteByte(1);
                                    outpack.WriteString(role_item.forgename);
                                    outpack.WriteByte(0);
                                    this.BroadcastBuffer(outpack.Flush(), true);

                                    this.GetItemSystem().UpdateItemInfo(role_item.id);
                                    this.GetEudemonSystem().SendEudemonInfo(obj.GetEudemonInfo());
                                 
                                    //出战的时候也要同步改名
                                    if (this.GetEudemonSystem().GetBattleEudemon((uint)hotkey.type) != null)
                                    {
                                        obj.SendEudemonInfo();
                                    }
                                    break;
                                }
                            case 288: //进入/离开名人堂
                                {
                                    this.MsgBox("名人堂下一个版本开放！");
                                    break;
                                }
                        }


                        break;
                    }
                case PacketProtoco.C_EQUIPOPERATION:
                    {
                        NetMsg.MsgEquipOperation equip = new NetMsg.MsgEquipOperation();
                        equip.Create(netdata, null);
                      //  Log.Instance().WriteLog(GamePacketKeyEx.byteToText(netdata));
                        switch (equip.type)
                        {
                            //提升魔魂等级
                            case NetMsg.MsgEquipOperation.EQUIPSTRONG:
                            case NetMsg.MsgEquipOperation.EQUIPSTRONGEX:
                                {
                                    EquipOperation.Instance().EquipStrong(this, equip.itemid, equip.materialid);
                                    break;
                                }
                            //提升幻魔等级
                            case NetMsg.MsgEquipOperation.EQUIPLEVEL:
                                {
                                    EquipOperation.Instance().EquipLevel(this, equip.itemid, equip.materialid);
                                    break;
                                }
                            //提升装备品质
                            case NetMsg.MsgEquipOperation.EQUIPQUALITY:
                                {
                                    EquipOperation.Instance().EquipQuality(this, equip.itemid, equip.materialid);
                                    break;
                                }
                            //宝石镶嵌
                            case NetMsg.MsgEquipOperation.GEMSET:
                                {
                                    byte index = 0;//第几个洞
                                    uint gemid = 0;
                                    if (equip.materialid != 0)
                                    {
                                        gemid = equip.materialid;
                                        index = 0;
                                    }
                                    else if (equip.param != 0)
                                    {
                                        gemid = equip.param;
                                        index = 1;
                                    }
                                    else if (equip.param1 != 0)
                                    {
                                        gemid = equip.param1;
                                        index = 2;
                                    }
                                    if (gemid == 0) return;
                                    EquipOperation.Instance().GemSet(this, gemid, equip.itemid, index);
                                    break;
                                }
                            case NetMsg.MsgEquipOperation.MAMIC_ADD_GOD:
                                {
                                    EquipOperation.Instance().Magic_Add_God(this, equip.itemid, equip.materialid);
                                    break;
                                }
                            //宝石融合
                            case NetMsg.MsgEquipOperation.GEMFUSION:
                                {
                                    EquipOperation.Instance().GemFusion(this, equip.itemid);
                                    break;
                                }
                                //宝石替换
                            case NetMsg.MsgEquipOperation.GEMREPLACE:
                                {
                                    EquipOperation.Instance().GemReplace(this, netdata);
                                    break;
                                }
                            case NetMsg.MsgEquipOperation.EQUIP_GODEXP://提升神佑
                                {
                                    EquipOperation.Instance().Equip_GodExp(this,equip.itemid, equip.materialid);
                                    break;
                                }
                            case NetMsg.MsgEquipOperation.GUANJUE_GOLD:
                                {
                                    GuanJueManager.Instance().Donation(this, MONEYTYPE.GOLD, (int)equip.itemid);
                                    break;
                                }
                            case NetMsg.MsgEquipOperation.GUANJUE_GAMEGOLD:
                                {
                                    GuanJueManager.Instance().Donation(this, MONEYTYPE.GAMEGOLD, (int)equip.itemid);
                                    break;
                                }
                            case NetMsg.MsgEquipOperation.EXIT_GAME:
                                {
                                   byte[] data = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,29,0,0,0,0,0,0,0,0,0};
                                    PacketOut outpack = new PacketOut();
                                    outpack.WriteInt16(32);
                                    outpack.WriteInt16(1032);
                                    outpack.WriteInt32(System.Environment.TickCount);
                                    outpack.WriteBuff(data);
                                    this.SendData(outpack.Flush(), true);
                                    break;
                                }
                       
                        }

                        break;
                    }
                case PacketProtoco.C_STRONGPACK:
                    {

                        NetMsg.MsgStrongPack msgstrongpack = new NetMsg.MsgStrongPack();
                        msgstrongpack.Create(netdata);
                     
                        switch (msgstrongpack.param)
                        {
                            case NetMsg.MsgStrongPack.STRONGPACK_TYPE: //仓库操作
                                {
                                    switch (msgstrongpack.type)
                                    {
                                        case NetMsg.MsgStrongPack.STRONGPACK_TYPE_SAVE: //存道具
                                            {
                                                if (this.GetItemSystem().GetStrongItemCount() >= PlayerItem.MAX_STRONGITEM) return; //仓库道具已满，存不了了
                                                this.GetItemSystem().MoveItem(msgstrongpack.itemid, (ushort)NetMsg.MsgItemInfo.ITEMPOSTION_STRONG_PACK);
                                                break;
                                            }
                                        case NetMsg.MsgStrongPack.STRONGPACK_TYPE_GIVE://取道具
                                            {
                                                if (this.GetItemSystem().IsItemFull()) return; //包裹道具已满，存不了了
                                                this.GetItemSystem().MoveItem(msgstrongpack.itemid, (ushort)NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK);
                                                break;
                                            }
                                    }

                                    break;
                                }
                            case NetMsg.MsgStrongPack.CHEST_TYPE:
                                {
                                    switch (msgstrongpack.type)
                                    {
                                        case NetMsg.MsgStrongPack.CHEST_TYPE_GIVE: //取回衣柜时装
                                            {
                                                this.GetItemSystem().Give_FashionChest(msgstrongpack.itemid);
                                                break;
                                            }
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case PacketProtoco.C_GETFRIENDINFO:
                    {
                        //for (int i = 0; i < 10; i++)
                        //{
                        //    NetMsg.MsgFriendInfo info = new NetMsg.MsgFriendInfo();
                        //    info.Create(null, this.GetGamePackKeyEx());
                        //    info.level = 130;
                        //    info.type = 15;
                        //    info.actorid = this.GetTypeId() + 1;
                        //    info.name = "狂拽酷炫屌炸天"+i.ToString();
                        //    info.Online = 1;
                        //    this.SendData(info.GetBuffer());
                        //}


                        //info = new NetMsg.MsgFriendInfo();
                        //info.Create(null, this.GetGamePackKeyEx());
                        //info.level = 130;
                        //info.type = 15;
                        //info.actorid = this.GetTypeId() + 2;
                        //info.name = "离线人物";
                        //info.Online = 0;
                        //this.SendData(info.GetBuffer());
                        //  this.GetFriendSystem().SendAllFriendInfo();
                        break;
                    }
                case PacketProtoco.C_ADDFRIEND:
                    {
                        NetMsg.MsgFriendInfo info = new NetMsg.MsgFriendInfo();
                        info.Create(netdata);
                        switch (info.type)
                        {
                            case NetMsg.MsgFriendInfo.TYPE_ADDFRIEND:
                                {
                                    this.GetFriendSystem().RequestAddFriend(info);
                                    break;
                                }
                            case NetMsg.MsgFriendInfo.TYPE_AGREED: //接收好友请求
                                {
                                    this.GetFriendSystem().AddFriend(info.playerid, NetMsg.MsgFriendInfo.TYPE_FRIEND);
                                    break;
                                }
                            case NetMsg.MsgFriendInfo.TYPE_REFUSE: //拒绝好友请求
                                {
                                    this.GetFriendSystem().RefuseFriend(info.playerid);
                                    break;
                                }
                            case NetMsg.MsgFriendInfo.TYPE_KILL:  //绝交
                                {
                                    this.GetFriendSystem().DeleteFriend(info.playerid);
                                    break;
                                }
                        }

                        break;
                    }
                case PacketProtoco.C_TRAD:
                    {
                        NetMsg.MsgTradInfo info = new NetMsg.MsgTradInfo();
                        info.Create(netdata, null);


                        switch (info.type)
                        {
                            case NetMsg.MsgTradInfo.REQUEST_TRAD:
                                {
                                    this.GetTradSystem().RequstTrad(info);
                                    break;
                                }
                            case NetMsg.MsgTradInfo.QUIT_TRAD:
                                {
                                    this.GetTradSystem().QuitTrad(info);
                                    break;
                                }
                            case NetMsg.MsgTradInfo.GOLD_TRAD:
                                {
                                    this.GetTradSystem().SetTradGold((int)info.typeid);
                                    break;
                                }
                            case NetMsg.MsgTradInfo.GAMEGOLD_TRAD:
                                {
                                    this.GetTradSystem().SetTradGameGold((int)info.typeid);
                                    break;
                                }
                            case NetMsg.MsgTradInfo.ITEM_TRAD:
                                {
                                    this.GetTradSystem().AddTradItem(info.typeid);
                                    break;
                                }
                            case NetMsg.MsgTradInfo.SURE_TRAD:
                                {
                                    this.GetTradSystem().SureTrad();
                                    break;
                                }
                        }
                        break;
                    }
                case PacketProtoco.C_GUANJUE:
                    {
                        PackIn inguanjue = new PackIn(netdata);
                        inguanjue.ReadUInt16();
                        short type = inguanjue.ReadInt16();
                        byte page = inguanjue.ReadByte();
                        if (type == 2) //请求获得爵位数据
                        {
                            GuanJueManager.Instance().RequestData(this, page);
                        }

                        //                        byte[] data = {162,1,12,8,2,0,0,0,0,0,5,0,0,0,0,0,10,0,226,12,16,0,205,168,204,236,177,166,56,0,0,0,0,0,0,0,0,0,0,0,0,0,128,226,104,182,10,0,0,0,1,0,0,0,0,0,0,0,119,109,

                        //15,0,161,254,192,230,194,228,161,254,0,0,0,0,0,0,0,0,0,0,0,0,80,231,196,204,9,0,0,0,1,0,0,0,1,0,0,0,130,72,16,0,185,243,215,229,193,236,208,228,0,0,0,0,0,0

                        //,0,0,0,0,0,0,0,190,229,228,8,0,0,0,1,0,0,0,2,0,0,0,230,90,15,0,126,194,182,45,194,182,45,193,250,126,0,0,0,0,0,0,0,0,0,0,98,3,137,149,7,0,0,0,3,0,0,0,3,0,0

                        //,0,30,145,15,0,126,204,236,200,244,211,208,199,233,126,0,0,0,0,0,0,0,0,0,0,176,56,161,110,6,0,0,0,3,0,0,0,4,0,0,0,236,69,15,0,193,233,202,166,0,0,0,0,0,0,0

                        //,0,0,0,0,0,0,0,0,0,195,147,65,41,5,0,0,0,3,0,0,0,5,0,0,0,179,169,15,0,213,189,198,199,161,162,208,204,204,236,0,0,0,0,0,0,0,0,0,0,184,62,118,7,4,0,0,0,3,0,

                        //0,0,6,0,0,0,213,95,16,0,40,95,136,181,165,235,164,240,213,108,215,237,0,0,0,0,0,0,0,0,0,59,223,155,3,0,0,0,3,0,0,0,7,0,0,0,206,207,15,0,182,192,185,194,161

                        //,162,176,212,204,236,0,0,0,0,0,0,0,0,0,0,128,149,127,73,3,0,0,0,3,0,0,0,8,0,0,0,40,79,15,0,196,234,199,225,190,205,210,170,192,203,0,0,0,0,0,0,0,0,0,0,115,

                        //166,15,67,3,0,0,0,3,0,0,0,9,0,0,0};
                        //                        this.GetGamePackKeyEx().EncodePacket(ref data, data.Length);
                        //                        this.SendData(data);
                        break;
                    }
                case PacketProtoco.C_DANCING: //跳舞
                    {
                        PackIn inpack = new PackIn(netdata);
                        short _param = inpack.ReadInt16();
                        uint role_id = inpack.ReadUInt32();
                        uint target_id = inpack.ReadUInt32();
                        int param1 = inpack.ReadInt32();
                        short magic_id = inpack.ReadInt16();
                        short magic_lv = inpack.ReadInt16();

                        PacketOut outpack = new PacketOut();
                        switch (param1)
                        {
                            case 1280: //请求跳舞
                                {
                                    BaseObject targetobj = play.GetGameMap().FindObjectForID(target_id);
                                    if (targetobj == null)
                                    {
                                        return;
                                    }
                                    //距离太远
                                    if (Math.Abs(this.GetCurrentX() - targetobj.GetCurrentX()) > 2 ||
                                        Math.Abs(this.GetCurrentY() - targetobj.GetCurrentY()) > 2)
                                    {
                                        this.LeftNotice("离对方太远了，再走近点吧。");
                                        return;
                                    }
                                    //     收到网络协议:长度：28协议号:1049
                                    byte[] data1 = { 2, 5, 0, 0, 232, 0, 0, 0, 131, 0, 0, 0, 255, 255, 255, 255 };
                                    outpack.WriteUInt16(28);
                                    outpack.WriteUInt16(1049);
                                    outpack.WriteUInt32(this.GetTypeId());
                                    outpack.WriteUInt32(target_id);
                                    outpack.WriteBuff(data1);
                                    play.BroadcastBuffer(outpack.Flush(), true);

                                    outpack = new PacketOut();

                                    byte[] data2 = { 3, 5, 0, 0, 25, 2, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255 };
                                    outpack.WriteUInt16(28);
                                    outpack.WriteUInt16(1049);
                                    outpack.WriteUInt32(this.GetTypeId());
                                    outpack.WriteUInt32(target_id);
                                    outpack.WriteBuff(data2);
                                    play.BroadcastBuffer(outpack.Flush(), true);
                                    mnDancingId = magic_id;
                                    mnDancingTick = System.Environment.TickCount;
                                    break;
                                }
                            case 1285: //停止跳舞
                                {
                                    if (mnDancingId > 0 && System.Environment.TickCount - mnDancingTick > 2000)
                                    {
                                        byte[] data1 = { 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                                        outpack = new PacketOut();
                                        outpack.WriteUInt16(28);
                                        outpack.WriteUInt16(1049);
                                        outpack.WriteUInt32(this.GetTypeId());
                                        outpack.WriteUInt32(target_id);
                                        outpack.WriteBuff(data1);
                                        play.BroadcastBuffer(outpack.Flush(), true);
                                        mnDancingId = 0;
                                    }

                                    break;
                                }
                        }

                        //Log.Instance().WriteLog(GameBase.Network.GamePacketKeyEx.byteToText(netdata));
                        break;
                    }
                default:
                    {
                        Debug.WriteLine("未知封包,协议号:" + tag.ToString());
                        break;
                    }
            }


        }


        protected override void ProcessAction_Move(GameStruct.Action act)
        {
            byte runvalue = 1;
            if (act.GetObjectCount() > 0)
            {
                runvalue = (byte)act.GetObject(0);
            }

            RefreshVisibleObject();
            if (mVisibleList.Count > 0)
            {
                foreach (RefreshObject obj in mVisibleList.Values)
                {
                    BaseObject o = obj.obj;
                    switch (o.type)
                    {
                        case OBJECTTYPE.NPC:
                            {
                                if (obj.bRefreshTag) break;
                                this.SendNpcInfo(o);
                                obj.bRefreshTag = true;
                                break;
                            }
                        case OBJECTTYPE.MONSTER:
                        case OBJECTTYPE.CALLOBJECT:
                            {
                                if (obj.bRefreshTag) break;
                                this.SendMonsterInfo(o);
                                obj.bRefreshTag = true;
                                break;
                            }
                        case OBJECTTYPE.PLAYER:
                            {

                                this.SendRoleMoveInfo(o, runvalue, obj);
                                break;
                            }
                        case OBJECTTYPE.DROPITEM:
                            {
                                if (obj.bRefreshTag) break;
                                this.SendDropItemInfo(o);
                                obj.bRefreshTag = true;
                                break;
                            }
                        case OBJECTTYPE.EUDEMON:
                            {
                                if (obj.bRefreshTag) break;

                                (o as EudemonObject).SendEudemonInfo(this);
                                obj.bRefreshTag = true;
                                break;
                            }
                        case OBJECTTYPE.ROBOT:
                            {
                                if (obj.bRefreshTag) break;
                                (o as RobotObject).SendRobotInfo(this);
                                obj.bRefreshTag = true;
                                break;
                            }
                        case OBJECTTYPE.GUARDKNIGHT:
                            {
                                if (obj.bRefreshTag) break;
                                (o as GuardKnightObject).SendInfo(this);
                                obj.bRefreshTag = true;
                                break;
                            }
                        case OBJECTTYPE.EFFECT:
                            {
                                if (obj.bRefreshTag) break;
                                (o as EffectObject).SendInfo(this);
                                obj.bRefreshTag = true;
                                break;
                            }
                        case OBJECTTYPE.PTICH:  //摊位
                            {
                                if (obj.bRefreshTag) break;
                                (o as PtichObject).SendInfo(this);
                                obj.bRefreshTag = true;
                                break;
                            }
                    }
                    //加入到对方视野中-
                    o.AddVisibleObject(this);

                }
                //   mRefreshList.Clear();
            }
        }

        protected override void ProcessAction_Die(GameStruct.Action act)
        {
            //变为幽灵
            //TransformGhost();


            //死亡标记
            BaseObject obj = act.GetObject(0) as BaseObject;
            NetMsg.MsgMonsterDieInfo die = new NetMsg.MsgMonsterDieInfo();
            die.monsterid = this.GetTypeId();
            die.roleid = obj.GetTypeId();
            die.role_x = this.GetCurrentX();
            die.role_y = this.GetCurrentY();
            die.tag = 14;
            this.BroadcastBuffer(die.GetBuffer(), true);

            m_bGhost = true;
            mnGhostTick = System.Environment.TickCount;
            //死亡之后自动攻击目标为Null
            this.GetFightSystem().SetAutoAttackTarget(null);

            this.GetPKSystem().Die(obj);
            //亡念巫灵原地复活//要在删除状态之前调用该代码
            if (this.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_WANGNIANWULING) != null)
            {
                this.Alive(true);
            }

            //移除一些死亡后的状态
            this.GetTimerSystem().Die_DeleteState();

            //死亡后幻兽也要召回
            this.GetEudemonSystem().Eudemon_ReCallAll();
            //死亡后骑乘幻兽中就下马
            if (this.GetMountID() > 0)
            {
                this.TakeOffMount(0);
            }
            this.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_DIE);

        }
        protected override void ProcessAction_Attack(GameStruct.Action act)
        {

        }
        protected override void ProcessAction_Injured(GameStruct.Action act)
        {
            BaseObject target = act.GetObject(0) as BaseObject;
            uint value = (uint)act.GetObject(1);


            if (!IsDie())
            {

                //亡灵巫师- 被攻击后如果有召唤巫环状态- 就同时召唤出三个亡灵攻击对方。
                if (this.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_ZHAOHUANWUHUAN) != null &&
                    this.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_HUASHENWANGLING) != null && mZhaoHuanWuHuanObj == null)
                {
                    int nNewX = target.GetCurrentX() - DIR._DELTA_X[this.GetDir()];
                    int nNewY = target.GetCurrentY() - DIR._DELTA_Y[this.GetDir()];
                    MonsterObject Object_CALL = null;
                    uint[] CallObj_ID = { GameBase.Config.Define.DIYUXIEFU_MONSTER_ID, GameBase.Config.Define.SHIHUNWULING_MONSTER_ID, GameBase.Config.Define.SHENYUANELING_MONSTER_ID };
                    for (int i = 0; i < CallObj_ID.Length; i++)
                    {
                        GameStruct.MonsterInfo monster_info = ConfigManager.Instance().GetMonsterInfo(CallObj_ID[i]);
                        if (monster_info != null)
                        {
                            Object_CALL = new DiYuXieFu(this, target, (short)nNewX, (short)nNewY, this.GetDir(), monster_info.id, monster_info.ai);
                            this.GetGameMap().AddObject(Object_CALL, null);
                            Object_CALL.Alive(false);
                        }
                    }
                    SetZhaoHuanWuHuanObj(target);
                }
            }
            mTarget = target;


        }

        public override bool IsDie()
        {
            return this.GetBaseAttr().life == 0 ? true : false;
        }
        //private void AutoAttack()
        //{
        //    return;
        //    //if (GetTargetObj() == null) return;
        //    //if (GetTargetObj().IsDie())
        //    //{
        //    //    SetTargetObj(null);
        //    //    return;
        //    //}
        //    //if (System.Environment.TickCount - lastattacktime > attack_speed)
        //    //{
        //    //    uint injured = 10;
        //    //    //targetObject.Injured(this, injured,2);
        //    //    lastattacktime = System.Environment.TickCount;
        //    //}

        //}

        //进入游戏 参数1: 是否是创建角色第一次进入游戏
        public void EnterGame(GameBase.Network.GameSession _session, bool isFirst = false)
        {
            if (_session != null)
            {
                this.SetGameSession(_session);
            }
            if (this.GetGameSession() == null)
            {
                Log.Instance().WriteLog("玩家进入游戏EnterGame,会话对象为空！！");
                return;
            }
            this.CalcAttribute();//计算属性-
            this.GetGameSession().gameid = this.GetGameID();
            //加入到全局用户列表
            UserEngine.Instance().AddPlayerObject(this);
            PlayerObject pay = this;
            //公告信息
            NetMsg.MsgNotice msgNotice = new NetMsg.MsgNotice();
            msgNotice.Create(null, GetGamePackKeyEx());
            ////玩家个人信息

            // byte[] selfdata = {  238, 3, 64, 66, 15, 0, 17, 152, 2, 0, 101, 0, 0, 0, 125, 61, 2, 0, 0, 0, 0, 0, 133, 133, 207, 231, 1, 0, 0, 0, 14, 166, 0, 0, 0, 0, 0, 0, 231, 29, 0, 0, 180, 1, 0, 0, 31, 0, 100, 0, 74, 2, 0, 0, 0, 0, 46, 25, 46, 25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 112, 20, 0, 1, 1, 5, 0, 5, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 21, 10, 0, 0, 84, 154, 126, 156, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 87, 25, 0, 0, 0, 0, 0, 0, 54, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 127, 4, 0, 0, 91, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 6, 97, 118, 49, 51, 49, 52, 2, 206, 222, 0, 0, 0 };
            // this.GetGamePackKeyEx().EncodePacket(ref selfdata, selfdata.Length);
            // this.SendData(selfdata);
            // 
            NetMsg.MsgSelfRoleInfo rolemsg = new NetMsg.MsgSelfRoleInfo();
            //rolemsg.Create(selfdata, this.GetGamePackKeyEx());
            rolemsg.Create(null, this.GetGamePackKeyEx());
            rolemsg.roleid = this.GetTypeId();
            rolemsg.lookface = this.GetBaseAttr().lookface;

            rolemsg.profession = this.GetBaseAttr().profession;
            rolemsg.name = this.GetName();
            //一登录进满血
            this.GetBaseAttr().life = this.GetBaseAttr().life_max;
            rolemsg.life = (ushort)this.GetBaseAttr().life;
            rolemsg.maxlife = (ushort)this.GetBaseAttr().life_max;
            rolemsg.manna = (ushort)this.GetBaseAttr().mana_max;
            rolemsg.maxpetcall = this.GetBaseAttr().maxeudemon;
            //一登录进满蓝

            rolemsg.level = this.GetBaseAttr().level;
            rolemsg.gold = (uint)this.GetBaseAttr().gold;
            rolemsg.godlevel = this.GetBaseAttr().godlevel;
            rolemsg.gamegold = (uint)this.GetBaseAttr().gamegold;
           // rolemsg.maxpetcall = (ushort)GameBase.Config.Define.MAX_CALL_EUDEMON;//最大召唤宠物数量
            rolemsg.hair = (uint)this.GetBaseAttr().hair;
            //第一次进入游戏
            if (isFirst)
            {
                this.SendData(msgNotice.GetStartGameBuff());
                //个人信息
                //偷懒,一参加角色神等级20级 2016.1.21 封包协议号没分析全的结果
                //2016.2.1 神力等级归0
                rolemsg.godlevel = 0;
                session.SendData(rolemsg.GetBuffer());

                SendRoleOtherSystemInfo();
                ScripteManager.Instance().ExecuteAction(GameBase.Config.Define.FIRSTSCRIPTID, this);
 
                return;
            }



            GameMap map = MapManager.Instance().GetGameMapToID(this.GetBaseAttr().mapid);
            if (map == null)
            {
                Log.Instance().WriteLog("非法玩家,非法坐标.." + this.GetName() + " 地图id:" + this.GetBaseAttr().mapid.ToString()
                    +"已修正到回卡诺萨城");
                //重新回到卡诺萨城
                this.GetBaseAttr().mapid = 1000;
                this.SetPoint(145, 413);
                map = MapManager.Instance().GetGameMapToID(this.GetBaseAttr().mapid);
               // return;
            }

          
            MapManager.Instance().GetGameMapToID(this.GetBaseAttr().mapid).AddObject(this, GetGameSession());
            //发送爵位公告
            this.SendJueweiNotice();
            ////加入到正式列表
            //公告信息
            session.SendData(msgNotice.GetStartGameBuff());
            // session.GetGamePackKeyEx().EncodePacket(ref data, data.Length);
            session.SendData(rolemsg.GetBuffer());
            //session.SendData(data);
            SendRoleOtherSystemInfo();

            //测试增加装备 测试装备
            //NetMsg.MsgItemInfo item = new NetMsg.MsgItemInfo();
            //item.Create(null, session.GetGamePackKeyEx());
            //item.postion = NetMsg.MsgItemInfo.ITEMPOSITION_ARMOR;
            //item.id = 112434;
            //item.item_id = 135114;
            //item.amount = item.amount_limit = 1;
            //session.SendData(item.GetBuffer());
            //测试武器
            //item = new NetMsg.MsgItemInfo();
            //item.Create(null, session.GetGamePackKeyEx());
            //item.postion = NetMsg.MsgItemInfo.ITEMPOSITION_WEAPONR;
            //item.id = 112435;
            //item.item_id = 440244;
            //item.amount = item.amount_limit = 1;
            //session.SendData(item.GetBuffer());

            //item = new NetMsg.MsgItemInfo();
            //item.Create(null, session.GetGamePackKeyEx());
            //item.postion = NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK;
            //item.id = 112436;
            //item.item_id = 440244;
            //item.amount = item.amount_limit = 1;
            //session.SendData(item.GetBuffer());

            //NetMsg.MsgUpdateSP sp = new NetMsg.MsgUpdateSP();
            //sp.Create(null, session.GetGamePackKeyEx());
            //sp.role_id = this.GetTypeId();
            //sp.value = 37;
            //sp.sp = 100;
            //session.SendData(sp.GetBuffer());

            //sp = new NetMsg.MsgUpdateSP();
            //sp.Create(null, session.GetGamePackKeyEx());
            //sp.role_id = this.GetTypeId();
            //session.SendData(sp.GetBuffer());


            //进入地图
            NetMsg.MsgMapInfo mapinfo = new NetMsg.MsgMapInfo();
            mapinfo.Create(null, session.GetGamePackKeyEx());
            mapinfo.Init(this.GetBaseAttr().mapid, this.GetCurrentX(), this.GetCurrentY(), NetMsg.MsgMapInfo.ENTERMAP);
            session.SendData(mapinfo.GetBuffer());


            //初始化一些信息
            this.ChangeAttribute(UserAttribute.LOOKFACE, GetLookFace());
            //刷新可视列表;
            GameStruct.Action act = new GameStruct.Action(GameStruct.Action.MOVE);
            this.PushAction(act);

            ScriptTimerManager.Instance().PlayerEnterGame(this.GetBaseAttr().player_id);
            //发送天气信息
            this.GetGameMap().SendWeatherInfo(this);

            //
         //   this.MsgBox("QQ群号:306929937");
        }


        public void ExitGame()
        {
            SetExit(true);
            this.GetEudemonSystem().ExitGame();
            this.GetTimerSystem().ExitGame();
            //摊位要收摊
            PtichManager.Instance().ShutPtich(this,false);
            //通知dbserver 保存数据
            DBServer.Instance().SaveRoleData(this,true);

            UserEngine.Instance().RemovePlayObject(this);
    
            //发给其他好友下线信息
            GetFriendSystem().BrocatMsg(NetMsg.MsgFriendInfo.TYPE_OFFLIE);
            //有队伍就离队
            if (GetTeam() != null)
            {
                GetTeam().ExitTeam(this);
            }
            GetFightSystem().RemoveQiShiTuanGuardEffect(); //移除骑士团守护特效

            IDManager.RecoveryTypeID(this.GetTypeId(), this.type);

            //定时器-
            ScriptTimerManager.Instance().PlayerExitGame(this.GetBaseAttr().player_id);
            
        }
        //强制断开连接
        //用于被挤下线与封包攻击-
        public void Kick()
        {
            if (this.GetGameSession() != null)
            {
                this.GetGameSession().Dispose();
            }

            ExitGame();
        }
        private void SendRoleOtherSystemInfo()
        {
            //发送玩家技能信息
            GetMagicSystem().SendAllMagicInfo();
            //发送玩家道具信息
            GetItemSystem().SendAllItemInfo();
            //发送体力信息
            this.ChangeAttribute(UserAttribute.SP, 0);
    
            //发送幻兽信息
            GetEudemonSystem().SendAllEudemonInfo();
            //发送热键信息
            SendHotKeyInfo();
            //好友信息
            GetFriendSystem().SendAllFriendInfo();
            //发给其他好友上线信息
            GetFriendSystem().BrocatMsg(NetMsg.MsgFriendInfo.TYPE_ONLINE);
            //发送给军团信息
            GetLegionSystem().SendLegionInfo();
            //发送爵位信息
            GuanJueManager.Instance().SendGuanJueInfo(this);

        }

      
        public void FlyMap(uint mapid, short x, short y, byte dir)
        {
            GameMap map = MapManager.Instance().GetGameMapToID(mapid);
            if (map == null)
            {
                Log.Instance().WriteLog("未找到游戏地图id:" + mapid.ToString());
                return;
            }
            if (GetGameMap() != null)
            {
                GetGameMap().RemoveObj(this);
            }
            this.mGameMap = map;
            this.GetBaseAttr().mapid = mapid;
            this.SetPoint(x, y);
            this.SetDir(dir);
            map.AddObject(this, this.GetGameSession());
            //重新刷新可视列表

            GameStruct.Action act = new GameStruct.Action(GameStruct.Action.MOVE);
            this.PushAction(act);
            //发给玩家
            NetMsg.MsgMapInfo mapinfo = new NetMsg.MsgMapInfo();
            mapinfo.Create(null, GetGamePackKeyEx());
            mapinfo.Init(mapid, x, y, NetMsg.MsgMapInfo.ENTERMAP);
            this.SendData(mapinfo.GetBuffer());
            //发送天气信息
            this.GetGameMap().SendWeatherInfo(this);
        }

        
         public override void ClearThis()
        {
            base.ClearThis();
            //NetMsg.MsgClearObjectInfo info = new NetMsg.MsgClearObjectInfo();
            //info.id = this.GetTypeId();
            //this.BroadcastBuffer(info.GetBuffer());
            //GetGameMap().BroadcastBuffer(this, info.GetBuffer());
        }
         public void ClearThis(PlayerObject play)
         {
             NetMsg.MsgClearObjectInfo info = new NetMsg.MsgClearObjectInfo();
             info.Create(null, play.GetGamePackKeyEx());
             info.id = this.GetTypeId();
             play.SendData(info.GetBuffer());

             this.GetVisibleList().Remove(play.GetGameID());
         }
        public override void Dispose()
        {
            if (!IsExit())
            {
                ExitGame();
            }
            base.Dispose();
        }

        public bool IsGM()
        {
            if (GameServer.IsTestMode()) return true;
            String name = this.GetName();
            if (name.IndexOf("[PM]") <= 0) return false;
            return true;
        }



        public void ResetLevelExp()
        {
            ulong _exp = 0;
            GameStruct.LevelExp exp = ConfigManager.Instance().GetLevelExp(GameStruct.LevelExp.LEVELEXP_ROLE, GetBaseAttr().level);
            if (exp != null)
            {
                _exp = exp.exp;
            }
            GetBaseAttr().exp_max = _exp;
        }

        //重新计算战斗力
        public void CalcFightSoul()
        {
            int nSoul = this.GetBaseAttr().level; //等级基本战斗力- 
            //神等级
            nSoul += this.GetBaseAttr().godlevel;
            //强化等级取全套装备最低的强化等级为战斗力加成
            //

            int nStrongLv = 0;
            bool bAddStrongLv = true;
            //装备战斗力-------------------------------------------------------------
            for (int i = NetMsg.MsgItemInfo.ITEMPOSITION_HELMET; i < NetMsg.MsgItemInfo.ITEMPOSITION_SHOES + 1; i++)
            {
                GameStruct.RoleItemInfo info = GetItemSystem().GetEquipByPostion((byte)i);
                if (info != null)
                {
                    //装备品质加成 良品:1  上品:2   精品:3    极品:4  上品神器:5  精品神器:6 极品神器:7
                    String sQuaity = info.itemid.ToString();
                    int quaity = Convert.ToInt32(sQuaity.Substring(sQuaity.Length - 1));
                    nSoul += quaity;
                    //-------------------------------------------------------------
                    //打洞- 打一个洞得一点战斗力
                    nSoul += info.GetGemCount();
                    //镶嵌的宝石类型  16.战斗力+1  17.战斗力+3 18.战斗力+5
                    for (byte j = 0; j < info.GetGemCount(); j++)
                    {
                        byte gemtype = info.GetGemType(j);
                        if (gemtype >= 16 && gemtype <= 18)
                        
                        {
                            switch (gemtype)
                            {
                                case 16: { nSoul += 1; break; }
                                case 17: { nSoul += 3; break; }
                                case 18: { nSoul += 5; break; }
                            }
                        }
                    }

                    //取最低的套件中的强化等级
                    if (bAddStrongLv)
                    {
                        if (nStrongLv == 0)
                        {
                            nStrongLv = info.GetStrongLevel();
                        }
                        else if (nStrongLv > info.GetStrongLevel())
                        {
                            nStrongLv = info.GetStrongLevel();
                        }
                    }
                    //符合装备穿戴等级- +1点战斗力
                    if (EquipOperation.Instance().IsAccordWithEquip(this.GetBaseAttr().level,this.GetBaseAttr().profession, (byte)i, info))
                    {
                        nSoul++;
                    }
                        //accord with


                }
                else //有一个部位没有装备- 不加强化等级战斗力了
                {
                    bAddStrongLv = false;
                }
            }
            nSoul += nStrongLv;
            //爵位       设立数量   册封条件               战斗力分享
            //王/女王       3 	    捐献榜第1-3名 	        6
            //公爵 	        12 	    捐献榜第4-15名          5
            //侯爵 	        35 	    捐献榜第16-50名 	    4
            //伯爵 	        不限 	捐献金额>=2亿金币 	    3
            //子爵 	        不限 	捐献金额>=1亿金币 	    2
            //勋爵 	        不限 	捐献金额>=3千万金币 	1
            GUANGJUELEVEL gjlv = this.GetGuanJue();
            switch (gjlv)
            {
                case GUANGJUELEVEL.KING:
                case GUANGJUELEVEL.QUEEN: { nSoul += 6; break; }
                case GUANGJUELEVEL.DUKE: { nSoul += 5; break; }
                case GUANGJUELEVEL.MARQUIS: { nSoul += 4; break; }
                case GUANGJUELEVEL.EARL: { nSoul += 3; break; }
                case GUANGJUELEVEL.VISCOUNT: { nSoul += 2; break; }
                case GUANGJUELEVEL.LORD: { nSoul += 1; break; }
            }
          

            //圣耀符文
            GameStruct.RoleItemInfo item_info = GetItemSystem().GetEquipByPostion(NetMsg.MsgItemInfo.ITEMPOSTION_RUB_SHENGYAOFUWEN);
            if (item_info != null)
            {
                nSoul += item_info.GetStrongLevel() * 2; //追加战斗力宝石-  强化等级+战斗力=最终战斗力
            }
            //加伤害与减伤害暂时不实现功能- 只加战斗力
            //帝龙之泪
            item_info = GetItemSystem().GetEquipByPostion(NetMsg.MsgItemInfo.ITEMPOSTION_RUB_DILONGZHILEI);
            if (item_info != null)
            {
                nSoul += item_info.GetStrongLevel();
            }
            //曙光战魂
            item_info = GetItemSystem().GetEquipByPostion(NetMsg.MsgItemInfo.ITEMPOSTION_RUB_SHUGUANGZHANHUN);
            if (item_info != null)
            {
                nSoul += item_info.GetStrongLevel();
            }
            //幻兽星级-------------------------------------
            nSoul += this.GetEudemonSystem().CalcFightSoul();

            mnFightSoul = nSoul;
        }
        public override void CalcAttribute()
        {
            //先更新基础属性--
            GameStruct.BaseAttributeInfo attinfo = ConfigManager.Instance().GetAttributeInfo(GetBaseAttr().profession, GetBaseAttr().level);
            PlayerAttribute baseAttr = GetBaseAttr();
            baseAttr.resetAttr();
            if (attinfo != null)
            {

                baseAttr.life_max = attinfo.GetLife();
                baseAttr.attack_max = baseAttr.attack = attinfo.GetAttack();
                baseAttr.doage = attinfo.GetDoage();
                //亡灵巫师 异能者 血族也有蓝的
                //if (baseAttr.profession == JOB.MAGE)
                //{
                    baseAttr.mana_max = attinfo.GetMana();
                    baseAttr.magic_attack = baseAttr.magic_attack_max = attinfo.GetMagicAttack();
              // }

            }


            //装备属性--
            for (int i = NetMsg.MsgItemInfo.ITEMPOSITION_HELMET; i < NetMsg.MsgItemInfo.ITEMPOSITION_SHOES; i++)
            {
                GameStruct.RoleItemInfo info = GetItemSystem().GetEquipByPostion((byte)i);
                if (info != null)
                {
                    GameStruct.ItemTypeInfo baseitem = ConfigManager.Instance().GetItemTypeInfo(info.itemid);
                    if (baseitem != null)
                    {
                        baseAttr.attack += baseitem.attack_min;
                        baseAttr.attack_max += baseitem.attack_max;
                        baseAttr.magic_attack += baseitem.magic_attack_min;
                        baseAttr.magic_attack_max += baseitem.magic_attck_max;
                        baseAttr.doage += baseitem.dodge;
                        baseAttr.hitrate += baseitem.hitrate;
                        baseAttr.defense += baseitem.defense;
                        baseAttr.magic_defense += baseitem.magic_defense;

                        //装备强化加成--
                        GameStruct.ItemAdditionInfo itemaddition = ConfigManager.Instance().GetItemAdditionInfo((byte)i, info.GetStrongLevel());
                        if (itemaddition != null)
                        {
                            baseAttr.attack += itemaddition.min_attack;
                            baseAttr.attack_max += itemaddition.max_attack;
                            baseAttr.life += itemaddition.life;
                            baseAttr.defense += itemaddition.defense;
                            baseAttr.magic_attack += itemaddition.min_magic_attack;
                            baseAttr.magic_attack_max += itemaddition.max_magic_attack;
                            baseAttr.magic_defense += itemaddition.magic_defense;
                            baseAttr.doage += itemaddition.dodge;
                        }
                    }
                }
            }
            //计算战斗力
            this.CalcFightSoul();

        }

        public override byte GetLevel()
        {
            return GetBaseAttr().level;
        }
        public override int GetMinAck()
        {
            int atk = ((int)GetBaseAttr().attack) + this.GetEudemonSystem().GetFitEudemonMinAtk();
            return atk;
        }

        public override int GetMaxAck()
        {
            int atk = ((int)GetBaseAttr().attack_max) + this.GetEudemonSystem().GetFitEudemonMaxAtk();
            return atk;
        }

        public override int GetDefense()
        {
            //如果有合体幻兽.就取合体幻兽的
            EudemonObject obj = this.GetEudemonSystem().GetInjuredEudemon();
            if (obj != null)
            {
                return obj.GetDefense();
            }
            return (int)GetBaseAttr().defense;
        }

        public override int GetMagicDefense()
        {
            //如果有合体幻兽.就取合体幻兽的
            EudemonObject obj = this.GetEudemonSystem().GetInjuredEudemon();
            if (obj != null)
            {
                return obj.GetMagicDefense();
            }
            return (int)GetBaseAttr().magic_defense;
        }
        public override int GetMagicAck()
        {
            return (int)GetBaseAttr().magic_attack;
        }

        public override int GetMaxMagixAck()
        {
            return (int)GetBaseAttr().magic_attack_max;
        }
        public void SetPkMode(byte pkmode)
        {
            GetBaseAttr().pk_mode = pkmode;
        }

        //打开对话框
        public void OpenDialog(int dwData)
        {
            if (mNpcInfo == null)
            {

                return;
            }
            NetMsg.MsgOpenDialog msg = new NetMsg.MsgOpenDialog();
            msg.Create(null, this.GetGamePackKeyEx());
            msg.playid = GetTypeId();
            msg.npc_x = mNpcInfo.x;
            msg.npc_y = mNpcInfo.y;
            msg.npcid = mNpcInfo.id;
            msg.dialog_type = dwData;
            this.SendData(msg.GetBuffer());
     
            switch (dwData)
            {
                //发送仓库数据
                case NetMsg.MsgOpenDialog.OPENDIALOGTYPE_STRONG:
                    {
                        //需要延时一下，不然客户端无法显示-
                        System.Threading.Thread.Sleep(50);

                        List<GameStruct.RoleItemInfo> list_item = new List<GameStruct.RoleItemInfo>();
                        this.GetItemSystem().GetItemStrongInfo(list_item);

                        int nPage = list_item.Count / 6;
                        if (list_item.Count % 6 > 0) nPage++;
                       
                       for(int i = 0;i < nPage;i++)
                       {
                           NetMsg.MsgStrongInfo stronginfo = new NetMsg.MsgStrongInfo();
                           if (i > 0) stronginfo.param1 = 3;
                           stronginfo.playid = this.GetTypeId();
                           int nStartIndex = i * 6;
                           for (int j = 0; j < 6; j++)
                           {
                               if (nStartIndex >= list_item.Count) break;
                               stronginfo.list_item.Add(list_item[nStartIndex]);
                               nStartIndex++;
                           }
                           stronginfo.Create(null, this.GetGamePackKeyEx());
                           this.SendData(stronginfo.GetBuffer());

                        }
                       

                        //仓库的金币
                       byte[] gold = NetMsg.MsgStrongInfo.GetStrongMoneyBuffer(this.GetTypeId(), this.GetMoneyCount(MONEYTYPE.STRONGGOLD));
                       this.SendData(gold,true);
                        break;
                    }

            }

        }
        //在当前地图随机传送
        public void ScroolRandom(short _x = 0, short _y = 0)
        {

            int index = 0;
            short x = _x;
            short y = _y;
            if (x == 0 && y == 0)
            {
                while (true)
                {
                    x = (short)IRandom.Random(1, (int)this.GetGameMap().mnWidth);
                    y = (short)IRandom.Random(1, (int)this.GetGameMap().mnHeight);
                    if (this.GetGameMap().CanMove(x, y)) break;
                    if (index > 100) return;
                    index++;
                }

            }
            //先清除自身对象
            this.ClearThis();
            this.SetPoint(x, y);

            NetMsg.MsgScroolRandom msg = new NetMsg.MsgScroolRandom();
            msg.Create(null, GetGamePackKeyEx());
            msg.time = System.Environment.TickCount;
            msg.x = msg._x = this.GetCurrentX();
            msg.y = msg._y = this.GetCurrentY();
            msg.roleid = this.GetTypeId();
            this.SendData(msg.GetBuffer());

            this.GetVisibleList().Clear();

            GameStruct.Action act = new GameStruct.Action(GameStruct.Action.MOVE);
            this.PushAction(act);
            //幻兽也要跟随
            this.GetEudemonSystem().FlyPlay();
        }


        //更换地图 用于副本场景
        public void ChangeFubenMap(GameMap map, short x, short y)
        {
            if (map == null) return;
            this.GetGameMap().RemoveObj(this);
            //召回所有幻兽- 
            this.GetEudemonSystem().Eudemon_ReCallAll(true);
            map.AddObject(this, this.GetGameSession());

            //先清除自身对象
            this.ClearThis();

            this.SetPoint(x, y);
            //要发二个包
            NetMsg.MsgReCall1 msg = new NetMsg.MsgReCall1();
            msg.Create(null, GetGamePackKeyEx());
            msg.roleid = this.GetTypeId();
            msg.mapid = (int)this.GetGameMap().GetMapInfo().id;
            msg.x = this.GetCurrentX();
            msg.y = this.GetCurrentY();
            this.SendData(msg.GetBuffer());

            NetMsg.MsgReCall2 msg1 = new NetMsg.MsgReCall2();
            msg1.Create(null, GetGamePackKeyEx());
            msg1.roleid = this.GetTypeId();
            msg1.x = this.GetCurrentX();
            msg1.y = this.GetCurrentY();
            this.SendData(msg1.GetBuffer());

            this.GetVisibleList().Clear();

            GameStruct.Action act = new GameStruct.Action(GameStruct.Action.MOVE);
            this.PushAction(act);

            GetBaseAttr().mapid = map.GetMapInfo().id;

            this.SendJueweiNotice();
            this.SetTransmitIng(true);
        }
        //更换地图 用于普通场景
        public void ChangeMap(uint mapid, short x, short y)
        {
            GameMap target_map = MapManager.Instance().GetGameMapToID(mapid);
            if (target_map == null)
            {
                Log.Instance().WriteLog("传送地图失败,该地图id不存在," + mapid.ToString() + " x:" + x.ToString() + " y:" + y.ToString());
                return;
            }
            this.GetGameMap().RemoveObj(this);
            //召回所有幻兽- 
            this.GetEudemonSystem().Eudemon_ReCallAll(true);
            target_map.AddObject(this, this.GetGameSession());

            //先清除自身对象
            this.ClearThis();

            this.SetPoint(x, y);
            //要发二个包
            NetMsg.MsgReCall1 msg = new NetMsg.MsgReCall1();
            msg.Create(null, GetGamePackKeyEx());
            msg.roleid = this.GetTypeId();
            msg.mapid = (int)this.GetGameMap().GetMapInfo().id;
            msg.x = this.GetCurrentX();
            msg.y = this.GetCurrentY();
            this.SendData(msg.GetBuffer());

            NetMsg.MsgReCall2 msg1 = new NetMsg.MsgReCall2();
            msg1.Create(null, GetGamePackKeyEx());
            msg1.roleid = this.GetTypeId();
            msg1.x = this.GetCurrentX();
            msg1.y = this.GetCurrentY();
            this.SendData(msg1.GetBuffer());

            this.GetVisibleList().Clear();

            GameStruct.Action act = new GameStruct.Action(GameStruct.Action.MOVE);
            this.PushAction(act);

            GetBaseAttr().mapid = mapid;

            this.SendJueweiNotice();
            this.SetTransmitIng(true);
         
          //  this.GetEudemonSystem().FlyPlay();
          

        }
        //回城
        public void ReCallMap()
        {
            short recallx = (short)GetGameMap().GetMapInfo().recallx;
            short recally = (short)GetGameMap().GetMapInfo().recally;
            uint recallmapid = GetGameMap().GetMapInfo().recallid;

            ChangeMap(recallmapid, recallx, recally);

        }

        //广播
        public void BroadcastBuffer(byte[] data, bool isThis = false/*是否也发给自己*/)
        {
            //  this.RefreshVisibleObject();
            foreach (RefreshObject o in GetVisibleList().Values)
            {
                BaseObject obj = o.obj;
                if (obj.type == OBJECTTYPE.PLAYER && obj.GetGameSession() != null)
                {
                    NetMsg.BaseMsg msg = new NetMsg.BaseMsg();
                    msg.Create(data, obj.GetGamePackKeyEx());
                    obj.SendData(msg.GetBuffer());
                }
            }
            if (isThis)
            {
                if (this.GetGameSession() != null)
                {
                    NetMsg.BaseMsg msg = new NetMsg.BaseMsg();
                    msg.Create(data, this.GetGamePackKeyEx());
                    this.SendData(msg.GetBuffer());
                }

            }
        }

        //更改角色基本属性

        public void ChangeAttribute(GameStruct.UserAttribute type, int value, bool isBrocat = true)
        {
            if (this.GetGameSession() == null) return;
            int v = value;
            switch (type)
            {
                case GameStruct.UserAttribute.GOLD:
                    {
                        if (this.GetBaseAttr().gold + value> GameBase.Config.Define.MAX_GOLD) //最高三十亿
                        {
                            this.MsgBox("少年，别再刷了，最高二十亿金币！");
                            this.GetBaseAttr().gold = GameBase.Config.Define.MAX_GOLD;
                        }else
                        {
                            this.GetBaseAttr().gold += value;
                        }

                        if (this.GetBaseAttr().gold < 0) { this.GetBaseAttr().gold = 0; }
                        v = (int)this.GetBaseAttr().gold;
                       // if (v > PlayerItem.MAX_GOLD) this.GetBaseAttr().gold = PlayerItem.MAX_GOLD;
                        break;
                    }
                case GameStruct.UserAttribute.GAMEGOLD:
                    {
                        if (this.GetBaseAttr().gold + value > GameBase.Config.Define.MAX_GAMEGOLD) //最高三十亿
                        {
                            this.GetBaseAttr().gamegold = GameBase.Config.Define.MAX_GAMEGOLD;
                            this.MsgBox("少年，别再刷了，最高二十亿魔石！");
                        }
                        else
                        {
                            this.GetBaseAttr().gamegold += value;
                        }

                        if (this.GetBaseAttr().gamegold < 0) { this.GetBaseAttr().gamegold = 0; }
                        v = this.GetBaseAttr().gamegold;
                        break;
                    }
                case GameStruct.UserAttribute.LEVEL:
                    {
                        this.GetBaseAttr().level += (byte)value;
                        v = GetBaseAttr().level;
                        //重新计算属性
                        this.CalcAttribute();
                        this.GetBaseAttr().life = this.GetBaseAttr().life_max;
                        this.GetBaseAttr().mana = this.GetBaseAttr().mana_max;
                        //升级特效
                        PacketOut outpack = new PacketOut();
                        outpack.WriteInt16(28);
                        outpack.WriteInt16(1010);
                        outpack.WriteInt32(System.Environment.TickCount);
                        outpack.WriteUInt32(this.GetTypeId());
                        outpack.WriteInt32(0);
                        outpack.WriteInt32(0);
                        outpack.WriteInt32(1);
                        outpack.WriteInt32(9550);
                        this.BroadcastBuffer(outpack.Flush(), true);
                        break;
                    }
                case GameStruct.UserAttribute.EXP:
                    {
                        this.GetBaseAttr().exp += value;
                        v = (int)this.GetBaseAttr().exp;
                        //发送公告
                        // this.LeftNotice(string.Format(StringDefine.KILLEXP,value.ToString()));

                        break;
                    }
                case GameStruct.UserAttribute.LIFE:
                    {
                        v = (int)GetBaseAttr().life + value;
                        if (v < 0) v = 0;
                        GetBaseAttr().life = (uint)v;
                        if (v > GetBaseAttr().life_max) GetBaseAttr().life = GetBaseAttr().life_max;
                        //无敌状态
                        if (this.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_WUDI) != null)
                        {
                            GetBaseAttr().life = GetBaseAttr().life_max;
                        }
                        break;
                    }
                case GameStruct.UserAttribute.LIFE_MAX:
                    {
                        GetBaseAttr().life_max = (uint)value;

                        break;
                    }
                case GameStruct.UserAttribute.MANA:
                    {
                        v = (int)GetBaseAttr().mana + value;
                        if (v < 0) v = 0;
                        GetBaseAttr().mana = (uint)v;
                        break;
                    }
                case GameStruct.UserAttribute.MANA_MAX:
                    {
                        GetBaseAttr().mana_max = (uint)value;

                        break;
                    }
                case GameStruct.UserAttribute.LOOKFACE:
                    {
                        //	pPlayer->SetFace((m_pInfo->aUserAttrib[i].dwAttributeData/10000)%1000);
                        // pPlayer->SetSex((m_pInfo->aUserAttrib[i].dwAttributeData / 1000) % 10);
                        //pPlayer->Transform(m_pInfo->aUserAttrib[i].dwAttributeData / 10000000);
                        //如果是鬼魂状态不写到数据库
                        if (!IsGhost())
                        {
                            this.GetBaseAttr().lookface = (uint)value;
                        }

                        v = value;
                        break;
                    }
                case GameStruct.UserAttribute.HAIR:
                    {
                        this.GetBaseAttr().hair = (uint)value;
                        v = value;
                        break;
                    }
                case GameStruct.UserAttribute.STATUS:
                    {
                        v = value;
                        break;
                    }
                case GameStruct.UserAttribute.SP:
                    {
                        this.GetBaseAttr().sp += value;
                        v = this.GetBaseAttr().sp;
                        break;
                    }
                case GameStruct.UserAttribute.PK:
                    {
                        //防止溢出
                        if (this.GetBaseAttr().pk + value >= 30000)
                        {
                            this.GetBaseAttr().pk = 30000;
                        }
                        else
                        {
                            this.GetBaseAttr().pk += (short)value;
                        }
                       
                        if (this.GetBaseAttr().pk < 0) this.GetBaseAttr().pk = 0;
                        this.GetPKSystem().ResetPKNameType();
                        break;
                    }
                case GameStruct.UserAttribute.MAXEUDEMON:
                    {
                        this.GetBaseAttr().maxeudemon += (byte)value;
                        v = this.GetBaseAttr().maxeudemon;
                        break;
                    }

            }
            NetMsg.MsgUserAttribute msg = new NetMsg.MsgUserAttribute();
            msg.role_id = this.GetTypeId();
            if (isBrocat) { msg.Create(null, null); }

            msg.AddAttribute(type, (uint)v);

            if (type == GameStruct.UserAttribute.LEVEL)
            {
                msg.AddAttribute(UserAttribute.LIFE_MAX, this.GetBaseAttr().life_max);
                msg.AddAttribute(UserAttribute.LIFE, this.GetBaseAttr().life);
                if (this.GetBaseAttr().mana_max > 0)
                {
                    msg.AddAttribute(UserAttribute.MANA_MAX, this.GetBaseAttr().mana_max);
                    msg.AddAttribute(UserAttribute.MANA, this.GetBaseAttr().mana);
                }

            }
            //是否广播
            if (isBrocat)
            {
                this.BroadcastBuffer(msg.GetBuffer(), true);
            }
            else
            {
                this.SendData(msg.GetBuffer(), true);
            }
        }


        public void AddExp(int nDamage, int nAtkLev, int nDefLev)
        {
            int nExp = BattleSystem.AdjustExp(nDamage, nAtkLev, nDefLev);
            nExp = this.AdjustExp(nExp);
            //发送获得经验公告
            this.LeftNotice(string.Format(StringDefine.KILLEXP, nExp.ToString()));
            //this.GetBaseAttr().exp += nExp;
            this.ChangeAttribute(GameStruct.UserAttribute.EXP, nExp,false);
            bool bChangeLevel = false;
            while (true)
            {
                LevelExp exp = ConfigManager.Instance().GetLevelExp(GameStruct.LevelExp.LEVELEXP_ROLE, this.GetLevel());
                if (exp != null)
                {
                    if ((ulong)this.GetBaseAttr().exp >= exp.exp)
                    {
                        this.GetBaseAttr().exp -= (int)exp.exp;
                        this.GetBaseAttr().level += 1;
                        bChangeLevel = true;
                    }
                    else break;
                }
                else break; ;
            }

         
            if (bChangeLevel)
            {
                this.ChangeAttribute(GameStruct.UserAttribute.LEVEL, 0);
                this.ChangeAttribute(GameStruct.UserAttribute.EXP, 0,false);
            }
            //出征与合体的幻兽也增加经验
            this.GetEudemonSystem().AddExp(nExp);
        }

        public override void Injured(BaseObject obj, uint value, NetMsg.MsgAttackInfo info)
        {

           
            this.GetFightSystem().SetFighting();


            //幻兽优先受伤害
            if (!this.GetEudemonSystem().Eudemon_Injured(obj, value, info))
            {
                //如果没有幻兽抵挡. 玩家就受到真实伤害
                this.ChangeAttribute(GameStruct.UserAttribute.LIFE, -(int)value);
            }
            GameStruct.Action action;
            action = new GameStruct.Action(GameStruct.Action.INJURED, null);
            action.AddObject(obj);
            action.AddObject(value);
            action.AddObject(info);
            this.PushAction(action);
        }
        //左上角公告
        public void LeftNotice(String text)
        {
            NetMsg.MsgLeftNotice notice = new NetMsg.MsgLeftNotice();
            notice.Create(null, GetGamePackKeyEx());
            notice.SetRoleName(this.GetName());
            notice.SetText(text);
            this.SendData(notice.GetBuffer());
        }
        //聊天框公告
        public void ChatNotice(String text)
        {
            NetMsg.MsgNotice notice = new NetMsg.MsgNotice();
            notice.Create(null, GetGamePackKeyEx());
            byte[] buff = notice.GetChatNoticeBuff(text);
            this.SendData(buff);


        }

        //信息框 2015.9.21 23：36
        public void MsgBox(String text)
        {
            NetMsg.MsgNotice notice = new NetMsg.MsgNotice();
            notice.Create(null, GetGamePackKeyEx());
            byte[] buff = notice.GetMsgBoxBuff(text);
            this.SendData(buff);
        }

        public void SendHotKeyInfo()
        {
            NetMsg.MsgHotKey hotkey = new NetMsg.MsgHotKey();
            hotkey.Create(null, GetGamePackKeyEx());

            hotkey.type = 0;
            hotkey.tag = 214;
            hotkey.tag2 = 4;
            hotkey.str = "";
            for (int i = 0; i < mListHotKey.Count; i++)
            {
                hotkey.str += mListHotKey[i].GetString() + "-";

            }
            this.SendData(hotkey.GetBuffer());

            //通知客户端显示快捷键图标
            PacketOut ooutpack = new PacketOut(GetGamePackKeyEx());
            ooutpack.WriteUInt16(14);
            ooutpack.WriteUInt16(1015);
            ooutpack.WriteInt32(0);
            ooutpack.WriteInt16(656);
            ooutpack.WriteInt32(2);
            this.SendData(ooutpack.Flush());


        }

        public void AddHotKeyInfo(GameStruct.HotkeyInfo info)
        {
            mListHotKey.Add(info);
        }
        public void ClearHotKey(byte group)
        {
            int i = mListHotKey.Count;
            if (i > 0)
            {
                while (true)
                {
                    i--;
                    if (mListHotKey[i].group == group)
                    {
                        mListHotKey.RemoveAt(i);
                    }
                    if (i <= 0) break;
                }
            }


        }
        public String GetHotKeyInfo()
        {
            String str = "";
            for (int i = 0; i < mListHotKey.Count; i++)
            {
                str += mListHotKey[i].GetString(true) + ",";
            }
            return str;
        }

        public void SetHotKeyInfo(String text)
        {
            if (text.Length > 0)
            {
                String[] str = text.Split(',');
                for (int i = 0; i < str.Length; i++)
                {
                    String[] t = str[i].Split('|');
                    if (t.Length == 7)
                    {
                        byte group = Convert.ToByte(t[0]);
                        String data = "";
                        //前面的分隔符是组标识--需要去掉
                        data = str[i].Substring(str[i].IndexOf('|') + 1);
                        GameStruct.HotkeyInfo info = new GameStruct.HotkeyInfo(group, data);
                        AddHotKeyInfo(info);

                    }
                }

            }

        }
        //变鬼魂
        public void TransformGhost()
        {
            m_bGhost = true;
            ChangeAttribute(UserAttribute.LOOKFACE, GetLookFace(), true);
        }

        public int GetMoneyCount(MONEYTYPE type)
        {
            switch (type)
            {
                case MONEYTYPE.GOLD:
                    {
                        return (int)this.GetBaseAttr().gold;
                    }
                case MONEYTYPE.GAMEGOLD:
                    {
                        return (int)this.GetBaseAttr().gamegold;
                    }
                case MONEYTYPE.STRONGGOLD:
                    {
                        return (int)this.GetBaseAttr().stronggold;
                    }
            }
            return -1;
        }

        public void ChangeMoney(MONEYTYPE type, int value)
        {
            switch (type)
            {
                case MONEYTYPE.GOLD:
                    {
                        this.ChangeAttribute(UserAttribute.GOLD, value);
                        break;
                    }
                case MONEYTYPE.GAMEGOLD:
                    {
                        this.ChangeAttribute(UserAttribute.GAMEGOLD, value);
                        break;
                    }
                case MONEYTYPE.STRONGGOLD:
                    {
                        this.GetBaseAttr().stronggold += value;
                        if (this.GetBaseAttr().stronggold > PlayerItem.MAX_GOLD) this.GetBaseAttr().stronggold = PlayerItem.MAX_GOLD;
                        break;
                    }
            }
        }

        //播放机器人动作
        public void PlayRobotAction(uint action_id)
        {
            //foreach (BaseObject obj in mVisibleList.Values)
            //{
            //    if (obj.type == OBJECTTYPE.ROBOT)
            //    {
            //        (obj as RobotObject).PlayFaceAcion(action_id);
            //    }
            //}
        }
        //刷新角色信息
        public void RefreshRoleInfo()
        {
            //this.RefreshVisibleObject();
            //foreach (RefreshObject obj in mVisibleList.Values)
            //{
            //    if (obj.type == OBJECTTYPE.PLAYER)
            //    {
            //        (obj as PlayerObject).SendRoleInfo(this);
            //    }
            //}
        }

        //发送爵位公告
        private void SendJueweiNotice()
        {
            String str = "";
            GUANGJUELEVEL lv = this.GetGuanJue();
            switch (lv)
            {
                case GUANGJUELEVEL.KING:
                    {
                        str = string.Format("王国的守护者，继承神之豪光的不朽之王{0}来到了{1}！他的驾临为人们带来了无限的勇气与希望。",
                            this.GetName(), this.GetGameMap().GetMapInfo().name);

                        break;
                    }
                case GUANGJUELEVEL.QUEEN:
                    {
                        str = string.Format("{0}女王驾临{1}，她的光辉如正午最灿烂的太阳，她的笑容如永夜不曾陨落的恒星，令人赞绝不已。",
                            this.GetName(), this.GetGameMap().GetMapInfo().name);
                        break;
                    }
                case GUANGJUELEVEL.DUKE:
                    {
                        str = string.Format("由于{0}公爵的光临，{1}的人们在这一刻将同分享属于亚特王国无上的骄傲与荣耀。",
                            this.GetName(), this.GetGameMap().GetMapInfo().name);
                        break;
                    }
                case GUANGJUELEVEL.MARQUIS:
                    {
                        str = string.Format("{0}侯爵光临{1}，他伟岸的身影如同雄鹰给人们带来希望。",
                            this.GetName(), this.GetGameMap().GetMapInfo().name);
                        break;

                    }
            }
            if (str.Length > 0)
            {
                this.GetGameMap().BroadcastMsg(BROADCASTMSGTYPE.LEFT, str);
            }
        }

        //骑乘
        //eudemon_id 幻兽id
        //nMountID 坐骑id

        public void TakeMount(uint eudemon_id, uint nMountID)
        {
            //锁定状态不允许骑乘
            if (this.IsLock())
                return;
 
            this.GetEudemonSystem().TakeMount(eudemon_id);

            byte[] data = { 36, 0, 244, 7, 209, 0, 7, 0 };
            //75为 75星品质的幻兽
            byte[] data1 = { 75, 0, 0, 0, 1, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0 };
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(data);
            outpack.WriteUInt32(this.GetTypeId());
            outpack.WriteUInt32(nMountID);
            outpack.WriteUInt32(eudemon_id);
            outpack.WriteBuff(data1);
            this.BroadcastBuffer(outpack.Flush(), true);
            mnMountID = nMountID;

            this.GetMagicSystem().SetMoveSpeed(GameBase.Config.Define.ROLE_RIG_MOVE_SPEED);

            
        }
        //下马
        //eudemon_id 幻兽id
        public void TakeOffMount(uint eudemon_id)
        {
            //锁定状态不允许骑乘
            if (this.IsLock())
                return;
            this.GetEudemonSystem().TakeOffMount(eudemon_id);
            //收到网络协议:长度：28协议号:1009
            //{28,0,241,3,39,31,97,5,2,32,201,122,111,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            PacketOut outpack = new PacketOut();
            outpack.WriteUInt16(28);
            outpack.WriteUInt16(1009);
            outpack.WriteUInt32(this.GetTypeId());
            outpack.WriteUInt32(eudemon_id);
            byte[] data1 = { 111, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            outpack.WriteBuff(data1);
            this.BroadcastBuffer(outpack.Flush(), true);
            mnMountID = 0;
            this.GetMagicSystem().SetMoveSpeed(GameBase.Config.Define.ROLE_MOVE_SPEED);
        }
        //是否在骑乘状态
        public bool IsMountState() { return mnMountID != 0; }
        //如果可以pk该对象返回true
        //OBJ 被PK的对象
        //bCrime 是否立即闪蓝[用于范围技能]
        public override bool CanPK(BaseObject obj, bool bGoCrime = true)
        {
            bool bCrime = true; //犯罪标识
            PlayerObject _play = null;
            if (obj.type == OBJECTTYPE.EUDEMON)
            {
                _play = (obj as EudemonObject).GetOwnerPlay();

            }
            if (obj.type == OBJECTTYPE.PLAYER)
            {
                _play = (obj as PlayerObject);
            }
            if (_play == null) return true;
            byte pkmode = this.GetBaseAttr().pk_mode;
            bool bCanPk = false;
            if (pkmode == GameBase.Config.Define.PK_MODE_FREE)
            {
                bCanPk = true;
            }
            if (pkmode == GameBase.Config.Define.PK_MODE_SAFE) return false; //安全pk模式
            if (pkmode == GameBase.Config.Define.PK_MODE_GUARD)
            {
                if (_play.GetPKSystem().IsPKing() ||
                    _play.GetPKSystem().GetNameType() == GameBase.Config.Define.PK_NAME_BLACK)//蓝名 or 黑名
                {
                    bCanPk = true;
                    bCrime = false;
                }
            }
            //对方已隐身
            if (_play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_HIDDEN) != null)
            {
                bCanPk = false;
            }

            //闪蓝-PK犯罪状态
            if (bCanPk && bGoCrime == true)
            {
                this.GetPKSystem().SetPKIng(true, bCrime);
            }
            return bCanPk;
        }

        public void PlayAction(uint action_id)
        {
            //死亡 与锁定--
            if (this.IsDie() || this.IsLock())
            {
                return;
            }
            this.SetCurrentAction(action_id);
            PacketOut outpack;
            outpack = new PacketOut();
            outpack.WriteUInt16(28);
            outpack.WriteUInt16(1010);
            outpack.WriteUInt32(0);
            outpack.WriteUInt32(this.GetTypeId());
            outpack.WriteUInt32(23855267);
            outpack.WriteUInt32(this.GetDir());
            outpack.WriteUInt32(action_id);
            outpack.WriteUInt32(9530);
            byte[] data = outpack.Flush();
            this.BroadcastBuffer(data, true);
            //foreach (RefreshObject o in mVisibleList.Values)
            //{
            //    BaseObject obj = o.obj;
            //    if (obj.type == OBJECTTYPE.PLAYER)
            //    {
            //        PlayerObject _play = obj as PlayerObject;
            //        outpack = new PacketOut(_play.GetGamePackKeyEx());
            //        outpack.WriteBuff(data);
            //        _play.SendData(outpack.Flush());
            //    }
            //}
        }

        //复活--
        //isSitu 是否原地复活
        public void Alive(bool isSitu = false)
        {
            this.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_DIE);
            //满血 满蓝
            m_bGhost = false;

            this.ChangeAttribute(UserAttribute.STATUS, 0, true);
            this.ChangeAttribute(UserAttribute.LOOKFACE, GetLookFace(), true);
            this.ChangeAttribute(UserAttribute.LIFE, (int)this.GetBaseAttr().life_max);
            this.ChangeAttribute(UserAttribute.MANA, (int)this.GetBaseAttr().mana_max);
            //回城
            if (!isSitu)
            {
                this.ReCallMap();
            }
            byte[] tick = { 16, 0, 244, 3, 84, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            this.GetGamePackKeyEx().EncodePacket(ref tick, tick.Length);
            this.SendData(tick);


        }
       
        public void Ptich()
        {

            GameStruct.NPCInfo info = this.GetCurrentNpcInfo();
            if (info == null) return;
            short nPtichId = (short)((int)info.id - GameBase.Config.Define.PTICH_START_ID);
            if (PtichManager.Instance().PtichHasPlay(nPtichId))
            {
                return;
            }
            this.SetCurrentPtichID(nPtichId);
            //16,39,0,0 摆摊需要金币
            //4,202,165,213,189 摊位的主人名称 字符串
            //75, 0 摊位的编号
            // byte[] senddata = { 42, 0, 105, 4, 16, 39, 0, 0, 64, 66, 15, 0, 174, 88, 159, 122, 6, 0, 0, 0, 145, 116, 47, 102, 5, 0, 0, 0, 117, 180, 50, 5, 1, 0, 75, 0, 1, 4, 202, 165, 213, 189 };
            byte[] senddata = { 42, 0, 105, 4, 244, 1, 0, 0, 64, 66, 15, 0, 36, 52, 156, 8, 3, 0, 0, 0, 30, 214, 44, 135, 2, 0, 0, 0, 164, 3, 178, 5, 1, 0};
            PacketOut outpack = new PacketOut();
            outpack.WriteBuff(senddata);
            outpack.WriteInt16((short)(nPtichId + 1));
            byte[] data2 = { 1, 4, 202, 165, 213, 189 };
            outpack.WriteBuff(data2);
            this.SendData(outpack.Flush(), true);
         
          
        }


    }
}
