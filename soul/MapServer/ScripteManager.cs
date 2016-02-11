using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using GameBase.Config;
using GameStruct;
using GameBase.Network.Internal;

namespace MapServer
{
    //脚本引擎
    public class ScripteManager
    {
        private static ScripteManager m_Instance = null;
        private Dictionary<uint, GameStruct.ActionInfo> mDicScripte;
        private byte mnSelectIndex;     //选项索引

        private bool mbEndTag;      //是否发送npc对话框结尾标记
        private String mszStr; //创建军团啊 仓库密码啊-- 客户端发过来的字符串
        public static ScripteManager Instance()
        {
            if (m_Instance == null)
            {
                m_Instance = new ScripteManager();
            }
            return m_Instance;
        }

        public ScripteManager()
        {
            mszStr = "";
            mDicScripte = new Dictionary<uint, GameStruct.ActionInfo>();


        }


        public void reset()
        {
            mnSelectIndex = 1;

            mbEndTag = false;
        }

        public void ClearAllScripte()
        {
            mDicScripte.Clear();
        }
        public uint LoadScripteFile(String path, bool reload = false)
        {
            String line = "";
            uint nRetId = 0;
            try
            {
                if (path == "null") return 0;
                if (!File.Exists(path))
                {
                    Log.Instance().WriteLog("载入脚本文件失败:" + path);
                    return 0;
                }

                FileStream f = new FileStream(path, FileMode.Open);
                StreamReader read = new StreamReader(f, System.Text.ASCIIEncoding.Default);
                
                while (true)
                {
                     line = read.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    if (line.Length <= 0) continue;
                    if (line[0] == '/' && line[1] == '/') continue;//注释
                    String[] param = line.Split('\t');
                    if (param.Length != 6)
                    {
                        //添加特殊脚本命令- call 可特殊指向其他脚本id 2015.9.21
                        if (param.Length == 2 && param[0] == "call")
                        {
                            nRetId = Convert.ToUInt32(param[1]);
                            f.Dispose();
                            return nRetId;
                        }
                        Log.Instance().WriteLog("载入脚本文件错误:" + path + " 数据:" + line);

                        f.Dispose();
                        return 1;
                    }

                    GameStruct.ActionInfo info = new GameStruct.ActionInfo();
                    info.id = Convert.ToUInt32(param[0]);
                    info.id_next = Convert.ToUInt32(param[1]);
                    info.id_nextfail = Convert.ToUInt32(param[2]);
                    info.type = Convert.ToUInt32(param[3]);
                    info.data = Convert.ToUInt32(param[4]);
                    info.param = param[5];
                    if (!reload)
                    {
                        if (mDicScripte.ContainsKey(info.id))
                        {
                            Log.Instance().WriteLog("重复id,读出文件:" + path);
                            Log.Instance().WriteLog(info.id.ToString() + " " + info.id_next.ToString() + " " + info.id_nextfail.ToString() + " " + info.type.ToString() +
                                " " + info.data.ToString() + " " + info.param.ToString());
                            Log.Instance().WriteLog(mDicScripte[info.id].id.ToString() + " " + mDicScripte[info.id].id_next.ToString() + " " + mDicScripte[info.id].id_nextfail.ToString() + " " + mDicScripte[info.id].type.ToString() +
                                " " + mDicScripte[info.id].data.ToString() + " " + mDicScripte[info.id].param.ToString());
                        }
                    }

                    //返回起始id
                    if (nRetId == 0)
                    {
                        nRetId = info.id;
                    }
                    mDicScripte[info.id] = info;
                }


                f.Dispose();
              
            }
            catch (System.Exception ex)
            {
                Log.Instance().WriteLog(path);
                Log.Instance().WriteLog(line);
                
            }
            return nRetId;
        }

        public void ExecuteActionForNpc(uint npcid, PlayerObject play)
        {
            GameStruct.NPCInfo info = ConfigManager.Instance().GetNpcInfoToID(npcid);
            play.SetCurrentNpcInfo(info);
            
            if (info == null)
            {
                Log.Instance().WriteLog("执行脚本失败.npcid脚本未找到:" + npcid.ToString());
                return;
            }

            ExecuteAction(info.ScriptID, play);
        }
        //执行脚本
        //id 脚本id
        //play 玩家对象
        public void ExecuteAction(uint id, PlayerObject play)
        {
            reset();
            if (play != null)
            {
                play.ClearScriptMenuLink();
            }
            uint nextid = id;
            while (true)
            {
                if (!mDicScripte.ContainsKey(nextid)) break;
                GameStruct.ActionInfo info = mDicScripte[nextid];
                 bool ret  = false;
                //做一个异常处理，防止脚本写错服务端崩溃
                try
                {
                    ret = SWITCH(info, play);
                }
                catch (System.Exception ex)
                {
                    Log.Instance().WriteLog(ex.Message);
                    Log.Instance().WriteLog(ex.StackTrace);
                    Log.Instance().WriteLog("执行脚本失败,脚本id:" + info.id.ToString() + "玩家昵称:" + play.GetName());
                    ret = false;
                }
              
                if (ret) nextid = info.id_next;
                else nextid = info.id_nextfail;
                //npc对话已经完成,发送结尾标记-
                if (info.id_next == 0 && mbEndTag && play != null)
                {
                    NetMsg.MsgNpcReply reple = new NetMsg.MsgNpcReply();
                    reple.Create(null, play.GetGamePackKeyEx());
                    play.SendData(reple.Flush());
                    break;
                }
                //退出
                if (nextid == 0) break;
            }
        }

        //执行对应的选项脚本
        //index npc对话框点击选项的索引
        //play 玩家对象
        //szStr 客户端上传的字符串内容
        public void ExecuteOptionId(byte index, PlayerObject play, String szStr = "")
        {
            mszStr = szStr;
            if (play.GetMenuLink().ContainsKey(index))
            {
                ExecuteAction(play.GetMenuLink()[index], play);
            }
        }
        //执行回调脚本
        //scriptid 脚本id
        //play 玩家对象
        //szStr 客户端上传的字符串
        public void ExecuteOptionId(uint scriptid, PlayerObject play, String szStr = "")
        {
            mszStr = szStr;
            ExecuteAction(scriptid, play);
        }

