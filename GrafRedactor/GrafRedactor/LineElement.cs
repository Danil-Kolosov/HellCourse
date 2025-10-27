using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GrafRedactor
{
    public class LineElement : FigureElement
    {
        protected float _length;
        protected PointF _startPoint;
        protected PointF _endPoint;

        public float Length
        {
            get => _length;
            set
            {
                if (_length != value)
                {
                    _length = value;
                    UpdateEndPoint();
                    OnPropertyChanged();
                }
            }
        }

        public float Thickness { get; set; }

        public PointF StartPoint
        {
            get => _startPoint;
            set
            {
                if (_startPoint != value)
                {
                    _startPoint = value;
                    Position = _startPoint;
                    UpdateLengthAndRotation();
                    OnPropertyChanged();
                }
            }
        }

        public PointF EndPoint
        {
            get => _endPoint;
            set
            {
                if (_endPoint != value)
                {
                    _endPoint = value;
                    UpdateLengthAndRotation();
                    OnPropertyChanged();
                }
            }
        }

        public LineElement(PointF startPoint, PointF endPoint, Color color, float thickness = 3f)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            Color = color;
            Thickness = thickness;
            UpdateLengthAndRotation();
            Position = _startPoint;
        }

        public LineElement(PointF position, float length, float angle, Color color, float thickness = 3f)
        {
            Position = position;
            _startPoint = position;
            _length = length;
            Rotation = angle;
            Color = color;
            Thickness = thickness;
            UpdateEndPoint();
        }

        private void UpdateEndPoint()
        {
            float angleRad = Rotation * (float)Math.PI / 180f;
            _endPoint = new PointF(
                _startPoint.X + _length * (float)Math.Cos(angleRad),
                _startPoint.Y + _length * (float)Math.Sin(angleRad)
            );
        }

        protected void UpdateLengthAndRotation()
        {
            float dx = _endPoint.X - _startPoint.X;
            float dy = _endPoint.Y - _startPoint.Y;
            _length = (float)Math.Sqrt(dx * dx + dy * dy);
            Rotation = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
        }

        // Проверка попадания точки на линию
        public override bool ContainsPoint(PointF point)
        {
            // Упрощенная проверка - расстояние от точки до отрезка
            float distance = DistanceToLine(point, _startPoint, _endPoint);
            return distance <= Math.Max(Thickness, 10f); // Учитываем толщину линии + запас
        }

        private float DistanceToLine(PointF point, PointF lineStart, PointF lineEnd)
        {
            float lineLengthSquared = (lineEnd.X - lineStart.X) * (lineEnd.X - lineStart.X) +
                                    (lineEnd.Y - lineStart.Y) * (lineEnd.Y - lineStart.Y);

            if (lineLengthSquared == 0)
                return Distance(point, lineStart);

            float t = Math.Max(0, Math.Min(1,
                ((point.X - lineStart.X) * (lineEnd.X - lineStart.X) +
                 (point.Y - lineStart.Y) * (lineEnd.Y - lineStart.Y)) / lineLengthSquared));

            PointF projection = new PointF(
                lineStart.X + t * (lineEnd.X - lineStart.X),
                lineStart.Y + t * (lineEnd.Y - lineStart.Y)
            );

            return Distance(point, projection);
        }

        private float Distance(PointF p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public override RectangleF GetBoundingBox()
        {
            float minX = Math.Min(_startPoint.X, _endPoint.X);
            float minY = Math.Min(_startPoint.Y, _endPoint.Y);
            float maxX = Math.Max(_startPoint.X, _endPoint.X);
            float maxY = Math.Max(_startPoint.Y, _endPoint.Y);

            return new RectangleF(minX - Thickness, minY - Thickness,
                                maxX - minX + Thickness * 2, maxY - minY + Thickness * 2);
        }

        public override void Move(PointF delta, float height, float weight)
        {
            if (_startPoint.X + delta.X > weight)
                delta.X = weight - _startPoint.X;
            if (_startPoint.Y + delta.Y > height)
                delta.Y = height - _startPoint.Y;
            if (_endPoint.X + delta.X > weight)
                delta.X = weight - _endPoint.X;
            if (_endPoint.Y + delta.Y > height)
                delta.Y = height - _endPoint.Y;

            if (_startPoint.X + delta.X < 0)
                delta.X = -_startPoint.X;
            if (_startPoint.Y + delta.Y < 0)
                delta.Y = -_startPoint.Y;
            if (_endPoint.X + delta.X < 0)
                delta.X = -_endPoint.X;
            if (_endPoint.Y + delta.Y < 0)
                delta.Y = -_endPoint.Y;


            StartPoint = new PointF(/*Math.Max(0, Math.Min(*/_startPoint.X + delta.X/*, weight))*/, /*Math.Max(0, Math.Min(*/_startPoint.Y + delta.Y/*, height))*/);
            EndPoint = new PointF(/*Math.Max(0, Math.Min(*/_endPoint.X + delta.X/*, weight))*/, /*Math.Max(0, Math.Min(*/_endPoint.Y + delta.Y/*, height))*/);
        }

        public override void Rotate(float angle)
        {
            // Вращение вокруг центра линии
            PointF center = new PointF(
                (_startPoint.X + _endPoint.X) / 2,
                (_startPoint.Y + _endPoint.Y) / 2
            );

            float angleRad = angle * (float)Math.PI / 180f; //Из-за того, что система координат экрана перевернутая
                                                             //- в нормальной системе будет выглядеть что формула вращает
                                                             //при положительном по часововй,
                                                             //но на самом деле все правильно - она против, но в перевернутой
                                                             //системе координат
                                                             //решение - минус у угла
            float cos = (float)Math.Cos(angleRad);
            float sin = (float)Math.Sin(angleRad);

            // Поворачиваем точки относительно центра
            StartPoint = RotatePoint(_startPoint, center, cos, sin);
            EndPoint = RotatePoint(_endPoint, center, cos, sin);
        }

        private PointF RotatePoint(PointF point, PointF center, float cos, float sin)
        {
            //Переносим точку в систему координат с центром в center
            float translatedX = point.X - center.X;
            float translatedY = point.Y - center.Y;

            // Поворачиваем
            float rotatedX = translatedX * cos - translatedY * sin;
            float rotatedY = translatedX * sin + translatedY * cos;

            // Возвращаем в исходную систему координат
            return new PointF(rotatedX + center.X, rotatedY + center.Y);
            //return new PointF(point.X*cos - point.Y*sin, point.X*sin + point.Y*cos);
        }

        public override void Scale(PointF center, float sx, float sy)
        {            
            StartPoint = ScalePoint(_startPoint, center, sx, sy);
            EndPoint = ScalePoint(_endPoint, center, sx, sy);
        }

        protected PointF ScalePoint(PointF point, PointF center, float sx, float sy)
        {
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;

            return new PointF(
                center.X + dx * sx,
                center.Y + dy * sy
            );
        }

        public override void ScaleAverage(float scaleFactor)
        {
            PointF center = new PointF(
                (_startPoint.X + _endPoint.X) / 2,
                (_startPoint.Y + _endPoint.Y) / 2
            );

            StartPoint = ScalePoint(_startPoint, center, scaleFactor, scaleFactor);
            EndPoint = ScalePoint(_endPoint, center, scaleFactor, scaleFactor);
        }

        //private PointF ScalePoint(PointF point, PointF center, float scaleFactor) ///было!
        //{
        //    float dx = point.X - center.X;
        //    float dy = point.Y - center.Y;

        //    return new PointF(
        //        center.X + dx * scaleFactor,
        //        center.Y + dy * scaleFactor
        //    );
        //}

        public override void Mirror(bool horizontal)
        {
            PointF center = new PointF(
                (_startPoint.X + _endPoint.X) / 2,
                (_startPoint.Y + _endPoint.Y) / 2
            );

            if (horizontal)
            {
                // Зеркалирование по горизонтали
                StartPoint = new PointF(2 * center.X - _startPoint.X, _startPoint.Y);
                EndPoint = new PointF(2 * center.X - _endPoint.X, _endPoint.Y);
            }
            else
            {
                // Зеркалирование по вертикали
                StartPoint = new PointF(_startPoint.X, 2 * center.Y - _startPoint.Y);
                EndPoint = new PointF(_endPoint.X, 2 * center.Y - _endPoint.Y);
            }
        }

        public override void Mirror(float A, float B, float C)
        {
            StartPoint = MirrorPoint(StartPoint, A, B, C);
            EndPoint = MirrorPoint(EndPoint, A, B, C);
        }

        private PointF MirrorPoint(PointF point, float A, float B, float C)
        {
            // Формула отражения точки относительно прямой Ax + By + C = 0
            float denominator = A * A + B * B;

            if (Math.Abs(denominator) < 0.0001f)
                return point; // Прямая вырождена

            //float x = point.X;
            //float y = point.Y;

            //// Вычисляем отраженную точку
            //float mirroredX = x - 2 * A * (A * x + B * y + C) / denominator;
            //float mirroredY = y - 2 * B * (A * x + B * y + C) / denominator;

            // Нормализуем коэффициенты чтобы избежать численных ошибок
            float length = (float)Math.Sqrt(A * A + B * B);
            if (length < 0.0001f) return point;

            A /= length;
            B /= length;
            C /= length;

            // Расстояние от точки до прямой (со знаком)
            float distance = A * point.X + B * point.Y + C;

            // Отраженная точка
            PointF mirrored = new PointF(
                point.X - 2 * A * distance,
                point.Y - 2 * B * distance
            );

            //return new PointF(mirroredX, mirroredY);
            return mirrored;
        }

        public override void Draw(Graphics graphics)
        {
            using (Pen pen = new Pen(Color, Thickness))
            {
                pen.EndCap = LineCap.Round;
                pen.StartCap = LineCap.Round;
                graphics.DrawLine(pen, _startPoint, _endPoint);
            }
        }

        public override void DrawSelection(Graphics graphics)
        {
            // Рисуем bounding box
            using (Pen selectionPen = new Pen(Color.Blue, 1))
            {
                selectionPen.DashStyle = DashStyle.Dash;
                graphics.DrawRectangle(selectionPen,
                    GetBoundingBox().X, GetBoundingBox().Y,
                    GetBoundingBox().Width, GetBoundingBox().Height);
            }

            // Рисуем маркеры на концах линии
            float handleSize = 6f;
            using (Brush handleBrush = new SolidBrush(Color.Red))
            {
                graphics.FillRectangle(handleBrush,
                    _startPoint.X - handleSize / 2, _startPoint.Y - handleSize / 2,
                    handleSize, handleSize);
                graphics.FillRectangle(handleBrush,
                    _endPoint.X - handleSize / 2, _endPoint.Y - handleSize / 2,
                    handleSize, handleSize);
            }
        }

        public (float A, float B, float C, float Z) GetEquation()
        {
            // Уравнение прямой: Ax + By + C = 0
            float A = StartPoint.Y - EndPoint.Y;
            float B = EndPoint.X - StartPoint.X;
            float C = StartPoint.X * EndPoint.Y - EndPoint.X * StartPoint.Y;

            return (A, B, C, 0);
        }

        public override void LinkChange(FigureElement el)
        {
            if (el is LineElement line)
                StartPoint = line.EndPoint;
        }

        public override void Projection(string coordinateAxis) 
        {
            switch (coordinateAxis) 
            {
                case "x":
                    _startPoint.X = 0;
                    _endPoint.X = 0;
                    break;
                case "y":
                    _startPoint.Y = 0;
                    _endPoint.Y = 0;
                    break;
                default:
                    throw new ArgumentException("Выбрана не правильная ось");
            }
        }
    }
}