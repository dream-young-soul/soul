using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameBase.Config;
using GameStruct;

//装备操作类- 用于装备提升等级、品质、与魔魂等级--
namespace MapServer
{
    public class EquipOperation
    {
        private const byte MAX_STRONGLEVEL = 12;//最高提升的魔魂等级

        private static EquipOperation mInstance = null;


        private List<GameStruct.EquipStrongInfo> mListStrong;
        public EquipOperation()
        {
            mListStrong = new List<GameStruct.EquipStrongInfo>();
        }


        public static EquipOperation Instance()
        {
            if (mInstance == null)
            {
                mInstance = new EquipOperation();
            }

            return mInstance;
        }

        public bool Load()
        {
            //载入装备强化信息

            VerPacket pack = ConfigManager.Instance().GetVerPacket();
            String text = pack.LoadFileToText(TextDefine.CONFIG_FILE_EQUIPSTRONG);
            CsvFile csv = new CsvFile(text);
            GameStruct.EquipStrongInfo info;
            String v;
            for (int i = 0; i < csv.GetLine(); i++)
            {
                info = new GameStruct.EquipStrongInfo();
                v = csv.GetFieldInfoToValue(i, "level");
                info.level = Convert.ToByte(v);
                v = csv.GetFieldInfoToValue(i, "chance");
                info.chance = Convert.ToInt32(v);
                mListStrong.Add(info);
            }


            return true;
        }
        //提升装备品质
        public void EquipQuality(PlayerObject play, uint srcid, uint materialid)
        {
            const int LINGHUNJINGSHI = 1037160; //灵魂晶石ID
            const int LINGHUNWANG = 1037169;    //灵魂王
            const int SHENQIZHILEI = 1037200; //神祈之泪
           // const int HUNWUSHENQIZHILEI = 1025754; //魂武神祈之泪
            const int MAX_QUALIY = 7;    //最高品质 神器
            GameStruct.RoleItemInfo item = play.GetItemSystem().FindItem(srcid);
            GameStruct.RoleItemInfo materialitem = play.GetItemSystem().FindItem(materialid);
            if (item == null || materialitem == null) return;
            if (item.GetQuality() == MAX_QUALIY) return; //已达到最高品质
            int rand = GameStruct.IRandom.Random(1, 100);
            bool bUpdate = false;
            if (!play.GetItemSystem().IsEquip(item.itemid)) return; //不是装备就不提升了

            NetMsg.MsgEquipOperationRet ret = new NetMsg.MsgEquipOperationRet();
            ret.Create(null, play.GetGamePackKeyEx());
            ret.srcid = srcid;
            ret.destid = materialid;
            ret.type = 196610;//{2,0,3,0}
            if (materialitem.itemid == LINGHUNJINGSHI ||
                materialitem.itemid == SHENQIZHILEI)
            {
                //检测是否有足够的灵魂晶石或者神祈之类
                if (!play.GetItemSystem().DeleteItemByID(materialid)) return;
                if (rand < this.RateSuccForQuality(item))
                {
                    item.UpQuality();
                    bUpdate = true;
                    ret.ret = 1;
                }
            }
            else if(materialitem.itemid == LINGHUNWANG &&
               item.GetQuality() <= 4/*极品武器以下可以用灵魂王升级*/) 
            {
                //检测是否有足够的灵魂王
                 if (!play.GetItemSystem().DeleteItemByID(materialid)) return;
                item.UpQuality();
                bUpdate = true;
                ret.ret = 1;

            }
            if (bUpdate)
            {
                //锻造者名称-
                if (item.forgename.Length == 0)
                {
                    item.forgename = play.GetName();
                }
                play.GetItemSystem().UpdateItemInfo(item.id);
            }
            play.SendData(ret.GetBuffer());
        }
        //装备提升魔魂等级参数:玩家对象 道具基本id 道具id 是否百分百成功
        public void EquipStrong(PlayerObject play, uint srcid, uint materialid)
        {


            const int MOHUNJINGSHI = 1037150;   //魔魂晶石id
            const int MOHUNZHIXIN = 1037159; //魔魂之心id
            GameStruct.RoleItemInfo item = play.GetItemSystem().FindItem(srcid);
            GameStruct.RoleItemInfo materialitem = play.GetItemSystem().FindItem(materialid);
            if (item == null || materialitem == null) return;
            if (item.GetStrongLevel() >= MAX_STRONGLEVEL) return;
            if (item.GetStrongLevel() >= mListStrong.Count) return;
            int rand = GameStruct.IRandom.Random(1, 100);
            bool bUpdate = false;
            NetMsg.MsgEquipOperationRet ret = new NetMsg.MsgEquipOperationRet();
            ret.Create(null, play.GetGamePackKeyEx());
            ret.srcid = srcid;
            ret.destid = materialid;
            ret.type = 196611;//{3,0,3,0}
            
            if (materialitem.itemid != MOHUNJINGSHI && item.GetStrongLevel() > 9) return; //大于9级后必须使用魔魂晶石提升等级
            if (materialitem.itemid == MOHUNJINGSHI)
            {
                //检测是否有足够的魔魂晶石
                if (!play.GetItemSystem().DeleteItemByID(materialid)) return;
                if (rand < mListStrong[item.GetStrongLevel()].chance)
                {
                    item.UpStrongLevel(1);
                    bUpdate = true;
                    ret.ret = 1;
                }
                else
                {
                    ret.ret = 0;
                    //强化等级9以下不往下掉了 2016.1.24
                    if (item.GetStrongLevel() > 9 && item.DecStrongLevel())
                        bUpdate = true;

                }
            }
            else if(materialitem.itemid == MOHUNZHIXIN) //魔魂之心必成功
            {
                if (!play.GetItemSystem().DeleteItemByID(materialid)) return;
                item.UpStrongLevel(1);
                bUpdate = true;
                ret.ret = 1;
            }

            if (bUpdate)
            {
               
                play.GetItemSystem().UpdateItemInfo(item.id);
            }

            play.SendData(ret.GetBuffer());

        }

