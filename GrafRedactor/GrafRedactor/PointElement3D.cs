using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GrafRedactor
{
    public class PointElement3D : FigureElement
    {
        private Point3D _point3D;
        private Point3D _zeroRotatedPoint;
        private float _size = 6f; // Размер точки

        public Point3D Point3D
        {
            get => _point3D;
            set
            {
                if (_point3D != value)
                {
                    _point3D = value;
                    _zeroRotatedPoint = value;
                    Update2DProjection();
                    OnPropertyChanged();
                }
            }
        }

        public Point3D ZeroRotatedPoint
        {
            get => _zeroRotatedPoint;
            set
            {
                if (_zeroRotatedPoint != value)
                {
                    _zeroRotatedPoint = value;
                    _point3D = value;
                    Update2DProjection();
                    OnPropertyChanged();
                }
            }
        }

        public float Size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged();
                }
            }
        }

        public PointElement3D(Point3D point3D)
        {
            _point3D = point3D;
            _zeroRotatedPoint = point3D;
            Color = Color.Black; // Черный цвет по умолчанию
            Update2DProjection();
            is3D = true;
        }

        public PointElement3D(PointF point2D) : this(new Point3D(point2D))
        {
        }

        public PointElement3D(float x, float y, float z = 0) : this(new Point3D(x, y, z))
        {
        }

        private void Update2DProjection()
        {
            // Ортографическая проекция - используем только X,Y координаты
            PointF point2D = new PointF(_point3D.X, _point3D.Y);

            // Центрирование (опционально)
            var drawingArea = SimpleCamera.DrawingArea;
            float centerX = drawingArea.Width / 2;
            float centerY = drawingArea.Height / 2;

            // Обновляем позицию
            _position = point2D;
        }

        // Основные методы
        public override bool ContainsPoint(PointF point)
        {
            // Проверяем, находится ли точка внутри кружочка
            float distance = (float)Math.Sqrt(
                Math.Pow(point.X - _position.X, 2) +
                Math.Pow(point.Y - _position.Y, 2));
            return distance <= _size;
        }

        public override RectangleF GetBoundingBox()
        {
            // Возвращаем прямоугольник, ограничивающий точку
            return new RectangleF(
                _position.X - _size / 2,
                _position.Y - _size / 2,
                _size,
                _size
            );
        }

        public override void Draw(Graphics graphics, LineCap endType = LineCap.Round)
        {
            //временно убрано
            //using (Brush brush = new SolidBrush(Color))
            //{
            //    graphics.FillEllipse(brush,
            //        _position.X - _size / 2,
            //        _position.Y - _size / 2,
            //        _size,
            //        _size);
            //}
        }

        public override void DrawSelection(Graphics graphics)
        {
            // Рисуем выделение - синий кружок вокруг точки
            //using (Pen selectionPen = new Pen(Color.Blue, 2f))
            //{
            //    selectionPen.DashStyle = DashStyle.Dash;
            //    graphics.DrawEllipse(selectionPen,
            //        _position.X - _size,
            //        _position.Y - _size,
            //        _size * 2,
            //        _size * 2);
            //}
        }

        // 3D операции
        public void Move3D(Point3D delta)
        {
            Point3D = new Point3D(
                _point3D.X + delta.X,
                _point3D.Y + delta.Y,
                _point3D.Z + delta.Z
            );
            ZeroRotatedPoint = _point3D;
        }

        public override void Move(PointF delta, float height, float weight, float deltaZ, string axeName)
        {
            switch (axeName)
            {
                case "xoy":
                    _position = new PointF(_position.X + delta.X, _position.Y + delta.Y);
                    Point3D = new Point3D(_point3D.X + delta.X, _point3D.Y + delta.Y, _point3D.Z);
                    ZeroRotatedPoint = _point3D;
                    break;
                case "yoz":
                    _position = new PointF(_position.X + delta.X, _position.Y + delta.Y);
                    ZeroRotatedPoint = new Point3D(_zeroRotatedPoint.X, _zeroRotatedPoint.Y + delta.X, _zeroRotatedPoint.Z + delta.Y);
                    break;
                case "xoz":
                    _position = new PointF(_position.X + delta.X, _position.Y + delta.Y);
                    ZeroRotatedPoint = new Point3D(_zeroRotatedPoint.X + delta.X, _zeroRotatedPoint.Y, _zeroRotatedPoint.Z + delta.Y);
                    break;
            }
            OnPropertyChanged();
        }

        public void Rotate3D(Point3D center, float angleX, float angleY, float angleZ)
        {
            _point3D = RotatePoint3D(_zeroRotatedPoint, center, angleX, angleY, angleZ);
            ZeroRotatedPoint = _point3D;
            Update2DProjection();
        }

        private Point3D RotatePoint3D(Point3D point, Point3D center, float angleX, float angleY, float angleZ)
        {
            // Аналогично реализации в LineElement3D
            float x = point.X - center.X;
            float y = point.Y - center.Y;
            float z = point.Z - center.Z;

            float radX = angleX * (float)Math.PI / 180f;
            float radY = angleY * (float)Math.PI / 180f;
            float radZ = angleZ * (float)Math.PI / 180f;

            float cosX = (float)Math.Cos(radX), sinX = (float)Math.Sin(radX);
            float cosY = (float)Math.Cos(radY), sinY = (float)Math.Sin(radY);
            float cosZ = (float)Math.Cos(radZ), sinZ = (float)Math.Sin(radZ);

            float x1 = x * cosY * cosZ + y * (sinX * sinY * cosZ - cosX * sinZ) + z * (cosX * sinY * cosZ + sinX * sinZ);
            float y1 = x * cosY * sinZ + y * (sinX * sinY * sinZ + cosX * cosZ) + z * (cosX * sinY * sinZ - sinX * cosZ);
            float z1 = x * -sinY + y * sinX * cosY + z * cosX * cosY;

            return new Point3D(
                x1 + center.X,
                y1 + center.Y,
                z1 + center.Z);
        }

        // Остальные обязательные методы (можно оставить базовую реализацию)
        public override void Rotate(float angle, PointF center = new PointF())
        {
            // Для точки вращение не имеет особого смысла, но можно реализовать если нужно
        }

        public override void ScaleAverage(float scaleFactor)
        {
            Size *= scaleFactor;
        }

        public override void Scale(PointF center, float sx, float sy)
        {
            // Масштабирование позиции точки
            float dx = _position.X - center.X;
            float dy = _position.Y - center.Y;

            _position = new PointF(
                center.X + dx * sx,
                center.Y + dy * sy
            );

            // Обновляем 3D координаты
            Point3D = new Point3D(_position.X, _position.Y, _point3D.Z);
            OnPropertyChanged();
        }

        public override void Mirror(bool horizontal)
        {
            // Зеркальное отражение точки
        }

        public override void Mirror(float A, float B, float C)
        {
            // Зеркальное отражение относительно прямой
        }

        public override void Projection(string coordinateAxis)
        {
            // Проецирование на координатные плоскости
            switch (coordinateAxis.ToLower())
            {
                case "xoy":
                case "xy":
                    ZeroRotatedPoint = new Point3D(_zeroRotatedPoint.X, _zeroRotatedPoint.Y, 0);
                    break;
                case "xoz":
                case "xz":
                    ZeroRotatedPoint = new Point3D(_zeroRotatedPoint.X, 0, _zeroRotatedPoint.Z);
                    break;
                case "yoz":
                case "yz":
                    ZeroRotatedPoint = new Point3D(0, _zeroRotatedPoint.Y, _zeroRotatedPoint.Z);
                    break;
            }
            Point3D = ZeroRotatedPoint;
        }

        // Дополнительные полезные методы
        public PointF Get2DPosition() => _position;

        public Point3D GetRealPoint() => _zeroRotatedPoint;

        public void SetRealPoint(Point3D point, Point3D center, float totalRotationX, float totalRotationY, float totalRotationZ)
        {
            ZeroRotatedPoint = point;
            _point3D = RotatePoint3D(ZeroRotatedPoint, center, totalRotationX, totalRotationY, totalRotationZ);
            Update2DProjection();
        }
    }
}