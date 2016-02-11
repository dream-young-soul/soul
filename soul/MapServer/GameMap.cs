using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using GameBase.Config;
using System.Diagnostics;
using GameBase.Network.Internal;
using GameStruct;
namespace MapServer
{

    //2015.11.14 因为魔域的地图尺寸太大，过于浪费内存，现在给出一个暂时优化方案，把地形与高度屏蔽掉，到时候用到地形与高度再解除屏蔽
  
    public class GameMap
    {

        private GameStruct.MapInfo info;
        public uint mnVersion; //版本号
        public uint mnWidth; //地图宽度
        public uint mnHeight; //地图高度
        private GameStruct.MapGridInfo[,] mMapGridInfo;
        public GameStruct.MapGridInfo[,] GetMapGridInfo() {
            if (mMapGridInfo == null)
            {
                mMapGridInfo = new GameStruct.MapGridInfo[mnWidth, mnHeight];
            }
            return mMapGridInfo;
        }
        private Dictionary<uint, BaseObject> mDicObject;
     
        public Dictionary<uint, BaseObject> GetAllObject() { return mDicObject; }
        private List<BaseObject> mListDeleteObj;//这里存储要删除的对象
        private List<BaseObject> mListAddObj; //这里存储要添加的对象
        private List<GameStruct.MapRegionInfo> mListRegionInfo; //地图注册信息

        public int last_null_tick; //地图最后没人的时间戳
       
        
        public MapPath mPath;
        public uint GetID()
        {
            return info.id;
        }

