using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        // Виртуальные методы для сериализации
        public virtual JObject Serialize()
        {
            return new JObject
            {
                ["Type"] = GetType().Name,
                ["Position"] = new JObject
                {
                    ["X"] = Position.X,
                    ["Y"] = Position.Y
                },
                ["Color"] = ColorTranslator.ToHtml(Color),
                ["Rotation"] = Rotation,
                ["IsSelected"] = IsSelected,
                ["IsGrouped"] = IsGrouped,
                ["GroupId"] = GroupId,
                ["Is3D"] = Is3D
            };
        }

        public virtual void Deserialize(JObject data)
        {
            Position = new PointF(
                (float)data["Position"]["X"],
                (float)data["Position"]["Y"]
            );

            Color = ColorTranslator.FromHtml((string)data["Color"]);
            Rotation = (float)data["Rotation"];
            IsSelected = (bool)data["IsSelected"];
            IsGrouped = (bool)data["IsGrouped"];
            GroupId = (string)data["GroupId"];
            Is3D = (bool)data["Is3D"];
        }

        // Статический метод для создания правильного экземпляра из данных
        public static FigureElement CreateFromData(JObject data)
        {
            string type = (string)data["Type"];

            switch (type)
            {
                case "LineElement":
                    return CreateLineElement(data);
                case "LineElement3D":
                    return CreateLineElement3D(data);
                case "Cube3D":
                    return CreateCube3D(data);
                case "PointElement3D":
                    return CreatePointElement3D(data);
                default:
                    throw new ArgumentException($"Unknown type: {type}");
            }
        }

        private static LineElement CreateLineElement(JObject data)
        {
            var start = new PointF(
                (float)data["StartPoint"]["X"],
                (float)data["StartPoint"]["Y"]
            );
            var end = new PointF(
                (float)data["EndPoint"]["X"],
                (float)data["EndPoint"]["Y"]
            );
            var color = ColorTranslator.FromHtml((string)data["Color"]);
            float thickness = (float)data["Thickness"];

            var element = new LineElement(start, end, color, thickness);
            element.Deserialize(data);
            return element;
        }

        private static LineElement3D CreateLineElement3D(JObject data)
        {
            var start3D = new Point3D(
                (float)data["StartPoint3D"]["X"],
                (float)data["StartPoint3D"]["Y"],
                (float)data["StartPoint3D"]["Z"]
            );
            var end3D = new Point3D(
                (float)data["EndPoint3D"]["X"],
                (float)data["EndPoint3D"]["Y"],
                (float)data["EndPoint3D"]["Z"]
            );
            var color = ColorTranslator.FromHtml((string)data["Color"]);
            float thickness = (float)data["Thickness"];

            var element = new LineElement3D(start3D, end3D, color, thickness);
            element.Deserialize(data);
            return element;
        }

        private static Cube3D CreateCube3D(JObject data)
        {
            var center = new Point3D(
                (float)data["Center"]["X"],
                (float)data["Center"]["Y"],
                (float)data["Center"]["Z"]
            );
            float size = (float)data["Size"];
            var color = ColorTranslator.FromHtml((string)data["Color"]);
            string currentAxisName = (string)data["CurrentAxisName"];
            float zs = (float)data["Zs"];

            var element = new Cube3D(center, size, color, currentAxisName, zs);
            element.Deserialize(data);
            return element;
        }

        private static PointElement3D CreatePointElement3D(JObject data)
        {
            var point3D = new Point3D(
                (float)data["Point3D"]["X"],
                (float)data["Point3D"]["Y"],
                (float)data["Point3D"]["Z"]
            );

            var element = new PointElement3D(point3D);
            element.Deserialize(data);
            return element;
        }

    }
}