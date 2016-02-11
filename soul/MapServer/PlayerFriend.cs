using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Network.Internal;
//好友系统-- 
//2015.10.3
namespace MapServer
{
    public class PlayerFriend
    {
        PlayerObject play;
        List<RoleData_Friend> mList;
        public const int MAX_FRIEND_COUNT = 50;//最多五十个好友
       
        public PlayerFriend(PlayerObject _play)
        {
            play = _play;
            mList = new List<RoleData_Friend>();
        }

        public void SendFriendInfo(RoleData_Friend info, byte type = NetMsg.MsgFriendInfo.TYPE_FRIEND)
        {
            NetMsg.MsgFriendInfo data = new NetMsg.MsgFriendInfo();
            data.Create(null, play.GetGamePackKeyEx());
            data.playerid = info.friendid;
            data.type = type;
            data.name = info.friendname;
            PlayerObject obj = UserEngine.Instance().FindPlayerObjectToPlayerId((int)info.friendid);
            if (obj != null)
            {
                data.Online = 1;
                data.level = obj.GetBaseAttr().level;
                data.fightpower = (uint)obj.GetFightSoul();
            }
            play.SendData(data.GetBuffer());
        }
            //发送所有好友信息
        public void SendAllFriendInfo()
        {
            for (int i = 0; i < mList.Count; i++)
            {
                if (mList[i].id != -1)
                {
                    SendFriendInfo(mList[i]);
                }
                
            }
        }

        public void DB_Load(GameBase.Network.Internal.ROLEDATA_FRIEND info)
        {
            for (int i = 0; i < info.list_item.Count; i++)
            {
                mList.Add(info.list_item[i]);
            }
        }
        public void DB_Save()
        {
            GameBase.Network.Internal.ROLEDATA_FRIEND info = new GameBase.Network.Internal.ROLEDATA_FRIEND();
            info.playerid = play.GetBaseAttr().player_id;
            info.SetSaveTag();
            for (int i = 0; i < mList.Count; i++)
            {
                info.list_item.Add(mList[i]);
            }
            DBServer.Instance().GetDBClient().SendData(info.GetBuffer());

        }

        //广播所有好友信息
        public void BrocatMsg(byte type)
        {
            for (int i = 0; i < mList.Count; i++)
            {
                PlayerObject obj = UserEngine.Instance().FindPlayerObjectToPlayerId((int)mList[i].friendid);
                if (obj != null)
                {
                    NetMsg.MsgFriendInfo data = new NetMsg.MsgFriendInfo();
                    data.Create(null, obj.GetGamePackKeyEx());
                    data.fightpower = (uint)play.GetFightSoul();
                    data.level = play.GetBaseAttr().level;
                    data.playerid = (uint)play.GetBaseAttr().player_id;
                    data.name = play.GetName();
                    data.type = type;
                    obj.SendData(data.GetBuffer());
                }
            }
        }
        //请求加为好友
        public void RequestAddFriend(NetMsg.MsgFriendInfo info)
        {
            PlayerObject target = UserEngine.Instance().FindPlayerObjectToTypeID(info.playerid);
            if (target == null)
            {
                play.LeftNotice("对方已离线,无法添加好友！");
                return;
            }
            if (mList.Count >= MAX_FRIEND_COUNT)
            {
                play.LeftNotice("阁下好友人数已满,无法再次添加！");
                return;
            }
            for (int i = 0; i < mList.Count; i++)
            {
                if (mList[i].friendid == target.GetBaseAttr().player_id)
                {
                    play.ChatNotice(string.Format("{0}已经是你的好友了！", mList[i].friendname));
                    return;
                }
            }
            //发给被请求玩家
            NetMsg.MsgFriendInfo data = new NetMsg.MsgFriendInfo();
            data.Create(null, target.GetGamePackKeyEx());
            data.playerid = (uint)play.GetBaseAttr().player_id;
            data.fightpower = (uint)play.GetFightSoul();
            data.type = NetMsg.MsgFriendInfo.TYPE_ADDFRIEND;
            data.Online = 1;
            data.level = play.GetBaseAttr().level;
            data.name = play.GetName();
            target.SendData(data.GetBuffer());
           // target.GetFriendSystem().SetFriendTarget(play.GetTypeId()) ; 
            play.LeftNotice("已经发送结为好友的请求");
            target.LeftNotice(string.Format("血与火的洗礼证明了坚固的友谊，{0}对你报以信任的眼神，你是否接受？", play.GetName()));
        }


