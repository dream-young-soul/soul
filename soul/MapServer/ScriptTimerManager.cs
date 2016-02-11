using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;

//定时脚本管理器
//可以用做指定时间的活动等等事件
//2015.10.15
namespace MapServer
{
    //考虑到程序运行速度- 如果延迟了，可以进行校验..所以暂时抛弃秒的计算。
     class ScriptTimerInfo
    {
        public String name; //名称
        public int year;    //年
        public int month;   //月
        public int day;     //日
        public int hour;    //时
        public int minute;  //分
        public int second;  //秒- 暂时废弃
        public uint script_id;//脚本id
        public bool bTag;   //这个标记是否已经执行过该事件了.. true为已执行 false 为未执行
    }

     class PlayTimeOut
     {
         public int time_id;    //定时器的id
         public int id; //玩家数据库里的id- 不能是typeid 因为他是动态的
         public uint callback_scripte_id; //到期的回调脚本id 如果为0 需要脚本手动删除-不然内存泄漏哟-切记
         public GameBase.Core.TimeOut TimeOut;
         public bool IsOnline;
         public PlayTimeOut()
         {
             time_id = 0;
             callback_scripte_id = 0;
             id = 0;
             TimeOut = new GameBase.Core.TimeOut();
             IsOnline = true;
         }
    }
    public class ScriptTimerManager
    {
        private static ScriptTimerManager mInstance = null;
        private List<ScriptTimerInfo> mListInfo;
        private int mClearTagTick;
        private int mRunTick;

        //-----------玩家游戏内的脚本定时
        private List<PlayTimeOut> mListPlayTimeOut;
        private GameBase.Core.TimeOut mPlayTimeOut;
        public static ScriptTimerManager Instance()
        {
            if (mInstance == null)
            {
                mInstance = new ScriptTimerManager();
            }
            return mInstance;
        }

