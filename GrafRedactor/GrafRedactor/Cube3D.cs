using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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
        public float _zc = float.MaxValue;
        public PointElement3D TcX = new PointElement3D(0,0,0);
        public PointElement3D TcY = new PointElement3D(0, 0, 0);
        public PointElement3D TcZ = new PointElement3D(0, 0, 0);

        public Cube3D(Point3D center, float size, Color color, string currentAxisName, float zs)
        {
            this.center = center;
            this.size = size;
            this.color = color;

            InitializeEdges();
            UpdateCubeGeometry();
            is3D = true;
            Rotate3DWithScene(0,0,0, new Point3D(0, 0, 0), zs, currentAxisName);
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

            // Обновляем координаты ребер БЕЗ создания подписок на события
            // Нижняя грань
            edges[0].StartPoint3D = vertices[0]; edges[0].EndPoint3D = vertices[1];
            edges[1].StartPoint3D = vertices[1]; edges[1].EndPoint3D = vertices[2];
            edges[2].StartPoint3D = vertices[2]; edges[2].EndPoint3D = vertices[3];
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

            // Обновляем ZeroRatated точки для всех ребер
            foreach (LineElement3D edge in edges)
            {
                edge.ZeroRatatedStartPoint = edge.StartPoint3D;
                edge.ZeroRatatedEndPoint = edge.EndPoint3D;
            }
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
        public override void Move(PointF delta, float height, float width, float deltaZ, string axeName)
        {
            throw new NotImplementedException();
        }

        private void Move3DPriv(Point3D delta)
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

        private void CalculateTc(float ZC, float angleY, float anfleX) 
        {
            float angleRadY = angleY * (float)Math.PI / 180f;
            float angleRadX = anfleX * (float)Math.PI / 180f;
            TcX.Point3D.X = center.X + (float)(ZC / (Math.Tan(angleRadY) * Math.Cos(anfleX)));
            TcX.Point3D.Y = center.Y + (float)(ZC * Math.Tan(anfleX));
            TcX.Point3D.Z = center.Z + 0;

            TcY.Point3D.X = 0;
            TcY.Point3D.Y = (float)(-ZC / Math.Tan(anfleX));
            TcY.Point3D.Z = 0;

            TcZ.Point3D.X = (float)(-ZC * (Math.Tan(angleRadY) / Math.Cos(anfleX)));
            TcZ.Point3D.Y = (float)(ZC * Math.Tan(anfleX));
            TcZ.Point3D.Z = 0;

        }

        private void MoveTc(Point3D delta)
        {
            
            TcX.Move3D(delta);
            TcY.Move3D(delta);
            TcZ.Move3D(delta);
        }

        public void Move3D(PointF delta, float height, float width, float deltaZ, string axeName)
        {
            // Сначала обновляем центр куба в зависимости от плоскости
            Point3D newCenter = center;

            switch (axeName.ToLower())
            {
                case "xoy":
                    newCenter = new Point3D(
                        center.X + delta.X,
                        center.Y + delta.Y,
                        center.Z + deltaZ
                    );
                    break;
                case "yoz":
                    newCenter = new Point3D(
                        center.X + deltaZ,  // X меняется через deltaZ
                        center.Y + delta.X, // Y меняется через delta.X  
                        center.Z + delta.Y  // Z меняется через delta.Y
                    );
                    break;
                case "xoz":
                    newCenter = new Point3D(
                        center.X + delta.X, // X меняется через delta.X
                        center.Y + deltaZ,  // Y меняется через deltaZ
                        center.Z + delta.Y  // Z меняется через delta.Y
                    );
                    break;
            }

            // Перемещаем все ребра куба
            foreach (LineElement3D edge in edges)
            {
                edge.Move(delta, height, width, deltaZ, axeName);
            }

            // Обновляем центр куба
            center = newCenter;

            if(_zc != float.MaxValue) 
            {
                MoveTc(new Point3D(delta, deltaZ));
                this.Rotate3DWithScene(0, 0, 0, new Point3D(0, 0, 0), _zc, axeName);
            }
            
            // Обновляем геометрию куба (ВАЖНО!) НАХЕР ЭТО НАДО ТУТ
            //UpdateCubeGeometry();

            // Уведомляем об изменении
            OnPropertyChanged();
        }


        public override void Rotate(float angle, PointF cent)
        {
            if (cent.IsEmpty)
                Rotate3D(0, 0, angle, center, Rectangle.Empty, float.MaxValue);
            else
                Rotate3D(0, 0, angle, new Point3D(cent), Rectangle.Empty, float.MaxValue);
        }

        private bool CanPerformRotation3D(float angleX, float angleY, float angleZ, Point3D center, Rectangle drawingArea)
        {
            return true;
            throw new NotImplementedException();
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

        private Point3D[] GetUniqueVertices()
        {
            HashSet<Point3D> vertices = new HashSet<Point3D>();
            foreach (LineElement3D edge in edges)
            {
                vertices.Add(edge.ZeroRatatedStartPoint);
                vertices.Add(edge.ZeroRatatedEndPoint);
            }
            return vertices.ToArray();
        }

        private void UpdateEdgesFromVertices(Point3D[] vertices)
        {
            if (vertices.Length != 8)
            {
                throw new ArgumentException("Для куба требуется 8 вершин");
            }

            // Обновляем координаты ребер на основе новых вершин
            // Нижняя грань
            UpdateEdge(0, vertices[0], vertices[1]);
            UpdateEdge(1, vertices[1], vertices[2]);
            UpdateEdge(2, vertices[2], vertices[3]);
            UpdateEdge(3, vertices[3], vertices[0]);

            // Верхняя грань
            UpdateEdge(4, vertices[4], vertices[5]);
            UpdateEdge(5, vertices[5], vertices[6]);
            UpdateEdge(6, vertices[6], vertices[7]);
            UpdateEdge(7, vertices[7], vertices[4]);

            // Вертикальные ребра
            UpdateEdge(8, vertices[0], vertices[4]);
            UpdateEdge(9, vertices[1], vertices[5]);
            UpdateEdge(10, vertices[2], vertices[6]);
            UpdateEdge(11, vertices[3], vertices[7]);
        }

        private void UpdateEdge(int edgeIndex, Point3D startPoint, Point3D endPoint)
        {
            if (edgeIndex < 0 || edgeIndex >= edges.Count)
                return;

            LineElement3D edge = edges[edgeIndex];

            // Обновляем базовые координаты (без поворотов)
            edge.StartPoint3D = startPoint;
            edge.StartPoint3D = endPoint;
        }

        private void UpdateCenterFromVertices(Point3D[] vertices)
        {
            float sumX = 0, sumY = 0, sumZ = 0;
            foreach (Point3D vertex in vertices)
            {
                sumX += vertex.X;
                sumY += vertex.Y;
                sumZ += vertex.Z;
            }

            this.center = new Point3D(
                sumX / vertices.Length,
                sumY / vertices.Length,
                sumZ / vertices.Length
            );
        }

        public void Rotate3D(float angleX, float angleY, float angleZ, Point3D rotationCenter, 
            Rectangle drawingArea /*= new Rectangle()*/, float zc)
        {
            //if (CanPerformRotation3D(angleX, angleY, angleZ, rotationCenter, drawingArea))
            //{
            //    // Получаем текущие вершины
            //    Point3D[] vertices = CalculateVertices();

            //    // Вращаем каждую вершину
            //    Point3D[] rotatedVertices = new Point3D[vertices.Length];
            //    for (int i = 0; i < vertices.Length; i++)
            //    {
            //        rotatedVertices[i] = RotatePoint3D(vertices[i], rotationCenter, angleX, angleY, angleZ);
            //    }

            //    // Обновляем рёбра с новыми вершинами
            //    UpdateEdgesFromVertices(rotatedVertices);

            //    // Обновляем центр куба из новых вершин
            //    UpdateCenterFromVertices(rotatedVertices);

            //    // Обновляем геометрию (на всякий случай)
            //    UpdateCubeGeometry();
            //}
            if (_zc != float.MaxValue)
            {
                zc = _zc;
            }
            if (CanPerformRotation3D(angleX, angleY, angleZ, center, drawingArea))
            {
                foreach (LineElement3D line in edges)
                {
                    line.Rotate3D(center/*new Point3D(0,0,0)ЭТО НЕ ЦЕНТР*/, angleX, angleY, angleZ, zc);
                }

                this.center = RotatePoint3D(Center, center, angleX, angleY, angleZ);
                if (_zc != float.MaxValue)
                {
                    CalculateTc(zc, angleY, angleX);
                }
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

        public void Rotate3DWithScene(float angleX, float angleY, float angleZ, Point3D center, float zc, string currentAxiName) 
        {
            if (_zc != float.MaxValue)
            {
                zc = _zc;
            }            
            foreach (LineElement3D line in edges)
            {
                line.Rotate3DWithScene(center/*new Point3D(0,0,0)ЭТО НЕ ЦЕНТР*/, angleX, angleY, angleZ, zc, currentAxiName);
            }
            //CalculateTc(zc, angleY, angleX);
            //OnPropertyChanged();
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

            // Рисуем точки схождения если активны
            if (_zc != float.MaxValue)
            {
                TcX.Draw(graphics); TcY.Draw(graphics); TcZ.Draw(graphics);
                // Рисуем линии от центра куба к точкам схода для отладки
                //using (Pen debugPen = new Pen(Color.Purple, 1f))
                //{
                //    debugPen.DashStyle = DashStyle.Dash;

                //    PointF center2D = new PointF(center.X, center.Y);

                //    graphics.DrawLine(debugPen, center2D, new PointF(TcX.Point3D.X, TcX.Point3D.Y));
                //    graphics.DrawLine(debugPen, center2D, new PointF(TcY.Point3D.X, TcY.Point3D.Y));
                //    graphics.DrawLine(debugPen, center2D, new PointF(TcZ.Point3D.X, TcZ.Point3D.Y));
                //}
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
            Projection3D(coordinateAxis);
            
            // Для куба проекция применяется ко всем вершинам
            //старрое
            //foreach (var edge in edges)
            //{
            //    edge.Projection(coordinateAxis);
            //}
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
                throw new Exception();
                center = value; //это уже в move3d учтено                
            }
        }

        public void SetMovingCenter(PointF delta, float height, float width, float deltaZ, string axeName) 
        {
            Move3D(delta, height, width, deltaZ, axeName);
        }

        public float Size => size;
        public Color CubeColor => color;
        
            
        public void Projection3D(string projectionType)
        {
            projectionType = projectionType.ToLower();
            if (projectionType.Length == 1) 
            {
                foreach (FigureElement edge in edges)
                {
                    edge.Projection(projectionType);
                }
                    return;
            }
            switch (projectionType)
            {
                case "xoy":
                case "xy":
                case "":
                    // Проецирование на плоскость XOY (Z = 0)
                    foreach (LineElement3D edge in edges)
                    {
                        edge.Projection3D(projectionType);
                        //if (edge is LineElement3D line3D)
                        //{
                            
                            /*line3D.StartPoint3D = new Point3D(line3D.ZeroRatatedStartPoint.X, line3D.ZeroRatatedStartPoint.Y, 0);
                            line3D.EndPoint3D = new Point3D(line3D.ZeroRatatedEndPoint.X, line3D.ZeroRatatedEndPoint.Y, 0);
                            line3D.ZeroRatatedStartPoint = line3D.StartPoint3D;
                            line3D.ZeroRatatedEndPoint = line3D.EndPoint3D;*/
                        //}
                    }
                    center = new Point3D(center.X, center.Y, 0);
                    break;

                case "xoz":
                case "xz":
                    // Проецирование на плоскость XOZ (Y = 0)
                    foreach (LineElement3D edge in edges)
                    {
                        edge.Projection3D(projectionType);
                        /*if (edge is LineElement3D line3D)
                        {
                            line3D.StartPoint3D = new Point3D(line3D.ZeroRatatedStartPoint.X, 0, line3D.ZeroRatatedStartPoint.Z);
                            line3D.EndPoint3D = new Point3D(line3D.ZeroRatatedEndPoint.X, 0, line3D.ZeroRatatedEndPoint.Z);
                            line3D.ZeroRatatedStartPoint = line3D.StartPoint3D;
                            line3D.ZeroRatatedEndPoint = line3D.EndPoint3D;
                        }*/
                    }
                    center = new Point3D(center.X, 0, center.Z);
                    break;

                case "yoz":
                case "yz":
                    // Проецирование на плоскость YOZ (X = 0)
                    foreach (LineElement3D edge in edges)
                    {
                        edge.Projection3D(projectionType);
                        //if (edge is LineElement3D line3D)
                        //{
                        //    line3D.StartPoint3D = new Point3D(0, line3D.ZeroRatatedStartPoint.Y, line3D.ZeroRatatedStartPoint.Z);
                        //    line3D.EndPoint3D = new Point3D(0, line3D.ZeroRatatedEndPoint.Y, line3D.ZeroRatatedEndPoint.Z);
                        //    line3D.ZeroRatatedStartPoint = line3D.StartPoint3D;
                        //    line3D.ZeroRatatedEndPoint = line3D.EndPoint3D;
                        //}
                    }
                    center = new Point3D(0, center.Y, center.Z);
                    break;
            }

            //UpdateCubeGeometry();
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

            else 
            {
                if (mirrorLine is LineElement mirrorLine2d)
                {
                    // Получаем направляющий вектор и точку на зеркальной прямой
                    Point3D linePoint = new Point3D(mirrorLine2d.StartPoint, 0);
                    Point3D lineDirection = new Point3D(
                        mirrorLine2d.EndPoint.X - mirrorLine2d.StartPoint.X,
                        mirrorLine2d.EndPoint.Y - mirrorLine2d.StartPoint.Y,
                        0
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

        public override JObject Serialize()
        {
            var json = base.Serialize();

            json["Center"] = new JObject
            {
                ["X"] = center.X,
                ["Y"] = center.Y,
                ["Z"] = center.Z
            };

            json["Size"] = size;
            json["CubeColor"] = ColorTranslator.ToHtml(color);
            json["Zc"] = _zc;
            json["CurrentAxisName"] = ""; // Добавьте это поле в класс
            json["Zs"] = 0; // Добавьте это поле в класс

            // Сериализуем все ребра
            var edgesArray = new JArray();
            foreach (var edge in edges)
            {
                edgesArray.Add(((LineElement3D)edge).Serialize());
            }
            json["Edges"] = edgesArray;

            // Сериализуем точки схода
            json["TcX"] = new JObject
            {
                ["X"] = TcX.Point3D.X,
                ["Y"] = TcX.Point3D.Y,
                ["Z"] = TcX.Point3D.Z
            };

            json["TcY"] = new JObject
            {
                ["X"] = TcY.Point3D.X,
                ["Y"] = TcY.Point3D.Y,
                ["Z"] = TcY.Point3D.Z
            };

            json["TcZ"] = new JObject
            {
                ["X"] = TcZ.Point3D.X,
                ["Y"] = TcZ.Point3D.Y,
                ["Z"] = TcZ.Point3D.Z
            };

            return json;
        }

        public override void Deserialize(JObject data)
        {
            base.Deserialize(data);

            center = new Point3D(
                (float)data["Center"]["X"],
                (float)data["Center"]["Y"],
                (float)data["Center"]["Z"]
            );

            size = (float)data["Size"];
            color = ColorTranslator.FromHtml((string)data["CubeColor"]);
            _zc = (float)data["Zc"];

            string currentAxisName = (string)data["CurrentAxisName"];
            float zs = (float)data["Zs"];

            // Восстанавливаем ребра
            edges.Clear();
            foreach (JObject edgeData in data["Edges"])
            {
                var edge = FigureElement.CreateFromData(edgeData) as LineElement3D;
                if (edge != null)
                {
                    edges.Add(edge);
                }
            }

            // Восстанавливаем точки схода
            TcX = new PointElement3D(
                new Point3D(
                    (float)data["TcX"]["X"],
                    (float)data["TcX"]["Y"],
                    (float)data["TcX"]["Z"]
                )
            );

            TcY = new PointElement3D(
                new Point3D(
                    (float)data["TcY"]["X"],
                    (float)data["TcY"]["Y"],
                    (float)data["TcY"]["Z"]
                )
            );

            TcZ = new PointElement3D(
                new Point3D(
                    (float)data["TcZ"]["X"],
                    (float)data["TcZ"]["Y"],
                    (float)data["TcZ"]["Z"]
                )
            );

            // Инициализируем куб
            InitializeEdges();
            UpdateCubeGeometry();
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