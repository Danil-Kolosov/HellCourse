using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace FractalLandscape
{
    public partial class MainForm : Form
    {
        private int recursionDepth = 5;
        private float roughness = 0.5f;
        private float heightScale = 0.5f;
        private Random random = new Random();
        private float rotationX = (float)Math.PI / 2;
        private float rotationY = 0f;
        private Point lastMousePosition;
        private bool isMouseDown = false;

        // Храним вершины и треугольники отдельно
        private List<Vector3> vertices = new List<Vector3>();
        private List<int[]> triangles = new List<int[]>();
        private Dictionary<string, int> vertexCache = new Dictionary<string, int>();

        public MainForm()
        {
            InitializeComponent();
            this.Text = "Фрактальный ландшафт 3D (правильный алгоритм)";
            this.Size = new Size(1000, 700);
            this.BackColor = Color.White;
            this.DoubleBuffered = true;

            SetupControls();
            GenerateLandscape();
        }

        private void SetupControls()
        {
            Panel controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Height = 120;
            controlPanel.BackColor = Color.LightGray;

            // Глубина рекурсии
            Label depthLabel = new Label();
            depthLabel.Text = "Детализация (1-8):";
            depthLabel.Location = new Point(10, 15);
            depthLabel.AutoSize = true;

            NumericUpDown depthInput = new NumericUpDown();
            depthInput.Location = new Point(120, 10);
            depthInput.Width = 50;
            depthInput.Minimum = 1;
            depthInput.Maximum = 8;
            depthInput.Value = recursionDepth;
            depthInput.ValueChanged += (s, e) =>
            {
                recursionDepth = (int)depthInput.Value;
                GenerateLandscape();
                this.Invalidate();
            };

            // Шероховатость
            Label roughnessLabel = new Label();
            roughnessLabel.Text = "Шероховатость:";
            roughnessLabel.Location = new Point(10, 45);
            roughnessLabel.AutoSize = true;

            TrackBar roughnessTrack = new TrackBar();
            roughnessTrack.Location = new Point(120, 40);
            roughnessTrack.Width = 150;
            roughnessTrack.Minimum = 10;
            roughnessTrack.Maximum = 100;
            roughnessTrack.Value = (int)(roughness * 100);
            roughnessTrack.Scroll += (s, e) =>
            {
                roughness = roughnessTrack.Value / 100f;
                GenerateLandscape();
                this.Invalidate();
            };

            // Высота
            Label heightLabel = new Label();
            heightLabel.Text = "Высота:";
            heightLabel.Location = new Point(10, 75);
            heightLabel.AutoSize = true;

            TrackBar heightTrack = new TrackBar();
            heightTrack.Location = new Point(120, 70);
            heightTrack.Width = 150;
            heightTrack.Minimum = 10;
            heightTrack.Maximum = 100;
            heightTrack.Value = (int)(heightScale * 100);
            heightTrack.Scroll += (s, e) =>
            {
                heightScale = heightTrack.Value / 100f;
                GenerateLandscape();
                this.Invalidate();
            };

            // Кнопки
            Button regenerateButton = new Button();
            regenerateButton.Text = "Новый ландшафт";
            regenerateButton.Location = new Point(300, 10);
            regenerateButton.Click += (s, e) =>
            {
                random = new Random(DateTime.Now.Millisecond);
                GenerateLandscape();
                this.Invalidate();
            };

            Button resetButton = new Button();
            resetButton.Text = "Сбросить вид";
            resetButton.Location = new Point(300, 40);
            resetButton.Click += (s, e) =>
            {
                rotationX = (float)Math.PI / 2;
                rotationY = 0f;
                this.Invalidate();
            };

            //// Чекбокс для отображения сетки
            //CheckBox wireframeCheck = new CheckBox();
            //wireframeCheck.Text = "Показывать сетку";
            //wireframeCheck.Location = new Point(300, 75);
            //wireframeCheck.Checked = true;
            //wireframeCheck.CheckedChanged += (s, e) => this.Invalidate();

            // Информация
            Label helpLabel = new Label();
            helpLabel.Text = "";//"Зажмите ЛКМ для вращения | W/S/A/D - управление | R - сброс";
            helpLabel.Location = new Point(450, 15);
            helpLabel.AutoSize = true;
            helpLabel.Font = new Font("Arial", 8);

            // Добавляем элементы
            controlPanel.Controls.AddRange(new Control[] {
                depthLabel, depthInput,
                roughnessLabel, roughnessTrack,
                heightLabel, heightTrack,
                regenerateButton, resetButton,
                /*wireframeCheck,*/ helpLabel
            });

            this.Controls.Add(controlPanel);

            // Обработчики мыши
            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isMouseDown = true;
                    lastMousePosition = e.Location;
                }
            };

            this.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    isMouseDown = false;
            };

            this.MouseMove += (s, e) =>
            {
                if (isMouseDown)
                {
                    rotationY += (e.X - lastMousePosition.X) * 0.01f;
                    rotationX += (e.Y - lastMousePosition.Y) * 0.01f;
                    lastMousePosition = e.Location;
                    this.Invalidate();
                }
            };

            // Обработчики клавиш
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                float speed = 0.05f;
                switch (e.KeyCode)
                {
                    case Keys.W: rotationX -= speed; break;
                    case Keys.S: rotationX += speed; break;
                    case Keys.A: rotationY -= speed; break;
                    case Keys.D: rotationY += speed; break;
                    case Keys.R: rotationX = rotationY = 0; break;
                    case Keys.Space: GenerateLandscape(); break;
                }
                this.Invalidate();
            };
        }

        private void GenerateLandscape()
        {
            vertices.Clear();
            triangles.Clear();
            vertexCache.Clear();

            // Создаем начальные вершины большого треугольника
            // Используем правильную геометрию для непрерывной поверхности
            Vector3 v0 = new Vector3(-1, 0, -0.5f);
            Vector3 v1 = new Vector3(1, 0, -0.5f);
            Vector3 v2 = new Vector3(0, 0, 1);

            // Добавляем начальные вершины
            int i0 = AddVertex(v0);
            int i1 = AddVertex(v1);
            int i2 = AddVertex(v2);

            // Добавляем начальный треугольник
            triangles.Add(new int[] { i0, i1, i2 });

            // Начинаем рекурсивное разбиение
            for (int i = 0; i < recursionDepth; i++)
            {
                List<int[]> newTriangles = new List<int[]>();

                foreach (var tri in triangles)
                {
                    Vector3 a = vertices[tri[0]];
                    Vector3 b = vertices[tri[1]];
                    Vector3 c = vertices[tri[2]];

                    // Находим середины сторон (с правильным кэшированием!)
                    int ab = GetOrCreateMidpoint(tri[0], tri[1], a, b, i);
                    int bc = GetOrCreateMidpoint(tri[1], tri[2], b, c, i);
                    int ca = GetOrCreateMidpoint(tri[2], tri[0], c, a, i);

                    // Создаем 4 новых треугольника
                    newTriangles.Add(new int[] { tri[0], ab, ca });
                    newTriangles.Add(new int[] { ab, tri[1], bc });
                    newTriangles.Add(new int[] { ca, bc, tri[2] });
                    newTriangles.Add(new int[] { ab, bc, ca });
                }

                triangles = newTriangles;
            }

            // Нормализуем высоты для лучшего отображения
            NormalizeHeights();
        }

        private int AddVertex(Vector3 v)
        {
            vertices.Add(v);
            return vertices.Count - 1;
        }

        private int GetOrCreateMidpoint(int i1, int i2, Vector3 v1, Vector3 v2, int depth)
        {
            // Создаем уникальный ключ для ребра (независимо от порядка вершин)
            string key = i1 < i2 ? $"{i1}_{i2}" : $"{i2}_{i1}";

            if (vertexCache.TryGetValue(key, out int existingIndex))
            {
                return existingIndex;
            }

            // Вычисляем середину
            Vector3 midpoint = new Vector3(
                (v1.X + v2.X) / 2,
                (v1.Y + v2.Y) / 2,
                (v1.Z + v2.Z) / 2
            );

            // Добавляем случайное смещение по высоте
            // Амплитуда уменьшается с увеличением глубины
            float amplitude = roughness * heightScale * (float)Math.Pow(0.5f, depth);
            midpoint.Y += (float)(random.NextDouble() * 2 - 1) * amplitude;

            int newIndex = AddVertex(midpoint);
            vertexCache[key] = newIndex;

            return newIndex;
        }

        private void NormalizeHeights()
        {
            // Находим минимальную и максимальную высоту
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var v in vertices)
            {
                if (v.Y < minY) minY = v.Y;
                if (v.Y > maxY) maxY = v.Y;
            }

            // Нормализуем высоты к диапазону [0, heightScale]
            float range = maxY - minY;
            if (range > 0.001f)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vector3 v = vertices[i];
                    v.Y = (v.Y - minY) / range * heightScale;
                    vertices[i] = v;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(Color.White);

            int drawingHeight = this.ClientSize.Height - 120;
            Rectangle viewport = new Rectangle(0, 120, this.ClientSize.Width, drawingHeight);

            // Матрицы преобразования
            Matrix4x4 rotX = Matrix4x4.CreateRotationX(rotationX);
            Matrix4x4 rotY = Matrix4x4.CreateRotationY(rotationY);
            Matrix4x4 transform = rotX * rotY;

            // Рисуем треугольники
            using (Pen wirePen = new Pen(Color.FromArgb(100, Color.Black), 0.8f))
            using (Pen silhouettePen = new Pen(Color.FromArgb(200, Color.DarkBlue), 1.5f))
            {
                // Сначала рисуем все треугольники как сетку
                foreach (var tri in triangles)
                {
                    DrawTriangle(e.Graphics, tri, transform, viewport, wirePen);
                }

                // Затем рисуем силуэт (контур) для лучшей объемности
                //DrawSilhouette(e.Graphics, transform, viewport, silhouettePen);
            }

            // Рисуем оси координат
            DrawAxes(e.Graphics, transform, viewport);

            // Информация
            string info = "";//$"Вершин: {vertices.Count}, Треугольников: {triangles.Count} | Вращение: X={rotationX:F2}, Y={rotationY:F2}";
            e.Graphics.DrawString(info, new Font("Arial", 9), Brushes.Black, 10, drawingHeight + 90);
        }

        private void DrawTriangle(Graphics g, int[] tri, Matrix4x4 transform, Rectangle viewport, Pen pen)
        {
            Vector3 v1 = Vector3.Transform(vertices[tri[0]], transform);
            Vector3 v2 = Vector3.Transform(vertices[tri[1]], transform);
            Vector3 v3 = Vector3.Transform(vertices[tri[2]], transform);

            PointF p1 = Project(v1, viewport);
            PointF p2 = Project(v2, viewport);
            PointF p3 = Project(v3, viewport);

            // Проверяем, не выходит ли треугольник за границы экрана
            if (IsTriangleVisible(p1, p2, p3))
            {
                g.DrawLine(pen, p1, p2);
                g.DrawLine(pen, p2, p3);
                g.DrawLine(pen, p3, p1);
            }
        }

        private void DrawSilhouette(Graphics g, Matrix4x4 transform, Rectangle viewport, Pen pen)
        {
            // Находим ребра, которые являются границами (принадлежат только одному треугольнику)
            Dictionary<string, int> edgeCount = new Dictionary<string, int>();

            foreach (var tri in triangles)
            {
                AddEdge(edgeCount, tri[0], tri[1]);
                AddEdge(edgeCount, tri[1], tri[2]);
                AddEdge(edgeCount, tri[2], tri[0]);
            }

            // Рисуем только граничные ребра (силуэт)
            foreach (var edge in edgeCount)
            {
                if (edge.Value == 1) // Ребро принадлежит только одному треугольнику
                {
                    string[] parts = edge.Key.Split('_');
                    int i1 = int.Parse(parts[0]);
                    int i2 = int.Parse(parts[1]);

                    Vector3 v1 = Vector3.Transform(vertices[i1], transform);
                    Vector3 v2 = Vector3.Transform(vertices[i2], transform);

                    PointF p1 = Project(v1, viewport);
                    PointF p2 = Project(v2, viewport);

                    g.DrawLine(pen, p1, p2);
                }
            }
        }

        private void AddEdge(Dictionary<string, int> edgeCount, int i1, int i2)
        {
            string key = i1 < i2 ? $"{i1}_{i2}" : $"{i2}_{i1}";
            if (edgeCount.ContainsKey(key))
                edgeCount[key]++;
            else
                edgeCount[key] = 1;
        }

        private bool IsTriangleVisible(PointF p1, PointF p2, PointF p3)
        {
            // Простая проверка - если все точки за пределами экрана, не рисуем
            RectangleF bounds = new RectangleF(0, 0, this.ClientSize.Width, this.ClientSize.Height);
            return bounds.Contains(p1) || bounds.Contains(p2) || bounds.Contains(p3);
        }

        private PointF Project(Vector3 point, Rectangle viewport)
        {
            float scale = Math.Min(viewport.Width, viewport.Height) / 3f;
            float centerX = viewport.Width / 2f + viewport.X;
            float centerY = viewport.Height / 2f + viewport.Y + 50; // Смещаем вниз для лучшего обзора

            // Простая ортографическая проекция
            return new PointF(
                point.X * scale + centerX,
                -point.Z * scale + centerY  // Z становится вертикалью на экране
            );
        }

        private void DrawAxes(Graphics g, Matrix4x4 transform, Rectangle viewport)
        {
            float scale = 50;
            Vector3 origin = Vector3.Zero;

            // Преобразуем оси
            Vector3 xAxis = Vector3.TransformNormal(new Vector3(1, 0, 0), transform);
            Vector3 yAxis = Vector3.TransformNormal(new Vector3(0, 1, 0), transform);
            Vector3 zAxis = Vector3.TransformNormal(new Vector3(0, 0, 1), transform);

            PointF o = Project(origin, viewport);
            PointF x = Project(xAxis * 0.2f, viewport); // Укорачиваем для наглядности
            PointF y = Project(yAxis * 0.2f, viewport);
            PointF z = Project(zAxis * 0.2f, viewport);

            g.DrawLine(Pens.Red, o, x);
            g.DrawLine(Pens.Green, o, y);
            g.DrawLine(Pens.Blue, o, z);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(984, 661);
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