        public GameMap(GameStruct.MapInfo mapinfo)
        {
            mListRegionInfo = null;
            info = mapinfo;
            mDicObject = new Dictionary<uint, BaseObject>();
            mListDeleteObj = new List<BaseObject>();
            mListAddObj = new List<BaseObject>();
            last_null_tick = System.Environment.TickCount;
        }
        //复制地图对象- 用于做副本使用
        public GameMap Clone()
        {
            GameMap new_map = new GameMap(this.info);
            new_map.mnWidth = this.mnWidth;
            new_map.mnHeight = this.mnHeight;
            new_map.mPath = new MapPath(mnHeight, mnWidth);
      
            for (uint i = 0; i < mnHeight; i++)
            {
                for (uint j = 0; j < mnWidth; j++)
                {
                    new_map.GetMapGridInfo()[j, i] = this.mMapGridInfo[j, i];
                    if (new_map.GetMapGridInfo()[j, i].Mask > 0)
                    {
                        new_map.mPath.SetPointMask((short)j, (short)i, MapPath.MASK_CLOSE);
                    }
                }
            }
            foreach (BaseObject obj in this.GetAllObject().Values)
            {
                new_map.AddObject(obj, obj.GetGameSession());
            }
            return new_map;

        }
        public bool Create()
        {
 
            if (info == null) return false;
            if (!File.Exists(info.dmappath))
            {
                Log.Instance().WriteLog("地图文件不存在:" + info.dmappath);
                return false;
            }
            FileStream stream = new FileStream(info.dmappath, FileMode.Open);
            BinaryReader read = new BinaryReader(stream);
            mnVersion = read.ReadUInt32();    //版本号
            uint dwData = read.ReadUInt32();      //附加数据
            byte[] b = read.ReadBytes(260);
            String sPuzPath = System.Text.Encoding.Default.GetString(b);    //spul路径
            uint nWidth = read.ReadUInt32(); //地图宽度
            uint nHeight = read.ReadUInt32(); //地图高度
            mnWidth = nWidth;
            mnHeight = nHeight;
            mPath = new MapPath(nWidth,nHeight);
            
            mMapGridInfo = new GameStruct.MapGridInfo[nWidth, nHeight];
            for (uint i = 0; i < nHeight; i++)
            {
                uint dwCheckData = 0;
                for (uint j = 0; j < nWidth; j++)
                {
                    GameStruct.MapGridInfo grid;
                    ushort usMask = read.ReadUInt16();    //掩码
                    ushort usTerrain = read.ReadUInt16(); //地形
                    short sAltitude = read.ReadInt16();   //高度
                    //校验数据
                    dwCheckData += usMask * (usTerrain + i + 1) + ((uint)sAltitude + 2) * (j + 1 + usTerrain);
                    grid.Mask = (byte)usMask;
                    //grid.Terrain = usTerrain;
                    //grid.Altitude = sAltitude;
                    mMapGridInfo[j, i] = grid;
                    if(usMask > 0)
                    {
                          mPath.SetPointMask((short)j, (short)i, MapPath.MASK_CLOSE);
                    }
                   
                }
                uint dwMapCheckData;
                dwMapCheckData = read.ReadUInt32();
                if (dwMapCheckData != dwCheckData)
                {
                    Log.Instance().WriteLog("载入地图文件失败..路径:" + info.dmappath);
                    return false;
                }
            }
            stream.Dispose();
            return true;
        }
        public void Process()
        {
            if (mDicObject.Count == 0 && 
                mListDeleteObj.Count == 0 &&
                mListAddObj.Count == 0) return; //地图没人..就不处理了
            //删除离开地图的玩家
            if (mListDeleteObj.Count > 0)
            {
                uint id;
                for (int i = 0; i < mListDeleteObj.Count; i++)
                {
                    BaseObject obj = mListDeleteObj[i] as BaseObject;
                    if (obj.type == OBJECTTYPE.MONSTER ||
                        obj.type == OBJECTTYPE.GUARDKNIGHT) id = obj.GetTypeId();
                    else id = obj.GetGameID();
                    //obj.Dispose();
                    //if (obj.type == OBJECTTYPE.PLAYER)
                    //{
                    //    UserEngine.Instance().RemovePlayObject(obj as PlayerObject);
                    //}
                    if (mDicObject.ContainsKey(id)) { mDicObject.Remove(id); }
                }
                mListDeleteObj.Clear();
            }

            //要添加的对象
            if (mListAddObj.Count > 0)
            {
                for (int i = 0; i < mListAddObj.Count; i++)
                {
                    BaseObject obj = mListAddObj[i];
                    if (obj.type == OBJECTTYPE.MONSTER) //怪物是以typeid为主键
                    {
                        mDicObject[obj.GetTypeId()] = obj;
                    }
                    else mDicObject[obj.GetGameID()] = obj;
                }
                mListAddObj.Clear();
            }
          //地图所有对象run
            //需要拷贝一份词典，因为在循环里面会改变该词典的集合-- 
            //需要优化 这里效率太低了
           // Dictionary<uint, BaseObject> tempdic = new Dictionary<uint, BaseObject>(mDicObject);
            //2015.11.12 已解决，声明了一个删除的列表- mListDeleteObj 在removeobj后加入到列表 process再进行删除-内存不涨了 好开心o(∩_∩)o 哈哈
            foreach (BaseObject obj in mDicObject.Values)
            {
                if (!obj.Run())
                {
                    mListDeleteObj.Add(obj);
                }
            }


        
          

        }

        public BaseObject GetObject(uint id)
        {
            if (mDicObject.ContainsKey(id))
            {
                return mDicObject[id];
            }
            return null;
        }
        public GameStruct.MapInfo GetMapInfo()
        {
            return info;
        }

        public bool CanMove(short x, short y)
        {
            if (x >= mnWidth || y >= mnHeight || x < 0 || y < 0) return false;
            return mMapGridInfo[x, y].Mask == 0;
        }

 


        //怪物是以typeid为主键
      //  public void AddObject(MonsterObject obj)
      // {
          // mDicObject[obj.GetTypeId()] = obj;
          // obj.mGameMap = this;
           //switch (obj.type)
           //{
           //    case OBJECTTYPE.MONSTER:
           //        {
           //            GameStruct.Action action = new GameStruct.Action(GameStruct.Action.MOVE, null);
           //            obj.PushAction(action);
           //            break;
           //        }
           // }
     //  }

