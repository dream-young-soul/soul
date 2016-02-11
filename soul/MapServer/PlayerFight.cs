using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStruct;
using GameBase.Config;
//战斗系统
//用于攻击怪物啊..pk啊之类的-- 
//2015.9.6 从playerObject移植过来的attack magicattack
namespace MapServer
{
    public class PlayerFight
    {
        PlayerObject play;
        BaseObject mAutoTarget; //自动普通攻击对象
        private int mnAutoAttackTick;
        private int mnLastAttackTick;

        private byte mnYanHunQiangIndex ; //焰魂枪·裂地三段斩
        private byte mnYanHunQiangExIndex; //焰魂枪·流焰四段斩
        private List<BaseObject> mListQiShiTuanGuard; //骑士团守护的五个对象

        private int mnLiuXingYunHuoCount = 0; //凝聚的流星陨火数量
        private GameBase.Core.TimeOut mLiuXingYunHuoTime;
        //移除骑士团守护的技能特效
        public void RemoveQiShiTuanGuardEffect()
        {
            if (mListQiShiTuanGuard == null) return;
            BaseObject obj = null;
            for (int i = 0; i < mListQiShiTuanGuard.Count; i++)
            {
                obj = mListQiShiTuanGuard[i];
                obj.ClearThis();
                obj.GetGameMap().RemoveObj(obj);
            }
            mListQiShiTuanGuard.Clear();
        }
        public PlayerFight(PlayerObject _play)
        {
            play = _play;
            mAutoTarget = null;
            mnAutoAttackTick = System.Environment.TickCount;
            mnLastAttackTick = System.Environment.TickCount;
            mnYanHunQiangIndex = mnYanHunQiangExIndex = 0;
            mListQiShiTuanGuard = null;
            mLiuXingYunHuoTime = null;
        }


        //被动技能
        public bool PassiveMagic(NetMsg.MsgAttackInfo info)
        {
            if (mAutoTarget == null) return false;
     
            if (IRandom.Random(1, 100) > 50) { return false; }
            byte bJob = play.GetJob();
            switch (bJob)
            {
                case JOB.WARRIOR: //战士被动技能
                    {
                     
                        //风斩、旋风斩、四连斩、六连斩
                        uint[] magic_id = { 1000, 1002, 1005, 1009 }; 
                      
                        List<GameStruct.RoleMagicInfo> list_magic = null;
                        GameStruct.MagicTypeInfo baseMagicInfo = null;
                        Dictionary<uint, GameStruct.RoleMagicInfo> DicInfo = play.GetMagicSystem().GetDicMagic();
                        foreach (GameStruct.RoleMagicInfo _Info in DicInfo.Values)
                        {
                           for(int i = 0;i < magic_id.Length;i++)
                            {
                               if(_Info.magicid == magic_id[i])
                               {
                                    if (list_magic == null) list_magic = new List<GameStruct.RoleMagicInfo>();
                                   list_magic.Add(_Info);
                                   break;
                               }
                              
                            }
                        }
                        if (list_magic == null) return false;
                        int nIndex = IRandom.Random(0, list_magic.Count);
                        baseMagicInfo = ConfigManager.Instance().GetMagicTypeInfo(list_magic[nIndex].magicid, list_magic[nIndex].level);
                        if (IRandom.Random(1, 100) < baseMagicInfo.percent)
                        {
                            info.usType = baseMagicInfo.typeid;
                            info.idTarget = mAutoTarget.GetTypeId();
                            MagicAttack(info);
                            return true;
                        }
                        break;
                    }
            }
          
            return false;
        }
        public void Attack(NetMsg.MsgAttackInfo info)
        {

            
            if (!play.GetMagicSystem().CheckAttackSpeed())
            {
                return;
            }
            BaseObject targetobj = play.GetGameMap().FindObjectForID(info.idTarget);
            if (targetobj == null)
            {
                return;
            }
            if (targetobj.IsDie()) return;
            if (targetobj.IsLock()) return; //被锁定了
            //与怪物的距离判断--
            if (Math.Abs(play.GetCurrentX() - targetobj.GetCurrentX()) > 3 &&
                Math.Abs(play.GetCurrentY() - targetobj.GetCurrentY()) > 3)
            {
                SetAutoAttackTarget(null);
                return;
            }
            SetAutoAttackTarget(targetobj);
            //触发被动技能
            if (PassiveMagic(info))
            {
                return;
            }
            uint injured = BattleSystem.AdjustDamage(play, targetobj);
            //经验--
            injured = BattleSystem.AdjustDamage(play, targetobj, true);
            NetMsg.MsgMonsterInjuredInfo injuredinfo = new NetMsg.MsgMonsterInjuredInfo();
            injuredinfo.roleid = play.GetTypeId();
            injuredinfo.role_x = play.GetCurrentX();
            injuredinfo.role_y = play.GetCurrentY();
            injuredinfo.injuredvalue = injured;
            injuredinfo.monsterid = targetobj.GetTypeId();
            injuredinfo.tag = 2;
            byte[] msg = injuredinfo.GetBuffer();
            // GetGameMap().BroadcastBuffer(this, msg);
            play.BroadcastBuffer(msg, true);
            targetobj.Injured(play, injured, info);

            
            play.CanPK(targetobj);
           // if (info.tag == 2) //普通攻击设置自动攻击对象
           // {
           //     targetObject = targetobj;
           // }
          //  lastattacktime = System.Environment.TickCount;
        }

        public void Run()
        {
            AutoAttack();

            //法师- 凝聚流星陨火
            if (play.GetJob() == JOB.MAGE)
            {
                if (play.GetMagicSystem().IsLiuXingYunHuo() 
                    && mnLiuXingYunHuoCount < Define.LIUXINGYUNHUO_MAX_COUNT
                    && play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_YUANSUZHANGKONG) != null)
                {
                    if (mLiuXingYunHuoTime == null)
                    {
                        mLiuXingYunHuoTime = new GameBase.Core.TimeOut();
                        mLiuXingYunHuoTime.SetInterval(Define.LIUXINGYUNHUO_TIME);
                       
                      
                        mnLiuXingYunHuoCount++;
                        play.ChangeAttribute(UserAttribute.LIUXINGYUNHUO, mnLiuXingYunHuoCount);
                    }
                    if (mLiuXingYunHuoTime.ToNextTime())
                    {
                        mnLiuXingYunHuoCount++;
                        play.ChangeAttribute(UserAttribute.LIUXINGYUNHUO, mnLiuXingYunHuoCount);
                    
                    }
                }
            }

        }

