using Robot;
using System.Drawing;
using System;

public class CircleElement : FigureElement
{
    public float Radius { get; set; }

    public CircleElement(PointF position, float radius, Color color)
    {
        Position = position;
        Radius = radius;
        Color = color;
    }

    public override void Draw(Graphics graphics)
    {
        using (Brush brush = new SolidBrush(Color))
        {
            graphics.FillEllipse(brush,
                Position.X - Radius,
                Position.Y - Radius,
                Radius * 2,
                Radius * 2);
        }
    }

    public override void LinkChange(FigureElement el)
    {
        if (el is LineElement line)
            Position = line.EndPoint;
    }
}