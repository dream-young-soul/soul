using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using GameBase.Network.Internal;
using GameBase.Core;

//军团数据-
//2015.10.8
namespace DBServer
{
    public class Legion
    {
        private static Legion mInstance = null;
        public Dictionary<uint, LegionInfo> mDicInfo;
        public static Legion GetInstance()
        {
            if (mInstance == null)
            {
                mInstance = new Legion();
            }
            return mInstance;
        }
        public Legion()
        {
            mDicInfo = new Dictionary<uint, LegionInfo>();
        }
        //从数据库载入军团配置
        public void DB_Load()
        {

            LegionInfo info = null;
            String sql = string.Format("select * from cq_legion");
            MySqlCommand command = new MySqlCommand(sql, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (!reader.HasRows) break;
                info = new LegionInfo();
                info.id = reader.GetUInt32("id");
                info.name = reader.GetString("name");
                info.name = Coding.Latin1ToGB2312(info.name);
                info.title = reader.GetByte("member_title");
                info.leader_id = reader.GetInt32("leader_id");
                info.leader_name = reader.GetString("leader_name");
                info.leader_name = Coding.Latin1ToGB2312(info.leader_name);
                info.money = reader.GetInt64("money");
                info.notice = reader.GetString("notice");
                info.notice = Coding.Latin1ToGB2312(info.notice);
                mDicInfo[info.id] = info;
            }

            MysqlConn.Conn_Close();
            command.Dispose();
            //载入行会成员信息

            foreach (LegionInfo obj in mDicInfo.Values)
            {
                 sql = string.Format("select * from cq_legion_members where legion_id={0}",obj.id);
                 command = new MySqlCommand(sql, MysqlConn.GetConn());
                 MysqlConn.Conn_Open();
                 reader = command.ExecuteReader();
                 while (reader.Read())
                 {
                     if (!reader.HasRows) break;
                     LegionMember member = new LegionMember();
                     member.rank = reader.GetInt16("rank");
                     member.members_name = reader.GetString("members_name");
                     member.members_name = Coding.Latin1ToGB2312(member.members_name);
                     member.money = reader.GetInt64("money");
                     obj.list_member.Add(member);

                }
                 MysqlConn.Conn_Close();
                 command.Dispose();
            }
        }


        //创建军团
        public void CreateLegion(LegionInfo info,int player_id)
        {
            int legion_id = Data.CreateLegion(info);
            CreateLegion_Ret ret = new CreateLegion_Ret();
            if (legion_id != -1) ret.ret = 1;
            ret.play_id = player_id;
            ret.legion_id = legion_id;
            info.id = (uint)legion_id;

            mDicInfo[info.id] = info;

            LegionMember member = new LegionMember();
            member.money = info.money;
            member.members_name = info.leader_name;
            member.rank = GameBase.Config.Define.LEGION_PLACE_JUNTUANZHANG;
            member.money = info.money; //初始化的贡献度
            AddLegionMembers(info.id,member);
            ret.money = info.money;
            ret.boss_id = member.id;
            SessionManager.Instance().SendMapServer(0, ret.GetBuffer());
        }

        //加入成员
        public void AddLegionMembers(uint legion_id, LegionMember members)
        {
            if (!mDicInfo.ContainsKey(legion_id)) return;
            uint id = Data.UpdateLegionMembers(legion_id, members);
            if (id == 0) return;
            members.id = id;
            mDicInfo[legion_id].list_member.Add(members);

        }
        //更新军团
        public void UpdateLegion(LegionInfo info)
        {
            if (!mDicInfo.ContainsKey(info.id)) return;
            mDicInfo[info.id] = info;
            //更新军团长信息
            Data.UpdateLegion(info);
            //更新军团成员信息
            for (int i = 0; i < info.list_member.Count; i++)
            {
                if (info.list_member[i].boChange)
                {
                    Data.UpdateLegionMembers(info.id, info.list_member[i]);
                    info.list_member[i].boChange = false;
                }
               
            }
        }
        //发送军团数据给mapserver组
        public void SendData(int mapserverid = 0)
        {
            LEGIONINFO info = new LEGIONINFO();
            foreach (LegionInfo obj in mDicInfo.Values)
            {
                info.list_item.Add(obj);
            }
            byte[] data = info.GetBuffer();
            SessionManager.Instance().SendMapServer(mapserverid, data);
        }
    }
}
