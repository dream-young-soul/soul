using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//字符串过滤- 名称过滤
namespace DBServer
{
    public class Filter
    {
        private List<String> list_name;
        private static Filter mInstance = null;
        public static Filter Instance()
        {
            if(mInstance == null)
            {
                mInstance = new Filter();
            }
            return mInstance;
            
        }

        public Filter()
        {
            list_name = new List<String>();
        }

        public bool LoadFilterNameFile(String sPath)
        {
            if (!File.Exists(sPath)) return false;
            FileStream f = new FileStream(sPath, FileMode.Open);
            StreamReader stream = new StreamReader(f);
            while (true)
            {
                String sLine = stream.ReadLine();
                if (sLine == null) break;
                list_name.Add(sLine);
            }
            f.Dispose();
            return true;
        }
        //检测名称是否有敏感词- 有返回true,没有返回false
        public bool CheckFileterName(String name)
        {
            for (int i = 0; i < list_name.Count; i++)
            {
                if (name.IndexOf(list_name[i]) >= 0)
                {
                    return true;
                }
            }
                return false;
        }
    }
}
