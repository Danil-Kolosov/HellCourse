using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrafRedactor
{
    public class Point3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Point3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3D(PointF point2D, float z = 0)
        {
            X = point2D.X;
            Y = point2D.Y;
            Z = z;
        }

        public PointF ToPoint2D()
        {
            return new PointF(X, Y);
        }

        // Математические операторы
        public static Point3D operator +(Point3D a, Point3D b)
            => new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Point3D operator -(Point3D a, Point3D b)
            => new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Point3D operator *(Point3D p, float scalar)
            => new Point3D(p.X * scalar, p.Y * scalar, p.Z * scalar);

        public override string ToString()
            => $"({X:F1}, {Y:F1}, {Z:F1})";
    }
}
