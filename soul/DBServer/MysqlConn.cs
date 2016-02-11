using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using GameBase.Config;
using GameBase.Network.Internal;
using GameBase.Core;
using System.Data;

namespace DBServer
{

    public class MysqlString
    {
        public const String ADDACCOUNT = "insert into account(account,password,vip) values('{0}','{1}',{2})";
        //等级初始为1级
        public const String CREATE = "insert into cq_user(accountid,name,lookface,profession,level) values({0},'{1}',{2},{3},1)";
        public const String SAVEROLE_ATTR = "update cq_user set name='{0}',lookface={1},hair={2},level={3},exp={4},life={5}," +
            "mana={6},profession={7},pk={8},gold={9},gamegold={10},stronggold={11},mapid={12},record_x={13},record_y={14},hotkey='{15}',guanjue={16},godlevel={17},maxeudemon={18} where accountid={19} ";
        public const String SAVEROLE_ITEM = "insert into cq_item(playerid,itemid,postion,stronglv,gem1,gem2,forgename,amount,war_ghost_exp,di_attack,shui_attack,huo_attack,feng_attack,property,gem3,god_exp,god_strong) values" +
            "({0},{1},{2},{3},{4},{5},'{6}',{7},{8},{9},{10},{11},{12},{13},{14},{15},{16})";
        public const String UPDATEROLE_ITEM = "update cq_item set itemid={0},postion={1},stronglv={2},gem1={3},gem2={4},forgename='{5}'," +
            "amount={6},war_ghost_exp={7},di_attack={8},shui_attack={9},huo_attack={10},feng_attack={11},property={12},gem3={13},god_exp={14},god_strong={15} where playerid={16} and id={17}";
        public const String LOADROLEDATA_ITEM = "select * from cq_item where playerid={0}";
        public const String DELETEROLEDATA_ITEM = "delete from cq_item where playerid={0} and id ={1}";
        public const String LOADROLEDATA_MAGIC = "select * from cq_magic where ownerid={0}";
        public const String ADDMAGIC = "insert into cq_magic(ownerid,magicid,level,exp) values({0},{1},{2},{3})";
        public const String UPDATEMAGIC = "update cq_magic set magicid={0},level={1},exp={2} where ownerid={3} and id={4}";
        public const String UPDATEONLINESTATE = "update account set serverindex ={0} where id={1}";
        public const String LOADROLEDATA_EUDEMON = "select * from cq_eudemon where ownerid ={0}";
        public const String SAVEROLEDATA_EUDEMON = "insert into cq_eudemon(itemid,ownerid,name,phyatk_grow_rate,phyatk_grow_rate_max,magicatk_grow_rate,magicatk_grow_rate_max,life_grow_rate,defense_grow_rate,magicdef_grow_rate,life,atk_min,atk_max,magicatk_min,magicatk_max,defense,magicdef,luck,intimacy,level,card,exp,quality,wuxing,recall_count) values" +
              "({0},{1},'{2}',{3},{4},{5},{6},'{7}',{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24})";
        public const String UPDATEROLEDATA_EUDEMON = "update cq_eudemon set itemid={0},ownerid={1},name='{2}',phyatk_grow_rate={3},phyatk_grow_rate_max={4},magicatk_grow_rate={5},magicatk_grow_rate_max={6},life_grow_rate={7},defense_grow_rate={8},magicdef_grow_rate={9},life={10},atk_min={11},atk_max={12},magicatk_min={13},magicatk_max={14},defense={15},magicdef={16},luck={17},intimacy={18},level={19},card={20},exp={21},quality={22},wuxing={23},recall_count={24}" +
            " where id={25}";
        public const String DELETEROLEDATA_EUDEMON = "delete from cq_eudemon where id={0} and ownerid={1}";

        public const String LOADROLEDATA_EUDEMON_MAGIC = "select * from cq_eudemon_magic where ownerid={0}";
        public const String ADD_EUDEMON_MAGIC = "insert into cq_eudemon_magic(ownerid,magicid,level,exp) values({0},{1},{2},{3})";
        public const String UPDATE_EUDEMON_MAGIC = "update cq_eudemon_magic set magicid={0},level={1},exp={2} where ownerid={3} and id={4}";
        public const String DELETE_EUDEMON_MAGIC = "delete from cq_eudemon_magic where magicid={0}";