        //设置自动攻击对象
        public void SetAutoAttackTarget(BaseObject obj)
        {

            mAutoTarget = obj;
            mnAutoAttackTick = System.Environment.TickCount;
        }
        //自动普通攻击
        private void AutoAttack()
        {
            //清除自动攻击对象
            if (mAutoTarget != null)
            {
                if (mAutoTarget.IsDie()) mAutoTarget = null;
            }
            if (play.IsDie() || play.IsLock()) return;
            if (mAutoTarget != null && !mAutoTarget.IsLock())
            {
                if (System.Environment.TickCount - mnAutoAttackTick > 1500/*1.5秒攻击一次*/)
                {

                    NetMsg.MsgAttackInfo info = new NetMsg.MsgAttackInfo();
                    //触发被动技能
                    if (PassiveMagic(info))
                    {
                        return;
                    }
                    info.idTarget = mAutoTarget.GetTypeId();
                    Attack(info);
                    mnAutoAttackTick = System.Environment.TickCount;
                }
            
            }
        }
        public void MagicAttack(NetMsg.MsgAttackInfo info)
        {
           
            
            if (!play.GetMagicSystem().isMagic(info.usType)) return;
            //检测施法速度
            ushort magiclv = play.GetMagicSystem().GetMagicLevel(info.usType);
            if (!play.GetMagicSystem().CheckMagicAttackSpeed((ushort)info.usType, (byte)magiclv))
            {
                return;
            }
            //--------------------------------------------------------------------------
             //暗黑龙骑--焰魂枪·裂地三段斩
            if (info.usType == GameStruct.MagicTypeInfo.YANHUNQIANG_LIEDI)
            {
                uint nMagicId = info.usType + mnYanHunQiangIndex;
                //没有下一段技能
                if (!play.GetMagicSystem().isMagic(nMagicId))
                {
                    mnYanHunQiangIndex = 0;
                    nMagicId = info.usType;
                }
                info.usType = nMagicId;
                mnYanHunQiangIndex++;
                if (mnYanHunQiangIndex >= 3) mnYanHunQiangIndex = 0;
            }
            //--------------------------------------------------------------------------
            //暗黑龙骑- 焰魂枪·流焰 四段斩
            if (info.usType == GameStruct.MagicTypeInfo.YANHUNQIANG_LIUYAN)
            {
                uint nMagicId = info.usType + mnYanHunQiangExIndex;
                //没有下一段技能
                if (!play.GetMagicSystem().isMagic(nMagicId))
                {
                    mnYanHunQiangExIndex = 0;
                    nMagicId = info.usType;
                }
                info.usType = nMagicId;
                mnYanHunQiangExIndex++;
                if (mnYanHunQiangExIndex >= 4) mnYanHunQiangExIndex = 0;
            }
            //------------------------------------------------------------------------
            GameStruct.MagicTypeInfo typeinfo = ConfigManager.Instance().GetMagicTypeInfo(info.usType);
            if (typeinfo == null) return;
            uint injured = 0;
            BaseObject targetobj = null;
          
            //xp技能校验-
            if (typeinfo.use_xp > 0 && play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_XPFULL_ATTACK) == null) return;
            //加经验
            if (typeinfo.need_exp > 0 && play.GetBaseAttr().level >= typeinfo.need_level)
            {
                play.GetMagicSystem().AddMagicExp(info.usType, 1);
            }
            if (typeinfo.use_ep > 0 && play.GetBaseAttr().sp < typeinfo.use_ep)
            {
                return;
            }
            if (typeinfo.use_mp > 0 && play.GetBaseAttr().mana < typeinfo.use_mp) return;
            //消耗体力
            if (typeinfo.use_ep > 0 && play.GetBaseAttr().sp > typeinfo.use_ep)
            {
                play.ChangeAttribute(UserAttribute.SP, (int)-typeinfo.use_ep);
            }
            //消耗魔法
            if (typeinfo.use_mp > 0 && play.GetBaseAttr().mana > typeinfo.use_mp)
            {
                play.ChangeAttribute(UserAttribute.MANA, (int)-typeinfo.use_mp);
            }
            switch (typeinfo.sort)
            {
                case GameStruct.MagicTypeInfo.MAGICSORT_ATTACK:
                case GameStruct.MagicTypeInfo.MAGICSORT_JUMP_ATTACK: //跳斩单体攻击
                    {
                        targetobj = play.GetGameMap().FindObjectForID(info.idTarget);
                        if (targetobj == null)
                        {
                            return;
                        }
                        if (targetobj.IsDie()) return;
                        if (targetobj.IsLock()) return; //被锁定了
                        byte bdir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), targetobj.GetCurrentX(), targetobj.GetCurrentY());
                        play.SetDir(bdir);
                        //距离判断,防止非法封包
                        if (Math.Abs(play.GetCurrentX() - targetobj.GetCurrentY()) > typeinfo.distance &&
                            Math.Abs(play.GetCurrentY() - targetobj.GetCurrentY()) > typeinfo.distance)
                        { return; }
                        //连击技能
                        if (!play.CanPK(targetobj)) return;
                        if (IsComboMagic(typeinfo.typeid))
                        {
                            this.ComboMagic(info, targetobj);
                            //血袭- 增加buff与状态
                            if (info.usType == GameStruct.MagicTypeInfo.XUEXI)
                            {
                                play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_XUEXI, Define.XUEXI_TIME);
                            }
                            return;
                        }
                        //单体魔法攻击
                        injured = BattleSystem.AdjustDamage(play, targetobj, true);
                        //怪物承受XP技能加倍伤害
                        if (targetobj.type == OBJECTTYPE.MONSTER &&
                            typeinfo.use_xp > 0)
                        {
                            injured = injured * Define.XP_MULTIPLE;
                        }
                        NetMsg.MsgMonsterMagicInjuredInfo magicattack = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicattack.time = System.Environment.TickCount;
                        magicattack.roleid = play.GetTypeId();
                        magicattack.role_x = play.GetCurrentX();
                        magicattack.role_y = play.GetCurrentY();
     
                        magicattack.monsterid = targetobj.GetTypeId();
                        magicattack.tag = 21;
                        magicattack.magicid = (ushort)info.usType;
                        magicattack.magiclv = magiclv;
                        play.BroadcastBuffer(magicattack.GetBuffer(), true);


                        NetMsg.MsgGroupMagicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                        //有轨迹的魔法--
                        magicattackex.SetSigleAttack(targetobj.GetTypeId());
                        magicattackex.nID = play.GetTypeId();
                        //magicattackex.nX = (short)info.usPosX;
                        //magicattackex.nY = (short)info.usPosY;
                        
                        magicattackex.nMagicID = (ushort)info.usType;
                        magicattackex.nMagicLv = magiclv;
                        magicattackex.bDir = play.GetDir();
                        magicattackex.AddObject(targetobj.GetTypeId(),(int) injured);
                        play.BroadcastBuffer(magicattackex.GetBuffer(),true);
                      
                       
                        targetobj.Injured(play, injured, info);
                        ////跳斩单体攻击
                        //位置还没计算好..2015.10.27 暂时搁置
                        //位置已解决- 2015.11.1
                        if (typeinfo.sort == GameStruct.MagicTypeInfo.MAGICSORT_JUMP_ATTACK)
                        {
                            int nNewX = targetobj.GetCurrentX() - (DIR._DELTA_X[bdir] + DIR._DELTA_X[bdir]);
                            int nNewY = targetobj.GetCurrentY() - (DIR._DELTA_Y[bdir] + DIR._DELTA_Y[bdir]);
                            play.SetPoint((short)nNewX,(short) nNewY);
                        }
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_BOMB: //范围攻击 
                case GameStruct.MagicTypeInfo.MAGICSORT_JUMPBOMB: //跳斩范围攻击
                case GameStruct.MagicTypeInfo.MAGICSORT_POINTBOMB: //指定鼠标位置攻击
                    {
                         byte bdir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(bdir);
                        //亡灵巫师巫怒噬魂
                        if (typeinfo.typeid == GameStruct.MagicTypeInfo.WUNUSHIHUN)
                        {
                            if (targetobj == null)
                            {
                                return;
                            }
                            int nNewX = targetobj.GetCurrentX() - (DIR._DELTA_X[bdir] + DIR._DELTA_X[bdir]);
                            int nNewY = targetobj.GetCurrentY() - (DIR._DELTA_Y[bdir] + DIR._DELTA_Y[bdir]);
                            play.SetPoint((short)nNewX, (short)nNewY);
                            targetobj = play.GetGameMap().FindObjectForID(info.idTarget);
                           
                        }
                     

                        if (typeinfo.sort == GameStruct.MagicTypeInfo.MAGICSORT_JUMPBOMB)   //跳斩
                        {
                            if (Math.Abs(play.GetCurrentX() - info.usPosX) > typeinfo.distance &&
                               Math.Abs(play.GetCurrentY() - info.usPosY) > typeinfo.distance)
                            { return; }
                            play.SetPoint((short)info.usPosX, (short)info.usPosY); //跳斩嘛。先跳过去
                        }

                        NetMsg.MsgGroupMagicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                        magicattackex.nID = play.GetTypeId();
                        magicattackex.nX = play.GetCurrentX();
                        magicattackex.nY = play.GetCurrentY();
                        if (typeinfo.sort == GameStruct.MagicTypeInfo.MAGICSORT_POINTBOMB)
                        {
                           
                            magicattackex.nX = (short)info.usPosX;
                            magicattackex.nY = (short)info.usPosY;
                        }
                        magicattackex.nMagicID = (ushort)info.usType;
                        magicattackex.nMagicLv = magiclv;
                        magicattackex.bDir = play.GetDir();

                        //被攻击的对象
                        List<BaseObject> list = this.RefreshMagicVisibleObject(typeinfo.typeid,info);
                        if (list != null)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                
                                injured = BattleSystem.AdjustDamage(play, list[i], true);
                                //怪物承受XP技能加倍伤害
                                if (list[i].type == OBJECTTYPE.MONSTER &&
                                    typeinfo.use_xp > 0)
                                {
                                    injured = injured * Define.XP_MULTIPLE;
                                }
                                list[i].Injured(play, injured, info);
                                magicattackex.AddObject(list[i].GetTypeId(), (int)injured);
                            }
                        }
                
                        byte[] msg = magicattackex.GetBuffer();
                       
                        play.BroadcastBuffer(msg, true);
                        //血族 血雨旋涡
                        if (typeinfo.typeid == GameStruct.MagicTypeInfo.XUEYUXUANWO)
                        {
                            int nAddX = 0;
                            int nAddY = 0;
                                switch(bdir)
                                {
                                    case DIR.LEFT_DOWN:
                                    case DIR.RIGHT_UP:
                                        {
                                            nAddX = 10;
                                            nAddY = 15;
                                            break;
                                        }
                                    case DIR.LEFT:
                                    case DIR.UP:
                                    case DIR.RIGHT:
                                    case DIR.DOWN:
                                        {
                                            nAddY = 10;
                                            nAddX = 10;
                                            break;
                                        }
                                    case DIR.LEFT_UP:
                                    case DIR.RIGHT_DOWN:
                                        {
                                            nAddX = 15; nAddY = 10;
                                            break;
                                        }
                                }
                            int nNewX = play.GetCurrentX() + (DIR._DELTA_X[bdir] * nAddX);
                            int nNewY = play.GetCurrentY() + (DIR._DELTA_Y[bdir] * nAddY);
                            play.SetPoint((short)nNewX, (short)nNewY);
                        }
                        
                        break;
                    }

                case GameStruct.MagicTypeInfo.MAGICSORT_FAN: //扇形攻击
                    {
                        
                        byte bdir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(bdir);
                        NetMsg.MsgGroupMagicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                        magicattackex.nID = play.GetTypeId();
                        magicattackex.nX = play.GetCurrentX();
                        magicattackex.nY = play.GetCurrentY();
                        magicattackex.nMagicID = (ushort)info.usType;
                        magicattackex.nMagicLv = magiclv;
                        magicattackex.bDir = bdir;
                        //被攻击的对象
                        List<BaseObject> list = this.RefreshMagicVisibleObject(typeinfo.typeid, info);
                        if (list != null)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                injured = BattleSystem.AdjustDamage(play, list[i], true);
                                //怪物承受XP技能加倍伤害
                                if (list[i].type == OBJECTTYPE.MONSTER &&
                                    typeinfo.use_xp > 0)
                                {
                                    injured = injured * Define.XP_MULTIPLE;
                                }
                                list[i].Injured(play, injured, info);
                                magicattackex.AddObject(list[i].GetTypeId(), (int)injured);
                            }
                        }
         
                        byte[] msg = magicattackex.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_LINE: //直线型攻击
                    {
        

                        byte bByte = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(bByte);

                        NetMsg.MsgMonsterMagicInjuredInfo magicattack = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicattack.roleid = play.GetTypeId();
                        magicattack.role_x = play.GetCurrentX();
                        magicattack.role_y = play.GetCurrentY();
                        magicattack.tag = 21;
                        magicattack.magicid = (ushort)info.usType;
                        magicattack.magiclv = magiclv;
                        play.BroadcastBuffer(magicattack.GetBuffer(), true);
                      
                        
                        NetMsg.MsgGroupMagicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                        magicattackex.nID = play.GetTypeId();
                        magicattackex.nX = (short)info.usPosX;
                        magicattackex.nY = (short)info.usPosY;
                        magicattackex.nMagicID = (ushort)info.usType;
                        magicattackex.nMagicLv = magiclv;
                        magicattackex.bDir = play.GetDir();
                        List<BaseObject> list = this.RefreshMagicVisibleObject(typeinfo.typeid, info);
                        if (list != null)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                injured = BattleSystem.AdjustDamage(play, list[i], true);
                                //怪物承受XP技能加倍伤害
                                if (list[i].type == OBJECTTYPE.MONSTER &&
                                    typeinfo.use_xp > 0)
                                {
                                    injured = injured * Define.XP_MULTIPLE;
                                }
                                list[i].Injured(play, injured, info);
                                magicattackex.AddObject(list[i].GetTypeId(), (int)injured);
                            }
                        }
                        play.BroadcastBuffer(magicattackex.GetBuffer(), true);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_ATTACHSTATUS: //引诱
                    {
                        foreach (RefreshObject refobj in play.GetVisibleList().Values)
                        {
                            BaseObject obj = refobj.obj;
                            if (obj.type == OBJECTTYPE.MONSTER)
                            {
                                if ((obj as MonsterObject).GetAi().GetTargetObject() == null)
                                {
                                    (obj as MonsterObject).GetAi().SetAttackTarget(play);
                                }
                              
                            }
                        }
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg,true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                        
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_STEALTH:    //潜行
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                       
                        play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_STEALTH, Define.STEALTH_TIME);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_HIDEDEN://隐身
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                        if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_HIDDEN) != null)
                        {
                            play.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_HIDDEN);
                        }
                        else
                        {
                            play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_HIDDEN, Define.HIDEDEM_TIME);
                        }
                        
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_YUANSUZHANGKONG:        //法师 元素掌控
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                        if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_YUANSUZHANGKONG) != null)
                        {
                            play.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_YUANSUZHANGKONG);
                            //流星陨火要清空
                            mnLiuXingYunHuoCount = 0;
                            play.ChangeAttribute(UserAttribute.LIUXINGYUNHUO, mnLiuXingYunHuoCount);
                        }
                        else
                        {
                            play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_YUANSUZHANGKONG, 0);
                        }

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                       
                        
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_LIUXINGYUNHUO:  //法师 流星陨石
                    {
                        targetobj = play.GetGameMap().FindObjectForID(info.idTarget);
                        if (targetobj == null)
                        {
                            return;
                        }
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        if (mnLiuXingYunHuoCount <= 0) break;

                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        for (int i = 0; i < mnLiuXingYunHuoCount; i++)
                        {
                            injured = BattleSystem.AdjustDamage(play, targetobj, true);
                            NetMsg.MsgGroupMagicAttackInfo magicattackex = new NetMsg.MsgGroupMagicAttackInfo();
                            //有轨迹的魔法--
                            magicattackex.SetSigleAttack(targetobj.GetTypeId());
                            magicattackex.nID = play.GetTypeId();
                          
                            magicattackex.nMagicID = (ushort)info.usType;
                            magicattackex.nMagicLv = magiclv;
                            magicattackex.bDir = play.GetDir();
                            magicattackex.AddObject(targetobj.GetTypeId(), (int)injured);
                            play.BroadcastBuffer(magicattackex.GetBuffer(), true);

                            targetobj.Injured(play, injured, info);

                         
                        }
                            mnLiuXingYunHuoCount = 0;
                        play.ChangeAttribute(UserAttribute.LIUXINGYUNHUO, mnLiuXingYunHuoCount);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_JUYANSHENGDUN://法师 巨岩圣盾
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);

                       
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                        play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_JUYANSHENGDUN, 0);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_YUANSUZHAOHUAN: //法师 元素召唤
                    {
                        if (play.IsMountState()) play.TakeOffMount(0);
                        else play.TakeMount(0,Define.YUANSUZHAOHUAN_MOUNTID);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_GULINGQIYUE: //亡灵巫师 骨灵契约
                    {
                        if (play.IsMountState()) play.TakeOffMount(0);
                        else play.TakeMount(0,Define.GULINGQIYUE_MOUNTID);
                        break;
                     
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_ZHAOHUANWUHUAN:
                    {


                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = play.GetDir();
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                        if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_ZHAOHUANWUHUAN) != null)
                        {
                            play.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_ZHAOHUANWUHUAN);
                        }
                        else
                        {
                            play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_ZHAOHUANWUHUAN);
                        }
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_JIANGLINGZHOUYU:    //亡灵巫师- 降灵咒雨
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        EffectObject _effobj = new EffectObject(play, Define.JIANGLINGZHOUYU, 10, 14, Define.JIANGLINGZHOUYU_TIME, (short)info.usPosX, (short)info.usPosY);
                        play.GetGameMap().AddObject(_effobj);
                        _effobj.RefreshVisibleObject();
                        _effobj.SendInfo(play);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_ANSHAXIELONG:   //暗沙邪龙
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);
      
                        play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_ANSHAXIELONG);
                        
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_MINGGUOSHENGNV: //冥国圣女
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_MINGGUOSHENGNV);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_WANGNIANWULING: //亡念巫灵
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_WANGNIANWULING);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_SHENYUANELING: //深渊恶灵
                case GameStruct.MagicTypeInfo.MAGICSORT_DIYUXIEFU: //地狱邪蝠
                case GameStruct.MagicTypeInfo.MAGICSORT_SHIHUNWULING://蚀魂巫灵
                    {
                        targetobj = play.GetGameMap().FindObjectForID(info.idTarget);
                        if (targetobj == null)
                        {
                            return;
                        }
                        uint monster_id = Define.SHENYUANELING_MONSTER_ID;
                        if (typeinfo.sort == GameStruct.MagicTypeInfo.MAGICSORT_DIYUXIEFU)
                        {
                            monster_id = Define.DIYUXIEFU_MONSTER_ID;
                        }
                        else if (typeinfo.sort == GameStruct.MagicTypeInfo.MAGICSORT_SHIHUNWULING)
                        {
                            monster_id = Define.SHIHUNWULING_MONSTER_ID;
                        }
                        GameStruct.MonsterInfo monster_info = ConfigManager.Instance().GetMonsterInfo(monster_id);
                        if(monster_info == null)
                        {
                            Log.Instance().WriteLog("获取深渊恶灵怪物ID失败");
                            break;
                        }
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                        //play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_SHENYUANELING);

                        int nNewX = targetobj.GetCurrentX() - DIR._DELTA_X[play.GetDir()];
                        int nNewY = targetobj.GetCurrentY() - DIR._DELTA_Y[play.GetDir()];
                        MonsterObject Object_CALL = null;
                        if (typeinfo.sort == GameStruct.MagicTypeInfo.MAGICSORT_DIYUXIEFU)
                        {
                            Object_CALL = new DiYuXieFu(play, targetobj, (short)nNewX, (short)nNewY, play.GetDir(), monster_info.id, monster_info.ai);
                        }
                        else if (typeinfo.sort == GameStruct.MagicTypeInfo.MAGICSORT_SHIHUNWULING)
                        {
                            Object_CALL = new ShiHunWuLing(play, targetobj, (short)nNewX, (short)nNewY, play.GetDir(), monster_info.id, monster_info.ai);
                        }
                        else if (typeinfo.sort == GameStruct.MagicTypeInfo.MAGICSORT_SHENYUANELING)
                        {
                           Object_CALL= new ShenYuanELing(play, targetobj, (short)nNewX, (short)nNewY, play.GetDir(), monster_info.id, monster_info.ai);
                        }
                       
                        play.GetGameMap().AddObject(Object_CALL, null);
                       // Object_CALL.RefreshVisibleObject();
                        Object_CALL.Alive(false);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_DRAGON_MOLONGSHOUHU: //暗黑龙骑魔龙守护
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_MOLONGSHOUHU, Define.STATUS_MOLONGSHOUHU_TIME, true);
                        //添加buff图标

                        break;
                    }
     
                case GameStruct.MagicTypeInfo.MAGICSORT_DRAGON_QISHITUANSHOUHU: //骑士团守护
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        //创建四个守护骑士
                        GuardKnightObject obj = null;
                        GameStruct.MonsterInfo monster = ConfigManager.Instance().GetMonsterInfo(Define.GUARDKNIGHTID);
                        if (monster == null)
                        {
                            Log.Instance().WriteLog("创建守护骑士失败！！,无此怪物id!");
                            break;
                        }
                        if (mListQiShiTuanGuard == null)
                        {mListQiShiTuanGuard = new List<BaseObject>();}
                       // else
                       // {
                            RemoveQiShiTuanGuardEffect();
                       // }
                        short[] _x = { -5, -5, +5, +5 };
                        short[] _y = { +5, -5, -5, +5 };
                        byte[] _dir = { DIR.LEFT_DOWN, DIR.LEFT_UP, DIR.RIGHT_UP, DIR.RIGHT_DOWN };
                        for (int i = 0; i < _x.Length; i++)
                        {
                            short x = (short)(play.GetCurrentX() + _x[i]);
                            short y = (short)(play.GetCurrentY() + _y[i]);
                            
                            obj = new GuardKnightObject(play,  x, y, _dir[i], monster.id, monster.ai);
                            play.GetGameMap().AddObject(obj,null);
                            obj.RefreshVisibleObject();
                            obj.SendInfo(play);
                            mListQiShiTuanGuard.Add(obj);
                          
                            play.AddVisibleObject(obj, true);
                        }
                        //地面特效-
                        EffectObject _effobj = new EffectObject(play, Define.GUARDKNIGHT_EFFID, 10, 15, Define.GUARDKNIGHT_TIME, play.GetCurrentX(), play.GetCurrentY());
                        play.GetGameMap().AddObject(_effobj);
                        _effobj.RefreshVisibleObject();
                        _effobj.SendInfo(play);
                        mListQiShiTuanGuard.Add(_effobj);
                       
                        play.AddVisibleObject(_effobj, true);

                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_DRAGON_QISHITUANCHONGFENG: //骑士团冲锋
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                        //创建四个冲锋骑士
                        FightKnightObject obj = null;
                        GameStruct.MonsterInfo monster = ConfigManager.Instance().GetMonsterInfo(Define.FIGHTKNIGHTID);
                        if (monster == null)
                        {
                            Log.Instance().WriteLog("创建黑暗骑士失败！！,无此怪物id!");
                            break;
                        }
                      //先不管这个技能了---烦
                        short[] _x = { -5, -5, +5, +5 };
                        short[] _y = { +5, -5, -5, +5 };
                        byte[] _dir = { DIR.LEFT_DOWN, DIR.LEFT_UP, DIR.RIGHT_UP, DIR.RIGHT_DOWN };
                        for (int i = 0; i < Define.FIGHTKNIGHT_AMOUNT; i++)
                        {
                          
                            short x = (short)(play.GetCurrentX() + _x[i]);
                            short y = (short)(play.GetCurrentY()+ _y[i] );

                            obj = new FightKnightObject(x, y, play.GetDir(), monster.id, Define.FIGHTKNIGHT_TIME);
                            play.GetGameMap().AddObject(obj, null);
                            obj.RefreshVisibleObject();
                            obj.SendInfo(play);
                           
                            play.AddVisibleObject(obj, true);
                        }
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_MIXINSHU:   //血族 迷心术
                    {
                        byte dir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), (short)info.usPosX, (short)info.usPosY);
                        play.SetDir(dir);
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = dir;
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                        play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_MIXINSHU,Define.MIXINSHU_TIME);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_SINGLE_DANCING: //单人舞
                case GameStruct.MagicTypeInfo.MAGICSORT_DOUBLE_DANCING: //双人舞
                    {
                       
                        NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                        magicinfo.roleid = play.GetTypeId();
                        magicinfo.role_x = play.GetCurrentX();
                        magicinfo.role_y = play.GetCurrentY();
                        magicinfo.injuredvalue = 0;
                        magicinfo.monsterid = play.GetTypeId();
                        magicinfo.tag = 21;
                        magicinfo.magicid = (ushort)info.usType;
                        magicinfo.magiclv = magiclv;
                        byte[] msg = magicinfo.GetBuffer();
                        play.BroadcastBuffer(msg, true);

                        NetMsg.MsgMagicAttackInfo _info = new NetMsg.MsgMagicAttackInfo();
                        _info.id = _info.targetid = play.GetTypeId();
                        _info.magicid = (ushort)info.usType;
                        _info.level = magiclv;
                        _info.dir = play.GetDir();
                        msg = _info.GetBuffer();
                        play.BroadcastBuffer(msg, true);
                        play.SetDancing((short)info.usType);
                        break;
                    }
                
                    //{
                    //    break;
                    //}
                    //{
                    //    break;
                    //}
            }

     
           // lastattacktime = System.Environment.TickCount;
        }


        //连击技能
        private void ComboMagic(NetMsg.MsgAttackInfo info, BaseObject target)
        {
            byte[] msg = null;
            ushort magiclv = play.GetMagicSystem().GetMagicLevel(info.skillid);
            GameStruct.MagicTypeInfo baseinfo = ConfigManager.Instance().GetMagicTypeInfo(info.usType);
         
            //施法动作
            NetMsg.MsgMonsterMagicInjuredInfo magicinfo = new NetMsg.MsgMonsterMagicInjuredInfo();
            magicinfo.roleid = play.GetTypeId();
            magicinfo.role_x = play.GetCurrentX();
            magicinfo.role_y = play.GetCurrentY();
            magicinfo.injuredvalue = 0;
            magicinfo.monsterid = play.GetTypeId();
            magicinfo.tag = 21;
            magicinfo.magicid = (ushort)info.usType;
            magicinfo.magiclv = magiclv;
            msg = magicinfo.GetBuffer();
            play.BroadcastBuffer(msg);
            //play.GetGameMap().BroadcastBuffer(play, msg);
            int _locktime = ConfigManager.Instance().GetTrackTime(baseinfo.track_id);
            int _target_locktime = ConfigManager.Instance().GetTrackTime(baseinfo.track_id2);
         
            int trackcount = ConfigManager.Instance().GetTrackNumber(baseinfo.track_id);
            //锁定自己与目标
            play.Lock(_locktime);
            target.Lock(_target_locktime, target.type == OBJECTTYPE.PLAYER);
            //计算伤害值
             NetMsg.MsgMagicAttackInfo magicattack ;
            for (int i = 0; i < trackcount; i++)
            {
                ////如果是影轮回，就有几率穿透伤害
                //magicattack = new NetMsg.MsgMagicAttackInfo();
                //magicattack.id = play.GetTypeId();

                //magicattack.value = 0;
                //magicattack.magicid = (ushort)GameStruct.MagicTypeInfo.ZHENSHIDAJI;
                //magicattack.level = magiclv;
                //magicattack.targetid = target.GetTypeId();
                //msg = magicattack.GetBuffer();
                //play.BroadcastBuffer(msg, true);
               // target.Injured(play, injured, info);

                //优先攻击合体的幻兽
                uint target_id = target.GetTypeId();
                if (target.type == OBJECTTYPE.PLAYER)
                {
                    EudemonObject eudemon_obj = (target as PlayerObject).GetEudemonSystem().GetInjuredEudemon();
                    if (eudemon_obj != null)
                    {
                        target_id = eudemon_obj.GetTypeId();
                    }
                }
              
                magicattack = new NetMsg.MsgMagicAttackInfo();
                magicattack.id = play.GetTypeId();
                uint injured = BattleSystem.AdjustDamage(play, target);
                magicattack.value = injured;
                magicattack.magicid = (ushort)info.usType;
                magicattack.level = magiclv;
                magicattack.targetid = target_id;
                msg = magicattack.GetBuffer();
                play.BroadcastBuffer(msg, true);
                target.Injured(play, injured, info);
            }
          
            if (baseinfo.track_id > 0)
            {
                //取得攻击方向
                byte attackdir = GameStruct.DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), target.GetCurrentX(), target.GetCurrentY());
                play.SetDir(attackdir);
                target.SetDir(attackdir);
               
                NetMsg.MsgCombo combo = new NetMsg.MsgCombo();
                combo.CalcTag(info.usType, play, target);
                short x = 0;
                short y = 0;
                GameStruct.TrackInfo trackinfo = ConfigManager.Instance().GetTrackInfo(baseinfo.track_id);
                GameStruct.TrackInfo track2 = ConfigManager.Instance().GetTrackInfo(baseinfo.track_id2);
                for (int i = 0; i < trackcount; i++)
                {
                    //怪物
                    if (track2.step > 0)
                    {
                        if (GameStruct.DIR.GetNexPoint(target, ref x, ref y)) { target.SetPoint(x, y); }
                    }
                    //角色        
                    if (trackinfo.step > 0)
                    {
                        for (int j = 0; j < trackinfo.step;j++ )
                        {
                            if (GameStruct.DIR.GetNexPoint(play, ref x, ref y)) { play.SetPoint(x, y); }
                        }
                        
                    }
                  
                    combo.AddComboInfo(info.usType, play, target, trackinfo.action, track2.action);

                    trackinfo = ConfigManager.Instance().GetTrackInfo(trackinfo.id_next);
                    if (track2.id_next != 0)
                    {
                        track2 = ConfigManager.Instance().GetTrackInfo(track2.id_next);
                    }
                }
                msg = combo.GetBuffer();
                play.BroadcastBuffer(msg, true);

            }
        }


        public List<BaseObject> RefreshMagicVisibleObject(uint magicid,NetMsg.MsgAttackInfo magicinfo)
        {
            MagicTypeInfo info = ConfigManager.Instance().GetMagicTypeInfo(magicid);
            List<BaseObject> list = new List<BaseObject>();
            list.Clear();
            if (info == null) return list;
            short x = 0; short y = 0; 
            x = play.GetCurrentX();
            y = play.GetCurrentY();
            switch (info.sort)
            {
                case GameStruct.MagicTypeInfo.MAGICSORT_BOMB: //矩形范围攻击，以自身为原点-
                case GameStruct.MagicTypeInfo.MAGICSORT_JUMPBOMB: //跳斩- 
                    {

                        list = this.GetBombVisibleObj(magicinfo);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_FAN: //扇形攻击
                    {

                        list = this.GetFanVisibleObj(magicinfo);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_LINE:   //直线型攻击
                    {
                        list = this.GetLineVisibleObj(magicinfo);
                        break;
                    }
                case GameStruct.MagicTypeInfo.MAGICSORT_POINTBOMB: //鼠标指向范围攻击
                    {
                        list = this.GetPointBombVisibleObj(magicinfo);
                        break;
                    }
            }
           
            return list;
        }

        //获取扇形范围内的对象
        private List<BaseObject> GetFanVisibleObj(NetMsg.MsgAttackInfo magicinfo)
        {
            List<BaseObject> list_obj = new List<BaseObject>();
            GameStruct.MagicTypeInfo typeinfo = ConfigManager.Instance().GetMagicTypeInfo(magicinfo.usType);
            if (typeinfo == null) return list_obj;

            //play.RefreshVisibleObject();
            int nRange = (int)typeinfo.range + Define.MAX_SIZEADD;
            int	nSize		= nRange*2 + 1;
            int nWidth = (int)typeinfo.width;
            foreach (RefreshObject refobj in play.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                if (!IsAddMagicVisibleObj(obj))
                {
                    continue;
                }


                GameStruct.Point pos =play.GetPoint();
                GameStruct.Point magicpos = new GameStruct.Point();
                magicpos.x = (short)magicinfo.usPosX;
                magicpos.y = (short)magicinfo.usPosY;
                //原版代码翻译过来..不好使-- 就用了蹩脚的方法2015.10.26
               // GameStruct.Point posThis = new GameStruct.Point();
                //bool bFind = true;
                //for (int i = CutTrail(pos.x - nRange, 0); i <= pos.x + nRange && i < play.GetGameMap().mnWidth; i++)
                //{
                //    for (int j = CutTrail(pos.y - nRange, 0); j <= pos.y + nRange && j < play.GetGameMap().mnHeight; j++)
                //    {
                //        posThis.x = (short)i;
                //        posThis.y = (short)j;
                //        if (play.GetPoint().CheckFanDistance(posThis, pos, nRange, nWidth, magicpos))
                //        {
                //            int idx = POS2INDEX(posThis.x - pos.x + nRange, posThis.y - pos.y + nRange, nSize, nSize);
                //            int objIdx = POS2INDEX(obj.GetCurrentX() - pos.x + nRange, obj.GetCurrentY() - pos.y + nRange, nSize, nSize);
                //            if (idx == objIdx)
                //            {
                //                list_obj.Add(obj);
                //                bFind = true;
                //                break;
                //            }
                //            //inline int POS2INDEX(int x, int y, int cx, int cy) { return (x + y*cx); }

                //        }
                //        if (bFind)
                //        {
                //            break;
                //        }

                //    }
                //}
                //bFind = false;
               // posThis.x = (short)CutTrail(pos.x - nRange, 0);
               // posThis.y = (short)CutTrail(pos.y - nRange, 0);

                //if (play.GetPoint().CheckFanDistance(posThis, pos, nRange, nWidth, magicpos))
                if(play.GetPoint().CheckFanDistance(obj.GetPoint(),magicpos,nRange))
                {
                    list_obj.Add(obj);
                }
            }

            return list_obj;
        }

        private int  POS2INDEX(int x, int y, int cx, int cy) { return (x + y*cx); }
    
        private int CutTrail(int x, int y)
        {
            return x >= y ? x : y;
        }

        //是否可以加入到技能伤害列表
        //obj 检测到的对象
        public bool IsAddMagicVisibleObj(BaseObject obj)
        {
            if (obj.IsDie() ||   /*已死亡*/
                obj.IsLock() || //已锁定
                (obj.type != OBJECTTYPE.PLAYER &&
                obj.type != OBJECTTYPE.MONSTER))
            {
                return false;
            }
            if (!play.CanPK(obj,false)) return false;
            return true;
        }
        //获取鼠标范围点的对象
        private List<BaseObject> GetPointBombVisibleObj(NetMsg.MsgAttackInfo magicinfo)
        {
            List<BaseObject> list_obj = new List<BaseObject>();
            GameStruct.MagicTypeInfo typeinfo = ConfigManager.Instance().GetMagicTypeInfo(magicinfo.usType);
            if (typeinfo == null) return list_obj;
            int nRange = (int)typeinfo.range;
            GameStruct.Point point = new GameStruct.Point();
            point.x = (short)magicinfo.usPosX; point.y = (short)magicinfo.usPosY;
            foreach (RefreshObject refobj in play.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                if (!IsAddMagicVisibleObj(obj))
                {
                    continue;
                }
                if (point.CheckVisualDistance(obj.GetCurrentX(), obj.GetCurrentY(), nRange))
                {
                    list_obj.Add(obj);
                }
            }
            return list_obj;
        }
        //获取范围内的对象
        private List<BaseObject> GetBombVisibleObj(NetMsg.MsgAttackInfo magicinfo)
        {
            List<BaseObject> list_obj = new List<BaseObject>();
            GameStruct.MagicTypeInfo typeinfo = ConfigManager.Instance().GetMagicTypeInfo(magicinfo.usType);
            if (typeinfo == null) return list_obj;
            int nRange = (int)typeinfo.range;
            //play.RefreshVisibleObject();
            foreach (RefreshObject refobj in play.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                if (!IsAddMagicVisibleObj(obj))
                {
                    continue;
                }
                if (play.GetPoint().CheckVisualDistance(obj.GetCurrentX(), obj.GetCurrentY(), nRange))
                {
                    list_obj.Add(obj);
                }
            }
            return list_obj;
        }
        //取直线型内的对象-
        private List<BaseObject> GetLineVisibleObj(NetMsg.MsgAttackInfo magicinfo)
        {
            List<BaseObject> list_obj = new List<BaseObject>();
            GameStruct.MagicTypeInfo typeinfo = ConfigManager.Instance().GetMagicTypeInfo(magicinfo.usType);
            if (typeinfo == null) return list_obj;
            int nRange = (int)typeinfo.range;
            play.RefreshVisibleObject();
            GameStruct.Point point = play.GetPoint();
           // List<GameStruct.Point> setpoint = DDALine(point.x, point.y, (short)magicinfo.usPosX, (short)magicinfo.usPosY, nRange);
           // if (setpoint == null || setpoint.Count == 0) return list_obj;
            byte magicDir = DIR.GetDirByPos(play.GetCurrentX(),play.GetCurrentY(),(short)magicinfo.usPosX,(short)magicinfo.usPosY);
            foreach (RefreshObject refobj in play.GetVisibleList().Values)
            {
                BaseObject obj = refobj.obj;
                if (!IsAddMagicVisibleObj(obj))
                {
                    continue;
                }
                if (!play.GetPoint().CheckVisualDistance(obj.GetCurrentX(), obj.GetCurrentY(), nRange)) continue;

                byte targetDir = DIR.GetDirByPos(play.GetCurrentX(), play.GetCurrentY(), obj.GetCurrentX(), obj.GetCurrentY());
                if (targetDir == magicDir)
                {
                    list_obj.Add(obj);
                }
                //for (int i = 0; i < setpoint.Count; i++)
                //{
                //    if(setpoint[i].x == obj.GetCurrentX() &&
                //        setpoint[i].y == obj.GetCurrentY())
                //    {
                //        list_obj.Add(obj);
                //    }
                //}
            }
            return list_obj;
        }

        //private List<GameStruct.Point> DDALine(short x0, short y0, short x1, short y1, int nRange)
        //{
        //    List<GameStruct.Point> setpoint = null;
        //    if (x0 == x1 && y0 == y1) return setpoint;
        //    float scale = 1.0f*nRange/(float)Math.Sqrt((double)((x1-x0)*(x1-x0)+(y1-y0)*(y1-y0)));
        //    x1 = (short)(0.5f + scale * (x1 - x0) + x0);
        //    y1 = (short)(0.5f+scale*(y1-y0)+y0);

        //    setpoint = this.DDALineEx(x0, y0, x1, y1);
        //    return setpoint;
        //}

        //private List<GameStruct.Point> DDALineEx(short x0, short y0, short x1, short y1)
        //{
        //    List<GameStruct.Point> setpoint = new List<GameStruct.Point>();
        //    setpoint.Clear();
        //    if (x0 == x1 && y0 == y1)
        //        return null;
        //    int dx = x1 - x0;
        //    int dy = y1 - y0;
        //    int abs_dx = Math.Abs(dx);
        //    int abs_dy = Math.Abs(dy);
        //    if (abs_dx > abs_dy)
        //    {
        //        int _0_5 = abs_dx * (dy > 0 ? 1 : -1);
        //        int numerator = dy * 2;
        //        int denominator = abs_dx * 2;
        //        // x 增益
        //        if (dx > 0)
        //        {
        //            // x0 ++
        //            for (int i = 1; i <= abs_dx; i++)
        //            {
        //                GameStruct.Point point = new GameStruct.Point();
        //                point.x = (short)(x0 + i);
        //                point.y = (short)(y0 + ((numerator * i + _0_5) / denominator));
        //                setpoint.Add(point);
        //            }
        //        }
        //        else if (dx < 0)
        //        {
        //            // x0 --
        //            for (int i = 1; i <= abs_dx; i++)
        //            {
        //                GameStruct.Point point = new GameStruct.Point();
        //                point.x = (short)(x0 - i);
        //                point.y = (short)(y0 + ((numerator * i + _0_5) / denominator));
        //                setpoint.Add(point);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        int _0_5 = abs_dy * (dx > 0 ? 1 : -1);
        //        int numerator = dx * 2;
        //        int denominator = abs_dy * 2;
        //        // y 增益
        //        if (dy > 0)
        //        {
        //            // y0 ++
        //            for (int i = 1; i <= abs_dy; i++)
        //            {
        //                GameStruct.Point point = new GameStruct.Point();
        //                point.y = (short)(y0 + i);
        //                point.x = (short)(x0 + ((numerator * i + _0_5) / denominator));
        //                int nAmount = setpoint.Count;
        //                setpoint.Add(point);
        //            }
        //        }
        //        else if (dy < 0)
        //        {
        //            // y0 -- 
        //            for (int i = 1; i <= abs_dy; i++)
        //            {
        //                GameStruct.Point point = new GameStruct.Point();
        //                point.y = (short)(y0 - i);
        //                point.x = (short)(x0 + ((numerator * i + _0_5) / denominator));
        //                setpoint.Add(point);
        //            }
        //        }
        //    }
        //    return setpoint;
        //}
        //是否是连击技能
        public bool IsComboMagic(uint magic_id)
        {
            byte bJob = play.GetJob();
            //战士
           // if (bJob == JOB.WARRIOR)
            {
                
                if (magic_id == GameStruct.MagicTypeInfo.FEITIANZHAN ||
                    magic_id == GameStruct.MagicTypeInfo.FEITIANLIANZHAN ||
                    magic_id == GameStruct.MagicTypeInfo.SILIANZHAN ||
                    magic_id == GameStruct.MagicTypeInfo.LIULIANZHAN ||
                    magic_id == GameStruct.MagicTypeInfo.LONGHUNFENGBAO 
                    )
                {
                    return true;
                }
            }
            //血族
           // else if (bJob == JOB.BLOODCLAN)
            {
               if(magic_id == GameStruct.MagicTypeInfo.XUEYINGLUNHUI ||
                magic_id == GameStruct.MagicTypeInfo.XUEYINGQIANHUAN ||
                magic_id == GameStruct.MagicTypeInfo.XUEYINGXINGMANG ||
                magic_id == GameStruct.MagicTypeInfo.XUEXI ||
                magic_id == GameStruct.MagicTypeInfo.SHUNYINGJI   )
                     {
                         return true;
                     }
            }
            //亡灵巫师
            
            //else if(bJob == JOB.UNDEAD_MAGE)
            {

                if(magic_id == GameStruct.MagicTypeInfo.LIEHUNSHAN)
                {
                    return true;
                }

            }
            //暗黑龙骑
           // else if (bJob == JOB.DRAGONRIDE)
            {
                if(magic_id == GameStruct.MagicTypeInfo.LONGHUNFENGBAO ||
                    magic_id == GameStruct.MagicTypeInfo.LONGQIANGLIEHUN ||
                    magic_id == GameStruct.MagicTypeInfo.LONGQIANGSUIHUN ||
                    magic_id == GameStruct.MagicTypeInfo.LONGQIANGZANGHUN)
                {
                    return true;
                }
            }
            
           
       
            return false;
        }
        //如果可以pk该对象返回true
        //已改为放在playerObject 类
        //private bool CanPK(BaseObject obj)
        //{
        //    PlayerObject _play = null;
        //    if(obj.type == OBJECTTYPE.EUDEMON)
        //    {
        //        _play = (obj as EudemonObject).GetOwnerPlay();
               
        //    }
        //    if (obj.type == OBJECTTYPE.PLAYER)
        //    {
        //        _play = (obj as PlayerObject);
        //    }
        //    if (_play == null) return true;
        //    byte pkmode = play.GetBaseAttr().pk_mode;
        //    bool bCanPk = false;
        //    if (pkmode == Define.PK_MODE_FREE)
        //    {
        //        bCanPk = true;
        //    }
        //    if (pkmode == Define.PK_MODE_SAFE) return false; //安全pk模式
        //    if (pkmode == Define.PK_MODE_GUARD)
        //    {
        //        if(_play.GetPKSystem().IsPKing() ||
        //            _play.GetPKSystem().GetNameType() == Define.PK_NAME_BLACK)//蓝名 or 黑名
        //        {
        //            bCanPk = true;
        //        }
        //    }
        //    //对方已隐身
        //    if (_play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_HIDDEN) != null)
        //    {
        //        bCanPk = false;
        //    }


        //    return bCanPk;
        //}

        //是否不是在休闲状态
        public bool IsFighting()
        {
            bool bRet = play.GetPKSystem().IsPKing();
            if (!bRet)
            {
                return bRet;
            }
            bRet = System.Environment.TickCount - mnLastAttackTick > 10000/*十秒*/ ? false : true;
            return bRet;
        }

        public void SetFighting()
        {
            mnLastAttackTick = System.Environment.TickCount;
        }
    }
}