        private bool SWITCH(GameStruct.ActionInfo info, PlayerObject play)
        {
            bool ret = true;

            switch (info.type)
            {
                case ActionID.ACTION_MENU_TEXT:
                    {
                        Action_MenuText(info, play);
                        mbEndTag = true; //要发送npc结尾标记,显示npc对话框
                        break;
                    }
                case ActionID.ACTION_MENU_LINK:
                    {
                        Action_MenuLink(info, play);
                        mnSelectIndex++;
                        break;
                    }
                case ActionID.ACTION_MENU_EDIT:
                    {
                        Action_MenuEdit(info, play);
                        break;
                    }
                case ActionID.ACTION_MENU_PIC:
                    {
                        Action_MenuImage(info, play);
                        break;
                    }
                case ActionID.ACTION_MESSAGEBOX:
                    {
                        Action_MessageBox(info, play);
                        break;
                    }
                case ActionID.ACTION_MAP_ENTERMAP:
                    {
                        Action_Map_EnterMap(info, play);
                        break;
                    }
                case ActionID.ACTION_MAP_RANDOM:
                    {
                        Action_Map_Random(info, play);
                        break;
                    }
                case ActionID.ACTION_MAP_RECALL:
                    {
                        Action_Map_ReCall(info, play);
                        break;
                    }
                case ActionID.ACTION_MAP_CHANGE:
                    {
                        Action_Map_Change(info, play);
                        break;
                    }
                case ActionID.ACTION_ITEM_ADD:
                    {
                        Action_Item_Add(info, play);
                        break;
                    }
                case ActionID.ACTION_ITEM_DELETE:
                    {
                        Action_Item_Delete(info, play);
                        break;
                    }
                case ActionID.ACTION_ITEM_DELETE_NAME:
                    {
                        ret = Action_Item_Delete_Name(info, play);
                        break;
                    }
                case ActionID.ACTION_ITEM_DELETE_ITEMID:
                    {
                        ret = Action_Item_Delete_ItemID(info, play);
                        break;
                    }
                case ActionID.ACTION_ITEM_LEVEL:
                    {
                        ret = Action_Item_Level(info, play);
                        break;
                    }
                case ActionID.ACTION_EQUIP_OPERATION:
                    {
                        ret = Action_Equip_Operation(info, play);
                        break;
                    }
                case ActionID.ACTION_CHECK_BAG_SIZE:
                    {
                        ret = Action_Check_Bag_Size(info, play);
                        break;
                    }
                case ActionID.ACTION_CHECK_PROFESSION:
                    {
                        ret = Action_CheckProfession(info, play);
                        break;
                    }
                case ActionID.ACTION_CHECK_LEVEL:
                    {
                        ret = Action_CheckLevel(info, play);
                        break;
                    }
                case ActionID.ACTION_SET_ROLE_PRO:
                    {
                        Action_Set_Role_Pro(info, play);
                        break;
                    }
                case ActionID.ACTION_GET_ROLE_PRO:
                    {
                       ret = Action_Get_Role_Pro(info, play);
                        break;
                    }
                case ActionID.ACTION_ADDMAGIC:
                    {
                        Action_AddMagic(info, play);
                        break;
                    }
                case ActionID.ACTION_OPENDIALOG:
                    {
                        Action_OpenDialog(info, play);
                        break;
                    }

                case ActionID.ACTION_LEARNMAGIC:
                    {
                        Action_LearnMagic(info, play);
                        break;
                    }
                case ActionID.ACTION_CHECKMAGIC:
                    {
                        ret = Action_CheckMagic(info, play);
                        break;
                    }
                case ActionID.ACTION_LEFTNOTICE:
                    {
                        String str = Sprintf_string(info.param, play);
                        play.LeftNotice(str);
                        break;
                    }
                case ActionID.ACTION_CHATNOTICE:
                    {
                        String str = Sprintf_string(info.param, play);
                        play.ChatNotice(str);
                        break;
                    }
                case ActionID.ACTION_SCREENNOTICE:
                    {
                        String str = Sprintf_string(info.param, play);
                         UserEngine.Instance().SceneNotice(str);
                        break;
                    }
                case ActionID.ACTION_MSGBOX:
                    {
                        String str = Sprintf_string(info.param, play);
                        play.MsgBox(str);
                        break;
                    }
                case ActionID.ACTION_PITCH:
                    {
                      
                        play.Ptich();
                        break;
                    }
                case ActionID.ACTION_GETPAYGAMEGOLD:
                    {
                        PayManager.Instance().GetMoney(play);
                        break;
                    }
                case ActionID.ACTION_FUCK_NIAN:
                    {
                        Action_Fuck_Nian(info, play);
                        break;
                    }
              
                case ActionID.ACTION_EUDEMON_CREATE:
                    {
                        Action_Eudemon_Create(info, play);
                        break;
                    }
                case ActionID.ACTION_EUDEMON_CREATEEX:
                    {
                        Action_Eudemon_CreateEx(info, play);
                        break;
                    }
                case ActionID.ACTION_RECALL_EUDEMON:
                    {
                        Action_Recall_Eudemon(info, play);
                        break;
                    }
                case ActionID.ACTION_LEGION_CREATE:
                    {
                        ret = Action_Legion_Create(info, play);
                        break;
                    }
                case ActionID.ACTION_LEGION_CHANGE_TITLE:
                    {
                        Action_Legion_ChangeTitle(info, play);
                        break;
                    }
                case ActionID.ACTION_TIMEOUT_CREATE:
                    {
                        ret = Action_TimeOut_Create(info, play);
                        break;
                    }
                case ActionID.ACTION_TIMEOUT_CHECK:
                    {
                        ret = Action_TimeOut_Check(info, play);
                        break;
                    }
                case ActionID.ACTION_TIMEOUT_DELETE:
                    {
                        Action_TimeOut_Delete(info, play);
                        break;
                    }
                case ActionID.ACTION_MAGIC_OPERATION:
                    {
                        ret = Action_Magic_Operation(info, play);
                        break;
                    }
                case ActionID.ACTION_RANDOM_INIT: 
                    {
                        Action_Random_Init(info, play);
                        break;
                    }
                case ActionID.ACTION_RANDOM_COMPARE:
                    {
                        ret = Action_Random_Compare(info, play);
                        break;
                    }
                case ActionID.ACTION_GET_EUDEMON_PRO:
                    {
                        ret = Action_Get_Eudemon_Pro(info, play);
                        break;
                    }
                case ActionID.ACTION_SET_EUDEMON_PRO:
                    {
                       Action_Set_Eudemon_Pro(info, play);
                        break;
                    }
                case ActionID.ACTION_FUBEN_CREATE:
                    {
                        ret = Action_Fuben_Create(info, play);
                        break;
                    }

            }
            return ret;
        }


        private void Action_MenuText(ActionInfo info, PlayerObject play)
        {
            NetMsg.MsgNpcReply msg = new NetMsg.MsgNpcReply();
            msg.Create(null, play.GetGamePackKeyEx());
            msg.interactType = 257;
            msg.optionid = 255;
            msg.text = Sprintf_string(info.param, play);
            play.SendData(msg.GetBuffer());
        }


        private void Action_MenuLink(ActionInfo info, PlayerObject play)
        {
            NetMsg.MsgNpcReply msg = new NetMsg.MsgNpcReply();
            msg.Create(null, play.GetGamePackKeyEx());
            msg.interactType = 258;
            msg.param = 111;
            msg.param2 = 112;
            msg.param3[1] = 113;
            msg.param3[2] = 114;
            msg.param3[0] = 115;
            if (info.id_next == 0) msg.optionid = 255;
            else msg.optionid = mnSelectIndex;/**选项索引**/


            String[] option = info.param.Split(' ');
            play.GetMenuLink()[mnSelectIndex] = Convert.ToUInt32(option[1]);
            msg.text = option[0];
            play.SendData(msg.GetBuffer());

        }

        private void Action_MenuEdit(ActionInfo info, PlayerObject play)
        {
            NetMsg.MsgNpcReply msg = new NetMsg.MsgNpcReply();
            msg.Create(null, play.GetGamePackKeyEx());
            msg.interactType = 259;

            String[] option = info.param.Split(' ');
            if (option.Length != 3)
            {
                Log.Instance().WriteLog("Action_MenuEdit参数数量不对." + info.param);
            }
            ushort nAcceptLen = Convert.ToUInt16(option[0]);
            uint idTask = Convert.ToUInt32(option[1]);
            play.SetTaskID(idTask);
            String sText = option[2];
            msg.param2 = nAcceptLen;
            msg.text = sText;
            play.SendData(msg.GetBuffer());
            //byte[] data1 = { 29, 0, 240, 7, 0, 0, 0, 0, 15, 0, 0, 3, 1, 12, 190, 252, 205, 197, 207, 235, 189, 208, 161, 173, 161, 173, 0, 0, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref data1, data1.Length);
            //play.SendData(data1);



        }
        private void Action_MessageBox(ActionInfo info, PlayerObject play)
        {
            String str = Sprintf_string(info.param, play);
            play.MsgBox(str);
        }
        private void Action_MenuImage(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            NetMsg.MsgNpcReply msg = new NetMsg.MsgNpcReply();
            msg.Create(null, play.GetGamePackKeyEx());
            ushort id = Convert.ToUInt16(option[2]);
            play.SendData(msg.NpcImage(id));
        }


        private void Action_Map_EnterMap(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            if (option.Length < 2)
            {
                Log.Instance().WriteLog("脚本参数错误,id:" + info.id.ToString() + " param:" + info.param);
                return;
            }
            uint mapid = Convert.ToUInt32(option[0]);
            short x = Convert.ToInt16(option[1]);
            short y = Convert.ToInt16(option[2]);
            byte dir = Convert.ToByte(option[3]);
            play.FlyMap(mapid, x, y, dir);
        }

        private void Action_Item_Add(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            if (option.Length < 2)
            {
                Log.Instance().WriteLog("脚本参数错误,id:" + info.id.ToString() + " param:" + info.param);
                return;
            }
            uint itemid = Convert.ToUInt32(option[0]);
            byte postion = Convert.ToByte(option[1]);
            byte stronglv = 0;
            byte amount = 1;
            if (option.Length >= 3) amount = Convert.ToByte(option[2]);
            if (option.Length >= 4) stronglv = Convert.ToByte(option[3]);
            byte gem1 = 0;
            byte gem2 = 0;
            byte gem3 = 0;
            if (option.Length >= 5) gem1 = Convert.ToByte(option[4]);
            if (option.Length >= 6) gem2 = Convert.ToByte(option[5]);
            if (option.Length >= 7) gem3 = Convert.ToByte(option[6]);
            byte war_ghost_exp = 0;
            if (option.Length >= 8) war_ghost_exp = Convert.ToByte(option[7]);
            byte di_attack = 0;
            if (option.Length >= 9) di_attack = Convert.ToByte(option[8]);
            byte shui_attack = 0;
            byte huo_attack = 0;
            byte feng_attack = 0;
            if (option.Length >= 10) shui_attack = Convert.ToByte(option[9]);
            if (option.Length >= 11) huo_attack = Convert.ToByte(option[10]);
            if (option.Length >= 12) feng_attack = Convert.ToByte(option[11]);

            for (int i = 0; i < amount; i++)
            {
                play.GetItemSystem().AwardItem(itemid, postion, amount, stronglv, gem1, gem2, gem3, war_ghost_exp, di_attack, shui_attack, huo_attack, feng_attack);
            }

            // play.GetItemSystem().AwardItem(itemid, postion, 1, stronglv, gem1, gem2, gem3, war_ghost_exp, di_attack, shui_attack, huo_attack, feng_attack);

        }

