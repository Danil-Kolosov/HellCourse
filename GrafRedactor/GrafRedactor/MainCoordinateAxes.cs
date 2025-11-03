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
            axisX = new LineElement3D(new Point3D(margin, margin, 0), new Point3D(margin + lenght, margin, 0), Color.Red, 2f); //System.Drawing.Color.AliceBlue
            axisY = new LineElement3D(new Point3D(margin, margin, 0), new Point3D(margin, margin + lenght, 0), Color.Green, 2f);
            axisZ = new LineElement3D(new Point3D(margin, margin, 0), new Point3D(margin, margin, lenght), Color.Blue, 2f);
        }

        public MainCoordinateAxes(Point3D center, float lenght)
        {
            Initialize(center, lenght);
        }

        private void Initialize(Point3D center, float lenght)
        {
            axisX = new LineElement3D(center, new Point3D(center.X + lenght, center.Y, 0), Color.Red, 2f); //System.Drawing.Color.AliceBlue
            axisY = new LineElement3D(center, new Point3D(center.X, center.Y + lenght, 0), Color.Green, 2f);
            axisZ = new LineElement3D(center, new Point3D(center.X, center.Y, lenght), Color.Blue, 2f);
        }

        public override bool ContainsPoint(PointF point)
        {
            throw new NotImplementedException();
        }

        public override void Draw(Graphics graphics, LineCap endType = LineCap.ArrowAnchor)
        {
            axisX.Draw(graphics, endType);
            axisY.Draw(graphics, endType);
            axisZ.Draw(graphics, endType);

            // Подписи осей
            using (Font font = new Font("Arial", 10))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                graphics.DrawString("X", font, brush,
                    100 + 5, 100 - 10);
                graphics.DrawString("X", font, brush,
                    axisX.EndPoint.X + 5, axisX.EndPoint.Y - 10);
                graphics.DrawString("Y", font, brush,
                    axisY.EndPoint.X - 10, axisY.EndPoint.Y + 5);
                graphics.DrawString("Z", font, brush,
                    axisZ.EndPoint.X - 10, axisZ.EndPoint.Y - 10);
            }
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

        public override void Move(PointF delta, float height, float weight, string axeName)
        {
            throw new NotImplementedException();
        }

        public override void Projection(string coordinateAxis)
        {
            throw new NotImplementedException();
        }

        public override void Rotate(float angle, PointF cent)
        {
            Point3D center = new Point3D(margin, margin, 0);
            axisX.Rotate3D(center, angle, angle, angle);
            axisY.Rotate3D(center, angle, angle, angle);
            axisZ.Rotate3D(center, angle, angle, angle);
        }

        public void Rotate3D(float angleX, float angleY, float angleZ, Point3D center /*= new Point3D(margin, margin, 0)*/)
        {
            //int3D center = new Point3D(margin, margin, 0); ЧЕЕЕРТ ИЗА ЭТОГО Криов возвращалосьт???
            //Point3D center = new Point3D(0, 0, 0);  

            axisX.Rotate3DWithScene(center, angleX, angleY, angleZ);
            axisY.Rotate3DWithScene(center, angleX, angleY, angleZ);
            axisZ.Rotate3DWithScene(center, angleX, angleY, angleZ);
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
}
