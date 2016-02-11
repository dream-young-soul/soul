using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using GameBase.Network;
using GameBase;
using GameBase.Config;
using GameBase.Network.Internal;
using GameBase.Core;

namespace DBServer
{
    public class InternalSession
    {
        private Socket mSocket = null;
        private InternalPacket mPacket;
        private byte mType;             //会话类型
        private String mName;
        public int lastTime;        //最后会话时间
        public Socket GetSocket() { return mSocket; }
        public InternalPacket GetPacket() { return mPacket; }
        public byte GetSessionType() { return mType; }
        public void SetSessionType(byte _type) { mType = _type; }
        public String GetSessionName() { return mName; }
        public void SetSessionName(String _name) { mName = _name; }
        public int GetLastTime() { return lastTime; }
        public void SetLastTime(int _lasttime) { lastTime = _lasttime; }
        private TcpServer mTcpServer;
        public TcpServer GetTcpServer(){return mTcpServer;}
        public InternalSession(TcpServer server,Socket s)
        {
            mPacket = new InternalPacket();
            mName = "";
            mType = 0;
            lastTime = System.Environment.TickCount;
            mTcpServer = server;
            mSocket = s;
        }

        public void Run()
        {
            byte[] data = mPacket.GetData();
            if (data == null) return;
            PackIn inpack = new PackIn(data);
            ushort param = inpack.ReadUInt16();
            switch (param)
            {
                    
                case GameBase.Network.Internal.Define.OPENLOGINSERVER:
                case GameBase.Network.Internal.Define.OPENMAPSERVER:
                    {
                        mType = inpack.ReadByte();
                        mName = inpack.ReadString();
                        Log.Instance().WriteLog("server connect...type:" + mType.ToString() + " name:" + mName);
                        //如果是mapserver 组 就发额外的数据
                        if (mType == GameBase.Network.Internal.Define.TYPE_MAPSERVER)
                        {
                            //爵位
                            GuanJue.GetInstance().SendData(0);
                            //军团信息
                            Legion.GetInstance().SendData(0);
                            //充值信息
                            PayManager.Instance().SendData(0);
                        }
                        break;
                    }
                case GameBase.Network.Internal.Define.QUERYROLE:
                    {

                        ProcessQueryRole(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.ROLEINFO_RET:
                    {
                        ProcessRoleInfo_Ret(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.QUERYROLENAME:
                    {
                        ProcessQueryRoleName(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.CREATEROLE:
                    {
                        ProcessCreateRole(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.SAVEROLEDATA_ATTR:
                    {
                        ProcessSaveRoleData_Attr(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.ADDROLEDATA_ITEM:
                    {
                        ProcessAddRoleData_Item(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.DELETEROLEDATA_ITEM:
                    {
                        ProcessDeleteRoleData_Item(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.SAVEROLEDATA_ITEM:
                    {
                        ProcessSaveRoleData_Item(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.SAVEROLEDATA_MAGIC:
                    {
                        ProcessSaveRoleData_Magic(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.SAVEROLEDATA_EUDEMON:
                    {
                        ProcessSaveRoleData_Eudemon(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.SAVEROLEDATA_FRIEND:
                    {
                        ProcessSaveRoleData_Friend(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.UPDATEGUANJUEDATA:
                    {
                        ProcessUpdateGuanJueData(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.CREATELEGION:
                    {
                        ProcessCreateLegion(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.UPDATELEGION:
                    {
                        ProcessUpdateLegion(data);
                        break;
                    }
                case GameBase.Network.Internal.Define.UPDATEPAYRECINFO:
                    {
                        ProcessUpdatePayrecInfo(data);
                        break;
                    }
            }
        }


        private void ProcessQueryRole(byte[] data)
        {
            GameBase.Network.Internal.QueryRole info = new GameBase.Network.Internal.QueryRole();
            info.Create(data);
            byte ret = 0;
            String sAccount = info.GetAccount();
            int accountid = Data.QueryAccount(sAccount);
          
            //查询数据库是否有该角色--
            if (accountid != -1)
            {
                ret = 1;
                //-优先查询是否在线,要是在线就踢掉了--
                int mapserverindex = -1;
                if (Data.IsOnline(sAccount,ref mapserverindex))
                {
                    ret = 2;
                    //发送给mapserver踢掉该玩家,因为被挤下线了
                    GameBase.Network.Internal.KickGamePlay kickplay = new GameBase.Network.Internal.KickGamePlay();
                    kickplay.accountid = accountid;
                    SessionManager.Instance().SendMapServer(mapserverindex, kickplay.GetBuffer());
                    //设置该玩家帐号为离线状态
                    Data.SetOnlineState(accountid, -1);

                }
                if (ret == 1)
                {
                    Log.Instance().WriteLog("查询角色成功:" + sAccount+"id:"+accountid.ToString());
                    //发送给mapserver服务器
                    GameBase.Network.Internal.RoleInfo roleinfo = Data.QueryRoleInfo(accountid);
                    roleinfo.gameid = info.gameid;
                    roleinfo.mKey = info.key;
                    roleinfo.mKey1 = info.key2;
                    roleinfo.sAccount = sAccount;
                    SessionManager.Instance().SendMapServer(roleinfo.mapid, roleinfo.GetBuffer());

                    //如果有角色，读取角色的其他信息
                    //物品数据
                    GameBase.Network.Internal.ROLEDATA_ITEM item = new GameBase.Network.Internal.ROLEDATA_ITEM();
                    item.key = info.key;
                    item.key2 = info.key2;
                    item.playerid = roleinfo.playerid;
                    item.SetLoadTag();
                    Data.LoadRoleData_Item(item);
                    if (item.mListItem.Count > 0) //没数据就不发了。。
                    {
                        SessionManager.Instance().SendMapServer(0, item.GetBuffer());
                    }
                    //技能数据
                    GameBase.Network.Internal.RoleData_Magic magic = new GameBase.Network.Internal.RoleData_Magic();
                    magic.SetLoadTag();
                    magic.ownerid = roleinfo.playerid;
                    magic.key = roleinfo.mKey;
                    magic.key2 = roleinfo.mKey1;
                    Data.LoadRoleData_Magic(magic);
                    if (magic.mListMagic.Count > 0)
                    {
                        SessionManager.Instance().SendMapServer(0, magic.GetBuffer());
                    }
                    //幻兽数据--
                    List<RoleData_Item> list_eudemondata = item.GetEudemonItemList();
                    if (list_eudemondata != null)
                    {
                        GameBase.Network.Internal.ROLEDATE_EUDEMON eudemon = new GameBase.Network.Internal.ROLEDATE_EUDEMON();
                        eudemon.SetLoadTag();
                        eudemon.playerid = roleinfo.playerid;
                        eudemon.key = roleinfo.mKey;
                        eudemon.key2 = roleinfo.mKey1;
                        Data.LoadRoleData_Eudemon(eudemon);
                        SessionManager.Instance().SendMapServer(0, eudemon.GetBuffer());
                    }
                    //好友数据
                    GameBase.Network.Internal.ROLEDATA_FRIEND friend = new GameBase.Network.Internal.ROLEDATA_FRIEND();
                    friend.SetLoadTag();
                    friend.playerid = roleinfo.playerid;
                    friend.key = roleinfo.mKey;
                    friend.key2 = roleinfo.mKey1;
                    Data.LoadRoleData_Friend(friend);
                    SessionManager.Instance().SendMapServer(0, friend.GetBuffer());
                }
                else
                {
                    Log.Instance().WriteLog("查询角色失败:" + sAccount + "id:" + accountid.ToString());
                }
 
              
            }
            if (ret == 1) return; //有该角色
            //发送给loginserver服务器--
            GameBase.Network.Internal.QueryRole_Ret queryrole_ret = new GameBase.Network.Internal.QueryRole_Ret();
            queryrole_ret.gameid = info.gameid;
            queryrole_ret.key = info.key;
            queryrole_ret.key2 = info.key2;
            queryrole_ret.ret = ret;
  
            mTcpServer.SendData(mSocket, queryrole_ret.GetBuffer());

        }

        //角色信息返回
        private void ProcessRoleInfo_Ret(byte[] data)
        {
         
            GameBase.Network.Internal.RoleInfo_Ret ret = new GameBase.Network.Internal.RoleInfo_Ret();
            ret.Create(data);
            Data.SetOnlineState(ret.accountid, 0);
            //发给loginserver服务器
            GameBase.Network.Internal.QueryRole_Ret queryrole_ret = new GameBase.Network.Internal.QueryRole_Ret();
            queryrole_ret.gameid = ret.gameid;
            queryrole_ret.key = ret.key;
            queryrole_ret.key2 = ret.key2;
            queryrole_ret.ret = 1;
            SessionManager.Instance().SendLoginServer(queryrole_ret.GetBuffer());
            Log.Instance().WriteLog("通知loginserver登录服务器:" + ret.accountid.ToString());
        }

        private void ProcessQueryRoleName(byte[] data)
        {
            GameBase.Network.Internal.QueryRoleName info = new GameBase.Network.Internal.QueryRoleName();
            info.Create(data);
            QueryRoleName_Ret ret = Data.QueryRoleName(info.name);
            ret.gameid = info.gameid;

            SessionManager.Instance().SendMapServer(0,ret.GetBuffer());
   
        }

        public void ProcessCreateRole(byte[] data)
        {
            GameBase.Network.Internal.CreateRole info = new GameBase.Network.Internal.CreateRole();
            info.Create(data);


            GameBase.Network.Internal.CreateRole_Ret ret = new GameBase.Network.Internal.CreateRole_Ret();
            ret.gameid = info.gameid;


            info.name = Coding.GB2312ToLatin1(info.name);
            ret.tag = Data.CreateRole(info.accountid, info.name, info.lookface, info.profession,ref ret.playerid);
            SessionManager.Instance().SendMapServer(0,ret.GetBuffer());
        }

        public void ProcessSaveRoleData_Attr(byte[] data)
        {
            GameBase.Network.Internal.SaveRoleData_Attr info = new GameBase.Network.Internal.SaveRoleData_Attr();
            info.Create(data);
            //设置角色为离线状态
            if (info.IsExit)
            {
                Data.SetOnlineState(info.accountid, -1);
            }
            if (!Data.SaveRoleData_Attr(info))
            {
                Log.Instance().WriteLog("保存角色信息失败,角色id:" + info.accountid.ToString() + " 角色名称:" + info.name);
            } 
        }

        public void ProcessAddRoleData_Item(byte[] data)
        {
            GameBase.Network.Internal.AddRoleData_Item info = new GameBase.Network.Internal.AddRoleData_Item();
            info.Create(data);
            uint _key = 0;
            if (!Data.AddRoleData_Item(info, ref _key))
            {
                Log.Instance().WriteLog("保存角色物品信息失败,角色id:" + info.item.playerid.ToString());
              
            }
            GameBase.Network.Internal.AddRoleData_Item_Ret ret = new GameBase.Network.Internal.AddRoleData_Item_Ret();
            ret.id = _key;
            ret.gameid = info.gameid;
            ret.sordid = info.sortid;
            SessionManager.Instance().SendMapServer(0, ret.GetBuffer());
        }

        public void ProcessDeleteRoleData_Item(byte[] data)
        {
            GameBase.Network.Internal.DeleteItemByID info = new GameBase.Network.Internal.DeleteItemByID();
            info.Create(data);
            if(!Data.DeleteRoleData_Item(info.playerid,info.id))
            {
                Log.Instance().WriteLog("删除角色物品信息失败,角色id:" + info.playerid.ToString());
            }
            const int EUDEMON_PACK = 53;//幻兽位置
            if (info.postion == EUDEMON_PACK)
            {
                Data.DeleteRoleData_Eudemon(info.playerid,info.id);
            }

        }

        public void ProcessSaveRoleData_Item(byte[] data)
        {
            GameBase.Network.Internal.ROLEDATA_ITEM info = new GameBase.Network.Internal.ROLEDATA_ITEM();
            info.Create(data);
            Data.SaveRoleData_Item(info);
        }

        public void ProcessSaveRoleData_Magic(byte[] data)
        {
            GameBase.Network.Internal.RoleData_Magic info = new GameBase.Network.Internal.RoleData_Magic();
            info.Create(data);
            Data.SaveRoleData_Magic(info);
        }

        public void ProcessSaveRoleData_Eudemon(byte[] data)
        {
            GameBase.Network.Internal.ROLEDATE_EUDEMON info = new GameBase.Network.Internal.ROLEDATE_EUDEMON();
            info.Create(data);
            Data.SaveRoleData_Eudemon(info);
        }
        public void ProcessSaveRoleData_Friend(byte[] data)
        {
            GameBase.Network.Internal.ROLEDATA_FRIEND info = new GameBase.Network.Internal.ROLEDATA_FRIEND();
            info.Create(data);
            Data.SaveRoleData_Friend(info);
        }

        public void ProcessUpdateGuanJueData(byte[] data)
        {
            GameBase.Network.Internal.UPDATEGUANJUEDATA info = new GameBase.Network.Internal.UPDATEGUANJUEDATA();
            info.Create(data);
            GuanJue.GetInstance().UpdateGuanJueInfo(info.info);
        }

        private void ProcessCreateLegion(byte[] data)
        {
            GameBase.Network.Internal.LegionOption option = new GameBase.Network.Internal.LegionOption();
            option.Create(data);
            Legion.GetInstance().CreateLegion(option.mInfo,option.player_id);
        }

        private void ProcessUpdateLegion(byte[] data)
        {
            GameBase.Network.Internal.LegionOption option = new GameBase.Network.Internal.LegionOption();
            option.Create(data);
            Legion.GetInstance().UpdateLegion(option.mInfo);
        }

        private void ProcessUpdatePayrecInfo(byte[] data)
        {
            GameBase.Network.Internal.PackUpdatePayRecInfo option = new GameBase.Network.Internal.PackUpdatePayRecInfo();
            option.Create(data);
            PayManager.Instance().SetPayTag(option.account);
        }
    }
}