        private bool Action_CheckProfession(ActionInfo info, PlayerObject play)
        {
            byte nProfession = Convert.ToByte(info.param);
            if (play.GetBaseAttr().profession == nProfession)
            {
                return true;
            }
            return false;
        }

        private bool Action_CheckLevel(ActionInfo info, PlayerObject play)
        {
            String[] str = info.param.Split(' ');
            if (str.Length == 2)
            {
                byte level = Convert.ToByte(str[1]);

                switch (str[0])
                {
                    case "<":
                        {
                            return play.GetBaseAttr().level < level;
                        }
                    case "=":
                        {
                            return play.GetBaseAttr().level == level;
                        }
                    case ">":
                        {
                            return play.GetBaseAttr().level > level;
                        }
                }
            }
            return false;

        }
        //检测玩家属性
        private bool Action_Get_Role_Pro(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            if (option.Length != 3)
            {
                Log.Instance().WriteLog("参数错误:Action_Get_Role_Pro" + info.param);
                return false;
            }
            String op = option[0];
            int nParam = Convert.ToInt32(option[2]);
            switch (op)
            {
                case "gold":
                    {
                        switch (option[1])
                        {
                            case ">":
                                { return play.GetBaseAttr().gold > nParam; }
                            case "<":
                                { return play.GetBaseAttr().gold < nParam; }
                            case "=":
                                { return play.GetBaseAttr().gold == nParam; }
                            case ">=":
                                { return play.GetBaseAttr().gold >= nParam; }
                            case "<=":
                                { return play.GetBaseAttr().gold <= nParam; }
                        }
                        break;
                    }
                case "gamegold":
                    {
                        switch (option[1])
                        {
                            case ">":
                                { return play.GetBaseAttr().gamegold > nParam; }
                            case "<":
                                { return play.GetBaseAttr().gamegold < nParam; }
                            case "=":
                                { return play.GetBaseAttr().gamegold == nParam; }
                            case ">=":
                                { return play.GetBaseAttr().gamegold >= nParam; }
                            case "<=":
                                { return play.GetBaseAttr().gamegold <= nParam; }
                        }
                        break;
                    }
                case "level":
                    {
                        switch (option[1])
                        {
                            case ">":
                                { return play.GetBaseAttr().level > nParam; }
                            case "<":
                                { return play.GetBaseAttr().level < nParam; }
                            case "=":
                                { return play.GetBaseAttr().level == nParam; }
                            case ">=":
                                { return play.GetBaseAttr().level >= nParam; }
                            case "<=":
                                { return play.GetBaseAttr().level <= nParam; }
                        }
                        break;
                    }
                case "godlevel":
                    {
                        switch (option[1])
                        {
                            case ">":
                                { return play.GetBaseAttr().godlevel > nParam; }
                            case "<":
                                { return play.GetBaseAttr().godlevel < nParam; }
                            case "=":
                                { return play.GetBaseAttr().godlevel == nParam; }
                            case ">=":
                                { return play.GetBaseAttr().godlevel >= nParam; }
                            case "<=":
                                { return play.GetBaseAttr().godlevel <= nParam; }
                        }
                        break;
                    }
                case "pk":
                    {
                        switch (option[1])
                        {
                            case ">":
                                { return play.GetBaseAttr().pk > nParam; }
                            case "<":
                                {
                                    return play.GetBaseAttr().pk < nParam;
                                }
                            case "=":
                                {
                                    return play.GetBaseAttr().pk > nParam;
                                }
                            case ">=":
                                {
                                    return play.GetBaseAttr().pk >= nParam;
                                }
                            case "<=":
                                {
                                    return play.GetBaseAttr().pk <= nParam;
                                }
                               
                        }
                        break;
                    }
                case "maxeudemon":
                    {
                        switch (option[1])
                        {
                            case ">":
                                { return play.GetBaseAttr().maxeudemon > nParam; }
                            case "<":
                                {
                                    return play.GetBaseAttr().maxeudemon < nParam;
                                }
                            case "=":
                                {
                                    return play.GetBaseAttr().maxeudemon == nParam;
                                }
                            case ">=":
                                {
                                    return play.GetBaseAttr().maxeudemon >= nParam;
                                }
                            case "<=":
                                {
                                    return play.GetBaseAttr().maxeudemon <= nParam;
                                }

                        }
                        break;
                    }
                //case "battle_eudemon_count": //出战幻兽
                //    {
                //        //switch (option[1])
                //        //{
                           
                //        //}
                //        break;
                //    }

            }

            return false;
        }
        //设置玩家属性
        private void Action_Set_Role_Pro(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            //if (option.Length != 2)
            //{
            //    Log.Instance().WriteLog("参数错误:Action_Set_Role_Pro"+info.param);
            //    return;
            //}
            String op = option[0];
            switch (op)
            {
                case "level":   //等级
                    {
                        byte level = Convert.ToByte(option[2]);
                        switch (option[1])
                        {
                            case "+":
                                {
                                    play.ChangeAttribute(UserAttribute.LEVEL, level);
                                    break;
                                }
                            case "-":
                                {
                                    play.ChangeAttribute(UserAttribute.LEVEL, -level);
                                    break;
                                }
                            case "=":
                                {
                                    
                                    play.GetBaseAttr().level = level;
                                    play.ChangeAttribute(UserAttribute.LEVEL, 0);
                                    break;
                                }
                         }
                       
                      
                        break;
                    }
                case "godlevel"://神等级
                    {
                        byte godlevel = Convert.ToByte(option[2]);
                        switch (option[1])
                        {
                            case "+":
                                {
                                    play.GetBaseAttr().godlevel += godlevel;
                                    break;
                                }
                            case "-":
                                {
                                    play.GetBaseAttr().godlevel -= godlevel;
                                    break;
                                }
                            case "=":
                                {
                                    play.GetBaseAttr().godlevel = godlevel;

                                    break;
                                }
                        }
                        break;
                    }
                case "hair": //发型
                    {
                        play.ChangeAttribute(UserAttribute.HAIR, Convert.ToInt32(option[1]));
                        break;
                    }
                case "gold": //金币
                    {
                        int nGold = Convert.ToInt32(option[2]);
                        switch (option[1])
                        {
                            case "+":
                                {
                                    play.ChangeAttribute(UserAttribute.GOLD, nGold);
                                    break;
                                }
                            case "-":
                                {
                                    play.ChangeAttribute(UserAttribute.GOLD, -nGold);
                                    break;
                                }
                            case "=":
                                {
                                    play.GetBaseAttr().gold = nGold;
                                    play.ChangeAttribute(UserAttribute.GOLD, 0);
                                    break;
                                }
                        }
                        break;
                    }
                case "gamegold": //魔石
                    {
                        int nGameGold = Convert.ToInt32(option[2]);
                        switch (option[1])
                        {
                            case "+":
                                {
                                    play.ChangeAttribute(UserAttribute.GAMEGOLD, nGameGold);
                                    break;
                                }
                            case "-":
                                {
                                    play.ChangeAttribute(UserAttribute.GAMEGOLD, -nGameGold);
                                    break;
                                }
                            case "=":
                                {
                                    play.GetBaseAttr().gamegold = nGameGold;
                                    play.ChangeAttribute(UserAttribute.GAMEGOLD, 0);
                                    break;
                                }
                        }
                        break;
                    }
                case "job": //职业
                    {
                        byte bJob = Convert.ToByte(option[2]);
                        break;
                    }
                case "pk":
                    {
                        switch (option[1])
                        {
                            case "=":
                                { play.GetBaseAttr().pk = Convert.ToInt16(option[2]); break; }
                            case "+":
                                {  play.GetBaseAttr().pk += Convert.ToInt16(option[2]); break; }
                            case "-":
                                { play.GetBaseAttr().pk -= Convert.ToInt16(option[2]); break; }
                        }
                        play.ChangeAttribute(UserAttribute.PK, 0, false);
                        break;
                    }
                case "maxeudemon":
                    {
                        switch (option[1])
                        {
                            case "=":
                                { play.GetBaseAttr().maxeudemon = Convert.ToByte(option[2]); break; }
                            case "+":
                                { play.GetBaseAttr().maxeudemon += Convert.ToByte(option[2]); break; }
                            case "-":
                                { play.GetBaseAttr().maxeudemon -= Convert.ToByte(option[2]); break; }
                        }
                        play.ChangeAttribute(UserAttribute.MAXEUDEMON, 0,false);
                        break;
                    }
            }

        }

