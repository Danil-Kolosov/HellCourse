using Robot;
using System.Drawing.Drawing2D;
using System.Drawing;
using System;

public class LineElement : FigureElement
{
    private float _length;

    public float Length {
        get => _length;
        set
        {
            if (_length != value)
            {
                _length = value;
                OnPropertyChanged(); // Вызываем событие
            }
        }
    }
    public float Thickness { get; set; }

    public LineElement(PointF position, float length, float angle, Color color, float thickness = 3f)
    {
        Position = position;
        Length = length;
        Rotation = angle;
        Color = color;
        Thickness = thickness;
    }

    public PointF EndPoint
    {
        get
        {
            float angleRad = Rotation * (float)Math.PI / 180f;
            return new PointF(
                Position.X + Length * (float)Math.Cos(angleRad),
                Position.Y + Length * (float)Math.Sin(angleRad)
            );
        }
    }

    public override void Draw(Graphics graphics)
    {
        using (Pen pen = new Pen(Color, Thickness))
        {
            pen.EndCap = LineCap.Round;
            graphics.DrawLine(pen, Position, EndPoint);
        }
    }

    public override void LinkChange(FigureElement el)
    {
        if (el is LineElement line)
            Position = line.EndPoint;
    }
}