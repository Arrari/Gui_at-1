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
        private Polygons figure1 = new Polygons();
        private Polygons figure2 = new Polygons();
        private Polygons solution = new Polygons();
        private float scale = 100;

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
        private IntPoint GeneratePoint(int x, int y)
        {
            int Q = 10;
            return new IntPoint(
              Convert.ToInt64(x),
              Convert.ToInt64(y));
        }

        private List<IntPoint> StringToInt(string text)
        {
            List<IntPoint> massive = new List<IntPoint>();
            int x = 0;
            int y = 0;
            bool IsX = true;
            string[] mystring = text.Split(' ');
            foreach (string str in mystring)
            {
                if (IsX)
                {
                    x = int.Parse(str);
                    IsX = false;
                }
                else
                {
                    y = int.Parse(str);
                    IsX = true;
                    massive.Add(GeneratePoint(x, y));
                }
            }
            return massive; 
        }
        private void GenerateFigures(string textbox1, string textbox2)
        {
            
            figure1.Clear();
            figure2.Clear();

            List<IntPoint> massive1 = StringToInt(textbox1);
            List<IntPoint> massive2 = StringToInt(textbox2);
            Polygon polygon1 = new Polygon(massive1.Count);
            for (int i = 0; i < massive1.Count; ++i)
                polygon1.Add(massive1[i]);
            figure1.Add(polygon1);

            Polygon polygon2 = new Polygon(massive2.Count);
            for (int i = 0; i < massive2.Count; ++i)
                polygon2.Add(massive2[i]);
            figure2.Add(polygon2);
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
                GenerateFigures(textBox1.Text, textBox2.Text);
                using (Graphics newgraphic = Graphics.FromImage(mybitmap))
                using (GraphicsPath path = new GraphicsPath())

                {
                    newgraphic.SmoothingMode = SmoothingMode.AntiAlias;
                    newgraphic.Clear(Color.White);
                    path.FillMode = FillMode.Winding;

                    //draw subjects ...
                    foreach (Polygon pg in figure1)
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
                        foreach (Polygon pg in figure2)
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
