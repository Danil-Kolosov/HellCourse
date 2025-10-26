using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrafRedactor
{
    class LineElement3D : LineElement
    {
        private float _startZ;
        private float _endZ;
        private float _scaleFactorS = 1;
        private float _scaleFactorE = 1;
        private Point3D _startPointR;
        public Point3D StartPointR
        {
            get => _startPointR;
            set
            {
                if (_startPointR != value)
                {
                    _startPointR = value;
                    StartPoint = SimpleCamera.ProjectTo2D(_startPointR);
                }
            }
        }
        public float StartZ
        {
            get => _startZ;
            set
            {
                if (_startZ != value)
                {
                    _startZ = value;
                    //var camera = new SimpleCamera(); // Или передавать камеру из формы
                    //Point3D start3d = new Point3D(StartPoint, _startZ);

                    //PointF start2D = camera.ProjectTo2D(start3d);


                    /*float scaleFactor = SimpleCamera.GetScaleFactor(_startZ);
                    if(scaleFactor != _scaleFactor)
                    { 
                        StartPoint = ScalePoint(StartPoint, _scaleFactor / scaleFactor, _scaleFactor / scaleFactor);
                        _scaleFactor = scaleFactor;
                    }*/


                    //Position = start2D; оно уже внутри присвоение StartPoint сделается
                    //OnPropertyChanged(); это тоже
                }
            }
        }

        public float EndZ
        {
            get => _endZ;
            set
            {
                if (_endZ != value)
                {
                    _endZ = value;
                    //var camera = new SimpleCamera(); // Или передавать камеру из формы
                    //Point3D end3d = new Point3D(EndPoint, _endZ);

                    //PointF end2D = camera.ProjectTo2D(end3d);

                    /*float scaleFactor = SimpleCamera.GetScaleFactor(_endZ);
                    if (scaleFactor != _scaleFactor)
                    {
                        EndPoint = ScalePoint(EndPoint, _scaleFactor / scaleFactor, _scaleFactor / scaleFactor);
                        _scaleFactor = scaleFactor;
                    }*/

                    //EndPoint = end2D;
                    //OnPropertyChanged(); оно уже внутри присвоение StartPoint сделается
                }
            }
        }

        public LineElement3D(PointF position, float length, float angle, Color color, float thickness = 3) : base(position, length, angle, color, thickness)
        {
        }
        public LineElement3D(PointF startPoint, PointF endPoint, Color color, float thickness = 3f) : base(startPoint, endPoint, color, thickness)
        {
        }

        public bool SetStartZ(Rectangle drawingArea, GroupManager groupManager, float value) 
        {
            PointF center;
            if (GroupId == null)
            {
                center = new PointF((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);
            }
            else 
            {
                center = groupManager.GetGroupCenter(GroupId);
            }
            _startZ = value;
            float scaleFactor = SimpleCamera.GetScaleFactor(_startZ);
            if (scaleFactor != _scaleFactorS)
            {
                PointF testPoint = ScalePoint(StartPoint, center, _scaleFactorS / scaleFactor, _scaleFactorS / scaleFactor);
                if (drawingArea.Contains(Point.Round(testPoint)))
                {
                    StartPoint = testPoint;
                    _scaleFactorS = scaleFactor;
                }
                else
                    return false;
            }
            return true;
        }

        public bool SetEndZ(Rectangle drawingArea, GroupManager groupManager, float value)
        {
            PointF center;
            if (GroupId == null)
            {
                center = new PointF((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);
            }
            else
            {
                center = groupManager.GetGroupCenter(GroupId);
            }
            _endZ = value;
            float scaleFactor = SimpleCamera.GetScaleFactor(_endZ);
            if (scaleFactor != _scaleFactorE)
            {
                PointF testPoint = ScalePoint(EndPoint, center, _scaleFactorE / scaleFactor, _scaleFactorE / scaleFactor);
                if (drawingArea.Contains(Point.Round(testPoint)))
                {
                    EndPoint = testPoint;
                    _scaleFactorE = scaleFactor;
                }
                else
                    return false;
            }
            return true;
        }

        //private PointF ScalePoint(PointF point, PointF center, float sx, float sy)
        //{
        //    float dx = point.X - center.X;
        //    float dy = point.Y - center.Y;

        //    return new PointF(
        //        center.X + dx * sx,
        //        center.Y + dy * sy
        //    );
        //}

        //public override void Draw(Graphics graphics)
        //{
        //    //var camera = new SimpleCamera(); // Или передавать камеру из формы

        //    //PointF start2D = camera.ProjectTo2D(StartPoint); Теоретически это уже не нужно -
        //    //предусмотрено при присвоении z координате других цифр
        //    //PointF end2D = camera.ProjectTo2D(EndPoint);

        //    using (Pen pen = new Pen(Color, Thickness))
        //    {
        //        pen.EndCap = LineCap.Round;
        //        pen.StartCap = LineCap.Round;
        //        graphics.DrawLine(pen, start2D, end2D);
        //    }
        //}

        //public override void DrawSelection(Graphics graphics) Это тоже уже предусмотрено
        //{
        //    PointF start2D = StartPoint.ToPoint2D();
        //    PointF end2D = EndPoint.ToPoint2D();

        //    using (Pen selectionPen = new Pen(Color.Blue, 1))
        //    {
        //        selectionPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
        //        var bbox = GetBoundingBox();
        //        graphics.DrawRectangle(selectionPen, bbox.X, bbox.Y, bbox.Width, bbox.Height);
        //    }

        //    float handleSize = 6f;
        //    using (Brush handleBrush = new SolidBrush(Color.Red))
        //    {
        //        graphics.FillRectangle(handleBrush,
        //            start2D.X - handleSize / 2, start2D.Y - handleSize / 2,
        //            handleSize, handleSize);
        //        graphics.FillRectangle(handleBrush,
        //            end2D.X - handleSize / 2, end2D.Y - handleSize / 2,
        //            handleSize, handleSize);
        //    }
        //}
    }
}
