using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using GameBase.Network;
using GameBase.Network.Internal;
using GameBase.Core;

//数据库的爵位数据
//2015.10.5
namespace DBServer
{
    //这个类读的数据并不存储到数据库，将会一直驻留在内存.只要mapserver组一连接就会发送
    public class GuanJue
    {
      
        private static GuanJue mInstnce = null;
        private List< GuanJueInfo> mListInfo;
        public static GuanJue GetInstance()
        {
            if (mInstnce == null)
            {
                mInstnce = new GuanJue();
            }
            return mInstnce;
        }

        public GuanJue()
        {
            mListInfo = new List<GuanJueInfo>();
        }
        //载入数据
        public void DB_Load()
        {
            String sql = string.Format("select id,name,guanjue from cq_user ORDER BY guanjue DESC");
            MySqlCommand command = new MySqlCommand(sql, MysqlConn.GetConn());
          
            MysqlConn.Conn_Open();
            MySqlDataReader reader = command.ExecuteReader();
            //最多五十个排名
            const int MAX_JUEWEI = 50;
            int index = 0;
            while(reader.Read())
            {
                if (!reader.HasRows)break;
                ulong guanjue = reader.GetUInt64("guanjue");
                if (guanjue == 0) break;
               
                GuanJueInfo info = new GuanJueInfo();
                info.guanjue = guanjue;
                info.id = reader.GetUInt32("id");
                info.name = reader.GetString("name");
                info.name = Coding.Latin1ToGB2312(info.name);
                mListInfo.Add(info);
                
                index++;
                if (index > MAX_JUEWEI) break;
            }

            MysqlConn.Conn_Close();
            command.Dispose();
        }
        //发送爵位数据到mapserver
        public void SendData(int mapid = 0)
        {
            GUANJUEINFO info = new GUANJUEINFO();
            for (int i = 0; i < mListInfo.Count; i++)
            {
                info.list_item.Add(mListInfo[i]);
            }
            SessionManager.Instance().SendMapServer(mapid, info.GetBuffer());
        }
        //更新爵位信息
        public void UpdateGuanJueInfo(GuanJueInfo info)
        {
            //先删-- 然后再排序
            for (int i = 0; i < mListInfo.Count; i++)
            {
                if (mListInfo[i].id == info.id)
                {
                    mListInfo.RemoveAt(i);
                    break;
                }
            }
          
            bool bInsert = false;
            for (int i = 0; i < mListInfo.Count; i++)
            {
                if (info.guanjue > mListInfo[i].guanjue)
                {
                    mListInfo.Insert(i, info);
                    bInsert = true;
                    break;
                }
            }
            if (!bInsert && mListInfo.Count < GameBase.Config.Define.MAX_JUEWEICOUNT)
            {
                mListInfo.Add(info);
            }
        }
        
    }
}
