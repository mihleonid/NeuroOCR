using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace NeuroOCR
{
    public partial class Form1 : Form
    {
        NeuroNet neuroNet = new NeuroNet();
        private Bitmap img;
        private Bitmap bigimg;
        private Bitmap source;
        private Bitmap bigsource;
        private short white = 240;
        private short whitec = 130;
        private bool wc = true;
        private byte free = 20;
        private bool dia = true;
        private uint plo = 30;
        private int light = 0;
        private int lighta = 0;
        private int lightaa = 0;
        private bool pc = false;
        private bool anal = false;
        private bool count = true;
        private List<List<Bitmap>> llb = new List<List<Bitmap>>();
        private Point tmpcoordinatsintext=new Point(-1,-1);
        private List<Point> was = new List<Point>();
        public Form1()
        {
            InitializeComponent();
            DragNDrop(this);
            foreach (Control cnt in Controls)
            {
                DragNDrop(cnt);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            String a;
            if (!File.Exists("ocr.file"))
            {
                File.WriteAllText("ocr.file", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!.?");
                a = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!.?";
            }
            else
            {
                a = File.ReadAllText("ocr.file");
            }
            for (int i = 0; i < a.Length; i++)
            {
                this.listBox1.Items.Add(a[i].ToString());
            }
        }
        private void DragNDrop(Control cnt)
        {
            cnt.AllowDrop = true;
            cnt.DragEnter += new DragEventHandler(fDragEnter);
            cnt.DragDrop += new DragEventHandler(fDragDrop);
        }
        private void fDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) { e.Effect = DragDropEffects.Copy; }
        }
        private void fDragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    MkImg(file);
                }
            }
            catch { }
        }
        private void MkImg(String file)
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            bigimg = new Bitmap(fs);
            bigimg = Transparent2Color(bigimg);
            BlackWhite(bigimg);
            bigsource = bigimg.Clone(new Rectangle(0,0,bigimg.Width,bigimg.Height), PixelFormat.DontCare);
            pictureBox2.Image = bigimg;
            tmpcoordinatsintext = new Point(-1,-1);
            fs.Close();
            if (checkBox7.Checked)
            {
               button1_Click(new object(), new EventArgs());
            }
        }
        private void Chop() {
            List<Bitmap> lines = new List<Bitmap>();
            int listen = -1;
            Bitmap aline = null;
            for (int i = 0; i < bigimg.Height; i++)
            {
                Bitmap line = new Bitmap(bigimg.Width, 1);
                bool s = false;
                for (int j = 0; j < bigimg.Width; j++)
                {
                    Point p = new Point(j, i);
                    Color c = bigimg.GetPixel(p.X, p.Y);
                    if (c.B<numericUpDown8.Value) {
                        s =true;
                    }
                    line.SetPixel(p.X, 0, c);
                }
                if (s)
                {
                    aline = Glue(aline, line);
                    listen = (int)numericUpDown9.Value;
                }
                else {
                    if (listen > 0)
                    {
                        aline = Glue(aline, line);
                        listen--;
                    }
                    else {
                        if (listen==0) {
                            lines.Add(aline);
                            aline = null;
                            listen--;
                        }
                    }
                }
            }
            if (listen>=0) {
                lines.Add(aline);
                aline = null;
            }
            listen = -1;
            llb.Clear();
            foreach (Bitmap b in lines) {
                List<Bitmap> letters = new List<Bitmap>();
                llb.Add(letters);
                for (int i=0;i<b.Width;i++) {
                    bool s = false;
                    Bitmap line = new Bitmap(1, b.Height);
                    for (int j=0;j<b.Height;j++) {
                        Color c = b.GetPixel(i, j);
                        if (c.B< numericUpDown8.Value) {
                            s = true;
                        }
                        line.SetPixel(0, j, c);
                    }
                    if (s)
                    {
                        aline = Glue2(aline, line);
                        listen = (int)numericUpDown10.Value;
                    }
                    else
                    {
                        if (listen > 0)
                        {
                            aline = Glue2(aline, line);
                            listen--;
                        }
                        else
                        {
                            if (listen == 0)
                            {
                                letters.Add(aline);
                                aline = null;
                                listen--;
                            }
                        }
                    }
                }
                if (listen >= 0)
                {
                    letters.Add(aline);
                    aline = null;
                }
            }
            tmpcoordinatsintext = new Point(0, 0);
            MkLetter();
        }
        #region letter
        private void MkLetter() {
            if (tmpcoordinatsintext==new Point(-1,-1)) {
                Chop();
            }
            try {
            img = Transparent2Color(ResizeImage(llb[tmpcoordinatsintext.X][tmpcoordinatsintext.Y]));
            source = (Bitmap)img.Clone();
            if (checkBox5.Checked)
            {
                PreFormat();
            }
            pictureBox1.Image = img;
            } catch {}
        }
        private void Next() {
            if (tmpcoordinatsintext == new Point(-1, -1))
            {
                Chop();
            }
            else
            {
                tmpcoordinatsintext.Y += 1;
                try
                {
                    var tr=llb[tmpcoordinatsintext.X][tmpcoordinatsintext.Y];
                }
                catch {
                    tmpcoordinatsintext.Y = 0;
                    tmpcoordinatsintext.X += 1;
                    if (checkBox9.Checked) {
                        textBox2.Text += "\r\n";
                    }
                    try
                    {
                        var tr = llb[tmpcoordinatsintext.X][tmpcoordinatsintext.Y];
                    }
                    catch
                    {
                        tmpcoordinatsintext = new Point(-2,-2);
                    }
                }
            }
            MkLetter();
        }
        private void Recognize() {
            if (img!=null) {
                textBox1.Text = neuroNet.Recognize(new TableOCR(img));
            }
        }
        private void Insert() {
            textBox2.Text += textBox1.Text;
        }
        #endregion
        public void PreFormat()
        {
            int top = 0;
            int left = 0;
            int bottom = 0;
            int right = 0;
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    UInt32 pixel = (UInt32)(img.GetPixel(i, j).ToArgb());
                    float R = (float)((pixel & 0x00FF0000) >> 16);
                    float G = (float)((pixel & 0x0000FF00) >> 8);
                    float B = (float)(pixel & 0x000000FF);
                    R = (int)Math.Round((R + G + B) / 3.0f);
                    if (pc)
                    {
                        R += R / 100 * light;
                    }
                    else
                    {
                        R += light;
                    }
                    if (R > 255)
                    {
                        R = 255;
                    }
                    if (R < 0)
                    {
                        R = 0;
                    }
                    if (wc)
                    {
                        if (R >= whitec)
                        {
                            img.SetPixel(i, j, Color.White);
                        }
                        else
                        {
                            if (pc)
                            {
                                R += R / 100 * lighta;
                            }
                            else
                            {
                                R += lighta;
                            }
                            if (R > 255)
                            {
                                R = 255;
                            }
                            if (R < 0)
                            {
                                R = 0;
                            }
                            img.SetPixel(i, j, Color.FromArgb((int)(0xFF000000 | ((UInt32)R << 16) | ((UInt32)R << 8) | ((UInt32)R))));
                        }
                    }
                    else
                    {
                        if (pc)
                        {
                            R += R / 100 * lighta;
                        }
                        else
                        {
                            R += lighta;
                        }
                        if (R > 255)
                        {
                            R = 255;
                        }
                        if (R < 0)
                        {
                            R = 0;
                        }
                        img.SetPixel(i, j, Color.FromArgb((int)(0xFF000000 | ((UInt32)R << 16) | ((UInt32)R << 8) | ((UInt32)R))));
                    }
                }
            }
            if (count)
            {
                was.Clear();
                for (int i = 0; i < 50; i++)
                {
                    for (int j = 0; j < 50; j++)
                    {
                        Point po = new Point(i, j);
                        if (!was.Contains(po)) {
                        List<Point> l = Points(po);
                        was.AddRange(l);
                        if (l.Count < plo)
                        {
                            foreach (Point p in l)
                            {
                                img.SetPixel(p.X, p.Y, Color.White);
                            }
                        }
                        }
                    }
                }
                was.Clear();
            }
            if (wc)
            {
                for (int i = 0; i < 50; i++)
                {
                    uint s = 0;
                    for (int j = 0; j < 50; j++)
                    {
                        UInt32 pixel = (UInt32)(img.GetPixel(j, i).ToArgb());
                        float R = (pixel & 0x00FF0000) >> 16;
                        float G = (pixel & 0x0000FF00) >> 8;
                        float B = pixel & 0x000000FF;
                        uint N = (uint)Math.Round((R + G + B) / 3.0f);
                        s += N;
                    }
                    s /= 50;
                    if (s >= white)
                    {
                        top++;
                    }
                    else
                    {
                        break;
                    }
                }
                for (int i = 0; i < 50; i++)
                {
                    uint s = 0;
                    for (int j = 0; j < 50; j++)
                    {
                        UInt32 pixel = (UInt32)(img.GetPixel(i, j).ToArgb());
                        float R = (float)((pixel & 0x00FF0000) >> 16);
                        float G = (float)((pixel & 0x0000FF00) >> 8);
                        float B = (float)(pixel & 0x000000FF);
                        uint N = (uint)Math.Round((R + G + B) / 3.0f);
                        s += N;
                    }
                    s /= 50;
                    if (s >= white)
                    {
                        left++;
                    }
                    else
                    {
                        break;
                    }
                }
                for (int i = 49; i >= 0; i--)
                {
                    uint s = 0;
                    for (int j = 0; j < 50; j++)
                    {
                        UInt32 pixel = (UInt32)(img.GetPixel(j, i).ToArgb());
                        float R = (pixel & 0x00FF0000) >> 16;
                        float G = (pixel & 0x0000FF00) >> 8;
                        float B = pixel & 0x000000FF;
                        uint N = (uint)Math.Round((R + G + B) / 3.0f);
                        s += N;
                    }
                    s /= 50;
                    if (s >= white)
                    {
                        bottom++;
                    }
                    else
                    {
                        break;
                    }
                }

                for (int i = 49; i >= 0; i--)
                {
                    uint s = 0;
                    for (int j = 0; j < 50; j++)
                    {
                        UInt32 pixel = (UInt32)(img.GetPixel(i, j).ToArgb());
                        float R = (pixel & 0x00FF0000) >> 16;
                        float G = (pixel & 0x0000FF00) >> 8;
                        float B = pixel & 0x000000FF;
                        uint N = (uint)Math.Round((R + G + B) / 3.0f);
                        s += N;
                    }
                    s /= 50;
                    if (s >= white)
                    {
                        right++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (top + bottom > 50)
                {
                    bottom = 0;
                    top = 0;
                }

                if (left + right > 50)
                {
                    right = 0;
                    left = 0;
                }
                img = ResizeImage(img.Clone(new Rectangle(new Point(left, top), new Size(50 - left - right, 50 - top - bottom)), PixelFormat.DontCare));
            }
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    int R = img.GetPixel(i, j).B;
                    if (pc)
                    {
                        R += R / 100 * lightaa;
                    }
                    else
                    {
                        R += lightaa;
                    }
                    if (R > 255)
                    {
                        R = 255;
                    }
                    if (R < 0)
                    {
                        R = 0;
                    }
                    img.SetPixel(i, j, Color.FromArgb(R, R, R));
                }
            }
        }
        #region BitmapTools
        #region Glue
        /*private Bitmap Glue(Bitmap a, Bitmap b)
        {
            if (a == null)
            {
                return b;
            }
            if (b == null)
            {
                return a;
            }
            Bitmap i = new Bitmap(a.Width, a.Height + b.Height);
            try
            {
                for (int k = 0; k < i.Height; k++)
                {
                    for (int j = 0; j < a.Width; j++)
                    {
                        if (k >= a.Height)
                        {
                            i.SetPixel(j, k, b.GetPixel(j, k - a.Height));
                        }
                        else
                        {
                            i.SetPixel(j, k, a.GetPixel(j, k));
                        }
                    }
                }
            }
            catch
            {
                return a;
            }
            return i;
        }
        private Bitmap Glue2(Bitmap a, Bitmap b)
        {
            if (a == null)
            {
                return b;
            }
            if (b == null)
            {
                return a;
            }
            Bitmap i = new Bitmap(a.Width + b.Width, a.Height);
            try
            {
                for (int k = 0; k < i.Width; k++)
                {
                    for (int j = 0; j < a.Height; j++)
                    {
                        if (k >= a.Width)
                        {
                            i.SetPixel(k, j, b.GetPixel(k - a.Width, j));
                        }
                        else
                        {
                            i.SetPixel(k, j, a.GetPixel(k, j));
                        }
                    }
                }
            }
            catch
            {
                return a;
            }
            return i;
        }*/
        private Bitmap Glue(Bitmap a, Bitmap b)
        {
            if (a == null)
            {
                return b;
            }
            if (b == null)
            {
                return a;
            }
            Bitmap i = new Bitmap(a.Width, a.Height + b.Height);
            try
            {
                Graphics graphics = Graphics.FromImage(i);
                graphics.DrawImage(a, 0, 0);
                graphics.DrawImage(b, 0, a.Height);
                graphics.Dispose();
            }
            catch
            {
                return a;
            }
            return i;
        }
        private Bitmap Glue2(Bitmap a, Bitmap b)
        {
            if (a == null)
            {
                return b;
            }
            if (b == null)
            {
                return a;
            }
            Bitmap i = new Bitmap(a.Width + b.Width, a.Height);
            try
            {
                Graphics graphics = Graphics.FromImage(i);
                graphics.DrawImage(a, 0, 0);
                graphics.DrawImage(b, a.Width, 0);
                graphics.Dispose();
            }
            catch
            {
                return a;
            }
            return i;
        }
        #endregion
        private void BlackWhite(Bitmap bmp) {
            for (int i=0;i<bmp.Height;i++) {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Point p = new Point(j,i);
                    Color c = bmp.GetPixel(p.X, p.Y);
                    int px = (int)Math.Round((float)((float)(c.R + c.G + c.B)) / 3.0f);
                    bmp.SetPixel(p.X, p.Y, Color.FromArgb(px,px,px));
                }
            }
        }
        private Bitmap Transparent2Color(Bitmap bmp1)
        {
            Color target = Color.White;
            Bitmap bmp2 = new Bitmap(bmp1.Width, bmp1.Height);
            Rectangle rect = new Rectangle(Point.Empty, bmp1.Size);
            using (Graphics G = Graphics.FromImage(bmp2))
            {
                G.Clear(target);
                G.DrawImageUnscaledAndClipped(bmp1, rect);
            }
            return bmp2;
        }
        public static Bitmap ResizeImage(Image image, int width = 50, int height = 50)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        #endregion
        private List<Point> Points(Point start)
        {
            //List<Point> l = RecColor(start, img.GetPixel(start.X, start.Y).B); //TODO: правильная волшебная палочка
            //return l;
            MagicStick ms = new MagicStick(img, dia, free);
            return ms.GetPoints(start);
        }
        private List<Point> RecColor(Point start, uint color)
        {
            List<Point> l = new List<Point>();
            try
            {
                if ((!(was.Contains(start))) && (start.X < 50) && (start.X >= 0) && (start.Y < 50) && (start.Y >= 0) && (img.GetPixel(start.X, start.Y).B + free >= color) && (color >= img.GetPixel(start.X, start.Y).B - free))
                {
                    was.Add(start);
                    l.Add(start);
                    l.AddRange(RecColor(new Point(start.X, start.Y + 1), color));
                    l.AddRange(RecColor(new Point(start.X + 1, start.Y), color));
                    l.AddRange(RecColor(new Point(start.X, start.Y - 1), color));
                    if (dia)
                    {
                        l.AddRange(RecColor(new Point(start.X + 1, start.Y + 1), color));
                        l.AddRange(RecColor(new Point(start.X - 1, start.Y - 1), color));
                        l.AddRange(RecColor(new Point(start.X + 1, start.Y - 1), color));
                        l.AddRange(RecColor(new Point(start.X - 1, start.Y + 1), color));
                    }
                }
            }
            catch { }
            return l;
        }
        #region Form
        private void button1_Click(object sender, EventArgs e)
        {
            if (tmpcoordinatsintext == new Point(-1, -1))
            {
                Chop();
            }
            else {
                tmpcoordinatsintext = new Point(0,0);
                MkLetter();
            }
            while (tmpcoordinatsintext!=new Point(-2,-2)) {
                button15_Click(sender, e);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            neuroNet.HelpClever(new TableOCR(img), (String)listBox1.SelectedItem);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            neuroNet.HardClever(new TableOCR(img), (String)listBox1.SelectedItem);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            white = (short)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            whitec = (short)numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            free = (byte)numericUpDown3.Value;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dia = checkBox1.Checked;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            plo = (uint)numericUpDown4.Value;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            img = (Bitmap)source.Clone();
            PreFormat();
            pictureBox1.Image = img;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            neuroNet.HardNotClever(new TableOCR(img), (String)listBox1.SelectedItem);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            neuroNet.Flush();
        }
        #region lighter
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            light = (int)numericUpDown5.Value;
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            lighta = (int)numericUpDown6.Value;
        }
        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            lightaa = (int)numericUpDown11.Value;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                numericUpDown5.Maximum = 100;
                numericUpDown6.Maximum = 100;
                numericUpDown5.Minimum = -100;
                numericUpDown6.Minimum = -100;
                numericUpDown5.Value = 0;
                numericUpDown6.Value = 0;
                numericUpDown11.Value = 0;
                numericUpDown11.Minimum = -100;
                numericUpDown11.Maximum = 100;
            }
            else
            {
                numericUpDown5.Maximum = 255;
                numericUpDown6.Maximum = 255;
                numericUpDown5.Minimum = -255;
                numericUpDown6.Minimum = -255;
                numericUpDown5.Value = 0;
                numericUpDown6.Value = 0;
                numericUpDown11.Value = 0;
                numericUpDown11.Minimum = -255;
                numericUpDown11.Maximum = 255;
            }
            pc = checkBox2.Checked;
        }
        #endregion
        private void button9_Click(object sender, EventArgs e)
        {
            neuroNet.Fix((uint)numericUpDown7.Value, anal);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            anal = checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            count = checkBox4.Checked;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            neuroNet.Clear((String)listBox1.SelectedItem);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            neuroNet.MegaClever(new TableOCR(img), (String)listBox1.SelectedItem);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            neuroNet.SoftClever(new TableOCR(img), (String)listBox1.SelectedItem);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists("ocr")) {
                    Directory.CreateDirectory("ocr");
                }
                if (Directory.Exists("ocrbest"))
                {
                    foreach (String fi in Directory.GetFiles("ocrbest"))
                    {
                        try
                        {
                            File.Delete(Path.Combine("ocr", Path.GetFileName(fi)));
                        }
                        catch { }
                        try
                        {
                            File.Copy(fi, Path.Combine("ocr", Path.GetFileName(fi)));
                        }
                        catch { }
                    }
                }
            }
            catch { }
            try {
                Application.Restart();
            } catch { }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            wc = checkBox8.Checked;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            neuroNet.Hard();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            try {
                Application.Restart();
            } catch { }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            try {
                neuroNet.Remove((String)listBox1.SelectedItem);
                listBox1.Items.Remove(listBox1.SelectedItem);
            } catch { }
        }
        private void button20_Click(object sender, EventArgs e)
        {
            Chop();
        }
        #endregion

        private void button14_Click(object sender, EventArgs e)
        {
            Next();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            Recognize();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            Insert();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Recognize();
            Insert();
            Next();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (tmpcoordinatsintext!=new Point(-1,-1)) {
                tmpcoordinatsintext = new Point(0,0);
            }
            MkLetter();
        }
    }
}
