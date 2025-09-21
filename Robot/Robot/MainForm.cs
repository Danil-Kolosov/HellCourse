using System;
using System.Drawing;
using System.Drawing.Drawing2D; // Добавляем для сглаживания
using System.Windows.Forms;

namespace Robot
{
    public partial class MainForm : Form
    {
        private RobotModel excavator;
        //private FigureElement selectedElement;
        //private PointF lastMousePosition;

        public MainForm()
        {
            InitializeComponent();
            excavator = new RobotModel(new PointF(600, 610));
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            excavator.Draw(e.Graphics);
        }

        private void TrackBar_Scroll(object sender, EventArgs e)
        {
            //if (sender == trackBarLength)
            //    line.Length = trackBarLength.Value;
            //else if (sender == trackBarAngle)
            //    line.Angle = trackBarAngle.Value;

            if (sender == trackBarMove)
                excavator.MoveX(trackBarMove.Value - excavator.BasePosition.X);
                //excavator.MoveEntireModel(new PointF(trackBarMove.Value - excavator.BasePosition.X, 0));
            //excavator.BasePosition = new PointF(trackBarMove.Value, excavator.BasePosition.Y);            
            else if (sender == trackBarBoomLength)
                excavator.Boom.Length = trackBarBoomLength.Value;
            else if (sender == trackBarSubArmAngle)
                excavator.SubArm.Rotation = trackBarSubArmAngle.Value;
            else if (sender == trackBarArmLength)
                excavator.Arm.Length = trackBarArmLength.Value;
            else if (sender == trackBarArmAngle)
                excavator.Arm.Rotation = trackBarArmAngle.Value;
            


            UpdateLabels();
            Invalidate(); // Запрашиваем перерисовку
        }

        private void UpdateLabels()
        {
            labelMove.Text = $"Позиция: {trackBarMove.Value}";
            labelBoomLength.Text = $"Длина: {trackBarBoomLength.Value}";
            labelSubArmAngle.Text = $"Угол: {trackBarSubArmAngle.Value}";
            labelArmLength.Text = $"Длина: {trackBarArmLength.Value}";
            labelArmAngle.Text = $"Угол: {trackBarArmAngle.Value}";
        }

        //private void MainForm_MouseDown(object sender, MouseEventArgs e)
        //{
        //    selectedElement = excavator.GetElementAtPoint(e.Location);
        //    lastMousePosition = e.Location;
        //}

        //private void MainForm_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        if (selectedElement != null)
        //        {
        //            PointF delta = new PointF(
        //                e.X - lastMousePosition.X,
        //                e.Y - lastMousePosition.Y);

        //            excavator.MoveEntireModel(delta);
        //            lastMousePosition = e.Location;
        //            Invalidate();
        //        }
        //    }
        //}
    }
}