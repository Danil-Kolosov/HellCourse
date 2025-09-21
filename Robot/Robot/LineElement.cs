using Robot;
using System.Drawing.Drawing2D;
using System.Drawing;
using System;

public class LineElement : FigureElement
{
    private float _length;

    public event Action<float> OnChangedLenght; // Событие изменения
    //public event Action<FigureElement> OnChanged;
    public float Length {
        get => _length;
        set
        {
            if (_length != value)
            {
                OnChangedLenght?.Invoke(Length - value); // Вызываем событие
                _length = value;
                OnPropertyChanged();
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
    public override void LinkLenghtChange(float deltaL/*, float deltaA*/)
    {
        Position = new PointF(Position.X,Position.Y + deltaL);
        //Rotation += deltaA;
    }

    public override void LinkChange(FigureElement el)
    {
        if (el is LineElement line)
            Position = line.EndPoint;
    }
    //public override bool ContainsPoint(PointF point)
    //{
    //    // Упрощённая проверка - считаем что линия не кликабельна
    //    return false;
    //}
}