using Robot;
using System.Drawing;

public class RectangleElement : FigureElement
{
    public SizeF Size { get; set; }

    public RectangleElement(PointF position, SizeF size, Color color)
    {
        Position = position;
        Size = size;
        Color = color;
    }

    public override void Draw(Graphics graphics)
    {
        using (Brush brush = new SolidBrush(Color))
        {
            RectangleF rect = new RectangleF(
                Position.X - Size.Width / 2,
                Position.Y - Size.Height / 2,
                Size.Width,
                Size.Height);

            //Поворот прямоугольника
            if (Rotation != 0)
            {
                //graphics.TranslateTransform(Position.X, Position.Y);
                //graphics.RotateTransform(Rotation);
                //graphics.FillRectangle(brush,
                //    -Size.Width / 2,
                //    -Size.Height / 2,
                //    Size.Width,
                //    Size.Height);
                //graphics.ResetTransform();
            }
            else
            {
                graphics.FillRectangle(brush, rect);
            }
        }
    }

    public override void LinkChange(FigureElement el)
    {
        if (el is LineElement line)
            Position = line.EndPoint;
    }
}