using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapServer
{
  
    //队伍全局管理
    //2015.10.13
    public class TeamManager
    {
        private static TeamManager mInstance = null;
        private uint mID;
        private List<Team> mListTeam;
       
        public static TeamManager Instance()
        {
            if (mInstance == null)
            {
                mInstance = new TeamManager();
            }
            return mInstance;
        }
        public TeamManager()
        {
            mID = 1000;
            mListTeam = new List<Team>();
           
        }

        //创建队伍.返回队伍id
        public  Team CreateTeam()
        {
            mID++;
            Team team = new Team(mID);
            mListTeam.Add(team);
            mListTeam.Add(team);
            return team;
        }


        //删除队伍
        public void DeleteTeam(uint teamid)
        {
            for (int i = 0; i < mListTeam.Count; i++)
            {
                if (mListTeam[i].GetTeamID() == teamid)
                {
                    mListTeam.RemoveAt(i);
                    break;
                }
            }
        }
    }

      public class Team
      {
          const int MAX_TEAM_COUNT = 5;//队伍最大人数
          private uint mID;
          private List<PlayerObject> mlistMember;
          public Team(uint id)
          {
              mID = id;
              mlistMember = new List<PlayerObject>();
          }

          public uint GetTeamID()
          {
              return mID;
          }
          //队伍是否已满
          public bool IsTeamFull()
          {
              return MAX_TEAM_COUNT >= mlistMember.Count; 
          }
          //加入队伍成员
          public void AddMember(PlayerObject play)
          {
              mlistMember.Add(play);
          }
          //删除队伍成员
          public void DeleteMember(PlayerObject play)
          {
              ExitTeam(play);
          }

          //取队长
          public PlayerObject GetCaptain()
          {
              if (mlistMember.Count <= 0) return null;
              return mlistMember[0];
          }

          //退出队伍
          public void ExitTeam(PlayerObject _play)
          {
              for (int i = 0; i < mlistMember.Count; i++)
              {
                  PlayerObject play = mlistMember[i];
                  if (play.GetBaseAttr().player_id == _play.GetBaseAttr().player_id)
                  {
                      _play.SetTeam(null);
                      mlistMember.RemoveAt(i);
                      break;
                  }
               }
              //队伍人数不足就解散队伍
              if (mlistMember.Count == 0)
              {
                  DeleteTeam();
              }
          }
          //解散队伍
          public void DeleteTeam()
          {
              for (int i = 0; i < mlistMember.Count; i++)
              {
                  PlayerObject play = mlistMember[i];
                  play.SetTeam(null);
              }
              mlistMember.Clear();
              TeamManager.Instance().DeleteTeam(mID);
          }

          //可以同步玩家各种属性信息- 目前只拿来同步血量 信息
          public void ShareInfo(PlayerObject obj)
          {
              for (int i = 0; i < mlistMember.Count; i++)
              {
                  PlayerObject play = mlistMember[i];
                  if (play.GetBaseAttr().player_id == obj.GetBaseAttr().player_id) continue; //不发给自己
                  NetMsg.MsgUserAttribute msg = new NetMsg.MsgUserAttribute();
                  msg.Create(null, play.GetGamePackKeyEx());
                  msg.role_id = play.GetTypeId();
                  msg.AddAttribute(GameStruct.UserAttribute.LIFE,obj.GetBaseAttr().life);
                  msg.AddAttribute(GameStruct.UserAttribute.LIFE_MAX, obj.GetBaseAttr().life_max);
                  play.SendData(msg.GetBuffer());
              }
            

          }
      }
}
