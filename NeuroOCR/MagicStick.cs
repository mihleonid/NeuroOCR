using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace NeuroOCR
{
    class MagicStick
    {
        private Bitmap bmp;
        private bool dia;
        private byte free;
        private byte color;
        private Stack<Point> need = new Stack<Point>();
        public MagicStick(Bitmap bitmap, bool diag, byte allow)
        {
            bmp = bitmap;
            dia = diag;
            free = allow;
        }
        public List<Point> GetPoints(Point start)
        {
            need.Clear();
            color = GetColor(start);
            List<Point> was = new List<Point>();
            List<Point> l = new List<Point>();
            need.Push(start);
            while (Peek())
            {
                Point tmp = need.Pop();
                if (!was.Contains(tmp))
                {
                    was.Add(tmp);
                    if (IsThis(tmp))
                    {
                        l.Add(tmp);
                        need.Push(new Point(tmp.X + 1, tmp.Y));
                        need.Push(new Point(tmp.X - 1, tmp.Y));
                        need.Push(new Point(tmp.X, tmp.Y + 1));
                        need.Push(new Point(tmp.X, tmp.Y - 1));
                        if (dia)
                        {
                            need.Push(new Point(tmp.X + 1, tmp.Y - 1));
                            need.Push(new Point(tmp.X + 1, tmp.Y + 1));
                            need.Push(new Point(tmp.X - 1, tmp.Y + 1));
                            need.Push(new Point(tmp.X - 1, tmp.Y - 1));
                        }
                    }
                }
            }
            return l;
        }
        private bool Peek()
        {
            return (need.Count != 0);
        }
        private bool IsThis(Point p)
        {
            if ((p.X < 0) || (p.Y < 0) || (p.X >= bmp.Width) || (p.Y >= bmp.Height))
            {
                return false;
            }
            return (Math.Abs(GetColor(p) - color) <= free);
        }
        private byte GetColor(Point p)
        {
            return bmp.GetPixel(p.X, p.Y).B;
        }
    }
}
