using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using TVGL;
using TVGL.IOFunctions;

namespace PrimitiveClassificationOfTessellatedSolids
{
    class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            MainWindow window = null;
            var thread = new Thread(() =>
            {
                window = new MainWindow();
            var writer = new TextWriterTraceListener(System.Console.Out);
            Debug.Listeners.Add(writer);
            Console.WriteLine("Enter the input directory:");
            var inptDir = Console.ReadLine();
            while (inptDir == null || !inptDir.Any())
            {
                Console.WriteLine("I need a valid input directory. Simply copy and paste the address of the location of the tessellated model plus the file name:");
                inptDir = Console.ReadLine();
            }
            TessellatedSolid ts = null;
            var s = File.OpenRead(inptDir);
            ts = IO.Open(s, inptDir, false)[0];
            TesselationToPrimitives.Run(ts);
            var cc = 0;
            foreach (var f in ts.Faces)
            {
                cc++;
                //var c = Colors.Red;
                Point3DCollection vOrder = new Point3DCollection();
                if (f.Edges[0].OwnedFace == f)
                {
                    vOrder.Add(new Point3D(f.Edges[0].From.Position[0], f.Edges[0].From.Position[1], f.Edges[0].From.Position[2]));
                    vOrder.Add(new Point3D(f.Edges[0].To.Position[0], f.Edges[0].To.Position[1], f.Edges[0].To.Position[2]));
                }
                else
                {
                    vOrder.Add(new Point3D(f.Edges[0].To.Position[0], f.Edges[0].To.Position[1], f.Edges[0].To.Position[2]));
                    vOrder.Add(new Point3D(f.Edges[0].From.Position[0], f.Edges[0].From.Position[1], f.Edges[0].From.Position[2]));
                }
                if (f.Edges[1].From == f.Edges[0].From || f.Edges[1].From == f.Edges[0].To)
                    vOrder.Add(new Point3D(f.Edges[1].To.Position[0], f.Edges[1].To.Position[1], f.Edges[1].To.Position[2]));
                else vOrder.Add(new Point3D(f.Edges[1].From.Position[0], f.Edges[1].From.Position[1], f.Edges[1].From.Position[2]));
                var c = new System.Windows.Media.Color { A = f.Color.A, B = f.Color.B, G = f.Color.G, R = f.Color.R };
                window.view1.Children.Add(new ModelVisual3D
                {
                    Content =
                        new GeometryModel3D
                        {
                            Geometry = new MeshGeometry3D
                            {
                                Positions = vOrder,
                                TriangleIndices = new Int32Collection(new int[] { 0, 1, 2 }),
                                Normals = new Vector3DCollection(new[] { new Vector3D(f.Normal[0], f.Normal[1], f.Normal[2]) })
                            },
                            Material = new DiffuseMaterial(new SolidColorBrush(c))
                        }
                });
            }
            window.view1.ZoomExtentsWhenLoaded = true;
            window.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}