        //幻魔晶石 装备升级
        public void EquipLevel(PlayerObject play,uint srcid,uint materialid)
        {
            const int HUANMOJINGSHI = 1037170; //幻魔晶石id
            const int HUANMOZHIXIN = 1037179; //幻魔之心

            const int MAX_LEVEL_EQUIP1 = 9; //其他装备的提升等级
            const int MAX_LEVEL_EQUIP2 = 25; //武器的提升等级

            GameStruct.RoleItemInfo item = play.GetItemSystem().FindItem(srcid);
            GameStruct.RoleItemInfo materialitem = play.GetItemSystem().FindItem(materialid);
            if (item == null || materialitem == null) return;
            bool bUpdate = false;
            NetMsg.MsgEquipOperationRet ret = new NetMsg.MsgEquipOperationRet();
            ret.Create(null, play.GetGamePackKeyEx());
            ret.srcid = srcid;
            ret.destid = materialid;
            ret.type = 196612;//{4,0,3,0}
            int l = RateSuccForEquipLevel(item);
            if (item.IsShield() || item.IsArmor() || item.IsHelmet())
            {
                if (item.GetLevel() > MAX_LEVEL_EQUIP1)
                    return ;
                if (materialitem.itemid == HUANMOZHIXIN)
                {
                    if (!play.GetItemSystem().DeleteItemByID(materialid)) return;
                    item.UpLevel();
                    bUpdate = true;
                    ret.ret = 1;
                }
                else if(materialitem.itemid == HUANMOJINGSHI)
                {
                    if (!play.GetItemSystem().DeleteItemByID(materialid)) return;
                    if (GameStruct.IRandom.Random(1, 100) < l)
                    {
                        item.UpLevel();
                        bUpdate = true;
                        ret.ret = 1;
                    }
                }
               
            }
            else
            {
                if (item.GetLevel() > MAX_LEVEL_EQUIP2)
                    return ;
                if (materialitem.itemid == HUANMOZHIXIN)
                {
                    if (!play.GetItemSystem().DeleteItemByID(materialid)) return;
                    item.UpLevel();
                    ret.ret = 1;
                    bUpdate = true;
                }
                else if(materialitem.itemid == HUANMOJINGSHI)
                {
                    if (!play.GetItemSystem().DeleteItemByID(materialid)) return;
                    if (GameStruct.IRandom.Random(1, 100) < l)
                    {
                        item.UpLevel();
                        ret.ret = 1;
                        bUpdate = true;
                    }
                }
               
   
            }

            if (bUpdate)
            {
                play.GetItemSystem().UpdateItemInfo(item.id);
            }
            play.SendData(ret.GetBuffer());
            return ;    
        }
        /*
        通过灵魂晶石来锻造提高武器的品质，每个等级锻造的成功率不一样。
        1--2，2--3，3--4，4--5的升级成功率都是100%。
        普通升良（0--1）：30%成功
        良升上（1--2）：12%成功
        上升精（2--3）：6%成功
        精升极（3--4）：4%成功

        */
        private int RateSuccForQuality(GameStruct.RoleItemInfo item)
        {
            int iQuality = item.GetQuality();
            if (iQuality == 0) return 30;
            else if (iQuality == 1) return 12;
            else if (iQuality == 2) return 6;
            else if (iQuality == 3) return 4;
            //神器
            else if (iQuality == 4) return 12;
            else if (iQuality == 5) return 6;
            else if (iQuality == 6) return 4;
            return -1;
        }
                //武器升级时的成功率
        /*
        武器类（包括武器，项链，手镯，戒指，鞋子等）：
        该类物品TYPE的百位和十位实际上就是武器的等级，等级为0--22，一共可升级22次。但如果升级时在itemtype表中找不到对应的数据，则表示不能升级了（例如：450205，如果要升级则升级为450215，但因为数据库中没有450215的数据，所以表示该武器不能再升级了。）
        升级成功率：
        当等级<4时，升级成功率为100%
        当等级<7时，升级成功率为：35%
        当等级<10时，升级成功率为：20%
        当等级<13时，升级成功率为：10%
        当等级<16时，升级成功率为：7%
        当等级<19时，升级成功率为：4%
        当等级>=19时，升级成功率为：2%

        装备类（包括衣服，帽子和盾牌）：
        该类物品TYPE的十位表示等级，等级为0--9，一共可升级9次。但如果升级时在itemtype表中找不到对应的数据，则表示不能升级了。
        当等级<2时，升级成功率100%
        当等级<4时，升级成功率为：35%
        当等级<6时，升级成功率为：20%
        当等级<7时，升级成功率为：10%
        当等级<8时，升级成功率为：7%
        当等级<9时，升级成功率为：4%
        */
        private int RateSuccForEquipLevel(GameStruct.RoleItemInfo pEquipItem)
        {

            int nLevel = pEquipItem.GetLevel();
            if (pEquipItem.IsShield() || pEquipItem.IsArmor() || pEquipItem.IsHelmet())
            {
                if (nLevel >= 0 && nLevel < 2) return 100;
                else if (nLevel >= 2 && nLevel < 4) return 35;
                else if (nLevel >= 4 && nLevel < 6) return 20;
                else if (nLevel >= 6 && nLevel < 7) return 10;
                else if (nLevel >= 7 && nLevel < 8) return 7;
                else if (nLevel >= 8 && nLevel < 9) return 4;
            }
            else
            {
                if (nLevel >= 0 && nLevel < 4) return 100;
                else if (nLevel >= 4 && nLevel < 7) return 35;
                else if (nLevel >= 7 && nLevel < 10) return 20;
                else if (nLevel >= 10 && nLevel < 13) return 10;
                else if (nLevel >= 13 && nLevel < 16) return 7;
                else if (nLevel >= 16 && nLevel < 19) return 4;
                else if (nLevel >= 19 && nLevel < 22) return 2;
            }
            return 0;
        }


