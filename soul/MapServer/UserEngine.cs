using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using GameBase.Network;
using GameBase.Config;
//全局用户管理
//2015.8.10
namespace MapServer
{
    //临时角色-- 
    public class TempPlayObject
    {
        public int key;
        public int key2;
        public bool isRole;
        public int accountid;
      
        public PlayerObject play;
        public TempPlayObject()
        {
            key = key2 = 0;
            play = null;
            isRole = false;
         
        }
    }
    public class UserEngine
    {
        
        private static UserEngine m_Instance = null;
        private Dictionary<UInt32,PlayerObject> m_DicPlayerObject = null;
        private Dictionary<UInt32, TempPlayObject> m_DicTempPlayObject = null; //临时角色信息
        private List<PlayerObject> mListSaveRole; //保存玩家数据的队列
        private List<PlayerObject> mListCacheRole;  //缓存玩家队列
        private int mnCachePlaySaveTick; //保存缓存玩家队列时间戳
        public UserEngine()
        {
            m_DicPlayerObject = new Dictionary<UInt32, PlayerObject>();
            m_DicPlayerObject.Clear();

            m_DicTempPlayObject = new Dictionary<UInt32, TempPlayObject>();
            m_DicTempPlayObject.Clear();

            mListCacheRole = new List<PlayerObject>();
            mListCacheRole.Clear();
            mListSaveRole = new List<PlayerObject>();
            mnCachePlaySaveTick = System.Environment.TickCount;
        }


        public static UserEngine Instance() 
        {
            if (m_Instance == null)
            {
                m_Instance = new UserEngine();
            }
            return m_Instance;
        }

        public PlayerObject CreatePlayObject()
        {
            PlayerObject play = new PlayerObject();
            m_DicPlayerObject[play.GetGameID()] = play;
            return play;
        }


        public PlayerObject FindPlayerObjectToSocket(Socket s)
        {
            GameSession session;
            foreach (PlayerObject obj in m_DicPlayerObject.Values)
            {
                session = obj.GetGameSession();
                if (session != null && session.m_Socket == s)
                {
                    return obj;
                }
            }
            return null;
        }



        public PlayerObject FindPlayerObjectToID(UInt32 id)
        {
            if (m_DicPlayerObject.ContainsKey(id))
            {
                return m_DicPlayerObject[id];
            }
            return null;
        }

        public PlayerObject FindPlayerObjectToTypeID(uint id)
        {
            foreach (PlayerObject obj in m_DicPlayerObject.Values)
            {
                if (obj.GetTypeId() == id)
                {
                    return obj;
                }
            }
            return null;
        }

        public PlayerObject FindPlayerObjectToPlayerId(int play_id)
        {
            foreach (PlayerObject obj in m_DicPlayerObject.Values)
            {
                if (obj.GetBaseAttr().player_id == play_id)
                {
                    return obj;
                }
            }
            return null;
        }
        public PlayerObject FindPlayerObjectToAccountId(int Accountid)
        {
            foreach (PlayerObject obj in m_DicPlayerObject.Values)
            {
                if (obj.GetBaseAttr().account_id == Accountid)
                {
                    return obj;
                }
            }
            return null;
        }
        public PlayerObject FindPlayerObjectToName(String name)
        {
            foreach (PlayerObject obj in m_DicPlayerObject.Values)
            {
                if (obj.GetName() == name)
                {
                    return obj;
                }
            }
            return null;
        }
        //仅仅用于正常断线玩家
        public void RemovePlayObjectToSocket(Socket s)
        {
            
            GameSession session;
            foreach (PlayerObject obj in m_DicPlayerObject.Values)
            {
                
                session = obj.GetGameSession();
                if (session != null && session.m_Socket == s)
                {
                    obj.Dispose();
                    m_DicPlayerObject.Remove(obj.GetID());
                    break;
                }
            }
        }

        public void RemovePlayObject(PlayerObject obj)
        {
            //从地图中删除
            if (obj == null) return;
            if (obj.GetGameMap() != null)
            {
                obj.GetGameMap().RemoveObj(obj);
            }
            obj.Dispose();
            if (m_DicPlayerObject.ContainsKey(obj.GetGameID()))
            {
                m_DicPlayerObject.Remove(obj.GetGameID());
            }
        }
        public void AddPlayerObject(PlayerObject obj)
        {
            m_DicPlayerObject[obj.GetGameID()] = obj;
        }



