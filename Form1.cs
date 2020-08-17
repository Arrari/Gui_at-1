using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using ClipperLib;

namespace WindowsFormsApplication1
{
    using Polygon = List<IntPoint>;
    using Polygons = List<List<IntPoint>>;
    public partial class Form1 : Form
    {
        private Bitmap mybitmap;
        private Polygons subjects = new Polygons();
        private Polygons clips = new Polygons();
        private Polygons solution = new Polygons();
        private float scale = 100;
        private void label1_Click(object sender, EventArgs e)
        {

        }
        public Form1()
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(Form1_MouseWheel);
            mybitmap = new Bitmap(
              pictureBox1.ClientRectangle.Width,
              pictureBox1.ClientRectangle.Height,
              PixelFormat.Format32bppArgb);
        }
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
           
        }
        private IntPoint GenerateRandomPoint(int l, int t, int r, int b, Random rand)
        {
            int Q = 10;
            return new IntPoint(
              Convert.ToInt64((rand.Next(r / Q) * Q + l + 10) * scale),
              Convert.ToInt64((rand.Next(b / Q) * Q + t + 10) * scale));
        }
        private void GenerateRandomPolygon(int count)
        {
            int Q = 10;
            Random rand = new Random();
            int l = 10;
            int t = 10;
            int r = (pictureBox1.ClientRectangle.Width - 20) / Q * Q;
            int b = (pictureBox1.ClientRectangle.Height - 20) / Q * Q;

            subjects.Clear();
            clips.Clear();

            Polygon subj = new Polygon(count);
            for (int i = 0; i < count; ++i)
                subj.Add(GenerateRandomPoint(l, t, r, b, rand));
            subjects.Add(subj);

            Polygon clip = new Polygon(count);
            for (int i = 0; i < count; ++i)
                clip.Add(GenerateRandomPoint(l, t, r, b, rand));
            clips.Add(clip);
        }
        private void bRefresh_Click(object sender, EventArgs e)
        {
            DrawBitmap();
        }
        static private PointF[] PolygonToPointFArray(Polygon pg, float scale)
        {
            PointF[] result = new PointF[pg.Count];
            for (int i = 0; i < pg.Count; ++i)
            {
                result[i].X = (float)pg[i].X / scale;
                result[i].Y = (float)pg[i].Y / scale;
            }
            return result;
        }
        private void DrawBitmap(bool justClip = false)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                GenerateRandomPolygon((int)numericUpDown1.Value);
                using (Graphics newgraphic = Graphics.FromImage(mybitmap))
                using (GraphicsPath path = new GraphicsPath())

                {
                    newgraphic.SmoothingMode = SmoothingMode.AntiAlias;
                    newgraphic.Clear(Color.White);
                    path.FillMode = FillMode.Winding;

                    //draw subjects ...
                    foreach (Polygon pg in subjects)
                    {
                        PointF[] pts = PolygonToPointFArray(pg, scale);
                        path.AddPolygon(pts);
                        pts = null;
                    }
                    using (Pen myPen = new Pen(Color.FromArgb(196, 0xC3, 0xC9, 0xCF), (float)0.6))
                    using (SolidBrush myBrush = new SolidBrush(Color.FromArgb(127, 0xDD, 0xDD, 0xF0)))
                    {
                        newgraphic.FillPath(myBrush, path);
                        newgraphic.DrawPath(myPen, path);
                        path.Reset();

                        //draw clips ...
                        path.FillMode = FillMode.Winding;
                        foreach (Polygon pg in clips)
                        {
                            PointF[] pts = PolygonToPointFArray(pg, scale);
                            path.AddPolygon(pts);
                            pts = null;
                        }
                        myPen.Color = Color.FromArgb(196, 0xF9, 0xBE, 0xA6);
                        myBrush.Color = Color.FromArgb(127, 0xFF, 0xE0, 0xE0);
                        newgraphic.FillPath(myBrush, path);
                        newgraphic.DrawPath(myPen, path);
                    }
                }

                pictureBox1.Image = mybitmap;

            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
            DrawBitmap();
        }
        private void bClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (pictureBox1.ClientRectangle.Width == 0 ||
                pictureBox1.ClientRectangle.Height == 0) return;
            if (mybitmap != null)
                mybitmap.Dispose();
            mybitmap = new Bitmap(
                pictureBox1.ClientRectangle.Width,
                pictureBox1.ClientRectangle.Height,
                PixelFormat.Format32bppArgb);
            pictureBox1.Image = mybitmap;
            DrawBitmap();
        }
        private void nudCount_ValueChanged(object sender, EventArgs e)
        {
            DrawBitmap(true);
        }
    }
}
