using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using Point = System.Drawing.Point;
using System;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.CodeDom;
using System.Runtime.InteropServices.WindowsRuntime;

namespace CG_Project_3
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyImage.Source = BitmapToImageSource(Mybitmap);
        }
        public class Line
        {
            public Point P1;
            public Point P2;
            public int Thickness;
            public Color color;

            public Line(Point point, Point point2, int th, Color col)
            {
                P1 = point;
                P2 = point2;
                Thickness = th;
                color = col;
            }
        }
        public class Circle
        {
            public Point Center;
            public int Radius;
            public Color color;

            public Circle(Point point, int rad, Color col)
            {
                Center = point;
                Radius = rad;
                color = col;
            }
        }
        public class Polygon
        {
            public List<Point> vertices;
            public int Thickness;
            public Color color;

            public Polygon(List<Point> ver, int th, Color col)
            {
                vertices = ver;
                Thickness = th;
                color = col;
            }
        }
        Color currcol = Color.FromArgb(255, 255, 255, 255);
        static int bitsize = 700;
        Bitmap Mybitmap = new Bitmap(bitsize, bitsize);
        private List<Line> Lines = new List<Line> { };
        private List<Circle> Circles = new List<Circle> { };
        private List<Polygon> Polygons = new List<Polygon> { };
        // Points for the new line.
        private bool IsDrawing;
        private Line NewLine;
        //private bool DrawingLine = false; 
        //private bool DrawingCircle = false; 
        //private bool DrawingPoly = false;

        private int MovingSegment;
        private int movingpoly;
        private int movingpolyseg;
        private bool MovingStartEndPoint;
        private double over_dist_squared = 10;






        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (Lines.Count == 0 && Circles.Count == 0 && Polygons.Count == 0)
                return;
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Drawings";
            dialog.Filter = "Text Files (*.txt)|*.txt";
            dialog.DefaultExt = "txt";
            dialog.AddExtension = true;
            string filetext = "";
            if (dialog.ShowDialog().Value)
            {
                foreach (Line line in Lines)
                {
                    filetext += "L " + line.P1 + " " + line.P2 + " " + line.Thickness + " " + line.color.ToString() + "\r\n";
                }
                foreach (Circle circle in Circles)
                {
                    filetext += "C " + circle.Center + " " + circle.Radius + " " + circle.color.ToString() + "\r\n";
                }
                foreach (Polygon poly in Polygons)
                {
                    filetext += "P ";
                    foreach (Point verti in poly.vertices)
                    {
                        filetext += verti + " ";
                    }


                    filetext += poly.Thickness + " " + poly.color.ToString() + "\r\n";
                }



                File.WriteAllText(dialog.FileName, filetext);

            }

        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Filters";
            dialog.Title = "Select a Photo";
            string path = " ";
            dialog.Filter = "All supported files|*.txt;";

            if (dialog.ShowDialog() == true)
            {
                path = dialog.FileName;
                ParseThroughFile(path);
                Canvas_Paint(Canvas, new EventArgs());
            }

        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Lines.Clear();
            Circles.Clear();
            Polygons.Clear();


            Canvas_Paint(Canvas, new EventArgs());
        }

        private void ParseThroughFile(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            List<string> items = new List<string>();
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');


                if (words[0] == "L")
                {

                    string[] point1 = words[1].Split('=');
                    int p1x = int.Parse(point1[1].Trim('[', 'Y', '=', ','));
                    int p1y = int.Parse(point1[2].Trim('}', 'Y', '=', ','));
                    string[] point2 = words[2].Split('=');
                    int p2x = int.Parse(point2[1].Trim('[', 'Y', '=', ','));
                    int p2y = int.Parse(point2[2].Trim('}', 'Y', '=', ','));
                    int acol = int.Parse(words[5].Trim('[', 'A', '=', ','));
                    int rcol = int.Parse(words[6].Trim('[', 'R', '=', ','));
                    int gcol = int.Parse(words[7].Trim('[', 'G', '=', ','));
                    int bcol = int.Parse(words[8].Trim(']', 'B', '=', ','));
                    Color coloo = Color.FromArgb(acol, rcol, gcol, bcol);
                    Line FileLine = new Line(new Point(p1x, p1y), new Point(p2x, p2y), int.Parse(words[3]), coloo);
                    Lines.Add(FileLine);
                }
                else if (words[0] == "C")
                {
                    string[] point1 = words[1].Split('=');
                    int p1x = int.Parse(point1[1].Trim('[', 'Y', '=', ','));
                    int p1y = int.Parse(point1[2].Trim('}', 'Y', '=', ','));

                    int acol = int.Parse(words[4].Trim('[', 'A', '=', ','));
                    int rcol = int.Parse(words[5].Trim('[', 'R', '=', ','));
                    int gcol = int.Parse(words[6].Trim('[', 'G', '=', ','));
                    int bcol = int.Parse(words[7].Trim(']', 'B', '=', ','));
                    Color coloo = Color.FromArgb(acol, rcol, gcol, bcol);
                    Circle FileCircle = new Circle(new Point(p1x, p1y), int.Parse(words[2]), coloo);
                    Circles.Add(FileCircle);

                }
                else if (words[0] == "P")
                {
                    List<Point> verti = new List<Point> { };
                    for (int k = 1; k < words.Length - 6; k++)
                    {
                        string[] point1 = words[k].Split('=');
                        int p1x = int.Parse(point1[1].Trim('[', 'Y', '=', ','));
                        int p1y = int.Parse(point1[2].Trim('}', 'Y', '=', ','));
                        verti.Add(new Point(p1x, p1y));
                    }
                    int thi = int.Parse(words[words.Length - 6]);
                    int acol = int.Parse(words[words.Length - 4].Trim('[', 'A', '=', ','));
                    int rcol = int.Parse(words[words.Length - 3].Trim('[', 'R', '=', ','));
                    int gcol = int.Parse(words[words.Length - 2].Trim('[', 'G', '=', ','));
                    int bcol = int.Parse(words[words.Length - 1].Trim(']', 'B', '=', ','));
                    Color coloo = Color.FromArgb(acol, rcol, gcol, bcol);

                    Polygon FilePoly = new Polygon(verti, thi, coloo);
                    Polygons.Add(FilePoly);


                }


            }
        }

        private void AntiMidpointCircle(Circle circle)
        {
            Color L = circle.color; /*Line color*/
            Color B = Color.Black; /*Background Color*/
            int x = circle.Radius;
            int y = 0;
            if (CircleCheck.IsChecked.Value)
            {
                for (int i = -1; i < 2; i++)
                {
                    if (circle.Center.X + i < 700 && circle.Center.X + i > 0 && circle.Center.Y + i < 700 && circle.Center.Y + i > 0)
                        Mybitmap.SetPixel(circle.Center.X + i, circle.Center.Y + i, Color.Red);
                    if (circle.Center.X + 3 - i < 700 && circle.Center.X + 3 - i > 0 && circle.Center.Y + i < 700 && circle.Center.Y + i > 0)
                        Mybitmap.SetPixel(circle.Center.X + 3 - i, circle.Center.Y + i, Color.Red);
                }
            }
            if (circle.Center.X + x < 700 && circle.Center.X + x > 0 && circle.Center.Y + y < 700 && circle.Center.Y + y > 0)
                Mybitmap.SetPixel(circle.Center.X + x, circle.Center.Y + y, L);
            if (circle.Center.X - x < 700 && circle.Center.X - x > 0 && circle.Center.Y + y < 700 && circle.Center.Y + y > 0)
                Mybitmap.SetPixel(circle.Center.X - x, circle.Center.Y + y, L);

            if (circle.Center.X + y < 700 && circle.Center.X + y > 0 && circle.Center.Y + x < 700 && circle.Center.Y + x > 0)
                Mybitmap.SetPixel(circle.Center.X + y, circle.Center.Y + x, L);
            if (circle.Center.X + y < 700 && circle.Center.X + y > 0 && circle.Center.Y - x < 700 && circle.Center.Y - x > 0)
                Mybitmap.SetPixel(circle.Center.X + y, circle.Center.Y - x, L);

            while (x > y)
            {
                
                ++y;
                x = Convert.ToInt32(Math.Ceiling(Math.Sqrt(circle.Radius * circle.Radius - y * y)));
                float T = (float)(Math.Ceiling(Math.Sqrt(circle.Radius * circle.Radius - y * y)) - Math.Sqrt(circle.Radius * circle.Radius - y * y));
                Color c2 = Color.FromArgb(Convert.ToInt32(L.A * (1-T)+B.A *T), Convert.ToInt32(L.R * (1 - T) + B.R * T), Convert.ToInt32(L.G * (1 - T) + B.G * T), Convert.ToInt32(L.G * (1 - T) + B.G * T));
                Color c1 = Color.FromArgb(Convert.ToInt32(B.A * (1 - T) + L.A * T), Convert.ToInt32(B.R * (1 - T) + L.R * T), Convert.ToInt32(B.G * (1 - T) + L.G * T), Convert.ToInt32(B.G * (1 - T) + L.G * T));
                if (circle.Center.X + x < 700 && circle.Center.X + x > 0 && circle.Center.Y + y < 700 && circle.Center.Y + y > 0)
                    Mybitmap.SetPixel(circle.Center.X + x, circle.Center.Y + y, c2);

                if (circle.Center.X + y < 700 && circle.Center.X + y > 0 && circle.Center.Y + x < 700 && circle.Center.Y + x > 0)
                    Mybitmap.SetPixel(circle.Center.X + y, circle.Center.Y + x, c2);

                if (circle.Center.X - y < 700 && circle.Center.X - y > 0 && circle.Center.Y + x < 700 && circle.Center.Y + x > 0)
                    Mybitmap.SetPixel(circle.Center.X - y, circle.Center.Y + x, c2);

                if (circle.Center.X - x < 700 && circle.Center.X - x > 0 && circle.Center.Y + y < 700 && circle.Center.Y + y > 0)
                    Mybitmap.SetPixel(circle.Center.X - x, circle.Center.Y + y, c2);

                if (circle.Center.X - x < 700 && circle.Center.X - x > 0 && circle.Center.Y - y < 700 && circle.Center.Y - y > 0)
                    Mybitmap.SetPixel(circle.Center.X - x, circle.Center.Y - y, c2);

                if (circle.Center.X - y < 700 && circle.Center.X - y > 0 && circle.Center.Y - x < 700 && circle.Center.Y - x > 0)
                    Mybitmap.SetPixel(circle.Center.X - y, circle.Center.Y - x, c2);

                if (circle.Center.X + y < 700 && circle.Center.X + y > 0 && circle.Center.Y - x < 700 && circle.Center.Y - x > 0)
                    Mybitmap.SetPixel(circle.Center.X + y, circle.Center.Y - x, c2);

                if (circle.Center.X + x < 700 && circle.Center.X + x > 0 && circle.Center.Y - y < 700 && circle.Center.Y - y > 0)
                    Mybitmap.SetPixel(circle.Center.X + x, circle.Center.Y - y, c2);




                if (circle.Center.X + x < 700 && circle.Center.X + x > 0 && circle.Center.Y + y < 700 && circle.Center.Y + y > 0)
                    Mybitmap.SetPixel(circle.Center.X + x -1, circle.Center.Y + y, c1);

                if (circle.Center.X + y < 700 && circle.Center.X + y > 0 && circle.Center.Y + x < 700 && circle.Center.Y + x > 0)
                    Mybitmap.SetPixel(circle.Center.X + y, circle.Center.Y + x -1, c1);

                if (circle.Center.X - y < 700 && circle.Center.X - y > 0 && circle.Center.Y + x < 700 && circle.Center.Y + x > 0)
                    Mybitmap.SetPixel(circle.Center.X - y, circle.Center.Y + x -1, c1);

                if (circle.Center.X - x < 700 && circle.Center.X - x > 0 && circle.Center.Y + y < 700 && circle.Center.Y + y > 0)
                    Mybitmap.SetPixel(circle.Center.X - x + 1, circle.Center.Y + y, c1);

                if (circle.Center.X - x < 700 && circle.Center.X - x > 0 && circle.Center.Y - y < 700 && circle.Center.Y - y > 0)
                    Mybitmap.SetPixel(circle.Center.X - x+1, circle.Center.Y - y, c1);

                if (circle.Center.X - y < 700 && circle.Center.X - y > 0 && circle.Center.Y - x < 700 && circle.Center.Y - x > 0)
                    Mybitmap.SetPixel(circle.Center.X - y, circle.Center.Y - x +1, c1);

                if (circle.Center.X + y < 700 && circle.Center.X + y > 0 && circle.Center.Y - x < 700 && circle.Center.Y - x > 0)
                    Mybitmap.SetPixel(circle.Center.X + y, circle.Center.Y - x+1, c1);

                if (circle.Center.X + x < 700 && circle.Center.X + x > 0 && circle.Center.Y - y < 700 && circle.Center.Y - y > 0)
                    Mybitmap.SetPixel(circle.Center.X + x-1, circle.Center.Y - y, c1);


            }


           
        }
        private void MidpointCircle(Circle circle)
        {
            if (AntiAliasing.IsChecked.Value)
            {
                AntiMidpointCircle(circle);
                return;
            }

            if (circle == null)
                return;
            int dE = 3;
            int dSE = 5 - 2 * circle.Radius;
            int d = 1 - circle.Radius;
            int x = 0;
            int y = circle.Radius;

            if (CircleCheck.IsChecked.Value)
            {
                for (int i = -1; i < 2; i++)
                {
                    if (circle.Center.X + i < 700 && circle.Center.X + i > 0 && circle.Center.Y + i < 700 && circle.Center.Y + i > 0)
                        Mybitmap.SetPixel(circle.Center.X + i, circle.Center.Y + i, Color.Red);
                    if (circle.Center.X + 3 - i < 700 && circle.Center.X + 3 - i > 0 && circle.Center.Y + i < 700 && circle.Center.Y + i > 0)
                        Mybitmap.SetPixel(circle.Center.X + 3 - i, circle.Center.Y + i, Color.Red);
                }
            }
            if (circle.Center.X + y < 700 && circle.Center.X + y > 0 && circle.Center.Y + x < 700 && circle.Center.Y + x > 0)
                Mybitmap.SetPixel(circle.Center.X + y, circle.Center.Y + x, circle.color);
            if (circle.Center.X - y < 700 && circle.Center.X - y > 0 && circle.Center.Y + x < 700 && circle.Center.Y + x > 0)
                Mybitmap.SetPixel(circle.Center.X - y, circle.Center.Y + x, circle.color);

            if (circle.Center.X + x < 700 && circle.Center.X + x > 0 && circle.Center.Y + y < 700 && circle.Center.Y + y > 0)
                Mybitmap.SetPixel(circle.Center.X + x, circle.Center.Y + y, circle.color);
            if (circle.Center.X + x < 700 && circle.Center.X + x > 0 && circle.Center.Y - y < 700 && circle.Center.Y - y > 0)
                Mybitmap.SetPixel(circle.Center.X + x, circle.Center.Y - y, circle.color);

            while (y > x)
            {
                if (d < 0) //move to E
                {
                    d += dE;
                    dE += 2;
                    dSE += 2;
                }
                else //move to SE
                {
                    d += dSE;
                    dE += 2;
                    dSE += 4;
                    --y;
                }
                ++x;
                if (circle.Center.X + x < 700 && circle.Center.X + x > 0 && circle.Center.Y + y < 700 && circle.Center.Y + y > 0)
                    Mybitmap.SetPixel(circle.Center.X + x, circle.Center.Y + y, circle.color);

                if (circle.Center.X + y < 700 && circle.Center.X + y > 0 && circle.Center.Y + x < 700 && circle.Center.Y + x > 0)
                    Mybitmap.SetPixel(circle.Center.X + y, circle.Center.Y + x, circle.color);

                if (circle.Center.X - y < 700 && circle.Center.X - y > 0 && circle.Center.Y + x < 700 && circle.Center.Y + x > 0)
                    Mybitmap.SetPixel(circle.Center.X - y, circle.Center.Y + x, circle.color);

                if (circle.Center.X - x < 700 && circle.Center.X - x > 0 && circle.Center.Y + y < 700 && circle.Center.Y + y > 0)
                    Mybitmap.SetPixel(circle.Center.X - x, circle.Center.Y + y, circle.color);

                if (circle.Center.X - x < 700 && circle.Center.X - x > 0 && circle.Center.Y - y < 700 && circle.Center.Y - y > 0)
                    Mybitmap.SetPixel(circle.Center.X - x, circle.Center.Y - y, circle.color);

                if (circle.Center.X - y < 700 && circle.Center.X - y > 0 && circle.Center.Y - x < 700 && circle.Center.Y - x > 0)
                    Mybitmap.SetPixel(circle.Center.X - y, circle.Center.Y - x, circle.color);

                if (circle.Center.X + y < 700 && circle.Center.X + y > 0 && circle.Center.Y - x < 700 && circle.Center.Y - x > 0)
                    Mybitmap.SetPixel(circle.Center.X + y, circle.Center.Y - x, circle.color);

                if (circle.Center.X + x < 700 && circle.Center.X + x > 0 && circle.Center.Y - y < 700 && circle.Center.Y - y > 0)
                    Mybitmap.SetPixel(circle.Center.X + x, circle.Center.Y - y, circle.color);


            }
        }


        private void lineDDA(Line line)
        {
            if (AntiAliasing.IsChecked.Value)
            {
                AntilineDDA(line);
                return;
            }

            // calculate dx & dy 
            int dx = line.P2.X - line.P1.X;
            int dy = line.P2.Y - line.P1.Y;

            // calculate steps required for generating pixels 
            int steps = Math.Abs(dx) > Math.Abs(dy) ? Math.Abs(dx) : Math.Abs(dy);

            // calculate increment in x & y for each steps 
            float Xinc = dx / (float)steps;
            float Yinc = dy / (float)steps;

            // Put pixel for each step 
            float X = line.P1.X;

            float Y = line.P1.Y;
            for (int i = 0; i <= steps; i++)
            {
                if (line.Thickness % 2 == 1)
                {
                    for (int j = 0; j <= (line.Thickness) / 2; j++)
                    {
                        if (Math.Abs(dx) > Math.Abs(dy))
                        {
                            if (Math.Round(X) < 700 && Math.Round(X) > 0 && Math.Round(Y - j) < 700 && Math.Round(Y - j) > 0)
                            {
                                Mybitmap.SetPixel(Convert.ToInt32(Math.Round(X)), Convert.ToInt32(Math.Round(Y - j)), line.color);  // put pixel at (X,Y) 

                            }
                            if (Math.Round(X) < 700 && Math.Round(X) > 0 && Math.Round(Y + j) < 700 && Math.Round(Y + j) > 0)
                            {
                                Mybitmap.SetPixel(Convert.ToInt32(Math.Round(X)), Convert.ToInt32(Math.Round(Y + j)), line.color);  // put pixel at (X,Y) 

                            }
                        }

                        else
                        {
                            if (Math.Round(X - j) < 700 && Math.Round(X - j) > 0 && Math.Round(Y) < 700 && Math.Round(Y) > 0)
                            {
                                Mybitmap.SetPixel(Convert.ToInt32(Math.Round(X - j)), Convert.ToInt32(Math.Round(Y)), line.color);  // put pixel at (X,Y) 
                            }
                            if (Math.Round(X + j) < 700 && Math.Round(X + j) > 0 && Math.Round(Y) < 700 && Math.Round(Y) > 0)
                            {
                                Mybitmap.SetPixel(Convert.ToInt32(Math.Round(X + j)), Convert.ToInt32(Math.Round(Y)), line.color);  // put pixel at (X,Y) 
                            }
                        }
                    }
                }
                else
                    for (int j = 0; j <= (line.Thickness) / 2; j++)
                    {
                        if (Math.Abs(dx) > Math.Abs(dy))
                        {
                            if (Math.Round(X) < 700 && Math.Round(X) > 0 && Math.Round(Y - j - 1 / 2) < 700 && Math.Round(Y - j - 1 / 2) > 0)
                            {
                                Mybitmap.SetPixel(Convert.ToInt32(Math.Round(X)), Convert.ToInt32(Math.Round(Y - j - 1 / 2)), line.color);  // put pixel at (X,Y) 

                            }
                            if (Math.Round(X) < 700 && Math.Round(X) > 0 && Math.Round(Y + j - 1 / 2) < 700 && Math.Round(Y + j - 1 / 2) > 0)
                            {
                                Mybitmap.SetPixel(Convert.ToInt32(Math.Round(X)), Convert.ToInt32(Math.Round(Y + j - 1 / 2)), line.color);  // put pixel at (X,Y) 

                            }
                        }

                        else
                        {
                            if (Math.Round(X - j - 1 / 2) < 700 && Math.Round(X - j - 1 / 2) > 0 && Math.Round(Y) < 700 && Math.Round(Y) > 0)
                            {
                                Mybitmap.SetPixel(Convert.ToInt32(Math.Round(X - j - 1 / 2)), Convert.ToInt32(Math.Round(Y)), line.color);  // put pixel at (X,Y) 
                            }
                            if (Math.Round(X + j - 1 / 2) < 700 && Math.Round(X + j - 1 / 2) > 0 && Math.Round(Y) < 700 && Math.Round(Y) > 0)
                            {
                                Mybitmap.SetPixel(Convert.ToInt32(Math.Round(X + j - 1 / 2)), Convert.ToInt32(Math.Round(Y)), line.color);  // put pixel at (X,Y) 
                            }
                        }
                    }


                X += Xinc;           // increment in x at each step 
                Y += Yinc;           // increment in y at each step 
            }


        }
        private void AntilineDDA(Line line)
        {
            Color L = line.color; /*Line color*/
            Color B = Color.Black; /*Background Color*/
           
            int dx = line.P2.X - line.P1.X;
            int dy = line.P2.Y - line.P1.Y;

            // calculate steps required for generating pixels 
            int steps = Math.Abs(dx) > Math.Abs(dy) ? Math.Abs(dx) : Math.Abs(dy);

            // calculate increment in x & y for each steps 
            float Xinc = dx / (float)steps;
            float Yinc = dy / (float)steps;

            // Put pixel for each step 
            float X = line.P1.X;
            float Y = line.P1.Y;

            for (int i = 0; i <= steps; i++)
            {
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    float T = (float)(Y - Math.Floor(Y));
                    Color c1 = Color.FromArgb(Convert.ToInt32(L.A * (1 - T) + B.A * T), Convert.ToInt32(L.R * (1 - T) + B.R * T), Convert.ToInt32(L.G * (1 - T) + B.G * T), Convert.ToInt32(L.G * (1 - T) + B.G * T));
                    Color c2 = Color.FromArgb(Convert.ToInt32(B.A * (1 - T) + L.A * T), Convert.ToInt32(B.R * (1 - T) + L.R * T), Convert.ToInt32(B.G * (1 - T) + L.G * T), Convert.ToInt32(B.G * (1 - T) + L.G * T));

                    Mybitmap.SetPixel(Convert.ToInt32(X), Convert.ToInt32(Math.Floor(Y)), c1);
                    Mybitmap.SetPixel(Convert.ToInt32(X), Convert.ToInt32(Math.Floor(Y)) + 1, c2);
                }
                else
                {
                    float T = (float)(X - Math.Floor(X));
                    Color c1 = Color.FromArgb(Convert.ToInt32(L.A * (1 - T) + B.A * T), Convert.ToInt32(L.R * (1 - T) + B.R * T), Convert.ToInt32(L.G * (1 - T) + B.G * T), Convert.ToInt32(L.G * (1 - T) + B.G * T));
                    Color c2 = Color.FromArgb(Convert.ToInt32(B.A * (1 - T) + L.A * T), Convert.ToInt32(B.R * (1 - T) + L.R * T), Convert.ToInt32(B.G * (1 - T) + L.G * T), Convert.ToInt32(B.G * (1 - T) + L.G * T));

                    Mybitmap.SetPixel(Convert.ToInt32(Math.Floor(X)), Convert.ToInt32(Y), c1);
                    Mybitmap.SetPixel(Convert.ToInt32(Math.Floor(X)) + 1, Convert.ToInt32(Y), c2);
                }

                X += Xinc;           // increment in x at each step 
                Y += Yinc;           // increment in y at each step 
            }
        }

    
        private void PolyDDA(Polygon poly)
        {
            if (AntiAliasing.IsChecked.Value)
            {
                for (int i = 0; i < poly.vertices.Count; i++)
                {
                    Line line = new Line(poly.vertices[i], poly.vertices[(i + 1) % (poly.vertices.Count)], poly.Thickness, poly.color);
                    AntilineDDA(line);
                }
            }
            else
            {
                for (int i = 0; i < poly.vertices.Count; i++)
                {
                    Line line = new Line(poly.vertices[i], poly.vertices[(i + 1) % (poly.vertices.Count)], poly.Thickness, poly.color);
                    lineDDA(line);
                }
            }
        }
        private void DrawingPolyDDA(Polygon poly)
        {
            if (AntiAliasing.IsChecked.Value)
            {
                for (int i = 0; i < poly.vertices.Count - 1; i++)
                {
                    Line line = new Line(poly.vertices[i], poly.vertices[(i + 1) % (poly.vertices.Count)], poly.Thickness, poly.color);
                    AntilineDDA(line);
                }
            }
            else
            {
                for (int i = 0; i < poly.vertices.Count - 1; i++)
                {
                    Line line = new Line(poly.vertices[i], poly.vertices[(i + 1) % (poly.vertices.Count)], poly.Thickness, poly.color);
                    lineDDA(line);
                }
            }
        }
        // Draw the lines.
        private void Canvas_Paint(object sender, EventArgs e)
        {
            // Draw the segments.
            Mybitmap = new Bitmap(bitsize, bitsize);

            for (int i = 0; i < Lines.Count; i++)
            {
                // Draw the line.
                lineDDA(Lines[i]);
            }

            for (int i = 0; i < Circles.Count; i++)
            {
                // Draw the circle.
                MidpointCircle(Circles[i]);
            }

            for (int i = 0; i < Polygons.Count; i++)
            {
                // Draw the line.
                PolyDDA(Polygons[i]);
            }

            for (int i = 680; i < 700; i++)
            {
                Mybitmap.SetPixel(i, i, Color.Red);
                Mybitmap.SetPixel(1379 - i, i, Color.Red);

            }
            if (IsDrawing)
            {

                if (LineCheck.IsChecked.Value)
                    lineDDA(NewLine);
                else if (CircleCheck.IsChecked.Value)
                {
                    MidpointCircle(NewCircle);
                }
                else if (PolyCheck.IsChecked.Value)
                {
                    DrawingPolyDDA(NewPolygon);
                }
            }

            MyImage.Source = BitmapToImageSource(Mybitmap);

        }
        // The mouse is up. See whether we're over an end point or segment.
        private void Canvas_MouseMove_NotDown(object sender, EventArgs e)
        {


            System.Windows.Input.Cursor new_cursor = System.Windows.Input.Cursors.Cross;

            int x = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).X);
            int y = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).Y);

            positionx.Content = "X:   " + x.ToString() + " px";
            positiony.Content = "Y:   " + y.ToString() + " px";

            // See what we're over.
            Point hit_point;
            int segment_number;

            if (MouseIsOverEndpoint(x, y, out segment_number,
                out hit_point))
                new_cursor = System.Windows.Input.Cursors.Arrow;

            else if (MouseIsOverSegment(x, y, out segment_number))
                new_cursor = System.Windows.Input.Cursors.Hand;
            //Set the new cursor.
            if (Canvas.Cursor != new_cursor)
                Canvas.Cursor = new_cursor;

        }
        int OffsetX;
        int OffsetY;
        // See what we're over and start doing whatever is appropriate.
        int moving_circle_number;
        private void Canvas_MouseDown(object sender, EventArgs e)
        {
            // See what we're over.
            Point hit_point;
            int segment_number;

            int x = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).X);
            int y = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).Y);



            if (MouseIsOverEndpoint(x, y, out segment_number, out hit_point))
            {
                Canvas.MouseMove -= Canvas_MouseMove_NotDown;
                Canvas.MouseMove += Canvas_MouseMove_MovingEndPoint;
                Canvas.MouseUp += Canvas_MouseUp_MovingEndPoint;
                OffsetX = hit_point.X - x;
                OffsetY = hit_point.Y - y;
                if (LineCheck.IsChecked.Value)
                {

                    // Remember the segment number.
                    MovingSegment = segment_number;
                    if (ColorCheck.IsChecked.Value)
                        Lines[segment_number].color = currcol;
                    if (ChangeLineThickness.IsChecked.Value)
                        Lines[segment_number].Thickness = Convert.ToInt32(LineThickness.Value);
                    // See if we're moving the start end point.
                    MovingStartEndPoint = (Lines[segment_number].P1.Equals(hit_point));

                    // Remember the offset from the mouse to the point.

                }
                else if (PolyCheck.IsChecked.Value)
                {
                    movingpoly = poly_number;
                    movingpolyseg = poly_segment_number;

                    // Remember the segment number.
                    if (ColorCheck.IsChecked.Value)
                        Polygons[poly_number].color = currcol;
                    if (ChangePolyThickness.IsChecked.Value)
                        Polygons[poly_number].Thickness = Convert.ToInt32(PolyThickness.Value);
                    // See if we're moving the start end point.

                    // Remember the offset from the mouse to the point.

                }
            }
            else if (MouseIsOverSegment(x, y, out segment_number))
            {
                Canvas.MouseMove -= Canvas_MouseMove_NotDown;
                Canvas.MouseMove += Canvas_MouseMove_MovingSegment;
                Canvas.MouseUp += Canvas_MouseUp_MovingSegment;
                if (LineCheck.IsChecked.Value)
                {
                    // Start moving this segment.


                    // Remember the segment number.
                    MovingSegment = segment_number;
                    if (ColorCheck.IsChecked.Value)
                        Lines[segment_number].color = currcol;
                    if (ChangeLineThickness.IsChecked.Value)
                        Lines[segment_number].Thickness = Convert.ToInt32(LineThickness.Value);
                    // Remember the offset from the mouse
                    // to the segment's first point.
                    OffsetX = Lines[segment_number].P1.X - x;
                    OffsetY = Lines[segment_number].P1.Y - y;
                }
                else if (PolyCheck.IsChecked.Value)
                {
                    // Start moving this segment.


                    // Remember the segment number.
                    movingpoly = poly_number;
                    movingpolyseg = poly_segment_number;

                    if (ColorCheck.IsChecked.Value)
                        Polygons[poly_number].color = currcol;
                    if (ChangePolyThickness.IsChecked.Value)
                        Polygons[poly_number].Thickness = Convert.ToInt32(PolyThickness.Value);
                    // Remember the offset from the mouse
                    // to the segment's first point.
                    OffsetX = Polygons[poly_number].vertices[poly_segment_number].X - x;
                    OffsetY = Polygons[poly_number].vertices[poly_segment_number].Y - y;
                }
                else if (CircleCheck.IsChecked.Value)
                {

                    // Remember the segment number.
                    moving_circle_number = circle_number;
                    if (ColorCheck.IsChecked.Value)
                        Circles[circle_number].color = currcol;
                    if (ChangeRadius.IsChecked.Value)
                        Circles[circle_number].Radius = Convert.ToInt32(CircleRadiuss.Value);
                    // See if we're moving the start end point.
                    // Remember the offset from the mouse to the point.
                    OffsetX = Circles[circle_number].Center.X - x;
                    OffsetY = Circles[circle_number].Center.Y - y;
                }
            }
            else
            {
                if (LineCheck.IsChecked.Value)
                {
                    // Start drawing a new segment.
                    Canvas.MouseMove -= Canvas_MouseMove_NotDown;
                    Canvas.MouseMove += Canvas_MouseMove_Drawing;
                    Canvas.MouseUp += Canvas_MouseUp_Drawing;

                    IsDrawing = true;
                    NewLine = new Line(new Point(x, y), new Point(x, y), Convert.ToInt32(LineThickness.Value), currcol);

                }
                else if (CircleCheck.IsChecked.Value)
                {
                    // Start drawing a new segment.
                    Canvas.MouseMove -= Canvas_MouseMove_NotDown;
                    Canvas.MouseMove += Canvas_MouseMove_Drawing;
                    Canvas.MouseUp += Canvas_MouseUp_Drawing;

                    IsDrawing = true;
                    NewCircle = new Circle(new Point(x, y), Convert.ToInt32(CircleRadiuss.Value), currcol);

                }
                else if (PolyCheck.IsChecked.Value)
                {
                    // Start drawing a new segment.
                    Canvas.MouseMove -= Canvas_MouseMove_NotDown;
                    Canvas.MouseMove += Canvas_MouseMove_Poly_Down;
                    Canvas.MouseDown -= Canvas_MouseDown;
                    Canvas.MouseDown += Canvas_MouseDown_Next_Poly;
                    Canvas.MouseUp += Canvas_MouseUp_Drawing_Poly;

                    IsDrawing = true;
                    NewPolygon = new Polygon(new List<Point> { new Point(x, y) }, Convert.ToInt32(PolyThickness.Value), currcol);
                    MidpointCircle(new Circle(NewPolygon.vertices[0], Convert.ToInt32(over_dist_squared), Color.Red));
                    MyImage.Source = BitmapToImageSource(Mybitmap);

                }



            }

        }
        Circle NewCircle;
        Polygon NewPolygon;
        // See if the mouse is over an end point.
        int poly_number;
        int poly_segment_number;

        private void Canvas_MouseMove_Poly_Down(object sender, EventArgs e)
        {
            int x = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).X);
            int y = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).Y);
            // Save the new point.
            NewPolygon.vertices[NewPolygon.vertices.Count - 1] = new Point(x, y);

            // Redraw.
            Canvas_Paint(Canvas, new EventArgs());
            test.Content = NewPolygon.vertices.Count;
            MidpointCircle(new Circle(NewPolygon.vertices[0], Convert.ToInt32(over_dist_squared), Color.Red));
            MyImage.Source = BitmapToImageSource(Mybitmap);

        }
        private void Canvas_MouseMove_Poly_Up(object sender, EventArgs e)
        {


        }
        private void Canvas_MouseUp_Drawing_Poly(object sender, EventArgs e)
        {

            int x = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).X);
            int y = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).Y);
            Point mouse_pt = new Point(x, y);


            if (FindDistanceToPointSquared(mouse_pt, NewPolygon.vertices[0]) < Math.Pow(over_dist_squared, 2) && NewPolygon.vertices.Count >= 3)
            {
                NewPolygon.vertices.RemoveAt(NewPolygon.vertices.Count - 1);
                Polygons.Add(NewPolygon);
                IsDrawing = false;
                test.Content = "now";
                Canvas.MouseMove -= Canvas_MouseMove_Poly_Down;
                Canvas.MouseMove += Canvas_MouseMove_NotDown;
                Canvas.MouseDown -= Canvas_MouseDown_Next_Poly;
                Canvas.MouseDown += Canvas_MouseDown;
                Canvas.MouseUp -= Canvas_MouseUp_Drawing_Poly;
                Canvas_Paint(Canvas, new EventArgs());
            }
            else
            {

                NewPolygon.vertices[NewPolygon.vertices.Count - 1] = mouse_pt;
                MidpointCircle(new Circle(NewPolygon.vertices[0], Convert.ToInt32(over_dist_squared), Color.Red));
                MyImage.Source = BitmapToImageSource(Mybitmap);

                Canvas.MouseMove -= Canvas_MouseMove_Poly_Down;
                Canvas.MouseMove += Canvas_MouseMove_Poly_Up;


            }



        }
        private void Canvas_MouseDown_Next_Poly(object sender, EventArgs e)
        {
            int x = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).X);
            int y = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).Y);
            Point mouse_pt = new Point(x, y);

            NewPolygon.vertices.Add(mouse_pt);

            MidpointCircle(new Circle(NewPolygon.vertices[0], Convert.ToInt32(over_dist_squared), Color.Red));
            MyImage.Source = BitmapToImageSource(Mybitmap);
            Canvas_Paint(Canvas, new EventArgs());
            Canvas.MouseMove -= Canvas_MouseMove_Poly_Up;
            Canvas.MouseMove += Canvas_MouseMove_Poly_Down;

        }


        private bool MouseIsOverEndpoint(int x, int y, out int segment_number, out Point hit_pt)
        {
            Point mouse_pt = new Point(x, y);
            segment_number = -1;
            hit_pt = new Point(-1, -1);
            if (LineCheck.IsChecked.Value)
            {
                for (int i = 0; i < Lines.Count; i++)
                {
                    // Check the starting point.
                    if (FindDistanceToPointSquared(mouse_pt, Lines[i].P1) < over_dist_squared)
                    {
                        // We're over this point.
                        segment_number = i;
                        hit_pt = Lines[i].P1;
                        return true;
                    }

                    // Check the end point.
                    if (FindDistanceToPointSquared(mouse_pt, Lines[i].P2) < over_dist_squared)
                    {
                        // We're over this point.
                        segment_number = i;
                        hit_pt = Lines[i].P2;
                        return true;
                    }
                }
            }
            else if (PolyCheck.IsChecked.Value)
            {
                for (int i = 0; i < Polygons.Count; i++)
                {
                    // Check the starting point.
                    for (int j = 0; j < Polygons[i].vertices.Count; j++)
                    {
                        if (FindDistanceToPointSquared(mouse_pt, Polygons[i].vertices[j]) < over_dist_squared)
                        {
                            // We're over this point.
                            poly_number = i;
                            poly_segment_number = j;
                            hit_pt = Polygons[i].vertices[j];
                            return true;
                        }
                    }

                }
            }
            segment_number = -1;
            hit_pt = new Point(-1, -1);
            return false;


        }

        private void Canvas_MouseMove_MovingSegment(object sender, EventArgs e)
        {
            int x = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).X);
            int y = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).Y);

            if (LineCheck.IsChecked.Value)
            {
                // See how far the first point will move.
                int new_x1 = x + OffsetX;
                int new_y1 = y + OffsetY;

                int dx = new_x1 - Lines[MovingSegment].P1.X;
                int dy = new_y1 - Lines[MovingSegment].P1.Y;

                if (dx == 0 && dy == 0) return;

                // Move the segment to its new location.
                Lines[MovingSegment].P1 = new Point(new_x1, new_y1);
                Lines[MovingSegment].P2 = new Point(Lines[MovingSegment].P2.X + dx, Lines[MovingSegment].P2.Y + dy);
                if (ColorCheck.IsChecked.Value)
                    Lines[MovingSegment].color = currcol;
                if (ChangeLineThickness.IsChecked.Value)
                    Lines[MovingSegment].Thickness = Convert.ToInt32(LineThickness.Value);
            }
            else if (CircleCheck.IsChecked.Value)
            {
                // See how far the first point will move.
                int new_x1 = x + OffsetX;
                int new_y1 = y + OffsetY;

                int dx = new_x1 - Circles[moving_circle_number].Center.X;
                int dy = new_y1 - Circles[moving_circle_number].Center.Y;

                if (dx == 0 && dy == 0) return;

                Circles[moving_circle_number].Center = new Point(new_x1, new_y1);
                if (ColorCheck.IsChecked.Value)
                    Circles[moving_circle_number].color = currcol;
                if (ChangeRadius.IsChecked.Value)
                    Circles[moving_circle_number].Radius = Convert.ToInt32(CircleRadiuss.Value);


            }
            else if (PolyCheck.IsChecked.Value)
            {

                if (ColorCheck.IsChecked.Value)
                    Polygons[movingpoly].color = currcol;
                if (ChangeLineThickness.IsChecked.Value)
                    Polygons[movingpoly].Thickness = Convert.ToInt32(LineThickness.Value);

                if (MoveWholePoly.IsChecked.Value)
                {
                    // See how far the first point will move.
                    int new_x1 = x + OffsetX;
                    int new_y1 = y + OffsetY;

                    int dx = new_x1 - Polygons[movingpoly].vertices[movingpolyseg].X;
                    int dy = new_y1 - Polygons[movingpoly].vertices[movingpolyseg].Y;

                    if (dx == 0 && dy == 0) return;
                    int next_poly_segment_number = movingpolyseg + 1;
                    if (next_poly_segment_number == Polygons[movingpoly].vertices.Count)
                        next_poly_segment_number = 0;
                    // Move the segment to its new location.
                    Polygons[movingpoly].vertices[movingpolyseg] = new Point(new_x1, new_y1);
                    for (int k = 0; k < Polygons[movingpoly].vertices.Count - 1; k++)
                    {
                        int currentmove = (movingpolyseg + k + 1) % Polygons[movingpoly].vertices.Count;
                        Polygons[movingpoly].vertices[currentmove] = new Point(Polygons[movingpoly].vertices[currentmove].X + dx, Polygons[movingpoly].vertices[currentmove].Y + dy);
                    }


                }

                else
                {
                    // See how far the first point will move.
                    int new_x1 = x + OffsetX;
                    int new_y1 = y + OffsetY;

                    int dx = new_x1 - Polygons[movingpoly].vertices[movingpolyseg].X;
                    int dy = new_y1 - Polygons[movingpoly].vertices[movingpolyseg].Y;

                    if (dx == 0 && dy == 0) return;
                    int next_poly_segment_number = movingpolyseg + 1;
                    if (next_poly_segment_number == Polygons[movingpoly].vertices.Count)
                        next_poly_segment_number = 0;
                    // Move the segment to its new location.
                    Polygons[movingpoly].vertices[movingpolyseg] = new Point(new_x1, new_y1);
                    Polygons[movingpoly].vertices[next_poly_segment_number] = new Point(Polygons[movingpoly].vertices[next_poly_segment_number].X + dx, Polygons[movingpoly].vertices[next_poly_segment_number].Y + dy);
                }
            }
            System.Windows.Input.Cursor new_cursor;
            if (x > 680 && y > 680)
            {

                positionx.Content = "X:   " + x.ToString() + " px";
                positiony.Content = "Y:   " + y.ToString() + " px";

                new_cursor = System.Windows.Input.Cursors.Wait;


                if (Canvas.Cursor != new_cursor)
                    Canvas.Cursor = new_cursor;
            }
            else
            {
                new_cursor = System.Windows.Input.Cursors.Cross;

                if (Canvas.Cursor != new_cursor)
                    Canvas.Cursor = new_cursor;
            }
            // Redraw.
            Canvas_Paint(Canvas, new EventArgs());
        }

        // Stop moving the segment.
        private void Canvas_MouseUp_MovingSegment(object sender, EventArgs e)
        {
            // Reset the event handlers.
            Canvas.MouseMove += Canvas_MouseMove_NotDown;
            Canvas.MouseMove -= Canvas_MouseMove_MovingSegment;
            Canvas.MouseUp -= Canvas_MouseUp_MovingSegment;

            int x = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).X);
            int y = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).Y);
            if (LineCheck.IsChecked.Value)
            {
                if (x > 680 && y > 680)
                {
                    Lines.RemoveAt(MovingSegment);
                    Lines.Remove(null);
                }
            }


            else if (CircleCheck.IsChecked.Value)
            {
                if (x > 680 && y > 680)
                {
                    Circles.RemoveAt(moving_circle_number);
                    Circles.Remove(null);

                }
            }

            else if (PolyCheck.IsChecked.Value)
            {
                if (x > 680 && y > 680)
                {
                    Polygons.RemoveAt(movingpoly);
                    Polygons.Remove(null);

                }
            }

            // Redraw.
            Canvas_Paint(Canvas, new EventArgs());
        }

        int circle_number;
        double FindDistanceToSegmentSquared(Point p, Point v, Point w)
        {
            if ((p.X > v.X && p.X > w.X) || (p.X < v.X && p.X < w.X) || (p.Y > v.Y && p.Y > w.Y) || (p.Y < v.Y && p.Y < w.Y))
                return 1000;
            return Math.Abs((w.Y - v.Y) * p.X - (w.X - v.X) * p.Y + w.X * v.Y - w.Y * v.X) / Math.Sqrt(Math.Pow(w.Y - v.Y, 2) + Math.Pow(w.X - v.X, 2));
        }
        private bool MouseIsOverSegment(int x, int y, out int segment_number)
        {
            segment_number = -1;
            Point mouse_pt = new Point(x, y);
            if (LineCheck.IsChecked.Value)
            {
                for (int i = 0; i < Lines.Count; i++)
                {
                    // See if we're over the segment.

                    if (FindDistanceToSegmentSquared(mouse_pt, Lines[i].P1, Lines[i].P2) < over_dist_squared)
                    {
                        segment_number = i;
                        return true;
                    }
                }
            }
            else if (CircleCheck.IsChecked.Value)
            {
                for (int i = 0; i < Circles.Count; i++)
                {
                    if (FindDistanceToPointSquared(mouse_pt, Circles[i].Center) < over_dist_squared)
                    {
                        // We're over this point.
                        circle_number = i;
                        return true;
                    }
                }
            }
            else if (PolyCheck.IsChecked.Value)
            {
                for (int i = 0; i < Polygons.Count; i++)
                {
                    // See if we're over the segment.
                    for (int j = 0; j < Polygons[i].vertices.Count - 1; j++)
                    {
                        if (FindDistanceToSegmentSquared(mouse_pt, Polygons[i].vertices[j], Polygons[i].vertices[j + 1]) < over_dist_squared)
                        {
                            poly_number = i;
                            poly_segment_number = j;
                            return true;
                        }
                    }
                    if (FindDistanceToSegmentSquared(mouse_pt, Polygons[i].vertices[Polygons[i].vertices.Count - 1], Polygons[i].vertices[0]) < over_dist_squared)
                    {
                        poly_number = i;
                        poly_segment_number = Polygons[i].vertices.Count - 1;
                        return true;
                    }
                }
            }

            segment_number = -1;
            return false;
        }
        private double FindDistanceToPointSquared(Point mouse_pt, Point point)
        {


            return (Math.Pow(mouse_pt.X - point.X, 2) + Math.Pow(mouse_pt.Y - point.Y, 2));
        }

        // See if the mouse is over a line segment.


        // We're drawing a new segment.
        private void Canvas_MouseMove_Drawing(object sender, EventArgs e)
        {
            int x = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).X);
            int y = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).Y);
            // Save the new point.
            if (LineCheck.IsChecked.Value)
                NewLine.P2 = new Point(x, y);
            else if (CircleCheck.IsChecked.Value)
                NewCircle.Center = new Point(x, y);

            // Redraw.
            Canvas_Paint(Canvas, new EventArgs());

        }

        // Stop drawing.
        private void Canvas_MouseUp_Drawing(object sender, EventArgs e)
        {
            if (LineCheck.IsChecked.Value)
            {
                IsDrawing = false;

                // Reset the event handlers.
                Canvas.MouseMove -= Canvas_MouseMove_Drawing;
                Canvas.MouseMove += Canvas_MouseMove_NotDown;
                Canvas.MouseUp -= Canvas_MouseUp_Drawing;

                // Create the new segment.
                Lines.Add(NewLine);
            }
            else if (CircleCheck.IsChecked.Value)
            {
                IsDrawing = false;

                Canvas.MouseMove -= Canvas_MouseMove_Drawing;
                Canvas.MouseMove += Canvas_MouseMove_NotDown;
                Canvas.MouseUp -= Canvas_MouseUp_Drawing;

                // Create the new segment.
                Circles.Add(NewCircle);

            }
            // Redraw.
            Canvas_Paint(Canvas, new EventArgs());
        }
        // We're moving an end point.
        private void Canvas_MouseMove_MovingEndPoint(object sender, EventArgs e)
        {
            int x = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).X);
            int y = Convert.ToInt32(System.Windows.Input.Mouse.GetPosition(Canvas).Y);
            if (LineCheck.IsChecked.Value)
            {
                // Move the point to its new location.
                if (MovingStartEndPoint)
                    Lines[MovingSegment].P1 =
                        new Point(x + OffsetX, y + OffsetY);
                else
                    Lines[MovingSegment].P2 =
                        new Point(x + OffsetX, y + OffsetY);
            }
            else if (PolyCheck.IsChecked.Value)
            {
                // Move the point to its new location.
                Polygons[movingpoly].vertices[movingpolyseg] = new Point(x + OffsetX, y + OffsetY);

            }

            // Redraw.
            Canvas_Paint(Canvas, new EventArgs());
        }
        private void LineCheck_Checked(object sender, RoutedEventArgs e)
        {
            CircleCheck.IsChecked = false;
            PolyCheck.IsChecked = false;
        }
        private void CircleCheck_Checked(object sender, RoutedEventArgs e)
        {
            PolyCheck.IsChecked = false;
            LineCheck.IsChecked = false;
            Canvas_Paint(Canvas, new EventArgs());

        }

        private void PolyCheck_Checked(object sender, RoutedEventArgs e)
        {
            CircleCheck.IsChecked = false;
            LineCheck.IsChecked = false;

        }

        private void ColorPicker1_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            System.Windows.Media.Color mediacolor = ColorPicker1.SelectedColor.Value; // your color

            currcol = Color.FromArgb(
                mediacolor.A, mediacolor.R, mediacolor.G, mediacolor.B);
            test.Content = currcol.ToString();
        }

        // Stop moving the end point.
        private void Canvas_MouseUp_MovingEndPoint(object sender, EventArgs e)
        {

            // Reset the event handlers.
            Canvas.MouseMove += Canvas_MouseMove_NotDown;
            Canvas.MouseMove -= Canvas_MouseMove_MovingEndPoint;
            Canvas.MouseUp -= Canvas_MouseUp_MovingEndPoint;



            // Redraw.
            Canvas_Paint(Canvas, new EventArgs());
        }

        private void AntiAliasing_Click(object sender, RoutedEventArgs e)
        {
            Canvas_Paint(Canvas, new EventArgs());
        }
        // We're moving an end point.



    }

}
