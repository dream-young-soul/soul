using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MapServer
{
    public class GameDefine
    {
    }
    //游戏方向 2015.8.12
    public struct DIR
    {
        public const byte DOWN_LEFT = 0; //左下
        public const byte LEFT = 1;       //左
        public const byte DOWN_TOP = 2; //左上
        public const byte TOP = 3;          //上
        public const byte RIGHT_TOP = 4; //右上
        public const byte RIGHT = 5;    //右
        public const byte RIGHT_DOWN = 6; //右下
        public const byte DOWN = 7;         //下
    }
}
