using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Robot
{
    public class RobotModel
    {
        public PointF BasePosition { get; set; } // Базовая позиция всей конструкции
        private List<PointF> _originalPositions; // Храним оригинальные позиции относительно базы
        public List<FigureElement> Elements { get; private set; }

        // Ссылки на важные элементы для удобства управления
        public LineElement Boom { get; private set; }
        public LineElement SubArm { get; private set; }
        public LineElement Arm { get; private set; }
        public RectangleElement Cabin { get; private set; }

        public RobotModel(PointF basePosition)
        {
            BasePosition = basePosition;
            Elements = new List<FigureElement>();
            _originalPositions = new List<PointF>();
            CreateRobot();
            SaveOriginalPositions();
        }

        private void SaveOriginalPositions(FigureElement temp = null/*, float tempA =0*/)
        {
            _originalPositions.Clear();
            foreach (var element in Elements)
            {
                // Сохраняем позиции относительно базовой точки
                _originalPositions.Add(new PointF(
                    element.Position.X - BasePosition.X,
                    element.Position.Y - BasePosition.Y
                ));
            }
        }

        private void CreateRobot()
        {
            // 1. Прямоугольник - кабина
            Cabin = new RectangleElement(
                new PointF(BasePosition.X, BasePosition.Y),
                new SizeF(160, 14),
                Color.SteelBlue);
            Elements.Add(Cabin);

            

            // 3. 6 линий - стрела, рукоять, гидроцилиндры и т.д.
            Boom = new LineElement(
                new PointF(BasePosition.X, BasePosition.Y - Cabin.Size.Height/2), 
                100, -90, Color.Black, 4f);
            Elements.Add(Boom);
            

            SubArm = new LineElement(
                Boom.EndPoint, // Начинается там, где заканчивается стрела
                80, -30, Color.Orange, 3f);
            Elements.Add(SubArm);

            Arm = new LineElement(
                SubArm.EndPoint, // Начинается там, где заканчивается стрела
                100, 0, Color.Orange, 3f);
            Elements.Add(Arm);


            Boom.OnChanged += SubArm.LinkChange; //теперь так
            SubArm.OnChanged += Arm.LinkChange;
            //SubArm.OnChanged += this.SaveOriginalPositions;
            //Boom.OnChangedLenght += SubArm.LinkLenghtChange;
            //// 4 слэша - страый боллее менее рабочий варинат
            ////Boom.OnChanged += SubArm.LinkChange;
            ////Boom.OnChangedLenght += this.SaveOriginalPositions; старый подход с 2 линками изменений
            ////нужно было в SaveOriginalPositions паараметр float
            ////Boom.OnChangedRotate += SubArm.LinkRotateChange;
            //Boom.OnChangedRotate += SubArm.LinkChange;
            //Boom.OnChangedLenght += Arm.Length;
            //Boom.OnChangedRotate += Arm.Rotation;

            // 2. 4 круга - колёса/гусеницы
            CircleElement wheel1 = new CircleElement(new PointF(BasePosition.X - 70, BasePosition.Y + 15), 10, Color.Black);
            Elements.Add(wheel1);
            //шарнир1
            CircleElement linkBall1 = new CircleElement(Boom.EndPoint, 5, Color.Black);
            Elements.Add(linkBall1);
            Boom.OnChanged += linkBall1.LinkChange;
            //Boom.OnChanged += this.SaveOriginalPositions;
            //ширнир2
            CircleElement linkBall2 = new CircleElement(SubArm.EndPoint, 5, Color.Black);
            Elements.Add(linkBall2);
            SubArm.OnChanged += linkBall2.LinkChange;
            CircleElement wheel2 = new CircleElement(new PointF(BasePosition.X + 70, BasePosition.Y + 15), 10, Color.Black);
            Elements.Add(wheel2);
            //Cabin.OnChanged += wheel1.LinkChange;
            //Cabin.OnChanged += wheel2.LinkChange;


            // Добавьте остальные 4 линии по аналогии...

            // Создаем букву "П"
            PShape PLetter = new PShape();
            // Обновляем позицию буквы "П" относительно конца стрелы
            PLetter.UpdatePosition(Arm.EndPoint, Arm.Rotation - 90);
            PLetter.AddToModel(this); // Добавляем линии в общую модель
            Arm.OnChanged += PLetter.LinkChange;
            /*Круто но жаль при изменении тыкается на конец линии а надо центром
            float Width = 60f;    // Ширина буквы П
            float Height = 40f;   // Высота буквы П
            float Thickness = 3f; // Толщина линий
            Color color = Color.Blue;
            float parentAngle = -90;
            PointF endPoint = Arm.EndPoint;
            float angleRad = parentAngle * (float)Math.PI / 180f;
            PointF topLineStart = new PointF(
            endPoint.X - Width / 2 * (float)Math.Cos(angleRad),
            endPoint.Y - Width / 2 * (float)Math.Sin(angleRad));

            PointF topLineEnd = new PointF(
                endPoint.X + Width / 2 * (float)Math.Cos(angleRad),
                endPoint.Y + Width / 2 * (float)Math.Sin(angleRad));
    
            LineElement topLine = new LineElement(topLineStart, Width, parentAngle, color, Thickness);
            Arm.OnChanged += topLine.LinkChange;
            Elements.Add(topLine);
            LineElement leftLine = new LineElement(topLineStart, Height, parentAngle + 90f, color, Thickness);
            Elements.Add(leftLine);
            LineElement rightLine = new LineElement(topLineEnd, Height, parentAngle + 90f, color, Thickness);
            Elements.Add(rightLine);*/
            //Elements.Add(new LineElement(new PointF(BasePosition.X, BasePosition.Y - 40), 60, 75, Color.Gray, 2f));
            //Elements.Add(new LineElement(new PointF(BasePosition.X, BasePosition.Y - 40), 60, 15, Color.Gray, 2f));
            //Elements.Add(new LineElement(Boom.EndPoint, 40, -60, Color.Gray, 2f));
            //Elements.Add(new LineElement(Arm.EndPoint, 30, -120, Color.Gray, 2f));
        }

        public void Draw(Graphics graphics)
        {
            foreach (var element in Elements)
            {
                element.Draw(graphics);
            }
        }


        public void MoveEntireModel(PointF newBasePosition)
        {
            BasePosition = newBasePosition;
            UpdateElementPositions();
        }

        public void MoveX(float deltaX)
        {
            SaveOriginalPositions();
            BasePosition = new PointF(BasePosition.X + deltaX, BasePosition.Y);
            UpdateElementPositions();
            SaveOriginalPositions();
        }

        private void UpdateElementPositions()
        {
            // Обновляем позиции всех элементов на основе новой базовой позиции
            for (int i = 0; i < Elements.Count; i++)
            {
                PointF originalOffset = _originalPositions[i];
                Elements[i].Position = new PointF(
                    BasePosition.X + originalOffset.X,
                    BasePosition.Y + originalOffset.Y
                );
            }

            // Обновляем связи между элементами (стрела → рука)
            //UpdateConnections();
        }

        private void UpdateConnections()
        {
            // Обновляем связи, например, если стрела изменила позицию - двигаем руку
            if (Boom != null && Arm != null)
            {
                //Arm.Position = Boom.EndPoint;
            }

            // Здесь можно добавить обновление других связей
        }
        //****************************
        /*public void MoveEntireModel(PointF delta)
        {
            BasePosition = new PointF(BasePosition.X + delta.X, BasePosition.Y + delta.Y);
            RebuildModel();
        }

        public void RotateElement(FigureElement element, float angleDelta)
        {
            if (element is LineElement line)
            {
                line.Rotation += angleDelta;
                UpdateConnectedElements(line);
            }
        }

        private void UpdateConnectedElements(LineElement changedLine)
        {
            // Обновляем элементы, которые связаны с изменённой линией
            if (changedLine == Boom && Arm != null)
            {
                Arm.Position = changedLine.EndPoint;
            }
            // Добавьте логику для других связанных элементов
        }

        private void RebuildModel()
        {
            // Перестраиваем всю модель при перемещении базы
            Elements.Clear();
            CreateRobot();
        }*/

        //*************************************

        //public FigureElement GetElementAtPoint(PointF point)
        //{
        //    return Elements.FirstOrDefault(element => element.ContainsPoint(point));
        //}
    }
}


/*вроде матрицей смещения 
public class ExcavatorModel
{
    private float _offsetX = 0;

    public void MoveX(float deltaX)
    {
        _offsetX = deltaX;
        ApplyTransformToAllElements();
    }

    private void ApplyTransformToAllElements()
    {
        foreach (var element in Elements)
        {
            // Для каждого элемента вычисляем новую позицию
            // на основе оригинальной позиции + смещение
            element.Position = new PointF(
                element.OriginalPosition.X + _offsetX,
                element.OriginalPosition.Y
            );
        }
        UpdateConnections();
    }
}*/