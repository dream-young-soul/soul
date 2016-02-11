using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;
using GameBase.Network;
//游戏机器人类
//2015.10.10
namespace MapServer
{
    //机器人军团
    public class RobotLegionManager
    {
        private static uint legion_start_id = 100000; //起始机器人军团id
        private static RobotLegionManager mInstance = null;
        public Dictionary<String, uint> mDicLegion;
        
        public static RobotLegionManager GetInstance()
        {
            if (mInstance == null)
            {
                mInstance = new RobotLegionManager();
            }
            return mInstance;
        }
        public RobotLegionManager()
        {
            mDicLegion = new  Dictionary<String, uint>();
           
           
        }

        public void CreateLegion(String legion_name)
        {
            if(mDicLegion.ContainsKey(legion_name))return;
            mDicLegion[legion_name] = legion_start_id;
            legion_start_id++;
        }

        public uint GetLegionId(String legion_name)
        {

            if (mDicLegion.ContainsKey(legion_name))
            {
                return mDicLegion[legion_name];
            }
            return 0;
         
        }
    }
    public class RobotObject : BaseObject
    {
        private GameStruct.RobotInfo mInfo;
        //private List<uint> mListPlay;
        public RobotObject()
        {
            type = OBJECTTYPE.ROBOT;
            typeid = IDManager.CreateTypeId(OBJECTTYPE.PLAYER);
           // mListPlay = new List<uint>();
        }

        //发送机器人的角色信息
        //play 玩家对象

        public void SendRobotInfo(PlayerObject play/*bool boRepeat = true*/)
        {
            
            uint legion_id = RobotLegionManager.GetInstance().GetLegionId(mInfo.legion_name);
            NetMsg.MsgRoleInfo role = new NetMsg.MsgRoleInfo();
            role.Create(null, play.GetGamePackKeyEx());
            role.role_id = this.GetTypeId();
            role.x = mInfo.x;
            role.y = mInfo.y;
            role.armor_id = mInfo.armor_id;
            role.wepon_id = mInfo.wepon_id;
            role.face_sex = role.face_sex1 = mInfo.lookface;
            role.dir = mInfo.dir;
            role.guanjue = mInfo.guanjue;
            role.hair_id = mInfo.hair;
            role.rid_id = mInfo.rid_id;
            role.str.Add(mInfo.name);
            
            //军团
            if (mInfo.legion_name.Length > 0)
            {
                role.legion_id = legion_id;
                role.legion_title = mInfo.legion_title;
                role.legion_place = mInfo.legion_place;
                role.legion_id1 = legion_id;
            }
            play.SendData(role.GetBuffer());
            //军团名称-
            if (legion_id > 0)
            {
                NetMsg.MsgLegionName legion = new NetMsg.MsgLegionName();
                legion.Create(null, play.GetGamePackKeyEx());
                legion.legion_id = legion_id;
                legion.legion_name = mInfo.legion_name;
                play.SendData(legion.GetBuffer());
                //if (boRepeat)
                //{
                //    mListPlay.Add(play.GetTypeId());
                //}
            }
            //取该玩家对象- 如果是王、女王、公爵则行礼
            //2015.11.21 遇到玩家就行礼
            //GameStruct.GUANGJUELEVEL lv = play.GetGuanJue();
            //if (lv == GameStruct.GUANGJUELEVEL.KING ||
            //    lv == GameStruct.GUANGJUELEVEL.QUEEN ||
            //    lv == GameStruct.GUANGJUELEVEL.DUKE
            //    )
            //{
          //      this.PlayFaceAcion(Define._ACTION_GENUFLECT, play);
           // }
            this.PlayFaceAcion(Define._ACTION_GENUFLECT, play);
        }

        public void SetRobotInfo(GameStruct.RobotInfo _info)
        {
             mInfo = _info;
             this.SetPoint(mInfo.x, mInfo.y);
        }
        public override bool Run()
        {
            //if (mListPlay.Count > 0)
            //{
            //    uint typeid = mListPlay[0];
            //    mListPlay.RemoveAt(0);
            //    PlayerObject play = UserEngine.Instance().FindPlayerObjectToTypeID(typeid);
            //    if (play != null)
            //    {
            //        this.SendRobotInfo(play, false);
            //    }
            //}
            return true;
        }
        public override void RefreshVisibleObject()
        {
            base.RefreshVisibleObject();
            foreach (BaseObject o in mGameMap.GetAllObject().Values)
            {
                if (o.GetGameID() == this.GetGameID()) continue;
                //机器人视野只有玩家
                if (o.type != OBJECTTYPE.PLAYER) continue;
                if (GetPoint().CheckVisualDistance(o.GetCurrentX(), o.GetCurrentY()))
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

        //播放动作-
        //action_id :动作id
        //play : 参数如果不为空，则只对该角色播放动作，否则广播
        public void PlayFaceAcion(uint action_id,PlayerObject play = null)
        {
            PacketOut outpack ;
            if(play == null)
            {
                outpack = new PacketOut();
            }else 
            {
                outpack = new PacketOut(play.GetGamePackKeyEx());
            }
            outpack.WriteUInt16(28);
            outpack.WriteUInt16(1010);
            outpack.WriteUInt32(0);
            outpack.WriteUInt32(this.GetTypeId());
            outpack.WriteUInt32(23855267);
            outpack.WriteUInt32(mInfo.dir);
            outpack.WriteUInt32(action_id);
            outpack.WriteUInt32(9530);
            byte[] data = outpack.Flush();
            if(play != null)
            {
                play.SendData(data);
                return;
            }
            //this.RefreshVisibleObject();
            foreach (RefreshObject o in mVisibleList.Values)
            {
                BaseObject obj = o.obj;
                if (obj.type == OBJECTTYPE.PLAYER)
                {
                    PlayerObject _play = obj as PlayerObject;
                    outpack = new PacketOut(_play.GetGamePackKeyEx());
                    outpack.WriteBuff(data);
                    _play.SendData(outpack.Flush());
                }
            }
        }
    }
}
