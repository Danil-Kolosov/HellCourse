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
                    OnPropertyChanged(); // Вызываем событие
                }
            } 
        } // Относительная позиция элемента
        public Color Color { get; set; }
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
                    OnPropertyChanged(); // Вызываем событие
                }
            }
        } // Угол поворота элемента

        public abstract void Draw(Graphics graphics);                

        public virtual void LinkChange(FigureElement el) 
        {
            if (el is LineElement line)
                Position = line.EndPoint;
        }
    }
}