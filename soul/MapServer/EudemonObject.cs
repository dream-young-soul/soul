using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;
using GameBase.Network.Internal;
using GameBase.Network;
using GameStruct;

//幻兽对象-
//2015.9.29
namespace MapServer
{
    public class EudemonObject : BaseObject
    {
        private RoleData_Eudemon mInfo = null;
        public RoleData_Eudemon GetAttr() { return mInfo; }
        private PlayerObject play;
        public RoleData_Eudemon GetEudemonInfo() { return mInfo; }
        public void SetEudemonInfo(RoleData_Eudemon info) { mInfo = info; }
        private Dictionary<uint, PlayerObject> mPlayObject; //玩家的可视列表
        public Dictionary<uint, PlayerObject> GetPlayObjectList() { return mPlayObject; }
        private GameStruct.MonsterInfo mMonsterInfo = null;
        public void SetMosterInfo(GameStruct.MonsterInfo info) { mMonsterInfo = info; }
        public GameStruct.MonsterInfo GetMonsterInfo() { return mMonsterInfo; }
        private EUDEMONSTATE mState = EUDEMONSTATE.NROMAL;
        public EUDEMONSTATE GetState() { return mState; }
        public void SetState(EUDEMONSTATE _state){mState = _state;}
        private bool mbIsCombo; //合体的幻兽受到连击技能处理
        private GameBase.Core.TimeOut mAttackSpeed;         //普通攻击速度
        private List<GameBase.Core.TimeOut> mMagicAttackSpeed; //魔法攻击速度

        private bool mbRiding; //是否正在骑乘中
        public void SetRiding(bool v) { mbRiding = v; }
        public bool IsRiding() { return mbRiding; } //是否正在骑乘中
        public uint GetEudemonId() 
        {
            if(mInfo == null) return 0;
            return mInfo.itemid;
        }
        public EudemonObject(RoleData_Eudemon info,PlayerObject _play)
        {
            type = OBJECTTYPE.EUDEMON;
            mInfo = info;
            play = _play;
            mPlayObject = new Dictionary<uint, PlayerObject>();
            typeid = info.GetTypeID();
            mMonsterInfo = EudemonObject.GetMonsterInfo(play, info.itemid);
            mbIsCombo = false;
            mAttackSpeed = new GameBase.Core.TimeOut();
            mAttackSpeed.SetInterval(GameBase.Config.Define.ROLE_ATTACK_SPEED);
            mAttackSpeed.Update();

            mMagicAttackSpeed = new List<GameBase.Core.TimeOut>();
            this.SetRiding(false);
            this.SetState(EUDEMONSTATE.NROMAL);
           
        }
        public override bool Run()
        {
            base.Run();
            //幻兽被玩家连击后- 超出角色距离就瞬移到角色位置
            if (Math.Abs(play.GetCurrentX() - this.GetCurrentX()) > GameBase.Config.Define.MAX_EUDEMON_PLAY_DISTANCE ||
                Math.Abs(play.GetCurrentY() - this.GetCurrentY()) > GameBase.Config.Define.MAX_EUDEMON_PLAY_DISTANCE)
            {
                this.FlyPlay();
                //this.ClearThis();
                //this.SetPoint(play.GetCurrentX(), play.GetCurrentY());
                //this.SendEudemonInfo();
                return true;
            }
            //连招解锁
            if (this.IsLock())
            {
             
                if (!this.CheckLockTime())
                {
                    this.UnLock();
                }

            }
          
            //出征状态下的死亡
            if (this.GetState() == EUDEMONSTATE.BATTLE)
            {
                if (this.IsDie() && !this.IsLock() && !this.GetAttr().bDie)
                {
                    GameStruct.Action action = new GameStruct.Action(GameStruct.Action.DIE, null);
                    this.PushAction(action);
               }
           }

            //合体状态下的死亡
            if (this.GetState() == EUDEMONSTATE.FIT)
            {
                if (mbIsCombo && IsDie() && !play.IsLock())
                {
                    mbIsCombo = false;
                }
                if (IsDie() && !mbIsCombo && !this.GetAttr().bDie)
                {
                    GameStruct.Action action = new GameStruct.Action(GameStruct.Action.DIE, null);
                    this.PushAction(action);
                }
            }

            if (this.GetState() == EUDEMONSTATE.FIT)
            {
                this.SetPoint(play.GetCurrentX(), play.GetCurrentY());
            }
            return true;
        }
        public bool CheckMagicAttackSpeed(ushort magicid, byte magiclv)
        {
            GameStruct.MagicTypeInfo type = ConfigManager.Instance().GetMagicTypeInfo(magicid, magiclv);
            if (type.delay_ms == 0) return true;
            if (type == null) return false;

            bool bFind = false;
            bool bError = false;
            GameBase.Core.TimeOut time = null;
            for (int i = 0; i < mMagicAttackSpeed.Count; i++)
            {
                time = mMagicAttackSpeed[i];
                if ((ushort)time.GetObject() == magicid)
                {
                    if (time.ToNextTime())
                    {
                        bFind = true;
                        break;
                    }
                    else
                    {
                     
                        bError = true;
                    }
                }
            }

            //把其他的技能施法速度更新一遍
            for (int i = 0; i < mMagicAttackSpeed.Count; i++) { mMagicAttackSpeed[i].Update(); }
            if (!bFind && bError == false)
            {
                time = new GameBase.Core.TimeOut();
                time.SetInterval(type.delay_ms);
                time.SetObject(magicid);
                time.Update();
                mMagicAttackSpeed.Add(time);
                return true;
            }

            return bFind;
        }
        public override bool IsDie()
        {
            return this.GetAttr().life == 0;
        }
        public override void ClearThis()
        {
            
            NetMsg.MsgClearObjectInfo info = new NetMsg.MsgClearObjectInfo();
            info.id = GetTypeId();
            byte[] msg = info.GetBuffer();

            foreach (RefreshObject refobj in this.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                if (obj.type == OBJECTTYPE.PLAYER && obj.GetGameSession() != null)
                {
                    if (obj.GetGameID() == play.GetGameID()) continue; //不发给宿主玩家
                    NetMsg.BaseMsg data = new NetMsg.BaseMsg();
                    data.Create(msg, obj.GetGamePackKeyEx());
                    obj.SendData(data.GetBuffer());

                }
            }

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
            //base.ClearThis();

           
        
        }