       // public void SetFriendTarget(uint typeid)
       // {
       //     mTargetId = typeid;
       // }
        //public uint GetFriendTarget() { return mTargetId; }
        //添加好友
        public void AddFriend(uint playerid,byte type,bool party =true)
        {
            PlayerObject target = UserEngine.Instance().FindPlayerObjectToPlayerId((int)playerid);
            if (target == null) return;
            for (int i = 0; i < mList.Count; i++)
            {
                if (mList[i].friendid == (uint)target.GetBaseAttr().player_id &&
                    mList[i].friendtype == type)
                {
                    if (mList[i].id != -1) //删除标识
                    {
                        return;
                    }
                    else
                    {
                        mList.RemoveAt(i);
                        break;
                    }
                    
                }
            }
   
            play.LeftNotice(string.Format("{0},{1}锸血为盟，宣誓从此将生死与共，永不背叛！", play.GetName(), target.GetName()));
            
            RoleData_Friend friend = new RoleData_Friend();
            friend.id = 0;
            friend.friendid = (uint)target.GetBaseAttr().player_id;
            friend.friendtype = type;
            friend.friendname = target.GetName();
            mList.Add(friend);
            SendFriendInfo(friend);

            if (party)
            {
                target.GetFriendSystem().AddFriend((uint)play.GetBaseAttr().player_id, type, false);
            }
        }

        //删除好友
        public void DeleteFriend(uint playerid,bool deleteparty = true)
        {

           
            RoleData_Friend friend = null;
            for (int i = 0; i < mList.Count; i++)
            {
                if (mList[i].friendid == playerid)
                {
                    friend = mList[i];
                    break;
                }

            }


            play.LeftNotice(string.Format("由于彼此间的友谊出现了无法修复的裂痕，【{0}】决定与【{1}】断绝好友关系", play.GetName(), friend.friendname));
            friend.id = -1;
            SendFriendInfo(friend, NetMsg.MsgFriendInfo.TYPE_KILL);
            if (deleteparty)
            {
             
                PlayerObject target = UserEngine.Instance().FindPlayerObjectToPlayerId((int)playerid);
                if (target == null) return;
                target.GetFriendSystem().DeleteFriend((uint)play.GetBaseAttr().player_id, false);
            }
      
        }
        
        //拒绝添加好友请求
        public void RefuseFriend(uint playerid)
        {
            PlayerObject target = UserEngine.Instance().FindPlayerObjectToPlayerId((int)playerid);
            if (target == null) return;
            target.LeftNotice("对方拒绝你的好友请求。");
        }

        public void GetFriendInfo(int playerid)
        {
            PlayerObject obj = UserEngine.Instance().FindPlayerObjectToPlayerId(playerid);
            if (obj == null) return;

            GameBase.Network.PacketOut outpack = new GameBase.Network.PacketOut(play.GetGamePackKeyEx());
            outpack.WriteUInt16(52);
            outpack.WriteUInt16(2033);
            outpack.WriteInt32(playerid);
            outpack.WriteUInt32(obj.GetBaseAttr().lookface);
            outpack.WriteByte(obj.GetBaseAttr().level);
            outpack.WriteByte(obj.GetBaseAttr().profession); //职业
            outpack.WriteInt32(0);
            outpack.WriteInt16(0);
            outpack.WriteByte(206);
            outpack.WriteByte(222);
            byte[] data = new byte[30];
            outpack.WriteBuff(data);
            play.SendData(outpack.Flush());
            
        }
    }



}

