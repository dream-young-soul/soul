using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;
//技能系统
//2015.8.29--
namespace MapServer
{


    public class PlayerMagic
    {
        PlayerObject play;
        private Dictionary<uint, GameStruct.RoleMagicInfo> mDicMagic;

        public Dictionary<uint, GameStruct.RoleMagicInfo> GetDicMagic() { return mDicMagic; }
        private bool mbLiuXingYunHuo = false;

        //是否有流星陨火技能
        public bool IsLiuXingYunHuo()
        { return mbLiuXingYunHuo;}

        //角色普通攻击间隔速度
        public GameBase.Core.TimeOut mNormalAttackSpeed;
        //间隔玩家普通攻击间隔
        public bool CheckAttackSpeed()
        {
            if (mNormalAttackSpeed.ToNextTime())
            {
                return true;
            }
         //   Log.Instance().WriteLog("攻击速度太快..延时中！！" + play.GetName() + "Delay:" + mNormalAttackSpeed.GetDelayMS().ToString());
            return false;
        }
        //角色移动间隔速度
        private GameBase.Core.TimeOut mMoveSpeed;

        //检测玩家移动间隔，true为可以移动 false禁止移动
        //11.11 先去掉限制。。会影响体验
        public bool CheckMoveSpeed()
        {
            //if (mMoveSpeed.ToNextTime())
            //{
            //    return true;
            //}
            return true;
            // Log.Instance().WriteLog("移动速度太快..延时中！！" + play.GetName()+"Delay:"+mMoveSpeed.GetDelayMS().ToString());
            // return false;
        }
        //设置移动速度-- 因为玩家有跑步与骑乘
        public void SetMoveSpeed(float fSpeed)
        {
            mMoveSpeed.SetInterval(fSpeed);
            mMoveSpeed.Update();
        }
        
        //角色魔法攻击间隔速度
        private List<GameBase.Core.TimeOut> mMagicAttackSpeed;
        public bool CheckMagicAttackSpeed(ushort magicid, byte magiclv)
        {
            GameStruct.MagicTypeInfo type = ConfigManager.Instance().GetMagicTypeInfo(magicid, magiclv);
 
            if (type == null) return false;
            if (type.delay_ms == 0) return true;
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
                    else bError = true;
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
        public PlayerMagic(PlayerObject _play)
        {
            play = _play;
            mDicMagic = new Dictionary<uint, GameStruct.RoleMagicInfo>();

            mNormalAttackSpeed = new GameBase.Core.TimeOut();
            mNormalAttackSpeed.SetInterval(GameBase.Config.Define.ROLE_ATTACK_SPEED);
            mNormalAttackSpeed.Update();
            mMoveSpeed = new GameBase.Core.TimeOut();
            mMoveSpeed.SetInterval(GameBase.Config.Define.ROLE_MOVE_SPEED);
            mMoveSpeed.Update();
            mMagicAttackSpeed = new List<GameBase.Core.TimeOut>();
        }


        public void AddMagicInfo(uint magidid, byte level, uint exp)
        {
            GameStruct.RoleMagicInfo magicinfo = new GameStruct.RoleMagicInfo();
            magicinfo.magicid = magidid;
            magicinfo.level = level;
            magicinfo.exp = exp;
            magicinfo.id = 0;
            mDicMagic[magidid] = magicinfo;
            SendMagicInfo(magicinfo);
            if (magidid == GameStruct.MagicTypeInfo.LIUXINGYUNHUO) mbLiuXingYunHuo = true;
        }
        public void AddMagicInfo(GameBase.Network.Internal.MagicInfo info)
        {
            GameStruct.RoleMagicInfo magicinfo = new GameStruct.RoleMagicInfo();
            magicinfo.magicid = info.magicid;
            magicinfo.level = info.level;
            magicinfo.exp = info.exp;
            magicinfo.id = info.id;
            mDicMagic[magicinfo.magicid] = magicinfo;
            if (magicinfo.magicid == GameStruct.MagicTypeInfo.LIUXINGYUNHUO) mbLiuXingYunHuo = true;
        }
        public void SendMagicInfo(GameStruct.RoleMagicInfo info)
        {
            NetMsg.MsgMagicInfo magicinfo = new NetMsg.MsgMagicInfo();
            magicinfo.Create(null, play.GetGamePackKeyEx());
            magicinfo.id = play.GetTypeId();
            magicinfo.magicid = (ushort)info.magicid;
            magicinfo.level = info.level;
            magicinfo.exp = info.exp;
            play.SendData(magicinfo.GetBuffer());
        }
        public void SendAllMagicInfo()
        {
            foreach (GameStruct.RoleMagicInfo info in mDicMagic.Values)
            {
                SendMagicInfo(info);
            }
        }

        public void DB_Save()
        {
            if (mDicMagic.Count <= 0) return;
            GameBase.Network.Internal.RoleData_Magic magic = new GameBase.Network.Internal.RoleData_Magic();
            magic.SetSaveTag();
            magic.ownerid = play.GetBaseAttr().player_id;
            foreach (GameStruct.RoleMagicInfo info in mDicMagic.Values)
            {
                GameBase.Network.Internal.MagicInfo item = new GameBase.Network.Internal.MagicInfo();
                item.id = info.id;
                item.magicid = info.magicid;
                item.level = info.level;
                item.exp = info.exp;
                magic.mListMagic.Add(item);
            }
                
            DBServer.Instance().GetDBClient().SendData(magic.GetBuffer());
        }

        public ushort GetMagicLevel(uint typeid)
        {
            if (mDicMagic.ContainsKey(typeid))
            {
                GameStruct.RoleMagicInfo info = mDicMagic[typeid];
                return info.level;
            }
            return 0;
        }

        ////增加技能经验
        public void AddMagicExp(uint typeid, uint exp)
        {
            if (mDicMagic.ContainsKey(typeid))
            {
                  GameStruct.MagicTypeInfo typeinfo = ConfigManager.Instance().GetMagicTypeInfo((uint)(typeid));
                  if (typeinfo == null) return;
                if(typeinfo.need_exp == 0)return;
                mDicMagic[typeid].exp += exp;
              
                
                if(mDicMagic[typeid].exp >= typeinfo.need_exp)
                {
                    mDicMagic[typeid].level ++;
                    mDicMagic[typeid].exp = 0;
                }
                SendMagicInfo(mDicMagic[typeid]);
            }
        }
        public bool isMagic(uint typeid)
        {
            return mDicMagic.ContainsKey(typeid);
        }
    }
}
