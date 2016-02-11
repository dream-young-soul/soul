using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Network.Internal;
using GameBase.Config;
using GameBase.Core;
//爵位实例
//2015.10.5
namespace MapServer
{
    public class GuanJueManager
    {
        private static GuanJueManager mInstance = null;
        private const int MAX_JUEWEI = 50;//最多五十个爵位
        private List<GameBase.Network.Internal.GuanJueInfo> mList;
        public static GuanJueManager Instance()
        {
            if(mInstance == null)
            {
                mInstance = new GuanJueManager();
            }
            return mInstance;
        }

        public GuanJueManager()
        {
            mList = new List<GameBase.Network.Internal.GuanJueInfo>();
        }
        public void DB_Load(GameBase.Network.Internal.GUANJUEINFO info)
        {
            mList.Clear();
            for (int i = 0; i < info.list_item.Count; i++)
            {
                mList.Add(info.list_item[i]);
            }
            Log.Instance().WriteLog("从DBserver加载爵位数据成功!");
        }

        public void DB_Update(PlayerObject play)
        {
            //发给dbserver 更新爵位信息
            GameBase.Network.Internal.UPDATEGUANJUEDATA updatedb = new GameBase.Network.Internal.UPDATEGUANJUEDATA();
            updatedb.info.id = (uint)play.GetBaseAttr().player_id;
            updatedb.info.name = play.GetName();
            updatedb.info.guanjue = play.GetBaseAttr().guanjue;
            DBServer.Instance().GetDBClient().SendData(updatedb.GetBuffer());
        }
        //客户端请求获取爵位数据- 
        //参数 page=页码 下标从0开始
        public void RequestData(PlayerObject play,byte page)
        {
            int start = page * 10 ;
            if (start < 0) start = 0;
            if (start >= mList.Count) return;//超出了
            NetMsg.MsgGuanJueInfo info = new NetMsg.MsgGuanJueInfo();
            info.Create(null,play.GetGamePackKeyEx());
            for (int i = start; i < start + 10; i++)
            {
                if (i >= mList.Count) break;
                NetMsg.MsgGuanJueItem item = new NetMsg.MsgGuanJueItem();
                item.guanjue = mList[i].guanjue;
                item.name = mList[i].name;
                item.pos = i;
                info.list_item.Add(item);
               
             
            }
            info.page = page;
            play.SendData(info.GetBuffer());
        }
        //玩家捐献爵位
        public void Donation(PlayerObject play, GameStruct.MONEYTYPE type, int value)
        {
            const int MIN_GOLD = 3000000; //最低捐献金额- 防封包
            GameStruct.GUANGJUELEVEL oldlv = play.GetGuanJue();
            int gold = 0;
            switch (type )
            {
                case GameStruct.MONEYTYPE.GOLD:
                    {
                        if (gold < MIN_GOLD)
                        {
                            play.LeftNotice("最低捐献" + MIN_GOLD.ToString() + "万金币起。");
                            return;
                        }
                        if (play.GetMoneyCount(GameStruct.MONEYTYPE.GOLD) < value)
                        {
                            play.LeftNotice("金币不足,无法捐献！");
                            return;
                        }
                        gold = value;
                        play.ChangeAttribute(GameStruct.UserAttribute.GOLD, -gold);
                        break;
                    }
                case GameStruct.MONEYTYPE.GAMEGOLD:
                    {
                        if (play.GetMoneyCount(GameStruct.MONEYTYPE.GAMEGOLD) < value)
                        {
                            play.LeftNotice("魔石不足,无法捐献！");
                            return;
                        }
                        play.ChangeAttribute(GameStruct.UserAttribute.GAMEGOLD, -value);
                        //转换成金币 一个魔石等于7100金币
                        const int _gold = 7100;
                        gold = value * _gold;
                        if (gold < MIN_GOLD)
                        {
                            play.LeftNotice("最低捐献"+MIN_GOLD.ToString()+"万金币起。");
                            return;
                        }
                        break;
                    }
            }

            play.GetBaseAttr().guanjue += (uint)gold;

            SetValue(play.GetBaseAttr().player_id,play.GetName() ,play.GetBaseAttr().guanjue);
            //通知客户端

            //重新计算一下等级
            GameStruct.GUANGJUELEVEL level = this.GetLevel(play);
            //爵位被改变- 发公告
            if (oldlv != level)
            {
                this.SendChangeGuanJueMsg(play, level);
            }
            if (level != play.GetGuanJue())
            {
                play.SetGuanJue(level);
              
            }
            this.SendGuanJueInfo(play);

            DB_Update(play);
        }


