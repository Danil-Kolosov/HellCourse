using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Pawn3D
{
    public partial class MainForm : Form
    {
        private GLControl glControl;
        private Panel controlPanel;
        private Timer renderTimer;

        // Параметры пешки
        private float sphereRadius = 0.8f;
        // Прозрачность пешки (0.0 - полностью прозрачная, 1.0 - полностью непрозрачная)
        private float pawnOpacity = 0.3f;
        private float ellipsoidRadiusX = 0.8f, ellipsoidRadiusY = 0.4f, ellipsoidRadiusZ = 0.8f;
        private float paraboloidA = 1.0f, paraboloidB = 1.0f, paraboloidHeight = 2.0f;
        private float cylinderRadius = 1.8f, cylinderHeight = 0.6f;

        // Для управления 3D
        private float rotationX = 0;
        private float rotationY = 0;
        private Point lastMousePos;
        private float cameraDistance = 15f;

        // Список точек
        private List<Point3D> points = new List<Point3D>();

        public MainForm()
        {
            InitializeComponent();
            InitializeControls();
            InitializeTimer();

            // Автоматическое вращение
            rotationX = 30;
            rotationY = 30;
        }

        private void InitializeComponent()
        {
            this.Text = "3D Пешка";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Основной контейнер
            var mainTable = new TableLayoutPanel();
            mainTable.Dock = DockStyle.Fill;
            mainTable.ColumnCount = 2;
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            mainTable.RowCount = 1;
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // 3D область
            var glPanel = new Panel();
            glPanel.Dock = DockStyle.Fill;
            glPanel.BackColor = Color.Black;
            mainTable.Controls.Add(glPanel, 0, 0);

            // Панель управления
            controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Fill;
            controlPanel.AutoScroll = true;
            controlPanel.BackColor = Color.White;
            mainTable.Controls.Add(controlPanel, 1, 0);

            this.Controls.Add(mainTable);

            // Инициализация GLControl
            glControl = new GLControl();
            glControl.Dock = DockStyle.Fill;
            glControl.BackColor = Color.Black;
            glControl.Load += GLControl_Load;
            glControl.Paint += GLControl_Paint;
            glControl.Resize += GLControl_Resize;
            glControl.MouseDown += GLControl_MouseDown;
            glControl.MouseMove += GLControl_MouseMove;
            glControl.MouseWheel += GLControl_MouseWheel;

            glPanel.Controls.Add(glControl);
        }

        private void InitializeControls()
        {
            controlPanel.Padding = new Padding(10);

            int yPos = 10;

            // Заголовок
            var title = new Label()
            {
                Text = "Управление",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(250, 25)
            };
            controlPanel.Controls.Add(title);
            yPos += 35;

            // Координаты точки
            var coordLabel = new Label()
            {
                Text = "Координаты точки:",
                Location = new Point(10, yPos),
                Size = new Size(250, 20)
            };
            controlPanel.Controls.Add(coordLabel);
            yPos += 25;

            var xLabel = new Label() { Text = "X:", Location = new Point(10, yPos), Size = new Size(20, 20), ForeColor = Color.Red };
            var xBox = new TextBox() { Text = "0", Location = new Point(30, yPos), Size = new Size(70, 20) };

            var yLabel = new Label() { Text = "Y:", Location = new Point(110, yPos), Size = new Size(20, 20), ForeColor = Color.Green };
            var yBox = new TextBox() { Text = "2.0", Location = new Point(130, yPos), Size = new Size(70, 20) };

            var zLabel = new Label() { Text = "Z:", Location = new Point(210, yPos), Size = new Size(20, 20), ForeColor = Color.Blue };
            var zBox = new TextBox() { Text = "0", Location = new Point(230, yPos), Size = new Size(70, 20) };

            controlPanel.Controls.Add(xLabel);
            controlPanel.Controls.Add(xBox);
            controlPanel.Controls.Add(yLabel);
            controlPanel.Controls.Add(yBox);
            controlPanel.Controls.Add(zLabel);
            controlPanel.Controls.Add(zBox);
            yPos += 30;

            // Кнопки точек
            var addPointBtn = new Button()
            {
                Text = "Добавить точку",
                Location = new Point(10, yPos),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White
            };
            addPointBtn.Click += (s, e) =>
            {
                // Поддерживаем различные форматы чисел
                string xText = xBox.Text.Replace(',', '.');  // Заменяем запятую на точку
                string yText = yBox.Text.Replace(',', '.');
                string zText = zBox.Text.Replace(',', '.');

                if (float.TryParse(xText, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(yText, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(zText, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float z))
                {
                    bool isInside = IsPointInPawn(x, y, z);
                    points.Add(new Point3D(x, y, z, isInside));
                    UpdateStats();

                    // Автоматическая перерисовка
                    if (glControl != null && !glControl.IsDisposed)
                    {
                        glControl.Invalidate();
                    }
                }
                else
                {
                    MessageBox.Show("Пожалуйста, введите корректные дробные числа.\n" +
                                   "Примеры: 0.5, -1.25, 3.14", "Ошибка ввода",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            //var addRandomBtn = new Button()
            //{
            //    Text = "10 случайных",
            //    Location = new Point(140, yPos),
            //    Size = new Size(120, 30),
            //    BackColor = Color.FromArgb(118, 75, 162),
            //    ForeColor = Color.White
            //};

            var addRandomBtn = new Button()
            {
                Text = "10 случайных",
                Size = new Size(120, 30),
                Location = new Point(140, yPos + 30),
                BackColor = Color.FromArgb(118, 75, 162),
                ForeColor = Color.White
            };
            addRandomBtn.Click += (s, e) => AddRandomPoints();

            var clearBtn = new Button()
            {
                Text = "Очистить точки",
                Location = new Point(140, yPos),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(220, 80, 80),
                ForeColor = Color.White
            };
            clearBtn.Click += (s, e) =>
            {
                points.Clear();
                UpdateStats();
            };

            controlPanel.Controls.Add(addPointBtn);
            controlPanel.Controls.Add(addRandomBtn);
            controlPanel.Controls.Add(clearBtn);
            yPos += 40;

            // Статистика
            var statsLabel = new Label()
            {
                Text = "Статистика:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(250, 20)
            };
            controlPanel.Controls.Add(statsLabel);
            yPos += 25;

            var totalLabel = new Label()
            {
                Text = "Всего точек: 0",
                Location = new Point(10, yPos),
                Size = new Size(150, 20)
            };
            var insideLabel = new Label()
            {
                Text = "Внутри пешки: 0",
                Location = new Point(10, yPos + 25),
                Size = new Size(150, 20)
            };

            controlPanel.Controls.Add(totalLabel);
            controlPanel.Controls.Add(insideLabel);
            yPos += 60;

            // Параметры пешки
            var paramsLabel = new Label()
            {
                Text = "Параметры пешки:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(250, 20)
            };
            controlPanel.Controls.Add(paramsLabel);
            yPos += 25;

            // Создаем простые слайдеры для параметров
            CreateParameterSlider("Шар - радиус", 0.3f, 1.5f, sphereRadius, yPos, val => { sphereRadius = val; });
            yPos += 50;
            CreateParameterSlider("Эллипсоид - X", 0.3f, 1.5f, ellipsoidRadiusX, yPos, val => { ellipsoidRadiusX = val; });
            yPos += 50;
            CreateParameterSlider("Эллипсоид - Y", 0.1f, 1.0f, ellipsoidRadiusY, yPos, val => { ellipsoidRadiusY = val; });
            yPos += 50;
            CreateParameterSlider("Эллипсоид - Z", 0.3f, 1.5f, ellipsoidRadiusZ, yPos, val => { ellipsoidRadiusZ = val; });
            yPos += 50;
            CreateParameterSlider("Параболоид - a", 0.5f, 2.0f, paraboloidA, yPos, val => { paraboloidA = val; });
            yPos += 50;
            CreateParameterSlider("Параболоид - b", 0.5f, 2.0f, paraboloidB, yPos, val => { paraboloidB = val; });
            yPos += 50;
            CreateParameterSlider("Параболоид - высота", 1.0f, 3.0f, paraboloidHeight, yPos, val => { paraboloidHeight = val; });
            yPos += 50;
            CreateParameterSlider("Цилиндр - радиус", 0.5f, 3.0f, cylinderRadius, yPos, val => { cylinderRadius = val; });
            yPos += 50;
            CreateParameterSlider("Цилиндр - высота", 0.2f, 1.5f, cylinderHeight, yPos, val => { cylinderHeight = val; });

            // Сохраняем ссылки для обновления статистики
            this.totalLabel = totalLabel;
            this.insideLabel = insideLabel;
        }

        private Label totalLabel, insideLabel;

        private void CreateParameterSlider(string label, float min, float max, float initial, int yPos, Action<float> onChange)
        {
            var panel = new Panel();
            panel.Location = new Point(10, yPos);
            panel.Size = new Size(280, 40);

            var labelCtrl = new Label()
            {
                Text = label,
                Location = new Point(0, 0),
                Size = new Size(150, 20)
            };

            var valueLabel = new Label()
            {
                Text = initial.ToString("F2"),
                Location = new Point(160, 0),
                Size = new Size(50, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            var trackBar = new TrackBar()
            {
                Location = new Point(0, 20),
                Size = new Size(230, 20),
                Minimum = (int)(min * 100),
                Maximum = (int)(max * 100),
                Value = (int)(initial * 100),
                TickFrequency = 10,
                TickStyle = TickStyle.None
            };

            trackBar.Scroll += (s, e) =>
            {
                float value = trackBar.Value / 100f;
                valueLabel.Text = value.ToString("F2");
                onChange?.Invoke(value);

                // Динамическая перепроверка всех точек
                RecheckAllPoints();
            };

            panel.Controls.Add(labelCtrl);
            panel.Controls.Add(valueLabel);
            panel.Controls.Add(trackBar);
            controlPanel.Controls.Add(panel);
        }

        private void UpdateStats()
        {
            if (totalLabel != null && insideLabel != null)
            {
                int insideCount = 0;
                foreach (var point in points)
                {
                    if (point.IsInside) insideCount++;
                }
                totalLabel.Text = $"Всего точек: {points.Count}";
                insideLabel.Text = $"Внутри пешки: {insideCount}";
            }
        }

        private void RecheckAllPoints()
        {
            foreach (var point in points)
            {
                point.IsInside = IsPointInPawn(point.X, point.Y, point.Z);
            }
            UpdateStats();

            // Запускаем перерисовку, чтобы точки изменили цвет
            if (glControl != null && !glControl.IsDisposed)
            {
                glControl.Invalidate();
            }
        }

        private void InitializeTimer()
        {
            renderTimer = new Timer();
            renderTimer.Interval = 16; // ~60 FPS
            renderTimer.Tick += (s, e) =>
            {
                if (glControl != null && !glControl.IsDisposed)
                {
                    glControl.Invalidate();
                }
            };
            renderTimer.Start();
        }

        // Функция Хэвисайда
        private float Heaviside(float x)
        {
            return x >= 0 ? 1 : 0;
        }

        // Проверка принадлежности точки пешке
        private bool IsPointInPawn(float x, float y, float z)
        {
            // Высота основания цилиндра
            float cylinderBase = 0;
            float cylinderTop = cylinderHeight;

            // Параболоид
            float paraboloidBase = cylinderTop;
            float paraboloidTop = paraboloidBase + paraboloidHeight;

            // Эллипсоид
            float ellipsoidBase = paraboloidTop;
            float ellipsoidCenterY = ellipsoidBase + ellipsoidRadiusY;

            // Шар
            float sphereBase = ellipsoidBase + ellipsoidRadiusY * 2;
            float sphereCenterY = sphereBase + sphereRadius;

            // Проверка цилиндра
            float inCylinder = Heaviside(cylinderRadius * cylinderRadius - (x * x + z * z)) *
                              Heaviside(y - cylinderBase) *
                              Heaviside(cylinderTop - y);

            // Проверка параболоида
            float localYParaboloid = y - paraboloidBase;
            float inParaboloid = Heaviside(paraboloidHeight - localYParaboloid) *
                                Heaviside(localYParaboloid) *
                                Heaviside(paraboloidHeight - ((x * x) / (paraboloidA * paraboloidA) +
                                                             (z * z) / (paraboloidB * paraboloidB)) - localYParaboloid);

            // Проверка эллипсоида
            float localYEllipsoid = y - ellipsoidCenterY;
            float inEllipsoid = Heaviside(1 - ((x / ellipsoidRadiusX) * (x / ellipsoidRadiusX) +
                                              (localYEllipsoid / ellipsoidRadiusY) * (localYEllipsoid / ellipsoidRadiusY) +
                                              (z / ellipsoidRadiusZ) * (z / ellipsoidRadiusZ))) *
                               Heaviside(y - ellipsoidBase);

            // Проверка шара
            float localYSphere = y - sphereCenterY;
            float inSphere = Heaviside(sphereRadius * sphereRadius - (x * x + localYSphere * localYSphere + z * z)) *
                            Heaviside(y - sphereBase);

            float totalHeaviside = inCylinder + inParaboloid + inEllipsoid + inSphere;

            return totalHeaviside > 0;
        }

        private void GLControl_Load(object sender, EventArgs e)
        {
            if (glControl == null) return;

            try
            {
                // Инициализация OpenGL
                GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);

                // Включение освещения
                GL.Enable(EnableCap.Lighting);
                GL.Enable(EnableCap.Light0);
                GL.Enable(EnableCap.ColorMaterial);

                float[] lightPos = { 5.0f, 10.0f, 5.0f, 1.0f };
                GL.Light(LightName.Light0, LightParameter.Position, lightPos);

                float[] lightAmbient = { 0.3f, 0.3f, 0.3f, 1.0f };
                GL.Light(LightName.Light0, LightParameter.Ambient, lightAmbient);

                float[] lightDiffuse = { 0.8f, 0.8f, 0.8f, 1.0f };
                GL.Light(LightName.Light0, LightParameter.Diffuse, lightDiffuse);

                GL.Enable(EnableCap.Normalize);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации OpenGL: {ex.Message}");
            }
        }

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            if (glControl == null || glControl.IsDisposed) return;

            try
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                // Настройка проекции
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();

                float aspect = glControl.Width > 0 ? (float)glControl.Width / glControl.Height : 1.0f;
                Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, aspect, 0.1f, 100.0f);
                GL.LoadMatrix(ref perspective);

                // Настройка вида
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();

                // Позиция камеры
                GL.Translate(0, 0, -cameraDistance);
                GL.Rotate(rotationX, 0, 1, 0);
                GL.Rotate(rotationY, 1, 0, 0);

                // Оси координат
                DrawAxes();

                // Сетка
                DrawGrid();

                // Пешка
                DrawPawn();

                // Точки
                DrawPoints();

                glControl.SwapBuffers();
            }
            catch (Exception ex)
            {
                // Ошибка рисования - показываем сообщение
                e.Graphics.Clear(Color.Black);
                e.Graphics.DrawString("Ошибка рисования 3D",
                    new Font("Arial", 12), Brushes.White, 10, 10);
                e.Graphics.DrawString(ex.Message,
                    new Font("Arial", 10), Brushes.Yellow, 10, 40);
            }
        }

        private void AddRandomPoints()
        {
            Random rand = new Random();

            // Генерируем 10 случайных точек
            for (int i = 0; i < 10; i++)
            {
                // Диапазоны для генерации (подобраны под размеры пешки)
                float maxRadius = Math.Max(Math.Max(cylinderRadius,
                    paraboloidA * (float)Math.Sqrt(paraboloidHeight)),
                    sphereRadius) * 1.5f;

                float totalHeight = cylinderHeight + paraboloidHeight +
                                   ellipsoidRadiusY * 2 + sphereRadius * 2;

                // Генерируем случайные координаты
                float angle = (float)(rand.NextDouble() * Math.PI * 2);
                float radius = (float)(rand.NextDouble() * maxRadius);
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;
                float y = (float)(rand.NextDouble() * totalHeight);

                // Проверяем принадлежность точке
                bool isInside = IsPointInPawn(x, y, z);

                // Добавляем точку
                points.Add(new Point3D(x, y, z, isInside));
            }

            // Обновляем интерфейс
            UpdateStats();

            // Перерисовываем 3D вид
            if (glControl != null && !glControl.IsDisposed)
            {
                glControl.Invalidate();
            }

            // Показываем сообщение
            MessageBox.Show("Сгенерировано 10 случайных точек",
                           "Генерация точек",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Information);
        }

        private void DrawAxes()
        {
            GL.Disable(EnableCap.Lighting);

            // Ось X (красная)
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(1.0f, 0.0f, 0.0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(3, 0, 0);
            GL.End();

            // Ось Y (зеленая)
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(0.0f, 1.0f, 0.0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 6, 0);
            GL.End();

            // Ось Z (синяя)
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(0.0f, 0.0f, 1.0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 3);
            GL.End();

            GL.Enable(EnableCap.Lighting);
        }

        private void DrawGrid()
        {
            GL.Disable(EnableCap.Lighting);
            GL.Color3(0.3f, 0.3f, 0.3f);

            GL.Begin(PrimitiveType.Lines);
            for (int i = -5; i <= 5; i++)
            {
                GL.Vertex3(i, 0, -5);
                GL.Vertex3(i, 0, 5);
                GL.Vertex3(-5, 0, i);
                GL.Vertex3(5, 0, i);
            }
            GL.End();

            GL.Enable(EnableCap.Lighting);
        }

        private void DrawPawn()
        {
            GL.Enable(EnableCap.Lighting);

            // Включаем смешивание для прозрачности
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Отключаем запись в буфер глубины для правильного смешивания
            GL.DepthMask(false);

            // 1. ЦИЛИНДР - основание на Y=0
            GL.PushMatrix();
            GL.Color4(0.8f, 0.8f, 0.8f, pawnOpacity);
            // Цилиндр рисуется от Y=0 до Y=height, НЕ смещаем его!
            GLUT.Cylinder(cylinderRadius, cylinderRadius, cylinderHeight, 20, 2);
            GL.PopMatrix();

            // 2. ПАРАБОЛОИД - стоит на цилиндре
            GL.PushMatrix();
            GL.Color4(0.7f, 0.7f, 0.7f, pawnOpacity);
            // Основание параболоида на верхней грани цилиндра (Y = cylinderHeight)
            GL.Translate(0, cylinderHeight, 0);
            DrawSimpleParaboloid();
            GL.PopMatrix();

            // 3. ЭЛЛИПСОИД - стоит на параболоиде
            GL.PushMatrix();
            GL.Color4(0.9f, 0.9f, 0.9f, pawnOpacity);
            // Центр эллипсоида над параболоидом
            // Параболоид имеет высоту paraboloidHeight, а его вершина в Y = cylinderHeight + paraboloidHeight
            // Эллипсоид центрирован по Y, так что нижняя точка: cylinderHeight + paraboloidHeight
            GL.Translate(0, cylinderHeight + paraboloidHeight + ellipsoidRadiusY, 0);
            GL.Scale(ellipsoidRadiusX, ellipsoidRadiusY, ellipsoidRadiusZ);
            GLUT.Sphere(1.0f, 20, 20);
            GL.PopMatrix();

            // 4. ШАР - на вершине эллипсоида
            GL.PushMatrix();
            GL.Color4(0.6f, 0.6f, 0.6f, pawnOpacity);
            // Шар центрирован, так что нижняя точка: cylinderHeight + paraboloidHeight + 2*ellipsoidRadiusY
            GL.Translate(0, cylinderHeight + paraboloidHeight + ellipsoidRadiusY * 2 + sphereRadius, 0);
            GLUT.Sphere(sphereRadius, 20, 20);
            GL.PopMatrix();

            // Восстанавливаем настройки глубины и выключаем смешивание
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
        }

        private void DrawSimpleParaboloid()
        {
            int segments = 20;
            float angleStep = 2.0f * (float)Math.PI / segments;
            int heightSegments = 10;

            // Перевернутый параболоид: основание широкое, вершина узкая
            // Уравнение: y = h - (x²/a² + z²/b²)

            for (int h = 0; h < heightSegments; h++)
            {
                float t1 = (float)h / heightSegments;
                float t2 = (float)(h + 1) / heightSegments;

                // Для перевернутого параболоида: радиус уменьшается с высотой
                float r1 = (float)Math.Sqrt(paraboloidHeight * (1 - t1));
                float r2 = (float)Math.Sqrt(paraboloidHeight * (1 - t2));

                // Высота от основания
                float y1 = t1 * paraboloidHeight;
                float y2 = t2 * paraboloidHeight;

                GL.Begin(PrimitiveType.QuadStrip);
                for (int i = 0; i <= segments; i++)
                {
                    float angle = i * angleStep;
                    float cos = (float)Math.Cos(angle);
                    float sin = (float)Math.Sin(angle);

                    float x1 = paraboloidA * r1 * cos;
                    float z1 = paraboloidB * r1 * sin;

                    float x2 = paraboloidA * r2 * cos;
                    float z2 = paraboloidB * r2 * sin;

                    // Нормаль для параболоида
                    Vector3 normal1 = new Vector3(-2 * x1 / (paraboloidA * paraboloidA), 1, -2 * z1 / (paraboloidB * paraboloidB));
                    normal1.Normalize();
                    GL.Normal3(normal1);

                    GL.Vertex3(x1, y1, z1);

                    Vector3 normal2 = new Vector3(-2 * x2 / (paraboloidA * paraboloidA), 1, -2 * z2 / (paraboloidB * paraboloidB));
                    normal2.Normalize();
                    GL.Normal3(normal2);

                    GL.Vertex3(x2, y2, z2);
                }
                GL.End();
            }
        }

        private void DrawPoints()
        {
            // Включаем смешивание для точек (если понадобится)
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Убедимся, что точки рисуются поверх
            GL.Disable(EnableCap.DepthTest);

            GL.Disable(EnableCap.Lighting);

            foreach (var point in points)
            {
                GL.PushMatrix();
                GL.Translate(point.X, point.Y, point.Z);

                if (point.IsInside)
                {
                    GL.Color3(0.0f, 1.0f, 0.0f); // Зеленый
                }
                else
                {
                    GL.Color3(1.0f, 0.0f, 0.0f); // Красный
                }

                // Простая сфера для точки
                GLUT.Sphere(0.08f, 8, 8);

                GL.PopMatrix();
            }

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Disable(EnableCap.Blend);
        }

        private void GLControl_Resize(object sender, EventArgs e)
        {
            if (glControl == null || glControl.IsDisposed) return;

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
        }

        private void GLControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastMousePos = e.Location;
            }
        }

        private void GLControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                float dx = e.X - lastMousePos.X;
                float dy = e.Y - lastMousePos.Y;

                rotationX += dx * 0.5f;
                rotationY += dy * 0.5f;

                lastMousePos = e.Location;
            }
        }

        private void GLControl_MouseWheel(object sender, MouseEventArgs e)
        {
            cameraDistance += e.Delta * 0.01f;
            cameraDistance = Math.Max(5, Math.Min(50, cameraDistance));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (renderTimer != null)
            {
                renderTimer.Stop();
                renderTimer.Dispose();
            }

            if (glControl != null)
            {
                glControl.Dispose();
            }
        }
    }

    // Простой класс для рисования примитивов
    public static class GLUT
    {
        public static void Sphere(float radius, int slices, int stacks)
        {
            for (int i = 0; i < stacks; i++)
            {
                float phi1 = (float)Math.PI * i / stacks;
                float phi2 = (float)Math.PI * (i + 1) / stacks;

                GL.Begin(PrimitiveType.QuadStrip);
                for (int j = 0; j <= slices; j++)
                {
                    float theta = 2.0f * (float)Math.PI * j / slices;
                    float cosTheta = (float)Math.Cos(theta);
                    float sinTheta = (float)Math.Sin(theta);

                    for (int k = 1; k >= 0; k--)
                    {
                        float phi = (k == 0) ? phi1 : phi2;
                        float sinPhi = (float)Math.Sin(phi);
                        float cosPhi = (float)Math.Cos(phi);

                        float x = radius * sinPhi * cosTheta;
                        float y = radius * cosPhi;
                        float z = radius * sinPhi * sinTheta;

                        GL.Normal3(x / radius, y / radius, z / radius);
                        GL.Vertex3(x, y, z);
                    }
                }
                GL.End();
            }
        }

        public static void Cylinder(float baseRadius, float topRadius, float height, int slices, int stacks)
        {
            float angleStep = 2.0f * (float)Math.PI / slices;
            float heightStep = height / stacks;

            // Боковая поверхность
            for (int i = 0; i < stacks; i++)
            {
                float y1 = i * heightStep;
                float y2 = (i + 1) * heightStep;
                float r1 = baseRadius + (topRadius - baseRadius) * i / stacks;
                float r2 = baseRadius + (topRadius - baseRadius) * (i + 1) / stacks;

                GL.Begin(PrimitiveType.QuadStrip);
                for (int j = 0; j <= slices; j++)
                {
                    float angle = j * angleStep;
                    float cos = (float)Math.Cos(angle);
                    float sin = (float)Math.Sin(angle);

                    float nx = cos;
                    float nz = sin;

                    GL.Normal3(nx, 0, nz);
                    GL.Vertex3(r1 * cos, y1, r1 * sin);
                    GL.Vertex3(r2 * cos, y2, r2 * sin);
                }
                GL.End();
            }

            // Верхняя крышка
            if (topRadius > 0)
            {
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Normal3(0, 1, 0);
                GL.Vertex3(0, height, 0);
                for (int i = 0; i <= slices; i++)
                {
                    float angle = i * angleStep;
                    GL.Vertex3(topRadius * (float)Math.Cos(angle), height, topRadius * (float)Math.Sin(angle));
                }
                GL.End();
            }

            // Нижняя крышка
            if (baseRadius > 0)
            {
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Normal3(0, -1, 0);
                GL.Vertex3(0, 0, 0);
                for (int i = slices; i >= 0; i--)
                {
                    float angle = i * angleStep;
                    GL.Vertex3(baseRadius * (float)Math.Cos(angle), 0, baseRadius * (float)Math.Sin(angle));
                }
                GL.End();
            }
        }

    }

    public class Point3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public bool IsInside { get; set; }

        public Point3D(float x, float y, float z, bool isInside)
        {
            X = x;
            Y = y;
            Z = z;
            IsInside = isInside;
        }

        // Для удобства обновления
        public void UpdateIsInside(bool isInside)
        {
            IsInside = isInside;
        }
    }
}