        //装备打洞 参数:玩家对象,要打洞的道具id,打的洞序号[0为第一个洞]
        public void OpenGem(PlayerObject play, uint srcid,uint destid )
        {
            const int YUEGUANGBAOHE = 723002; //月光宝盒id
            const int YUEGUANGBAOHEZENGQIANGBAN = 820300; //月光宝盒增强版 id
            const int SHENSHENGYUEGUANGBAOHE = 742178; //神圣月光宝盒
            GameStruct.RoleItemInfo item = play.GetItemSystem().FindItem(destid);
            GameStruct.RoleItemInfo srcitem = play.GetItemSystem().FindItem(srcid);
            if (item == null || srcitem == null) return;
            byte index = 0;
            switch (srcitem.itemid)
            {
                case YUEGUANGBAOHE:
                    {
                        if (item.GetGemCount() != 0) return;
                        index = 0;
                        break;
                    }
                case YUEGUANGBAOHEZENGQIANGBAN:
                    {
                     
                        if (item.GetGemCount() != 1) return;
                        index = 1;
                        break;
                    }
                    //第三个洞 2015.11.21 道具协议有bug。 先不开
                    //11.21 开了
                case SHENSHENGYUEGUANGBAOHE:
                    {
                        if (item.GetGemCount() != 2) return;
                        index = 2;
                        break;
                       
                    }
                //第三个洞其他协议打-- 这个不打。。-。- 2015.9.21
                default:
                    {
                        return;
                    }
            }
            play.GetItemSystem().DeleteItemByID(srcid);
            item.OpenGem(index);
            play.GetItemSystem().UpdateItemInfo(item.id);

        }

      
        //宝石镶嵌
        public void GemSet(PlayerObject play, uint srcid, uint destid, byte index)
        {
            GameStruct.RoleItemInfo item = play.GetItemSystem().FindItem(destid);
            GameStruct.RoleItemInfo srcitem = play.GetItemSystem().FindItem(srcid);
            if (item == null || srcitem == null)
            {
                return;
            }
            if (!srcitem.IsGem()) return;
            if (item.GetGemCount() < index) return;
            if (item.GetGemType(index) != 255) return; //已有宝石-- 要先拆除再镶嵌

            play.GetItemSystem().DeleteItemByID(srcid);
            item.SetGemType(index, srcitem.GetGemType());

            play.GetItemSystem().UpdateItemInfo(destid);
        }
        //宝石融合
        public void GemFusion(PlayerObject play, uint destid)
        {
            //查询所需材料id
            GameStruct.ItemTypeInfo destitem = ConfigManager.Instance().GetItemTypeInfo(destid);
            GameStruct.ItemTypeInfo srcitem = ConfigManager.Instance().GetItemTypeInfo(destid - 10);
            if (destitem == null || srcitem == null)
            {
                return;
            }
            GameStruct.GemInfo geminfo = ConfigManager.Instance().GetGemInfo(destitem.id);
            if (geminfo == null) return;

            if (!play.GetItemSystem().DeleteItemByItemID(srcitem.id, geminfo.amount))
            {
                play.MsgBox("合成失败,数量不足");
                return;
            }
            //给予道具
            play.GetItemSystem().AwardItem(destitem.id,NetMsg.MsgItemInfo.ITEMPOSITION_BACKPACK);
            play.MsgBox("合成宝石成功!");
        }

