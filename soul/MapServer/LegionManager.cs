using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Network.Internal;
using GameBase.Config;

//军团类-从dbserver数据库发下来的军团信息
//2015.10.8
namespace MapServer
{

    public class Legion
    {
        private LegionInfo mInfo;
        public LegionInfo GetBaseInfo() { return mInfo; }
        
        public void SetBaseInfo(LegionInfo info){mInfo = info;}
        public Legion()
        {

        }
    }
    public class LegionManager
    {
        private static LegionManager mInstance = null;
        private Dictionary<uint, Legion> mDicLegion;
        private List<LegionInfo> mListTemp;
        public static LegionManager Instance()
        {
            if (mInstance == null)
            {
                mInstance = new LegionManager();
            }
            return mInstance;
        }

        public LegionManager()
        {
            mDicLegion = new Dictionary<uint, Legion>();
            mListTemp = new List<LegionInfo>();
        }
        public void DB_Load(LEGIONINFO info)
        {
            mDicLegion.Clear();
            for (int i = 0; i < info.list_item.Count; i++)
            {
                Legion obj = new Legion();
                obj.SetBaseInfo(info.list_item[i]);
                mDicLegion[info.list_item[i].id] = obj;
            }
           
            Log.Instance().WriteLog("从DBserver加载军团数据库成功!");
        }

        public Legion GetLegion(uint id)
        {
            if (mDicLegion.ContainsKey(id))
            {
                return mDicLegion[id];
            }
            return null;
        }

        //创建军团
        public void CreateLegion(int player_id,String legion_name,String leader_name,byte title,long money,String notice)
        {
            //已经创建军团的过程中，返回
            for(int i = 0;i < mListTemp.Count;i++)
            {
                if (mListTemp[i].leader_id == player_id)
                {
                    return;
                }
            }
            LegionOption option = new LegionOption();
            option.SetCreateTag();
            option.player_id = player_id;
            option.mInfo.leader_id = player_id;
            option.mInfo.leader_name = leader_name;
            option.mInfo.name = legion_name;
            option.mInfo.money = money;
            option.mInfo.notice = notice;
            DBServer.Instance().GetDBClient().SendData(option.GetBuffer());

            LegionInfo info = new LegionInfo();
            info.leader_id = player_id;
            info.name = legion_name;
            info.leader_name = leader_name;
            info.money = money;
            info.notice = notice;
            mListTemp.Add(info);

        }
        //创建军团返回
        public void CreateLegion_Ret(CreateLegion_Ret info)
        {
           
            LegionInfo le = null;
            for(int i = 0;i < mListTemp.Count;i++)
            {
                if (mListTemp[i].leader_id == info.play_id)
                {
                    le = mListTemp[i];
                    le.id = (uint)info.legion_id;
                    mListTemp.Remove(le);
                    break;
                }
            }
            if(info == null || info.ret ==0 || le == null)return;
            PlayerObject play = UserEngine.Instance().FindPlayerObjectToPlayerId(info.play_id);
            if (play == null) return;

            //加入军团长
            LegionMember member = new LegionMember();
            member.boChange = true;
            member.members_name = play.GetName();
            member.money = info.money;
            member.id = info.boss_id;
            member.rank = GameBase.Config.Define.LEGION_PLACE_JUNTUANZHANG;
            le.list_member.Add(member);



            Legion l = new Legion();
            l.SetBaseInfo(le);
            mDicLegion[le.id] = l;
            play.GetLegionSystem().SetLegion(l,true);
           

           
            
         
        }
        public void UpdateLegionInfo(uint legion_id,int player_id)
        {
            if (!mDicLegion.ContainsKey(legion_id)) return;
            Legion info = mDicLegion[legion_id];

            //发送给dbserver 更新军团数据
            LegionOption option = new LegionOption();
            option.SetUpdateTag();
            option.player_id = player_id;
            option.mInfo = info.GetBaseInfo();
            DBServer.Instance().GetDBClient().SendData(option.GetBuffer());
        }

   
  

        public Legion GetPlayerLegion(String playername)
        {
            foreach (Legion obj in mDicLegion.Values)
            {
                for (int i = 0; i < obj.GetBaseInfo().list_member.Count; i++)
                {
                    if (obj.GetBaseInfo().list_member[i].members_name == playername)
                    {
                        return obj;
                    }
                }
            }
            return null;
        }

        public bool IsExist(String legionname)
        {
            foreach (Legion obj in mDicLegion.Values)
            {
                if (obj.GetBaseInfo().name == legionname)
                {
                    return true;
                }
            }
            return false;
        }

        //军团添加成员
        public void AddMember(uint legion_id,PlayerObject play)
        {
            //已经有军团了
            if(play.GetLegionSystem().IsHaveLegion())return;
            Legion legion = GetLegion(legion_id);
            if (legion == null) return;
            LegionMember member = new LegionMember();
            member.members_name = play.GetName();
            member.money = 0;
            member.rank = GameBase.Config.Define.LEGION_PLACE_PUTONGTUANYUAN;
            legion.GetBaseInfo().list_member.Add(member);
            play.GetLegionSystem().SetLegion(legion,true);
          
            UpdateLegionInfo(legion_id, play.GetBaseAttr().player_id);
        }

        //更改成员职位
        public void ChangeMemberPlace(uint legion_id, String play_name,short place)
        {
            Legion legion = GetLegion(legion_id);
            for (int i = 0; i < legion.GetBaseInfo().list_member.Count; i++)
            {
                if (legion.GetBaseInfo().list_member[i].members_name == play_name)
                {
                    legion.GetBaseInfo().list_member[i].rank = place;
                    break;
                }
            }
        }
        //退出军团
        public void QuitLegion(PlayerObject play)
        {
            Legion legion =  play.GetLegionSystem().GetLegion();
            if(legion == null)return;
            uint legion_id = legion.GetBaseInfo().id;
            play.GetLegionSystem().SetLegion(null,true);
            int player_id = play.GetBaseAttr().player_id;
            for (int i = 0; i < legion.GetBaseInfo().list_member.Count; i++)
            {
                if (legion.GetBaseInfo().list_member[i].members_name == play.GetName())
                {
                    legion.GetBaseInfo().list_member[i].id = 0;
                    break;
                }
            }
            this.UpdateLegionInfo(legion_id, player_id);
        }
    }

    
}
