using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Core;
using GameBase.Network;
using GameBase.Network.Internal;

namespace MapServer
{
    public class PlayerLegion
    {
        private PlayerObject play;
        private Legion legion;
        public Legion GetLegion() { return legion; }
        
        public PlayerLegion(PlayerObject _play)
        {
            legion = null;
            
            play = _play;
        }

        public void Init(Legion _legion = null)
        {
            if (_legion != null)
            {
                legion = _legion;
                return;
            }
            legion = LegionManager.Instance().GetPlayerLegion(play.GetName());
       
        }

        public LegionMember GetMember(String name)
        {
            for (int i = 0; i < legion.GetBaseInfo().list_member.Count; i++)
            {
                if (legion.GetBaseInfo().list_member[i].members_name == name)
                {
                    return legion.GetBaseInfo().list_member[i];
                }
            }
            return null;
        }
        public void SendLegionInfo()
        {
            //广播给周围的人刷新自身信息
            play.RefreshRoleInfo();
            NetMsg.MsgSelfLegionInfo self_info = new NetMsg.MsgSelfLegionInfo();
            self_info.Create(null, play.GetGamePackKeyEx());
            if (legion == null)
            {
                self_info.legion_name = "";
                self_info.legion_id = 0;
                self_info.money = 0;
                self_info.devote =0;
                self_info.place = 0;
                self_info.title = 0;
                self_info.title = 0;
                play.SendData(self_info.GetBuffer());

           
                 
                return;
            }

         
            //byte[] dat5 = { 108, 0, 82, 4, 246, 0, 0, 0, 144, 208, 3, 0, 144, 208, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 232, 3, 0, 0, 4/*！！！头衔*/, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 210, 176, 177, 200, 186, 243, 204, 236, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 246, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref dat5, dat5.Length);
            //play.SendData(dat5);
            //byte[] data10 = { 21, 0, 247, 3, 246, 0, 0, 0, 3, 0, 1, 8, 196, 234, 201, 217, 187, 196, 204, 198, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref data10, data10.Length);
            //play.SendData(data10);

            //return;
           
           
            self_info.legion_name = legion.GetBaseInfo().name;
            self_info.legion_id = legion.GetBaseInfo().id;
            self_info.money = (int)legion.GetBaseInfo().money;
            self_info.devote = GetDevote();
            self_info.place = GetPlace();
            self_info.title = legion.GetBaseInfo().title;
            play.SendData(self_info.GetBuffer());

            //收到网络协议:长度：21协议号:1015
            byte[] legion_name = Coding.GetDefauleCoding().GetBytes(legion.GetBaseInfo().name);
            PacketOut outpack = new PacketOut(play.GetGamePackKeyEx());
            outpack.WriteUInt16((ushort)(12 + legion_name.Length));
            outpack.WriteUInt16(1015);
            outpack.WriteUInt32(legion.GetBaseInfo().id);
            outpack.WriteUInt16(3);
            outpack.WriteByte(1);
            outpack.WriteString(legion.GetBaseInfo().name);
            outpack.WriteByte(0);

            play.SendData(outpack.Flush());
          
        }

        //获取军团贡献度
        public int GetDevote()
        {
            LegionMember member = GetMember(play.GetName());
            if (member == null) return 0;
            return (int)member.money;

        }
        public void SetLegion(Legion _legion,bool bSendData = false)
        {
            legion = _legion;
            if (bSendData)
            {
                SendLegionInfo();
            }
        }

        public bool IsHaveLegion()
        {
            return legion != null;
        }

        //更改军团称谓
        public void ChangeLegionTitle(byte title)
        {
            if (title >= GameBase.Config.Define.LEGION_JUNTUAN && 
                title <= GameBase.Config.Define.LEGION_HUIZHANG)
            {
                legion.GetBaseInfo().title = title;
                this.SendLegionInfo();
                LegionManager.Instance().UpdateLegionInfo(legion.GetBaseInfo().id,play.GetBaseAttr().player_id);
            }
          
        }

        //获取军团职位
        public short GetPlace()
        {
            LegionMember member = GetMember(play.GetName());
            if (member == null) return 0;
            return member.rank;
        }
    }
}
