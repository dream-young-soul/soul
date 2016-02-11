using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//日志管理类
//2015.8.5
namespace GameBase.Config
{
    public class Log
    {
        private static Log m_Instance = null;
        private FileStream m_F = null;
        private StreamWriter m_Write = null;
        private bool m_bDebug = true;
        private bool m_bS = false;
        public static Log Instance()
        {
            if (m_Instance == null)
            {
                m_Instance = new Log();
            }
            return m_Instance;
        }

        //初始化日志目录路径
        public void Init(String sDir,bool debug = true)
        {
            m_bDebug = debug;
            if (!Directory.Exists(sDir))
            {
                Directory.CreateDirectory(sDir);
            }
            //创建日志文件
            String sTime = DateTime.Now.ToString();
            sTime = sTime.Replace("/", "-");
            sTime = sTime.Replace(":", "-");
            String sPath = sDir + "/" + sTime + ".txt";
            m_F = new FileStream(sPath, FileMode.Create);
            m_Write = new StreamWriter(m_F);
            m_bS = true;
            WriteLog("init log");
        }

        public void WriteLog(String sLog)
        
        {
            try
            {
                lock (m_Write)
                {
                    if (!m_bS) return;
                    String s = DateTime.Now.ToString() + "       " + sLog;
                    m_Write.WriteLine(s);
                    m_Write.Flush();
                    if (m_bDebug)
                    {
                        Console.WriteLine(s);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
          

          
        }
        public void Dispose()
        {
            lock (m_Write)
            {
                m_F.Close();
                m_F.Dispose();
                m_Write = null;
                m_Instance = null;
            }

        }
    }
}