        public void SendWeatherInfo(PlayerObject play)
        {
            //下雪天气-
            //if (obj.type == OBJECTTYPE.PLAYER)
            //{

            //下雪天气
            GameBase.Network.PacketOut outpack = new GameBase.Network.PacketOut();
            outpack.WriteInt16(20);
            outpack.WriteInt16(1110);
            outpack.WriteUInt32(this.GetMapInfo().id);
            outpack.WriteUInt32(this.GetMapInfo().id);

            if (this.GetMapInfo().issnows)
            {
                byte[] data3 = { 0, 0, 32, 0, 128, 0, 18, 0 };
                outpack.WriteBuff(data3);
            }
            else
            {
                outpack.WriteInt32(0);
                outpack.WriteInt32(0);
            }
            play.SendData(outpack.Flush(), true);
            //}
        }
        public void AddObject(BaseObject obj, GameBase.Network.GameSession session = null)
        {
            mListAddObj.Add(obj);

          //  mDicObject[obj.GetGameID()] = obj;
            obj.mGameMap = this;
            obj.session = session;
            if (obj.type == OBJECTTYPE.PLAYER)
            {
                last_null_tick = System.Environment.TickCount;
            }
          
        }
        //删除地图对象
        public void RemoveObj(BaseObject obj)
        {
            uint id ;
            if(obj.type == OBJECTTYPE.MONSTER) id = obj.GetTypeId();
            else id =obj.GetGameID();
            if ( mDicObject.ContainsKey(id))
            {
                if (obj.type == OBJECTTYPE.PLAYER)
                {
                    PlayerObject play = obj as PlayerObject;
                    play.ClearThis(); //广播删除自己
                }
                if (obj.type == OBJECTTYPE.EUDEMON)
                {
                    EudemonObject eudemon = obj as EudemonObject;
                    eudemon.ReCall();
                }
                if (obj.type == OBJECTTYPE.PTICH)
                {
                    PtichObject ptich = obj as PtichObject;
                    ptich.ClearThis();
                }
                //加到临时删除列表- 下次process时处理删除
                mListDeleteObj.Add(obj);
              //  mDicObject.Remove(id);
            }
            if (this.GetObjectCount(OBJECTTYPE.PLAYER) == 0)
            {
                last_null_tick = System.Environment.TickCount;
            }
        }
        //创建怪物
        public void CreateMonster(GameStruct.GeneratorInfo info)
        {
            GameStruct.MonsterInfo minfo = ConfigManager.Instance().GetMonsterInfo(info.monsterid);
            if (minfo == null)
            {
                Log.Instance().WriteLog("无法找到怪物id:" + info.monsterid.ToString());
                return;
            }
            MonsterObject obj ;
            Random rd = new Random();

            for (int i = 0; i < info.amount; i++)
            {

                short cx = 0;
                short cy = 0;
                byte index = 0;
                while (true)
                {
                    cx = (short)rd.Next((int)info.bound_x, (int)(info.bound_x + info.bound_cx));
                    cy = (short)rd.Next((int)info.bound_y, (int)(info.bound_y + info.bound_cy));
                    if (CanMove(cx, cy))
                    {
 
                        break;
                    }
                    index++;
                    if (index >= 100)
                    {
                        cx = cy = 0;
                        break;
                    }
                }
                if (cx == 0 && cy == 0)
                {
                    Log.Instance().WriteLog("创建怪物失败,无法找到落脚点" + this.GetMapInfo().name + "怪物名称:" + minfo.name + "地图id:"+
                        info.mapid.ToString()+" x:"+info.bound_x.ToString()+" y:"+info.bound_y.ToString());
                    return;
                }
                obj = new MonsterObject(minfo.id, minfo.ai,cx,cy,true);
                if (info.dir == DIR.MAX_DIRSIZE)
                {
                    obj.SetDir(GameStruct.DIR.Random_Dir());

                }
                else
                {
                    obj.SetDir(info.dir);
                }
                obj.SetRebirthTime(info.time);
           
               
                AddObject(obj);

            }

        }

        //查找目标
        public BaseObject FindObjectForID(uint id)
        {
            BaseObject ret = FindMonsterObject(id);
            if (ret != null) return ret;
            foreach(BaseObject o in mDicObject.Values)
            {
                if(o.GetTypeId() == id)
                {
                    return o;
                }
            }

            return null;
        }
        public MonsterObject FindMonsterObject(uint id)
        {
            if (mDicObject.ContainsKey(id))
            {
                return mDicObject[id] as MonsterObject;
            }
            return null;
        }

