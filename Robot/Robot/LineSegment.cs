using System;
using System.Drawing;

namespace Robot
{
    /// <summary>
    /// Класс для представления отрезка линии с возможностью вращения и изменения длины
    /// </summary>
    public class LineSegment
    {
        public PointF StartPoint { get; set; }  // Начальная точка линии
        public float Length { get; set; }       // Длина линии
        public float Angle { get; set; }        // Угол в градусах (0 - горизонтально вправо)



        /// <summary>
        /// Конечная точка линии (вычисляется автоматически)
        /// </summary>
        public PointF EndPoint
        {
            get
            {
                // Преобразуем угол из градусов в радианы
                float angleRad = Angle * (float)Math.PI / 180f;

                // Вычисляем конечную точку используя тригонометрию
                float endX = StartPoint.X + Length * (float)Math.Cos(angleRad);
                float endY = StartPoint.Y + Length * (float)Math.Sin(angleRad);

                return new PointF(endX, endY);
            }
        }

        /// <summary>
        /// Конструктор линии
        /// </summary>
        public LineSegment(PointF startPoint, float initialLength, float initialAngle)
        {
            StartPoint = startPoint;
            Length = initialLength;
            Angle = initialAngle;
        }

        /// <summary>
        /// Отрисовка линии
        /// </summary>
        public void Draw(Graphics graphics, Pen pen)
        {
            PointF endPoint = EndPoint;
            graphics.DrawLine(pen, StartPoint, endPoint);
        }
    }
}