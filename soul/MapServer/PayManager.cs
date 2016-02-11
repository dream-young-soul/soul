using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Network.Internal;

//全局充值记录信息
namespace MapServer
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
        private Dictionary<String, PayRecInfo> mDicPayRecInfo;
        public static PayManager Instance()
        {
            if (mInstance == null)
            {
                mInstance = new PayManager();
            }
            return mInstance;
        }

        public PayManager()
        {
            mDicPayRecInfo = new Dictionary<String, PayRecInfo>();
        }
        //设置已充值标记
        public void SetPayTag(String account)
        {
            PackUpdatePayRecInfo pack = new PackUpdatePayRecInfo();
            pack.account = account;
            DBServer.Instance().GetDBClient().SendData(pack.GetBuffer());
          
        }

        //提取元宝
        public void GetMoney(PlayerObject play)
        {
            String sAccount = play.GetBaseAttr().sAccount;
            if (!mDicPayRecInfo.ContainsKey(sAccount))
            {
                play.MsgBox("没有可提取的魔石!");
                return;
            }
            int gamegold = mDicPayRecInfo[sAccount].money;
            play.ChangeMoney(GameStruct.MONEYTYPE.GAMEGOLD, gamegold);
            play.MsgBox("提取魔石[" + gamegold.ToString() + "]点!");
            SetPayTag(sAccount);
            mDicPayRecInfo.Remove(sAccount);
       
        }
        public void DB_Load(PackPayRecInfo info)
        {
            if (mDicPayRecInfo.ContainsKey(info.account))
            {
                mDicPayRecInfo[info.account].money = info.money;
                return;
            }
            PayRecInfo pack_info = new PayRecInfo();
            pack_info.account = info.account;
            pack_info.id = info.id;
            pack_info.order = info.order;
            pack_info.money = info.money;
            mDicPayRecInfo[info.account] = pack_info;
        }
        
    }
}