        //增加玩家技能
        private void Action_AddMagic(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            uint magicid = Convert.ToUInt32(option[0]);
            byte level = 0;
            if (option.Length >= 2) level = Convert.ToByte(option[1]);
            uint exp = 0;
            if (option.Length >= 3) exp = Convert.ToUInt32(option[2]);
            play.GetMagicSystem().AddMagicInfo(magicid, level, exp);
        }
        private void Action_OpenDialog(ActionInfo info, PlayerObject play)
        {
            int dwData = Convert.ToInt32(info.data);
            play.OpenDialog(dwData);

        }



        private void Action_LearnMagic(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            uint magicid = Convert.ToUInt32(option[0]);
            byte level = 0;
            uint exp = 0;
            if (option.Length >= 2)
            {
                level = Convert.ToByte(option[1]);
            }
            if (option.Length >= 3)
            {
                exp = Convert.ToUInt32(option[2]);
            }
            play.GetMagicSystem().AddMagicInfo(magicid, level, exp);
        }
        private bool Action_Map_Random(ActionInfo info, PlayerObject play)
        {
            play.ScroolRandom();//随机传送
            return true;
        }

        private bool Action_Map_ReCall(ActionInfo info, PlayerObject play)
        {
            play.ReCallMap();
            return true;
        }

        private void Action_Map_Change(ActionInfo info, PlayerObject play)
        {
            String[] str = info.param.Split(' ');
            if (str.Length == 3)
            {
                uint mapid = Convert.ToUInt32(str[0]);
                short x = Convert.ToInt16(str[1]);
                short y = Convert.ToInt16(str[2]);
                if (play.GetGameMap().GetMapInfo().id == mapid)
                {
                    play.ScroolRandom(x, y);
                }
                else
                {
                    play.ChangeMap(mapid, x, y);
                }

            }
            else
            {
                Log.Instance().WriteLog("Action_Map_Change 参数错误:" + info.param);
            }

        }

        private bool Action_Item_Delete_ItemID(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            uint nItemID = Convert.ToUInt32(option[0]);
            int nAmount = Convert.ToInt32(option[1]);
            if (nAmount <= 0)
            {
                Log.Instance().WriteLog("Action_Item_Delete_Name 参数错误");
                return false;
            }
            int count = 0;
            GameStruct.RoleItemInfo item = play.GetItemSystem().FindItem(nItemID, ref count);
            if (count < nAmount) return false;
            play.GetItemSystem().DeleteItemByItemID(nItemID, nAmount);
            return true;
        }
        private bool Action_Item_Delete_Name(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            String sName = option[0];
            int nAmount = Convert.ToInt32(option[1]);
            if (nAmount <= 0)
            {
                Log.Instance().WriteLog("Action_Item_Delete_Name 参数错误");
                return false;
            }
            int count = 0;
            GameStruct.RoleItemInfo item = play.GetItemSystem().FindItem(sName,ref count);
            if (count < nAmount) return false;
            play.GetItemSystem().DeleteItemByItemName(sName, nAmount);
            return true;
            
        }
        private void Action_Item_Delete(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');

            uint itemid = Convert.ToUInt32(option[0]);
            ushort amount = 1;
            //如果为0就删除当前的脚本道具
            if (itemid == 0)
            {
                itemid = play.GetItemSystem().GetScriptItemId();
            }
            if (option.Length == 2)
            {
                amount = Convert.ToUInt16(option[1]);
            }

            GameStruct.RoleItemInfo item = play.GetItemSystem().FindItem(itemid);
            if (item == null) return;
            item.amount -= amount;
            //没有使用数量了就删除该道具
            if (item.amount == 0)
            {
                play.GetItemSystem().DeleteItemByID(item.id);
            }
            else
            {
                //更新道具信息
                play.GetItemSystem().UpdateItemInfo(item.id);
            }
        }

        private bool Action_Check_Bag_Size(ActionInfo info, PlayerObject play)
        {
            String[] split = info.param.Split(' ');

            String pack = split[0];
            int nAddSize = Convert.ToInt32(split[1]);
            switch (pack.ToLower())
            {
                case "backpack": //人物背包
                    {
                        return play.GetItemSystem().GetBagCount() + nAddSize > PlayerItem.MAXBAG_COUNT;
                       
                    }
            }
            return false;
        }
        private bool Action_Equip_Operation(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            String command = option[0].ToLower();
            RoleItemInfo role_item = null;
            switch (command)
            {
                case "checkequip": //检测装备位置是否存在
                    {
                        byte postion = Convert.ToByte(option[1]);
                        role_item = play.GetItemSystem().GetEquipByPostion(postion);
                        return role_item == null ? false : true;
                    }
                case "setequippro": //设置装备属性
                    {
                        byte postion = Convert.ToByte(option[1]);
                        role_item = play.GetItemSystem().GetEquipByPostion(postion);
                        if (role_item == null) return false;
                        String op = option[2];
                        bool ret = true;
                        switch (op)
                        {
                            case "shui_attack":
                                {
                                    String opex = option[3];
                                    byte v = Convert.ToByte(option[4]);
                                    switch (opex)
                                    {
                                        case "=":
                                            {    role_item.shui_attack = v;   break; }
                                        case "-":
                                            {   role_item.shui_attack -= v;     break;  }
                                        case "+":
                                            {   role_item.shui_attack += v; break; }
                                    }
                                    break;
                                }
                            case "di_attack":
                                {
                                    String opex = option[3];
                                    byte v = Convert.ToByte(option[4]);
                                    switch (opex)
                                    {
                                        case "=":    {   role_item.di_attack = v;  break;  }
                                        case "-":   {   role_item.di_attack -= v;     break; }
                                        case "+": {   role_item.di_attack += v;  break; }
                                    }
                                    break;

                                }
                            case "huo_attack":
                                {
                                    String opex = option[3];
                                    byte v = Convert.ToByte(option[4]);
                                    switch (opex)
                                    {
                                        case "=":{   role_item.huo_attack = v;  break;  }
                                        case "-": {   role_item.huo_attack -= v;   break;  }
                                        case "+":  {    role_item.huo_attack += v;     break;  }
                                    }
                                    break;

                                }
                            case "feng_attack":
                                {
                                    String opex = option[3];
                                    byte v = Convert.ToByte(option[4]);
                                    switch (opex)
                                    {
                                        case "=": {   role_item.feng_attack = v;     break;    }
                                        case "-":     {         role_item.feng_attack -= v;     break;       }
                                        case "+":  {        role_item.feng_attack += v;   break;      }
                                    }
                                    break;

                                }
                            case "hole": //装备打洞 0为第一个洞 1为第二个洞 2为第三个洞
                                {
                                   String opex = option[3];
                                    byte v = Convert.ToByte(option[4]);
                                    
                                    switch (opex)
                                    {
                                        case "=": { role_item.OpenGem(v); break; }
                                      
                                      
                                    }
                                    break;
                                }
                            default:
                                {
                                    ret = false;
                                    break;
                                }
                        }
                        play.GetItemSystem().UpdateItemInfo(role_item.id);
                        play.CalcAttribute();//重新计算属性
                        return ret;
                    }
                case "checkequippro": //获取装备属性
                    {
                        byte postion = Convert.ToByte(option[1]);
                        role_item = play.GetItemSystem().GetEquipByPostion(postion);
                        if (role_item == null) return false;
                        String op = option[2];
                        String opex = option[3];
                        int v = Convert.ToInt32(option[4]);
                        switch (op)
                        {
                            case "shui_attack":
                                {
                                    switch (opex)
                                    {
                                        case "=":
                                            { return role_item.shui_attack == v; }
                                        case ">":
                                            { return role_item.shui_attack > v;}
                                        case ">=":
                                            {return role_item.shui_attack >= v;}
                                        case "<":
                                            {return role_item.shui_attack < v;    }
                                        case "<=":
                                            {  return role_item.shui_attack <= v;}
                                        default: { break; }
                                    }
                                    break;
                                }
                            case "di_attack":
                                {
                                    switch (opex)
                                    {
                                        case "=":
                                            {     return role_item.di_attack == v; }
                                        case ">":
                                            {return role_item.di_attack > v; }
                                        case ">=":
                                            { return role_item.di_attack >= v; }
                                        case "<":
                                            {  return role_item.di_attack < v; }
                                        case "<=":
                                            { return role_item.di_attack <= v;}
                                        default: { break; }
                                    }
                                    break;
                                }
                            case "huo_attack":
                                {
                                    switch (opex)
                                    {
                                        case "=":
                                            {   return role_item.huo_attack == v; }
                                        case ">":
                                            {       return role_item.huo_attack > v; }
                                        case ">=":
                                            {     return role_item.huo_attack >= v;   }
                                        case "<":
                                            {     return role_item.huo_attack < v;  }
                                        case "<=":
                                            {    return role_item.huo_attack <= v;  }
                                        default: { break; }
                                    }
                                    break;
                                }
                            case "feng_attack":
                                {
                                    switch (opex)
                                    {
                                        case "=":      {    return role_item.feng_attack == v;  }
                                        case ">":     {     return role_item.feng_attack > v;  }
                                        case ">=":  {    return role_item.feng_attack >= v;  }
                                        case "<":  {    return role_item.feng_attack < v;  }
                                        case "<=":{       return role_item.feng_attack <= v;  }
                                        default: { break; }
                                    }
                                    break;
                                }
                            case "hole": //装备打洞数量
                                {
                                    switch (opex)
                                    {
                                        case "=": { return role_item.GetGemCount() == v; }
                                    }
                                    break;
                                }
                            default: { break; }
                        }
                        break;
                    }
            }
            return false;
        }
        //检测道具使用等级
        private bool Action_Item_Level(ActionInfo info, PlayerObject play)
        {
            uint id = play.GetItemSystem().GetScriptItemId();
            GameStruct.RoleItemInfo iteminfo = play.GetItemSystem().FindItem(id);
            if (iteminfo == null) return false;
            GameStruct.ItemTypeInfo baseinfo = ConfigManager.Instance().GetItemTypeInfo(iteminfo.itemid);
            if (baseinfo == null) return false;
            String[] option = info.param.Split(' ');
            byte level = Convert.ToByte(option[1]);
            switch (option[0])
            {
                case ">":
                    {
                        return level > baseinfo.req_level;
                    }
                case "<":
                    {
                        return level < baseinfo.req_level;
                    }
                case "=":
                    {
                        return level == baseinfo.req_level;
                    }
                case ">=":
                    {
                        return level >= baseinfo.req_level;
                    }
                case "<=":
                    {
                        return level <= baseinfo.req_level;
                    }
            }
            return true;
        }
        //检测玩家是否有该技能
        public bool Action_CheckMagic(ActionInfo info, PlayerObject play)
        {
            uint magicid = Convert.ToUInt32(info.param);
            return play.GetMagicSystem().isMagic(magicid);
        }

