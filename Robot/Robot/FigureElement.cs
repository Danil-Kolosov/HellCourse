using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Robot
{
    public abstract class FigureElement
    {
        private float _rotation;
        private PointF _position;
        public PointF Position 
        {
            get => _position;
            set 
            {
                if (_position != value)
                {
                    _position = value;
                    //OnChangedRotate?.Invoke((LineElement)this); // Вызываем событие
                    OnPropertyChanged(); // Вызываем событие
                }
            } 
        } // Относительная позиция элемента
        public Color Color { get; set; }
        public event Action<LineElement> OnChangedRotate; // Событие изменения
        public event Action<FigureElement> OnChanged; // Событие изменения

        // Виртуальный метод, который вызывается при любом изменении
        protected virtual void OnPropertyChanged(FigureElement el = null)
        {
            if(el != null)
                OnChanged?.Invoke(el);
            else
               OnChanged?.Invoke(this);
        }

        public float Rotation {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    OnChangedRotate?.Invoke((LineElement)this); // Вызываем событие
                    OnPropertyChanged();
                    //OnChanged?.Invoke(this); // Вызываем событие
                }
            }
        } // Угол поворота элемента

        public abstract void Draw(Graphics graphics);
        //public abstract bool ContainsPoint(PointF point); находится ли точка внутри фигуры - для кликабельности. тут нафиг надо

        // Преобразование точки с учётом поворота
        protected PointF RotatePoint(PointF point, PointF center, float angle)
        {
            float angleRad = angle * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(angleRad);
            float sin = (float)Math.Sin(angleRad);

            float x = point.X - center.X;
            float y = point.Y - center.Y;

            return new PointF(
                x * cos - y * sin + center.X,
                x * sin + y * cos + center.Y
            );
        }
        public abstract void LinkLenghtChange(float deltaL/*, float deltaA*/);
        public virtual void LinkRotateChange(LineElement el) 
        {
            Position = el.EndPoint;
        }
        public virtual void LinkChange(FigureElement el) 
        {
            if (el is LineElement line)
                Position = line.EndPoint;
        }
    }
}