        /*穿戴的装备是否符合穿戴条件
        level 等级
        profession 职业
        equip_pos 穿戴位置
        item_info 道具信息*/
        //2016.1.20
        //20多级以下的装备没有做判断- 暂时不写 2016.1.20
        public bool IsAccordWithEquip(byte level,byte profession, byte equip_pos, RoleItemInfo item_info)
        {
            uint nStartItemId = 0; //起始道具ID
            int nAddIndex = 0;  //索引总数- 就是该部位的装备所有数量
            switch (equip_pos)
            {
                case NetMsg.MsgItemInfo.ITEMPOSITION_HELMET: //头盔
                    {
                        switch (profession)
                        {
                            case JOB.WARRIOR: { nStartItemId = 111000; break; }
                            case JOB.MAGE: { nStartItemId = 115000; break; }
                            case JOB.POWER: { nStartItemId = 113000; break; }
                            case JOB.BLOODCLAN: { nStartItemId = 117000; break; }
                            case JOB.UNDEAD_MAGE: { nStartItemId = 119000; break; }
                            case JOB.DRAGONRIDE: { nStartItemId = 112000; break; }

                        }
                        nAddIndex = 14;
                        break;
                    }
                case NetMsg.MsgItemInfo.ITEMPOSITION_NECKLACE: //项链
                    {
                        switch (profession)
                        {
                            case JOB.WARRIOR: { nStartItemId = 121000; break; }
                            case JOB.MAGE: { nStartItemId = 125000; break; }
                            case JOB.POWER: { nStartItemId = 123000; break; }
                            case JOB.BLOODCLAN: { nStartItemId = 127000; break; }
                            case JOB.UNDEAD_MAGE: { nStartItemId = 129000; break; }
                            case JOB.DRAGONRIDE: { nStartItemId = 122000; break; }

                        }
                        nAddIndex = 14;
                        break;
                    }
                case NetMsg.MsgItemInfo.ITEMPOSITION_ARMOR: //盔甲
                    {
                        switch (profession)
                        {
                            case JOB.WARRIOR: { nStartItemId = 131000; break; }
                            case JOB.MAGE: { nStartItemId = 135000; break; }
                            case JOB.POWER: { nStartItemId = 133000; break; }
                            case JOB.BLOODCLAN: { nStartItemId = 137000; break; }
                            case JOB.UNDEAD_MAGE: { nStartItemId = 139000; break; }
                            case JOB.DRAGONRIDE: { nStartItemId = 132000; break; }

                        }
                        nAddIndex = 14;
                        break;
                    }
                case NetMsg.MsgItemInfo.ITEMPOSITION_WEAPONR: //武器
                    {
                        switch (profession)
                        {
                            case JOB.WARRIOR: {
                                if (item_info.itemid >= 420000) nStartItemId = 420000; //剑
                                else nStartItemId = 410000; //刀
                                break;}
                            case JOB.MAGE: { nStartItemId = 440000; break; }
                            case JOB.POWER: { nStartItemId = 490000; break; }
                            case JOB.BLOODCLAN: { nStartItemId = 450000; break; }
                            case JOB.UNDEAD_MAGE: { nStartItemId = 430000; break; }
                            case JOB.DRAGONRIDE: { nStartItemId = 480000; break; }

                        }
                        nAddIndex = 27;
                        break;
                    }

              case NetMsg.MsgItemInfo.ITEMPOSITION_TREASURE: //手镯
                    {
                        switch (profession)
                        {
                            case JOB.WARRIOR: { nStartItemId = 141000; break; }
                            case JOB.MAGE: { nStartItemId = 145000; break; }
                            case JOB.POWER: { nStartItemId = 143000; break; }
                            case JOB.BLOODCLAN: { nStartItemId = 147000; break; }
                            case JOB.UNDEAD_MAGE: { nStartItemId = 149010; break; }
                            case JOB.DRAGONRIDE: { nStartItemId = 142000; break; }

                        }
                        nAddIndex = 14;
                        break;
                    }
             case NetMsg.MsgItemInfo.ITEMPOSITION_SHOES: //鞋子
                    {
                        switch (profession)
                        {
                            case JOB.WARRIOR: { nStartItemId = 161000; break; }
                            case JOB.MAGE: { nStartItemId = 165000; break; }
                            case JOB.POWER: { nStartItemId = 163000; break; }
                            case JOB.BLOODCLAN: { nStartItemId = 167000; break; }
                            case JOB.UNDEAD_MAGE: { nStartItemId = 169000; break; }
                            case JOB.DRAGONRIDE: { nStartItemId = 162000; break; }

                        }
                        nAddIndex = 10;
                        break;
                    }

            }
            GameStruct.ItemTypeInfo type_info = null;
            for (int i = 0; i < nAddIndex; i++)
            {
                type_info = ConfigManager.Instance().GetItemTypeInfo(nStartItemId);
           
                if (type_info != null && type_info.req_level >= level)
                {
                    if (type_info.req_level == level)
                    {
                        break;
                    }
                    nStartItemId -= 10;
                    type_info =  ConfigManager.Instance().GetItemTypeInfo(nStartItemId);
                    break;
                }
                nStartItemId += 10;
            }
       
            if (type_info == null) return false;
            //符合穿戴需求
            GameStruct.ItemTypeInfo  _item_info = ConfigManager.Instance().GetItemTypeInfo(item_info.itemid);
            if(_item_info == null)return false;
            //道具名称一样- 
            if (String.Compare(type_info.name, _item_info.name) == 0)
            {
                return true;
            }
       
            return false;
        }