        private void Action_Eudemon_CreateEx(ActionInfo info, PlayerObject play)
        {
            String[] split = info.param.Split(' ');
            if (split.Length < 1)
            {
                Log.Instance().WriteLog("Action_Eudemon_CreateEx 参数错误" + info.param + "id" + info.id.ToString());
                return;
            }
            uint itemid = Convert.ToUInt32(split[0]);
            if (ConfigManager.Instance().GetItemTypeInfo(itemid) == null)
            {
                Log.Instance().WriteLog("Action_Eudemon_CreateEx 物品id不存在"+itemid.ToString());
                return;
            }
            byte level = 0;
            if (split.Length >= 2) level = Convert.ToByte(split[1]);
            int quality = 0;
            if (split.Length >= 3) quality = Convert.ToInt32(split[2]);
            byte wuxing = 0;
            if (split.Length >= 4) wuxing = Convert.ToByte(split[3]);

            
           GameStruct.RoleItemInfo item =  play.GetItemSystem().AwardItem(itemid, NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK);
           if (level != 0 || quality != 0 || wuxing != 0)
           {
               item.typeid = IDManager.CreateTypeId(OBJECTTYPE.EUDEMON);
               RoleData_Eudemon eudemon = new RoleData_Eudemon();
               eudemon.typeid = item.typeid;
               eudemon.level = level;
               eudemon.quality = quality;
               eudemon.wuxing = wuxing;
               play.GetEudemonSystem().AddTempEudemon(eudemon);

           }
        }
        private void Action_Eudemon_Create(ActionInfo info, PlayerObject play)
        {
            String[] split = info.param.Split(' ');
            if (split.Length < 1)
            {
                Log.Instance().WriteLog("Action_Eudemon_Create 参数错误" + info.param + "id" + info.id.ToString());
                return;
            }
            uint itemid = Convert.ToUInt32(split[0]);

            int count = 1;
            if (split.Length >= 2)
            {
                count = Convert.ToInt32(split[1]);
            }
               
            //幻兽背包已满
            if (play.GetItemSystem().GetEudemonCount()+ count > PlayerEudemon.MAX_EUDEMON_COUNT)
            {
                play.ChatNotice("幻兽背包已满!!");
                return;

            }

            GameStruct.ItemTypeInfo baseitem = ConfigManager.Instance().GetItemTypeInfo(itemid);
            if (baseitem == null)
            {
                Log.Instance().WriteLog("创建幻兽出错,找不到该幻兽id" + itemid.ToString());
                return;
            }
            for (int i = 0; i < count; i++)
            {
                play.GetItemSystem().AwardItem(itemid, NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK);
            }
            //   GameStruct.RoleItemInfo iteminfo = null;
            //if (itemid == 0)
            //{
            //    iteminfo  = play.GetItemSystem().FindItem(play.GetItemSystem().GetScriptItemId());
            //    if (iteminfo == null) return;
            //}
            //else //没有道具的情况下增加幻兽,要先增加道具--
            //{

            //}

            // iteminfo.postion = NetMsg.MsgItemInfo.ITEMPOSITION_EUDEMON_PACK;
            // play.GetItemSystem().UpdateItemInfo(iteminfo.id);

            //增加幻兽
            //  play.GetEudemonSystem().AddEudemon(iteminfo);
        }

