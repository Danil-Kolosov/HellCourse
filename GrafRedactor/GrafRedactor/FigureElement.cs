using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GrafRedactor
{
    public abstract class FigureElement
    {
        protected float _rotation;
        protected PointF _position;
        protected bool is3D = false;
        public bool Is3D { get => is3D; set { is3D = value; } }


        public PointF Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color Color { get; set; }
        public bool IsSelected { get; set; }
        public bool IsGrouped { get; set; }
        public string GroupId { get; set; }
        public event Action<FigureElement> OnChanged;

        protected virtual void OnPropertyChanged(FigureElement el = null)
        {
            if (el != null)
                OnChanged?.Invoke(el);
            else
                OnChanged?.Invoke(this);
        }

        public float Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    OnPropertyChanged();
                }
            }
        }

        // Новые методы для взаимодействия
        public abstract bool ContainsPoint(PointF point);
        public abstract RectangleF GetBoundingBox();
        public abstract void Move(PointF delta, float height, float weight, float deltaZ, string axeName);
        public abstract void Rotate(float angle, PointF centr = new PointF());
        public abstract void ScaleAverage(float scaleFactor);
        public abstract void Scale(PointF center, float sx, float sy);
        public abstract void Mirror(bool horizontal);
        public abstract void Mirror(float A, float B, float C);
        public abstract void Draw(Graphics graphics, LineCap endType = LineCap.Round);
        public abstract void DrawSelection(Graphics graphics);
        public abstract void Projection(string coordinateAxis);

        public virtual void LinkChange(FigureElement el)
        {
            if (el is LineElement line)
                Position = line.EndPoint;
        }
    }
}