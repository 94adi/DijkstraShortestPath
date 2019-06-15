using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Windows;
using System.IO;

namespace howto_enlarge_polygon
{
    public partial class Form1 : Form
    {
        private List<PointF> Points = new List<PointF>();
        public List<PointF> AllPoints = new List<PointF>();
        private PointF CurrentPoint; //current point on the picturebox (while mouse is moving)
        private bool Drawing = false;
        public bool new_poly = true;
        public bool first_time = true;
        public int node_ID = -1;
        public int mode;
        public int start_stop; //start_stop = 0 -> selecting start point ; start_stop = 1 -> selecting stop point
        public int first_index;
        Bitmap img;
        Graphics gfx;
        Pen pen;

        public struct Poly
        {
            public List<PointF> myPoints;
        }

        Poly current_poly;
        public PointF start;
        public PointF stop;
        public Pen mypen;
        public PointF current_extended1;
        public PointF current_extended2;
        public PointF current_normal1;
        public PointF current_normal2;
        public struct Nodes
        {
            public Dictionary<int, Dictionary<int, int>> node;
        }
        public struct iNodes
        {
            public int ID;
            public PointF point;
            public Dictionary<int, int> neighbor;
        }
        iNodes[] myinodes;
        Nodes[] mynodes;
        Poly[] Polygon;
        Dictionary<int, Dictionary<int, int>> vertices = new Dictionary<int, Dictionary<int, int>>();
        public int poly_counter;
        public int IDcounter = -1;
        public Form1()
        {
            InitializeComponent();
            InitVariables();
        }

        private void picCanvas_Paint(object sender, PaintEventArgs e)
        {
            Draw_Current_Forms();
        }

