using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace GrafRedactor
{
    public static class SimpleCamera
    {
        public static float Distance { get; set; } = 500f; // Расстояние от камеры до плоскости проекции

        //переименоваьть в "подойти/отойти"
        public static PointF ProjectTo2D(Point3D point3D) 
        {
            // Простая перспектива: чем дальше Z, тем меньше объект
            //float scale = Distance / (Distance - point3D.Z);
            //return new PointF(point3D.X * scale, point3D.Y * scale);
            return new PointF(point3D.X*Distance/(point3D.Z+Distance), point3D.Y * Distance / (point3D.Z + Distance));
        }

        public static (PointF, PointF, float, float) ProjectTo2D(Point3D point3D, PointF pointStart, PointF pointEnd, string GroupId, float _scaleFactorS, float _scaleFactorE)
        {
            PointF center;
            if (GroupId == null)
            {
                center = new PointF((pointStart.X + pointEnd.X) / 2, (pointStart.Y + pointEnd.Y) / 2);
            }
            else
            {
                center = groupManager.GetGroupCenter(GroupId);
            }
            float scaleFactor = SimpleCamera.GetScaleFactor(point3D.Z);
            if (scaleFactor != _scaleFactorS)
            {
                PointF testPoint = ScalePoint(pointStart, center, _scaleFactorS / scaleFactor, _scaleFactorS / scaleFactor);

                if (drawingArea.Contains(Point.Round(testPoint)))
                {
                    pointStart = testPoint;
                    _scaleFactorS = scaleFactor;
                    return (testPointS, testPointE, scaleFactorS, scaleFactorE); 
                    //добавить тут же высчитывание для старта и для конца - и их возвращение поучается
                }
                else
                    return ();
            }
            
        }

        public static PointF ScalePoint(PointF point, PointF center, float sx, float sy)
        {
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;

            return new PointF(
                center.X + dx * sx,
                center.Y + dy * sy
            );
        }
        public static float GetScaleFactor(float pointZ)
        {
            return Distance / (pointZ + Distance);
        }

        //добавить методы (матрицы) повернуть голову влево/право вверх/вниз
    }
}