        public bool Action_Legion_Create(ActionInfo info, PlayerObject play)
        {
            String legionname = mszStr;
            mszStr = "";
            if (legionname.Length <= 0) return false;
            if (play.GetLegionSystem().IsHaveLegion())
            {
                return false;
            }
            //是否有重复的军团
            if (LegionManager.Instance().IsExist(legionname))
            {
                return false;
            }
            String[] option = info.param.Split(' ');
            int level = Convert.ToInt32(option[0]);//创建军团所需等级
            int money = Convert.ToInt32(option[1]); //创建军团的费用
            int capital = Convert.ToInt32(option[2]); //军团起始资金
            if (play.GetBaseAttr().level < level )
            {
                return false;
            }

            if (play.GetMoneyCount(MONEYTYPE.GOLD) < money )
            {
                return false;
            }
            
            play.ChangeMoney(MONEYTYPE.GOLD, money);


            LegionManager.Instance().CreateLegion(play.GetBaseAttr().player_id, legionname,play.GetName(), GameBase.Config.Define.LEGION_JUNTUAN, capital, "公告");
            //发给数据库服务器创建军团
         
            return true;
            //246, 0, 0, 0 为军团id
            //1107 为军团协议
           // 收到网络协议:长度：16协议号:1302
//            byte[] dat1 = {16,0,22,5,61,2,0,0,182,113,0,0,88,2,0,0};
//            play.GetGamePackKeyEx().EncodePacket(ref dat1, dat1.Length);
//            play.SendData(dat1);
//            //收到网络协议:长度：20协议号:1017

//            byte[] dat2 = { 20, 0, 249, 3, 64, 66, 15, 0, 1, 0, 0, 0, 9, 0, 0, 0, 100, 0, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref dat2, dat2.Length);
//            play.SendData(dat2);
////收到网络协议:长度：20协议号:1017
//            byte[] dat3 = {20,0,249,3,65,66,15,0,1,0,0,0,70,0,0,0,64,0,0,0};
//            play.GetGamePackKeyEx().EncodePacket(ref dat3, dat3.Length);
//            play.SendData(dat3);

////收到网络协议:长度：16协议号:1012

//            byte[] dat4 = { 16, 0, 244, 3, 64, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref dat4, dat4.Length);
//            play.SendData(dat4);
//收到网络协议:长度：108协议号:1106/
              //头衔注释
            //0.1.军团长 2.帮主  3.教主 4.会长
           // byte[] dat5 = {108,0,82,4,246,0,0,0,144,208,3,0,144,208,3,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,232,3,1,0,4/*！！！头衔*/,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,210,176,177,200,186,243,204,236,0,0,0,0,0,0,0,0,0,0,246,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
            //byte[] dat5 = { 108, 0, 82, 4, 246, 0, 0, 0, 144, 208, 3, 0, 144, 208, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4/*！！！头衔*/, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 210, 176, 177, 200, 186, 243, 204, 236, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 246, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref dat5, dat5.Length);
            //play.SendData(dat5);

            //收到网络协议:长度：21协议号:1015
            //{27,0,247,3,117,1,0,0,3,0,1,14,169,89,211,200,207,170,161,239,180,180,187,212,187,205,0}
            //byte[] data10 = { 21, 0, 247, 3, 246, 0, 0, 0, 3, 0, 1, 8, 196, 234, 201, 217, 187, 196, 204, 198, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref data10, data10.Length);
            //play.SendData(data10);

//收到网络协议:长度：172协议号:2056
//            byte[] dat6 = { 172, 0, 8, 8, 64, 66, 15, 0, 20, 0, 0, 0, 3, 0, 0, 0, 144, 208, 3, 0, 2, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 9, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 144, 208, 3, 0, 11, 0, 0, 0, 144, 208, 3, 0, 12, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 21, 0, 0, 0, 0, 0, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref dat6, dat6.Length);
//            play.SendData(dat6);

////收到网络协议:长度：108协议号:1106
//            byte[] dat7 = {108,0,82,4,246,0,0,0,144,208,3,0,144,208,3,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,232,3,0,0,0,0,232,3,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,210,176,177,200,186,243,204,236,0,0,0,0,0,0,0,0,0,0,246,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
//            play.GetGamePackKeyEx().EncodePacket(ref dat7, dat7.Length);
//            play.SendData(dat7);

////收到网络协议:长度：172协议号:2056
//            byte[] dat8 = { 172, 0, 8, 8, 64, 66, 15, 0, 20, 0, 0, 0, 3, 0, 0, 0, 144, 208, 3, 0, 2, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 9, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 144, 208, 3, 0, 11, 0, 0, 0, 144, 208, 3, 0, 12, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 21, 0, 0, 0, 0, 0, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref dat8, dat8.Length);
//            play.SendData(dat8);

////收到网络协议:长度：108协议号:1106
//            byte[] dat9={108,0,82,4,246,0,0,0,144,208,3,0,160,247,3,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,232,3,0,0,0,0,232,3,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,210,176,177,200,186,243,204,236,0,0,0,0,0,0,0,0,0,0,246,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
//            play.GetGamePackKeyEx().EncodePacket(ref dat9, dat9.Length);
//            play.SendData(dat9);

////收到网络协议:长度：172协议号:2056
//            byte[] dat10 = { 172, 0, 8, 8, 64, 66, 15, 0, 20, 0, 0, 0, 3, 0, 0, 0, 144, 208, 3, 0, 2, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 9, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 144, 208, 3, 0, 11, 0, 0, 0, 144, 208, 3, 0, 12, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 21, 0, 0, 0, 0, 0, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref dat10, dat10.Length);
//            play.SendData(dat10);

////收到网络协议:长度：108协议号:1106
//            byte[] dat11={108,0,82,4,246,0,0,0,144,208,3,0,160,247,3,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,232,3,0,0,0,0,232,3,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,210,176,177,200,186,243,204,236,0,0,0,0,0,0,0,0,0,0,246,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
//            play.GetGamePackKeyEx().EncodePacket(ref dat11, dat11.Length);
//            play.SendData(dat11);
//收到网络协议:长度：172协议号:2056
//            byte[] dat12 = { 172, 0, 8, 8, 64, 66, 15, 0, 20, 0, 0, 0, 3, 0, 0, 0, 144, 208, 3, 0, 2, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 9, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 144, 208, 3, 0, 11, 0, 0, 0, 144, 208, 3, 0, 12, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 21, 0, 0, 0, 0, 0, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref dat12, dat12.Length);
//            play.SendData(dat12);

////收到网络协议:长度：189协议号:1014
//            byte[] dat13 = { 189, 0, 246, 3, 64, 66, 15, 0, 33, 191, 2, 0, 33, 191, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 246, 0, 0, 0, 21, 0, 2, 0, 102, 66, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 172, 0, 114, 1, 132, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 97, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 232, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 197, 10, 0, 0, 246, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 8, 210, 176, 177, 200, 186, 243, 204, 236, 0, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref dat13, dat13.Length);
//            play.SendData(dat13);

////收到网络协议:长度：46协议号:1004
//            byte[] dat15 = { 46, 0, 236, 3, 0, 255, 255, 0, 63, 8, 0, 0, 71, 8, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 4, 6, 83, 89, 83, 84, 69, 77, 8, 210, 176, 177, 200, 186, 243, 204, 236, 0, 0, 0, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref dat15, dat15.Length);
//            play.SendData(dat15);



            //收到网络协议:长度：16协议号:1015

            //byte[] data = { 16, 0, 247, 3, 1, 0, 0, 0, 101, 0, 1, 3, 50, 52, 54, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref data, data.Length);
            //play.SendData(data);
            ////收到网络协议:长度：16协议号:1015

            //byte[] data1 = { 16, 0, 247, 3, 1, 0, 0, 0, 100, 0, 1, 3, 50, 52, 54, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref data1, data1.Length);
            //play.SendData(data1);
            //收到网络协议:长度：29协议号:1107

            //byte[] data2 = { 29, 0, 83, 4, 14, 0, 0, 0, 246, 0, 0, 0, 0, 0, 0, 0, 0, 1, 8, 196, 234, 201, 217, 187, 196, 204, 198, 0, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref data2, data2.Length);
            //play.SendData(data2);
            //收到网络协议:长度：20协议号:1107

            //byte[] data3 = { 20, 0, 83, 4, 119, 0, 0, 0, 246, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref data3, data3.Length);
            //play.SendData(data3);
            //收到网络协议:长度：16协议号:2036

            //byte[] data4 = { 16, 0, 244, 7, 205, 0, 2, 0, 246, 0, 0, 0, 0, 0, 0, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref data4, data4.Length);
            //play.SendData(data4);
            ////收到网络协议:长度：56协议号:2036

            //byte[] data5 = { 56, 0, 244, 7, 210, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 246, 0, 0, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref data5, data5.Length);
            //play.SendData(data5);
            //收到网络协议:长度：12协议号:1015

            //byte[] data6 = { 12, 0, 247, 3, 0, 0, 0, 0, 152, 0, 0, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref data6, data6.Length);
            //play.SendData(data6);
            ////收到网络协议:长度：20协议号:1017

            //byte[] data7 = { 20, 0, 249, 3, 64, 66, 15, 0, 1, 0, 0, 0, 4, 0, 0, 0, 11, 139, 16, 0 };
            //play.GetGamePackKeyEx().EncodePacket(ref data7, data7.Length);
            //play.SendData(data7);

//收到网络协议:长度：74协议号:1107
//            byte[] data8 = { 74, 0, 83, 4, 171, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, 5, 48, 32, 48, 32, 48, 5, 49, 32, 48, 32, 48, 5, 50, 32, 48, 32, 48, 5, 51, 32, 48, 32, 48, 5, 52, 32, 48, 32, 48, 5, 53, 32, 48, 32, 48, 5, 54, 32, 48, 32, 48, 5, 55, 32, 48, 32, 48, 5, 56, 32, 48, 32, 48, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref data8, data8.Length);
//            play.SendData(data8);
////
////收到网络协议:长度：16协议号:1182
////
//            byte[] data9 = { 16, 0, 158, 4, 7, 0, 0, 0, 232, 3, 0, 0, 3, 0, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref data9, data9.Length);
//            play.SendData(data9);

//收到网络协议:长度：16协议号:1302
//
//            byte[] data11 = { 16, 0, 22, 5, 61, 2, 0, 0, 253, 101, 0, 0, 88, 2, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref data11, data11.Length);
//            play.SendData(data11);
////收到网络协议:长度：28协议号:1010
////
//            byte[] data12 = { 28, 0, 242, 3, 118, 202, 19, 86, 65, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 73, 37, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref data12, data12.Length);
//            play.SendData(data12);
//收到网络协议:长度：28协议号:1010
//
//            byte[] data13 = { 28, 0, 242, 3, 118, 202, 19, 86, 65, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 73, 37, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref data13, data13.Length);
//            play.SendData(data13);
////收到网络协议:长度：28协议号:1010
////
//            byte[] data14 = { 28, 0, 242, 3, 118, 202, 19, 86, 194, 148, 53, 119, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 73, 37, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref data14, data14.Length);
//            play.SendData(data14);
//收到网络协议:长度：28协议号:1010
//
//            byte[] data15 = { 28, 0, 242, 3, 118, 202, 19, 86, 193, 148, 53, 119, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 73, 37, 0, 0 };
//            play.GetGamePackKeyEx().EncodePacket(ref data15, data15.Length);
//            play.SendData(data15);
////收到网络协议:长度：20协议号:1017
////{
//            byte[] data16 ={20,0,249,3,65,66,15,0,1,0,0,0,70,0,0,0,64,0,0,0};
//            play.GetGamePackKeyEx().EncodePacket(ref data16, data16.Length);
//            play.SendData(data16);
//收到网络协议:长度：192协议号:1014 天晴猪宝贝
                    //byte[] data17 ={192,0,246,3,65,66,15,0,3,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,64,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,132,16,2,0,0,0,0,0,0,0,0,0,0,0,0,0,159,0,110,1,100,0,0,0,0,5,0,0,0,0,0,0,125,10,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,197,10,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,11,183,201,184,235,54,50,56,91,71,77,93,0,0,0};
                    //play.GetGamePackKeyEx().EncodePacket(ref data17, data17.Length);
                    //play.SendData(data17);
//
//收到网络协议:长度：28协议号:2036
//
                    //byte[] data18= { 28, 0, 244, 7, 109, 0, 5, 0, 65, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    //play.GetGamePackKeyEx().EncodePacket(ref data18, data18.Length);
                    //play.SendData(data18);
//收到网络协议:长度：28协议号:1017
//
                    //byte[] data19 = { 28, 0, 249, 3, 65, 66, 15, 0, 2, 0, 0, 0, 36, 0, 0, 0, 0, 4, 0, 0, 70, 0, 0, 0, 64, 0, 0, 0 };
                    //play.GetGamePackKeyEx().EncodePacket(ref data19, data19.Length);
                    //play.SendData(data19);



//收到网络协议:长度：108协议号:1106
//这个是更改军团头衔 会长 军团战 帮主之类的
                    //byte[] data20 = { 108, 0, 82, 4, 246, 0, 0, 0, 144, 208, 3, 0, 160, 247, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 232, 3, 0, 0, 0, 0, 232, 3, 1, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 210, 176, 177, 200, 186, 243, 204, 236, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 246, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    //play.GetGamePackKeyEx().EncodePacket(ref data20, data20.Length);
                    //play.SendData(data20);


            //收到网络协议:长度：172协议号:2056
                    //byte[] data21 = { 172, 0, 8, 8, 76, 152, 15, 0, 20, 0, 0, 0, 3, 0, 0, 0, 144, 208, 3, 0, 2, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 9, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 144, 208, 3, 0, 11, 0, 0, 0, 144, 208, 3, 0, 12, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 21, 0, 0, 0, 0, 0, 0, 0 };
                    //play.GetGamePackKeyEx().EncodePacket(ref data21, data21.Length);
                    //play.SendData(data21);
//
//收到网络协议:长度：189协议号:1014 野比后天
                    //byte[] data22 = { 189, 0, 246, 3, 76, 152, 15, 0, 33, 191, 2, 0, 33, 191, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 246, 0, 0, 0, 21, 0, 2, 0, 102, 66, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 172, 0, 114, 1, 132, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 97, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 232, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 197, 10, 0, 0, 246, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 8, 210, 176, 177, 200, 186, 243, 204, 236, 0, 0, 0 };
                    //play.GetGamePackKeyEx().EncodePacket(ref data22, data22.Length);
                    //play.SendData(data22);
//
            //return true;
        }

