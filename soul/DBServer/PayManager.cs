using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using GameBase.Network;
using GameBase.Network.Internal;

//充值数据库管理信息
namespace DBServer
{
    class PayRecInfo
    {
        public int id; //主键ID
        public String order; //说明
        public String account; //帐号
        public int money;      //金额
       // byte state; //状态0.未提取 1.已提取
    }
   public class PayManager
    {
       private static PayManager mInstance = null;
       private int mLoadPayRecTick;
       private Dictionary<String, PayRecInfo> mDicPayInfo;
       public PayManager()
       {
           mLoadPayRecTick = System.Environment.TickCount;
           mDicPayInfo = new Dictionary<String, PayRecInfo>();
       }

       public static PayManager Instance()
       {
           if (mInstance == null)
           {
               mInstance = new PayManager();
           }
           return mInstance;
       }
       //发送爵位数据到mapserver
       public void SendData(int mapid = 0)
       {
           foreach (PayRecInfo info in mDicPayInfo.Values)
           {
               this.UpdateMapServer(mapid,info);
           }
         
       }
       //载入充值数据记录
       public void DB_Load()
       {
          String sql = string.Format("select * from cq_payrec where state = 0");
          MySqlCommand command = new MySqlCommand(sql, MysqlConn.GetConn());
          MysqlConn.Conn_Open();
          MySqlDataReader reader = command.ExecuteReader();
          while (reader.Read())
          {
              if (!reader.HasRows) break;
              int id = reader.GetInt32("id");
              PayRecInfo info = null;
              //检测到相同的id就不发了
              bool bfind = false;
              foreach( PayRecInfo baseinfo in mDicPayInfo.Values)
              {
                  if (baseinfo.id == id)
                  {
                      bfind = true;
                      break;
                  }
              }
              if (bfind) continue;
              info  = new PayRecInfo();
              if (!reader.HasRows) break;
              info.id = reader.GetInt32("id");
              info.money = reader.GetInt32("money");
              info.order = reader.GetString("order");
              info.account = reader.GetString("account");
              if (mDicPayInfo.ContainsKey(info.account))
              {
                  mDicPayInfo[info.account].money += info.money;
              }
              else
              {
                  mDicPayInfo[info.account] = info;
              }
                //发送到Mapserver子服务器更新-
              UpdateMapServer(0,mDicPayInfo[info.account]);
          }

          MysqlConn.Conn_Close();
          command.Dispose();
       }

       private void UpdateMapServer(int mapid,PayRecInfo info)
       {
           PackPayRecInfo pack_info = new PackPayRecInfo();
           pack_info.order = info.order;
           pack_info.account = info.account;
           pack_info.money = info.money;
           pack_info.id = info.id;
           SessionManager.Instance().SendMapServer(mapid, pack_info.GetBuffer());
       }
       //设置已提取魔石标记
       //sAccount 游戏帐号
       public void SetPayTag(String sAccount)
       {
           if (mDicPayInfo.ContainsKey(sAccount))
           {
               mDicPayInfo.Remove(sAccount);
           }
        
           String sql = String.Format("update cq_payrec set account='{0}',state=1", sAccount);
           MySqlCommand command = new MySqlCommand(sql, MysqlConn.GetConn());
           MysqlConn.Conn_Open();
           command.ExecuteNonQuery();
           MysqlConn.Conn_Close();
           command.Dispose();
       }

       public void Run()
       {
           //每隔三十秒读取充值数据库-
           if (System.Environment.TickCount - mLoadPayRecTick > 10000)
           {
               mLoadPayRecTick = System.Environment.TickCount;
               DB_Load();
           }
       }
    }
}
