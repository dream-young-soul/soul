using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Network;
using GameBase.Config;
using GameBase.Network.Internal;
using System.Net.Sockets;

//与dbserver通讯的处理
namespace MapServer
{
    class DBServer
    {
        private static DBServer m_Intsance = null;

        private  GameBase.Network.TcpClient mTcpDBClient;  //与dbserver通讯的客户端
        private bool mbConnect;
        public bool IsConnect(){return mbConnect;}//是否连接到了dbserver
        private  InternalPacket mDBPacket;                //收到dbserver的数据包

        private static Object _lock = new Object();
        private int mnReconnectTick;
        public static DBServer Instance()
        {
            if (m_Intsance == null)
            {
                m_Intsance = new DBServer();
            }
            return m_Intsance;
        }
        public DBServer()
        {
            mbConnect = false;
            mnReconnectTick = System.Environment.TickCount;
        }

        public void Init()
        {
            MemIniFile ini = new MemIniFile();
          
            if (!ini.LoadFromFile(TextDefine.GoldConfig))
            {
                Log.Instance().WriteLog("load golbalconfig error!");
                return;
            }
            //连接dbserver的客户 InternalPacket
            mDBPacket = new InternalPacket();
            mTcpDBClient = new GameBase.Network.TcpClient();
            mTcpDBClient.onConnect += new TcpClientEvent.OnConnectEventHandler(OnDBConnectEventHandler);
            mTcpDBClient.onReceive += new TcpClientEvent.OnReceiveEventHandler(OnDBReceiveEventHandler);
            mTcpDBClient.onClose += new TcpClientEvent.OnCloseEventHandler(OnDBClose);

            String sIP  = ini.ReadValue(TextDefine.DBServerSestion, TextDefine.NormalIPKey, TextDefine.NormalIP);
            int nPort = ini.ReadValue(TextDefine.DBServerSestion, TextDefine.NormalPortKey, TextDefine.DBServerPort);
            mTcpDBClient.Connect(sIP, nPort);
        }

        //与dbserver通讯的客户端

        private  void OnDBConnectEventHandler(bool isSucceed)
        {
            if (isSucceed)
            {
                Log.Instance().WriteLog("dbserver connect success!");
                GameBase.Network.Internal.OpenMapSession pack = new GameBase.Network.Internal.OpenMapSession();
                mTcpDBClient.SendData(pack.GetBuff());
                mbConnect = true;
            }
            else
            {
                Log.Instance().WriteLog("dbserver connect error!");
                Log.Instance().WriteLog("Reconnect  dbserver ip:" + mTcpDBClient.GetConnectIP() +
                    " port:" + mTcpDBClient.GetConnectPort().ToString());
               // System.Threading.Thread.Sleep(5000);
                mbConnect = false;
               // mTcpDBClient.ReConnect();
            }
            
        }

        private  void OnDBReceiveEventHandler(byte[] data, int nSize)
        {
            lock (_lock)
            {
                byte[] buff = new byte[nSize];
                Buffer.BlockCopy(data, 0, buff, 0, nSize);
                mDBPacket.ProcessNetMsg(buff);
            }
 
            
        }

        private  void OnDBClose(Socket s)
        {
            lock (_lock)
            {
                mbConnect = false;
                mDBPacket.ClearPacket();
                Log.Instance().WriteLog("dbserver close!!!reconnect ");
             
                mTcpDBClient.SetSocket(null);
                mnReconnectTick = System.Environment.TickCount; //重连时间
            }

        }

