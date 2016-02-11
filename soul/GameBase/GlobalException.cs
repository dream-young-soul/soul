using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameBase.Config;
//全局异常处理类
//2015.8.5
namespace GameBase.Core
{
    public class GlobalException
    {
        public static void InitException()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }
        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Log.Instance().WriteLog(ex.Message);
            Log.Instance().WriteLog(ex.StackTrace);

        }
    }


    public class BaseFunc
    {

        public static uint ExchangeShortBits(uint nData, int nBits)
        {
            nData &= 0xFFFF;
            uint ret = ((nData >> nBits) | (nData << (16 - nBits))) & 0xFFFF;
            return (uint)ret;
        }

        public static uint ExchangeLongBits(ulong nData, int nBits)
        {
            ulong ret = (nData >> nBits) | (nData << (32 - nBits));
            return (uint)ret;
        }

        //合并整数
        public static int MakeLong(int lo, int hi)
        {
            return (lo & 0xffff) | ((hi & 0xffff) << 16);
        }
        //取整数低位
        public static short LoWord(int v)
        {
            return (short)(v & 0xffff);
        }
        //取整数高位
        public static short HiWord(int v)
        {
            return (short)((v >> 16) & 0xffff);
        }


    }

  
}