        public static GameStruct.MonsterInfo GetMonsterInfo(PlayerObject _play,uint _item_id)
        {
            GameStruct.MonsterInfo MonsterInfo = null;

            GameStruct.RoleItemInfo item = _play.GetItemSystem().FindItem(_item_id);
            if (item == null)
            {
                Log.Instance().WriteLog("幻兽出征失败,无法找到道具id:" + _item_id.ToString());
                return null;
            }

            GameStruct.ItemTypeInfo baseitem = ConfigManager.Instance().GetItemTypeInfo(item.itemid);
            if (baseitem == null)
            {
                Log.Instance().WriteLog("幻兽出征失败,无法找到道具id:code:1" + item.itemid.ToString());
                return null;
            }
            MonsterInfo = ConfigManager.Instance().GetMonsterInfo(baseitem.monster_type);
            if (MonsterInfo == null)
            {
                Log.Instance().WriteLog("幻兽出征失败,无法找到怪物idid:code:1" + baseitem.monster_type.ToString());
                return null;
            }
            return MonsterInfo;
        }
        //幻兽出征
        public void Battle()
        {
      
           if ( mMonsterInfo== null)
            {
                Log.Instance().WriteLog("幻兽出征失败,无法找到怪物idid:code:1" );
                return;
            }
            this.mGameMap = play.GetGameMap();
            this.GetGameMap().AddObject(this);
            short nNewX = (short)(play.GetCurrentX() );
            short nNewY = (short)(play.GetCurrentY() );
            this.SetPoint(nNewX, nNewY);
            this.RefreshVisibleObject();

        
            //要设置这个出征标记..发了这个就会出现召回 合体按钮了-。-  我也不知道为什么啦。
            PacketOut outpack = new PacketOut(play.GetGamePackKeyEx());
            outpack.WriteUInt16(24);
            outpack.WriteUInt16(2035);
            //outpack.WriteUInt32(mInfo.itemid);
            outpack.WriteUInt32(mInfo.GetTypeID());
            outpack.WriteUInt32(mMonsterInfo.id);
            outpack.WriteInt32(1);
            //byte[] data = { 202, 1, 253, 1, 253, 159, 138, 131 };
          //  outpack.WriteBuff(data);
            outpack.WriteInt16(this.GetCurrentX());
            outpack.WriteInt16(this.GetCurrentY());
            outpack.WriteUInt32(mInfo.GetTypeID());
            play.SendData(outpack.Flush());
            play.AddVisibleObject(this, true);
           

         //   play.LeftNotice("召唤幻兽" + mInfo.name);
            this.SetState(EUDEMONSTATE.BATTLE);
            this.SendEudemonInfo();
                //  play.SendData(eudemoninfo.GetBuffer());
            //出战后重新发送技能信息 快捷键才能使用.......奇葩！！！
            this.SendMagicInfo();
          
            
        }

        public void DeleteMagicInfo(ushort magicid)
        {
            for (int i = 0; i < mInfo.mListMagicInfo.Count; i++)
            {
                if (mInfo.mListMagicInfo[i].magicid == magicid)
                {
                    //未存到数据库的-直接删掉
                    if (mInfo.mListMagicInfo[i].id == 0)
                    {
                        mInfo.mListMagicInfo.RemoveAt(i);
                        break;
                    }
                    mInfo.mListMagicInfo[i].id = -1;
                    break;
                }
            }
            PacketOut outpack = new PacketOut();
            outpack.WriteInt16(28);
            outpack.WriteInt16(1010);
            outpack.WriteInt32(System.Environment.TickCount);
            outpack.WriteUInt32(this.GetTypeId());
            outpack.WriteInt16(this.GetCurrentX());
            outpack.WriteInt16(this.GetCurrentY());
            outpack.WriteInt32(0);
            outpack.WriteUInt16(magicid);
            outpack.WriteUInt16(0);
            outpack.WriteInt32(9585);
            play.SendData(outpack.Flush(), true);
        }
        //添加幻兽技能
        //magicid 技能id
        //magiclv 技能等级
        //exp     技能经验
        //ret  成功返回true 失败返回false
        public bool AddMagicInfo(ushort magicid, byte magiclv = 0, uint exp = 0)
        {

            GameStruct.MagicTypeInfo base_info = ConfigManager.Instance().GetMagicTypeInfo(magicid, magiclv);
            if (base_info == null) return false;
            bool bFind = false;
            for (int i = 0; i < mInfo.mListMagicInfo.Count; i++)
            {
                if (mInfo.mListMagicInfo[i].magicid == magicid)
                {
                    mInfo.mListMagicInfo[i].level = magiclv;
                    mInfo.mListMagicInfo[i].exp = exp;
                    bFind = true;
                    break;
                }
            }
            if (!bFind)
            {
                GameBase.Network.Internal.MagicInfo info = new GameBase.Network.Internal.MagicInfo();
                info.id = 0;
                info.level = magiclv;
                info.magicid = magicid;
                info.exp = exp;
                info.ownerid =(int)mInfo.id;
                mInfo.mListMagicInfo.Add(info);
            }
            NetMsg.MsgMagicInfo net_info = new NetMsg.MsgMagicInfo();
            net_info.id = this.GetTypeId();
            net_info.magicid = magicid;
            net_info.level = magiclv;
            net_info.exp = exp;
            play.SendData(net_info.GetBuffer(), true);
            return true;
        }
       
