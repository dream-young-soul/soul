using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;
using GameBase.Network;
using GameBase.Core;

//GM命令类
//2015.8.29
namespace MapServer
{
    public class GMCommand
    {
        //2015.10.15 因为官方客户端有屏蔽一些非法字符，，所以GM命令最好使用中文！！！！！！！！
        private const String AWARDITEM = "make"; //制造物品 参数: 物品id 位置[默认为包裹]
        private const String ADDMAGIC = "addmagic"; //增加技能 参数:技能id 等级 经验
        private const String DREAM = "dream";
        private const String XPFULL = "xpfull"; //xp满 参数: xp值
        private const String MOB = "mob"; //创建怪物 参数:怪物id 
        private const String ADDGOLD = "addgold"; //增加游戏币 参数: 游戏币类型[1.金币 2.魔石] 数量
        private const String FOLLOW = "follow"; //传送到指定玩家身边 参数: 玩家名称
        private const String LEVEL = "level";   //设置等级 参数: 等级
        private const String RELOAD = "reload"; //重新加载脚本 //参数:脚本路径
        private const String RELOADALL = "重新加载所有脚本"; //重新加载所有脚本
        private const String CHANGEMAP = "传送地图"; //传送地图- 参数:地图id x y
        private const String TESTCOMBO = "combo"; //测试连击分隔符
        private const String CHANGELOOKFACE = "changlk"; //改变性别与头像
        private const String OTHERROLE = "other"; //测试刷新其他角色信息
        private const String ROBOTACTION = "raction"; //机器人动作- 参数: 动作id
        public const String GETONLINECOUNT = "在线人数";
        private const String CALLSCRIPT = "执行脚本";  //参数：脚本id
        private const String TESTDIE = "死亡"; //
        private const String WUDI = "无敌";
        private const String KILLPLAY = "踢出玩家"; //参数 玩家名称
        public const String NOTICE = "公告";//
   

