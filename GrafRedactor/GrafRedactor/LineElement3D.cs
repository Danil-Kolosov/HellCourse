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
        private Point3D _startPoint3D;
        private Point3D _endPoint3D;

        private float _startZ;
        private float _endZ;
        private float _scaleFactorS = 1;
        private float _scaleFactorE = 1;
        private Point3D _startPointR;

        public Point3D StartPoint3D
        {
            get => _startPoint3D;
            set
            {
                if (_startPoint3D != value)
                {
                    _startPoint3D = value;
                    Update2DProjection();
                    OnPropertyChanged();
                }
            }
        }

        public Point3D EndPoint3D
        {
            get => _endPoint3D;
            set
            {
                if (_endPoint3D != value)
                {
                    _endPoint3D = value;
                    Update2DProjection();
                    OnPropertyChanged();
                }
            }
        }

        public LineElement3D(Point3D startPoint3D, Point3D endPoint3D, Color color, float thickness = 3f)
            : base(PointF.Empty, PointF.Empty, color, thickness)
        {
            _startPoint3D = startPoint3D;
            _endPoint3D = endPoint3D;
            Update2DProjection();
            is3D = true;
        }

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
        //public float StartZ
        //{
        //    get => _startZ;
        //    set
        //    {
        //        if (_startZ != value)
        //        {
        //            _startZ = value;
        //            //var camera = new SimpleCamera(); // Или передавать камеру из формы
        //            //Point3D start3d = new Point3D(StartPoint, _startZ);

        //            //PointF start2D = camera.ProjectTo2D(start3d);


        //            /*float scaleFactor = SimpleCamera.GetScaleFactor(_startZ);
        //            if(scaleFactor != _scaleFactor)
        //            { 
        //                StartPoint = ScalePoint(StartPoint, _scaleFactor / scaleFactor, _scaleFactor / scaleFactor);
        //                _scaleFactor = scaleFactor;
        //            }*/


        //            //Position = start2D; оно уже внутри присвоение StartPoint сделается
        //            //OnPropertyChanged(); это тоже
        //        }
        //    }
        //}

        //public float EndZ
        //{
        //    get => _endZ;
        //    set
        //    {
        //        if (_endZ != value)
        //        {
        //            _endZ = value;
        //            //var camera = new SimpleCamera(); // Или передавать камеру из формы
        //            //Point3D end3d = new Point3D(EndPoint, _endZ);

        //            //PointF end2D = camera.ProjectTo2D(end3d);

        //            /*float scaleFactor = SimpleCamera.GetScaleFactor(_endZ);
        //            if (scaleFactor != _scaleFactor)
        //            {
        //                EndPoint = ScalePoint(EndPoint, _scaleFactor / scaleFactor, _scaleFactor / scaleFactor);
        //                _scaleFactor = scaleFactor;
        //            }*/

        //            //EndPoint = end2D;
        //            //OnPropertyChanged(); оно уже внутри присвоение StartPoint сделается
        //        }
        //    }
        //}

        public LineElement3D(PointF position, float length, float angle, Color color, float thickness = 3) : base(position, length, angle, color, thickness)
        {
            is3D = true;
        }
        public LineElement3D(PointF startPoint, PointF endPoint, Color color, float thickness = 3f) : base(startPoint, endPoint, color, thickness)
        {
            //StartPoint3D = new Point3D(startPoint);
            //EndPoint3D = new Point3D(endPoint);
            _startPoint3D = new Point3D(startPoint);
            _endPoint3D = new Point3D(endPoint);
            Update2DProjection();
            is3D = true;
        }

        //public bool SetStartZ(Rectangle drawingArea, GroupManager groupManager, float value) 
        //{
        //    PointF center;
        //    if (GroupId == null)
        //    {
        //        center = new PointF((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);
        //    }
        //    else 
        //    {
        //        center = groupManager.GetGroupCenter(GroupId);
        //    }
        //    _startZ = value;
        //    float scaleFactor = SimpleCamera.GetScaleFactor(_startZ);
        //    if (scaleFactor != _scaleFactorS)
        //    {
        //        PointF testPoint = ScalePoint(StartPoint, center, _scaleFactorS / scaleFactor, _scaleFactorS / scaleFactor);
        //        if (drawingArea.Contains(Point.Round(testPoint)))
        //        {
        //            StartPoint = testPoint;
        //            _scaleFactorS = scaleFactor;
        //        }
        //        else
        //            return false;
        //    }
        //    return true;
        //}

        //public bool SetEndZ(Rectangle drawingArea, GroupManager groupManager, float value)
        //{
        //    PointF center;
        //    if (GroupId == null)
        //    {
        //        center = new PointF((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);
        //    }
        //    else
        //    {
        //        center = groupManager.GetGroupCenter(GroupId);
        //    }
        //    _endZ = value;
        //    float scaleFactor = SimpleCamera.GetScaleFactor(_endZ);
        //    if (scaleFactor != _scaleFactorE)
        //    {
        //        PointF testPoint = ScalePoint(EndPoint, center, _scaleFactorE / scaleFactor, _scaleFactorE / scaleFactor);
        //        if (drawingArea.Contains(Point.Round(testPoint)))
        //        {
        //            EndPoint = testPoint;
        //            _scaleFactorE = scaleFactor;
        //        }
        //        else
        //            return false;
        //    }
        //    return true;
        //}

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

        private void Update2DProjection()
        {

            PointF center;

            // Определяем центр масштабирования
            if (!string.IsNullOrEmpty(GroupId) && SimpleCamera.GroupManager != null)
            {
                // Если линия в группе - используем центр группы
                center = SimpleCamera.GroupManager.GetGroupCenter(GroupId);
            }
            else
            {
                // Иначе используем центр линии
                center = new PointF(
                    (_startPoint3D.X + _endPoint3D.X) / 2,
                    (_startPoint3D.Y + _endPoint3D.Y) / 2
                );
            }

            // Получаем масштабные коэффициенты для каждой точки
            float startScale = SimpleCamera.GetScaleFactor(_startPoint3D.Z);
            float endScale = SimpleCamera.GetScaleFactor(_endPoint3D.Z);

            // Масштабируем точки относительно выбранного центра
            PointF start2D = SimpleCamera.ScalePoint(
                new PointF(_startPoint3D.X, _startPoint3D.Y), center, startScale);

            PointF end2D = SimpleCamera.ScalePoint(
                new PointF(_endPoint3D.X, _endPoint3D.Y), center, endScale);

            // Проверяем границы
            var drawingArea = SimpleCamera.DrawingArea;
            if (!drawingArea.Contains(Point.Round(start2D)) || !drawingArea.Contains(Point.Round(end2D)))
            {
                // Если вышли за границы - корректируем Z
                // (здесь может быть более сложная логика коррекции)
                return;
            }

            // Обновляем 2D координаты
            //StartPoint = start2D;
            //EndPoint = end2D;
            //Position = start2D;
            // Обновляем 2D координаты БЕЗ вызова событий
            _startPoint = start2D;
            _endPoint = end2D;
            _position = start2D;
            UpdateLengthAndRotation();

            //// Преобразуем 3D точки в 2D проекцию
            //PointF start2D, end2D;
            //(start2D, end2D, _scaleFactorS, _scaleFactorE) = SimpleCamera.ProjectTo2D(_startPoint3D, _endPoint3D, _scaleFactorS, _scaleFactorE, /*StartPoint, EndPoint, */GroupId);

            //// Обновляем базовые свойства без вызова OnPropertyChanged
            //StartPoint = start2D;
            //EndPoint = end2D;
            //Position = start2D;

            //// Пересчитываем длину и угол
            //UpdateLengthAndRotation();
        }

        public bool ChangeZ(float startZDelta, float endZDelta)
        {
            // Сохраняем старые значения для отката
            float oldStartZ = _startPoint3D.Z;
            float oldEndZ = _endPoint3D.Z;

            // Пробуем изменить Z
            _startPoint3D.Z += startZDelta;
            _endPoint3D.Z += endZDelta;

            // Обновляем проекцию
            Update2DProjection();

            // Проверяем успешность операции
            var drawingArea = SimpleCamera.DrawingArea;
            var bbox = GetBoundingBox();

            bool isWithinBounds = drawingArea.Contains(Point.Round(bbox.Location)) &&
                                 drawingArea.Contains(Point.Round(new PointF(bbox.Right, bbox.Bottom)));

            if (!isWithinBounds)
            {
                // Если вышла за границы - откатываем изменения
                _startPoint3D.Z = oldStartZ;
                _endPoint3D.Z = oldEndZ;
                Update2DProjection();
                return false;
            }

            return true;
        }

        public void Move3D(Point3D delta)
        {
            StartPoint3D = new Point3D(
                _startPoint3D.X + delta.X,
                _startPoint3D.Y + delta.Y,
                _startPoint3D.Z + delta.Z
            );
            EndPoint3D = new Point3D(
                _endPoint3D.X + delta.X,
                _endPoint3D.Y + delta.Y,
                _endPoint3D.Z + delta.Z
            );
        }

        // Переопределяем методы для работы с 3D
        public override void Move(PointF delta, float height, float width) 
        {
            base.Move(delta, height, width);
            StartPoint3D.X = StartPoint.X;
            StartPoint3D.Y = StartPoint.Y;

            EndPoint3D.X = EndPoint.X;
            EndPoint3D.Y = EndPoint.Y;
            //Move3D(new Point3D(delta.X, delta.Y, 0));
        }

        public void Rotate3D(Point3D center, float angleX, float angleY, float angleZ)
        {
            StartPoint3D = RotatePoint3D(_startPoint3D, center, angleX, angleY, angleZ);
            EndPoint3D = RotatePoint3D(_endPoint3D, center, angleX, angleY, angleZ);
            Update2DProjection();
        }

        private Point3D RotatePoint3D(Point3D point, Point3D center, float angleX, float angleY, float angleZ)
        {
            // Перенос в систему координат с центром в center
            float x = point.X - center.X;
            float y = point.Y - center.Y;
            float z = point.Z - center.Z;

            // Преобразуем углы в радианы
            float radX = angleX * (float)Math.PI / 180f;
            float radY = angleY * (float)Math.PI / 180f;
            float radZ = angleZ * (float)Math.PI / 180f;

            // Вычисляем синусы и косинусы
            float cosX = (float)Math.Cos(radX), sinX = (float)Math.Sin(radX);
            float cosY = (float)Math.Cos(radY), sinY = (float)Math.Sin(radY);
            float cosZ = (float)Math.Cos(radZ), sinZ = (float)Math.Sin(radZ);

            // Матрица вращения (комбинированная: Z * Y * X)
            float x1 = x * cosY * cosZ + y * (sinX * sinY * cosZ - cosX * sinZ) + z * (cosX * sinY * cosZ + sinX * sinZ);
            float y1 = x * cosY * sinZ + y * (sinX * sinY * sinZ + cosX * cosZ) + z * (cosX * sinY * sinZ - sinX * cosZ);
            float z1 = x * -sinY + y * sinX * cosY + z * cosX * cosY;

            // Возврат в исходную систему координат
            return new Point3D(
                x1 + center.X,
                y1 + center.Y,
                z1 + center.Z
            );
        }
    }
}