        public void Run()
        {
            //这里可以根据服务器的压力要不要延迟数据库
            //下次再写
            if (mListSaveRole.Count > 0)
            {
                DBServer.Instance().SaveRoleData(mListSaveRole[0]);
                mListSaveRole.RemoveAt(0);
            }
            //缓存队列保存时间
            if (DBServer.Instance().IsConnect() && mListCacheRole.Count > 0)
            {
                if (System.Environment.TickCount - mnCachePlaySaveTick > 5000) //五秒保存一次玩家数据
                {
                    Log.Instance().WriteLog("保存玩家缓冲队列,玩家昵称:" + mListCacheRole[0].GetName());
                     mnCachePlaySaveTick = System.Environment.TickCount;
                     DBServer.Instance().SaveRoleData(mListCacheRole[0]);
                     mListCacheRole.RemoveAt(0); 
 
                    
                }
            }
        }
        //屏幕中间公告
        public void SceneNotice(String text)
        {
            NetMsg.MsgNotice notice = new NetMsg.MsgNotice();
            notice.Create(null, null);
            byte[] buff = notice.GetSceneNoticeBuff(text);
            UserEngine.Instance().BrocatBuffer(buff);
        }
        public void AddTempPlayObject(GameBase.Network.Internal.RoleInfo info)
        {

         
            TempPlayObject temp = new TempPlayObject();
            PlayerObject play = new PlayerObject();
            temp.play = play;
            temp.key = info.mKey;
            temp.key2 = info.mKey1;
            temp.isRole = info.isRole;
            temp.accountid = info.accountid;
            //基本属性
            m_DicTempPlayObject[play.GetGameID()] = temp;

            if (temp.isRole)
            {
                play.SetName(info.name);
                GameStruct.PlayerAttribute attr = play.GetBaseAttr();
                attr.account_id = info.accountid;
                attr.player_id = info.playerid;
                attr.mana = info.mana;
                attr.lookface = info.lookface;
                attr.hair = info.hair;
                attr.profession = info.profession;
                attr.level = info.lv;
                attr.exp = (int)info.exp;
                attr.life = info.life;
                attr.pk = info.pk;
                attr.gold = info.gold;
                attr.gamegold = info.gamegold;
                attr.stronggold = info.stronggold;
                attr.mapid = (uint)info.mapid;
                attr.guanjue = info.guanjue;
                attr.sAccount = info.sAccount;
                attr.godlevel = (byte)info.godlevel;
                attr.maxeudemon = info.maxeudemon;
                play.SetHotKeyInfo(info.hotkey);
                play.CalcSex();
                play.SetPoint(info.x, info.y);
                
                //官爵信息
                GameStruct.GUANGJUELEVEL gjlevel = GuanJueManager.Instance().GetLevel(play);
                play.SetGuanJue(gjlevel);
                //初始化军团信息
                play.GetLegionSystem().Init();
            }

            
           
           
        }

        public void RemoveTempPlayObject(int key, int key2)
        {
            foreach (TempPlayObject obj in m_DicTempPlayObject.Values)
            {
                if (obj.key == key && obj.key2 == key2)
                {
                    m_DicTempPlayObject.Remove(obj.play.GetGameID());
                    break;
                }
            }
        }
         public void RemoveTempPlayObject(uint gameid)
        {
            if (m_DicTempPlayObject.ContainsKey(gameid))
            {
                m_DicTempPlayObject.Remove(gameid);
             }
        }
        public TempPlayObject GetTempPlayObj(int key, int key2)
        {
            foreach (TempPlayObject obj in m_DicTempPlayObject.Values)
            {
                if (obj.key == key && obj.key2 == key2)
                {
                    return obj;
                    
                }
            }
            return null;
        }
        public TempPlayObject GetTempPlayObj(uint gameid)
        {
            if (m_DicTempPlayObject.ContainsKey(gameid))
            {
                return m_DicTempPlayObject[gameid];
            }
            return null;
        }

        //广播给所有玩家消息
        public void BrocatBuffer(byte[] data)
        {
            foreach (PlayerObject obj in m_DicPlayerObject.Values)
            {
                NetMsg.BaseMsg msg = new NetMsg.BaseMsg();
                msg.Create(data, obj.GetGamePackKeyEx());
                obj.SendData(msg.GetBuffer());
            }
        }

        //广播文字消息
        public void BroadcastMsg(GameBase.Config.BROADCASTMSGTYPE type,String msg)
        {
            foreach (PlayerObject obj in m_DicPlayerObject.Values)
            {
                if (obj.GetGameSession() != null)
                {
                    switch (type)
                    {
                        case GameBase.Config.BROADCASTMSGTYPE.LEFT:
                            {
                                obj.LeftNotice(msg);
                                break;
                            }
                        case GameBase.Config.BROADCASTMSGTYPE.CHAT:
                            {
                                obj.ChatNotice(msg);
                                break;
                            }
                        case GameBase.Config.BROADCASTMSGTYPE.SCREEN:
                            {
                                UserEngine.Instance().SceneNotice(msg);
                                break;
                            }
                    }
                }
            }
        }

        public void AddSaveRole(PlayerObject play)
        {
            for (int i = 0; i < mListSaveRole.Count; i++)
            {
                if (mListSaveRole[i].GetGameID() == play.GetGameID())
                {
                    return;
                }
            }
            mListSaveRole.Add(play);
        }

        public int GetOnlineCount()
        {
            return m_DicPlayerObject.Count;
        }

        public void RemoveCachePlay(PlayerObject play)
        {
            for (int i = 0; i < mListCacheRole.Count; i++)
            {
                if (mListCacheRole[i].GetBaseAttr().sAccount == play.GetBaseAttr().sAccount)
                {
                    mListCacheRole.RemoveAt(i);
                    break;
                }
            }
      
        }
        public void AddCachePlay(PlayerObject play)
        {
            for (int i = 0; i < mListCacheRole.Count; i++)
            {
                if (mListCacheRole[i].GetBaseAttr().sAccount == play.GetBaseAttr().sAccount)
                {
                    mListCacheRole.RemoveAt(i);
                    break;
                }
            }
            mListCacheRole.Add(play);
          
            
        }
        public PlayerObject GetCachePlay(String sAccount)
        {
            for (int i = 0;i < mListCacheRole.Count; i++)
            {
                if (mListCacheRole[i].GetBaseAttr().sAccount == sAccount)
                {
                    return mListCacheRole[i];
                }
            }
            return null;
        }
        public void Stop()
        {
            List<PlayerObject> list_obj = new List<PlayerObject>();
                
            foreach (PlayerObject play in m_DicPlayerObject.Values)
            {
                list_obj.Add(play);
            }
            for (int i = 0; i < list_obj.Count; i++)
            {
                list_obj[i].ExitGame();
            }
       
            Log.Instance().WriteLog("退出服务器！");
        }
    }
}
