using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace NeuroOCR
{
    class TableOCR
    {
        private List<List<int>> priority = new List<List<int>>();
        private String path;
        public TableOCR(String p)
        {
            path = p;
            path = path.Replace("?", "VV");
            path = path.Replace("i.", "SSi.");
            path = path.Replace("j.", "SSj.");
            path = path.Replace("a.", "SSa.");
            Read();
        }
        public TableOCR(String p, bool a)
        {
            String[] str = p.Split('\n');
            foreach (String s in str)
            {
                if (s.Trim() == "")
                {
                    break;
                }
                List<int> l = new List<int>();
                Priority.Add(l);
                foreach (String e in s.Split(' '))
                {
                    if ((e.Trim() == "") && (e != "0"))
                    {
                        break;
                    }
                    l.Add(int.Parse(e));
                }
            }
        }
        public TableOCR(Bitmap input)
        {
            for (int j = 0; j < input.Height; j++)
            {
                List<int> l = new List<int>();
                Priority.Add(l);
                for (int i = 0; i < input.Width; i++)
                {
                    UInt32 pixel = (UInt32)(input.GetPixel(i, j).ToArgb());
                    float R = (float)((pixel & 0x00FF0000) >> 16);
                    float G = (float)((pixel & 0x0000FF00) >> 8);
                    float B = (float)(pixel & 0x000000FF);
                    int N = (int)Math.Round((R + G + B) / 3.0f);
                    N = 255 - N;
                    l.Add(N);
                }
            }
        }
        public List<List<int>> Priority { get => priority; }
        public void Read()
        {
            Priority.Clear();
            try
            {
                String[] str = System.IO.File.ReadAllLines(path);
                foreach (String s in str)
                {
                    if (s.Trim() == "")
                    {
                        break;
                    }
                    List<int> l = new List<int>();
                    Priority.Add(l);
                    foreach (String e in s.Split(' '))
                    {
                        if ((e.Trim() == "") && (e != "0"))
                        {
                            break;
                        }
                        l.Add(int.Parse(e));
                    }
                }
            }
            catch
            {
                try
                {
                    System.IO.Directory.CreateDirectory("ocr");
                }
                catch { }
            }
        }
        public void Add(TableOCR ocr)
        {
            int y = 0;
            foreach (List<int> lu in ocr.Priority)
            {
                if (Priority.Count == y)
                {
                    Priority.Add(new List<int>());
                }
                int x = 0;
                foreach (int u in lu)
                {
                    if (Priority[y].Count > x)
                    {
                        Priority[y][x] += u;
                    }
                    else
                    {
                        Priority[y].Add(u);
                    }
                    x++;
                }
                y++;
            }
        }
        public int Multiple(TableOCR ocr)
        {
            int n = 0;
            int y = 0;
            foreach (List<int> lu in ocr.Priority)
            {
                if (Priority.Count > y)
                {
                    int x = 0;
                    foreach (int u in lu)
                    {
                        if (Priority[y].Count > x)
                        {
                            n += Priority[y][x] * u;
                        }
                        x++;
                    }
                }
                y++;
            }
            return n;
        }
        public void Delete(TableOCR ocr)
        {
            int y = 0;
            foreach (List<int> lu in ocr.Priority)
            {
                if (Priority.Count == y)
                {
                    Priority.Add(new List<int>());
                }
                int x = 0;
                foreach (int u in lu)
                {
                    if (Priority[y].Count > x)
                    {
                        Priority[y][x] -= u;
                    }
                    else
                    {
                        Priority[y].Add((-1) * u);
                    }
                    x++;
                }
                y++;
            }
        }
        public int Sqrt(int u) {
            if (u >= 0)
            {
                return (int)Math.Round(Math.Sqrt(u));
            }
            else {
                return ((-1)*(int)Math.Round(Math.Sqrt((-1)*u)));
            }
        }
        public int Sqa(int u)
        {
            if (u >= 0)
            {
                return (u*u);
            }
            else
            {
                return (u*u*(-1));
            }
        }
        public void Flush()
        {
            List<List<int>> n = new List<List<int>>();
            foreach (List<int> lu in Priority)
            {
                List<int> no = new List<int>();
                n.Add(no);
                foreach (int u in lu)
                {
                    no.Add(Sqrt(u));
                }
            }
            Priority.Clear();
            Priority.AddRange(n);
        }
        public void Hard()
        {
            List<List<int>> n = new List<List<int>>();
            foreach (List<int> lu in Priority)
            {
                List<int> no = new List<int>();
                n.Add(no);
                foreach (int u in lu)
                {
                    no.Add(Sqa(u));
                }
            }
            Priority.Clear();
            Priority.AddRange(n);
        }
        public void Clear()
        {
            List<List<int>> n = new List<List<int>>();
            foreach (List<int> lu in Priority)
            {
                List<int> no = new List<int>();
                n.Add(no);
                foreach (int u in lu)
                {
                    no.Add(0);
                }
            }
            Priority.Clear();
            Priority.AddRange(n);
        }
        public void Save()
        {
            String[] lines = new String[Priority.Count];
            int x = 0;
            foreach (List<int> lu in Priority)
            {
                String str = "";
                foreach (int u in lu)
                {
                    str += " " + u.ToString();
                }
                lines[x] = str.Substring(1);
                x++;
            }
            System.IO.File.WriteAllLines(path, lines);
        }
    }
}