        public void SendEudemonInfo(PlayerObject _play = null)
        {
            if (mMonsterInfo == null) return;
            //合体状态与休息状态下不发--
            if (this.GetState() == EUDEMONSTATE.FIT || this.GetState() == EUDEMONSTATE.NROMAL) return;
            //刷新幻兽数据
            NetMsg.MsgEudemonBattleInfo battleinfo = new NetMsg.MsgEudemonBattleInfo();
            battleinfo.id = this.GetTypeId();
            GameStruct.RoleItemInfo role_item = play.GetItemSystem().FindItem(this.GetEudemonInfo().itemid);
            if (role_item == null) return;

            battleinfo.lookface = mMonsterInfo.lookface;;
            battleinfo.name = role_item.forgename;
            battleinfo.monsterid = mMonsterInfo.id;
            battleinfo.play_id = play.GetTypeId();
            battleinfo.life = mInfo.life;
            battleinfo.life_max = mInfo.life;
            battleinfo.x = this.GetCurrentX();
            battleinfo.y = this.GetCurrentY();
            battleinfo.dir = play.GetDir();
            battleinfo.wuxing =(byte) mInfo.wuxing;
            battleinfo.wuxing =(byte) EudemonWuXing.LEI;
            if (mInfo.quality == 0) //没进化的，不显示至尊圣兽
            {
                battleinfo.param4 = 0;
            }
            else
            {
                battleinfo.param4 = 69888;//0, 17, 1, 0 //至尊圣兽
            }
           
            int nStar = (int)(mInfo.quality / 100);
            battleinfo.star = nStar;
            //111为幻兽星级
            //186, 90, 16, 0 = 1071802 幻兽对应的物品id
            //54, 55, 191, 0
            //前面四个字节貌似与幻兽神等级有关？
            //17 幻兽单项属性
            //1 为排名
            byte[] data = {   44, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 60, 89, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Buffer.BlockCopy(data, 0, battleinfo.param2, 0, data.Length);
            if (_play != null)
            {
                _play.SendData(battleinfo.GetBuffer(),true);
            }
            else
            {
                this.BrocatBuffer(battleinfo.GetBuffer());
            }
            PacketOut outpack = null;
            if (this.GetAttr().bDie)
            {

                outpack = new PacketOut();
                outpack.WriteInt16(20);
                outpack.WriteInt16(1017);
                outpack.WriteUInt32(this.GetTypeId());
                outpack.WriteInt32(1);
                outpack.WriteInt32(26);
                outpack.WriteInt32(6);
                this.BrocatBuffer(outpack.Flush());
             
            }
           // {28,0,241,3,39,31,97,5,2,32,201,122,132,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            byte[] data1 = { 132, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            outpack = new PacketOut();
            outpack.WriteInt16(28);
            outpack.WriteInt16(1009);
            outpack.WriteUInt32(play.GetTypeId());
            outpack.WriteUInt32(this.GetTypeId());
            outpack.WriteBuff(data1);
            this.BrocatBuffer(outpack.Flush());
            //要有血量，不然就挂了
            //NetMsg.MsgEudemonInfo eudemoninfo = new NetMsg.MsgEudemonInfo();
            //eudemoninfo.id = mInfo.GetTypeID();
            //eudemoninfo.AddAttribute(GameStruct.EudemonAttribute.Life, mInfo.life);
            //eudemoninfo.AddAttribute(GameStruct.EudemonAttribute.Life_Max, mInfo.life);
            //this.BrocatBuffer(eudemoninfo.GetBuffer());
        }

        //发送给玩家刷新信息
        public void SendPlayRefreshInfo(PlayerObject play)
        {
            if (play.GetGameSession() == null) return;
            if (mMonsterInfo == null) return;
            //刷新幻兽数据
            NetMsg.MsgEudemonBattleInfo battleinfo = new NetMsg.MsgEudemonBattleInfo();
            battleinfo.Create(null, play.GetGamePackKeyEx());
            battleinfo.id = mInfo.GetTypeID();
            battleinfo.lookface = mMonsterInfo.lookface;
            battleinfo.name = mInfo.name;
            battleinfo.monsterid = mMonsterInfo.id;
            battleinfo.x = play.GetCurrentX();
            battleinfo.y = play.GetCurrentY();
            battleinfo.dir = play.GetDir();
            play.SendData(battleinfo.GetBuffer()); 
          

            //要有血量，不然就挂了
            NetMsg.MsgEudemonInfo eudemoninfo = new NetMsg.MsgEudemonInfo();
            eudemoninfo.Create(null, play.GetGamePackKeyEx());
            eudemoninfo.id = mInfo.GetTypeID();
            eudemoninfo.AddAttribute(GameStruct.EudemonAttribute.Life, mInfo.life);
            eudemoninfo.AddAttribute(GameStruct.EudemonAttribute.Life_Max, mInfo.life);
            play.SendData(eudemoninfo.GetBuffer()); 
        }
        public override void RefreshVisibleObject()
        {
            base.RefreshVisibleObject();
            foreach (BaseObject o in this.GetGameMap().GetAllObject().Values)
            {
                if (o.GetGameID() == this.GetGameID()) continue;
              //  if (o.GetGameID() == play.GetGameID()) continue;//宿主玩家不加入到列表
                if (o.type == OBJECTTYPE.NPC)
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
            //            if (mPlayObject.ContainsKey(o.GetGameID()))
            //            {

            //                mPlayObject.Remove(o.GetGameID());
            //                NetMsg.MsgClearObjectInfo clearobj = new NetMsg.MsgClearObjectInfo();
            //                clearobj.Create(null, o.GetGamePackKeyEx());
            //                clearobj.id = this.GetTypeId();
            //                o.SendData(clearobj.GetBuffer());
            //            }
            //        }
            //    }
            //}
        }

        public void FlyPlay()
        {
            this.ClearThis();
            short nNewX = (short)(play.GetCurrentX() - 2);
            short nNewY = (short)(play.GetCurrentY() - 2);
            this.SetPoint(nNewX, nNewY);
            this.SendEudemonInfo();
        }
        public bool Move(NetMsg.MsgMoveInfo move)
        {
            byte dir = (byte)((int)move.dir % 8);
            this.SetDir(dir);
            short nNewX = this.GetCurrentX();
            short nNewY = this.GetCurrentY();
           //作弊判断，与宿主距离不得超过格子范围 否则传送回身边
            if (Math.Abs(play.GetCurrentX() - this.GetCurrentX()) > GameBase.Config.Define.MAX_EUDEMON_PLAY_DISTANCE ||
                Math.Abs(play.GetCurrentY() - this.GetCurrentY()) > GameBase.Config.Define.MAX_EUDEMON_PLAY_DISTANCE)
            {
                nNewX = (short)(play.GetCurrentX());
                nNewY = (short)(play.GetCurrentY());
                this.SetPoint(nNewX, nNewY);
                this.SendEudemonInfo();
                return false;
            }
            
            nNewX += DIR._DELTA_X[dir];
            nNewY += DIR._DELTA_Y[dir];
            if (!mGameMap.CanMove(nNewX, nNewY))
            {
               // Log.Instance().WriteLog("非法封包..禁止走路！！x:" + nNewX.ToString() + "y:" + nNewY.ToString());
                return false;
            }
            //// 跑步模式的阻挡判断
            bool IsRun = false;
            if (move.ucMode >= DIR.MOVEMODE_RUN_DIR0 && move.ucMode <= DIR.MOVEMODE_RUN_DIR7 )
            {
                nNewX += DIR._DELTA_X[move.ucMode - DIR.MOVEMODE_RUN_DIR0];
                nNewY += DIR._DELTA_Y[move.ucMode - DIR.MOVEMODE_RUN_DIR0];
                IsRun = true;
                //if (!mGameMap.CanMove(nNewX, nNewY))
                //{
                //    return false;
                //}
            }
            GameStruct.Action action = new GameStruct.Action(GameStruct.Action.MOVE);
            if (IsRun) action.AddObject(move.ucMode);
            this.SetPoint(nNewX, nNewY);
            PushAction(action);
            return true;
        }


        protected override void ProcessAction_Move(GameStruct.Action act)
        {
            byte runvalue = 1;
            if (act.GetObjectCount() > 0)
            {
                runvalue = (byte)act.GetObject(0);
            }
            //取现有列表- 不在范围内的就通知客户端了
            foreach (RefreshObject refobj in this.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                if (obj.type == OBJECTTYPE.PLAYER)
                {
                  //  Console.WriteLine("距离:x"+Math.Abs(this.GetCurrentX() - obj.GetCurrentX()).ToString()+" y:"+Math.Abs(this.GetCurrentY() - obj.GetCurrentY()).ToString());
                    if (!obj.GetPoint().CheckVisualDistance(this.GetCurrentX(), this.GetCurrentY(), GameBase.Config.Define.MAX_EUDEMON_OTHER_PLAY_DISTANCE))
                    {
                        
                        NetMsg.MsgClearObjectInfo info = new NetMsg.MsgClearObjectInfo();
                        info.id = this.GetTypeId();
                        (obj as PlayerObject).SendData(info.GetBuffer(),true);
                        obj.GetVisibleList().Remove(this.GetGameID());
                    }
                }
            }
            this.RefreshVisibleObject();
            foreach(RefreshObject refobj in this.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                if (obj.type == OBJECTTYPE.PLAYER)
                {
                    if (!obj.GetVisibleList().ContainsKey(this.GetGameID()))
                    {
                        obj.AddVisibleObject(this, true);
                        this.SendEudemonInfo();
                    }
                    //if (this.GetVisibleList().ContainsKey(obj.GetGameID()))
                    //{
                    //    this.AddVisibleObject(obj, true);
                    //    this.SendEudemonInfo();
                    //}
                }
            }

            NetMsg.MsgMoveInfo move = new NetMsg.MsgMoveInfo();
            move.id = this.GetTypeId();
            move.x = this.GetCurrentX();
            move.y = this.GetCurrentY();
            move.dir = this.GetDir();
            move.ucMode = runvalue;
            this.BrocatBuffer(move.GetBuffer());

            //if (mRefreshList.Count > 0)
            //{
            //    foreach (BaseObject o in mRefreshList.Values)
            //    {
            //        switch (o.type)
            //        {

            //            case OBJECTTYPE.PLAYER:
            //                {

            //                    // if (o.GetGameID() == play.GetGameID()) continue;
            //                    this.SendMoveInfo(o, runvalue);
            //                    break;
            //                }
            //        }
            //    }
            //    mRefreshList.Clear();
            //}
        }

        public void SendMoveInfo(BaseObject obj, byte runValue)
        {

            //if (obj.type != OBJECTTYPE.PLAYER) return;
            //PlayerObject play = obj as PlayerObject;
            //if (obj.GetGameSession() == null) return; //已经断线
            //if (!play.GetPlayObjectList().ContainsKey(this.GetGameID()) ||
            //    !this.GetPlayObjectList().ContainsKey(play.GetGameID()))
            //{
            //    this.SendPlayRefreshInfo(play);
            //    play.GetPlayObjectList()[this.GetGameID()] = this;
            //    this.GetPlayObjectList()[play.GetGameID()] = play;
            //  //  Log.Instance().WriteLog("发送刷新幻兽信息！！");
            //    return;
            //}

            //存在可视列表就发移动消息
            NetMsg.MsgMoveInfo move = new NetMsg.MsgMoveInfo();
            move.Create(null, obj.GetGamePackKeyEx());
            move.id = this.GetTypeId();
            move.x = this.GetCurrentX();
            move.y = this.GetCurrentY();
            move.dir = this.GetDir();
            move.ucMode = runValue;
            obj.SendData(move.GetBuffer());
        }

        //召回幻兽
        public void ReCall()
        {
          //  this.ClearThis();
            //幻兽召回的特效
            PacketOut outpack = new PacketOut();
            outpack.WriteUInt16(28);
            outpack.WriteUInt16(1009);
            outpack.WriteUInt32(this.GetTypeId());
            outpack.WriteUInt32(this.GetTypeId());
            outpack.WriteInt32(32);
            outpack.WriteInt32(0);
            outpack.WriteInt32(0);
            outpack.WriteInt32(0);
            this.BrocatBuffer(outpack.Flush());
          //  play.SendData(outpack.Flush());

            //按钮要变回出征
            NetMsg.MsgEudemonTag eudemontag = new NetMsg.MsgEudemonTag();
           // eudemontag.Create(null, play.GetGamePackKeyEx());
            eudemontag.playerid = play.GetTypeId();
            eudemontag.eudemonid = this.GetTypeId();
            eudemontag.SetReCallTag();
            this.BrocatBuffer(eudemontag.GetBuffer());
         //   play.SendData(eudemontag.GetBuffer());
            //公告
            //play.LeftNotice("幻兽" + mInfo.name + "被召回！");
            this.SetState(EUDEMONSTATE.NROMAL);
        }

        //是否拥有技能
        public bool IsHaveMagic(ushort magicid)
        {
            for (int i = 0; i < this.GetEudemonInfo().mListMagicInfo.Count; i++)
            {
                GameBase.Network.Internal.MagicInfo info = this.GetEudemonInfo().mListMagicInfo[i];
                if (info.magicid == magicid)
                {
                    return true;
                }
            }
            return false;
        }
            //获取幻兽技能等级
        public ushort GetMagicLevel(ushort magicid)
        {
            for (int i = 0; i < this.GetEudemonInfo().mListMagicInfo.Count; i++)
            {
                GameBase.Network.Internal.MagicInfo info = this.GetEudemonInfo().mListMagicInfo[i];
                if (info.magicid == magicid)
                {
                    return info.level;
                }
            }
            return 0;
        }
        //幻兽魔法攻击
        public void MagicAttack(NetMsg.MsgAttackInfo info)
        {

            if (!this.IsHaveMagic((ushort)info.usType)) return;
            GameStruct.MagicTypeInfo typeinfo = ConfigManager.Instance().GetMagicTypeInfo(info.usType);
           //
            if (typeinfo == null) return;
            BaseObject targetobj = null;
            uint injured = 0;
            ushort magiclv = this.GetMagicLevel((ushort)info.usType);
            if (!CheckMagicAttackSpeed((ushort)info.usType, (byte)magiclv))
            {
                return;
            }
            switch (typeinfo.sort)
            {
                case GameStruct.MagicTypeInfo.MAGICSORT_ATTACK:
                    {
                        targetobj = this.GetGameMap().FindObjectForID(info.idTarget);
                        if (targetobj == null)
                        {
                            return;
                        }
                        if (targetobj.IsDie()) return;
                        if (targetobj.IsLock()) return; //被锁定了
                        byte bdir = DIR.GetDirByPos(this.GetCurrentX(), this.GetCurrentY(), targetobj.GetCurrentX(), targetobj.GetCurrentY());
                        this.SetDir(bdir);
                        //距离判断,防止非法封包
                        if (Math.Abs(this.GetCurrentX() - targetobj.GetCurrentY()) > typeinfo.distance &&
                            Math.Abs(this.GetCurrentY() - targetobj.GetCurrentY()) > typeinfo.distance)
                        { return; }
                        //连击技能
                        if (!play.CanPK(targetobj)) return;
                        
                        //单体魔法攻击
                        injured = BattleSystem.AdjustDamage(this, targetobj, true);
                        if (injured <= 0) injured = 1;
                        NetMsg.MsgMonsterMagicInjuredInfo magicattack = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicattack.time = System.Environment.TickCount;
                        magicattack.roleid = this.GetTypeId();
                        magicattack.role_x = this.GetCurrentX();
                        magicattack.role_y = this.GetCurrentY();

                        magicattack.monsterid = targetobj.GetTypeId();
                        magicattack.tag = 21;
                        magicattack.magicid = (ushort)info.usType;
                        magicattack.magiclv = magiclv;
                        this.BrocatBuffer(magicattack.GetBuffer());


                        NetMsg.MsgGroupMagicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                        //有轨迹的魔法--
                        magicattackex.SetSigleAttack(targetobj.GetTypeId());
                        magicattackex.nID = this.GetTypeId();
                        //magicattackex.nX = (short)info.usPosX;
                        //magicattackex.nY = (short)info.usPosY;

                        magicattackex.nMagicID = (ushort)info.usType;
                        magicattackex.nMagicLv = magiclv;
                        magicattackex.bDir = this.GetDir();
                        magicattackex.AddObject(targetobj.GetTypeId(), (int)injured);
                        this.BrocatBuffer(magicattackex.GetBuffer());


                        targetobj.Injured(this, injured, info);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_BOMB: //范围攻击 
                    {
                        byte bdir = DIR.GetDirByPos(this.GetCurrentX(), this.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(bdir);

                        NetMsg.MsgGroupMagicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                        magicattackex.nID = this.GetTypeId();
                        magicattackex.nX = this.GetCurrentX();
                        magicattackex.nY = this.GetCurrentY();
     
                        magicattackex.nMagicID = (ushort)info.usType;
                        magicattackex.nMagicLv = magiclv;
                        magicattackex.bDir = this.GetDir();

                        //被攻击的对象
                        List<BaseObject> list = this.GetBombVisibleObj(info);
                        if (list != null)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {

                                injured = BattleSystem.AdjustDamage(this, list[i], true);

                                list[i].Injured(this, injured, info);
                                magicattackex.AddObject(list[i].GetTypeId(), (int)injured);
                            }
                        }

                        byte[] msg = magicattackex.GetBuffer();

                        this.BrocatBuffer(msg);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_FAN: //扇形攻击
                    {

                        byte bdir = DIR.GetDirByPos(this.GetCurrentX(), this.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        this.SetDir(bdir);
                        NetMsg.MsgGroupMagicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                        magicattackex.nID = this.GetTypeId();
                        magicattackex.nX = this.GetCurrentX();
                        magicattackex.nY = this.GetCurrentY();
                        magicattackex.nMagicID = (ushort)info.usType;
                        magicattackex.nMagicLv = magiclv;
                        magicattackex.bDir = bdir;
                        //被攻击的对象
                        List<BaseObject> list = this.GetFanVisibleObj(info);
                        if (list != null)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                injured = BattleSystem.AdjustDamage(this, list[i], true);
                                if (injured <= 0) injured = 1;
                                //怪物承受XP技能加倍伤害
                                if (list[i].type == OBJECTTYPE.MONSTER &&
                                    typeinfo.use_xp > 0)
                                {
                                    injured = injured * GameBase.Config.Define.XP_MULTIPLE;
                                }
                                list[i].Injured(this, injured, info);
                                magicattackex.AddObject(list[i].GetTypeId(), (int)injured);
                            }
                        }

                        byte[] msg = magicattackex.GetBuffer();
                        this.BrocatBuffer(msg);
                        break;
                    }
            }
        }

          //获取范围内的对象
        private List<BaseObject> GetBombVisibleObj(NetMsg.MsgAttackInfo magicinfo)
        {
            List<BaseObject> list_obj = new List<BaseObject>();
            GameStruct.MagicTypeInfo typeinfo = ConfigManager.Instance().GetMagicTypeInfo(magicinfo.usType);
            if (typeinfo == null) return list_obj;
            int nRange = (int)typeinfo.range;

            foreach (RefreshObject refobj in this.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                if (!play.GetFightSystem().IsAddMagicVisibleObj(obj))
                {
                    continue;
                }
                if (this.GetPoint().CheckVisualDistance(obj.GetCurrentX(), obj.GetCurrentY(), nRange))
                {
                    list_obj.Add(obj);
                }
            }
            return list_obj;
        }

        //获取扇形范围内的对象
        private List<BaseObject> GetFanVisibleObj(NetMsg.MsgAttackInfo magicinfo)
        {
            List<BaseObject> list_obj = new List<BaseObject>();
            GameStruct.MagicTypeInfo typeinfo = ConfigManager.Instance().GetMagicTypeInfo(magicinfo.usType);
            if (typeinfo == null) return list_obj;

            int nRange = (int)typeinfo.range + GameBase.Config.Define.MAX_SIZEADD;
            int nSize = nRange * 2 + 1;
            int nWidth = (int)typeinfo.width;
            foreach (RefreshObject refobj in this.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                if (!play.GetFightSystem().IsAddMagicVisibleObj(obj))
                {
                    continue;
                }

                GameStruct.Point pos = this.GetPoint();
                GameStruct.Point magicpos = new GameStruct.Point();
                magicpos.x = (short)magicinfo.usPosX;
                magicpos.y = (short)magicinfo.usPosY;
                if (this.GetPoint().CheckFanDistance(obj.GetPoint(), magicpos, nRange))
                {
                    list_obj.Add(obj);
                }
            }

            return list_obj;
        }
        //攻击
        public void Attack(NetMsg.MsgAttackInfo info)
        {

            if (!mAttackSpeed.ToNextTime())
            {
               return;
            }
          
           
            BaseObject targetobj = play.GetGameMap().FindObjectForID(info.idTarget);
            if (targetobj == null)
            {
                return;
            }
            if (mMonsterInfo == null) return;
            if (targetobj.IsDie()) return;
            if (targetobj.IsLock()) return; //被锁定了
            if (mInfo.bDie) return; //死亡 防作弊
            //与怪物的距离判断--反作弊
            if (Math.Abs(this.GetCurrentX() - targetobj.GetCurrentX()) > mMonsterInfo.range &&
                Math.Abs(this.GetCurrentY() - targetobj.GetCurrentY()) > mMonsterInfo.range)
            {
                return;
            }
            if (targetobj.type == OBJECTTYPE.PLAYER)
            {
                if (!play.CanPK(targetobj)) return;
            }
            if (targetobj.type == OBJECTTYPE.EUDEMON)
            {
                if (!play.CanPK((targetobj as EudemonObject).GetOwnerPlay())) { return; }
            }
            uint injured = 0;
            //经验--
            //战士幻兽使用近战攻击.法师幻兽使用魔法攻击
            switch (mMonsterInfo.eudemon_type)
            {
                case GameBase.Config.Define.EUDEMON_TYPE_WARRIOR:
                case GameBase.Config.Define.EUDEMON_TYPE_WARRIOR_RIG:
                    {
                        injured = BattleSystem.AdjustDamage(this, targetobj);
                        NetMsg.MsgMonsterInjuredInfo injuredinfo = new NetMsg.MsgMonsterInjuredInfo();
                        injuredinfo.roleid = this.GetTypeId();
                        injuredinfo.role_x = this.GetCurrentX();
                        injuredinfo.role_y = this.GetCurrentY();
                        injuredinfo.injuredvalue = injured;
                        injuredinfo.monsterid = targetobj.GetTypeId();
                        injuredinfo.tag = 2;
                        byte[] msg = injuredinfo.GetBuffer();
                        this.BrocatBuffer(msg);
                        break;
                    }
                case GameBase.Config.Define.EUDEMON_TYPE_MAGE:
                case GameBase.Config.Define.EUDEMON_TYPE_MAGE_RID:
                    {

                        injured = BattleSystem.AdjustDamage(this, targetobj,true);
                        if (injured == 0) injured = 1;
                        //NetMsg.MsgMonsterMagicInjuredInfo magicattack = new NetMsg.MsgMonsterMagicInjuredInfo();
                        //magicattack.time = System.Environment.TickCount;
                        //magicattack.roleid = this.GetTypeId();
                        //magicattack.role_x = this.GetCurrentX();
                        //magicattack.role_y = this.GetCurrentY();
                        //magicattack.monsterid = targetobj.GetTypeId();
                        //magicattack.tag = 21;
                        //magicattack.magicid = 1;
                        //magicattack.magiclv = 0;
                        //this.BrocatBuffer(magicattack.GetBuffer());

                        NetMsg.MsgGroupMagicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                        //有轨迹的魔法--
                        magicattackex.SetSigleAttack(targetobj.GetTypeId());
                        magicattackex.nID = this.GetTypeId();
                        //magicattackex.nX = (short)info.usPosX;
                        //magicattackex.nY = (short)info.usPosY;

                        magicattackex.nMagicID = 5000;
                        magicattackex.nMagicLv = 0;
                        magicattackex.bDir = this.GetDir();
                        magicattackex.AddObject(targetobj.GetTypeId(), (int)injured);
                        this.BrocatBuffer(magicattackex.GetBuffer());
                        break;
                    }
            }
           

            targetobj.Injured(this, injured, info);
        }

        //返回该幻兽的宿主对象
        public PlayerObject GetOwnerPlay()
        {
            return play;
        }

        //计算幻兽属性-
        public override void CalcAttribute()
        {
            this.GetAttr().life = (int)((this.GetAttr().init_life + (this.GetAttr().quality / 1000) + this.GetAttr().level) * this.GetAttr().life_grow_rate);
            this.GetAttr().life_max = this.GetAttr().life;

            //最小物攻
          this.GetAttr().atk_min = (int)((this.GetAttr().init_atk_min + (this.GetAttr().quality / 1000) + this.GetAttr().level) * this.GetAttr().phyatk_grow_rate);
            //最大物攻
          this.GetAttr().atk_max = (int)((this.GetAttr().init_atk_max + (this.GetAttr().quality / 1000) + this.GetAttr().level) * this.GetAttr().phyatk_grow_rate_max);
            //最小魔攻
          this.GetAttr().magicatk_min = (int)((this.GetAttr().init_magicatk_min + (this.GetAttr().quality / 1000) + this.GetAttr().level) * this.GetAttr().magicatk_grow_rate);
            //最大魔攻
          this.GetAttr().magicatk_max = (int)((this.GetAttr().init_magicatk_max + (this.GetAttr().quality / 1000) + this.GetAttr().level) * this.GetAttr().magicatk_grow_rate_max);
            //防御
          this.GetAttr().defense = (int)((this.GetAttr().init_defense + (this.GetAttr().quality / 1000) + this.GetAttr().level) * this.GetAttr().defense_grow_rate);
          //魔法防御
          this.GetAttr().magicdef = (int)((this.GetAttr().init_magicdef + (this.GetAttr().quality / 1000) + this.GetAttr().level) * this.GetAttr().magicdef_grow_rate);
      //  ibute.Atk_Max, info.atk_max);
        //    msg.AddAttribute(GameStruct.EudemonAttribute.Atk_Min, info.atk_min);
        //    msg.AddAttribute(GameStruct.EudemonAttribute.MagicAtk_Max, info.magicatk_max);
       //     msg.AddAttribute(GameStruct.EudemonAttribute.MagicAtk_Min, info.magicatk_min);
       //     msg.AddAttribute(GameStruct.EudemonAttribute.Defense, info.defense);
       //     msg.AddAttribute(GameStruct.EudemonAttribute.Magic_Defense, info.magicdef);
        }

        //死亡
        protected override void ProcessAction_Die(GameStruct.Action act)
        {

            this.GetAttr().life = 0;
            mInfo.bDie = true;
         //if (this.GetState() == EUDEMONSTATE.BATTLE)
            {
                NetMsg.MsgEudemonInfo msg = new NetMsg.MsgEudemonInfo();
                msg.id = this.GetTypeId();
                msg.AddAttribute(EudemonAttribute.Life, 0);
                play.SendData(msg.GetBuffer(), true);
                // 收到网络协议:长度：24协议号:2037
                //   byte[] data = {24,0,245,7,1,0,0,0,252,159,138,131,1,0,0,0,83,0,0,0,45,0,0,0};
                PacketOut outpack = new PacketOut();
                outpack.WriteUInt16(24);
                outpack.WriteUInt16(2037);
                outpack.WriteUInt32(1);
                outpack.WriteUInt32(this.GetTypeId());
                outpack.WriteInt32(1);
                outpack.WriteInt32(83);
                outpack.WriteInt32(45);
                play.SendData(outpack.Flush(), true);
                //收到网络协议:长度：20协议号:1017
                //{20,0,249,3,252,159,138,131,1,0,0,0,35,0,0,0,45,0,0,0}
                outpack = new PacketOut();
                outpack.WriteInt16(20);
                outpack.WriteInt16(1017);
                outpack.WriteUInt32(this.GetTypeId());
                outpack.WriteInt32(1);
                outpack.WriteInt32(35);
                outpack.WriteInt32(45);
                play.SendData(outpack.Flush(), true);
                //收到网络协议:长度：24协议号:2037
                //{24,0,245,7,1,0,0,0,252,159,138,131,1,0,0,0,8,0,0,0,149,0,0,0}
                outpack = new PacketOut();
                outpack.WriteUInt16(24);
                outpack.WriteUInt16(2037);
                outpack.WriteUInt32(1);
                outpack.WriteUInt32(this.GetTypeId());
                outpack.WriteInt32(1);
                outpack.WriteInt32(8);
                outpack.WriteInt32(149);
                play.SendData(outpack.Flush(), true);
            }
            //else 
            if (this.GetState() == EUDEMONSTATE.FIT)
            {
                //解体
                play.GetEudemonSystem().Eudemon_BreakUp(this.GetTypeId());
                //再召回
                play.GetEudemonSystem().Eudemon_Battle(this.GetTypeId());
            }


            if (this.GetState() == EUDEMONSTATE.BATTLE)
            {
                //广播
                this.SendEudemonInfo();
             }
       
        }


        public override void Injured(BaseObject obj, uint value, NetMsg.MsgAttackInfo info)
        {
            mbIsCombo = play.GetFightSystem().IsComboMagic(info.usType);
            this.GetAttr().life -= (int)value;
            if (this.GetAttr().life < 0)
            {
                this.GetAttr().life = 0;

            }
            if (!mbIsCombo && this.GetAttr().life <= 0)
            {
                GameStruct.Action action = new GameStruct.Action(GameStruct.Action.DIE, null);
                this.PushAction(action);

              
            }

            NetMsg.MsgEudemonInfo msg = new NetMsg.MsgEudemonInfo();
            msg.id = this.GetTypeId();
            msg.AddAttribute(EudemonAttribute.Life, this.GetAttr().life);
            this.BrocatBuffer(msg.GetBuffer());
            
        }

        protected override void ProcessAction_Injured(GameStruct.Action act)
        {
           //base.ProcessAction_Injured(act);
        }


        //发送技能信息
        public void SendMagicInfo()
        {
            for (int i = 0; i < mInfo.mListMagicInfo.Count; i++)
            {
                if (mInfo.mListMagicInfo[i].id == -1) return; //已删除的技能
                NetMsg.MsgMagicInfo info = new NetMsg.MsgMagicInfo();
                info.id = this.GetTypeId();
                info.magicid = (ushort)mInfo.mListMagicInfo[i].magicid;
                info.exp = (uint)mInfo.mListMagicInfo[i].exp;
                play.SendData(info.GetBuffer(), true);
            }
        }
        //更改幻兽属性
        //type 属性
        //value 值
        //isBrocat 是否广播
        public void ChangeAttribute(GameStruct.EudemonAttribute type, int value, bool isBrocat = true)
        {
            RoleData_Eudemon eudemon_info = play.GetEudemonSystem().FindEudemon(this.GetTypeId());
            if (eudemon_info == null) return;
            int v = value;
            switch (type)
            {
                case GameStruct.EudemonAttribute.Level:
                    {
                        eudemon_info.level += (short)value;
                        this.SetEudemonInfo(eudemon_info);
                       // this.GetAttr().level += (short)value;
                        this.CalcAttribute();

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
                        this.BrocatBuffer(outpack.Flush());
                        v = this.GetAttr().level;
                        break;
                    }
            }
            NetMsg.MsgEudemonInfo msg = new NetMsg.MsgEudemonInfo();

            msg.id = this.GetTypeId();
            msg.AddAttribute(type, v);
            if (isBrocat)
            {
                this.BrocatBuffer(msg.GetBuffer());
            }
            else
            {
                play.SendData(msg.GetBuffer(), true);
            }
           
        }
        public void AddExp(int nExp)
        {
            this.GetAttr().exp += nExp;
            bool bChangeLv = false;
            while (true)
            {
                GameStruct.LevelExp exp = ConfigManager.Instance().GetLevelExp(GameStruct.LevelExp.LEVELEXP_EUDEMON, (byte)this.GetAttr().level);
                if (exp == null) break;
                if (this.GetAttr().exp >= (int)exp.exp)
                {
                    this.GetAttr().exp -= (int)exp.exp;
                    this.GetAttr().level++;
                    bChangeLv = true;
                }
                else break;
            }

            if (bChangeLv)
            {  //升级特效
                PacketOut outpack = new PacketOut();
                outpack.WriteInt16(28);
                outpack.WriteInt16(1010);
                outpack.WriteInt32(System.Environment.TickCount);
                outpack.WriteUInt32(this.GetTypeId());
                outpack.WriteInt32(0);
                outpack.WriteInt32(0);
                outpack.WriteInt32(1);
                outpack.WriteInt32(9550);
                this.BrocatBuffer(outpack.Flush());
            }
            //下发经验数据
            NetMsg.MsgEudemonInfo msg = new NetMsg.MsgEudemonInfo();
            msg.id = this.GetTypeId();
            msg.AddAttribute(EudemonAttribute.Exp, this.GetAttr().exp);
            if (bChangeLv)
            {
                msg.AddAttribute(EudemonAttribute.Level, this.GetAttr().level);
                this.CalcAttribute();
                play.GetEudemonSystem().SendEudemonInfo(GetEudemonInfo());
            }
            play.SendData(msg.GetBuffer(), true);

        }

        public override int GetDefense()
        {
            return this.GetAttr().defense;
        }

        public override byte GetLevel()
        {
            return (byte)this.GetAttr().level;
        }

        public override int GetLuck()
        {
            return this.GetAttr().luck;
        }

        public override int GetMagicAck()
        {
            return this.GetAttr().magicatk_min;
        }

        public override int GetMaxMagixAck()
        {
            return this.GetAttr().magicatk_max;
        }

        public override int GetMagicDefense()
        {
            return this.GetAttr().magicdef;
        }

        public override int GetMaxAck()
        {
            return this.GetAttr().atk_max;
        }
        public override int GetMinAck()
        {
            return this.GetAttr().atk_min;
        }
    }
}
