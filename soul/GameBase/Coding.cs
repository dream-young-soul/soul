using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;

//字符编码操作类
///2015.8.26
namespace GameBase.Core
{
    public class Coding
    {
        private static Encoding gb2312 = null;
        private static Encoding utf8 = null;
        private static Encoding latin = null;
        public static Encoding GetDefauleCoding()
        {
            if (gb2312 == null)
            {
                gb2312 = Encoding.GetEncoding("gb2312");
                if (gb2312 == null)
                {
                    Log.Instance().WriteLog("获取系统默认字符编码失败,gb2312");
                }
            }
            return gb2312;
        }

        public static Encoding GetLatin1()
        {
            if (latin == null)
            {
                latin = Encoding.GetEncoding("latin1");
                if (latin == null)
                {
                    Log.Instance().WriteLog("获取latin1字符编码失败");
                }
            }
            return latin;
        }
        public static Encoding GetUtf8Coding()
        {
            if (utf8 == null)
            {
                utf8 = Encoding.GetEncoding(65001);
                if (utf8 == null)
                {
                    Log.Instance().WriteLog("获取utf8字符编码失败");
                }
            }
            return utf8;
        }

     
        public static String Utf8ToGB2312(byte[] text)
        {
            Init();
            return gb2312.GetString(text);
        }

        public static String Utf8ToGB2312(String text)
        {
            Init();
            byte[] sText = utf8.GetBytes(text);
            return Utf8ToGB2312(sText);
        }

        public static String GB2312ToUtf8(byte[] text)
        {
            Init();
          
            return utf8.GetString(text);
        }

        public static String GB2312ToUtf8(String text)
        {
            Init();
            byte[] sText = gb2312.GetBytes(text);
            return GB2312ToUtf8(sText);
        }


        public static String GB2312ToLatin1(byte[] text)
        {
            Init();
            return latin.GetString(text);
        }
        public static String GB2312ToLatin1(String text)
        {
            Init();
            byte[] sText = gb2312.GetBytes(text);
            return GB2312ToLatin1(sText);
        }
        public static String Latin1ToGB2312(String text)
        {
            Init();
            byte[] sText = latin.GetBytes(text);
            return Latin1ToGB2312(sText);
        }

        public static String Latin1ToGB2312(byte[] text)
        {
            Init();
            return gb2312.GetString(text);
        }

        public static String Latin1ToUft8(String text)
        {
            Init();
            byte[] sText = latin.GetBytes(text);
            return Latin1ToUft8(sText);
        }
        
        public static String Latin1ToUft8(byte[] text)
        {
            Init();
            return utf8.GetString(text);
        }

        public static String Uft8ToLatin1(String text)
        {
            Init();
            byte[] sText = utf8.GetBytes(text);
            return Uft8ToLatin1(sText);
        }

        public static String Uft8ToLatin1(byte[] text)
        {
            Init();
            return latin.GetString(text);
        }
        private static void Init()
        {
            GetDefauleCoding();
            GetUtf8Coding();
            GetLatin1();
        }
    }
}
