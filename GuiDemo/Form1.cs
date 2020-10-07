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
using System.Drawing.Text;

namespace WindowsFormsApplication1
{
    using Polygon = List<IntPoint>;
    using Polygons = List<List<IntPoint>>;

    public partial class Form1 : Form
    {
        public void InstantiateMyNumericUpDown()
        {
            // Create and initialize a NumericUpDown control.

            // Dock the control to the top of the form.

            // Set the Minimum, Maximum, and initial Value.
            numericUpDown2.Value = 1;
            numericUpDown2.Maximum = 100;
            numericUpDown2.Minimum = 1;

            // Add the NumericUpDown to the Form.
            Controls.Add(numericUpDown1);
        }

        Random rand = new Random();
        private Bitmap mybitmap;
        private Polygon figure1 = new Polygon();
        private Polygon figure2 = new Polygon();
        private Polygons solution = new Polygons();
        private Polygons cubes = new Polygons();
        private GraphicsPath pathRandomCubes = new GraphicsPath();
        private GraphicsPath pathSolution = new GraphicsPath();
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
        private Polygons GenerateRandomCubes(int countObs)
        {
            int scale = trackBar1.Value;
            Polygons obs = new Polygons();
            int height = rand.Next(0, pictureBox1.Width * scale / 10);
            int width = rand.Next(0, pictureBox1.Width * scale / 10);
            int x;
            int y;
            for (int i = 0; i < countObs; i++)
            {
                Polygon cube = new Polygon();
                x = rand.Next(0, pictureBox1.Width * scale);
                y = rand.Next(0, pictureBox1.Height * scale);
                cube.Add(GeneratePoint(x, y));
                x = x + rand.Next(0, pictureBox1.Width * scale / 10);
                cube.Add(GeneratePoint(x, y));
                y = y + rand.Next(0, pictureBox1.Width * scale / 10);
                cube.Add(GeneratePoint(x, y));
                x = x - rand.Next(0, pictureBox1.Width * scale / 10);
                cube.Add(GeneratePoint(x, y));
                obs.Add(cube);
            }
            return obs;
        }
        private void GenerateFigures(string textbox1, string textbox2)
        {

            figure1.Clear();
            figure2.Clear();

            Polygon massive1 = StringToInt(textbox1);
            Polygon massive2 = StringToInt(textbox2);
            for (int i = 0; i < massive1.Count; ++i)
                figure1.Add(massive1[i]);
            for (int i = 0; i < massive2.Count; ++i)
                figure2.Add(massive2[i]);
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

        private void DrawBitmap(bool newGenerateCubes, bool JustClip = false)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using (Graphics newgraphic = Graphics.FromImage(mybitmap))
                {

                    pathSolution = SumMinkowski();
                    Pen myPen = new Pen(Color.FromArgb(196, 0xEF, 0xBE, 0xA6));
                    SolidBrush myBrush = new SolidBrush(Color.FromArgb(127, 0xEF, 0xE0, 0xE0));
                    newgraphic.SmoothingMode = SmoothingMode.AntiAlias;
                    newgraphic.Clear(Color.White);
                    newgraphic.FillPath(myBrush, pathSolution);
                    newgraphic.DrawPath(myPen, pathSolution);

                    if (newGenerateCubes)
                    {
                        pathRandomCubes = RandomFigures();
                        myPen = new Pen(Color.FromArgb(196, 0xEF, 0xBE, 0xA6));
                        myBrush = new SolidBrush(Color.FromArgb(127, 0xEF, 0xE0, 0xE0));
                        newgraphic.FillPath(myBrush, pathRandomCubes);
                        newgraphic.DrawPath(myPen, pathRandomCubes);
                    }

                    GraphicsPath path = DoClipping();
                    myPen = new Pen(Color.FromArgb(255, 0x00, 0x00, 0x00));
                    myBrush = new SolidBrush(Color.FromArgb(255, 0xFF, 0xFF, 0xFF));
                    newgraphic.FillPath(myBrush, path);
                    newgraphic.DrawPath(myPen, path);
                }
                pictureBox1.Image = mybitmap;

            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        //Here comes Drawing
        private GraphicsPath SumMinkowski(bool justClip = false)
        {

            GenerateFigures(textBox1.Text, textBox2.Text);
            pathSolution.FillMode = FillMode.Winding;
            float scale = trackBar1.Value;
            PointF[] pts;
            GraphicsPath path = new GraphicsPath();
            //draw subjects ...
            //Minkovski w/ Timer
            if (numericUpDown1.Value != 0)
            {
                solution = Clipper.MinkowskiSum(figure1, figure2, false);
                for (int i = 0; i < numericUpDown1.Value; i++)
                {
                    solution = Clipper.SimplifyPolygons(solution);
                    solution = Clipper.MinkowskiSum(figure2, solution, true);

                }
                path.FillMode = FillMode.Winding;
                foreach (Polygon poly in solution)
                {
                    pts = PolygonToPointFArray(poly, scale);
                    path.AddPolygon(pts);
                    pts = null;
                }
            }
            return path;
        }
        private GraphicsPath DoClipping(bool justClip = false)
        {
            float scale = trackBar1.Value;
            Polygons solution_2 = new Polygons();
            Clipper c = new Clipper();
            c.AddPaths(solution, PolyType.ptSubject, true);
            c.AddPaths(cubes, PolyType.ptClip, true);
            GraphicsPath path = new GraphicsPath();
            path.FillMode = FillMode.Winding;
            c.Execute(ClipType.ctIntersection, solution_2, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
            foreach (Polygon poly in solution_2)
            {
                PointF[] pts = PolygonToPointFArray(poly, scale);
                path.AddPolygon(pts);
                pts = null;
            }
            return path;

        }
        private GraphicsPath RandomFigures(bool justClip = false)
        {
            float scale = trackBar1.Value;
            GraphicsPath path = new GraphicsPath();
            //Convert Obs to ints
            int count = Convert.ToInt32(numericUpDown2.Value);
            cubes = GenerateRandomCubes(count);
            foreach (Polygon poly in cubes)
            {
                PointF[] pts = PolygonToPointFArray(poly, scale);
                path.AddPolygon(pts);
                pts = null;
            }
            return path;
        }

        private void bRefresh_Click(object sender, EventArgs e)
        {
            pathSolution.Reset();
            DrawBitmap(false);
        }

        private void bRefresh_Click2(object sender, EventArgs e)
        {

            pathRandomCubes.Reset();
            DrawBitmap(true);
        }
        private void Form1_Load(object sender, EventArgs e)
        { 
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
        

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
