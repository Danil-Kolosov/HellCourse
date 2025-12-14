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

        private void SaveOriginalPositions(FigureElement temp = null)
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

            

            // 2. линии - стрела, рукоять, рука
            Boom = new LineElement(
                new PointF(BasePosition.X, BasePosition.Y - Cabin.Size.Height/2), 
                100, -90, Color.Black, 4f);
            Elements.Add(Boom);
            

            SubArm = new LineElement(
                Boom.EndPoint, // Начинается там, где заканчивается стрела
                80, -30, Color.Orange, 3f);
            Elements.Add(SubArm);


            Arm = new LineElement(
                SubArm.EndPoint, // Начинается там, где заканчивается рукоять
                100, -30, Color.Orange, 3f);
            Elements.Add(Arm);
            //Arm.Rotation = 0;


            Boom.OnChanged += SubArm.LinkChange;
            SubArm.OnChanged += Arm.LinkChange;            

            // 3. 2 круга - колёса, 2 - шарниры
            CircleElement wheel1 = new CircleElement(new PointF(BasePosition.X - 70, BasePosition.Y + 15), 10, Color.Black);
            Elements.Add(wheel1);

            CircleElement wheel2 = new CircleElement(new PointF(BasePosition.X + 70, BasePosition.Y + 15), 10, Color.Black);
            Elements.Add(wheel2);

            //шарнир1
            CircleElement linkBall1 = new CircleElement(Boom.EndPoint, 5, Color.Black);
            Elements.Add(linkBall1);
            Boom.OnChanged += linkBall1.LinkChange;

            //ширнир2
            CircleElement linkBall2 = new CircleElement(SubArm.EndPoint, 5, Color.Black);
            Elements.Add(linkBall2);
            SubArm.OnChanged += linkBall2.LinkChange;

            // Создаем букву "П"
            PShape PLetter = new PShape();
            // Обновляем позицию буквы "П" относительно конца стрелы
            PLetter.UpdatePosition(Arm.EndPoint, Arm.Rotation - 90);
            PLetter.AddToModel(this); // Добавляем линии в общую модель
            Arm.OnChanged += PLetter.LinkChange;            
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
        }       
    }
}