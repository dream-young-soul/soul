using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
//内存操作ini类-
namespace GameBase.Config
{
    /// <summary>
    /// Ini节点
    /// </summary>
    public class IniSection
    {
        private Dictionary<string, string> FDictionary;//节点值
        private String FSectionName;//节点名称
        public IniSection(String SName)
        {
            FSectionName = SName;
            FDictionary = new Dictionary<string, string>();
        }

        public string SectionName
        {
            get { return FSectionName; }
        }

        public int Count
        {
            get { return FDictionary.Count; }
        }

        public void Clear()
        {
            FDictionary.Clear();
        }

        //增加键值对
        public void AddKeyValue(string key, string value)
        {
            if (FDictionary.ContainsKey(key))
                FDictionary[key] = value;
            else
                FDictionary.Add(key, value);
        }

        public void WriteValue(string key, string value)
        {
            AddKeyValue(key, value);
        }

        public void WriteValue(string key, bool value)
        {
            AddKeyValue(key, Convert.ToString(value));
        }

        public void WriteValue(string key, int value)
        {
            AddKeyValue(key, Convert.ToString(value));
        }

        public void WriteValue(string key, float value)
        {
            AddKeyValue(key, Convert.ToString(value));
        }

        public void WriteValue(string key, DateTime value)
        {
            AddKeyValue(key, Convert.ToString(value));
        }

        public string ReadValue(string key, string defaultv)
        {
            if (FDictionary.ContainsKey(key))
                return FDictionary[key];
            else
                return defaultv;
        }

        public bool ReadValue(string key, bool defaultv)
        {
            string rt = ReadValue(key, Convert.ToString(defaultv));
            return Convert.ToBoolean(rt);
        }

        public int ReadValue(string key, int defaultv)
        {
            string rt = ReadValue(key, Convert.ToString(defaultv));
            return Convert.ToInt32(rt);
        }

        public float ReadValue(string key, float defaultv)
        {
            string rt = ReadValue(key, Convert.ToString(defaultv));
            return Convert.ToSingle(rt);
        }

        public DateTime ReadValue(string key, DateTime defaultv)
        {
            string rt = ReadValue(key, Convert.ToString(defaultv));
            return Convert.ToDateTime(rt);
        }

        public void SaveToStream(Stream stream)
        {
            StreamWriter SW = new StreamWriter(stream);
            SaveToStream(SW);
            SW.Dispose();
        }

        public void SaveToStream(StreamWriter SW)
        {
            SW.WriteLine("[" + FSectionName + "]");
            foreach (KeyValuePair<string, string> item in FDictionary)
            {
                SW.WriteLine(item.Key + "=" + item.Value);
            }

        }
    }

    /// <summary>
    /// 内存Ini解析
    /// </summary>
    public class MemIniFile
    {
        private ArrayList List;//所有节点信息

        private bool SectionExists(string SectionName)
        {
            foreach (IniSection ISec in List)
            {
                if (ISec.SectionName.ToLower() == SectionName.ToLower())
                    return true;
            }
            return false;
        }

        public IniSection FindSection(string SectionName)
        {
            foreach (IniSection ISec in List)
            {
                if (ISec.SectionName.ToLower() == SectionName.ToLower())
                    return ISec;
            }
            return null;
        }

        public MemIniFile()
        {
            List = new ArrayList();
        }

        public void LoadFromStream(Stream stream)
        {
            StreamReader SR = new StreamReader(stream,System.Text.ASCIIEncoding.Default);
            
            List.Clear();
            string st = null;
            IniSection Section = null;//节点
            int equalSignPos;
            string key, value;
            while (true)
            {
                st = SR.ReadLine();
                if (st == null)
                    break;
                st = st.Trim();
                if (st == "")
                    continue;
                if (st != "" && st[0] == '[' && st[st.Length - 1] == ']')
                {
                    st = st.Remove(0, 1);
                    st = st.Remove(st.Length - 1, 1);
                    Section = FindSection(st);
                    if (Section == null)
                    {
                        Section = new IniSection(st);
                        List.Add(Section);
                    }
                }
                else
                {
                    if (Section == null)
                    {
                        Section = FindSection("UnDefSection");
                        if (Section == null)
                        {
                            Section = new IniSection("UnDefSection");
                            List.Add(Section);
                        }
                    }
                    //开始解析         
                    equalSignPos = st.IndexOf('=');
                    if (equalSignPos != 0)
                    {
                        key = st.Substring(0, equalSignPos);
                        value = st.Substring(equalSignPos + 1, st.Length - equalSignPos - 1);
                        Section.AddKeyValue(key, value);//增加到节点
                    }
                    else
                        Section.AddKeyValue(st, "");
                }
            }
            SR.Dispose();
        }

        public void SaveToStream(Stream stream)
        {
            StreamWriter SW = new StreamWriter(stream);
            foreach (IniSection ISec in List)
            {
                ISec.SaveToStream(SW);
            }
            SW.Dispose();
        }

        public string ReadValue(string SectionName, string key, string defaultv)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec != null)
            {
                return ISec.ReadValue(key, defaultv);
            }
            else return defaultv;
        }

        public bool ReadValue(string SectionName, string key, bool defaultv)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec != null)
            {
                return ISec.ReadValue(key, defaultv);
            }
            else return defaultv;
        }

        public int ReadValue(string SectionName, string key, int defaultv)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec != null)
            {
                return ISec.ReadValue(key, defaultv);
            }
            else return defaultv;
        }

        public float ReadValue(string SectionName, string key, float defaultv)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec != null)
            {
                return ISec.ReadValue(key, defaultv);
            }
            else return defaultv;
        }

        public DateTime ReadValue(string SectionName, string key, DateTime defaultv)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec != null)
            {
                return ISec.ReadValue(key, defaultv);
            }
            else return defaultv;
        }

        public IniSection WriteValue(string SectionName, string key, string value)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec == null)
            {
                ISec = new IniSection(SectionName);
                List.Add(ISec);
            }
            ISec.WriteValue(key, value);
            return ISec;
        }

        public IniSection WriteValue(string SectionName, string key, bool value)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec == null)
            {
                ISec = new IniSection(SectionName);
                List.Add(ISec);
            }
            ISec.WriteValue(key, value);
            return ISec;
        }

        public IniSection WriteValue(string SectionName, string key, int value)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec == null)
            {
                ISec = new IniSection(SectionName);
                List.Add(ISec);
            }
            ISec.WriteValue(key, value);
            return ISec;
        }

        public IniSection WriteValue(string SectionName, string key, float value)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec == null)
            {
                ISec = new IniSection(SectionName);
                List.Add(ISec);
            }
            ISec.WriteValue(key, value);
            return ISec;
        }

        public IniSection WriteValue(string SectionName, string key, DateTime value)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec == null)
            {
                ISec = new IniSection(SectionName);
                List.Add(ISec);
            }
            ISec.WriteValue(key, value);
            return ISec;
        }

        public bool LoadFromFile(string FileName)
        {
            if (!File.Exists(FileName)) return false;
                //2015.9.14 允许其他进程读该文件，设置了共享方式
            FileStream FS = new FileStream(FileName, FileMode.Open,FileAccess.Read,FileShare.Read);
            LoadFromStream(FS);
            FS.Close();
            FS.Dispose();
            return true;
        }

        public void SaveToFile(string FileName)
        {
            FileStream FS = new FileStream(System.IO.Path.GetFullPath(FileName), FileMode.Create);
            SaveToStream(FS);
            FS.Close();
            FS.Dispose();
        }
    }
}