        public  void ProcessDBNetMsg()
        {

            if (!mbConnect && System.Environment.TickCount - mnReconnectTick > 5000)//五秒重连
            {
                mTcpDBClient.ReConnect();
                mnReconnectTick = System.Environment.TickCount;
            }
            byte[] buff = null;
            lock (_lock)
            {
                buff = mDBPacket.GetData();
            }
           
            if (buff == null) return;
            PackIn inpack = new PackIn(buff);
            ushort param = inpack.ReadUInt16();
            switch (param)
            {
                case GameBase.Network.Internal.Define.ROLEINFO:
                    {
                        GameBase.Network.Internal.RoleInfo roleinfo = new GameBase.Network.Internal.RoleInfo(buff);
                        //判断是否有缓存数据-
                        PlayerObject cacheplay = UserEngine.Instance().GetCachePlay(roleinfo.sAccount);
                        if (cacheplay != null)
                        {
                            Log.Instance().WriteLog("检测到角色缓存数据，保存中!" + cacheplay.GetName());
                            UserEngine.Instance().RemoveCachePlay(cacheplay);
                            cacheplay.ExitGame();
                            return;
                        }
                        UserEngine.Instance().AddTempPlayObject(roleinfo);

                        //回发给dbserver 表示收到了角色信息- 让他通知loginserver 发数据给玩家连接mapserver
                        GameBase.Network.Internal.RoleInfo_Ret ret = new GameBase.Network.Internal.RoleInfo_Ret();
                        ret.gameid = roleinfo.gameid;
                        ret.key = roleinfo.mKey;
                        ret.key2 = roleinfo.mKey1;
                        ret.accountid = roleinfo.accountid;
                        mTcpDBClient.SendData(ret.GetBuffer());
                        Log.Instance().WriteLog("收到临时角色信息:" + roleinfo.sAccount+" id:"+roleinfo.accountid.ToString());
                        break;
                    }
                case GameBase.Network.Internal.Define.QUERYROLENAME_RET:
                    {
                        QueryRoleName_Ret ret = new QueryRoleName_Ret();
                        ret.Create(buff);
                        TempPlayObject temp = UserEngine.Instance().GetTempPlayObj(ret.gameid);
                        if (temp == null)
                        {
                            Log.Instance().WriteLog("找到玩家对象..在--ProcessDBNetMsg code:2");
                            break;
                        }

                        NetMsg.MsgNotice notice = new NetMsg.MsgNotice();
                        notice.Create(null, temp.play.GetGamePackKeyEx());
                        temp.play.SendData(notice.GetQueryNameBuff(!ret.tag));

                        break;
                    }
                case GameBase.Network.Internal.Define.CREATEROLE_RET:
                    {
                      
                        CreateRole_Ret ret = new CreateRole_Ret();
                        ret.Create(buff);
                        TempPlayObject temp = UserEngine.Instance().GetTempPlayObj(ret.gameid);
                        if (temp == null)
                        {
                            Log.Instance().WriteLog("未找到玩家对象..在--ProcessDBNetMsg code:3");
                            break;
                        }

                        
                        UserEngine.Instance().RemoveTempPlayObject(ret.gameid); //从临时列表出移除--
                        //进入游戏
                        temp.play.GetBaseAttr().account_id = temp.accountid;
                        temp.play.GetBaseAttr().player_id = ret.playerid;
                        temp.play.EnterGame(null,true);
                        break;
                    }
                case GameBase.Network.Internal.Define.ADDROLEDATA_ITEM_RET:
                    {
                        GameBase.Network.Internal.AddRoleData_Item_Ret ret = new GameBase.Network.Internal.AddRoleData_Item_Ret();
                        ret.Create(buff);
                        PlayerObject play = UserEngine.Instance().FindPlayerObjectToID(ret.gameid);
                        if (play == null)
                        {
                            Log.Instance().WriteLog("未找到玩家对象..在--ProcessDBNetMsg code:4");
                            break;
                        }
                        play.GetItemSystem().AwardItem_Ret(ret.sordid, ret.id);
                        break;
                    }
                case GameBase.Network.Internal.Define.LOADROLEDATA_ITEM:
                    {
                        GameBase.Network.Internal.ROLEDATA_ITEM item = new GameBase.Network.Internal.ROLEDATA_ITEM();
                        item.Create(buff);
                        TempPlayObject play = UserEngine.Instance().GetTempPlayObj(item.key,item.key2);
                        if (play == null)
                        {
                            Log.Instance().WriteLog("未找到玩家对象..在--ProcessDBNetMsg code:5");
                            break;
                        }
                        for (int i = 0; i < item.mListItem.Count; i++)
                        {
                            play.play.GetItemSystem().AddItemInfo(item.mListItem[i]);
                        }
                         break;
                    }
                case GameBase.Network.Internal.Define.LOADROLEDATA_MAGIC:
                    {
                        GameBase.Network.Internal.RoleData_Magic magic = new GameBase.Network.Internal.RoleData_Magic();
                        magic.Create(buff);
                        TempPlayObject play = UserEngine.Instance().GetTempPlayObj(magic.key, magic.key2);
                        if (play == null)
                        {
                            Log.Instance().WriteLog("未找到玩家对象..在--ProcessDBNetMsg code:6");
                            break;
                        }
                        for(int i = 0;i < magic.mListMagic.Count;i++)
                        {
                            GameBase.Network.Internal.MagicInfo info = magic.mListMagic[i];
                            play.play.GetMagicSystem().AddMagicInfo(info);
                        }
                       
                        break;
                    }
                case GameBase.Network.Internal.Define.KICKGAMEPLAY:
                    {
                        GameBase.Network.Internal.KickGamePlay kickplay = new GameBase.Network.Internal.KickGamePlay();
                        kickplay.Create(buff);
                        PlayerObject play = UserEngine.Instance().FindPlayerObjectToAccountId(kickplay.accountid);
                        if (play != null)
                        {
                            SessionManager.Instance().RemoveSession(play.GetGameSession().m_Socket);
                            play.Kick();
                        }
                        break;
                    }
                case GameBase.Network.Internal.Define.LOADROLEDATA_EUDEMON:
                    {
                        GameBase.Network.Internal.ROLEDATE_EUDEMON eudemon = new GameBase.Network.Internal.ROLEDATE_EUDEMON();
                        eudemon.Create(buff);
                      
                        TempPlayObject play = UserEngine.Instance().GetTempPlayObj(eudemon.key, eudemon.key2);
                        if (play == null)
                        {
                            //只是为了下断点后延迟问题解决方案
                            PlayerObject _play = UserEngine.Instance().FindPlayerObjectToPlayerId(eudemon.playerid);
                            if (_play == null)
                            {
                                Log.Instance().WriteLog("未找到玩家对象..在--ProcessDBNetMsg code:6");
                            }
                            else
                            {
                                _play.GetEudemonSystem().DB_Load(eudemon);
                                _play.GetEudemonSystem().SendAllEudemonInfo();
                            }
                            
                            break;
                        }
                        else
                        {
                            play.play.GetEudemonSystem().DB_Load(eudemon);
                        }
                      
                       
                        break;
                    }
                case GameBase.Network.Internal.Define.LOADROLEDATA_FRIEND:
                    {
                        GameBase.Network.Internal.ROLEDATA_FRIEND friend = new GameBase.Network.Internal.ROLEDATA_FRIEND();
                        friend.Create(buff);
                        TempPlayObject play = UserEngine.Instance().GetTempPlayObj(friend.key, friend.key2);
                        if (play == null)
                        {
                            //只是为了下断点后延迟问题解决方案
                            PlayerObject _play = UserEngine.Instance().FindPlayerObjectToPlayerId(friend.playerid);
                            if (_play == null)
                            {
                                Log.Instance().WriteLog("未找到玩家对象..在--ProcessDBNetMsg code:7");
                            }
                            else
                            {
                                
                                _play.GetFriendSystem().DB_Load(friend);
                                _play.GetFriendSystem().SendAllFriendInfo();
                            }
                           
                            break;
                        }
                        else
                        {
                            play.play.GetFriendSystem().DB_Load(friend);
                        }
                       
                        break;
                    }
                case GameBase.Network.Internal.Define.GUANJUEDATA: //爵位数据
                    {
                        GameBase.Network.Internal.GUANJUEINFO juewei = new GameBase.Network.Internal.GUANJUEINFO();
                        juewei.Create(buff);
                        GuanJueManager.Instance().DB_Load(juewei);
                        break;
                    }
                case GameBase.Network.Internal.Define.LOADLEGION: //军团数据
                    {
                        GameBase.Network.Internal.LEGIONINFO info = new GameBase.Network.Internal.LEGIONINFO();
                        info.Create(buff);
                        LegionManager.Instance().DB_Load(info);
                        break;
                    }
                case GameBase.Network.Internal.Define.CREATELEGION_RET://创建军团返回数据
                    {
                        GameBase.Network.Internal.CreateLegion_Ret info = new GameBase.Network.Internal.CreateLegion_Ret();
                        info.Create(buff);
                        LegionManager.Instance().CreateLegion_Ret(info);
                        break;
                    }
                case GameBase.Network.Internal.Define.LOADPAYRECINFO: //充值数据
                    {
                        GameBase.Network.Internal.PackPayRecInfo info = new GameBase.Network.Internal.PackPayRecInfo();
                        info.Creaet(buff);
                        PayManager.Instance().DB_Load(info);
                        break;
                    }
            }
        }

