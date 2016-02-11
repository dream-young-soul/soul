using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using GameBase.Config;
using GameBase.Network;
using System.Diagnostics;
//与客户端的全局游戏会话类
namespace MapServer
{
    public class SessionManager
    {
        private Dictionary<Socket, GameBase.Network.GameSession> m_DicSession = null;
        private static SessionManager m_Instance = null;
        public static SessionManager Instance()
        {
            if (m_Instance == null)
            {
                m_Instance = new SessionManager();
            }
            return m_Instance;
        }
        public SessionManager()
        {
            m_DicSession = new Dictionary<Socket, GameBase.Network.GameSession>();
            m_DicSession.Clear();
        }

        public void Dispose()
        {
            foreach (GameSession session in m_DicSession.Values)
            {
                session.Dispose();
            }
            m_DicSession.Clear();
            m_DicSession = null;
            m_Instance = null;
        }
        public void AddSession(Socket s, TcpServer server)
        {

            if (IsSession(s))
            {
                Log.Instance().WriteLog("重用套接字..");
            }
            GameSession session = new GameSession(s, server);
            m_DicSession[s] = session;


        }

        public void RemoveSession(Socket s)
        {

            if (IsSession(s))
            {
                GameSession session = m_DicSession[s];
                m_DicSession.Remove(s);
            }

        }

        public bool IsSession(Socket s)
        {
            return m_DicSession.ContainsKey(s);
        }