        public const String LOADROLEDATA_FRIEND = "select * from cq_friend where userid={0}";
        public const String SAVEROLEDATA_FRIEND = "insert into cq_friend(userid,friendtype,friendid,friendname) values({0},{1},{2},'{3}')";
        public const String DELETEROLEDATA_FRIEND = "delete from cq_friend where userid={0} and friendid={1}";

        //军团
        public const String CREATE_LEGION = "insert into cq_legion(name,member_title,leader_id,leader_name,money,notice) values('{0}',{1},{2},'{3}',{4},'{5}')";
        public const String UPDATE_LEGION = "update cq_legion set name='{0}',member_title={1},leader_id={2},leader_name='{3}',money={4},notice='{5}' where id={6}";
        public const String CREATE_LEGION_MEMBERS = "insert into cq_legion_members(legion_id,members_name,money,rank) values({0},'{1}',{2},{3})";
        public const String UPDATE_LEGION_MEMBERS = "update cq_legion_members set money={0},rank={1} where legion_id={2} and members_name='{3}'";
    }
    public class MysqlConn
    {
        private static MySqlConnection conn = null;
        private static String msIP = "";
        private static int mnPort = 0;
        private static String msUser;
        private static String msPaswd;
        private static String msDatabase;
        public static bool Connect(String ip, int port, String user, String paswd, String database)
        {
            try
            {
          
                msIP = ip;
                mnPort = port;
                msUser = user;
                msPaswd = paswd;
                msDatabase = database;
                String mysqlStr = "Database={0};Data Source={1};User Id={2};Password={3};Charset=latin1;pooling=false;port={4};Pooling=true;Min Pool Size=0;Max Pool Size=100;Connection Lifetime=0;";//
                mysqlStr = String.Format(mysqlStr, database, ip, user, paswd, port.ToString());
                conn = new MySqlConnection(mysqlStr);
                
                //MySqlCommand setformat = new MySqlCommand("set names gb2312", conn);
                //conn.Open();
                //setformat.ExecuteNonQuery();
                //setformat.Dispose();
                //conn.Close();
            }
            catch (System.Exception ex)
            {
                Log.Instance().WriteLog("连接mysql出错." + ex.Message);
                return false;
            }
            return true;
        }

