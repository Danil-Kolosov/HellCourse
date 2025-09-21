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

    public override void LinkLenghtChange(float deltaL/*, float deltaA*/)
    {
        Position = new PointF(Position.X + deltaL, Position.Y);
        //!!!!!!!!!!!!!!!!
    }

    public override void LinkChange(FigureElement el)
    {
        if (el is LineElement line)
            Position = line.EndPoint;
    }

    //public override bool ContainsPoint(PointF point)
    //{
    //    float distance = (float)Math.Sqrt(
    //        Math.Pow(point.X - Position.X, 2) +
    //        Math.Pow(point.Y - Position.Y, 2));
    //    return distance <= Radius;
    //}
}