        //宝石替换
        public void GemReplace(PlayerObject play, byte[] data)
        {
            GameBase.Network.PackIn inpack = new GameBase.Network.PackIn(data);
            inpack.ReadInt16();
            inpack.ReadUInt32();
            uint itemid = inpack.ReadUInt32();
            RoleItemInfo src_item = play.GetItemSystem().FindItem(itemid);
            if (src_item == null)
            {
                play.MsgBox("替换失败,装备不存在。");
                return;
            }
            int gem1_type = inpack.ReadInt32();
            int gem2_type = inpack.ReadInt32();
            int gem3_type = inpack.ReadInt32();
            uint gem1_replace_id = inpack.ReadUInt32();
            uint gem2_replace_id = inpack.ReadUInt32();
            uint gem3_replace_id = inpack.ReadUInt32();

            RoleItemInfo gem1_item_info = play.GetItemSystem().FindItem(gem1_replace_id);
          
            RoleItemInfo gem2_item_info = play.GetItemSystem().FindItem(gem2_replace_id);
           
            RoleItemInfo gem3_item_info = play.GetItemSystem().FindItem(gem3_replace_id);
         
            //第一个洞
            if (gem1_item_info != null && gem1_item_info.IsGem())
            {
                if (src_item.GetGemCount() > 0)
                {
                    src_item.SetGemType(0, gem1_item_info.GetGemType());
                    play.GetItemSystem().DeleteItemByID(gem1_item_info.id);
                }
            }
            //第二个洞
            if (gem2_item_info != null && gem2_item_info.IsGem())
            {
                if (src_item.GetGemCount() > 1)
                {
                    src_item.SetGemType(1, gem2_item_info.GetGemType());
                    play.GetItemSystem().DeleteItemByID(gem2_item_info.id);
                }
            }
            //第三个洞
            if (gem3_item_info != null && gem3_item_info.IsGem())
            {
                if (src_item.GetGemCount() > 2)
                {
                    src_item.SetGemType(2, gem3_item_info.GetGemType());
                    play.GetItemSystem().DeleteItemByID(gem3_item_info.id);
               }
            }
            play.GetItemSystem().SendItemInfo(src_item);
            play.MsgBox("宝石替换成功");
           
           // src_item.SetGemType()
        }

