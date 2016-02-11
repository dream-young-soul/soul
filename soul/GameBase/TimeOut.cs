using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//定时器类
//2015.10.26
namespace GameBase.Core
{
    public class TimeOut
    {
        private int mnTick;     //时间戳
        private int mnInterval; //间隔时间- 秒
        private Object mObject; //附加参数
        public TimeOut()
        {
            mnTick = 0;
            mnInterval = 0;
        }

        //设置间隔--
        //nSec 秒
        public void SetInterval(int nSec)
        {
            mnInterval = nSec * 1000;
            Update();
        }

        //检测定时器是否已过期
        public bool IsToNextTime()
        {
            if (System.Environment.TickCount - mnTick > mnInterval)
            {
                
                return true;
            }
            return false;
        }
        //fmsg 毫秒
        public void SetInterval(float fMS)
        {
            mnInterval = (int)fMS;
        }

        public void SetObject(Object obj) { mObject = obj; }

        public Object GetObject() { return mObject; }
        public void Update()
        {
            mnTick = System.Environment.TickCount;
        }

        public bool ToNextTime()
        {
            if (System.Environment.TickCount - mnTick > mnInterval)
            {
                Update();
                return true;
            }
            return false;
        }

        //获取定时器运行了多久
        public int GetDelayMS() { return System.Environment.TickCount - mnTick; }

        //获取定时器还有多久时间结束- 返回ms
        public int GetTimeOutMS() { return mnInterval - (System.Environment.TickCount - mnTick); }

    }
}
