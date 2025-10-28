using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace GrafRedactor
{
    public static class SimpleCamera
    {
        private static GroupManager groupManager;
        private static Rectangle drawingArea;
        public static GroupManager GroupManager { get => groupManager; set { groupManager = value; } }
        public static Rectangle DrawingArea { get => drawingArea; set { drawingArea = value; } }
        public static float Distance { get; set; } = 500f; // Расстояние от камеры до плоскости проекции

                    //переименоваьть в "подойти/отойти"
                    //public static (PointF, PointF, float, float) ProjectTo2D(Point3D point3DS, Point3D point3DE, float _scaleFactorS, float _scaleFactorE,/* PointF startPointPlosk, PointF endPointPlosk,*/ string GroupId = null) 
                    //{
                    //    PointF pointStart = new PointF(point3DS.X, point3DS.Y);
                    //    PointF pointEnd = new PointF(point3DE.X, point3DE.Y);
                    //    PointF center;
                    //    if (GroupId == null)
                    //    {
                    //        center = new PointF((point3DS.X + point3DE.X) / 2, (point3DS.Y + point3DE.Y) / 2);
                    //    }
                    //    else
                    //    {
                    //        center = groupManager.GetGroupCenter(GroupId);
                    //    }
                    //    float scaleFactorS = SimpleCamera.GetScaleFactor(point3DS.Z);
                    //    float scaleFactorE = SimpleCamera.GetScaleFactor(point3DE.Z);
                    //    pointStart = ScalePoint(new PointF(point3DS.X, point3DS.Y), center, _scaleFactorS, _scaleFactorS);
                    //    pointEnd = ScalePoint(new PointF(point3DE.X, point3DE.Y), center, _scaleFactorE, _scaleFactorE);
                    //    if (scaleFactorS != _scaleFactorS)
                    //    {
                    //        PointF testPointS = ScalePoint(new PointF(point3DS.X, point3DS.Y), center, scaleFactorS, scaleFactorS/*_scaleFactorS / scaleFactorS, _scaleFactorS / scaleFactorS*/);                
                    //        if (drawingArea.Contains(Point.Round(testPointS)))
                    //        {
                    //            pointStart = testPointS;                                        
                    //        }
                    //        _scaleFactorS = scaleFactorS;
                    //    }
                    //    if (scaleFactorE != _scaleFactorE)
                    //    {
                    //        PointF testPointE = ScalePoint(new PointF(point3DE.X, point3DE.Y), center, scaleFactorE, scaleFactorE/*_scaleFactorE / scaleFactorE, _scaleFactorE / scaleFactorE*/);
                    //        if (drawingArea.Contains(Point.Round(testPointE)))
                    //        {
                    //            pointEnd = testPointE;
                    //        }
                    //        _scaleFactorE = scaleFactorE;
                    //    }
                    //    return (pointStart, pointEnd, _scaleFactorS, _scaleFactorE);

                    //    // Простая перспектива: чем дальше Z, тем меньше объект
                    //    //float scale = Distance / (Distance - point3D.Z);
                    //    //return new PointF(point3D.X * scale, point3D.Y * scale);
                    //    //return new PointF(point3D.X*Distance/(point3D.Z+Distance), point3D.Y * Distance / (point3D.Z + Distance));
                    //}

                    //public static PointF ProjectTo2D(Point3D point3D)
                    //{
                    //    // Простая перспективная проекция
                    //    float scale = Distance / (point3D.Z + Distance);
                    //    return new PointF(point3D.X * scale, point3D.Y * scale);
                    //}

        public static void SetAreaAndGroupManag(GroupManager groupManag, Rectangle drawArea) 
        {
            groupManager = groupManag;
            drawingArea = drawArea;
        }

        //public static (PointF, PointF, float, float) ProjectTo2D(Point3D point3D, PointF pointStart, PointF pointEnd, string GroupId, float _scaleFactorS, float _scaleFactorE)
        //{
        //    PointF center;
        //    if (GroupId == null)
        //    {
        //        center = new PointF((pointStart.X + pointEnd.X) / 2, (pointStart.Y + pointEnd.Y) / 2);
        //    }
        //    else
        //    {
        //        center = groupManager.GetGroupCenter(GroupId);
        //    }
        //    float scaleFactor = SimpleCamera.GetScaleFactor(point3D.Z);
        //    if (scaleFactor != _scaleFactorS)
        //    {
        //        PointF testPoint = ScalePoint(pointStart, center, _scaleFactorS / scaleFactor, _scaleFactorS / scaleFactor);

        //        if (drawingArea.Contains(Point.Round(testPoint)))
        //        {
        //            pointStart = testPoint;
        //            _scaleFactorS = scaleFactor;
        //            return (testPointS, testPointE, scaleFactorS, scaleFactorE); 
        //            //добавить тут же высчитывание для старта и для конца - и их возвращение поучается
        //        }
        //        else
        //            return ();
        //    }

        //}

        //public static PointF ScalePoint(PointF point, PointF center, float sx, float sy)
        //{
        //    float dx = point.X - center.X;
        //    float dy = point.Y - center.Y;

        //    return new PointF(
        //        center.X + dx * sx,
        //        center.Y + dy * sy
        //    );
        //}
        //public static float GetScaleFactor(float pointZ)
        //{
        //    if(pointZ >= Distance)
        //        return Distance;
        //    return Distance / (pointZ + Distance);
        //}

        //добавить методы (матрицы) повернуть голову влево/право вверх/вниз



        public static PointF ProjectTo2D(Point3D point3D)
        {
            // Простая перспективная проекция
            // Чем больше Z (ближе к камере), тем больше масштаб
            float scale = GetScaleFactor(point3D.Z);

            // Масштабируем координаты
            float x = point3D.X * scale;
            float y = point3D.Y * scale;

            return new PointF(x, y);
        }

        public static float GetScaleFactor(float pointZ)
        {
            // Z = 0: scale = 1 (нормальный размер)
            // Z > 0: scale > 1 (увеличивается)
            // Z < 0: scale < 1 (уменьшается)
            //return 1.0f + (pointZ / Distance);

            //Просто берем ортографическую проекцию - Z НЕ ВЛИЯЕТ!!
            return 1.0f;
        }

        public static PointF ScalePoint(PointF point, PointF center, float scale)
        {
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;

            return new PointF(
                center.X + dx * scale,
                center.Y + dy * scale
            );
        }
    }
}
