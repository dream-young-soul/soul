using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;
//玩家定时的一些操作系统 //如:pk 闪蓝 减pk值 体力值恢复等等
//跨天登录回调-
//2015.9.4
namespace MapServer
{
    public class PlayerTimer
    {
        private PlayerObject play;
        private ulong mi64Effect; //状态
        private ulong mi64EffectEx; //状态扩展
        private MonsterObject mObject_CALL; //亡灵巫师召唤出来的三个对象对象[暗沙邪龙、亡念巫灵、冥国圣女]
        public int GetEffect(bool hi = true) 
        {
            if (hi)
            {
                return (int)((mi64Effect >> 32) & 0xFFFFFFFF); 
            }
          //低位
            return (int)(mi64Effect & 0xFFFFFFFFFFFFFFFF); 
        }
        public int GetEffectEx(bool hi = true)
        {
            if (hi)
            {
                return (int)((mi64EffectEx >> 32) & 0xFFFFFFFF);
            }
            //低位
            return (int)(mi64EffectEx & 0xFFFFFFFFFFFFFFFF);
        }
        //private static uint sp_updatetime = 2000;
        //private  int sp_lastupdatetime ;


 
        private GameBase.Core.TimeOut mXpTime; //XP定时器
        private int mnXpVal;        //xp值

        private GameBase.Core.TimeOut mSPTime; //体力值定时器



        private List<GameStruct.RoleStatus> mListStatus;
        public PlayerTimer(PlayerObject _play)
        {
            mi64Effect = 0;
            mi64EffectEx = 0;
            play = _play;
         
            mXpTime = new GameBase.Core.TimeOut();
            mXpTime.SetInterval(Define.XP_ADD_SECS);
            mXpTime.Update();
            mnXpVal = 0;

            mSPTime = new GameBase.Core.TimeOut();
            mSPTime.SetInterval(Define.SP_ADD_SECS);
            mSPTime.Update();
            mListStatus = new List<GameStruct.RoleStatus>();
            mObject_CALL = null;
         
 
        }

        public void Run()
        {

            //删掉过期的状态
            int nLastTick = System.Environment.TickCount;
            int i = mListStatus.Count - 1;
            while (i >= 0)
            {
                if (mListStatus[i].nTime != 0 && nLastTick - mListStatus[i].nLastTick > mListStatus[i].nTime)
                {
                    DeleteStatus(mListStatus[i].nStatus);
                }
                i--;
            }
 
                //跨天在线回调脚本 一分钟判断一次
                //if (System.Environment.TickCount - mnNewDayTick > 1000 * 60)
                // {
                //ScripteManager.Instance().NewDayAction(play);
                //mnNewDayTick = System.Environment.TickCount;
                //}
           
            //定时恢复sp
            this.ProcXPVal();
            //定时恢复sp
            if (mSPTime.ToNextTime() && play.GetBaseAttr().sp < play.GetBaseAttr().sp_max)
            {
                if (!play.IsDie() && !play.GetFightSystem().IsFighting())
                {
                    play.ChangeAttribute(GameStruct.UserAttribute.SP, Define.SP_ADD_VALUE);
                }
            }

           
          
        }



        public void XPFull(short magicid)
        {
            //if (xp_full)
            //{
            //    xp_value = 100;
            //    xp_lastupdatetime = System.Environment.TickCount;
            //    xp_full = false;
            //    xp_bup = false;
            //    xp_ing = true;
            //}
            //xp_magicid = magicid;

            //play.ChangeAttribute(GameStruct.UserAttribute.STATUS1, 64);
            if (this.QueryStatus(GameStruct.RoleStatus.STATUS_XPFULL)!= null ||
                GameServer.IsTestMode())
            {
       
                this.AddStatus(GameStruct.RoleStatus.STATUS_XPFULL_ATTACK, Define.XP_MAX_FULL_SECS);
                if (magicid == GameStruct.MagicTypeInfo.LEITINGWANJUN) //雷霆万钧- 飞行
                {
                    this.AddStatus(GameStruct.RoleStatus.STATUS_FLY, Define.XP_MAX_FULL_SECS);
                   
                }
                //黑龙舞 飞行
                if (magicid == GameStruct.MagicTypeInfo.HEILONGWU)
                {
                     this.AddStatus(GameStruct.RoleStatus.STATUS_HEILONGWU, Define.XP_MAX_FULL_SECS);
                     this.AddStatus(GameStruct.RoleStatus.STATUS_FLY, Define.XP_MAX_FULL_SECS);
                    
                }
            }
            
        
         
            
        }

