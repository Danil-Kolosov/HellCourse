using System;
using System.Drawing;
using System.Windows.Forms;

namespace FractalTree2D
{
    public partial class MainForm : Form
    {
        private int recursionDepth = 8;
        private float randomness = 0.3f;
        private Random random = new Random();

        public MainForm()
        {
            InitializeComponent();
            this.Text = "Фрактальное дерево";
            this.Size = new Size(900, 700);
            this.BackColor = Color.White;
            this.DoubleBuffered = true;

            SetupControls();
        }

        private void SetupControls()
        {
            Panel controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Height = 80;
            controlPanel.BackColor = Color.LightGray;

            // Глубина рекурсии
            Label depthLabel = new Label();
            depthLabel.Text = "Глубина рекурсии (1-25):";
            depthLabel.Location = new Point(10, 15);
            depthLabel.AutoSize = true;

            NumericUpDown depthInput = new NumericUpDown();
            depthInput.Location = new Point(150, 10);
            depthInput.Width = 60;
            depthInput.Minimum = 1;
            depthInput.Maximum = 25;
            depthInput.Value = recursionDepth;
            depthInput.ValueChanged += (s, e) =>
            {
                recursionDepth = (int)depthInput.Value;
                this.Invalidate();
            };

            // Случайность
            Label randomLabel = new Label();
            randomLabel.Text = "Фактор случайности (0-100%):";
            randomLabel.Location = new Point(10, 45);
            randomLabel.AutoSize = true;

            TrackBar randomTrack = new TrackBar();
            randomTrack.Location = new Point(150, 40);
            randomTrack.Width = 200;
            randomTrack.Minimum = 0;
            randomTrack.Maximum = 100;
            randomTrack.Value = (int)(randomness * 100);
            randomTrack.Scroll += (s, e) =>
            {
                randomness = randomTrack.Value / 100f;
                this.Invalidate();
            };

            // Кнопка нового дерева
            Button regenerateButton = new Button();
            regenerateButton.Text = "Новое дерево";
            regenerateButton.Location = new Point(400, 10);
            regenerateButton.Click += (s, e) =>
            {
                random = new Random(DateTime.Now.Millisecond);
                this.Invalidate();
            };

            // Добавляем элементы
            controlPanel.Controls.AddRange(new Control[] {
                depthLabel, depthInput,
                randomLabel, randomTrack,
                regenerateButton
            });

            this.Controls.Add(controlPanel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Очищаем фон
            e.Graphics.Clear(Color.White);

            // Начинаем отрисовку с основания дерева
            PointF startPoint = new PointF(this.ClientSize.Width / 2, this.ClientSize.Height - 50);

            // Основные параметры дерева
            float initialLength = 180f; // Фиксированная длина
            float initialAngle = 270; // Растет вверх
            float initialThickness = 12f; // Фиксированная толщина

            // Рисуем дерево
            DrawBranch(e.Graphics, startPoint, initialAngle, initialLength, recursionDepth, initialThickness);
        }

        private void DrawBranch(Graphics g, PointF startPoint, float angle, float length, int depth, float thickness)
        {
            if (depth == 0 || length < 1)
                return;

            // Вычисляем конечную точку ветви
            float angleRad = angle * (float)Math.PI / 180f;
            PointF endPoint = new PointF(
                startPoint.X + length * (float)Math.Cos(angleRad),
                startPoint.Y + length * (float)Math.Sin(angleRad)
            );

            // Цвет ветви - темно-коричневый
            Color branchColor = Color.FromArgb(100, 70, 40);

            // Рисуем ветвь
            using (Pen branchPen = new Pen(branchColor, thickness))
            {
                g.DrawLine(branchPen, startPoint, endPoint);
            }

            // Если это не последний уровень, рисуем ответвления
            if (depth > 1)
            {
                // Уменьшаем толщину для следующих ветвей
                float newThickness = thickness * 0.65f;

                // Уменьшаем длину для следующих ветвей
                float newLength = length * 0.65f;

                // Базовые углы для левой и правой ветвей
                float baseAngle = 30; // Фиксированный угол 30°
                float leftAngle = angle - baseAngle;
                float rightAngle = angle + baseAngle;

                // Добавляем случайность к углам
                float angleRandomness = randomness * 40f;
                leftAngle += (float)(random.NextDouble() * 2 - 1) * angleRandomness;
                rightAngle += (float)(random.NextDouble() * 2 - 1) * angleRandomness;

                // Добавляем случайность к длине
                float leftLength = newLength * (1 + (float)(random.NextDouble() * 2 - 1) * randomness * 0.5f);
                float rightLength = newLength * (1 + (float)(random.NextDouble() * 2 - 1) * randomness * 0.5f);

                // Ограничиваем минимальную длину
                if (leftLength < 1) leftLength = 1;
                if (rightLength < 1) rightLength = 1;

                // Рисуем левую ветвь
                DrawBranch(g, endPoint, leftAngle, leftLength, depth - 1, newThickness);

                // Рисуем правую ветвь
                DrawBranch(g, endPoint, rightAngle, rightLength, depth - 1, newThickness);

                // С вероятностью зависящей от случайности рисуем третью ветвь
                if (random.NextDouble() < randomness * 0.3 && depth > 3)
                {
                    float middleAngle = angle + (float)(random.NextDouble() * 2 - 1) * 20f;
                    float middleLength = newLength * (0.4f + (float)random.NextDouble() * 0.4f);
                    DrawBranch(g, endPoint, middleAngle, middleLength, depth - 2, newThickness * 0.7f);
                }
            }
            else
            {
                // На последнем уровне рисуем листья
                DrawLeaf(g, endPoint);
            }
        }

        private void DrawLeaf(Graphics g, PointF position)
        {
            // Фиксированный размер листа
            float leafSize = 5;

            // Фиксированный зеленый цвет листа
            Color leafColor = Color.FromArgb(100, 180, 60);

            using (Brush leafBrush = new SolidBrush(leafColor))
            {
                // Рисуем лист
                g.FillEllipse(leafBrush,
                    position.X - leafSize / 2,
                    position.Y - leafSize / 2,
                    leafSize, leafSize);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(884, 661);
            this.Name = "MainForm";
            this.ResumeLayout(false);
        }
    }

    //static class Program
    //{
    //    [STAThread]
    //    static void Main()
    //    {
    //        Application.EnableVisualStyles();
    //        Application.SetCompatibleTextRenderingDefault(false);
    //        Application.Run(new MainForm());
    //    }
    //}
}