        //添加掉落道具信息
        public void AddDropItemObj(uint itemid,short x,short y,uint ownerid = 0,int time = 120000,GameStruct.RoleItemInfo info = null,RoleData_Eudemon eudemon = null)
        {
            DropItemObject obj = new DropItemObject(itemid, x, y,ownerid, time);
            obj.SetRoleItemInfo(info);
            obj.SetRoleEudemonInfo(eudemon);
            AddObject(obj);
            obj.RefreshVisibleObject();
            //广播给玩家
            obj.BroadcastInfo();
        }
        public MapPath GetMapPath() { return mPath; }
        //广播移动消息
        //public void BroadcastMove(BaseObject obj,byte[] data)
        //{
        //    foreach (BaseObject o in obj.mVisibleList.Values)
        //    {
        //        if (o.type == OBJECTTYPE.PLAYER)
        //        {
        //            PlayerObject po = o as PlayerObject;
        //            o.SendData(data);
                  
        //       }
        //   }
        //}
        //广播消息
        public void BroadcastBuffer(BaseObject obj, byte[] buff)
        {
          //  obj.RefreshVisibleObject();
            foreach (RefreshObject o in obj.GetVisibleList().Values)
            {
                BaseObject _obj = o.obj;
                if (_obj.type == OBJECTTYPE.PLAYER && _obj.GetGameSession() != null)
                {
                    NetMsg.BaseMsg basemsg = new NetMsg.BaseMsg();
                    basemsg.Create(buff, _obj.GetGamePackKeyEx());
                    _obj.SendData(basemsg.GetBuffer());
                }
            }
        }



        //取该坐标点是否有对象[优先判断是否有阻挡]
        public bool GetPointOfObj(BaseObject obj, short x, short y)
        {
            if (!CanMove(x, y)) return true;
            obj.RefreshVisibleObject();
            foreach (RefreshObject baseobj in obj.GetVisibleList().Values)
            {
                BaseObject _obj = baseobj.obj;
                if (_obj.GetCurrentX() == x && _obj.GetCurrentY() == y)
                {
                    return true;
                }
            }
            if (mListAddObj.Count > 0)
            {
                for (int i = 0; i < mListAddObj.Count; i++)
                {
                    BaseObject _obj = mListAddObj[i];
                    if (_obj.GetCurrentX() == x && _obj.GetCurrentY() == y)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //广播文字消息
        public void BroadcastMsg(GameBase.Config.BROADCASTMSGTYPE type, String msg)
        {
            foreach (BaseObject obj in mDicObject.Values)
            {
                if (obj.type != OBJECTTYPE.PLAYER) continue;
                PlayerObject play = obj as PlayerObject;
                if (obj.GetGameSession() != null)
                {
                    switch (type)
                    {
                        case GameBase.Config.BROADCASTMSGTYPE.LEFT:
                            {
                                play.LeftNotice(msg);
                                break;
                            }
                        case GameBase.Config.BROADCASTMSGTYPE.CHAT:
                            {
                                play.ChatNotice(msg);
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

        public void AddRegionInfo(GameStruct.MapRegionInfo info)
        {
            if (mListRegionInfo == null)
            {
                mListRegionInfo = new List<GameStruct.MapRegionInfo>();
            }
            mListRegionInfo.Add(info);
        }

        //该坐标点是否是安全区
        public bool IsSafeArea(short x,short y)
        {
            if(mListRegionInfo == null)return false;
            for(int i = 0;i < mListRegionInfo.Count;i++)
            {
                if(mListRegionInfo[i].type == GameStruct.MapRegionInfo.MAPREGIONINFO_TYPE_SAFE)
                {
                        int dis_x = Math.Abs(x- mListRegionInfo[i].bound_x);
                        int dis_y = Math.Abs(y - mListRegionInfo[i].bound_y);
                        if (dis_x <= mListRegionInfo[i].bound_cx && 
                            dis_y <= mListRegionInfo[i].bound_cy)
                        {
                            return true;
                        }
                     
                }
            }
            return false;
        }
        //获得地图指定对象数量
        public int GetObjectCount(byte type)
        {
            int count = 0;
            foreach(BaseObject obj in mDicObject.Values)
            {
                if (obj.type == type)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