        //法宝追加
        public void Magic_Add_God(PlayerObject play, uint srcid, uint destid)
        {
            RoleItemInfo srcinfo = play.GetItemSystem().FindItem(srcid);
            if (srcinfo == null) return;
            RoleItemInfo destinfo = play.GetItemSystem().FindItem(destid);
            if (destinfo == null) return;
            if (srcinfo.stronglv >= 12)
            {
                play.MsgBox("已达到最高法宝等级");
                return;
            }
            int godlv = srcinfo.god_exp / 10000;
            int addgodexp = 0;
            switch (destinfo.itemid)
            {
                case 1037231: //+1创世水晶
                    {
                        addgodexp = 20;
                        break;
                    }
                case 1037232: //+2创世水晶
                    {
                        addgodexp = 60;
                        break;
                    }
                case 1037233: //+3创世水晶
                    {
                        addgodexp = 180;
                        break;
                    }
            }
 
            //这四行代码只是为了弥补之前的BUG
            if (srcinfo.itemid == GameBase.Config.Define.ITEM_DILONGZHILEI_ID && srcinfo.god_strong >= 22500)
            {
                srcinfo.god_strong = 0;
                srcinfo.stronglv++;
            }
            //---------------------------------------------------------
            if (addgodexp == 0 && srcinfo.itemid != destinfo.itemid) return;
            if (addgodexp == 0) addgodexp = 20; //默认相同法宝+20经验
            addgodexp = 1000;
            if (srcinfo.itemid == GameBase.Config.Define.ITEM_SHUGUANGZHANHUN_ID && srcinfo.god_strong >= 7500) { return ;} //已经到最高经验值
            if (srcinfo.itemid == GameBase.Config.Define.ITEM_DILONGZHILEI_ID && srcinfo.god_strong >= 22500) { return; } 
            if(srcinfo.itemid == GameBase.Config.Define.ITEM_SHENGYAOFUWEN_ID && srcinfo.god_strong >= 30000){return;}
            srcinfo.god_strong += addgodexp;
            if (srcinfo.itemid == GameBase.Config.Define.ITEM_SHUGUANGZHANHUN_ID && srcinfo.god_strong >= 7500)
            {
                srcinfo.god_strong = 0;
                srcinfo.stronglv++;
            }
            if (srcinfo.itemid == GameBase.Config.Define.ITEM_DILONGZHILEI_ID && srcinfo.god_strong >= 22500)
            {
                srcinfo.god_strong = 0;
                srcinfo.stronglv++;
            }
            if (srcinfo.itemid == GameBase.Config.Define.ITEM_SHENGYAOFUWEN_ID && srcinfo.god_strong >= 30000)
            {
                srcinfo.god_strong = 0;
                srcinfo.stronglv++;
            }
        
            play.GetItemSystem().DeleteItemByID(destinfo.id);

          
            play.GetItemSystem().SendItemInfo(srcinfo);

            NetMsg.MsgEquipOperationRet ret = new NetMsg.MsgEquipOperationRet();
          

            ret.srcid = srcid;
            ret.destid = destid;

            byte[] ret_code = { 9, 0, 3, 0 }; ;
            ret.type = BitConverter.ToUInt32(ret_code, 0);
            ret.ret = 1;
            play.SendData(ret.GetBuffer(), true);
        }
          //提升神佑
        public void Equip_GodExp(PlayerObject play, uint srcid, uint destid)
        {
          
            RoleItemInfo srcinfo = play.GetItemSystem().FindItem(srcid);
            if (srcinfo == null) return;
            RoleItemInfo destinfo = play.GetItemSystem().FindItem(destid);
            if (destinfo == null) return;
            if (srcinfo.god_exp >=  90000)
            {
                play.MsgBox("已达到最高神佑等级");
                return;
            }
            
            int godlv = srcinfo.god_exp / 10000;
            int addgodexp = 0;
            switch (godlv)
            {
                case 0: { addgodexp = 1000; break; } //提升1级神佑
                case 1: { addgodexp = 500; break; } //二级
                case 2: { addgodexp = 200; break; } //三级
                case 3: { addgodexp = 125; break; }//四级
                case 4: { addgodexp = 83; break; }//五级
                case 5: { addgodexp = 55;break;} //六级
                case 6: {addgodexp = 40;break;}//七级
                case 7: { addgodexp = 28; break; }//八级
                case 8: { addgodexp = 20; break; } //九级
                default: { return; }

            }
            int rate = 1;//倍率
            switch(destinfo.itemid)
            {
                case 1037210://神谕之石
                    {
                        rate = 1;
                        break;
                    }
                case 1037260://5倍神谕之石
                    {
                        if (godlv < 3) return; //最低使用等级是3级
                        rate = 5;
                        break;
                    }
                case 1037261: //10倍神谕之石
                    {
                        if (godlv < 5) return; //最低使用等级是5级
                        rate = 10;
                        break;
                    }
                case 1037262:       //25倍神谕之石
                    {
                        if (godlv < 8) return; //最低使用等级8级
                        rate = 25;

                        break;
                    }
            }
            srcinfo.god_exp += addgodexp * rate;
            play.GetItemSystem().DeleteItemByID(destinfo.id);
          //  play.MsgBox("增加神佑经验"+(addgodexp * rate).ToString());
          
            play.GetItemSystem().SendItemInfo(srcinfo);

            NetMsg.MsgEquipOperationRet ret = new NetMsg.MsgEquipOperationRet();
         
            ret.srcid = srcid;
            ret.destid = destid;
            byte[] index = { 7, 0, 3, 0 };

            ret.type = BitConverter.ToUInt32(index, 0);
            ret.ret = 1;
            play.SendData(ret.GetBuffer(), true);
        }
    }
   
}
