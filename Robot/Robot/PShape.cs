using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot
{
    public class PShape : FigureElement
    {
        public LineElement TopLine { get; private set; }
        public LineElement LeftLine { get; private set; }
        public LineElement RightLine { get; private set; }

        public float Width { get; set; } = 60f;    // Ширина буквы П
        public float Height { get; set; } = 40f;   // Высота буквы П
        public float Thickness { get; set; } = 5f; // Толщина линий
        public Color Color { get; set; } = Color.Blue;

        public PShape()
        {
            // Создаем три линии буквы "П"
            TopLine = new LineElement(PointF.Empty, 0, 0, Color, Thickness);
            LeftLine = new LineElement(PointF.Empty, 0, 0, Color, Thickness);
            RightLine = new LineElement(PointF.Empty, 0, 0, Color, Thickness);
        }

        public void UpdatePosition(PointF endPoint, float parentAngle)
        {
            float angleRad = parentAngle * (float)Math.PI / 180f;

            // 1. Верхняя горизонтальная линия
            PointF topLineStart = new PointF(
                endPoint.X - Width / 2 * (float)Math.Cos(angleRad),
                endPoint.Y - Width / 2 * (float)Math.Sin(angleRad)
            );

            PointF topLineEnd = new PointF(
                endPoint.X + Width / 2 * (float)Math.Cos(angleRad),
                endPoint.Y + Width / 2 * (float)Math.Sin(angleRad)
            );

            // Обновляем существующие LineElement вместо создания новых
            TopLine.Position = topLineStart;
            TopLine.Length = Width;
            TopLine.Rotation = parentAngle;

            // 2. Левая вертикальная линия (перпендикулярно - +90 градусов)
            LeftLine.Position = topLineStart;
            LeftLine.Length = Height;
            LeftLine.Rotation = parentAngle + 90f; // Перпендикулярно родительской линии

            // 3. Правая вертикальная линия
            RightLine.Position = topLineEnd;
            RightLine.Length = Height;
            RightLine.Rotation = parentAngle + 90f;
        }

        public void AddToModel(RobotModel model)
        {
            // Добавляем все линии в модель для общего управления
            model.Elements.Add(TopLine);
            model.Elements.Add(LeftLine);
            model.Elements.Add(RightLine);
        }

        public override void Draw(Graphics graphics)
        {
            TopLine.Draw(graphics);
            LeftLine.Draw(graphics);
            RightLine.Draw(graphics);
        }

        public override void LinkChange(FigureElement el)
        {
            UpdatePosition(((LineElement)el).EndPoint, ((LineElement)el).Rotation - 90);
            //OnPropertyChanged(TopLine);
            //OnPropertyChanged(LeftLine);
            //OnPropertyChanged(RightLine);
            //с улом вопрос однако
        }

        

        public override void LinkLenghtChange(float deltaL)
        {
            //TopLine.LinkLenghtChange(deltaL);
        }
    }
}