        private void Action_Legion_ChangeTitle(ActionInfo info, PlayerObject play)
        {
            if (!play.GetLegionSystem().IsHaveLegion()) return;
            byte bTitle = Convert.ToByte(info.param);
            play.GetLegionSystem().ChangeLegionTitle(bTitle);
        }

          private bool  Action_TimeOut_Create(ActionInfo info, PlayerObject play)
          {
              String[] option = info.param.Split(' ');
              int time_id = Convert.ToInt32(option[0]);
              int time  = Convert.ToInt32(option[1]);
              uint callback_id = Convert.ToUInt32(option[2]);
              return ScriptTimerManager.Instance().AddPlayerTimeOut(time_id, play.GetBaseAttr().player_id, time, callback_id);
          }
         private bool  Action_TimeOut_Check(ActionInfo info, PlayerObject play)
          {
             
              int time_id = Convert.ToInt32(info.param);
              return ScriptTimerManager.Instance().CheckPlayerTimeOut(time_id, play.GetBaseAttr().player_id);
          }

         private void Action_TimeOut_Delete(ActionInfo info, PlayerObject play)
          {
              int time_id = Convert.ToInt32(info.param);
              ScriptTimerManager.Instance().DeletePlayerTimeOut(time_id, play.GetBaseAttr().player_id);
          }

        private bool Action_Magic_Operation(ActionInfo info,PlayerObject play)
         {
             bool ret = true;

             String[] split = info.param.Split(' ');
             String command = split[0];
             switch (command.ToLower())
             {
                 case "learnmagic":
                     {
                         byte profession = Convert.ToByte(split[1]);
                         uint magic_id = Convert.ToUInt32(split[2]);
                         byte level = Convert.ToByte(split[3]);
                         uint exp = Convert.ToUInt32(split[4]);
                         GameStruct.MagicTypeInfo type_info = ConfigManager.Instance().GetMagicTypeInfo(magic_id);
                         if (type_info == null) return false;
                         if (play.GetBaseAttr().profession != profession)
                         {
                             ret = false;
                             play.LeftNotice("职业不符,无法学习技能");
                             break;
                         }
                         //if (play.GetBaseAttr().level < type_info.need_level)
                         //{
                         //    ret = false;
                         //    play.LeftNotice("等级不足,无法学习!");
                         //    break;
                         //}
                         if (play.GetMagicSystem().isMagic(magic_id))
                         {
                             ret = false;
                             play.LeftNotice("你已经学会了" + type_info.name + ",请勿重复学习!");
                             break;

                         }
                         play.GetMagicSystem().AddMagicInfo(magic_id, level, exp);
                         play.LeftNotice("恭喜阁下学会" + type_info.name);   
                         break;
                     }
             }
             return ret;
         }
        private void Action_Random_Init(ActionInfo info, PlayerObject play)
         {
            String[] option = info.param.Split(' ');
            int min = Convert.ToInt32(option[0]);
            int max = Convert.ToInt32(option[1]);
            play.SetCurrentRandom(IRandom.Random(min, max));
         }

