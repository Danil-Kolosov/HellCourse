using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GrafRedactor
{

    public class MainCoordinateAxes : FigureElement
    {
        LineElement3D axisX;
        LineElement3D axisY;
        LineElement3D axisZ;

        float margin;

        public MainCoordinateAxes(float margin, float lenght)
        {
            this.margin = margin;
            Initialize(margin, lenght);
        }


        private void Initialize(float margin, float lenght)
        {
            // Оси теперь рисуются от центра (0,0,0) в правильных направлениях
            axisX = new LineElement3D(new Point3D(0, 0, 0), new Point3D(lenght, 0, 0), Color.Red, 2f);
            axisY = new LineElement3D(new Point3D(0, 0, 0), new Point3D(0, lenght, 0), Color.Green, 2f);
            axisZ = new LineElement3D(new Point3D(0, 0, 0), new Point3D(0, 0, lenght), Color.Blue, 2f);
        }

        public MainCoordinateAxes(Point3D center, float lenght)
        {
            Initialize(center, lenght);
        }

        private void Initialize(Point3D center, float lenght)
        {
            axisX = new LineElement3D(center, new Point3D(center.X + lenght, center.Y, center.Z), Color.Red, 2f);
            axisY = new LineElement3D(center, new Point3D(center.X, center.Y + lenght, center.Z), Color.Green, 2f);
            axisZ = new LineElement3D(center, new Point3D(center.X, center.Y, center.Z + lenght), Color.Blue, 2f);
        }

        public override void Draw(Graphics graphics, LineCap endType = LineCap.ArrowAnchor)
        {
            // Сохраняем текущее состояние Graphics
            //graphics.ScaleTransform(1, -1);
            //GraphicsState state = graphics.Save();
            //graphics.ScaleTransform(1, -1);
            // Рисуем оси (с трансформацией)
            axisX.Draw(graphics, endType);
            axisY.Draw(graphics, endType);
            axisZ.Draw(graphics, endType);

            // Восстанавливаем исходное состояние для текста (без трансформации)
            //graphics.Restore(state);
            graphics.ScaleTransform(1, -1);
            // Подписи осей - рисуем в нормальных координатах
            using (Font font = new Font("Arial", 10))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                // Преобразуем мировые координаты в экранные для текста
                PointF xLabelPos = new PointF(axisX.EndPoint.X + 5, -axisX.EndPoint.Y - 10);
                PointF yLabelPos = new PointF(axisY.EndPoint.X - 10, -axisY.EndPoint.Y + 5);
                PointF zLabelPos = new PointF(axisZ.EndPoint.X - 10, -axisZ.EndPoint.Y - 10);

                graphics.DrawString("X", font, brush, xLabelPos);
                graphics.DrawString("Y", font, brush, yLabelPos);
                graphics.DrawString("Z", font, brush, zLabelPos);
            }
            graphics.ScaleTransform(1, -1);
            // Восстанавливаем трансформацию для возможного последующего рисования
            //graphics.Restore(state);
            //проблема с ограничивающей областью и с зажатым колесиком если вести - тоже
             //   кролесико починилось
        }

        // Остальные методы остаются без изменений...
        public override bool ContainsPoint(PointF point)
        {
            throw new NotImplementedException();
        }

        public override void DrawSelection(Graphics graphics)
        {
            throw new NotImplementedException();
        }

        public override RectangleF GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        public override void Mirror(bool horizontal)
        {
            throw new NotImplementedException();
        }

        public override void Mirror(float A, float B, float C)
        {
            throw new NotImplementedException();
        }

        public override void Move(PointF delta, float height, float weight, float deltaZ, string axeName)
        {
            throw new NotImplementedException();
        }

        public override void Projection(string coordinateAxis)
        {
            throw new NotImplementedException();
        }

        public override void Rotate(float angle, PointF cent)
        {
            Point3D center = new Point3D(0, 0, 0); // Теперь центр в (0,0,0)
            axisX.Rotate3D(center, angle, angle, angle, float.MaxValue);
            axisY.Rotate3D(center, angle, angle, angle, float.MaxValue);
            axisZ.Rotate3D(center, angle, angle, angle, float.MaxValue);
        }

        public void Rotate3D(float angleX, float angleY, float angleZ, Point3D center = null)
        {
            center = center ?? new Point3D(0, 0, 0); // По умолчанию центр в (0,0,0)
            axisX.Rotate3DWithScene(center, angleX, angleY, angleZ, float.MaxValue, "coordinateAxes");
            axisY.Rotate3DWithScene(center, angleX, angleY, angleZ, float.MaxValue, "coordinateAxes");
            axisZ.Rotate3DWithScene(center, angleX, angleY, angleZ, float.MaxValue, "coordinateAxes");
        }

        public override void Scale(PointF center, float sx, float sy)
        {
            throw new NotImplementedException();
        }

        public override void ScaleAverage(float scaleFactor)
        {
            throw new NotImplementedException();
        }
    }

    //public class MainCoordinateAxes : FigureElement
    //{
    //    LineElement3D axisX;
    //    LineElement3D axisY;
    //    LineElement3D axisZ;

    //    float margin;

    //    public MainCoordinateAxes(float margin, float lenght) 
    //    {
    //        this.margin = margin;
    //        Initialize(margin, lenght);
    //    }


    //    private void Initialize(float margin, float lenght)
    //    {
    //        axisX = new LineElement3D(new Point3D(margin, margin, 0), new Point3D(margin + lenght, margin, 0), Color.Red, 2f); //System.Drawing.Color.AliceBlue
    //        axisY = new LineElement3D(new Point3D(margin, margin, 0), new Point3D(margin, margin + lenght, 0), Color.Green, 2f);
    //        axisZ = new LineElement3D(new Point3D(margin, margin, 0), new Point3D(margin, margin, lenght), Color.Blue, 2f);
    //    }

    //    public MainCoordinateAxes(Point3D center, float lenght)
    //    {
    //        Initialize(center, lenght);
    //    }

    //    private void Initialize(Point3D center, float lenght)
    //    {
    //        axisX = new LineElement3D(center, new Point3D(center.X + lenght, center.Y, 0), Color.Red, 2f); //System.Drawing.Color.AliceBlue
    //        axisY = new LineElement3D(center, new Point3D(center.X, center.Y + lenght, 0), Color.Green, 2f);
    //        axisZ = new LineElement3D(center, new Point3D(center.X, center.Y, lenght), Color.Blue, 2f);
    //    }

    //    public override bool ContainsPoint(PointF point)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Draw(Graphics graphics, LineCap endType = LineCap.ArrowAnchor)
    //    {
    //        axisX.Draw(graphics, endType);
    //        axisY.Draw(graphics, endType);
    //        axisZ.Draw(graphics, endType);

    //        // Подписи осей
    //        using (Font font = new Font("Arial", 10))
    //        using (Brush brush = new SolidBrush(Color.Black))
    //        {
    //            graphics.DrawString("X", font, brush,
    //                100 + 5, 100 - 10);
    //            graphics.DrawString("X", font, brush,
    //                axisX.EndPoint.X + 5, axisX.EndPoint.Y - 10);
    //            graphics.DrawString("Y", font, brush,
    //                axisY.EndPoint.X - 10, axisY.EndPoint.Y + 5);
    //            graphics.DrawString("Z", font, brush,
    //                axisZ.EndPoint.X - 10, axisZ.EndPoint.Y - 10);
    //        }
    //    }

    //    public override void DrawSelection(Graphics graphics)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override RectangleF GetBoundingBox()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Mirror(bool horizontal)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Mirror(float A, float B, float C)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Move(PointF delta, float height, float weight, string axeName)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Projection(string coordinateAxis)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Rotate(float angle, PointF cent)
    //    {
    //        Point3D center = new Point3D(margin, margin, 0);
    //        axisX.Rotate3D(center, angle, angle, angle);
    //        axisY.Rotate3D(center, angle, angle, angle);
    //        axisZ.Rotate3D(center, angle, angle, angle);
    //    }

    //    public void Rotate3D(float angleX, float angleY, float angleZ, Point3D center /*= new Point3D(margin, margin, 0)*/)
    //    {
    //        //int3D center = new Point3D(margin, margin, 0); ЧЕЕЕРТ ИЗА ЭТОГО Криов возвращалосьт???
    //        //Point3D center = new Point3D(0, 0, 0);  
    //        axisX.Rotate3DWithScene(center, angleX, angleY, angleZ);
    //        axisY.Rotate3DWithScene(center, angleX, angleY, angleZ);
    //        axisZ.Rotate3DWithScene(center, angleX, angleY, angleZ);
    //    }

    //    public override void Scale(PointF center, float sx, float sy)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void ScaleAverage(float scaleFactor)
    //    {
    //        throw new NotImplementedException();
    //    }




    //}
}
