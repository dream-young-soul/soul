using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace GameBase.Config
{
    class FieldInfo
    {
        private String[] key;
        private String[] value;
        public FieldInfo(String[] field,String text)
        {
            String[] v = text.Split(',');
            if (v.Length != field.Length)
            {
                Log.Instance().WriteLog("load csv error! not field..code 2" + text);
            }
            key = new String[field.Length];
            value = new String[field.Length];
            for (int i = 0; i < field.Length; i++)
            {
                value[i] = v[i];
                key[i] = field[i];
            }
        }

        public String GetFieldValueToKey(String k)
        {
            for(int i = 0;i < key.Length;i++)
            {
                if(key[i] == k)return value[i];
            }
            return "";
        }

        public String GetFileValueToRow(int row)
        {
            if (row >= key.Length) return "";
            return value[row];
        }

    }
    public class CsvFile
    {
        private System.Collections.Generic.Dictionary<int, FieldInfo> mDic;
        private String[] mField;
        public CsvFile(String text)
        {
            mDic = new System.Collections.Generic.Dictionary<int, FieldInfo>();
            byte[] b = GameBase.Core.Coding.GetDefauleCoding().GetBytes(text);
            MemoryStream stream = new MemoryStream(b);
           
            mField = null;

            StreamReader read = new StreamReader(stream, System.Text.ASCIIEncoding.Default);
            
            int nLine = 0;
            while (true)
            {
                String line = read.ReadLine();
                if (line == null) break;
                if(line.Length <= 1)continue;
                //注释
                if (line[0] == '/' && line[1] == '/') continue;
                if (line[0] == '#') //字段名称
                {
                    mField = line.Split(',');
                    mField[0] = mField[0].Substring(1);
                    continue;
                }
                if (mField == null)
                {
                    Log.Instance().WriteLog("load csv error! not field.." + text);
                    return;
                }
                //内容
                FieldInfo info = new FieldInfo(mField,line);
                mDic[nLine] = info;
                nLine += 1;
            }
            stream.Dispose();
        }

        //取列数-
        public int GetCol()
        {
            return mField.Length;
        }
        //取行数
        public int GetLine()
        {

            return mDic.Count;
        }

        public String GetFieldInfoToRow(int line,int row)
        {
            if (mDic.ContainsKey(line))
            {
                FieldInfo info = mDic[line];
                return info.GetFileValueToRow(row);
            }
            return null;
        }

        public String GetFieldInfoToValue(int line, String row)
        {
            if (mDic.ContainsKey(line))
            {
                FieldInfo info = mDic[line];
                return info.GetFieldValueToKey(row);
            }
            return null;
        }
    }
}
