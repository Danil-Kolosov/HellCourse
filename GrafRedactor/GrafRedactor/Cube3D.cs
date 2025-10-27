using System;
using System.Collections.Generic;
using System.Drawing;

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
        }

        private Point3D[] CalculateVertices()
        {
            float halfSize = size / 2;

            return new Point3D[]
            {
                // Нижняя грань (задние вершины имеют меньший Z для перспективы)
                new Point3D(center.X - halfSize, center.Y - halfSize, center.Z - halfSize), // 0: лево-перед-низ
                new Point3D(center.X + halfSize, center.Y - halfSize, center.Z - halfSize), // 1: право-перед-низ
                new Point3D(center.X + halfSize, center.Y + halfSize, center.Z - halfSize * 0.7f), // 2: право-зад-низ (меньше Z)
                new Point3D(center.X - halfSize, center.Y + halfSize, center.Z - halfSize * 0.7f), // 3: лево-зад-низ (меньше Z)
                
                // Верхняя грань
                new Point3D(center.X - halfSize, center.Y - halfSize, center.Z + halfSize), // 4: лево-перед-верх
                new Point3D(center.X + halfSize, center.Y - halfSize, center.Z + halfSize), // 5: право-перед-верх
                new Point3D(center.X + halfSize, center.Y + halfSize, center.Z + halfSize * 0.7f), // 6: право-зад-верх
                new Point3D(center.X - halfSize, center.Y + halfSize, center.Z + halfSize * 0.7f)  // 7: лево-зад-верх
            };
        }

        // ОБНОВЛЯЕМ ВСЕ МЕТОДЫ ДВИЖЕНИЯ И ПРЕОБРАЗОВАНИЙ
        public override void Move(PointF delta, float height, float width)
        {
            center = new Point3D(center.X + delta.X, center.Y + delta.Y, center.Z);
            UpdateCubeGeometry();
            OnPropertyChanged();
        }

        public void Move3D(Point3D delta)
        {
            center = new Point3D(center.X + delta.X, center.Y + delta.Y, center.Z + delta.Z);
            UpdateCubeGeometry();
            OnPropertyChanged();
        }

        public override void Rotate(float angle)
        {
            Rotate3D(0, 0, angle);
        }

        public void Rotate3D(float angleX, float angleY, float angleZ)
        {
            // Поворачиваем каждую вершину относительно центра куба
            Point3D[] vertices = CalculateVertices();
            Point3D[] rotatedVertices = new Point3D[8];

            for (int i = 0; i < 8; i++)
            {
                rotatedVertices[i] = RotatePoint3D(vertices[i], center, angleX, angleY, angleZ);
            }

            // Обновляем ребра с новыми вершинами
            UpdateEdgesWithVertices(rotatedVertices);
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

        public override void Scale(PointF center, float sx, float sy)
        {
            size *= (sx + sy) / 2;
            UpdateCubeGeometry();
            OnPropertyChanged();
        }

        public override void ScaleAverage(float scaleFactor)
        {
            size *= scaleFactor;
            UpdateCubeGeometry();
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

        public override void Draw(Graphics graphics)
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

        public Point3D Center => center;
        public float Size => size;
        public Color CubeColor => color;
    }
}