        public static MySqlConnection GetConn() 
        {
           
            return conn; 
        }
        public static void Conn_Open()
        {
            
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();
            }
            catch (System.Exception ex)
            {
                bool ret = Connect(msIP, mnPort, msUser, msPaswd, msDatabase);
                if (ret == false)
                {
                    Log.Instance().WriteLog("mysql意外断线，重连失败！！！");
                }
            }
           
        }
        public static void Conn_Close()
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            
        }
        public static void Dispose()
        {
            if (conn == null) return;
            conn.Dispose();
        }


    }


    public class Data
    {

        public static int GetAccountId(String sAcc)
        {
            MySqlCommand command = new MySqlCommand("select * from account where account = '" + sAcc + "'", MysqlConn.GetConn());
          
            MysqlConn.Conn_Open();
            MySqlDataReader reader = command.ExecuteReader();
            int accountid = -1;
            if (reader.Read())
            {
                if (reader.HasRows)
                {
                    accountid = reader.GetInt32("id");
                }
            }

            MysqlConn.Conn_Close();
            command.Dispose();
            return accountid;
        }
        //角色是否在线
        public static bool IsOnline(String sAcc, ref int mapserverindex)
        {
            MySqlCommand command = new MySqlCommand("select * from account where account = '" + sAcc + "'", MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            MySqlDataReader reader = command.ExecuteReader();
            bool ret = true;
            if (reader.Read())
            {
                if (reader.HasRows)
                {
                    ret = reader.GetInt32("serverindex") != -1;
                    if (ret)
                    {
                        mapserverindex = reader.GetInt32("serverindex");
                    }

                }
            }

            MysqlConn.Conn_Close();
            command.Dispose();
            return ret;
        }

        //设置在线状态 -1为离线 否则为地图服务器序号
        public static void SetOnlineState(int accountid, int mapserverindex)
        {
            MySqlCommand command;
            String sql = String.Format(MysqlString.UPDATEONLINESTATE, mapserverindex, accountid);
            command = new MySqlCommand(sql, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            command.ExecuteNonQuery();
            MysqlConn.Conn_Close();
            command.Dispose();
        }
        //查询帐号信息--
        public static int QueryAccount(String sAcc)
        {
            MySqlCommand command;
            int accountid = GetAccountId(sAcc);
            //测试模式就创建该帐号
            if (accountid == -1 && Global.mbTestMode && sAcc.Length > 0/*防止空帐号*/)
            {
                String sql = String.Format(MysqlString.ADDACCOUNT, sAcc, "123456", 1);
                command = new MySqlCommand(sql, MysqlConn.GetConn());
                MysqlConn.Conn_Open();
                command.ExecuteNonQuery();
                MysqlConn.Conn_Close();
                command.Dispose();
                //重新查询一下嘛。
                accountid = GetAccountId(sAcc);
            }

            return accountid;
        }
        private static string DBStringToNormal(string dbStr)
        {
            byte[] str = new byte[dbStr.Length];
            for (int i = 0; i < dbStr.Length; ++i)
                str[i] = (byte)(dbStr[i]);
           // return System.Text.Encoding..GetString(str, 0, str.Length);
            return Coding.GetDefauleCoding().GetString(str, 0, str.Length);
        }
        //查询角色信息
        public static GameBase.Network.Internal.RoleInfo QueryRoleInfo(int accountid)
        {
            GameBase.Network.Internal.RoleInfo ret = new GameBase.Network.Internal.RoleInfo();
            ret.isRole = false;
            ret.accountid = accountid;
            MySqlCommand command = new MySqlCommand("select * from cq_user where accountid = '" + accountid.ToString() + "'", MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            MySqlDataReader reader = command.ExecuteReader();
            //只有一个角色
            if (reader.Read())
            {
                ret.isRole = true;
                ret.playerid = reader.GetInt32("id");
                ret.name = reader.GetString("name");
                ret.name = Coding.Latin1ToGB2312(ret.name);
       
                ret.lookface = reader.GetUInt32("lookface");
                ret.hair = reader.GetUInt32("hair");
                ret.lv = reader.GetByte("level");
                ret.exp = reader.GetUInt32("exp");
                ret.life = reader.GetUInt32("life");
                ret.mana = reader.GetUInt32("mana");
                ret.profession = reader.GetByte("profession");
                ret.pk = reader.GetInt16("pk");
                ret.gold = reader.GetInt32("gold");
                ret.gamegold = reader.GetInt32("gamegold");
                ret.stronggold = reader.GetInt32("stronggold");
                ret.mapid = reader.GetInt32("mapid");
                ret.x = reader.GetInt16("record_x");
                ret.y = reader.GetInt16("record_y");
                ret.hotkey = reader.GetString("hotkey");
                ret.guanjue = reader.GetUInt64("guanjue");
                ret.godlevel = reader.GetByte("godlevel");
                ret.maxeudemon = reader.GetByte("maxeudemon");
            }

            //while (reader.Read())
            //{
            //    if (reader.HasRows)
            //    {

            //        break;
            //    }
            //}
            MysqlConn.Conn_Close();
            command.Dispose();
            return ret;
        }

        public static QueryRoleName_Ret QueryRoleName(String name)
        {

            QueryRoleName_Ret ret = new QueryRoleName_Ret();
            //优先检测要过滤的字符串名称
            bool isTag = false;
            if (Filter.Instance().CheckFileterName(name))
            {
                isTag = true;
            }
            if (!isTag)
            {
                MySqlCommand command = new MySqlCommand("select * from cq_user where name = '" + name + "'", MysqlConn.GetConn());
                MysqlConn.Conn_Open();
                MySqlDataReader reader = command.ExecuteReader();

                reader.Read();
                if (reader.HasRows)
                {
                    isTag = true; //存在该角色
                }
                MysqlConn.Conn_Close();
                command.Dispose();
            }

            ret.tag = isTag;
            return ret;
        }

        public static bool CreateRole(int accountid, String name, uint lookface, byte professin, ref int playerid)
        {
            try
            {

                MySqlCommand command;
                String sql = String.Format(MysqlString.CREATE, accountid.ToString(), name, lookface.ToString(), professin.ToString());
                String utf_sql = sql;
                command = new MySqlCommand(utf_sql, MysqlConn.GetConn());
                MysqlConn.Conn_Open();
                command.ExecuteNonQuery();
                MysqlConn.Conn_Close();
                command.Dispose();

                //取主键-- 不能用于多线程或者多个程序操作该数据库。。切记
                String _key = "select max(id) from cq_user";
                command = new MySqlCommand(_key, MysqlConn.GetConn());
                MysqlConn.Conn_Open();
                MySqlDataReader reader = command.ExecuteReader();

                reader.Read();
                if (reader.HasRows)
                {
                    playerid = reader.GetInt32(0);
                }
                MysqlConn.Conn_Close();
                command.Dispose();
            }
            catch (System.Exception ex)
            {
                Log.Instance().WriteLog("createrole error!");
                Log.Instance().WriteLog(ex.Message);
                Log.Instance().WriteLog(ex.StackTrace);
                return false;
            }

            return true;
        }

        public static bool SaveRoleData_Attr(GameBase.Network.Internal.SaveRoleData_Attr info)
        {
            //加一个判断弥补之前的bug--负数 2016.1.25
            if (info.gamegold < 0) info.gamegold = 0;
            if (info.gold < 0) info.gold = 0;
            MySqlCommand command = null;
            String name_latin1 = Coding.GB2312ToLatin1(info.name);
            String sql = "";
            try
            {

                sql = String.Format(MysqlString.SAVEROLE_ATTR, name_latin1, info.lookface, info.hair, info.level, info.exp, info.life, info.mana, info.profession,
                    info.pk, info.gold, info.gamegold, info.stronggold, info.mapid, info.x, info.y, info.hotkey, info.guanjue,info.godlevel,info.maxeudemon,info.accountid);
                String utf_sql = sql;
                command = new MySqlCommand(utf_sql, MysqlConn.GetConn());
                MysqlConn.Conn_Open();
                command.ExecuteNonQuery();
                MysqlConn.Conn_Close();
                command.Dispose();

            }
            catch (System.Exception ex)
            {
                Log.Instance().WriteLog("SaveRoleData_Attr error!");
                Log.Instance().WriteLog(ex.Message);
                Log.Instance().WriteLog(ex.StackTrace);
                Log.Instance().WriteLog("sql语句:" + sql);
               // if (MysqlConn.GetConn().State == ConnectionState.Open)
              //  {
                //    MysqlConn.GetConn().Close();
              //  }
                if (command != null) command.Dispose();
                return false;
            }
            return true;
        }

        public static bool AddRoleData_Item(GameBase.Network.Internal.AddRoleData_Item info, ref uint nkey)
        {
            try
            {
               
                info.item.forgename = Coding.GB2312ToLatin1(info.item.forgename);
                MySqlCommand command;
                String sql = String.Format(MysqlString.SAVEROLE_ITEM, info.item.playerid, info.item.itemid, info.item.postion, info.item.stronglv,  info.item.gem1,
                    info.item.gem2, info.item.forgename, info.item.amount, info.item.war_ghost_exp, info.item.di_attack, info.item.shui_attack, info.item.huo_attack, info.item.feng_attack,
                    info.item.property, info.item.gem3, info.item.god_exp, info.item.god_strong);
                String utf_sql = sql;
                command = new MySqlCommand(utf_sql, MysqlConn.GetConn());
                MysqlConn.Conn_Open();
                command.ExecuteNonQuery();

                MysqlConn.Conn_Close();
                command.Dispose();

                //取主键-- 不能用于多线程或者多个程序操作该数据库。。切记
                String _key = "select max(id) from cq_item";
                command = new MySqlCommand(_key, MysqlConn.GetConn());
                MysqlConn.Conn_Open();
                MySqlDataReader reader = command.ExecuteReader();
                nkey = 0;
                reader.Read();
                if (reader.HasRows)
                {
                    nkey = reader.GetUInt32(0);
                }
                MysqlConn.Conn_Close();
                command.Dispose();
            }
            catch (System.Exception ex)
            {
                Log.Instance().WriteLog("createrole error!");
                Log.Instance().WriteLog(ex.Message);
                Log.Instance().WriteLog(ex.StackTrace);
                return false;
            }

            return true;
        }

        public static void LoadRoleData_Item(GameBase.Network.Internal.ROLEDATA_ITEM info)
        {
            MySqlCommand command;
            String _key = String.Format(MysqlString.LOADROLEDATA_ITEM, info.playerid);
            command = new MySqlCommand(_key, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.HasRows) break;
                GameBase.Network.Internal.RoleData_Item item = new GameBase.Network.Internal.RoleData_Item();
                item.id = reader.GetUInt32("id");
                item.playerid = reader.GetInt32("playerid");
                item.itemid = reader.GetUInt32("itemid");
                item.postion = reader.GetUInt16("postion");
                item.stronglv = reader.GetByte("stronglv");
             
                item.gem1 = reader.GetByte("gem1");
                item.gem2 = reader.GetByte("gem2");
                item.forgename = reader.GetString("forgename");
                if (item.forgename.Length > 0)
                {
                    item.forgename = Coding.Latin1ToGB2312(item.forgename);
                }
                item.amount = reader.GetUInt16("amount");
                item.war_ghost_exp = reader.GetInt32("war_ghost_exp");
                item.di_attack = reader.GetByte("di_attack");
                item.shui_attack = reader.GetByte("shui_attack");
                item.huo_attack = reader.GetByte("huo_attack");
                item.feng_attack = reader.GetByte("feng_attack");
                item.property = reader.GetInt32("property");
                item.gem3 = reader.GetByte("gem3");
                item.god_exp = reader.GetInt32("god_exp");
                item.god_strong = reader.GetInt32("god_strong");

                info.mListItem.Add(item);
            }
            MysqlConn.Conn_Close();
            command.Dispose();

        }
        public static void SaveRoleData_Item(GameBase.Network.Internal.ROLEDATA_ITEM info)
        {
            try
            {
                MySqlCommand command;
                String sql;
                for (int i = 0; i < info.mListItem.Count; i++)
                {
                    GameBase.Network.Internal.RoleData_Item item = info.mListItem[i];
                    if (item.forgename.Length > 0)
                    {
                        item.forgename = Coding.GB2312ToLatin1(item.forgename);
                    }
                    if (item.id == 0) //id为0就插入
                    {
                        sql = String.Format(MysqlString.SAVEROLE_ITEM, info.playerid, item.itemid, item.postion, item.stronglv, item.gem1,
                        item.gem2, item.forgename, item.amount, item.war_ghost_exp, item.di_attack, item.shui_attack, item.huo_attack, item.feng_attack,
                        item.property, item.gem3, item.god_exp, item.god_strong);
                    }
                    else
                    {
                        sql = String.Format(MysqlString.UPDATEROLE_ITEM, item.itemid, item.postion, item.stronglv, item.gem1, item.gem2, item.forgename,
                        item.amount, item.war_ghost_exp, item.di_attack, item.shui_attack, item.huo_attack, item.feng_attack, item.property, item.gem3, item.god_exp, item.god_strong, info.playerid, item.id);
                    }
                    command = new MySqlCommand(sql, MysqlConn.GetConn());
                    MysqlConn.Conn_Open();
                    command.ExecuteNonQuery();

                    MysqlConn.Conn_Close();
                    command.Dispose();
                }
            }
            catch (System.Exception ex)
            {
                Log.Instance().WriteLog("---------------------------------------------------------------------------");
                Log.Instance().WriteLog("保存角色道具信息失败.角色id:" + info.playerid.ToString());
                Log.Instance().WriteLog(ex.Message);
                Log.Instance().WriteLog(ex.StackTrace);
                Log.Instance().WriteLog("---------------------------------------------------------------------------");
            }

        }

        public static void SaveRoleData_Magic(GameBase.Network.Internal.RoleData_Magic info)
        {
            MySqlCommand command;
            String sql;
            for (int i = 0; i < info.mListMagic.Count; i++)
            {
                GameBase.Network.Internal.MagicInfo item = info.mListMagic[i];
                if (item.id == 0)
                {
                    sql = String.Format(MysqlString.ADDMAGIC, info.ownerid, item.magicid, item.level, item.exp);
                }
                else
                {
                    sql = String.Format(MysqlString.UPDATEMAGIC, item.magicid, item.level, item.exp, info.ownerid, item.id);
                }
                command = new MySqlCommand(sql, MysqlConn.GetConn());
                MysqlConn.Conn_Open();
                command.ExecuteNonQuery();

                MysqlConn.Conn_Close();
                command.Dispose();
            }
        }
        public static bool DeleteRoleData_Item(int playerid, uint id)
        {
            MySqlCommand command;
            String sql = String.Format(MysqlString.DELETEROLEDATA_ITEM, playerid, id);
            command = new MySqlCommand(sql, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            command.ExecuteNonQuery();
            MysqlConn.Conn_Close();
            command.Dispose();
            return true;
        }

      
        
        public static void LoadRoleData_Magic(GameBase.Network.Internal.RoleData_Magic info)
        {
            MySqlCommand command;
            String _key = String.Format(MysqlString.LOADROLEDATA_MAGIC, info.ownerid);
            command = new MySqlCommand(_key, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.HasRows) break;
                GameBase.Network.Internal.MagicInfo item = new GameBase.Network.Internal.MagicInfo();
                item.id = reader.GetInt32("id");
                item.magicid = reader.GetUInt32("magicid");
                item.level = reader.GetByte("level");
                item.exp = reader.GetUInt32("exp");
                info.mListMagic.Add(item);
            }
            MysqlConn.Conn_Close();
            command.Dispose();
        }

        //载入幻兽技能数据
        public static void LoadRoleData_Eudemon_MagicInfo(GameBase.Network.Internal.ROLEDATE_EUDEMON info)
        {
            MySqlCommand command;
            for (int i = 0; i < info.list_item.Count; i++)
            {
                RoleData_Eudemon item = info.list_item[i];
                String _key = String.Format(MysqlString.LOADROLEDATA_EUDEMON_MAGIC, item.id);
                command = new MySqlCommand(_key, MysqlConn.GetConn());
                MysqlConn.Conn_Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.HasRows) break;
                    GameBase.Network.Internal.MagicInfo itemex = new GameBase.Network.Internal.MagicInfo();
                    itemex.id = reader.GetInt32("id");
                    itemex.ownerid = reader.GetInt32("ownerid");
                    itemex.magicid = reader.GetUInt32("magicid");
                    itemex.exp = reader.GetUInt32("level");
                    item.mListMagicInfo.Add(itemex);
                }
                MysqlConn.Conn_Close();
                command.Dispose();
            }
        }

        //载入幻兽数据
        public static void LoadRoleData_Eudemon(GameBase.Network.Internal.ROLEDATE_EUDEMON info)
        {

            MySqlCommand command;
            String _key = String.Format(MysqlString.LOADROLEDATA_EUDEMON, info.playerid);
            command = new MySqlCommand(_key, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.HasRows) break;
                GameBase.Network.Internal.RoleData_Eudemon item = new GameBase.Network.Internal.RoleData_Eudemon();
                item.id = reader.GetUInt32("id");
                item.itemid = reader.GetUInt32("itemid");
                item.name = reader.GetString("name");
                item.name = Coding.Latin1ToGB2312(item.name);
                item.phyatk_grow_rate = reader.GetFloat("phyatk_grow_rate");
                item.phyatk_grow_rate_max = reader.GetFloat("phyatk_grow_rate_max");
                item.magicatk_grow_rate = reader.GetFloat("magicatk_grow_rate");
                item.magicatk_grow_rate_max = reader.GetFloat("magicatk_grow_rate_max");
                item.life_grow_rate = reader.GetFloat("life_grow_rate");
                item.defense_grow_rate = reader.GetFloat("defense_grow_rate");
                item.magicdef_grow_rate = reader.GetFloat("magicdef_grow_rate");
                item.init_life = reader.GetInt32("life");
                item.init_atk_min = reader.GetInt32("atk_min");
                item.init_atk_max = reader.GetInt32("atk_max");
                item.init_magicatk_min = reader.GetInt32("magicatk_min");
                item.init_magicatk_max = reader.GetInt32("magicatk_max");
                item.init_defense = reader.GetInt32("defense");
                item.init_magicdef = reader.GetInt32("magicdef");
                item.luck = reader.GetInt32("luck");
                item.intimacy = reader.GetInt32("intimacy");
                item.level = reader.GetInt16("level");
                item.card = reader.GetInt32("card");
                item.exp = reader.GetInt32("exp");
                item.quality = reader.GetInt32("quality");
                item.wuxing = reader.GetInt32("wuxing");
                item.recall_count = reader.GetInt32("recall_count");
                info.list_item.Add(item);
            }
            MysqlConn.Conn_Close();
            command.Dispose();
            LoadRoleData_Eudemon_MagicInfo(info);
        }

        //保存幻兽技能数据
         public static void SaveRoleData_Eudemon_MagicInfo(GameBase.Network.Internal.ROLEDATE_EUDEMON info)
        {
             MySqlCommand command;
             String sql;
             for(int i = 0;i < info.list_item.Count;i++)
             {
                 RoleData_Eudemon item = info.list_item[i];
                 for(int j = 0;j < item.mListMagicInfo.Count;j++)
                 {
                     GameBase.Network.Internal.MagicInfo itemex = item.mListMagicInfo[j];
                     if(itemex.id == 0)
                     {
                         sql = String.Format(MysqlString.ADD_EUDEMON_MAGIC, info.list_item[i].id, itemex.magicid, itemex.level, item.exp);
                     }
                     else if (itemex.id == -1)  //删除技能
                     {
                         sql = String.Format(MysqlString.DELETE_EUDEMON_MAGIC,  itemex.magicid);
                     }
                     else
                     {
                          sql = String.Format(MysqlString.UPDATE_EUDEMON_MAGIC, itemex.magicid, itemex.level, itemex.exp, itemex.ownerid, itemex.id);
                     }
                      command = new MySqlCommand(sql, MysqlConn.GetConn());
                      MysqlConn.Conn_Open();
                    command.ExecuteNonQuery();

                    MysqlConn.Conn_Close();
                    command.Dispose();
                 }
             }
        }
        //保存幻兽数据
         public static void SaveRoleData_Eudemon(GameBase.Network.Internal.ROLEDATE_EUDEMON info)
        {

            String sql = "";
             MySqlCommand command;
            for (int i = 0; i < info.list_item.Count; i++)
            {
                RoleData_Eudemon item = info.list_item[i];
                item.name = Coding.GB2312ToLatin1(item.name);
                if (item.id == 0) //不存在就插入
                {
                    sql = String.Format(MysqlString.SAVEROLEDATA_EUDEMON,item.itemid,info.playerid,item.name,item.phyatk_grow_rate,item.phyatk_grow_rate_max,item.magicatk_grow_rate,
                        item.magicatk_grow_rate_max, item.life_grow_rate, item.defense_grow_rate, item.magicdef_grow_rate, item.init_life, item.init_atk_min, item.init_atk_max, item.init_magicatk_min, item.init_magicatk_max, item.init_defense, item.init_magicdef, item.luck,
                        item.intimacy,item.level,item.card,item.exp,item.quality,item.wuxing,item.recall_count);
                }else
                {
                    sql = String.Format(MysqlString.UPDATEROLEDATA_EUDEMON,item.itemid,info.playerid,item.name,item.phyatk_grow_rate,item.phyatk_grow_rate_max,item.magicatk_grow_rate,
                        item.magicatk_grow_rate_max, item.life_grow_rate, item.defense_grow_rate, item.magicdef_grow_rate, item.init_life, item.init_atk_min, item.init_atk_max, item.init_magicatk_min, item.init_magicatk_max, item.init_defense, item.init_magicdef, item.luck,
                        item.intimacy,item.level,item.card,item.exp,item.quality,item.wuxing,item.recall_count,item.id);
                }

                command = new MySqlCommand(sql, MysqlConn.GetConn());
                MysqlConn.Conn_Open();
                command.ExecuteNonQuery();
                MysqlConn.Conn_Close();
                command.Dispose();

                //如果是插入- 取出主键- 要用于存储幻兽技能
                if (item.id == 0)
                {
                    String _key = "select max(id) from cq_eudemon";
                    command = new MySqlCommand(_key, MysqlConn.GetConn());
                    MysqlConn.Conn_Open();
                    MySqlDataReader reader = command.ExecuteReader();
                    uint ret = 0;
                    reader.Read();
                    if (reader.HasRows)
                    {
                        ret = Convert.ToUInt32(reader[0].ToString());
                    }
                    item.id = ret;
                    MysqlConn.Conn_Close();
                    command.Dispose();
                }
             }
            SaveRoleData_Eudemon_MagicInfo(info);
        }


         public static bool DeleteRoleData_Eudemon(int playerid, uint id)
         {
             MySqlCommand command;
             String sql = String.Format(MysqlString.DELETEROLEDATA_EUDEMON,id, playerid );
             command = new MySqlCommand(sql, MysqlConn.GetConn());
             MysqlConn.Conn_Open();
             command.ExecuteNonQuery();
             MysqlConn.Conn_Close();
             command.Dispose();
             return true;
         }

        public static void LoadRoleData_Friend(GameBase.Network.Internal.ROLEDATA_FRIEND info)
         {
            MySqlCommand command;
            String _key = String.Format(MysqlString.LOADROLEDATA_FRIEND, info.playerid);
            command = new MySqlCommand(_key, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.HasRows) break;
                GameBase.Network.Internal.RoleData_Friend friend = new GameBase.Network.Internal.RoleData_Friend();
                friend.id = reader.GetInt32("id");
                friend.friendid = reader.GetUInt32("friendid");
                friend.friendname = reader.GetString("friendname");
                friend.friendname = Coding.Latin1ToGB2312(friend.friendname);
                friend.friendtype = reader.GetByte("friendtype");
                info.list_item.Add(friend);
            }
            MysqlConn.Conn_Close();
            command.Dispose();
         }

        public static void SaveRoleData_Friend(GameBase.Network.Internal.ROLEDATA_FRIEND info)
        {
            MySqlCommand command;
             String sql= "";
             bool bSave = false;
            for (int i = 0; i < info.list_item.Count; i++)
            {
                GameBase.Network.Internal.RoleData_Friend friend = info.list_item[i];
               
               
                if (friend.id == -1)
                {

                    //先删除己方- 
                    sql = String.Format(MysqlString.DELETEROLEDATA_FRIEND, friend.friendid, info.playerid);
                    command = new MySqlCommand(sql, MysqlConn.GetConn());
                    MysqlConn.Conn_Open();
                    command.ExecuteNonQuery();
                    MysqlConn.Conn_Close();
                    command.Dispose();

                    //删除自己
                    sql = String.Format(MysqlString.DELETEROLEDATA_FRIEND, info.playerid, friend.friendid);
                    bSave = true;
                }
                else if(friend.id == 0)
                {
                    friend.friendname = Coding.GB2312ToLatin1(friend.friendname);
                    sql = String.Format(MysqlString.SAVEROLEDATA_FRIEND, info.playerid,friend.friendtype, friend.friendid,friend.friendname);
                    bSave = true;
                }
                if (!bSave) continue;
                command = new MySqlCommand(sql, MysqlConn.GetConn());
                MysqlConn.Conn_Open();
                command.ExecuteNonQuery();
                MysqlConn.Conn_Close();
                command.Dispose();
            }
          
        }

        //创建军团 成功返回军团id，失败返回-1
        public static int CreateLegion(LegionInfo info)
        {
            MySqlCommand command;
            info.name = Coding.GB2312ToLatin1(info.name);
            info.notice = Coding.GB2312ToLatin1(info.notice);
            String leader_name = Coding.GB2312ToLatin1(info.leader_name);
            String sql = String.Format(MysqlString.CREATE_LEGION, info.name, info.title, info.leader_id, leader_name, info.money, info.notice);
            String utf_sql = sql;
            command = new MySqlCommand(utf_sql, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            command.ExecuteNonQuery();
            MysqlConn.Conn_Close();
            command.Dispose();

            //取主键-- 不能用于多线程或者多个程序操作该数据库。。切记
            String _key = "select max(id) from cq_legion";
            command = new MySqlCommand(_key, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            MySqlDataReader reader = command.ExecuteReader();
            int ret = -1;
            reader.Read();
            if (reader.HasRows)
            {
                ret =Convert.ToInt32(reader[0].ToString());
            }
            MysqlConn.Conn_Close();
            command.Dispose();
            return ret;
        }

        //更新军团信息
        public static void UpdateLegion(LegionInfo info)
        {
            info.name = Coding.GB2312ToLatin1(info.name);
            info.notice = Coding.GB2312ToLatin1(info.notice);
            info.leader_name = Coding.GB2312ToLatin1(info.leader_name);
            String sql = string.Format(MysqlString.UPDATE_LEGION, info.name, info.title, info.leader_id, info.leader_name, info.money, info.notice, info.id);
            MySqlCommand command;
            command = new MySqlCommand(sql, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            command.ExecuteNonQuery();
            MysqlConn.Conn_Close();
            command.Dispose();
        }
        //更新军团成员信息
        public static uint UpdateLegionMembers(uint legion_id, LegionMember member)
        {
            member.members_name = Coding.GB2312ToLatin1(member.members_name);
            //插入
            MySqlCommand command;
            String sql = "";
            if (member.id == 0)
            {
               
                sql = string.Format(MysqlString.CREATE_LEGION_MEMBERS, legion_id, member.members_name, member.money, member.rank);
            }
            else
            {
                sql = string.Format(MysqlString.UPDATE_LEGION_MEMBERS, member.money, member.rank, legion_id, member.members_name);
            }
            command = new MySqlCommand(sql, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            command.ExecuteNonQuery();
            MysqlConn.Conn_Close();
            command.Dispose();
            if (member.id != 0)
            {
                return member.id;
            }

            //取主键-- 不能用于多线程或者多个程序操作该数据库。。切记
            uint ret = 0;
            String _key = "select max(id) from cq_legion_members";
            command = new MySqlCommand(_key, MysqlConn.GetConn());
            MysqlConn.Conn_Open();
            MySqlDataReader reader = command.ExecuteReader();
            reader.Read();
            if (reader.HasRows)
            {
                ret = Convert.ToUInt32(reader[0].ToString());
            }
            MysqlConn.Conn_Close();
            command.Dispose();
            return ret;

        }

    }


}
