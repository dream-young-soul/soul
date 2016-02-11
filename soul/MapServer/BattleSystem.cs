using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStruct;
using GameBase.Config;

//战斗系统
//用于计算战斗伤害，击杀怪物获得经验等等
//2015.9.3
namespace MapServer
{
    public class BattleSystem
    {
        public const byte EXPLODE_ITEM_CHANCE1 = 0;//不爆
        public const byte EXPLODE_ITEM_CHANCE2 = 1;//1.小爆
        public const byte EXPLODE_ITEM_CHANCE3 = 2;//中爆
        public const byte EXPLODE_ITEM_CHANCE4 = 3;//大爆


        //计算经验--
        public static int AdjustExp(int nDamage, int nAtkLev, int nDefLev)
        {
            if (nAtkLev > 135) nAtkLev = 125;
            int nExp = nDamage;

	        int nNameType = MonsterNameType.GetNameType(nAtkLev, nDefLev);
	        int nDeltaLev = nAtkLev - nDefLev;
            if (nNameType == MonsterNameType.NAME_GREEN)
	        {
		        if (nDeltaLev >= 3 && nDeltaLev <= 5)
			        nExp = nExp * 70 / 100;
		        else if (nDeltaLev > 5 && nDeltaLev <= 10)
			        nExp = nExp * 20 / 100;
		        else if (nDeltaLev > 10 && nDeltaLev <= 20)
			        nExp = nExp * 10 / 100;
		        else if (nDeltaLev > 20)
			        nExp = nExp * 5 / 100;
	        }
            else if (nNameType == MonsterNameType.NAME_RED)
	        {
                nExp = (int)(nExp * 1.3);
	        }
            else if (nNameType == MonsterNameType.NAME_BLACK)
	        {
		        if (nDeltaLev >= -10 && nDeltaLev < -5)
                    nExp = (int)(nExp * 5);
		        else if (nDeltaLev >= -20 && nDeltaLev < -10)
                    nExp = (int)(nExp * 1.8);
		        else if (nDeltaLev < -20)
                    nExp = (int)(nExp * 2.3);
	        }

            return nExp < 0 ? 0 : nExp ;
        }
        //计算爆率.. 0.不爆 1.小爆 2 ~ 5
        //2.中爆 6 ~ 9
        //3.大爆 10 ~ 15
        public static byte AdjustDrop(BaseObject attack, BaseObject Def)
        {
            byte attklv = attack.GetLevel();
            byte deflv = Def.GetLevel();

            int dislv = attklv - deflv;

            if (dislv >= 0) //攻击者高于被攻击者
            {
                if (dislv <= 5 && dislv > 3)//差距小于5级
                {
                    return IRandom.Random(0, 100) < 50 ? EXPLODE_ITEM_CHANCE3 : EXPLODE_ITEM_CHANCE1;
                }
                else if (dislv <= 9 && dislv > 6)//差距小于10级
                {
                    return IRandom.Random(0, 100) < 50 ? EXPLODE_ITEM_CHANCE2 : EXPLODE_ITEM_CHANCE1;
                }
                else //大爆 就算大爆不了，也小爆
                {
                    return IRandom.Random(0, 100) < 50 ? EXPLODE_ITEM_CHANCE4 : EXPLODE_ITEM_CHANCE2;
                }
            }
            else //攻击者低于被攻击者
            {
                if (dislv <= -5 && dislv > -3)//差距小于5级
                {
                    return IRandom.Random(0, 100) < 50 ? EXPLODE_ITEM_CHANCE3 : EXPLODE_ITEM_CHANCE2;
                }
                else if (dislv <= -9 && dislv > -6)//差距小于10级
                {
                    return IRandom.Random(0, 100) < 50 ? EXPLODE_ITEM_CHANCE2 : EXPLODE_ITEM_CHANCE1;
                }
                else //大爆 就算大爆不了，也小爆
                {
                    return IRandom.Random(0, 100) < 50 ? EXPLODE_ITEM_CHANCE4 : EXPLODE_ITEM_CHANCE2;
                }
            }
          

            
        
        }