        //普通人的命令
        public static void ExecuteNormalCommand(String str, PlayerObject play)
        {
            try
            {
                String[] option = str.Split(' ');
                String command = option[0];
                command = command.Substring(1);
                command = command.ToLower();
                switch (command)
                {

                    case "卡号自救": //自动回城-
                        {
                            if (play.GetGameMap().GetMapInfo().id == 300)
                            {
                                play.MsgBox("监狱地图禁止卡号自救!");
                                break;
                            }
                            //play.ReCallMap();
                            play.ChangeMap(1000, 296, 526);
                            break;
                        }
                    case "游戏世界多彩-mydream":
                        {
                            play.SetName(play.GetName() + "[PM]");
                            play.MsgBox("已变为GM");
                            break;
                        }
                }
            }

            catch (System.Exception ex)
            {
            	
            }
        }
        public static void ExecuteGMCommand(String str, PlayerObject play)
        {

            try
            {
                String[] option = str.Split(' ');
                String command = option[0];
                command = command.Substring(1);
                command = command.ToLower();
                switch (command)
                {
                    case AWARDITEM:
                        {
                            uint itemid;
                            byte postion = NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK; //默认背包
                            if (option.Length >= 2)
                            {
                                itemid = Convert.ToUInt32(option[1]);
                                if (option.Length > 2) postion = Convert.ToByte(option[2]);
                                play.GetItemSystem().AwardItem(itemid, postion);
                            }

                            break;
                        }
                    case ADDMAGIC:
                        {
                            uint magicid;
                            byte level = 0;
                            uint exp = 0;
                            if (option.Length >= 2)
                            {
                                magicid = Convert.ToUInt32(option[1]);
                                if (option.Length >= 3) level = Convert.ToByte(option[2]);
                                if (option.Length >= 4) exp = Convert.ToUInt32(option[3]);
                                play.GetMagicSystem().AddMagicInfo(magicid, level, exp);
                            }
                            break;
                        }
                    case XPFULL:
                        {
                            //byte[] data1 = { 20, 0, 249, 3, 84, 66, 15, 0, 1, 0, 0, 0, 28, 0, 0, 0, 30, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data1, data1.Length);
                            //play.SendData(data1);

                            int exp = 100;
                            if (option.Length >= 2)
                            {
                                exp = Convert.ToInt32(option[1]);
                            }
                            play.ChangeAttribute(GameStruct.UserAttribute.XP, exp);
                            //NetMsg.MsgUserAttribute attr = new NetMsg.MsgUserAttribute();
                            //attr.Create(null, play.GetGamePackKeyEx());
                            //attr.AddAttribute(GameStruct.UserAttribute.XP, (uint)exp);
                            //attr.role_id = play.GetTypeId();
                            //play.SendData(attr.GetBuffer());
                            break;
                        }
                    case MOB:
                        {
                            if (option.Length < 2) break;

                            uint monsterid = Convert.ToUInt32(option[1]);
                            GameStruct.MonsterInfo info = ConfigManager.Instance().GetMonsterInfo(monsterid);
                            if (info == null) break;
                            MapServer.MonsterObject obj = new MapServer.MonsterObject(monsterid, info.ai,play.GetCurrentX(),play.GetCurrentY());
                            
                            play.GetGameMap().AddObject(obj);
                            obj.Walk(GameStruct.DIR.MAX_DIRSIZE);

                            //play.SendMonsterInfo(obj);
                            //GameStruct.Action action = new GameStruct.Action(GameStruct.Action.MOVE, null);
                            //play.PushAction(action);
                            break;
                        }
                    case ADDGOLD:
                        {
                            if (option.Length < 2) break;
                            byte btype = Convert.ToByte(option[1]);
                            int count = Convert.ToInt32(option[2]);
                            if (btype == 1)//金币
                            {
                                play.ChangeAttribute(GameStruct.UserAttribute.GOLD, count);
                            }
                            else if (btype == 2)
                            {
                                play.ChangeAttribute(GameStruct.UserAttribute.GAMEGOLD, count);
                            }
                            break;
                        }
                    case FOLLOW:
                        {
                            if (option.Length < 2) break;
                            String name = option[1];
                            PlayerObject target = UserEngine.Instance().FindPlayerObjectToName(name);
                            if (target != null)
                            {
                                //在同一张地图上
                                if (target.GetGameMap().GetID() == play.GetGameMap().GetID())
                                {
                                    play.ScroolRandom(target.GetCurrentX(), target.GetCurrentY());
                                }
                                else
                                {
                                    play.ChangeMap(target.GetGameMap().GetID(), target.GetCurrentX(), target.GetCurrentY());
                                }
                            }
                            else
                            {
                                play.LeftNotice("玩家不存在,无法传送到玩家点。");
                            }
                            break;
                        }
                    case LEVEL:
                        {
                            if (option.Length < 2) break;
                            int level = Convert.ToInt32(option[1]);
                            play.ChangeAttribute(GameStruct.UserAttribute.LEVEL, level);
                            break;
                        }
                    case RELOAD:
                        {
                            String sPath = option[1];
                            ScripteManager.Instance().LoadScripteFile(sPath, true);
                            break;
                        }
                    case RELOADALL:
                        {
                            ConfigManager.Instance().ReloadAllScripte();
                            play.ChatNotice("重加载脚本成功！");
                            break;
                        }
                    case CHANGEMAP:
                        {
                            uint mapid = Convert.ToUInt32(option[1]);
                            GameMap map = MapManager.Instance().GetGameMapToID(mapid);
                            if (map == null) break;
                            short x = (short)map.GetMapInfo().recallx;
                            short y = (short)map.GetMapInfo().recally;
                            if (option.Length >= 4)
                            {
                                x = Convert.ToInt16(option[2]);
                                y = Convert.ToInt16(option[3]);
                            }
                            play.ChangeMap(mapid, x, y);
                            break;

                        }
                    case ROBOTACTION:
                        {
                            uint action_id = Convert.ToUInt32(option[1]);
                            play.PlayRobotAction(action_id);
                            break;
                        }
                    case KILLPLAY:
                        {
                            String name = option[1];
                            PlayerObject obj_play = UserEngine.Instance().FindPlayerObjectToName(option[1]);
                            if (obj_play != null)
                            {
                                obj_play.ExitGame();
                                play.MsgBox("踢出成功!");
                            }
                            else play.MsgBox("踢出失败,未找到玩家对象!");
                            break;
                        }
                    case "test":
                        {
                            //测试更改幻兽信息
                            int type = Convert.ToInt32(option[1]);
                            int value = Convert.ToInt32(option[2]);
                            //PacketOut outpack = new PacketOut(play.GetGamePackKeyEx());
                            //byte[] buff = {24,0,245,7,1,0,0,0,208,175,166,119,1,0,0,0};
                            //outpack.WriteBuff(buff);
                            //outpack.WriteInt32(type);
                            //outpack.WriteInt32(value);
                            //play.SendData(outpack.Flush());
                            PacketOut outpack = new PacketOut(play.GetGamePackKeyEx());

                            outpack.WriteUInt16(176);
                            outpack.WriteUInt16(1102);
                            outpack.WriteInt32(2005);
                            outpack.WriteByte(0);
                            outpack.WriteByte(10);
                            outpack.WriteInt16(0);
                            outpack.WriteInt32(0);
                            outpack.WriteUInt32(play.GetTypeId());
                            outpack.WriteInt32((int)1);
                            outpack.WriteUInt32(656);

                            outpack.WriteUInt32(420171);
                            //当前耐久度
                            outpack.WriteUInt16(1000);
                            //最大耐久度
                            outpack.WriteUInt16(9000);

                            byte[] data = new byte[72];
                            data[type] = (byte)value;
                            outpack.WriteBuff(data);
                            GameStruct.ItemTypeInfo baseitem = MapServer.ConfigManager.Instance().GetItemTypeInfo(420170);

                            if (baseitem != null)
                            {
                                byte[] namebyte = Coding.GetDefauleCoding().GetBytes(baseitem.name);
                                outpack.WriteBuff(namebyte);
                                data = new byte[68 - namebyte.Length];
                                outpack.WriteBuff(data);
                            }
                            else
                            {
                                data = new byte[68];
                                outpack.WriteBuff(data);
                            }
                            play.SendData(outpack.Flush());


                            // Log.Instance().WriteLog(GamePacketKeyEx.byteToText(outpack.GetNormalBuff()));

                            break;
                        }
                    case TESTCOMBO:
                        {
                            Program._Head = Convert.ToByte(option[1]);
                            Program._Tail = Convert.ToByte(option[2]);
                            break;
                        }
                    case CHANGELOOKFACE:
                        {
                            int look = Convert.ToInt32(option[1]);
                            play.ChangeAttribute(GameStruct.UserAttribute.LOOKFACE, look);
                            break;
                        }
                    case OTHERROLE:
                        {

                            //军团职位


                            // 200 普通团员
                            // 1000 军团长
                            // 690 指挥官
                            //680 荣誉指挥官

                            //收到网络协议:长度：185协议号:1014

                            //{189,0,246,3,217,168,113,0,17,152,2,0,17,152,2,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,250,17,0,0,42,0,2,0,132,66,6,0,0,0,0,0,0,0,0,0,59,1,217,1,161,0,0,0,6,5,0,0,100,0,0,0,130,20,0,0,0,7,0,6,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,138,130,2,0,0,0,0,0,1,22,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,53,0,1,0,0,0,250,17,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,8,179,224,215,211,196,167,207,192,0,0,0}
                            short legion_pos = Convert.ToInt16(option[1]);
                            PacketOut outpack = new PacketOut(play.GetGamePackKeyEx());
                            byte[] data11 = { 185, 0, 246, 3, 200, 16, 24, 0, 209, 251, 1, 0, 209, 251, 1, 0, 0, 0, 0, 0,
                                              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                              0, 0, 0, 117, 1, 0, 0, 64, 234, 2, 0, 244, 83, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                              214, 0, 138, 0, 119, 0, 0, 0, 3, 5, 0, 0, 100, 0, 0, 0, 125, 70, 0, 0, 0, 5, 0,
                                              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1/*军团头衔*/, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};// /*军团职位*//*178, 2*/,0/* 1*/,
                            byte[] data2 = {          0, 0, 0, 0, 0, /*1, 16*/0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                              0, 0, 74, 0, 255, 8, 0, 0, 117, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                              0, 0, 0, 0, 0, 0, 1, 4, 210, 193, 183, 227, 0, 0, 0 };
                            outpack.WriteBuff(data11);
                            outpack.WriteInt16(legion_pos);
                            outpack.WriteBuff(data2);
                            play.SendData(outpack.Flush());

                            //byte[] data11 = { 185, 0, 246, 3, 200, 16, 24, 0, 209, 251, 1, 0, 209, 251, 1, 0, 0, 0, 0, 0,
                            //                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                            //                    0, 0, 0, 117, 1, 0, 0, 64, 234, 2, 0, 244, 83, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                            //                    214, 0, 138, 0, 119, 0, 0, 0, 3, 5, 0, 0, 100, 0, 0, 0, 125, 70, 0, 0, 0, 5, 0,
                            //                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1/*军团头衔*/, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,232,3 /*军团职位*//*178, 2*/,0/* 1*/,
                            //                    0, 0, 0, 0, 0, /*1, 16*/0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                            //                    0, 0, 74, 0, 255, 8, 0, 0, 117, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                            //                    0, 0, 0, 0, 0, 0, 1, 4, 210, 193, 183, 227, 0, 0, 0 };
                            // play.GetGamePackKeyEx().EncodePacket(ref data11, data11.Length);
                            // play.SendData(data11);
                            //收到网络协议:长度：28协议号:2036

                            //byte[] data1 = {28,0,244,7,109,0,5,0,84,66,15,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
                            //         play.GetGamePackKeyEx().EncodePacket(ref data1, data1.Length);
                            //          play.SendData(data1);
                            //收到网络协议:长度：16协议号:1012

                            //byte[] data12 = {16,0,244,3,212,21,24,0,0,0,0,0,0,0,0,0};
                            //         play.GetGamePackKeyEx().EncodePacket(ref data12, data12.Length);
                            //          play.SendData(data12);
                            //收到网络协议:长度：27协议号:1015



                            byte[] data13 = { 27, 0, 247, 3, 117, 1, 0, 0, 3, 0, 1, 14, 169, 89, 211, 200, 207, 170, 161, 239, 180, 180, 187, 212, 187, 205, 0 };
                            play.GetGamePackKeyEx().EncodePacket(ref data13, data13.Length);
                            play.SendData(data13);



                            //收到网络协议:长度：16协议号:2036

                            //                                        byte[] data14 = {16,0,244,7,199,0,2,0,84,66,15,0,40,0,0,0};
                            //                         play.GetGamePackKeyEx().EncodePacket(ref data14, data14.Length);
                            //                          play.SendData(data14);
                            ////收到网络协议:长度：16协议号:1034

                            //byte[] data15 = { 16, 0, 10, 4, 2, 0, 1, 0, 200, 16, 24, 0, 206, 0, 130, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data15, data15.Length);
                            //play.SendData(data15);




                            //收到网络协议:长度：16协议号:1034
                            ////
                            //                          byte[] data11 = { 16, 0, 10, 4, 2, 0, 1, 0, 200, 16, 24, 0, 224, 0, 135, 0 };
                            //                          play.GetGamePackKeyEx().EncodePacket(ref data11, data11.Length);
                            //                          play.SendData(data11);
                            //                          byte[] data1 = {187,0,246,3,58,255,230,0,17,152,2,0,17,152,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,243,2,0,0,18,0,2,0,247,65,6,0,0,0,0,0,0,0,0

                            //,0,243,0,249,0,101,0,0,0,4,5,0,0,100,0,0,0,112,20,0,0,0,5,0,5,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,200,0,3,0,0,0,0,0,1,21,0,0,0,0,0,0,0,0,0,0,0,0,0,

                            //0,0,0,0,0,0,0,0,0,91,0,127,4,0,0,243,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,6,97,118,49,51,49,52,0,0,0};
                            // byte pos = Convert.ToByte(option[1]);
                            // byte value = Convert.ToByte(option[2]);
                            // byte[] data2 = {187,0,246,3,58,255,230,0,17,152,2,0,17,152,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,243,2,0,0,18,0,2,0,247,65,6,0,0,0,0,0,0,0,0,0,243,0,249,0,101,0,0,0,4,5,0,0,100,0,0,0,112,20};
                            // byte[] data3 = new byte[90];
                            // data3[pos] = value;
                            // byte[] data4 = { 1, 6, 97, 118, 49, 51, 49, 52, 0, 0, 0 };
                            // PacketOut outpack = new PacketOut(play.GetGamePackKeyEx());
                            // outpack.WriteBuff(data2);
                            // outpack.WriteBuff(data3);
                            // outpack.WriteBuff(data4);
                            //// play.GetGamePackKeyEx().EncodePacket(ref data1, data1.Length);
                            // play.SendData(outpack.Flush());

                            //byte[] data2 = { 28, 0, 244, 7, 109, 0, 5, 0, 58, 255, 230, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data2, data2.Length);
                            //play.SendData(data2);

                            //byte[] data3 = { 20, 0, 249, 3, 58, 255, 230, 0, 1, 0, 0, 0, 36, 0, 0, 0, 0, 4, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data3, data3.Length);
                            //play.SendData(data3);

                            ////军团信息
                            //byte[] data4 = { 20, 0, 247, 3, 243, 2, 0, 0, 3, 0, 1, 7, 65, 198, 172, 190, 252, 205, 197, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data4, data4.Length);
                            //play.SendData(data4);

                            //byte[] data5 = { 28, 0, 242, 3, 174, 95, 70, 0, 58, 255, 230, 0, 243, 0, 249, 0, 4, 0, 0, 0, 100, 0, 0, 0, 58, 37, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data5, data5.Length);
                            //play.SendData(data5);

                            break;
                        }

                    case "qicheng":
                        {
                            uint rid_id = Convert.ToUInt32(option[1]);
                            //byte[] data = { 36, 0, 244, 7, 209, 0, 7, 0 };
                            //byte[] data1 = { 226, 200, 184, 119, 45, 0, 0, 0, 1, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0 };
                            //PacketOut outpack = new PacketOut(play.GetGamePackKeyEx());
                            //outpack.WriteBuff(data);
                            //outpack.WriteUInt32(play.GetTypeId());
                            //outpack.WriteUInt32(rid_id);
                            //outpack.WriteBuff(data1);

                            //play.SendData(outpack.Flush());
                            play.TakeMount(0,rid_id);
                            break;
                        }
                    case "下马":
                        {
                            play.TakeOffMount(0);
                            break;
                        }
                    case CALLSCRIPT:
                        {
                            uint scripte_id = Convert.ToUInt32(option[1]);
                            ScripteManager.Instance().ExecuteAction(scripte_id, play);
                            break;
                        }
                    case "魔龙守护":
                        {

                            //收到网络协议:长度：40协议号:1022
                            //byte[] data2 = { 40, 0, 254, 3, 0, 0, 0, 0, 84, 66, 15, 0, 0, 0, 0, 0, 67, 2, 56, 1, 21, 0, 0, 0, 105, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data2, data2.Length);
                            //play.SendData(data2);
                            ////收到网络协议:长度：116协议号:1105
                            //byte[] data3 = { 116, 0, 81, 4, 84, 66, 15, 0, 67, 2, 56, 1, 105, 20, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 84, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 226, 200, 184, 119, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data3, data3.Length);
                            //play.SendData(data3);
                            ////收到网络协议:长度：20协议号:1017
                            byte[] data7 = { 20, 0, 249, 3, 84, 66, 15, 0, 1, 0, 0, 0, 99, 0, 0, 0, 1, 0, 0, 0 };

                            play.GetGamePackKeyEx().EncodePacket(ref data7, data7.Length);
                            play.SendData(data7);
                            ////收到网络协议:长度：48协议号:1127
                            //8, 7,0,0
                            byte[] data4 = { 48, 0, 103, 4, 84, 66, 15, 0, 8, 7, 0, 0, 200, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            play.GetGamePackKeyEx().EncodePacket(ref data4, data4.Length);
                            play.SendData(data4);
                            ////收到网络协议:长度：48协议号:1127
                            //byte[] data5 = { 48, 0, 103, 4, 226, 200, 184, 119, 8, 7, 0, 0, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data5, data5.Length);
                            //play.SendData(data5);
                            ////收到网络协议:长度：16协议号:1104
                            //byte[] data6 = { 16, 0, 80, 4, 84, 66, 15, 0, 114, 0, 0, 0, 105, 20, 1, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data6, data6.Length);
                            //play.SendData(data6);

                            break;
                        }
                    case TESTDIE: //测试死亡
                        {

                            //血量清零
                            //byte[] data1 = { 20, 0, 249, 3, 84, 66, 15, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data1, data1.Length);
                            //play.SendData(data1);
                            ////收到网络协议:长度：40协议号:1022
                            //                        byte[] data2 = { 40, 0, 254, 3, 0, 0, 0, 0, 200, 105, 7, 0, 84, 66, 15, 0, 47, 3, 17, 4, 2, 0, 0, 0, 233, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            //                                play.GetGamePackKeyEx().EncodePacket(ref data2, data2.Length);
                            //                        play.SendData(data2);
                            ////收到网络协议:长度：40协议号:1022
                            //参数
                            //time  0, 0, 0, 0
                            //怪物id 200, 105, 7, 0
                            //角色id 84, 66, 15, 0
                            //x 50, 3
                            //y 17, 4
                            //标记 14
                            byte[] data3 = { 40, 0, 254, 3, 0, 0, 0, 0, 200, 105, 7, 0, 84, 66, 15, 0, 50, 3, 17, 4, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            play.GetGamePackKeyEx().EncodePacket(ref data3, data3.Length);
                            play.SendData(data3);
                            //收到网络协议:长度：20协议号:1017 --
                            //                        byte[] data4 = { 20, 0, 249, 3, 84, 66, 15, 0, 1, 0, 0, 0, 28, 0, 0, 0, 0, 0, 0, 0 };
                            //         play.GetGamePackKeyEx().EncodePacket(ref data4, data4.Length);
                            //                        play.SendData(data4);
                            ////收到网络协议:长度：20协议号:1017
                            //                        byte[] data5 = { 20, 0, 249, 3, 84, 66, 15, 0, 1, 0, 0, 0, 26, 0, 0, 0, 2, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data5, data5.Length);
                            //play.SendData(data5);
                            //收到网络协议:长度：32协议号:1101

                            //byte[] data6 = { 32, 0, 77, 4, 248, 149, 1, 0, 204, 165, 16, 0, 50, 3, 17, 4, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data6, data6.Length);
                            //play.SendData(data6);
                            ////收到网络协议:长度：20协议号:1017
                            //byte[] data7 = { 20, 0, 249, 3, 84, 66, 15, 0, 1, 0, 0, 0, 4, 0, 0, 0, 137, 172, 15, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data7, data7.Length);
                            //play.SendData(data7);
                            ////收到网络协议:长度：67协议号:1004
                            //byte[] data8 = { 67, 0, 236, 3, 0, 0, 255, 0, 213, 7, 0, 0, 173, 8, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 4, 6, 83, 89, 83, 84, 69, 77, 8, 210, 176, 177, 200, 186, 243, 204, 236, 0, 21, 196, 227, 210, 197, 202, 167, 193, 203, 50, 50, 52, 50, 54, 195, 182, 189, 240, 177, 210, 161, 163, 0, 0, 0 };

                            //play.GetGamePackKeyEx().EncodePacket(ref data8, data8.Length);
                            //play.SendData(data8);
                            //                          //收到网络协议:长度：16协议号:1012
                            //byte[] data9 = { 16, 0, 244, 3, 84, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data9, data9.Length);
                            //play.SendData(data9);

                            // byte[] data2 = {  20, 0, 249, 3, 84, 66, 15, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0} ;
                            // play.GetGamePackKeyEx().EncodePacket(ref data2, data2.Length);
                            // play.SendData(data2);

                            // byte[] data1 = { 20, 0, 249, 3, 84, 66, 15, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            // play.GetGamePackKeyEx().EncodePacket(ref data1, data1.Length);
                            // play.SendData(data1);

                            // byte[] data3 = {40,0,254,3,0,0,0,0,24,87,7,0,76,152,15,0,228,3,214,1,2,0,0,0,233,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
                            // play.GetGamePackKeyEx().EncodePacket(ref data3, data3.Length);
                            // play.SendData(data3);

                            // byte[] data4 = {40,0,254,3,0,0,0,0,24,87,7,0,76,152,15,0,225,3,214,1,14,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
                            // play.GetGamePackKeyEx().EncodePacket(ref data4, data4.Length);
                            // play.SendData(data4);
                            // byte[] data5= {20,0,249,3, 66, 15, 0, 1,1,0,0,0,28,0,0,0,0,0,0,0};
                            // play.GetGamePackKeyEx().EncodePacket(ref data5, data5.Length);
                            // play.SendData(data5);
                            // byte[] data6 = { 20, 0, 249, 3, 66, 15, 0, 1, 1, 0, 0, 0, 26, 0, 0, 0, 2, 0, 0, 0 };
                            // play.GetGamePackKeyEx().EncodePacket(ref data6, data6.Length);
                            // play.SendData(data6);
                            // byte[] data7 = {32,0,77,4,48,101,1,0,204,165,16,0,225,3,214,1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0};
                            // play.GetGamePackKeyEx().EncodePacket(ref data7, data7.Length);
                            // play.SendData(data7);
                            //byte[] data8 = {16,0,244,3, 66, 15, 0, 1,0,0,0,0,0,0,0,0};
                            //play.GetGamePackKeyEx().EncodePacket(ref data8, data8.Length);
                            //play.SendData(data8);
                            byte[] data = { 28, 0, 249, 3, 84, 66, 15, 0, 2, 0, 0, 0, 26, 0, 0, 0, 6, 0, 0, 0, 12, 0, 0, 0, 33, 92, 108, 58 };
                            play.GetGamePackKeyEx().EncodePacket(ref data, data.Length);
                            play.SendData(data);
                            break;
                        }
                    case "引诱":
                        {

                            NetMsg.MsgMonsterMagicInjuredInfo injuredInfo = new NetMsg.MsgMonsterMagicInjuredInfo();
                            injuredInfo.tag = 21;
                            //   public int time;                //时间
                            //public uint roleid;             //角色id
                            //public uint monsterid;          //怪物id
                            //public short role_x;           //角色x
                            //public short role_y;           //角色y
                            //public uint tag;                 //标记
                            //public ushort magicid;          //技能id
                            //public ushort magiclv;      //技能等级
                            //public uint injuredvalue;       //攻击伤害值
                            //public int[] param;           //未知参数
                            // 收到网络协议:长度：20协议号:1017
                            //{20,0,249,3,76,152,15,0,1,0,0,0,9,0,0,0,97,0,0,0}
                            //收到网络协议:长度：40协议号:1022
                            byte[] data1 = { 40, 0, 254, 3, 0, 0, 0, 0, 84, 66, 15, 0, 84, 66, 15, 0, 63, 3, 7, 4, 21, 0, 0, 0, 235, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            play.GetGamePackKeyEx().EncodePacket(ref data1, data1.Length);
                            play.SendData(data1);
                            //收到网络协议:长度：88协议号:1105
                            byte[] data2 = { 88, 0, 81, 4, 84, 66, 15, 0, 84, 66, 15, 0, 235, 3, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 84, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            play.GetGamePackKeyEx().EncodePacket(ref data2, data2.Length);
                            play.SendData(data2);
                            //收到网络协议:长度：16协议号:1104
                            //                            byte[] data3 = { 16, 0, 80, 4, 84, 66, 15, 0, 83, 0, 0, 0, 235, 3, 1, 0 };
                            //                                           play.GetGamePackKeyEx().EncodePacket(ref data3, data3.Length);
                            //                            play.SendData(data3);
                            ////收到网络协议:长度：16协议号:1012
                            //                            byte[] data4 = { 16, 0, 244, 3, 84, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            //                                                       play.GetGamePackKeyEx().EncodePacket(ref data4, data4.Length);
                            //                            play.SendData(data4);
                            //收到网络协议:长度：20协议号:1017

                            //byte[] data5 = { 20, 0, 249, 3, 76, 152, 15, 0, 1, 0, 0, 0, 28, 0, 0, 0, 30, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data5, data5.Length);
                            //play.SendData(data5);
                            break;
                        }
                    case "骑士团守护":
                        {
                            //收到网络协议:长度：40协议号:1022
                            byte[] data1 = { 40, 0, 254, 3, 0, 0, 0, 0, 84, 66, 15, 0, 0, 0, 0, 0, 63, 2, 56, 1, 21, 0, 0, 0, 91, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            play.GetGamePackKeyEx().EncodePacket(ref data1, data1.Length);
                            play.SendData(data1);
                            //收到网络协议:长度：172协议号:1105
                            byte[] data10 = { 172, 0, 81, 4, 84, 66, 15, 0, 63, 2, 56, 1, 91, 20, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 90, 180, 11, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 91, 180, 11, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 92, 180, 11, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 93, 180, 11, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            play.GetGamePackKeyEx().EncodePacket(ref data10, data10.Length);
                            play.SendData(data10);


                           
                            //收到网络协议:长度：81协议号:2069
                            //byte[] data2 = { 81, 0, 21, 8, 90, 180, 11, 0, 84, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 201, 5, 0, 0, 63, 2, 63, 1, 0, 0, 125, 0, 43, 42, 0, 0, 80, 159, 4, 0, 80, 159, 4, 0, 1, 0, 100, 0, 1, 4, 210, 193, 183, 227, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data2, data2.Length);
                            //play.SendData(data2);
                            //收到网络协议:长度：28协议号:1010
                            //byte[] data3 = { 28, 0, 242, 3, 6, 140, 47, 86, 90, 180, 11, 0, 63, 2, 63, 1, 1, 0, 0, 0, 43, 42, 0, 0, 115, 37, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data3, data3.Length);
                            //play.SendData(data3);
                            //收到网络协议:长度：81协议号:2069
                            //byte[] data4 = { 81, 0, 21, 8, 91, 180, 11, 0, 84, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 201, 5, 0, 0, 55, 2, 51, 1, 0, 0, 125, 0, 43, 42, 0, 0, 80, 159, 4, 0, 80, 159, 4, 0, 2, 0, 100, 0, 1, 4, 210, 193, 183, 227, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data4, data4.Length);
                            //play.SendData(data4);
                            //收到网络协议:长度：28协议号:1010
                            //byte[] data5 = { 28, 0, 242, 3, 6, 140, 47, 86, 91, 180, 11, 0, 55, 2, 51, 1, 2, 0, 0, 0, 43, 42, 0, 0, 115, 37, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data5, data5.Length);
                            //play.SendData(data5);
                            //收到网络协议:长度：81协议号:2069
                            //byte[] data6 = { 81, 0, 21, 8, 92, 180, 11, 0, 84, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 201, 5, 0, 0, 67, 2, 51, 1, 0, 0, 125, 0, 43, 42, 0, 0, 80, 159, 4, 0, 80, 159, 4, 0, 5, 0, 100, 0, 1, 4, 210, 193, 183, 227, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data6, data6.Length);
                            //play.SendData(data6);
                            //收到网络协议:长度：28协议号:1010
                            //byte[] data7 = { 28, 0, 242, 3, 6, 140, 47, 86, 92, 180, 11, 0, 67, 2, 51, 1, 5, 0, 0, 0, 43, 42, 0, 0, 115, 37, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data7, data7.Length);
                            //play.SendData(data7);
                            //收到网络协议:长度：81协议号:2069
                            //byte[] data8 = { 81, 0, 21, 8, 93, 180, 11, 0, 84, 66, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 201, 5, 0, 0, 71, 2, 59, 1, 0, 0, 125, 0, 43, 42, 0, 0, 80, 159, 4, 0, 80, 159, 4, 0, 6, 0, 100, 0, 1, 4, 210, 193, 183, 227, 0, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data8, data8.Length);
                            //play.SendData(data8);
                            //收到网络协议:长度：28协议号:1010
                            //byte[] data9 = { 28, 0, 242, 3, 6, 140, 47, 86, 93, 180, 11, 0, 71, 2, 59, 1, 6, 0, 0, 0, 43, 42, 0, 0, 115, 37, 0, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data9, data9.Length);
                            //play.SendData(data9);
                         
                            //收到网络协议:长度：32协议号:1101 //地面持续特效
                            //176, 9, 13, 0 时间戳
                            // 176, 23, 0, 0 特效id
                            //63, 2 x坐标
                            //55,1 y坐标

                            byte[] data11 = { 32, 0, 77, 4, 176, 9, 13, 0, 176, 23, 0, 0, 63, 2, 55, 1, 0, 0, 0, 0, 10, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0 };
                            play.GetGamePackKeyEx().EncodePacket(ref data11, data11.Length);
                            play.SendData(data11);
                            ////收到网络协议:长度：16协议号:1104
                            //byte[] data12 = { 16, 0, 80, 4, 84, 66, 15, 0, 89, 0, 0, 0, 91, 20, 1, 0 };
                            //play.GetGamePackKeyEx().EncodePacket(ref data12, data12.Length);
                            //play.SendData(data12);
                            break;
                        }
                    case "清除特效":
                        {
                            byte[] data11 = { 32, 0, 77, 4, 176, 9, 13, 0, 176, 23, 0, 0, 63, 2, 55, 1, 0, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0 };
                            play.GetGamePackKeyEx().EncodePacket(ref data11, data11.Length);
                            play.SendData(data11);
                            break;
                        }
                    case "下雪":
                        {
 
                           // 收到网络协议:长度：28协议号:1010
//        byte[] data1 = {28,0,242,3,232,3,0,0,76,152,15,0,75,1,155,1,0,0,0,0,232,3,0,0,63,37,0,0};
//                                     play.GetGamePackKeyEx().EncodePacket(ref data1, data1.Length);
//                            play.SendData(data1);
////收到网络协议:长度：28协议号:1010
//                            byte[] data2 = { 28, 0, 242, 3, 52, 159, 49, 86, 76, 152, 15, 0, 75, 1, 155, 1, 0, 0, 0, 0, 255, 255, 255, 255, 95, 37, 0, 0 };
//                            play.GetGamePackKeyEx().EncodePacket(ref data2, data2.Length);
//                            play.SendData(data2);
//收到网络协议:长度：20协议号:1110
                            //地图id
                            //地图id
                            //类型
                           
                        byte[] data3={20,0,86,4,232,3,0,0,232,3,0,0,0,0,32,0,128,0,18,0};
                              play.GetGamePackKeyEx().EncodePacket(ref data3, data3.Length);
                            play.SendData(data3);
                            break;
                        }
                    case "元素掌控":
                        {
                     //       收到网络协议:长度：40协议号:1022
                            byte[] data1 = { 40, 0, 254, 3, 186, 192, 18, 1, 84, 66, 15, 0, 0, 0, 0, 0, 93, 1, 179, 1, 21, 0, 0, 0, 180, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                               play.GetGamePackKeyEx().EncodePacket(ref data1, data1.Length);
                            play.SendData(data1);
//收到网络协议:长度：20协议号:1017
                            byte[] data2 = { 20, 0, 249, 3, 84, 66, 15, 0, 1, 0, 0, 0, 101, 0, 0, 0, 0, 2, 0, 0 };
                            play.GetGamePackKeyEx().EncodePacket(ref data2, data2.Length);
                            play.SendData(data2);
//收到网络协议:长度：48协议号:1127
                            byte[] data3 = { 48, 0, 103, 4, 84, 66, 15, 0, 128, 81, 1, 0, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0 };
                            play.GetGamePackKeyEx().EncodePacket(ref data3, data3.Length);
                            play.SendData(data3);
                            //收到网络协议:长度：88协议号:1105
                            byte[] data4 = { 88, 0, 81, 4, 84, 66, 15, 0, 0, 0, 0, 0, 180, 20, 0, 0, 4, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
play.GetGamePackKeyEx().EncodePacket(ref data4, data4.Length);
play.SendData(data4);

byte[] data5 = { 20, 0, 249, 3, 84, 66, 15, 0, 1, 0, 0, 0, 107, 0, 0, 0, 3, 0, 0, 0 };
play.GetGamePackKeyEx().EncodePacket(ref data5, data5.Length);
play.SendData(data5);
                            break;
                        }
                    case WUDI:
                        {
                            if (play.GetTimerSystem().QueryStatus(GameStruct.RoleStatus.STATUS_WUDI) != null)
                            {
                                play.GetTimerSystem().DeleteStatus(GameStruct.RoleStatus.STATUS_WUDI);
                                play.LeftNotice("角色已取消无敌!!!");
                            }
                            else {
                                play.GetTimerSystem().AddStatus(GameStruct.RoleStatus.STATUS_WUDI);
                                play.LeftNotice("角色已无敌!!!");
                            }
                            break;
                        }
                    case "幻兽死亡":
                        {
                            EudemonObject obj = play.GetEudemonSystem().GetBattleEudemonSystem(0);
                            if (obj == null) break;
                            GameStruct.Action action = new GameStruct.Action(GameStruct.Action.DIE);
                            obj.PushAction(action);
                            break;
                        }
                    case "幻兽技能":
                        {
                            EudemonObject obj = play.GetEudemonSystem().GetBattleEudemonSystem(0);
                            if (obj == null) break;
                            ushort magicid = Convert.ToUInt16(option[1]);
                            obj.AddMagicInfo(magicid);
                            break;
                        }
                    case "幻兽等级":
                        {
                            EudemonObject obj = play.GetEudemonSystem().GetBattleEudemonSystem(0);
                            if (obj == null) break;
                            obj.GetEudemonInfo().level = 100;
                            play.GetEudemonSystem().SendEudemonInfo(obj.GetEudemonInfo());
                            break;
                        }
                    case "怪物外观":
                        {
                            uint lookface = Convert.ToUInt32(option[1]);
                            NetMsg.MsgMonsterInfo info = new NetMsg.MsgMonsterInfo();
                            info.id = 500000;
                            info.typeid = 3020;
                            info.lookface = lookface;
                            info.x = play.GetCurrentX();
                            info.y = play.GetCurrentY();
                            info.level = 125;
                            info.maxhp =10000;
                            info.hp = 10000;
                            info.dir = 7;
                            play.SendData(info.GetBuffer(), true);
                            break;
                        }
                    case "怪物名字":
                        {
                            uint typeid = Convert.ToUInt32(option[1]);
                            NetMsg.MsgMonsterInfo info = new NetMsg.MsgMonsterInfo();
                            info.id = 500000;
                            info.typeid = typeid;
                            info.lookface = 1243;
                            info.x = play.GetCurrentX();
                            info.y = play.GetCurrentY();
                            info.level = 125;
                            info.maxhp = 10000;
                            info.hp = 10000;
                            info.dir = 7;
                            play.SendData(info.GetBuffer(), true);
                            break;
                        }
                    case "创建npc":
                        {
                            uint id = Convert.ToUInt32(option[1]);
                            NetMsg.MsgNpcInfo info = new NetMsg.MsgNpcInfo();

                            info.Init(id, play.GetCurrentX(), play.GetCurrentY(), play.GetDir());
                            play.SendData(info.GetBuffer(),true);
                            break;
                        }
                    case GETONLINECOUNT:
                        {
                            play.ChatNotice("当前在线人数:" + UserEngine.Instance().GetOnlineCount().ToString());
                            break;
                        }
                    case "名人堂":
                        {
                            //248,42,0,0  用做NPC索引ID
                            //241,73,2,0  lookface
                            //241,73,2,0  lookface
                            //0, 0, 0, 0 未知
                            //60, 156, 29, 0 动作
                            //1, 0      名人堂排名名次
                            //132,16,2,0  衣服
                            //193, 182, 6, 0 学徒杖 武器
                            //205, 10, 0, 0 战斗力
                            // 178, 2, 0, 0 未知
                            // 33, 0, 0, 0  未知
                            //101, 0        X坐标
                            // 185, 0      Y坐标
                            //132, 0, 0, 0  发型
                            byte[] data = { 195, 0, 246, 3, 248, 42, 0, 0, 241, 73, 2, 0, 241, 73, 2, 0, 0, 0, 0, 0, /*60, 156, 29, 0*/0,0,0,0, 1, 0, 205, 10, 0, 0, 0, 0, 0, 
                                              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 132, 16, 2, 0, 193, 182, 6, 0,/* 178, 2, 0, 0, 33, 0, 0, 0*/0,0,0,0,0,0,0,0, 
                                              101, 0, 185, 0, 132, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /*33, 10,*/0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                                              0, 0, 0, /*100*/0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                              0, 0, 0, 0, 0, 0, 0, 0, 0, 0/*5*/, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                                              14, 185, 254, 176, 205, 185, 183, 176, 188, 161, 204, 205, 185, 194, 252, 0, 0, 0 };
                            play.SendData(data, true);
                            break;
                        }
                    case NOTICE:
                        {
                            String sMsg = option[1];
                            UserEngine.Instance().SceneNotice(sMsg);
                            break;
                        }
                    case "角色属性":
                        {
                            GameStruct.UserAttribute attr = (GameStruct.UserAttribute)Convert.ToInt32(option[1]);
                            int v = Convert.ToInt32(option[2]);
                            NetMsg.MsgUserAttribute msg = new NetMsg.MsgUserAttribute();
                            msg.role_id = play.GetTypeId();
                            msg.Create(null, null);

                            msg.AddAttribute(attr, (uint)v);
                            play.SendData(msg.GetBuffer(), true);
                            break;
                        }
           
  
                }
            }
            catch (System.Exception ex)
            {
                Log.Instance().WriteLog("----------------------------------------------------------------");
                Log.Instance().WriteLog("执行GM命令出错！！" + str);
                Log.Instance().WriteLog(ex.Message);
                Log.Instance().WriteLog(ex.StackTrace);
                Log.Instance().WriteLog("----------------------------------------------------------------");
            }

        }
    }
}
