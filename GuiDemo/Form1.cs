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
//help me please
namespace WindowsFormsApplication1
{
    using Polygon = List<IntPoint>;
    using Polygons = List<List<IntPoint>>;
    public partial class Form1 : Form
    {


        private Bitmap mybitmap;
        private Polygon figure1 = new Polygon();
        private Polygon figure2 = new Polygon();
        private Polygons solution = new Polygons();

        public Form1()
        {

            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(Form1_MouseWheel);
            mybitmap = new Bitmap(
              pictureBox1.ClientRectangle.Width,
              pictureBox1.ClientRectangle.Height,
              PixelFormat.Format32bppArgb);
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            trackBar1.Maximum = 100;
            trackBar1.TickFrequency = 5;
            trackBar1.LargeChange = 3;
            trackBar1.SmallChange = 2;

        }

        private void trackBar1_Scroll(object sender, System.EventArgs e)
        {
            // Display the trackbar value in the text box.
            textBox3.Text = trackBar1.Value + ":1";
        }
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
           
        }
        //Generate point from INT array
        private IntPoint GeneratePoint(int x, int y)
        {
            return new IntPoint(
              Convert.ToInt64(x),
              Convert.ToInt64(y));
        }
        //Parser. Generate INT values from Strings
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
        //Calculate Poly's from Points
        private void GenerateFigures(string textbox1, string textbox2)
        {
            
            figure1.Clear();
            figure2.Clear();

            List<IntPoint> massive1 = StringToInt(textbox1);
            List<IntPoint> massive2 = StringToInt(textbox2);
            Polygon polygon1 = new Polygon(massive1.Count);
            for (int i = 0; i < massive1.Count; ++i)
                polygon1.Add(massive1[i]);
            figure1 = polygon1;

            Polygon polygon2 = new Polygon(massive2.Count);
            for (int i = 0; i < massive2.Count; ++i)
                polygon2.Add(massive2[i]);
            figure2 = polygon2;
        }
        private void bRefresh_Click(object sender, EventArgs e)
        {
            DrawBitmap();
        }
        //Generete Points array from single Points.
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

        //Here comes Drawing
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
                    float scale = trackBar1.Value;
                    PointF[] pts = PolygonToPointFArray(figure1, scale);
                    path.AddPolygon(pts);
                    pts = null;
                    //draw subjects ...
                    using (Pen myPen = new Pen(Color.FromArgb(196, 0xC3, 0xC9, 0xCF), (float)0.6))
                    using (SolidBrush myBrush = new SolidBrush(Color.FromArgb(127, 0xDD, 0xDD, 0xF0)))
                    {
                        newgraphic.FillPath(myBrush, path);
                        newgraphic.DrawPath(myPen, path);
                        path.Reset();

                        //draw clips ...
                        path.FillMode = FillMode.Winding;
                        pts = PolygonToPointFArray(figure2, scale);
                        path.AddPolygon(pts);
                        pts = null;
                        myPen.Color = Color.FromArgb(196, 0xF9, 0xBE, 0xA6);
                        myBrush.Color = Color.FromArgb(127, 0xFF, 0xE0, 0xE0);
                        newgraphic.FillPath(myBrush, path);
                        newgraphic.DrawPath(myPen, path);

                        solution = Clipper.MinkowskiSum(figure1, figure2, false);
                        path.FillMode = FillMode.Winding;
                        foreach (Polygon poly in solution)
                        {
                            pts = PolygonToPointFArray(poly, scale);
                            path.AddPolygon(pts);
                            pts = null;
                        }
                        myPen.Color = Color.FromArgb(196, 0xF9, 0xBE, 0xA6);
                        myBrush.Color = Color.FromArgb(127, 0xFE, 0x04, 0x00);
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

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