        private void picCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //NEW Polygon + old polygon finishes drawing
                poly_counter++; //index for polygon array
                if (Drawing)
                {
                    Drawing = false;

                    // Make sure the polygon is oriented counter clockwise.
                    if (PolygonIsOrientedClockwise(Points))
                    {
                        // Reverse the points.
                        List<PointF> pts = new List<PointF>();
                        for (int i = Points.Count - 1; i >= 0; i--)
                        {
                            IDcounter++;
                            if (i < (Points.Count - 1))
                            {
                                myinodes[IDcounter].ID = IDcounter;
                                myinodes[IDcounter].point = Points[i];
                                myinodes[IDcounter].neighbor.Add(myinodes[IDcounter - 1].ID, Get_Line_Length(Points[i + 1], Points[i]));
                                myinodes[IDcounter - 1].neighbor.Add(myinodes[IDcounter].ID, Get_Line_Length(Points[i + 1], Points[i]));
                                if (i == 0)
                                {
                                    myinodes[IDcounter].neighbor.Add(myinodes[first_index].ID, Get_Line_Length(Points[0], Points[(Points.Count - 1)]));
                                    myinodes[first_index].neighbor.Add(myinodes[IDcounter].ID, Get_Line_Length(Points[0], Points[(Points.Count - 1)]));
                                }
                            }
                            else if (i == (Points.Count - 1))
                            {
                                first_index = IDcounter;
                                myinodes[IDcounter].ID = IDcounter;
                                myinodes[IDcounter].point = Points[i];
                            }
                            pts.Add(Points[i]);
                            Polygon[poly_counter].myPoints.Add(Points[i]); //add points in polygon structure array
                        }                   
                        Points = pts;
                    }
                    else
                    {
                        //Do the same thing but no need to reverse points this time
                        for (int i = 0; i < Points.Count; i++)
                        {
                            IDcounter++;
                            if (i == 0)
                            {
                                first_index = IDcounter;
                                myinodes[IDcounter].ID = IDcounter;
                                myinodes[IDcounter].point = Points[i];
                            }
                            else
                            {
                                myinodes[IDcounter].ID = IDcounter;
                                myinodes[IDcounter].point = Points[i];
                                myinodes[IDcounter].neighbor.Add(myinodes[IDcounter - 1].ID, Get_Line_Length(Points[i - 1], Points[i]));
                                myinodes[IDcounter - 1].neighbor.Add(myinodes[IDcounter].ID, Get_Line_Length(Points[i - 1], Points[i]));
                                if (i == (Points.Count - 1))
                                {
                                    myinodes[IDcounter].neighbor.Add(myinodes[first_index].ID, Get_Line_Length(Points[0], Points[i]));
                                    myinodes[first_index].neighbor.Add(myinodes[IDcounter].ID, Get_Line_Length(Points[0], Points[i]));
                                }
                            }
                            Polygon[poly_counter].myPoints.Add(Points[i]);
                        }
                    }
                }
            }
            else
            {
                //If you left clicked....
                if (mode == 0)
                {
                    //IN ACEST MOD ATUNCI CAND APESI CLICK SE VA SALVA COORD PCT CURENT IN LISTA DE PUNCTE AFERENTA POLIGONULUI CURENT
                    if (!Drawing)
                    {
                        Drawing = true;
                        Points = new List<PointF>();
                    }
                    CurrentPoint = e.Location;
                    gfx.FillEllipse(new SolidBrush(pen.Color), CurrentPoint.X, CurrentPoint.Y, 5, 5);
                    Points.Add(CurrentPoint); //stores the coords of current poly before the final verion gets stored in the structure
                }
                else if (mode == 1)
                {

                    //In this mode you choose the START and STOP point
                    Drawing = false;
                    if (start_stop == 0)
                    {
                        start = new PointF(e.X, e.Y);
                        start_stop++;
                        IDcounter++;
                        myinodes[IDcounter].ID = IDcounter;
                        myinodes[IDcounter].point = start;
                    }
                    else if (start_stop == 1)
                    {
                        stop = new PointF(e.X, e.Y);
                        start_stop++;
                        IDcounter++;
                        myinodes[IDcounter].ID = IDcounter;
                        myinodes[IDcounter].point = stop;
                    }
                    //punctele de START si STOP
                }
                else if (mode == 2)
                {
                    Draw_All_Paths();
                    Find_Shorterst_Path();
                }
                picCanvas.Refresh();
            }
        }

        private void picCanvas_MouseMove(object sender, MouseEventArgs e)
        {

            label2.Text = e.X.ToString();
            label3.Text = e.Y.ToString();
            if (!Drawing) return;
            CurrentPoint = e.Location;
            picCanvas.Refresh();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (mode <= 2)
            {
                mode++;
                if (mode == 1)
                    button1.Text = "Process";
                if (mode == 2)
                    button1.Enabled = false;
            }
        }
        public int Get_Line_Length(PointF a, PointF b)
        {
            double length;
            int len;
            length = Math.Sqrt(Math.Pow((b.X - a.X), 2) + Math.Pow((b.Y - a.Y), 2));
            len = (int)length;
            return len;
        }
        public int get_ID(PointF x)
        {
            int ID = 0;
            foreach (var node in myinodes)
            {
                if (x == node.point)
                {
                    ID = node.ID;
                    break;
                }
            }
            return ID;
        }
        public void Add_New_Relationship(PointF a, PointF b)
        {
            int ID1 = get_ID(a);
            int ID2 = get_ID(b);
            int len = Get_Line_Length(a, b);
            if (!(myinodes[ID1].neighbor.ContainsKey(ID2)))
                myinodes[ID1].neighbor.Add(myinodes[ID2].ID, len);
            if (!(myinodes[ID2].neighbor.ContainsKey(ID1)))
                myinodes[ID2].neighbor.Add(myinodes[ID1].ID, len);
        }       
        public void add_vertex(int name, Dictionary<int, int> edges)
        {
            vertices[name] = edges;
        }
        // Return true if the polygon is oriented clockwise.
        public bool PolygonIsOrientedClockwise(List<PointF> points)
        {
            return (SignedPolygonArea(points) < 0);
        }

        // Return the polygon's area in "square units."
        // The value will be negative if the polygon is
        // oriented clockwise.
        private float SignedPolygonArea(List<PointF> points)
        {
            // Add the first point to the end.
            int num_points = points.Count;
            PointF[] pts = new PointF[num_points + 1];
            points.CopyTo(pts, 0);
            pts[num_points] = points[0];

            // Get the areas.
            float area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area +=
                    (pts[i + 1].X - pts[i].X) *
                    (pts[i + 1].Y + pts[i].Y) / 2;
            }

            // Return the result.
            return area;
        }
        // Clear the Points list.
        public List<PointF> shortest_path(PointF pstart, PointF pfinish)
        {
            var previous = new Dictionary<int, int>();
            var distances = new Dictionary<int, int>(); //stores current value of each node (where value = distance traveled from node to node)
            var nodes = new List<int>();
            List<PointF> points_path = new List<PointF>();
            List<int> path = null;
            int start = get_ID(pstart);
            int finish = get_ID(pfinish);
            //initialises the distances data container nodes and lengths
            //starting node gets 0 the rest get infinity or MaxValue constant
            foreach (var vertex in vertices)
            {
                if (vertex.Key == start)
                {
                    distances[vertex.Key] = 0;
                }
                else
                {
                    distances[vertex.Key] = int.MaxValue;
                }

                nodes.Add(vertex.Key); //list with the names of all the nodes
            }

            while (nodes.Count != 0) //while there are still nodes to visit
            {
                nodes.Sort((x, y) => distances[x] - distances[y]); //sorts the nodes ascending

                var smallest = nodes[0]; //node with the smallest value
                nodes.Remove(smallest); //remove the node -> node has been visited, no need to revisit

                if (smallest == finish) //you're done
                {
                    path = new List<int>();
                    while (previous.ContainsKey(smallest))
                    {
                        path.Add(smallest);
                        smallest = previous[smallest];
                    }

                    break;
                }

                if (distances[smallest] == int.MaxValue)
                {
                    break;
                }

                foreach (var neighbor in vertices[smallest]) //visit all neighbors in current node(smallest)
                {
                    var alt = distances[smallest] + neighbor.Value; //add to the current value from current node the value of neighbor
                    if (alt < distances[neighbor.Key]) //the new value is less than the current value of the neighbor node
                    {
                        //update the value of the neighbor
                        distances[neighbor.Key] = alt;
                        //consider the current neighbor as part of the shortest path
                        previous[neighbor.Key] = smallest;
                    }
                }
            }
            path.Add(start);
            path.Reverse();
            foreach (var el in path)
            {
                foreach(var el1 in myinodes)
                {
                    if(el1.ID == el)
                    {
                        points_path.Add(el1.point);
                    }
                }
            }
            return points_path;
        }
        public void Draw_All_Paths()
        {
            bool lines_intersect;
            bool segments_intersect;
            bool intersect = false;
            PointF intersection;
            PointF close_p1;
            PointF close_p2;
            PointF p;
            for (int i = 0; i < Polygon.Length; i++)
            {
                for (int j = 0; j < Polygon[i].myPoints.Count; j++)
                {
                    current_extended1 = Polygon[i].myPoints[j];
                    intersect = false;
                    for (int o = 0; o < Polygon.Length; o++)
                    {
                        for (int q = 0; q < Polygon[o].myPoints.Count - 1; q++)
                        {
                            FindIntersection(start, current_extended1, Polygon[o].myPoints[q], Polygon[o].myPoints[q + 1], out lines_intersect, out segments_intersect, out intersection, out close_p1, out close_p2);
                            if (segments_intersect == true)
                            {
                                if (intersection != current_extended1)
                                    intersect = true;
                            }
                            if (q == Polygon[o].myPoints.Count - 2)
                            {
                                FindIntersection(start, current_extended1, Polygon[o].myPoints[0], Polygon[o].myPoints[Polygon[o].myPoints.Count - 1], out lines_intersect, out segments_intersect, out intersection, out close_p1, out close_p2);
                                if (segments_intersect == true)
                                {
                                    if (intersection != current_extended1)
                                        intersect = true;
                                }
                            }
                        }

                    }
                    if (intersect == false)
                    {
                        gfx.DrawLine(Pens.Orange, start, Polygon[i].myPoints[j]);
                        Add_New_Relationship(start, current_extended1);
                        //MessageBox.Show("Intersect");
                    }
                }
            }
            for (int i = 0; i < Polygon.Length; i++)
            {
                for (int j = 0; j < Polygon[i].myPoints.Count; j++)
                {
                    current_extended1 = Polygon[i].myPoints[j];
                    intersect = false;
                    for (int o = 0; o < Polygon.Length; o++)
                    {
                        for (int q = 0; q < Polygon[o].myPoints.Count - 1; q++)
                        {
                            FindIntersection(stop, current_extended1, Polygon[o].myPoints[q], Polygon[o].myPoints[q + 1], out lines_intersect, out segments_intersect, out intersection, out close_p1, out close_p2);
                            if (segments_intersect == true)
                            {
                                if (intersection != current_extended1)
                                    intersect = true;
                            }
                            if (q == Polygon[o].myPoints.Count - 2)
                            {
                                FindIntersection(stop, current_extended1, Polygon[o].myPoints[0], Polygon[o].myPoints[Polygon[o].myPoints.Count - 1], out lines_intersect, out segments_intersect, out intersection, out close_p1, out close_p2);
                                if (segments_intersect == true)
                                {
                                    if (intersection != current_extended1)
                                        intersect = true;
                                }
                            }
                        }

                    }
                    if (intersect == false)
                    {
                        gfx.DrawLine(Pens.Orange, stop, Polygon[i].myPoints[j]);
                        Add_New_Relationship(stop, current_extended1);
                    }
                }
            }
            ///////////////////////////////////////////////////
            for (int i = 0; i < Polygon.Length; i++)
            {
                current_poly = Polygon[i];
                for (int j = 0; j < current_poly.myPoints.Count; j++)
                {
                    current_normal1 = current_poly.myPoints[j];
                    for (int k = 0; k < Polygon.Length; k++)
                    {
                        if (k != i) //it can't be the same polygon
                        {
                            for (int l = 0; l < Polygon[k].myPoints.Count; l++)
                            {
                                current_normal2 = Polygon[k].myPoints[l];
                                intersect = false;
                                for (int m = 0; m < Polygon.Length; m++)
                                {
                                    for (int n = 0; n < Polygon[m].myPoints.Count - 1; n++)
                                    {
                                        FindIntersection(current_normal1, current_normal2, Polygon[m].myPoints[n], Polygon[m].myPoints[n + 1], out lines_intersect, out segments_intersect, out intersection, out close_p1, out close_p2);
                                        if (segments_intersect == true)
                                        {
                                            if (intersection != current_normal1 && intersection != current_normal2)
                                                intersect = true;
                                        }
                                        if (n == Polygon[m].myPoints.Count - 2)
                                        {
                                            FindIntersection(current_normal1, current_normal2, Polygon[m].myPoints[0], Polygon[m].myPoints[Polygon[m].myPoints.Count - 1], out lines_intersect, out segments_intersect, out intersection, out close_p1, out close_p2);
                                            if (segments_intersect == true)
                                            {
                                                if (intersection != current_normal1 && intersection != current_normal2)
                                                    intersect = true;
                                            }
                                        }
                                    }
                                }
                                if (intersect == false)
                                {
                                    gfx.DrawLine(Pens.Orange, current_normal1, current_normal2);
                                    Add_New_Relationship(current_normal1, current_normal2);
                                }
                            }
                        }


                    }
                }
            }
        }
        public void Find_Shorterst_Path()
        {
            foreach (var node in myinodes)
            {
                add_vertex(node.ID, node.neighbor);
            }
            List<PointF> final = shortest_path(start, stop);
            for (int f = 0; f < final.Count - 1; f++)
            {
                gfx.DrawLine(new Pen(Color.Green, 2), final[f], final[f + 1]);
            }

        }
        public void InitVariables()
        {
            Polygon = new Poly[50];
            mynodes = new Nodes[100];
            myinodes = new iNodes[100];
            for (int k = 0; k < myinodes.Length; k++)
            {
                myinodes[k].neighbor = new Dictionary<int, int>();
            }
            poly_counter = -1;
            mode = 0;
            start_stop = 0;
            for (int i = 0; i < Polygon.Length; i++)
            {
                Polygon[i].myPoints = new List<PointF>();
            }
            for (int j = 0; j < mynodes.Length; j++)
            {
                mynodes[j].node = new Dictionary<int, Dictionary<int, int>>();
            }
            img = new Bitmap(picCanvas.Width, picCanvas.Height);

            gfx = Graphics.FromImage(img);

            gfx.Clear(Color.White);
            picCanvas.Image = img;

            pen = new Pen(Color.Black, 5);
        }
        private void FindIntersection(PointF p1, PointF p2, PointF p3, PointF p4,
                                     out bool lines_intersect, out bool segments_intersect,
                                     out PointF intersection,
                                     out PointF close_p1, out PointF close_p2)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;

            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PointF(float.NaN, float.NaN);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            float t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }
        public void Draw_Current_Forms()
        {
            picCanvas.Refresh();
            Graphics temp = picCanvas.CreateGraphics();
            if (Drawing)
            {
                for (int i = 0; i <= poly_counter; i++)
                    gfx.DrawPolygon(Pens.Red, Polygon[i].myPoints.ToArray());
                if (Points.Count >= 2)
                    gfx.DrawLines(Pens.Red, Points.ToArray());
                picCanvas.Image = img;
            }
            else
            {
                if (Points.Count >= 3)
                {

                    for (int i = 0; i <= poly_counter; i++)
                    {
                        gfx.DrawPolygon(Pens.Red, Polygon[i].myPoints.ToArray());
                    }
                }
                if (start_stop == 1)
                {
                    gfx.FillEllipse(new SolidBrush(pen.Color), start.X, start.Y, 5, 5);
                }
                if (start_stop == 2)
                {
                    gfx.FillEllipse(new SolidBrush(pen.Color), stop.X, stop.Y, 5, 5);
                }

            }
            picCanvas.Image = img;
        }
    } 
    }