        public ScriptTimerManager()
        {
            mListInfo = new List<ScriptTimerInfo>();
            mClearTagTick = System.Environment.TickCount;
            mRunTick = System.Environment.TickCount;
            mListPlayTimeOut = new List<PlayTimeOut>();
            mPlayTimeOut = new GameBase.Core.TimeOut();
            mPlayTimeOut.SetInterval(1000);
            mPlayTimeOut.Update();
        }
        public bool Load()
        {
            //载入脚本定时器信息
            VerPacket pack = ConfigManager.Instance().GetVerPacket();
            String text = pack.LoadFileToText(TextDefine.CONFIG_FILE_SCRIPTTIMER);
            CsvFile csv = new CsvFile(text);
            String v;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                ScriptTimerInfo info = new ScriptTimerInfo();
                info.name = csv.GetFieldInfoToValue(i, "name");
                v = csv.GetFieldInfoToValue(i, "year");
                info.year = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "month");
                info.month = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "day");
                info.day = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "hour");
                info.hour = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "minute");
                info.minute = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "second");
                info.second = Convert.ToInt32(v);
                v = csv.GetFieldInfoToValue(i, "script_id");
                info.script_id = Convert.ToUInt32(v);
                info.bTag = false;

                 mListInfo.Add(info);
            }

            return true;
        }

        public void Run()
        {
            //每半分钟一个轮循
            if (System.Environment.TickCount - mRunTick > 1000 * 30)
            {
                mRunTick = System.Environment.TickCount;
                for (int i = 0; i < mListInfo.Count; i++)
                {
                    if (DateTime.Now.Year != 0 && DateTime.Now.Year != mListInfo[i].year) continue;
                    if (DateTime.Now.Month != 0 && DateTime.Now.Month != mListInfo[i].month) continue;
                    if (DateTime.Now.Day != 0 && DateTime.Now.Day != mListInfo[i].day) continue;
                    if (DateTime.Now.Hour != mListInfo[i].hour) continue;
                    if (DateTime.Now.Minute != mListInfo[i].minute) continue;
                    ScripteManager.Instance().ExecuteAction(mListInfo[i].script_id, null);
                    mListInfo[i].bTag = true;
                }
            }

            
            //三十分钟-
            //恢复已运行的事件标记
            if (System.Environment.TickCount - mClearTagTick > 1000 * 60 * 30)
            {
                mClearTagTick = System.Environment.TickCount;
                for (int i = 0; i < mListInfo.Count; i++)
                {
                    if(DateTime.Now.Hour != mListInfo[i].hour &&
                        mListInfo[i].bTag)
                    {
                        mListInfo[i].bTag = false;
                    }
                }
            }
            //每一秒一个轮询
            if (mPlayTimeOut.ToNextTime() && 
                mListPlayTimeOut.Count > 0)
            {
                int amount = mListPlayTimeOut.Count;
                while (amount > 0)
                {
                    amount--;
                    if (mListPlayTimeOut[amount].TimeOut.IsToNextTime() &&
                        mListPlayTimeOut[amount].IsOnline &&
                        mListPlayTimeOut[amount].callback_scripte_id > 0)
                    {
                        PlayerObject play = UserEngine.Instance().FindPlayerObjectToPlayerId(mListPlayTimeOut[amount].id);
                        if (play == null) continue;
                        ScripteManager.Instance().ExecuteAction(mListPlayTimeOut[amount].callback_scripte_id, play);
                        mListPlayTimeOut.RemoveAt(amount);
                    }
                }
                
            }
        }

        //添加玩家定时器回调事件 如果已经有该定时器 返回false, 否则返回true
        //time_id 定时器id
        //id 玩家数据库的id
        //time 时间[秒]
        //callback_scripte_id 定时器到了的回调id
        
        public bool AddPlayerTimeOut(int time_id,int id, int time, uint callback_scripte_id)
        {
            if (id <= 0)
            {
                Log.Instance().WriteLog("添加角色定时器失败，无效的id" + id.ToString() + "回调:" + callback_scripte_id.ToString());
            }
            for (int i = 0;i < mListPlayTimeOut.Count; i++)
            {
                if (mListPlayTimeOut[i].time_id == time_id)
                {
                    return false;
                }
            }
            PlayTimeOut timeout = new PlayTimeOut();
            timeout.time_id = time_id;
            timeout.id = id;
            timeout.callback_scripte_id = callback_scripte_id;
            timeout.TimeOut.SetInterval(time);
            timeout.TimeOut.Update();
            mListPlayTimeOut.Add(timeout);
            return true;
        }

        //获取定时器剩余时间 秒
        public int GetPlayerTimeOutS(int time_id, int id)
        {
            for (int i = 0; i < mListPlayTimeOut.Count; i++)
            {
                if (mListPlayTimeOut[i].time_id == time_id &&
                    mListPlayTimeOut[i].id == id)
                {

                    return mListPlayTimeOut[i].TimeOut.GetTimeOutMS() / 1000;
                }
            }
            return 0;
        }
        //检查定时器是否过期
        //time_id 定时器id
        //id 玩家id
        public bool CheckPlayerTimeOut(int time_id,int id)
        {
           
            for (int i = 0; i < mListPlayTimeOut.Count; i++)
            {
                if (mListPlayTimeOut[i].time_id == time_id &&
                    mListPlayTimeOut[i].id == id)
                {
                 
                    return mListPlayTimeOut[i].TimeOut.IsToNextTime();
                }
            }
           
            //Log.Instance().WriteLog("没有找到玩家定时器!" + id.ToString() + " 定时器id:" + time_id.ToString());
            return false;
        }
        //删除定时器
        public void DeletePlayerTimeOut(int time_id, int id)
        {
            for (int i = 0; i < mListPlayTimeOut.Count; i++)
            {
                if (mListPlayTimeOut[i].time_id == time_id &&
                    mListPlayTimeOut[i].id == id)
                {

                    mListPlayTimeOut.RemoveAt(i);
                    break;
                }
            }
        }

        //玩家离线- 把定时器做个标记- 只是为了减少遍历
        public void PlayerExitGame(int id)
        {
            for (int i = 0; i < mListPlayTimeOut.Count; i++)
            {
                if (mListPlayTimeOut[i].id == id)
                {
                    mListPlayTimeOut[i].IsOnline = false;
                }
            }
        }
        //玩家上线- 把定时器做个标记只是为了减少遍历
        public void PlayerEnterGame(int id)
        {
            for (int i = 0; i < mListPlayTimeOut.Count; i++)
            {
                if (mListPlayTimeOut[i].id == id)
                {
                    mListPlayTimeOut[i].IsOnline = true;
                }
            }
        }
    }
}