        public  GameBase.Network.TcpClient GetDBClient() { return mTcpDBClient; }
        //发送玩家数据信息到dbserver 保存到数据库
        //play 玩家对象
        //isExit 是否是退出游戏 -
        public void SaveRoleData(PlayerObject play,bool isExit = false)
        {
            if (!this.IsConnect())
            {
                UserEngine.Instance().AddCachePlay(play);
                Log.Instance().WriteLog("保存玩家数据失败,dbserver未连接,已加入到数据库缓冲存储区");
                return;
            }
            //人物基本属性
            SaveRoleData_Attr data = new SaveRoleData_Attr();
            GameStruct.PlayerAttribute attr = play.GetBaseAttr();
          
            data.accountid = attr.account_id;
            data.IsExit = isExit;
            data.name = play.GetName();
            data.lookface = attr.lookface;
            data.hair = attr.hair;
            data.level = (byte)attr.level;
            data.exp = attr.exp;
            data.life = attr.life;
            data.mana = attr.mana;
            data.profession = attr.profession;
            data.pk = attr.pk;
            data.gold = attr.gold;
            data.gamegold = attr.gamegold;
            data.stronggold = attr.stronggold;
            data.godlevel = attr.godlevel;
            data.maxeudemon = attr.maxeudemon;
            if (play.GetGameMap() == null)
            {
                data.mapid = 1000;
                data.x = 145;
                data.y = 413;
            }
            else
            {
                data.mapid = play.GetGameMap().GetMapInfo().id;
                data.x = play.GetCurrentX();
                data.y = play.GetCurrentY();
            }
      
            data.hotkey = play.GetHotKeyInfo();
            data.guanjue = attr.guanjue;
            GetDBClient().SendData(data.GetBuffer());
            //保存道具信息
            play.GetItemSystem().DB_Save();
            //保存技能信息
            play.GetMagicSystem().DB_Save();
            //保存幻兽信息
            play.GetEudemonSystem().DB_Save();
            //好友信息
            play.GetFriendSystem().DB_Save();
 
        }
    }
}