        //true为可以使用xp技能 false为不能
        public bool isXPIng()
        {
            return true;
          //  return xp_ing;
        }
        //返回正在使用的xp技能
       // public short GetXPMagic() { return xp_magicid; }
        private void ProcXPVal()
        {
            if (play.IsDie() || 
                this.QueryStatus(GameStruct.RoleStatus.STATUS_XPFULL) != null)
            {
                mnXpVal = 0;
                //XP消失时间已到
                if (mXpTime.ToNextTime())
                {
                    mXpTime.SetInterval(Define.XP_ADD_SECS);
                    this.DeleteStatus(GameStruct.RoleStatus.STATUS_XPFULL);
                    play.ChangeAttribute(GameStruct.UserAttribute.XP, mnXpVal);
                }
                return;
            }
            //定时恢复xp
            if (mXpTime.ToNextTime() && 
                this.QueryStatus(GameStruct.RoleStatus.STATUS_XPFULL) == null &&
                this.QueryStatus(GameStruct.RoleStatus.STATUS_XPFULL_ATTACK) == null)
            {
                mnXpVal += Define.XP_ADD_VALUE;
                if (mnXpVal > Define.XP_MAX_USER) mnXpVal = Define.XP_MAX_USER;
                play.ChangeAttribute(GameStruct.UserAttribute.XP, mnXpVal);
                //XP满- 爆气
                if (mnXpVal >= Define.XP_MAX_USER)
                {
                    this.AddStatus(GameStruct.RoleStatus.STATUS_XPFULL,Define.XP_MAX_FULL_SECS);
                    mnXpVal = 0;
                    mXpTime.SetInterval(Define.XP_DROP_FULL_SECS);
                }
     

            }
        }

     
        //添加状态
        //nStatus 状态id
        //nTime   持续时间- 秒 0为无限时间
        //bCover  是否覆盖该状态
        public void AddStatus(int nStatus, int nTime = 0, bool bCover = true)
        {

            if (this.QueryStatus(GameStruct.RoleStatus.STATUS_DIE) != null)
            {
                return;
            }
            bool bAdd = false;
            bool bSendState = true;
            GameStruct.RoleStatus status = null;
            for (int i = 0; i < mListStatus.Count; i++)
            {
                status = mListStatus[i];
                if (status.nStatus == nStatus)
                {
                    if (bCover)
                    {
                        status.nTime = nTime * 1000;
                        status.nLastTick = System.Environment.TickCount;
                    }
                    else { status.nTime += nTime * 1000; }
                    bAdd = true;
                    break;
                }
            }
            if (!bAdd)
            {
                status = new GameStruct.RoleStatus();
                status.nStatus = nStatus;
                status.nTime = nTime * 1000;
                mListStatus.Add(status);
            }
            int nOldEff = GetEffect();
            switch (nStatus)
            {
                case GameStruct.RoleStatus.STATUS_XPFULL: //XP爆气-- 下降
                    {
                        //mi64Effect  &= ~ Define.KEEPEFFECT_XPFULL;
                        //play.ChangeAttribute(GameStruct.UserAttribute.STATUS1, 64,true);
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_XPFULL_ATTACK: //xp爆气- 技能被单击
                    {
                        this.DeleteStatus(GameStruct.RoleStatus.STATUS_XPFULL); //删除xp爆气buff
                        mXpTime.SetInterval(Define.XP_MAX_FULL_SECS);
                        mXpTime.Update();
                        mi64Effect |= Define.KEEPEFFECT_XPFULL;
                      
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_MOLONGSHOUHU: //魔龙守护
                    {
                        play.ChangeAttribute(GameStruct.UserAttribute.MOLONGSHOUHU_STATUS, 1,true);
                        ushort nMagicLv = play.GetMagicSystem().GetMagicLevel(GameStruct.MagicTypeInfo.MOLONGSHOUHU);
                        GameStruct.MagicTypeInfo magicInfo = ConfigManager.Instance().GetMagicTypeInfo(GameStruct.MagicTypeInfo.MOLONGSHOUHU, (byte)nMagicLv);
                        if (magicInfo != null)
                        {
                            //没有实际效果-- 待修正
                            //添加buff-
                            int nDefense = (int)magicInfo.power;
                            byte[] data1 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            GameBase.Network.PacketOut outpack = new GameBase.Network.PacketOut(play.GetGamePackKeyEx());
                            outpack.WriteUInt16(48);
                            outpack.WriteUInt16(1127);
                            outpack.WriteUInt32(play.GetTypeId());
                            outpack.WriteInt32(nTime);
                            outpack.WriteInt32(nDefense);
                            outpack.WriteBuff(data1);
                            play.SendData(outpack.Flush());
                        }
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_STEALTH: //潜行
                    {
                        mi64Effect |= Define.KEEPEFFECT_LURKER;
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_CRIME: //闪蓝
                    {
                        mi64Effect |= Define.KEEPEFFECT_CRIME;
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_RED: //红名
                    {
                        mi64Effect |= Define.KEEPEFFECT_RED;
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_BLOCK: //黑名
                    {
                        mi64Effect |= Define.KEEPEFFECT_DEEPRED;
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_FLY: //雷霆万钧单独处理.要把xp满的包发下去
                    {
                        mi64Effect |= Define.KEEPEFFECT_FLY;
                       
                        NetMsg.MsgUserAttribute msg = new NetMsg.MsgUserAttribute();
                        msg.role_id = play.GetTypeId();
                        msg.Create(null, null);
                        msg.AddAttribute(GameStruct.UserAttribute.STATUS, (uint)GetEffect(false));
                        msg.AddAttribute(GameStruct.UserAttribute.STATUS1, (uint)GetEffect());//这是xp满的标识
                        
                       
                        play.BroadcastBuffer(msg.GetBuffer(), true);
                        bSendState = false;
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_HIDDEN:
                    {
                        mi64Effect |= Define.KEEPEFFECT_HIDDEN;
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_HEILONGWU:
                    {
                        mi64Effect |= Define.KEEPEFFECT_HEILONGWU;
                    
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_YUANSUZHANGKONG:
                    {
                        //修改元素掌控状态
                        play.ChangeAttribute(GameStruct.UserAttribute.YUANSUZHANGKONG,512,true);
                       //添加buff
                        byte[] data1 = { 48, 0, 103, 4 };
                        byte[] data2 = { 128, 81, 1, 0, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0 };
                        GameBase.Network.PacketOut outpack = new GameBase.Network.PacketOut(play.GetGamePackKeyEx());
                        outpack.WriteBuff(data1);
                        outpack.WriteUInt32(play.GetTypeId());
                        outpack.WriteBuff(data2);
                        play.SendData(outpack.Flush());
                        bSendState = false;
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_JUYANSHENGDUN:    //法师 巨岩圣盾
                    {
                        byte[] data1 = { 48, 0, 103, 4 };
                        byte[] data2 = { 132, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0 };
                        GameBase.Network.PacketOut outpack = new GameBase.Network.PacketOut(play.GetGamePackKeyEx());
                        outpack.WriteBuff(data1);
                        outpack.WriteUInt32(play.GetTypeId());
                        outpack.WriteBuff(data2);
                        play.SendData(outpack.Flush());
                        bSendState = false;
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_ANSHAXIELONG://亡灵巫师- 暗杀邪龙
                    {
                        if (mObject_CALL != null)
                        {
                            mObject_CALL.ClearThis();
                        }  
                        GameStruct.MonsterInfo monster = ConfigManager.Instance().GetMonsterInfo(Define.ANSHAXIELONG_MONSTER_ID);
                        if (monster == null)
                        {
                            Log.Instance().WriteLog("创建暗沙邪龙失败！！,无此怪物id!");
                            break;
                        }

                        int nx = play.GetCurrentX()+GameStruct.DIR._DELTA_X[play.GetDir()];
                        int ny = play.GetCurrentY() + GameStruct.DIR._DELTA_Y[play.GetDir()];
                        mObject_CALL = new AnShaXieLongObject(play, (short)nx, (short)ny, play.GetDir(), monster.id, monster.ai);
                        play.GetGameMap().AddObject(mObject_CALL, null);
                        mObject_CALL.RefreshVisibleObject();
                        mObject_CALL.Alive(false);

                        break;
                    }
                case GameStruct.RoleStatus.STATUS_MINGGUOSHENGNV: //冥国圣女
                    {
                        if (mObject_CALL != null)
                        {
                            mObject_CALL.ClearThis();
                        }
                        GameStruct.MonsterInfo monster = ConfigManager.Instance().GetMonsterInfo(Define.MINGGUOSHENGNV_MONSTER_ID);
                        if (monster == null)
                        {
                            Log.Instance().WriteLog("创建冥国圣女失败！！,无此怪物id!");
                            break;
                        }

                        int nx = play.GetCurrentX() + GameStruct.DIR._DELTA_X[play.GetDir()];
                        int ny = play.GetCurrentY() + GameStruct.DIR._DELTA_Y[play.GetDir()];
                        mObject_CALL = new MingGuoShengNv(play, (short)nx, (short)ny, play.GetDir(), monster.id, monster.ai);
                        play.GetGameMap().AddObject(mObject_CALL, null);
                        mObject_CALL.RefreshVisibleObject();
                        mObject_CALL.Alive(false);
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_WANGNIANWULING: //亡念巫灵
                    {
                        if (mObject_CALL != null)
                        {
                            mObject_CALL.ClearThis();
                        }
                        GameStruct.MonsterInfo monster = ConfigManager.Instance().GetMonsterInfo(Define.WANGNIANWULONG_MONSTER_ID);
                        if (monster == null)
                        {
                            Log.Instance().WriteLog("创建亡念巫灵失败！！,无此怪物id!");
                            break;
                        }

                        int nx = play.GetCurrentX() + GameStruct.DIR._DELTA_X[play.GetDir()];
                        int ny = play.GetCurrentY() + GameStruct.DIR._DELTA_Y[play.GetDir()];
                        mObject_CALL = new WangNianWuLing(play, (short)nx, (short)ny, play.GetDir(), monster.id, monster.ai);
                        play.GetGameMap().AddObject(mObject_CALL, null);
                        mObject_CALL.RefreshVisibleObject();
                        mObject_CALL.Alive(false);
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_HUASHENWANGLING:
                    {
                        mi64EffectEx |= Define.KEEPEFFECT_HUASHENWULING;
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_HUASHENWUSHI:
                    {
                        DeleteStatus(GameStruct.RoleStatus.STATUS_HUASHENWANGLING);
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_ZHAOHUANWUHUAN:
                    {
                        mi64Effect |= Define.KEEPEFFECT_ZHAOHUANWUHUAN;
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_MIXINSHU:     //血族 迷心术
                    {
                        mi64EffectEx |= Define.KEEPEFFECT_MIXINSHU;

                        GameBase.Network.PacketOut outpack = new GameBase.Network.PacketOut(play.GetGamePackKeyEx());
                        //buff
                        byte[] data = { 120, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        outpack.WriteUInt16(48);
                        outpack.WriteUInt16(1127);
                        outpack.WriteUInt32(play.GetTypeId());
                        outpack.WriteBuff(data);
                        play.SendData(outpack.Flush());
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_XUEXI: //血族 血袭
                    {
                        mi64EffectEx |= Define.KEEPEFFECT_XUEXI;
                        GameBase.Network.PacketOut outpack = new GameBase.Network.PacketOut(play.GetGamePackKeyEx());
                        byte[] data = { 60, 0, 0, 0, 35, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        outpack.WriteUInt16(48);
                        outpack.WriteUInt16(1127);
                        outpack.WriteUInt32(play.GetTypeId());
                        outpack.WriteBuff(data);
                        play.SendData(outpack.Flush());
                        break;
                    }
                case GameStruct.RoleStatus.STATUS_DIE: //死亡状态
                    {
                        mi64Effect |= Define.KEEPEFFECT_DIE;
                        break;
                    }
 
            }
            if (bSendState)
            {
                this.SendState();
           //    play.ChangeAttribute(state, GetEffect(state == GameStruct.UserAttribute.STATUS1),true);
            }
           
            //if (GetEffect() != nOldEff)
            //{
            //    play.ChangeAttribute(state, GetEffect());
            //}
           

        }

        //发送状态信息
        public void SendState(PlayerObject _play = null)
        {


            NetMsg.MsgUserAttribute attr = new NetMsg.MsgUserAttribute();
       
            attr.role_id = play.GetTypeId();
            int hi = GetEffect();
            int lo = GetEffect(false);
           // if (lo > 0)
           // {
                attr.AddAttribute(GameStruct.UserAttribute.STATUS, (uint)lo);
           // }
           // if (hi > 0)
           // {
                attr.AddAttribute(GameStruct.UserAttribute.STATUS1, (uint)hi);
          //  }
            //新增扩展状态
                hi = GetEffectEx();
                lo = GetEffectEx(false);
                attr.AddAttribute(GameStruct.UserAttribute.STATUSEX, (uint)lo);

                if (_play != null)
                {
                    _play.SendData(attr.GetBuffer(),true);
                }
                else
                {
                    play.BroadcastBuffer(attr.GetBuffer(), true);
                }
           
        }
        //查询状态
        public GameStruct.RoleStatus QueryStatus(int nStatus)
        {
            for (int i = 0; i < mListStatus.Count; i++)
            {
                if (mListStatus[i].nStatus == nStatus)
                {
                    return mListStatus[i];
                }
            }
            return null;
        }


        //死亡与退出游戏后要删除的状态
        public void ExitGame()
        {
            Die_DeleteState();
        }
        public void Die_DeleteState()
        {
            this.DeleteStatus(GameStruct.RoleStatus.STATUS_XPFULL);
            this.DeleteStatus(GameStruct.RoleStatus.STATUS_XPFULL_ATTACK);
            this.DeleteStatus(GameStruct.RoleStatus.STATUS_FLY);
            this.DeleteStatus(GameStruct.RoleStatus.STATUS_HEILONGWU);
            this.DeleteStatus(GameStruct.RoleStatus.STATUS_STEALTH);
            this.DeleteStatus(GameStruct.RoleStatus.STATUS_HIDDEN);
            this.DeleteStatus(GameStruct.RoleStatus.STATUS_ANSHAXIELONG);
            this.DeleteStatus(GameStruct.RoleStatus.STATUS_MINGGUOSHENGNV);
            this.DeleteStatus(GameStruct.RoleStatus.STATUS_WANGNIANWULING);
            this.DeleteStatus(GameStruct.RoleStatus.STATUS_MIXINSHU);
        }
        //删除状态
        public void DeleteStatus(int nStatus)
        {
           // GameStruct.UserAttribute state = GameStruct.UserAttribute.STATUS;
            bool bFind = false;
            for (int i = 0; i < mListStatus.Count; i++)
            {
                if (mListStatus[i].nStatus == nStatus)
                {
                    mListStatus.RemoveAt(i);
                    bFind = true;
                    break;
                }
            }
            bool bSendState = true;
            if (bFind)
            {
                switch (nStatus)
                {
                    case GameStruct.RoleStatus.STATUS_XPFULL:
                        {
                            mnXpVal = 0;
                            //play.ChangeAttribute(GameStruct.UserAttribute.XP, mnXpVal);
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_XPFULL_ATTACK:
                        {
                            mi64Effect &= ~Define.KEEPEFFECT_XPFULL;
                            play.ChangeAttribute(GameStruct.UserAttribute.XP, 0);
                            mnXpVal = 0;
                            mXpTime.SetInterval(Define.XP_ADD_SECS);
                            mXpTime.Update();
                          //  state = GameStruct.UserAttribute.STATUS1;
                          
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_MOLONGSHOUHU: //魔龙守护
                        {
                            play.ChangeAttribute(GameStruct.UserAttribute.MOLONGSHOUHU_STATUS, 0);

                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_STEALTH: //潜行
                        {
                            mi64Effect &= ~Define.KEEPEFFECT_LURKER;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_FLY: //雷霆万钧 飞行
                        {
                            mi64Effect &= ~Define.KEEPEFFECT_FLY;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_HIDDEN:      //隐身
                        {
                            mi64Effect &= ~Define.KEEPEFFECT_HIDDEN;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_YUANSUZHANGKONG: //元素掌控
                        {
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_JUYANSHENGDUN://巨岩圣盾
                        {
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_HEILONGWU:
                        {
                            mi64Effect &= ~Define.KEEPEFFECT_HEILONGWU; //黑龙舞
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_ANSHAXIELONG://亡灵巫师- 暗杀邪龙
                        {
                            if (mObject_CALL != null)
                            {
                                mObject_CALL.ClearThis();
                            }
                            bSendState = false;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_MINGGUOSHENGNV: //冥国圣女
                        {
                            if (mObject_CALL != null)
                            {
                                mObject_CALL.ClearThis();
                            }
                            bSendState = false;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_WANGNIANWULING: //亡念巫灵
                        {
                            if (mObject_CALL != null)
                            {
                                mObject_CALL.ClearThis();
                            }
                            bSendState = false;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_CRIME: //闪蓝
                        {
                            mi64Effect &= ~Define.KEEPEFFECT_CRIME;
                            play.GetPKSystem().ResetPKNameType(); ;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_RED: //红名
                        {
                            mi64Effect &= ~Define.KEEPEFFECT_RED;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_BLOCK: //黑名
                        {
                            mi64Effect &= ~Define.KEEPEFFECT_DEEPRED;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_HUASHENWANGLING:
                        {
                            mi64EffectEx &= ~Define.KEEPEFFECT_HUASHENWULING;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_ZHAOHUANWUHUAN:
                        {
                            mi64Effect &= ~Define.KEEPEFFECT_ZHAOHUANWUHUAN;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_MIXINSHU:     //血族 迷心术
                        {
                            mi64EffectEx &= ~Define.KEEPEFFECT_MIXINSHU;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_XUEXI: //血族 血袭
                        {
                            mi64EffectEx &= ~Define.KEEPEFFECT_XUEXI;
                            break;
                        }
                    case GameStruct.RoleStatus.STATUS_DIE://死亡
                        {
                            mi64Effect &= ~Define.KEEPEFFECT_DIE;
                            break;
                        }
                }
                if (bSendState)
                {
                    this.SendState();
                }
                
               // play.ChangeAttribute(state, GetEffect(state == GameStruct.UserAttribute.STATUS1), true);
            }
     
        }
       
        
    }
}
