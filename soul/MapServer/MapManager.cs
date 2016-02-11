using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using GameBase.Config;

namespace MapServer
{

    public class FubenGameMap
    {
        public uint mMapID;
        public uint GetMapID(){return mMapID;}
        public List<GameMap> mGameMap;
      
        public FubenGameMap(uint id)
        {
            mMapID = id;
            mGameMap = new List<GameMap>();
      
        }
        public bool AddFubenMap(GameMap map)
        {
            if (mGameMap.Count >= GameBase.Config.Define.MAX_FUBEN_CLONE_COUNT)
            {
                return false;
            }
            mGameMap.Add(map);
            return true;
        }
        public GameMap GetFubenMap()
        {
            for (int i = 0; i < mGameMap.Count; i++)
            {
                if (mGameMap[i].GetObjectCount(OBJECTTYPE.PLAYER) == 0)
                {
                    mGameMap[i].last_null_tick = System.Environment.TickCount;
                    return mGameMap[i];
                }
            }
            return null;
        }
        public void Process()
        {
            List<GameMap> del_list = null;
            for (int i = 0; i < mGameMap.Count; i++)
            {
                if (mGameMap[i].GetObjectCount(OBJECTTYPE.PLAYER) == 0)
                {
                    if (System.Environment.TickCount - mGameMap[i].last_null_tick >
                        GameBase.Config.Define.LAST_FUBEN_NULL_DELETE_TIME)
                    {
                        if (del_list == null) { del_list = new List<GameMap>();}
                        del_list.Add(mGameMap[i]);
                    }
                }
                mGameMap[i].Process();
                
            }
            if (del_list != null && del_list.Count > 0)
            {
                for (int i = 0; i < del_list.Count; i++)
                {
                    mGameMap.Remove(del_list[i]);
                    del_list[i] = null;
                }

                del_list.Clear();
            }
        }
    }
    //地图全局管理
    public class MapManager
    {
        private Dictionary<uint, GameMap> m_DicMap;
        private Dictionary<uint, FubenGameMap> m_DicFubenMap;
        private static MapManager m_Instance = null;
        public static MapManager Instance()
        {
            if(m_Instance == null)
            {
                m_Instance = new MapManager();
            }
            return m_Instance;
        }

        public MapManager()
        {
            m_DicMap = new Dictionary<uint, GameMap>();
            m_DicFubenMap = new Dictionary<uint, FubenGameMap>();
        }
        public bool AddMap(GameMap map)
        {
            if (m_DicMap.ContainsKey(map.GetID()))
            {
                Log.Instance().WriteLog("增加地图失败,已经存在该地图--地图ID:" + map.GetID().ToString());
                return false;
            }
            m_DicMap[map.GetID()] = map;
            return true;
        }
        
        //加入副本地图
        public GameMap AddFubenMap(uint mapid)
        {
            GameMap map = this.GetGameMapToID(mapid);
            GameMap fb_map = null;
            if (map == null)
            {
                Log.Instance().WriteLog("创建副本地图失败,地图ID:" + mapid.ToString());
                return null; ;
            }
            FubenGameMap fuben_map = null;
            if (m_DicFubenMap.ContainsKey(map.GetMapInfo().id))
            {
                fuben_map = m_DicFubenMap[map.GetMapInfo().id];
            }
           
            if (fuben_map == null)
            {
                fuben_map = new FubenGameMap(map.GetMapInfo().id);
                m_DicFubenMap[map.GetMapInfo().id] = fuben_map;
               
            }//从现有的副本中取出
            else
            {
                
            }
           fb_map = fuben_map.GetFubenMap();
           if (fb_map == null)
            {
                fb_map = map.Clone();
            }
      
            return fb_map;
        }

        //
        //处理地图刷怪 地图事件等等信息
        public void Process()
        {
            foreach (GameMap map in m_DicMap.Values)
            {
                map.Process();
            }
            //处理副本
            foreach (FubenGameMap fb_map in m_DicFubenMap.Values)
            {
                fb_map.Process();
            }
        }

        //根据id查找地图
        public GameMap GetGameMapToID(uint id)
        {
            if (m_DicMap.ContainsKey(id))
            {
                return m_DicMap[id];
            }
            return null;
        }
    }
}
