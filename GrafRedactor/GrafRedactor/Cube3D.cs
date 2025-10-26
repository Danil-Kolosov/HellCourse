using System;
using System.Collections.Generic;
using System.Drawing;

namespace GrafRedactor
{
    public class Cube3D : FigureElement
    {
        private List<LineElement3D> edges = new List<LineElement3D>();
        private Point3D position;
        private float size;

        public bool Is3D => true;
        public List<LineElement3D> Edges => edges;

        public Point3D Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;
                    UpdateCubeGeometry();
                    OnPropertyChanged();
                }
            }
        }

        public float Size
        {
            get => size;
            set
            {
                if (size != value)
                {
                    size = value;
                    UpdateCubeGeometry();
                    OnPropertyChanged();
                }
            }
        }

        public Color Color { get; set; }

        public Cube3D(Point3D position, float size, Color color)
        {
            this.position = position;
            this.size = size;
            this.Color = color;

            InitializeEdges();
            UpdateCubeGeometry();
        }

        private void InitializeEdges()
        {
            // Создаем 12 ребер куба
            for (int i = 0; i < 12; i++)
            {
                var edge = new LineElement3D(new Point3D(0, 0, 0), new Point3D(0, 0, 0), Color, 2f);
                edge.OnChanged += OnEdgeChanged; // Подписываемся на изменения ребер
                edges.Add(edge);
            }

            // Связываем ребра между собой
            LinkEdges();
        }

        private void LinkEdges()
        {
            // Ребра группируются по 4 на каждой грани
            // Связываем смежные ребра так, чтобы при перемещении одного
            // связанные ребра автоматически обновлялись

            // Нижняя грань (Z = position.Z)
            LinkEdge(0, 1);  // Переднее нижнее
            LinkEdge(1, 2);  // Правое нижнее  
            LinkEdge(2, 3);  // Заднее нижнее
            LinkEdge(3, 0);  // Левое нижнее

            // Верхняя грань (Z = position.Z + size)
            LinkEdge(4, 5);  // Переднее верхнее
            LinkEdge(5, 6);  // Правое верхнее
            LinkEdge(6, 7);  // Заднее верхнее
            LinkEdge(7, 4);  // Левое верхнее

            // Вертикальные ребра
            LinkEdge(0, 4);  // Переднее левое
            LinkEdge(1, 5);  // Переднее правое
            LinkEdge(2, 6);  // Заднее правое
            LinkEdge(3, 7);  // Заднее левое
        }

        private void LinkEdge(int fromIndex, int toIndex)
        {
            // Связываем ребра так, чтобы конечная точка одного была начальной точкой другого
            edges[fromIndex].LinkChange(edges[toIndex]);
        }

        private void UpdateCubeGeometry()
        {
            // Вычисляем 8 вершин куба
            Point3D[] vertices = CalculateVertices();

            // Обновляем координаты ребер
            // Нижняя грань
            edges[0].StartPointR = vertices[0]; edges[0].EndPoint = vertices[1]; // Переднее
            edges[1].StartPoint = vertices[1]; edges[1].EndPoint = vertices[2]; // Правое
            edges[2].StartPoint = vertices[2]; edges[2].EndPoint = vertices[3]; // Заднее
            edges[3].StartPoint = vertices[3]; edges[3].EndPoint = vertices[0]; // Левое

            // Верхняя грань
            edges[4].StartPoint = vertices[4]; edges[4].EndPoint = vertices[5]; // Переднее
            edges[5].StartPoint = vertices[5]; edges[5].EndPoint = vertices[6]; // Правое
            edges[6].StartPoint = vertices[6]; edges[6].EndPoint = vertices[7]; // Заднее
            edges[7].StartPoint = vertices[7]; edges[7].EndPoint = vertices[4]; // Левое

            // Вертикальные ребра
            edges[8].StartPoint = vertices[0]; edges[8].EndPoint = vertices[4]; // Переднее левое
            edges[9].StartPoint = vertices[1]; edges[9].EndPoint = vertices[5]; // Переднее правое
            edges[10].StartPoint = vertices[2]; edges[10].EndPoint = vertices[6]; // Заднее правое
            edges[11].StartPoint = vertices[3]; edges[11].EndPoint = vertices[7]; // Заднее левое
        }

        private Point3D[] CalculateVertices()
        {
            float halfSize = size / 2;

            // 8 вершин куба
            return new Point3D[]
            {
                // Нижняя грань
                new Point3D(position.X - halfSize, position.Y - halfSize, position.Z - halfSize), // 0: лево-перед-низ
                new Point3D(position.X + halfSize, position.Y - halfSize, position.Z - halfSize), // 1: право-перед-низ
                new Point3D(position.X + halfSize, position.Y + halfSize, position.Z - halfSize), // 2: право-зад-низ
                new Point3D(position.X - halfSize, position.Y + halfSize, position.Z - halfSize), // 3: лево-зад-низ
                
                // Верхняя грань
                new Point3D(position.X - halfSize, position.Y - halfSize, position.Z + halfSize), // 4: лево-перед-верх
                new Point3D(position.X + halfSize, position.Y - halfSize, position.Z + halfSize), // 5: право-перед-верх
                new Point3D(position.X + halfSize, position.Y + halfSize, position.Z + halfSize), // 6: право-зад-верх
                new Point3D(position.X - halfSize, position.Y + halfSize, position.Z + halfSize)  // 7: лево-зад-верх
            };
        }

        private void OnEdgeChanged(FigureElement changedEdge)
        {
            // Когда одно ребро изменяется, обновляем всю геометрию куба
            UpdateCubeGeometry();
            OnPropertyChanged(); // Уведомляем об изменении куба
        }

        // Реализация абстрактных методов FigureElement
        public override bool ContainsPoint(PointF point)
        {
            // Проверяем попадание в любое из ребер
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

            // Общий bounding box всех ребер
            RectangleF bbox = edges[0].GetBoundingBox();
            for (int i = 1; i < edges.Count; i++)
            {
                bbox = RectangleF.Union(bbox, edges[i].GetBoundingBox());
            }
            return bbox;
        }

        public override void Move(PointF delta)
        {
            Move3D(new Point3D(delta.X, delta.Y, 0));
        }

        public override void Move3D(Point3D delta)
        {
            Position = new Point3D(
                position.X + delta.X,
                position.Y + delta.Y,
                position.Z + delta.Z
            );
        }

        public override void Rotate(float angle)
        {
            Rotate3D(0, 0, angle);
        }

        public override void Rotate3D(float angleX, float angleY, float angleZ)
        {
            Point3D center = Position;

            // Поворачиваем каждую вершину относительно центра куба
            foreach (var edge in edges)
            {
                edge.StartPoint = RotatePoint3D(edge.StartPoint, center, angleX, angleY, angleZ);
                edge.EndPoint = RotatePoint3D(edge.EndPoint, center, angleX, angleY, angleZ);
            }
        }

        private Point3D RotatePoint3D(Point3D point, Point3D center, float angleX, float angleY, float angleZ)
        {
            // Перенос в систему координат с центром в center
            Point3D translated = new Point3D(
                point.X - center.X,
                point.Y - center.Y,
                point.Z - center.Z
            );

            // Вращение вокруг Z (пока только Z для простоты)
            if (Math.Abs(angleZ) > 0.001f)
            {
                float angleRad = angleZ * (float)Math.PI / 180f;
                float cos = (float)Math.Cos(angleRad);
                float sin = (float)Math.Sin(angleRad);

                float newX = translated.X * cos - translated.Y * sin;
                float newY = translated.X * sin + translated.Y * cos;

                translated = new Point3D(newX, newY, translated.Z);
            }

            // Возврат в исходную систему координат
            return new Point3D(
                translated.X + center.X,
                translated.Y + center.Y,
                translated.Z + center.Z
            );
        }

        public override void Scale(float scaleFactor)
        {
            Scale3D(scaleFactor, scaleFactor, scaleFactor);
        }

        public override void Scale3D(float scaleX, float scaleY, float scaleZ)
        {
            Size = size * ((scaleX + scaleY + scaleZ) / 3); // Усредняем масштаб
        }

        public override void Mirror(bool horizontal)
        {
            Mirror3D(horizontal, !horizontal, false);
        }

        public override void Mirror3D(bool xAxis, bool yAxis, bool zAxis)
        {
            Point3D center = Position;

            foreach (var edge in edges)
            {
                edge.StartPoint = MirrorPoint3D(edge.StartPoint, center, xAxis, yAxis, zAxis);
                edge.EndPoint = MirrorPoint3D(edge.EndPoint, center, xAxis, yAxis, zAxis);
            }
        }

        private Point3D MirrorPoint3D(Point3D point, Point3D center, bool xAxis, bool yAxis, bool zAxis)
        {
            return new Point3D(
                xAxis ? 2 * center.X - point.X : point.X,
                yAxis ? 2 * center.Y - point.Y : point.Y,
                zAxis ? 2 * center.Z - point.Z : point.Z
            );
        }

        public override void Draw(Graphics graphics)
        {
            foreach (var edge in edges)
            {
                edge.Draw(graphics);
            }
        }

        public override void DrawSelection(Graphics graphics)
        {
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

        public override void LinkChange(FigureElement el)
        {
            // Для куба связывание не реализовано
        }
    }
}