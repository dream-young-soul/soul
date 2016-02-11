using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//A*寻路类
namespace MapServer
{
    public struct FindPoint
    {
        public short x;
        public short y;
    }
    class TTree
    {
        public int h;
        public short x;
        public short y;
        public byte dir;
        public TTree Father;
        public TTree()
        {
            h = 0;
            x = 0;
            y = 0;
            dir = 0;
            Father = null;
        }

    }

    class TLink
    {
        public TTree node;
        public int f;
        public TLink next;
        public TLink()
        {
            node = null;
            next = null;
            f = 0;
        }
    }
    public class MapPath
    {

        public const byte MASK_OPEN = 1;
        public const byte MASK_CLOSE = 0;
        private uint Width;
        public uint Height;
        private byte[,] mMapData; //1表示可以通过 0表示阻挡
        private TLink Queue;
        private uint[] mPassPoint;//有些地图是没有怪物的- 就不要浪费内存了 等需要寻路的时候再new 申请内存 2015.11.14
        public MapPath(uint nWidth, uint nHeight)
        {
            Queue = new TLink();
            Width = nWidth;
            Height = nHeight;
            mMapData = new byte[nHeight, nWidth];
            mPassPoint = null;
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    mMapData[i, j] = MASK_OPEN;
                }

            }
        }

        public void SetPointMask(short x, short y, byte tag)
        {
            if (x < 0 || y < 0) return;
            if (x >= Width || y >= Height) return;
            mMapData[y, x] = tag;
        }

        
        public List<FindPoint> FindPath(short scr_x, short scr_y, short dest_x, short dest_y)
        {

            if (mMapData[dest_y, dest_x] == MASK_CLOSE) return null;
            if (scr_x == dest_x && scr_y == dest_y) return null;

            //放在这里申请 只是为了节约内存，目前A*寻路也只是用于怪物- 没有地图的怪物可以省下这笔内存
            if (mPassPoint == null)
            {
                mPassPoint = new uint[Height * Width];
            }
            for (int i = 0; i < mPassPoint.Length; i++)
            {
                mPassPoint[i] = 0xFFFFFFFF;
            }

            Init_Queue();
            TTree root = new TTree();
            root.x = scr_x;
            root.y = scr_y;
            root.h = 0;
            root.Father = null;
            Enter_Queue(root, judge(scr_x, scr_y, dest_x, dest_y));
            short end_x = dest_x;
            short end_y = dest_y;
            int ii = 0;
            int index = 0;
            //bool bTry;
            while (true)
            {
                root = Get_From_Queue(); //将第一个弹出
                ii++;
                if (ii == 86610) ii = 0;
                if (root == null) break;
                index++;

                short x = root.x;
                short y = root.y;
                if (x == end_x && y == end_y)
                {
                    break;
                }

                Trytile(x, (short)(y - 1), end_x, end_y, root, 0); //尝试向上移动
                Trytile((short)(x + 1), (short)(y - 1), end_x, end_y, root, 1); //尝试向右上移动
                Trytile((short)(x + 1), y, end_x, end_y, root, 2); //尝试向右移动
                Trytile((short)(x + 1), (short)(y + 1), end_x, end_y, root, 3); //尝试向右下移动
                Trytile(x, (short)(y + 1), end_x, end_y, root, 4); //尝试向下移动
                Trytile((short)(x - 1), (short)(y + 1), end_x, end_y, root, 5); //尝试向左下移动
                Trytile((short)(x - 1), y, end_x, end_y, root, 6); //尝试向左移动
                Trytile((short)(x - 1), (short)(y - 1), end_x, end_y, root, 7); //尝试向左上移动
              //bTry = false;
             
                //if (Trytile(x, (short)(y - 1), end_x, end_y, root, 0)) bTry = true; //尝试向上移动
                //if (Trytile((short)(x + 1), (short)(y - 1), end_x, end_y, root, 1)) bTry = true; //尝试向右上移动
                //if (Trytile((short)(x + 1), y, end_x, end_y, root, 2)) bTry = true; //尝试向右移动
                //if (Trytile((short)(x + 1), (short)(y + 1), end_x, end_y, root, 3)) bTry = true; //尝试向右下移动
                //if (Trytile(x, (short)(y + 1), end_x, end_y, root, 4)) bTry = true; //尝试向下移动
                //if (Trytile((short)(x - 1), (short)(y + 1), end_x, end_y, root, 5)) bTry = true; //尝试向左下移动
                //if (Trytile((short)(x - 1), y, end_x, end_y, root, 6)) bTry = true; //尝试向左移动
                //if (Trytile((short)(x - 1), (short)(y - 1), end_x, end_y, root, 7)) bTry = true; //尝试向左上移动
            }

            if (root == null) return null;
            List<FindPoint> ret = new List<FindPoint>();
            FindPoint temp;
            temp.x = root.x;
            temp.y = root.y;
            ret.Add(temp);
            TTree p = root;
            root = root.Father;
            while (root != null)
            {
                temp.x = root.x;
                temp.y = root.y;
                ret.Add(temp);
                root = root.Father;
            }
            return ret;
        }

        private TTree Get_From_Queue()
        {
            TTree bestchoice;
            TLink Next;
            bestchoice = Queue.next.node;
            Next = Queue.next.next;
            Queue.next = null;
            Queue.next = Next;
            return bestchoice;
        }
        private void Init_Queue()
        {
            Queue = new TLink();
            Queue.node = null;
            Queue.f = -1;
            Queue.next = new TLink();
            Queue.next.f = 0xfffffff;
            Queue.next.node = null;
            Queue.next.next = null;
        }

        private void Enter_Queue(TTree node, int f)
        {
            TLink p = Queue;
            TLink Father = p;
            while (f > p.f)
            {
                Father = p;
                p = p.next;
                if (p == null) break;
            }
            TLink q = new TLink();
            q.f = f;
            q.node = node;
            q.next = p;
            Father.next = q;

        }

        // 估价函数,估价 x,y 到目的地的距离,估计值必须保证比实际值小		
        private int judge(int x, int y, int end_x, int end_y)
        {
            int nx = end_x - x;
            int ny = end_y - y;
            return Math.Abs(nx) + Math.Abs(ny);
        }

        // 尝试下一步移动到 x,y 可行否
        private bool Trytile(short x, short y, short end_x, short end_y, TTree father, byte dir)
        {
            TTree p;
            uint h;
            bool Result = false;
            if (mMapData[y, x] == MASK_CLOSE)
            {
                return Result;
            }
            p = father;
            while (p != null)
            {
                if (x == p.x && y == p.y)
                {
                    return false;
                }
                p = p.Father;
            }
            if (dir == 0 || dir == 2 || dir == 4 || dir == 6)
            {
                h = (uint)father.h + 10;
            }
            else
            {
                h = (uint)father.h + 14;
            }
            if (h >= mPassPoint[x * Height + y])
            {
                return false; //// 如果曾经有更好的方案移动到 (x,y) 失败
            }
            mPassPoint[x * Height + y] = h; // 记录这次到 (x,y) 的距离为历史最佳距离

            p = new TTree();
            p.Father = father;
            p.h = (int)h;
            p.x = x;
            p.y = y;
            p.dir = dir;
            Enter_Queue(p, p.h + judge(x, y, end_x, end_y));
            return true;
        }
    }





}