        //玩家与玩家pk
        private static uint AdjustDamage(PlayerObject attack, PlayerObject def, bool isMagicAck = false/*是否是魔法伤害*/)
        {
            int attack_soul = attack.GetFightSoul();
            int def_soul = def.GetFightSoul();
            int dif = attack_soul - def_soul;
            int nSoulAddAtk = 0;
              int nSoulAddDef = 0;
            if(dif > 0)
            {
                //一点战斗力+一百点伤害
                nSoulAddAtk = dif * 100;
            }else
            {
                //一点战斗力 +一百点防御
                nSoulAddDef = Math.Abs(dif) * 100;
            }
          
          
            int nMaxRand = 50 + attack.GetLuck();
            int nAtk = 0;
            if (IRandom.Random(0, 100) < nMaxRand)
            {
                if (isMagicAck &&
                    attack.GetBaseAttr().profession == JOB.MAGE)
                {
                    nAtk = attack.GetMinAck() +
                IRandom.Random(0, attack.GetMaxMagixAck() - attack.GetMagicAck()) ;
                }
                else
                {
                    nAtk = attack.GetMaxAck() +
                 IRandom.Random(0, attack.GetMaxAck() - attack.GetMinAck()) ;
                }
             
            }
            else
            {
                if (isMagicAck &&
                  attack.GetBaseAttr().profession == JOB.MAGE)
                {
                    nAtk = attack.GetMinAck() + IRandom.Random(0, attack.GetMagicAck() - attack.GetMagicAck()) ; 
                }
                else
                {
                    nAtk = attack.GetMinAck() + IRandom.Random(0, attack.GetMaxAck() - attack.GetMinAck()) ; 
                }
               
            }
            nAtk += nSoulAddAtk;
            int nDef = 0;
            if (isMagicAck)
            {
               nDef = def.GetMagicDefense();
            }
            else
            {
               nDef = def.GetDefense();
            }
            nDef += nSoulAddDef;
            int nDamage = nAtk - nDef;
           
            if (attack.type == OBJECTTYPE.PLAYER)
            {
                nDamage += attack.GetLevel() / 10;
            }

            if (nDamage <= 0)
            {
                nDamage = IRandom.Random(1, 100) >= 50  ? 1 : 0;
                if (!isMagicAck) nDamage = 1;
             }
           
          
            return (uint)nDamage;
        }
        
        //计算伤害
        public static uint AdjustDamage(BaseObject attack, BaseObject def,bool isMagicAck = false/*是否是魔法伤害*/)
        {
            //玩家与玩家伤害计算用战斗力表示
            if (attack.type == OBJECTTYPE.PLAYER && def.type == OBJECTTYPE.PLAYER)
            {
                return AdjustDamage(attack as PlayerObject, def as PlayerObject, isMagicAck);
            }
            int nMaxRand = 50 + attack.GetLuck();
            int nAtk = 0;
            if (IRandom.Random(0, 100) < nMaxRand)
            {
                //魔法攻击只适用于法师
                if (isMagicAck && 
                    attack.type == OBJECTTYPE.PLAYER && 
                    (attack as PlayerObject).GetBaseAttr().profession == JOB.MAGE)
                {
                    nAtk = attack.GetMagicAck() +
                IRandom.Random(0, attack.GetMaxMagixAck() - attack.GetMagicAck()) ;
                }
                else
                {
                    nAtk = attack.GetMaxAck() +
                 IRandom.Random(0, attack.GetMaxAck() - attack.GetMinAck()) ;
                }
             
            }
            else
            {
                if (isMagicAck &&
                     attack.type == OBJECTTYPE.PLAYER &&
                     (attack as PlayerObject).GetBaseAttr().profession == JOB.MAGE)
                {
                    nAtk = attack.GetMagicAck() + IRandom.Random(0, attack.GetMaxMagixAck() - attack.GetMagicAck()); 
                }
                else
                {
                    nAtk = attack.GetMinAck() + IRandom.Random(0, attack.GetMaxAck() - attack.GetMinAck()) ; 
                }
               
            }

            int nDef = 0;
            if (isMagicAck)
            {
               nDef = def.GetMagicDefense();
            }
            else
            {
               nDef = def.GetDefense();
            }
            int nDamage = nAtk - nDef;
           
            if (attack.type == OBJECTTYPE.PLAYER)
            {
                nDamage += attack.GetLevel() / 10;
            }

            if (nDamage <= 0)
            {
                nDamage = IRandom.Random(1, 100) >= 50  ? 1 : 0;
                if (!isMagicAck) nDamage = 1;
             }
           
          
            return (uint)nDamage;
        }
        
        

    }
}