        public void SetValue(int play_id,String name, ulong guanjue)
        {
            
           // bool bFind = false;
            for (int i = 0; i < mList.Count; i++)
            {
                if (mList[i].id == play_id)
                {
                    //mList[i].guanjue = guanjue;
                    mList.RemoveAt(i);
                    //bFind = true;
                    break;
                }
            }
           
            GuanJueInfo info = new GuanJueInfo();
            info.id = (uint)play_id;
            info.name = name;
            info.guanjue = guanjue;
            //插入--
            bool bInsert = false;
            for (int i = 0; i < mList.Count; i++)
            {
                if (guanjue > mList[i].guanjue)
                {
                    mList.Insert(i, info);
                    bInsert = true;
                    break;
               }
            }
            if (!bInsert && mList.Count < GameBase.Config.Define.MAX_JUEWEICOUNT)
            {
                mList.Add(info);
             }
        }
        //取当前玩家爵位等级
        public GameStruct.GUANGJUELEVEL GetLevel(PlayerObject play)
        {
            GameStruct.GUANGJUELEVEL level = GameStruct.GUANGJUELEVEL.NORMAL;
            int pos = -1;
            for (int i = 0; i < mList.Count; i++)
            {
                if (mList[i].id == (uint)play.GetBaseAttr().player_id)
                {
                    pos = i;
                    break;
                }
            }
            if (pos != -1)
            {
                //1-3名- 王- 女王
                if (pos >= 0 && pos <= 2)
                {
                    level = play.GetSex() == Sex.MAN ? GameStruct.GUANGJUELEVEL.KING : GameStruct.GUANGJUELEVEL.QUEEN;

                }
                //公爵 4-15名
                else if (pos >= 3 && pos <= 14)
                {
                    level = GameStruct.GUANGJUELEVEL.DUKE;
                }
                //侯爵 16-50名
                else if (pos >= 15 && pos <= 49)
                {
                    level = GameStruct.GUANGJUELEVEL.MARQUIS;
                }
                return level;

            }
            ulong guanjue = play.GetBaseAttr().guanjue;
            //伯爵 大于2亿
            if (guanjue >= 200000000)
            {
                level = GameStruct.GUANGJUELEVEL.EARL;
            }
                //子爵 大于1亿
            else if (guanjue >= 100000000)
            {
                level = GameStruct.GUANGJUELEVEL.VISCOUNT;

            } //勋爵 大于30000000
            else if (guanjue >= 30000000)
            {
                level = GameStruct.GUANGJUELEVEL.LORD;
            }
             return level;
        }

        public void SendGuanJueInfo(PlayerObject play)
        {
          //  byte[] data1 = { 25, 0, 247, 3, 0, 0, 0, 0, 113, 0, 1, 12, 49, 32, 45, 49, 32, 51, 48, 48, 48, 48, 48, 48, 0 };
            //捐献的
            ulong guanjue = play.GetBaseAttr().guanjue;
            byte[] byte_ = Coding.GetDefauleCoding().GetBytes(guanjue.ToString());
            GameBase.Network.PacketOut outpack = new GameBase.Network.PacketOut(play.GetGamePackKeyEx());
            outpack.WriteUInt16((ushort)(byte_.Length + 4 + 14));
            outpack.WriteUInt16(1015);
            outpack.WriteUInt32(0);
            outpack.WriteUInt16(113);
            outpack.WriteByte(1);
           
            //长度--
            outpack.WriteByte((byte)(byte_.Length + 5));
            String sjuewei = ((byte)play.GetGuanJue()).ToString();
            byte[] jueweibyte_ = Coding.GetDefauleCoding().GetBytes(sjuewei);
            outpack.WriteByte(jueweibyte_[0]); //爵位
            outpack.WriteByte(32); //分隔符
            outpack.WriteByte(45);
            outpack.WriteByte(49);
            outpack.WriteByte(32); //分隔符
            outpack.WriteBuff(byte_);
            outpack.WriteByte(0);
         
          
            play.SendData(outpack.Flush());
        
        }

        //发送爵位被改变的文字消息，全服广播
        public void SendChangeGuanJueMsg(PlayerObject play, GameStruct.GUANGJUELEVEL lv)
        {
            String str = "";
            switch (lv)
            {
                case GameStruct.GUANGJUELEVEL.KING:
                    {
                        str = string.Format("这一天将载入永恒的史册，玩家[{0}]赢得了至高无上的光荣与祝福，登上了国王的宝座！", play.GetName());
                        UserEngine.Instance().BroadcastMsg(BROADCASTMSGTYPE.SCREEN, str);
                        break;
                    }
                case GameStruct.GUANGJUELEVEL.QUEEN:
                    {
                        str = string.Format("这是万众曙目的时刻！[{0}]戴上了神圣的王冠，让我们为新的卡诺萨城女王欢呼喝彩吧！", play.GetName());
                        UserEngine.Instance().BroadcastMsg(BROADCASTMSGTYPE.SCREEN, str);
                        break;
                    }
                case GameStruct.GUANGJUELEVEL.DUKE:
                    {

                        str = string.Format("卡诺萨城的钟声轰然响起，[{0}]为王国做出来重大贡献，欶封为皇家公爵！", play.GetName());
                        UserEngine.Instance().BroadcastMsg(BROADCASTMSGTYPE.SCREEN, str);
                        break;
                    }
                case GameStruct.GUANGJUELEVEL.MARQUIS:
                    {
                        str = string.Format("光荣的号角响起，[{0}]受封为皇家候爵，愿他的荣耀之旗永闪光芒！", play.GetName());
                        UserEngine.Instance().BroadcastMsg(BROADCASTMSGTYPE.SCREEN, str);
                        break;
                    }
                case  GameStruct.GUANGJUELEVEL.EARL:
                    {
                        str = string.Format("恭祝玩家[{0}]受封为伯爵，庆祝的歌声将响彻全城，他的名字将与卡诺萨城同在", play.GetName());
                        UserEngine.Instance().BroadcastMsg(BROADCASTMSGTYPE.CHAT, str);
                        break;
                    }
                case  GameStruct.GUANGJUELEVEL.VISCOUNT:
                    {
                        str = string.Format("恭祝玩家[{0}]受封为子爵，在神圣的光芒下，让我们共同见证这份光荣！", play.GetName());
                        UserEngine.Instance().BroadcastMsg(BROADCASTMSGTYPE.CHAT, str);
                        break;
                    }
                case GameStruct.GUANGJUELEVEL.LORD:
                    {
                        str = string.Format("祝贺玩家[{0}]晋升为勋爵，卡诺萨城有多了一位尊贵的守护者。", play.GetName());
                        UserEngine.Instance().BroadcastMsg(BROADCASTMSGTYPE.CHAT, str);
                        break;
                    }
            }
            
        }
 
    }
}
