using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace GrafRedactor
{
    public class Cube3D : FigureElement
    {
        private List<LineElement3D> edges = new List<LineElement3D>();
        private Point3D center;
        private float size;
        private Color color;

        public Cube3D(Point3D center, float size, Color color)
        {
            this.center = center;
            this.size = size;
            this.color = color;

            InitializeEdges();
            UpdateCubeGeometry();
            is3D = true;
        }

        private void InitializeEdges()
        {
            // Очищаем старые ребра если они есть
            edges.Clear();

            // Создаем 12 ребер куба
            for (int i = 0; i < 12; i++)
            {
                var edge = new LineElement3D(new Point3D(0, 0, 0), new Point3D(0, 0, 0), color, 2f);
                edges.Add(edge);
            }
        }

        private void UpdateCubeGeometry()
        {
            // Вычисляем 8 вершин куба
            Point3D[] vertices = CalculateVertices();

            // Обновляем координаты ребер
            // Нижняя грань
            edges[0].StartPoint3D = vertices[0]; edges[0].EndPoint3D = vertices[1];
            //edges[1].OnChanged += edges[0].LinkChange;
            edges[0].OnChanged += edges[1].LinkChange;
            edges[1].StartPoint3D = vertices[1]; edges[1].EndPoint3D = vertices[2];
            edges[1].OnChanged += edges[2].LinkChange;
            edges[2].StartPoint3D = vertices[2]; edges[2].EndPoint3D = vertices[3];
            edges[2].OnChanged += edges[3].LinkChange;
            edges[3].StartPoint3D = vertices[3]; edges[3].EndPoint3D = vertices[0];

            // Верхняя грань
            edges[4].StartPoint3D = vertices[4]; edges[4].EndPoint3D = vertices[5];
            edges[5].StartPoint3D = vertices[5]; edges[5].EndPoint3D = vertices[6];
            edges[6].StartPoint3D = vertices[6]; edges[6].EndPoint3D = vertices[7];
            edges[7].StartPoint3D = vertices[7]; edges[7].EndPoint3D = vertices[4];

            // Вертикальные ребра
            edges[8].StartPoint3D = vertices[0]; edges[8].EndPoint3D = vertices[4];
            edges[9].StartPoint3D = vertices[1]; edges[9].EndPoint3D = vertices[5];
            edges[10].StartPoint3D = vertices[2]; edges[10].EndPoint3D = vertices[6];
            edges[11].StartPoint3D = vertices[3]; edges[11].EndPoint3D = vertices[7];
        }

        private Point3D[] CalculateVertices()
        {
            float halfSize = size / 2;

            return new Point3D[]
            {
                // Нижняя грань (задние вершины имеют меньший Z для перспективы)
                new Point3D(center.X - halfSize, center.Y - halfSize, center.Z - halfSize), // 0: лево-назад-низ
                new Point3D(center.X + halfSize, center.Y - halfSize, center.Z - halfSize), // 1: право-назад-низ
                new Point3D(center.X + halfSize, center.Y - halfSize, center.Z + halfSize), // 2: право-перед-низ
                new Point3D(center.X - halfSize, center.Y - halfSize, center.Z + halfSize), // 3: лево-перед-низ
                //new Point3D(center.X + halfSize, center.Y + halfSize, center.Z - halfSize * 0.7f), // 2: право-перед-низ (меньше Z)
                //new Point3D(center.X - halfSize, center.Y + halfSize, center.Z - halfSize * 0.7f), // 3: лево-перед-низ (меньше Z)
                
                // Верхняя грань
                new Point3D(center.X - halfSize, center.Y + halfSize, center.Z - halfSize), // 4: лево-перед-верх
                new Point3D(center.X + halfSize, center.Y + halfSize, center.Z - halfSize), // 5: право-перед-верх
                new Point3D(center.X + halfSize, center.Y + halfSize, center.Z + halfSize), // 6: право-зад-верх
                new Point3D(center.X - halfSize, center.Y + halfSize, center.Z + halfSize)  // 7: лево-зад-верх
                //new Point3D(center.X - halfSize, center.Y - halfSize, center.Z + halfSize), // 4: лево-перед-верх
                //new Point3D(center.X + halfSize, center.Y - halfSize, center.Z + halfSize), // 5: право-перед-верх
                //new Point3D(center.X + halfSize, center.Y + halfSize, center.Z + halfSize * 0.7f), // 6: право-зад-верх
                //new Point3D(center.X - halfSize, center.Y + halfSize, center.Z + halfSize * 0.7f)  // 7: лево-зад-верх
            };
        }

        // ОБНОВЛЯЕМ ВСЕ МЕТОДЫ ДВИЖЕНИЯ И ПРЕОБРАЗОВАНИЙ
        public override void Move(PointF delta, float height, float width, string axeName)
        {
            //center = new Point3D(center.X + delta.X, center.Y + delta.Y, center.Z);
            foreach (LineElement3D line in edges)
            {
                line.Move(delta, height, width);
            }
            center.X = center.X + delta.X;























            center.Y = center.Y + delta.Y;
            //UpdateCubeGeometry();
            //OnPropertyChanged();
        }

        public void Move3D(Point3D delta)
        {
            center.X += delta.X;
            center.Y += delta.Y;
            center.Z += delta.Z;
            foreach (LineElement3D line in edges)
            {
                line.Move3D(delta); //сделать как мув групп!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                    //главное - сделать чтобы рисовательная область и панель параметров не были закрываемы сверху и снизу
                                    //операции 3д
                                    //масштабирование
                                    //зеркалирование
                                    //проецирование
            }
            //UpdateCubeGeometry();
            //OnPropertyChanged();
        }

        public override void Rotate(float angle, PointF cent)
        {
            if (cent.IsEmpty)
                Rotate3D(0, 0, angle, center, Rectangle.Empty);
            else
                Rotate3D(0, 0, angle, new Point3D(cent), Rectangle.Empty);
        }

        private bool CanPerformRotation3D(float angleX, float angleY, float angleZ, Point3D center, Rectangle drawingArea)
        {

            foreach (var figure in edges)
            {
                if (figure is LineElement3D line)
                {
                    // Создаем копию для проверки
                    var testLine = new LineElement3D(line.ZeroRatatedStartPoint, line.ZeroRatatedEndPoint, line.Color, line.Thickness);
                    Point3D newStart = RotatePoint3D(testLine.ZeroRatatedStartPoint, center, angleX, angleY, angleZ);
                    Point3D newEnd = RotatePoint3D(testLine.ZeroRatatedEndPoint, center, angleX, angleY, angleZ);

                    // Обновляем реальные координаты
                    testLine.StartPoint3D = newStart;
                    testLine.ZeroRatatedEndPoint = newEnd;
                    testLine.StartPoint3D = newStart;
                    testLine.EndPoint3D = newEnd;

                    var bbox = testLine.GetBoundingBox();
                    if (bbox.Left < -drawingArea.Width / 2 || bbox.Right > drawingArea.Width/2 ||
                        bbox.Top < -drawingArea.Height / 2 || bbox.Bottom > drawingArea.Height/2)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Rotate3D(float angleX, float angleY, float angleZ, Point3D center, Rectangle drawingArea /*= new Rectangle()*/)
        {
            //if (CanPerformRotation3D(angleX, angleY, angleZ, center, drawingArea))
            //{
            //    // Вращаем всю группу как жесткое тело
            //    foreach (LineElement3D line3D in edges)
            //    {
            //        // Вращаем обе точки линии
            //        Point3D newStart = RotatePoint3D(line3D.ZeroRatatedStartPoint, center, angleX, angleY, angleZ);
            //        Point3D newEnd = RotatePoint3D(line3D.ZeroRatatedEndPoint, center, angleX, angleY, angleZ);

            //        // Обновляем реальные координаты
            //        //line3D.StartPoint3D = newStart;
            //        //line3D.ZeroRatatedEndPoint = newEnd;
            //        line3D.StartPoint3D = newStart;
            //        line3D.EndPoint3D = newEnd;

            //    }
            //}
            //return;

            if (CanPerformRotation3D(angleX, angleY, angleZ, center, drawingArea))
            {
                foreach (LineElement3D line in edges)
                {
                    line.Rotate3D(center/*new Point3D(0,0,0)ЭТО НЕ ЦЕНТР*/, angleX, angleY, angleZ);
                }

                this.center = RotatePoint3D(Center, center, angleX, angleY, angleZ);
                //если куб вращается относительно какой-то точки (начла координта), то почему-то его центр вращается не правильно
                //this.center.X = edges[0].StartPoint3D.X + size / 2;
                //this.center.Y = edges[0].StartPoint3D.Y + size / 2;
                //this.center.Z = edges[0].StartPoint3D.Z + size / 2;
                //сейчас считает правильно, \ти сверху непрвильно считают отчего-то так 
            }
            //пока так

            //// Поворачиваем каждую вершину относительно центра куба
            //Point3D[] vertices = CalculateVertices();
            //Point3D[] rotatedVertices = new Point3D[8];

            //for (int i = 0; i < 8; i++)
            //{
            //    rotatedVertices[i] = RotatePoint3D(vertices[i], center, angleX, angleY, angleZ);
            //}

            //// Обновляем ребра с новыми вершинами
            //UpdateEdgesWithVertices(rotatedVertices);
            //OnPropertyChanged();
        }

        public void Rotate3DWithScene(float angleX, float angleY, float angleZ, Point3D center) 
        {
            foreach (LineElement3D line in edges)
            {
                line.Rotate3DWithScene(center/*new Point3D(0,0,0)ЭТО НЕ ЦЕНТР*/, angleX, angleY, angleZ);
            }

            OnPropertyChanged();
        }

        private void UpdateEdgesWithVertices(Point3D[] vertices)
        {
            edges[0].StartPoint3D = vertices[0]; edges[0].EndPoint3D = vertices[1];
            edges[1].StartPoint3D = vertices[1]; edges[1].EndPoint3D = vertices[2];
            edges[2].StartPoint3D = vertices[2]; edges[2].EndPoint3D = vertices[3];
            edges[3].StartPoint3D = vertices[3]; edges[3].EndPoint3D = vertices[0];
            edges[4].StartPoint3D = vertices[4]; edges[4].EndPoint3D = vertices[5];
            edges[5].StartPoint3D = vertices[5]; edges[5].EndPoint3D = vertices[6];
            edges[6].StartPoint3D = vertices[6]; edges[6].EndPoint3D = vertices[7];
            edges[7].StartPoint3D = vertices[7]; edges[7].EndPoint3D = vertices[4];
            edges[8].StartPoint3D = vertices[0]; edges[8].EndPoint3D = vertices[4];
            edges[9].StartPoint3D = vertices[1]; edges[9].EndPoint3D = vertices[5];
            edges[10].StartPoint3D = vertices[2]; edges[10].EndPoint3D = vertices[6];
            edges[11].StartPoint3D = vertices[3]; edges[11].EndPoint3D = vertices[7];
        }

        private Point3D RotatePoint3D(Point3D point, Point3D center, float angleX, float angleY, float angleZ)
        {
            // Перенос в систему координат с центром в center
            Point3D translated = new Point3D(
                point.X - center.X,
                point.Y - center.Y,
                point.Z - center.Z
            );

            Point3D rotated = translated;

            // Вращение вокруг Z
            if (Math.Abs(angleZ) > 0.001f)
            {
                float angleRad = angleZ * (float)Math.PI / 180f;
                float cos = (float)Math.Cos(angleRad);
                float sin = (float)Math.Sin(angleRad);

                rotated = new Point3D(
                    rotated.X * cos - rotated.Y * sin,
                    rotated.X * sin + rotated.Y * cos,
                    rotated.Z
                );
            }

            // Вращение вокруг Y
            if (Math.Abs(angleY) > 0.001f)
            {
                float angleRad = angleY * (float)Math.PI / 180f;
                float cos = (float)Math.Cos(angleRad);
                float sin = (float)Math.Sin(angleRad);

                rotated = new Point3D(
                    rotated.X * cos + rotated.Z * sin,
                    rotated.Y,
                    -rotated.X * sin + rotated.Z * cos
                );
            }

            // Возврат в исходную систему координат
            return new Point3D(
                rotated.X + center.X,
                rotated.Y + center.Y,
                rotated.Z + center.Z
            );
        }

        // ОСТАЛЬНЫЕ МЕТОДЫ FigureElement
        public override bool ContainsPoint(PointF point)
        {
            foreach (var edge in edges)
            {
                if (edge.ContainsPoint(point))
                    return true;
            }
            return false;
        }

        public override RectangleF GetBoundingBox()
        {
            if (edges.Count == 0) return RectangleF.Empty;

            RectangleF bbox = edges[0].GetBoundingBox();
            for (int i = 1; i < edges.Count; i++)
            {
                bbox = RectangleF.Union(bbox, edges[i].GetBoundingBox());
            }
            return bbox;
        }

        public void Scale(Point3D center, float sx, float sy, float sz)
        {
            size *= (sx + sy + sz) / 3;
            //UpdateCubeGeometry();
            foreach (LineElement3D line in edges)
            {
                line.Scale(this.center, sx, sy, sz);
            }
            OnPropertyChanged();
        }

        public override void ScaleAverage(float scaleFactor)
        {
            size *= scaleFactor;
            //UpdateCubeGeometry();
            foreach (LineElement3D line in edges)
            {
                line.Scale(this.center, scaleFactor, scaleFactor, scaleFactor);
            }
            OnPropertyChanged();
        }

        public override void Mirror(bool horizontal)
        {
            // Простое зеркалирование - инвертируем соответствующие координаты
            if (horizontal)
            {
                center = new Point3D(-center.X, center.Y, center.Z);
            }
            else
            {
                center = new Point3D(center.X, -center.Y, center.Z);
            }
            UpdateCubeGeometry();
            OnPropertyChanged();
        }

        public override void Mirror(float A, float B, float C)
        {
            // Зеркалирование центра куба относительно плоскости
            Point3D mirroredCenter = MirrorPoint3D(center, A, B, C);
            center = mirroredCenter;
            UpdateCubeGeometry();
            OnPropertyChanged();
        }

        private Point3D MirrorPoint3D(Point3D point, float A, float B, float C)
        {
            // Формула отражения точки относительно плоскости Ax + By + Cz + D = 0
            // Для простоты считаем D = 0
            float denominator = A * A + B * B + C * C;
            if (Math.Abs(denominator) < 0.0001f) return point;

            float distance = (A * point.X + B * point.Y + C * point.Z) / denominator;

            return new Point3D(
                point.X - 2 * A * distance,
                point.Y - 2 * B * distance,
                point.Z - 2 * C * distance
            );
        }

        public override void Draw(Graphics graphics, LineCap endType = LineCap.Round)
        {
            // Рисуем ВСЕ ребра куба
            foreach (var edge in edges)
            {
                edge.Draw(graphics);
            }
        }

        public override void DrawSelection(Graphics graphics)
        {
            // Рисуем выделение для ВСЕХ ребер
            foreach (var edge in edges)
            {
                edge.DrawSelection(graphics);
            }

            // Дополнительно рисуем bounding box всего куба
            using (Pen cubePen = new Pen(Color.Green, 2))
            {
                cubePen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                var bbox = GetBoundingBox();
                graphics.DrawRectangle(cubePen, bbox.X, bbox.Y, bbox.Width, bbox.Height);
            }
        }

        public override void Projection(string coordinateAxis)
        {
            // Для куба проекция применяется ко всем вершинам
            foreach (var edge in edges)
            {
                edge.Projection(coordinateAxis);
            }
        }

        public override void LinkChange(FigureElement el)
        {
            // Не реализовано для куба
        }

        public override void Scale(PointF center, float sx, float sy)
        {
            throw new NotImplementedException();
        }

        //public Point3D Center => center;
        public Point3D Center 
        {
            get 
            {
                return center;
            }
            set 
            {
                Point3D delta = new Point3D(-center.X + value.X, -center.Y + value.Y, -center.Z + value.Z);
                Move3D(delta);
                center = value; //это уже в move3d учтено                
            }
        }
        public float Size => size;
        public Color CubeColor => color;
        плохо - масштабирование проверки не по половинам размера идут - пока не работает
            зеркалирование куба через 2д линию непроисходит
            3д линии через 2д линию непроисходит
        проецирование ввобще пока нтуда работает
            а так вроде норм
            
        public void Projection3D(string projectionType)
        {
            switch (projectionType.ToLower())
            {
                case "xoy":
                case "xy":
                    // Проецирование на плоскость XOY (Z = 0)
                    foreach (var edge in edges)
                    {
                        if (edge is LineElement3D line3D)
                        {
                            line3D.StartPoint3D = new Point3D(line3D.ZeroRatatedStartPoint.X, line3D.ZeroRatatedStartPoint.Y, 0);
                            line3D.EndPoint3D = new Point3D(line3D.ZeroRatatedEndPoint.X, line3D.ZeroRatatedEndPoint.Y, 0);
                            line3D.ZeroRatatedStartPoint = line3D.StartPoint3D;
                            line3D.ZeroRatatedEndPoint = line3D.EndPoint3D;
                        }
                    }
                    center = new Point3D(center.X, center.Y, 0);
                    break;

                case "xoz":
                case "xz":
                    // Проецирование на плоскость XOZ (Y = 0)
                    foreach (var edge in edges)
                    {
                        if (edge is LineElement3D line3D)
                        {
                            line3D.StartPoint3D = new Point3D(line3D.ZeroRatatedStartPoint.X, 0, line3D.ZeroRatatedStartPoint.Z);
                            line3D.EndPoint3D = new Point3D(line3D.ZeroRatatedEndPoint.X, 0, line3D.ZeroRatatedEndPoint.Z);
                            line3D.ZeroRatatedStartPoint = line3D.StartPoint3D;
                            line3D.ZeroRatatedEndPoint = line3D.EndPoint3D;
                        }
                    }
                    center = new Point3D(center.X, 0, center.Z);
                    break;

                case "yoz":
                case "yz":
                    // Проецирование на плоскость YOZ (X = 0)
                    foreach (var edge in edges)
                    {
                        if (edge is LineElement3D line3D)
                        {
                            line3D.StartPoint3D = new Point3D(0, line3D.ZeroRatatedStartPoint.Y, line3D.ZeroRatatedStartPoint.Z);
                            line3D.EndPoint3D = new Point3D(0, line3D.ZeroRatatedEndPoint.Y, line3D.ZeroRatatedEndPoint.Z);
                            line3D.ZeroRatatedStartPoint = line3D.StartPoint3D;
                            line3D.ZeroRatatedEndPoint = line3D.EndPoint3D;
                        }
                    }
                    center = new Point3D(0, center.Y, center.Z);
                    break;
            }

            UpdateCubeGeometry();
            OnPropertyChanged();
        }

        public void Mirror3DRelativeToLine(LineElement mirrorLine)
        {
            if (mirrorLine is LineElement3D mirrorLine3D)
            {
                // Получаем направляющий вектор и точку на зеркальной прямой
                Point3D linePoint = mirrorLine3D.ZeroRatatedStartPoint;
                Point3D lineDirection = new Point3D(
                    mirrorLine3D.ZeroRatatedEndPoint.X - mirrorLine3D.ZeroRatatedStartPoint.X,
                    mirrorLine3D.ZeroRatatedEndPoint.Y - mirrorLine3D.ZeroRatatedStartPoint.Y,
                    mirrorLine3D.ZeroRatatedEndPoint.Z - mirrorLine3D.ZeroRatatedStartPoint.Z
                );

                // Зеркалируем центр куба
                center = MirrorPointRelativeToLine(center, linePoint, lineDirection);

                // Зеркалируем все ребра
                foreach (var edge in edges)
                {
                    if (edge is LineElement3D line3D)
                    {
                        line3D.ZeroRatatedStartPoint = MirrorPointRelativeToLine(line3D.ZeroRatatedStartPoint, linePoint, lineDirection);
                        line3D.ZeroRatatedEndPoint = MirrorPointRelativeToLine(line3D.ZeroRatatedEndPoint, linePoint, lineDirection);
                        line3D.StartPoint3D = line3D.ZeroRatatedStartPoint;
                        line3D.EndPoint3D = line3D.ZeroRatatedEndPoint;
                    }
                }

                UpdateCubeGeometry();
                OnPropertyChanged();
            }
        }

        private Point3D MirrorPointRelativeToLine(Point3D point, Point3D linePoint, Point3D lineDirection)
        {
            // Вектор от точки на прямой до зеркалируемой точки
            Point3D v = new Point3D(
                point.X - linePoint.X,
                point.Y - linePoint.Y,
                point.Z - linePoint.Z
            );

            // Проекция вектора v на направляющий вектор прямой
            float t = (v.X * lineDirection.X + v.Y * lineDirection.Y + v.Z * lineDirection.Z) /
                      (lineDirection.X * lineDirection.X + lineDirection.Y * lineDirection.Y + lineDirection.Z * lineDirection.Z);

            // Точка проекции на прямой
            Point3D projection = new Point3D(
                linePoint.X + t * lineDirection.X,
                linePoint.Y + t * lineDirection.Y,
                linePoint.Z + t * lineDirection.Z
            );

            // Вектор от проекции до исходной точки
            Point3D w = new Point3D(
                point.X - projection.X,
                point.Y - projection.Y,
                point.Z - projection.Z
            );

            // Зеркальная точка
            return new Point3D(
                point.X - 2 * w.X,
                point.Y - 2 * w.Y,
                point.Z - 2 * w.Z
            );
        }


        /*
         * public void Mirror3D(float A, float B, float C, float D = 0)
{
    // Зеркалирование относительно плоскости Ax + By + Cz + D = 0
    center = MirrorPoint3D(center, A, B, C, D);
    
    // Зеркалируем все ребра
    foreach (var edge in edges)
    {
        if (edge is LineElement3D line3D)
        {
            line3D.StartPoint3D = MirrorPoint3D(line3D.ZeroRatatedStartPoint, A, B, C, D);
            line3D.EndPoint3D = MirrorPoint3D(line3D.ZeroRatatedEndPoint, A, B, C, D);
            line3D.ZeroRatatedStartPoint = line3D.StartPoint3D;
            line3D.ZeroRatatedEndPoint = line3D.EndPoint3D;
        }
    }
    
    UpdateCubeGeometry();
    OnPropertyChanged();
}

private Point3D MirrorPoint3D(Point3D point, float A, float B, float C, float D)
{
    // Формула отражения точки относительно плоскости
    float denominator = A * A + B * B + C * C;
    if (Math.Abs(denominator) < 0.0001f) return point;

    float distance = (A * point.X + B * point.Y + C * point.Z + D) / denominator;

    return new Point3D(
        point.X - 2 * A * distance,
        point.Y - 2 * B * distance,
        point.Z - 2 * C * distance
    );
}
         */

    }
}