        public void AddNetData(Socket s, byte[] data, int nLen)
        {
            if (IsSession(s))
            {
                GameSession session = m_DicSession[s];
                if (session == null || session.m_GamePack == null) return;
                byte[] bydata = new byte[nLen];
                Buffer.BlockCopy(data, 0, bydata, 0, nLen);
                session.m_GamePack.ProcessNetData(bydata);
                session.m_nLastTime = System.Environment.TickCount;
            }

        }
        public void ProcessNetMsg()
        {

                foreach (GameSession session in m_DicSession.Values)
                {
                    if (session == null || session.m_GamePack == null) continue;
                    byte[] retdata = session.m_GamePack.GetData();
                    if (retdata != null)
                    {
                        GameBase.Network.PackIn packin = new GameBase.Network.PackIn(retdata);
                        PlayerObject play = UserEngine.Instance().FindPlayerObjectToID(session.gameid);
                        ushort tag = packin.ReadUInt16();
                        if (play != null)
                        {
                            play.ProcessNetMsg(tag, retdata);
                            continue;
                        }
                        //第一次的封包一定是更新key哒..
                        if (play == null && tag != PacketProtoco.C_UPDATEKEY && tag != PacketProtoco.C_QUERYCREATEROLENAME
                            && tag != PacketProtoco.C_CREATEROLE) continue;

                        switch (tag)
                        {
                            case PacketProtoco.C_UPDATEKEY:
                                {

                                    int key = packin.ReadInt32();
                                    int key2 = packin.ReadInt32();
                                    TempPlayObject tempplay = UserEngine.Instance().GetTempPlayObj(key, key2);
                                    if (tempplay == null) return; //没有经过loginsserver进入的非法封包
                                    tempplay.play.SetGameSession(session);
                                    session.GetGamePackKeyEx().SunUpdateKey(key, key2);
                                    NetMsg.MsgNotice msgNotice;
                                    //没有角色就创建角色
                                    if (!tempplay.isRole)
                                    {
                                        msgNotice = new NetMsg.MsgNotice();
                                        msgNotice.Create(null, session.GetGamePackKeyEx());
                                        session.SendData(msgNotice.GetCreateRoleBuff());
                                        return;
                                    }
                                    //有角色就进游戏
                                    UserEngine.Instance().RemoveTempPlayObject(tempplay.play.GetGameID());
                                    tempplay.play.EnterGame(session);
                                    break;
                                }
                            case PacketProtoco.C_QUERYCREATEROLENAME:   //创建角色名查询
                                {
                                    NetMsg.MsgQueryCreateRoleName info = new NetMsg.MsgQueryCreateRoleName();
                                    info.Create(retdata, null);
                                    int key  = 0,key2 = 0;
                                    session.GetGamePackKeyEx().GetKey(ref key,ref key2);
                                    TempPlayObject temp = UserEngine.Instance().GetTempPlayObj(key,key2);
                                    if(temp == null)
                                    {
                                        Log.Instance().WriteLog("找到玩家对象-在ProcessNetMsg code:1");
                                        break;
                                    }
                                    
                                    //发给dbserver 查询
                                    GameBase.Network.Internal.QueryRoleName query = new GameBase.Network.Internal.QueryRoleName();
                                    query.gameid = temp.play.GetGameID();
                                    query.name = info.GetName();
                                    
                                    DBServer.Instance().GetDBClient().SendData(query.GetBuffer());

                                    //NetMsg.MsgNotice notice = new NetMsg.MsgNotice();
                                    //notice.Create(null, session.GetGamePackKeyEx());
                                    //session.SendData(notice.GetQueryNameBuff());
                                    break;
                                }
                            case PacketProtoco.C_CREATEROLE:    //创建角色
                                {

                                    int key = 0, key2 = 0;
                                    session.GetGamePackKeyEx().GetKey(ref key, ref key2);
                                    TempPlayObject temp = UserEngine.Instance().GetTempPlayObj(key, key2);
                                    if (temp == null)
                                    {
                                        Log.Instance().WriteLog("找到玩家对象-在ProcessNetMsg code:2");
                                        break;
                                    }
                                    NetMsg.MsgCreateRoleInfo info = new NetMsg.MsgCreateRoleInfo();
                                    info.Create(retdata, null);
                                    if (info.GetName().Length <= 0)
                                    {
                                        Log.Instance().WriteLog("角色名称为空!!");
                                        break;
                                    }
                                    PlayerObject _play = temp.play;
                                    _play.SetGameSession(session);

                                    _play.SetName(info.GetName());
                                    _play.GetBaseAttr().profession = (byte)info.profession;
                                    _play.GetBaseAttr().lookface = info.lookface;
                                   
                         
                                    //发给dbserver
                                    GameBase.Network.Internal.CreateRole create = new GameBase.Network.Internal.CreateRole();
                                    create.accountid = temp.accountid;
                                    create.lookface = info.lookface;
                                    create.name = info.GetName();
                                    create.profession = (byte)info.profession;
                                    create.gameid = temp.play.GetGameID();
                                    DBServer.Instance().GetDBClient().SendData(create.GetBuffer());
                                   //测试游戏
                                    //play = new PlayerObject();
                                    //play.mapid = 1000;
                                    //play.mPoint.x = 400;
                                    //play.mPoint.y = 440;
                                    //session.gameid = play.GetGameID();
                                    //MapManager.Instance().GetGameMapToID(play.mapid).AddObject(play, session);
                                    //UserEngine.Instance().AddPlayerObject(play);

                                    ////公告信息
                                    //NetMsg.MsgNotice msgNotice = new NetMsg.MsgNotice();
                                    //msgNotice.Create(null, session.GetGamePackKeyEx());
                                    //session.SendData(msgNotice.GetStartGameBuff());
                                    //byte[] roledata = {238, 3, 64, 66, 15, 0, 241, 73, 2, 0, 101,
                                    //      0, 0, 0, 60, 1, 0, 0, 0, 0, 0, 0, 81, 118,
                                    //      203, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                    //      31, 1, 0, 0, 73, 0, 0, 0, 30, 0, 100, 0, 99,
                                    //      0, 102, 0, 0, 0, 202, 3, 222, 3, 134, 7, 0, 0,
                                    //      0, 0, 0, 0, 0, 0, 0, 0, 255, 10, 0, 0, 1, 0, 0,
                                    //      0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 12, 0, 0,
                                    //      0, 0, 0, 0, 0, 0, 0, 20, 0, 4, 0, 0, 0, 0, 0, 0, 0,
                                    //      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                    //      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                    //      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                    //      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                    //      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 105,
                                    //      0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                    //      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                    //      0, 0, 0, 0, 2, 8, 102, 101, 111, 119, 102, 113, 101, 119, 2, 
                                    //      206, 222, 0, 0, 0 };
                                    ////session.GetGamePackKeyEx().EncodePacket(ref roledata, roledata.Length);
                                    ////session.SendData(roledata);
                                    ////
                                    //NetMsg.MsgSelfRoleInfo rolemsg = new NetMsg.MsgSelfRoleInfo();
                                    //rolemsg.Create(roledata, session.GetGamePackKeyEx());
                                    //rolemsg.roletype = 140001;
                                    //rolemsg.roleid = play.GetTypeId();
                                    //rolemsg.name = "测试角色" + play.GetTypeId().ToString();
                                    //play.Name = rolemsg.name;
                                    //session.SendData(rolemsg.GetBuffer());


                                    ////测试增加装备 测试装备
                                    //NetMsg.MsgItemInfo item = new NetMsg.MsgItemInfo();
                                    //item.Create(null, session.GetGamePackKeyEx());
                                    //item.postion = NetMsg.MsgItemInfo.ITEMPOSITION_ARMOR;
                                    //item.id = 112434;
                                    //item.item_id = 135114;
                                    //item.amount = item.amount_limit = 1;
                                    //session.SendData(item.GetBuffer());
                                    ////测试武器
                                    //item = new NetMsg.MsgItemInfo();
                                    //item.Create(null, session.GetGamePackKeyEx());
                                    //item.postion = NetMsg.MsgItemInfo.ITEMPOSITION_WEAPONR;
                                    //item.id = 112435;
                                    //item.item_id = 440244;
                                    //item.amount = item.amount_limit = 1;
                                    //session.SendData(item.GetBuffer());



                                    //NetMsg.MsgUpdateSP sp = new NetMsg.MsgUpdateSP();
                                    //sp.Create(null, session.GetGamePackKeyEx());
                                    //sp.role_id = play.GetTypeId();
                                    //sp.value = 37;
                                    //sp.sp = 100;
                                    //session.SendData(sp.GetBuffer());

                                    //sp = new NetMsg.MsgUpdateSP();
                                    //sp.Create(null, session.GetGamePackKeyEx());
                                    //sp.role_id = play.GetTypeId();
                                    //session.SendData(sp.GetBuffer());


                                    ////测试新增技能
                                    //ushort[] skill = { 3011 };//, 3002, 1010, 3005, 3009, 8003
                                    //for (int i = 0; i < skill.Length; i++)
                                    //{
                                    //    NetMsg.MsgMagicInfo magicinfo = new NetMsg.MsgMagicInfo();
                                    //    magicinfo.Create(null, session.GetGamePackKeyEx());
                                    //    magicinfo.id = play.GetTypeId();
                                    //    magicinfo.magicid = skill[i];
                                    //    magicinfo.level = 2;
                                    //    session.SendData(magicinfo.GetBuffer());
                                    //}



                                    ////进入地图
                                    //NetMsg.MsgMapInfo mapinfo = new NetMsg.MsgMapInfo();
                                    //mapinfo.Create(null, session.GetGamePackKeyEx());
                                    //mapinfo.Init(play.mapid, play.mPoint.x, play.mPoint.y);
                                    //session.SendData(mapinfo.GetBuffer());



                                    ////刷新可视列表;

                                    //play.RefreshVisibleObject();
                                    //GameStruct.Action act = new GameStruct.Action(GameStruct.Action.MOVE);
                                    //play.PushAction(act);
                                    break;
                                }
                        }
                       


 
                }

            }
        }
    }
}