        private bool Action_Random_Compare(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            int nValue  = Convert.ToInt32(option[1]);
            int nRandom = play.GetCurrentRandom();
            switch (option[0])
            {
                case ">":
                    {
                        return nValue > nRandom;
                       
                    }
                case "=":
                    {
                        return nValue == nRandom;
                    }
                case "<":
                    {
                        return nValue < nRandom;
                    }
                case ">=":
                    {
                        return nValue >= nRandom;
                    }
                case "<=":
                    {
                        return nValue <= nRandom;
                    }
            }
            return false;
        }


   
        public void     Action_Set_Eudemon_Pro(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            int index = Convert.ToInt32(option[0]);
            uint eudemon_id = 0;
            if (index == 0)
            {
                eudemon_id = play.GetUseItemEudemonId();
            }

            RoleData_Eudemon eudemon = play.GetEudemonSystem().FindEudemon(eudemon_id);
            EudemonObject eudemon_obj = play.GetEudemonSystem().GetEudmeonObject(eudemon_id);
            if (eudemon == null || eudemon_obj == null) return;
            String sPro = option[1];
            String op = option[2];
            int value = Convert.ToInt32(option[3]);
            switch (sPro)
            {
                case "quality":
                    {
                        switch (op)
                        {
                            case "+": { eudemon.quality += value; break; }
                            case "-": { eudemon.quality -= value; break; }
                            case "=": { eudemon.quality = value; break; }
                        }
                        break;
                    }
                case "wuxing":
                    {
                        switch (op)
                        {
                            case "=": { eudemon.wuxing = value; break; }
                              
                        }
                        break;
                    }
            }
         
            if (eudemon != null)
            {
                eudemon_obj.SetEudemonInfo(eudemon);
                play.GetEudemonSystem().SendEudemonInfo(eudemon, true, true);
            }
        }
        public bool Action_Get_Eudemon_Pro(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            int index = Convert.ToInt32(option[0]);
            uint eudemon_id = 0;
            if (index == 0)
            {
                eudemon_id = play.GetUseItemEudemonId();
            }
            
            RoleData_Eudemon eudemon = play.GetEudemonSystem().FindEudemon(eudemon_id);
          
            if (eudemon == null ) return false;
            String sPro = option[1];
            String op = option[2];
            int value = Convert.ToInt32(option[3]);
            switch (sPro)
            {
                case "quality":
                    {
                        switch (op)
                        {
                            case ">": { return eudemon.quality > value; }
                            case ">=": { return eudemon.quality >= value; }
                            case "=": { return eudemon.quality == value; }
                            case "<": { return eudemon.quality < value; }
                            case "<=": { return eudemon.quality <= value; }
                        }
                        return false;
                    
                    }
                case "wuxing":
                    {
                        switch (op)
                        {
                            case "=": { return eudemon.wuxing == value; }
                            case "!=": { return eudemon.wuxing != value; }
                        }
                        return false;
                    }
                case "level":
                    {
                        switch (op)
                        {
                            case ">": { return eudemon.level > value; }
                            case ">=": { return eudemon.level >= value; }
                            case "=": { return eudemon.level == value; }
                            case "<": { return eudemon.level < value; }
                            case "<=": { return eudemon.level <= value; }
                        }
                        break;
                    }
            }
        
            return false;
           
        }
        private void Action_Recall_Eudemon(ActionInfo info, PlayerObject play)
        {
            int type = Convert.ToInt32(info.param);
            switch (type)
            {
                case 0: //出征与合体的幻兽
                    {
                        play.GetEudemonSystem().Eudemon_ReCallAll(false);
                        play.GetEudemonSystem().Eudemon_BreakUpAll();
                        break;
                    }
                case 1: //出征的幻兽
                    {
                        play.GetEudemonSystem().Eudemon_ReCallAll(false);
                        break;
                    }
                case 2://合体的幻兽
                    {
                        play.GetEudemonSystem().Eudemon_BreakUpAll();
                        break;
                    }
            }
        }
        private void Action_Fuck_Nian(ActionInfo info, PlayerObject play)
        {
            int nLayer = Convert.ToInt32(info.param);
            int[,] dropitem = null; //1维数为道具id  2维数为概率
            switch (nLayer)
            {
                    //十一层
                case 11:
                    {
                        dropitem = new int[5, 5];
                        dropitem[0, 0] = 180000; dropitem[0, 1] = 1;
                        dropitem[1, 0] = 743388; dropitem[1, 1] = 9;
                        dropitem[2, 0] = 743382; dropitem[2, 1] = 90;
                        dropitem[3, 0] = 743381; dropitem[3, 1] = 250;
                        dropitem[4, 0] = 743380; dropitem[4, 1] = 650;
                        break;
                    }
                case 12://十二层
                    {
                        dropitem = new int[5, 5];
                        dropitem[0, 0] = 180020; dropitem[0, 1] = 1;
                        dropitem[1,0] = 743492; dropitem[1, 1] = 9;
                        dropitem[2, 0] = 743385; dropitem[2, 1] = 90;
                        dropitem[3, 0] = 743384; dropitem[3, 1] = 250;
                        dropitem[4, 0] = 743383; dropitem[4, 1] = 650;
                        break;
                       
                    }
                case 13://十三层
                    {
                        dropitem = new int[6, 6];
                        dropitem[0, 0] = 180040; dropitem[0, 1] = 1;
                        dropitem[1, 0] = 743495; dropitem[1, 1] = 9;
                        dropitem[2, 0] = 743389; dropitem[2, 1] = 90;
                        dropitem[3, 0] = 743386; dropitem[3, 1] = 100;
                        dropitem[4, 0] = 743385; dropitem[4, 1] = 150;
                        dropitem[5, 0] = 743384; dropitem[5, 1] = 650;
                        break;
                    }
                case 14://十四层
                    {
                        dropitem = new int[5, 5];
                        dropitem[0, 0] = 180060; dropitem[0, 1] = 1;
                        dropitem[1, 0] = 743497; dropitem[1, 1] = 9;
                        dropitem[2, 0] = 743389; dropitem[2, 1] = 90;
                        dropitem[3, 0] = 743386; dropitem[3, 1] = 250;
                        dropitem[4, 0] = 743385; dropitem[4, 1] = 650;
                        break;
                    }
                case 15: //十五层
                    {
                        dropitem = new int[5, 5];
                        dropitem[0, 0] = 180080; dropitem[0, 1] = 1;
                        dropitem[1, 0] = 743500; dropitem[1, 1] = 9;
                        dropitem[2, 0] = 743491; dropitem[2, 1] = 90;
                        dropitem[3, 0] = 743390; dropitem[3, 1] = 250;
                        dropitem[4, 0] = 743387; dropitem[4, 1] = 650;
                        break;
                    }
                case 16: //十六层
                    {
                        dropitem = new int[5, 5];
                        dropitem[0, 0] = 180100; dropitem[0, 1] = 1;
                        dropitem[1, 0] = 743501; dropitem[1, 1] = 9;
                        dropitem[2, 0] = 743493; dropitem[2, 1] = 90;
                        dropitem[3, 0] = 743491; dropitem[3, 1] = 250;
                        dropitem[4, 0] = 743390; dropitem[4, 1] = 650;
                        break;
                    }
                case 17: //十七层
                    {
                        dropitem = new int[5, 5];
                        dropitem[0, 0] = 180120; dropitem[0, 1] = 1;
                        dropitem[1, 0] = 743502; dropitem[1, 1] = 9;
                        dropitem[2, 0] = 743496; dropitem[2, 1] = 90;
                        dropitem[3, 0] = 743493; dropitem[3, 1] = 250;
                        dropitem[4, 0] = 743491; dropitem[4, 1] = 650;
                        break;
                    }
                case 18://十八层
                    {
                        dropitem = new int[5, 5];
                        dropitem[0, 0] = 180140; dropitem[0, 1] = 1;
                        dropitem[1, 0] = 743503; dropitem[1, 1] = 9;
                        dropitem[2, 0] = 743499; dropitem[2, 1] = 90;
                        dropitem[3, 0] = 743497; dropitem[3, 1] = 250;
                        dropitem[4, 0] = 743494; dropitem[4, 1] = 650;
                        break;
                    }
            }
            
            //传送到雷鸣交易行
            play.ChangeMap(1000, 296, 520);
            if (dropitem != null)
            {
                int rand = IRandom.Random(1, 1000);
                for (int i = 0; i < 10; i++) { rand = IRandom.Random(1, 1000); }
                int add_rand = 0;
                for (int i = 0; i < dropitem.Length; i++)
                {
                    add_rand += dropitem[i, 1];
                    if (rand <= add_rand)
                    {
                        GameStruct.ItemTypeInfo typeinfo = ConfigManager.Instance().GetItemTypeInfo((uint)dropitem[i, 0]);
                        if (typeinfo == null) continue;
                        play.GetItemSystem().AwardItem((uint)dropitem[i, 0], NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK);
                        play.MsgBox("小婊砸你被打出来了！");
                        break;
                    }
                }
            }
        }

        public bool Action_Fuben_Create(ActionInfo info, PlayerObject play)
        {
            String[] option = info.param.Split(' ');
            uint mapid = Convert.ToUInt32(option[0]);
            byte type = Convert.ToByte(option[1]);
            short x = Convert.ToInt16(option[2]);
            short y = Convert.ToInt16(option[3]);
            GameMap fb_map =  MapManager.Instance().AddFubenMap(mapid);
            if (fb_map == null)
            {
                return false;
            }
            if(type == 1)//单人副本
            {
                play.ChangeFubenMap(fb_map, x, y);
            }
            else if (type == 2)//组队副本
            {

            }
            return true;
        }
        //格式化字符串
        public String Sprintf_string(String text, PlayerObject play)
        {
            String ret = text;
            bool bBreak = false;
            while (true)
            {
                int pos = ret.IndexOf('[');
                if (pos == -1) break;
                int endpos = ret.IndexOf(']');
                if (endpos == -1) break;
                String command = ret.Substring(pos+1, endpos - pos -1);
                String[] option = command.Split(',');
                String sReq = ret.Substring(pos, endpos - pos + 1);
                String req = "";
                switch (option[0])
                {
                    case "username": //用户名称
                        {
                            ret = ret.Replace(command, play.GetName());
                            break;
                        }
                    case "itemname": //道具名称
                        {
                            GameStruct.RoleItemInfo roleitem = play.GetItemSystem().FindItem(play.GetItemSystem().GetScriptItemId());
                            if (roleitem != null)
                            {
                                GameStruct.ItemTypeInfo baseitem = ConfigManager.Instance().GetItemTypeInfo(roleitem.itemid);
                                if (baseitem != null)
                                {
                                    req = baseitem.name;
                                }
                            }
                            ret = ret.Replace(sReq, req);
                            break;
                        }
                    case "timeout": //定时器剩余时间
                        {
                            int time_id = Convert.ToInt32(option[1]);
                            req = ScriptTimerManager.Instance().GetPlayerTimeOutS(time_id, play.GetBaseAttr().player_id).ToString()+"秒";
                            ret = ret.Replace(sReq, req);
                            break;
                        }
                    default:
                        {
                            bBreak = true;
                            break;
                        }
                }
                if (bBreak) break;

            }
            return ret;
        }



    }
}
