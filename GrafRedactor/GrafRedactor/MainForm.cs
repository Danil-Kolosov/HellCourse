using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using System.Drawing.Drawing2D;
namespace GrafRedactor
{
    public partial class MainForm : Form
    {
        private MainCoordinateAxes coordinateAxes;// = new MainCoordinateAxes(150, 100);
        private string currentAxeName = "xoy";
        private float angleX = 0;
        private float angleY = 0;
        private float angleZ = 0;
        private bool is3DMode = false;
        private ToolStripStatusLabel statusLabel;
        private StatusStrip statusStrip;
        private List<FigureElement> figures = new List<FigureElement>();
        private FigureElement selectedFigure;
        private PointF lastMousePos;
        private bool isDragging = false;
        private bool isResizing = false;
        private bool isDrawing = false;
        private bool resizeStartPoint;
        private PointF drawingStartPoint;

        private bool isRotatingView = false;
        private float rotationSensitivity = 0.5f; // Чувствительность вращения
        private float resetAngleValueX = 0f;
        private float resetAngleValueY = 0f;
        private float resetAngleValueZ = 0f;
        private float totalRotationX = 0f;
        private float totalRotationY = 0f;
        private float totalRotationZ = 0f;
        
        


        //Группировка
        private GroupManager groupManager = new GroupManager();
        private List<FigureElement> selectedFigures = new List<FigureElement>(); // Множественное выделение
        private bool isGroupSelected = false;
        private string currentGroupId = null;

        // Элементы управления для панели параметров
        private Panel parametersPanel;
        private NumericUpDown numStartX, numStartY, numEndX, numEndY, numStartZ, numEndZ;
        private NumericUpDown numThickness;
        private ComboBox comboColor;
        private ComboBox comboOperation;
        private Button btnDelete;
        private Label lblStartPoint, lblEndPoint, lblStartX, lblStartY, lblEndX, lblEndY, lblThickness, lblColor, lblOperations, 
            lblEquation, lblEquation3D, lblEquation2D, lblStartZ, lblEndZ, lblCubeCenterPoint;

        // Константы для размеров
        private const int PARAMETERS_PANEL_WIDTH = 450;
        private const int MIN_DRAWING_WIDTH = 500;

        private float ZERO_POINT_DIFFERENCE_X = 0;
        private float ZERO_POINT_DIFFERENCE_Y = 50;
        private float ZC = /*500*/float.MaxValue;

        public MainForm()
        {
            InitializeComponent(); // Вызов метода из Designer
            InitializeCustomComponents(); // Своя инициализация
            selectedFigures = new List<FigureElement>();
            UpdateModeVisuals(); // Обновляем визуальное отображение режима
            

        }

        private void InitializeCustomComponents()
        {
            this.DoubleBuffered = true; // Убираем мерцание
            this.ResizeRedraw = true; // Важно: перерисовывать при изменении размера
            InitializeParametersPanel();
            // Обработчики событий мыши и клавиатуры
            this.MouseDown += MainForm_MouseDown;
            this.MouseMove += MainForm_MouseMove;
            this.MouseUp += MainForm_MouseUp;
            this.Paint += MainForm_Paint;
            this.KeyDown += MainForm_KeyDown;
            this.KeyPreview = true; // Для обработки клавиш Delete
            this.Resize += MainForm_Resize; // Обработчик изменения размера
            this.MouseWheel += MainForm_MouseWheel;
            // Обработчик для кнопки колесика
            this.MouseDown += MainForm_MouseDownForRotation;
            this.MouseUp += MainForm_MouseUpForRotation;

            InitializeStatusBar(); // Добавляем строку состояния
            UpdateModeVisuals(); // Обновляем интерфейс под текущий режим

            SimpleCamera.GroupManager = groupManager;
            SimpleCamera.DrawingArea = GetDrawingArea();

            ZERO_POINT_DIFFERENCE_X = GetDrawingAreaCenter().X;
            ZERO_POINT_DIFFERENCE_Y = GetDrawingAreaCenter().Y;

            // Инициализация осей координат - после установки облатси рисования в камере
            InitializeCoordinateAxes();
        }

        private void InitializeCoordinateAxes()
        {
            var drawingArea = GetDrawingArea();
            float margin = 150f; // Отступ от края
            float length = Math.Min(drawingArea.Width, drawingArea.Height) / 4; // Длина осей

            //coordinateAxes = new MainCoordinateAxes(drawingArea.Width/2, drawingArea.Width);
            //ИЛИ
            coordinateAxes = new MainCoordinateAxes(/*new Point3D(0,0,0) тут как раз мнимый*/CalculateSceneCenter(), 100);

        }

        private void InitializeParametersPanel()
        {
            parametersPanel = new Panel
            {
                Location = new Point(650, 0),
                Size = new Size(PARAMETERS_PANEL_WIDTH, 600),
                BackColor = Color.LightGray,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right // Якоря для автоматического изменения размера
            };

            int y = 10;

            // Заголовок панели параметров
            var lblTitle = new Label
            {
                Text = "Параметры линии",
                Location = new Point(20, y),
                Size = new Size(200, 25),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            parametersPanel.Controls.Add(lblTitle);
            y += 10;

            lblEquation = new Label
            {
                Text = "Уравнение прямой\n",
                Location = new Point(20, y+20),
                Size = new Size(150, 25),
                Font = new Font("Arial", 12),
                //ForeColor = Color.Black
            };            
            parametersPanel.Controls.Add(lblEquation);
            lblEquation3D
                = new Label
                {
                    Text = "Уравнение прямой 3D\n",
                    Location = new Point(170, y + 25),
                    Size = new Size(400, 25),
                    Font = new Font("Arial", 9),
                };
            parametersPanel.Controls.Add(lblEquation3D);
            lblEquation2D
                = new Label
                {
                    Text = "Уравнение прямой 2D\n",
                    Location = new Point(170, y + 25),
                    Size = new Size(400, 25),
                    Font = new Font("Arial", 12),
                };
            parametersPanel.Controls.Add(lblEquation2D);
            y += 40;

            // Координаты начальной точки
            lblStartPoint = AddLabel("Начальная точка:", 20, y);
            lblCubeCenterPoint = AddLabel("Центр куба:", 20, y);
            lblCubeCenterPoint.Visible = false;
            y += 25;
            lblStartX = AddLabel("X:", 40, y);
            numStartX = AddNumericUpDown(70, y, 0, 1000, 100);
            numStartX.ValueChanged += Parameters_ValueChanged;

            lblStartY = AddLabel("Y:", 160, y);
            numStartY = AddNumericUpDown(190, y, 0, 1000, 100);
            numStartY.ValueChanged += Parameters_ValueChanged;
           
            lblStartZ = AddLabel("Z:", 280, y);
            lblStartZ.Visible = false;
            numStartZ = AddNumericUpDown(330, y, -1000000000, 1000000000, 0); //??????!
            numStartZ.ValueChanged += Parameters_ValueChanged;
            parametersPanel.Controls.Add(numStartZ);//******
            y += 40;

            // Координаты конечной точки
            lblEndPoint = AddLabel("Конечная точка:", 20, y);
            y += 25;
            lblEndX = AddLabel("X:", 40, y);
            numEndX = AddNumericUpDown(70, y, 0, 1000, 200);
            numEndX.ValueChanged += Parameters_ValueChanged;

            lblEndY = AddLabel("Y:", 160, y);
            numEndY = AddNumericUpDown(190, y, 0, 1000, 200);
            numEndY.ValueChanged += Parameters_ValueChanged;

            lblEndZ = AddLabel("Z:", 280, y);
            lblEndZ.Visible = false;
            numEndZ = AddNumericUpDown(330, y, -1000000000, 1000000000, 0); //??????!
            numEndZ.ValueChanged += Parameters_ValueChanged;
            parametersPanel.Controls.Add(numEndZ);//****
            y += 40;

            // Толщина линии
            lblThickness = AddLabel("Толщина линии:", 20, y);
            y += 25;
            numThickness = AddNumericUpDown(20, y, 1, 50, 3);
            numThickness.ValueChanged += Parameters_ValueChanged;
            y += 40;

            //Операции
            lblOperations = AddLabel("Операции:", 180, y - 65);
            //lblEquation3D
            //    = new Label
            //    {
            //        Text = "Уравнение прямой 3D\n",
            //        Location = new Point(160, y - 10),
            //        Size = new Size(400, 25),
            //        Font = new Font("Arial", 9),
            //    };
            //parametersPanel.Controls.Add(lblEquation3D);
            comboOperation = new ComboBox
            {
                Location = new Point(180, y - 40),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboOperation.Items.AddRange(new object[] { "", "Смещение", "Вращение", "Масштабирование", "Общее масштабирование", "Зеркалирование", "Ортографическое проецирование", "Перспективное проецирование" });
            comboOperation.SelectedIndex = 0;
            comboOperation.SelectedIndexChanged += СomboOperation_SelectedIndexChanged;
            parametersPanel.Controls.Add(comboOperation);

            // Цвет линии
            lblColor = AddLabel("Цвет линии:", 20, y);
            y += 25;
            comboColor = new ComboBox
            {
                Location = new Point(20, y),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // Русские названия цветов
            comboColor.Items.AddRange(new object[] {
                "Черный", "Красный", "Зеленый", "Синий", "Оранжевый", "Фиолетовый",
                "Желтый", "Розовый", "Коричневый", "Серый"
            });
            comboColor.SelectedIndex = 0;
            comboColor.SelectedIndexChanged += ComboColor_SelectedIndexChanged;
            parametersPanel.Controls.Add(comboColor);
            y += 60;

            // Кнопка удаления
            btnDelete = new Button
            {
                Location = new Point(20, y),
                Size = new Size(100, 30),
                Text = "Удалить",
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnDelete.Click += BtnDelete_Click;
            parametersPanel.Controls.Add(btnDelete);

            // Инструкция
            var lblInstruction = new Label
            {
                Text = "Управление:\n• ЛКМ - рисовать/выделять\n• Drag - перемещать\n• Drag за точки - изменять размер\n• Delete - удалить",
                Location = new Point(20, 450),
                Size = new Size(300, 80),
                Font = new Font("Arial", 9),
                ForeColor = Color.DarkSlateGray
            };
            parametersPanel.Controls.Add(lblInstruction);

            

            // Кнопки группировки
            var btnGroup = new Button
            {
                Location = new Point(20, 380),
                Size = new Size(120, 30),
                Text = "Группировать",
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnGroup.Click += BtnGroup_Click;
            parametersPanel.Controls.Add(btnGroup);

            var btnUngroup = new Button
            {
                Location = new Point(150, 380),
                Size = new Size(150, 30),
                Text = "Разгруппировать",
                BackColor = Color.LightYellow,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnUngroup.Click += BtnUngroup_Click;
            parametersPanel.Controls.Add(btnUngroup);

            this.Controls.Add(parametersPanel);
        }

        // Обновление позиции панели параметров при изменении размера формы
        private void UpdateParametersPanelPosition()
        {
            //старое сверху закрывается
            //if (parametersPanel != null)
            //{
            //    parametersPanel.Location = new Point(this.ClientSize.Width - PARAMETERS_PANEL_WIDTH, 0);
            //    parametersPanel.Height = this.ClientSize.Height;
            //}

            if (parametersPanel != null)
            {
                int menuHeight = menuStrip.Height;
                parametersPanel.Location = new Point(this.ClientSize.Width - PARAMETERS_PANEL_WIDTH, menuHeight);
                parametersPanel.Height = this.ClientSize.Height - menuHeight - (statusStrip?.Height ?? 0);
            }
        }

        // Получение области рисования (вся форма минус панель параметров)
        private Rectangle GetDrawingArea()
        {
            //это рабочее старое - но тут сверху панель параметров закрывалась стрип меню
            //int drawingWidth = this.ClientSize.Width - (/*parametersPanel.Visible ?*/ PARAMETERS_PANEL_WIDTH/* : 0*/);
            //return new Rectangle(0, 0, drawingWidth, this.ClientSize.Height);

            int menuHeight = menuStrip.Height;
            int statusHeight = statusStrip?.Height ?? 0;
            int drawingWidth = this.ClientSize.Width - PARAMETERS_PANEL_WIDTH;
            int drawingHeight = this.ClientSize.Height - menuHeight - statusHeight;

            return new Rectangle(0, menuHeight, drawingWidth, drawingHeight);
        }

        // Обработчик изменения размера формы
        private void MainForm_Resize(object sender, EventArgs e)
        {
            UpdateParametersPanelPosition();

            // Обновляем максимальные значения координат в соответствии с новой областью рисования
            var drawingArea = GetDrawingArea();
            SimpleCamera.DrawingArea = drawingArea;
            if (numStartX != null)
            {
                numStartX.Maximum = drawingArea.Width;
                numStartY.Maximum = drawingArea.Height;
                numEndX.Maximum = drawingArea.Width;
                numEndY.Maximum = drawingArea.Height;
            }

            this.Invalidate(); // Принудительная перерисовка
        }

        //private Label AddLabel(string text, int x, int y)
        //{
        //    var label = new Label
        //    {
        //        Text = text,
        //        Location = new Point(x, y),
        //        Size = new Size(120, text.Length),
        //        Font = new Font("Arial", 10)
        //    };
        //    parametersPanel.Controls.Add(label);
        //    return label;
        //}
        private Label AddLabel(string text, int x, int y, Font font = null, int padding = 10)
        {
            if (font == null)
                font = new Font("Arial", 10);

            using (var graphics = this.CreateGraphics())
            {
                SizeF textSize = graphics.MeasureString(text, font);

                int width = (int)Math.Ceiling(textSize.Width) + padding;
                int height = (int)Math.Ceiling(textSize.Height) + 4;

                var label = new Label
                {
                    Text = text,
                    Location = new Point(x, y),
                    Size = new Size(width, height),
                    Font = font,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                parametersPanel.Controls.Add(label);
                return label;
            }
        }

        private NumericUpDown AddNumericUpDown(int x, int y, int min, int max, decimal value)
        {
            var numeric = new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(80, 25),
                Minimum = min,
                Maximum = max,
                Value = value,
                Font = new Font("Arial", 10)
            };
            parametersPanel.Controls.Add(numeric);
            return numeric;
        }

        private void UpdateParametersPanel()
        {
            //сейчас - опасная штука
            numStartX.Maximum = 1000000000;
            numStartY.Maximum = 1000000000;
            numEndX.Maximum = 1000000000;
            numEndY.Maximum = 1000000000;
            numStartX.Minimum = -1000000000;
            numStartY.Minimum = -1000000000;
            numEndX.Minimum = -1000000000;
            numEndY.Minimum = -1000000000;
            //было - не очень опасная штука
            //numStartX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
            //numStartY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
            //numEndX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
            //numEndY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
            //numStartX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
            //numStartY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
            //numEndX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
            //numEndY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;

            if (selectedFigure is LineElement line)
            {
                parametersPanel.Visible = true;
                UpdateParametersPanelPosition(); // Обновляем позицию

                //ZERO_POINT_DIFFERENCE_X = GetDrawingAreaCenter().X;
                //ZERO_POINT_DIFFERENCE_Y = GetDrawingAreaCenter().Y;

                // Обновляем максимальные значения в соответствии с текущей областью рисования
                var drawingArea = GetDrawingArea();
                
                //numStartX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                //numStartY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_Y;
                //numEndX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                //numEndY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_Y;
                //numStartX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                //numStartY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_Y;
                //numEndX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                //numEndY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_Y;

                (float a, float b, float c, float d) = line.GetEquation();
                lblEquation2D.Text = $"{a}x+{b}y+{c}=0"; //для 3 д тут z использовать еще
                lblEquation.Visible = true;
                lblEquation2D.Visible = true;
                lblCubeCenterPoint.Visible = false;
                lblStartPoint.Visible = true;
                lblEndPoint.Visible = true;
                numEndX.Visible = true;
                numEndY.Visible = true;
                numThickness.Visible = true;
                lblThickness.Visible = true; lblColor.Visible = true;
                lblEndX.Visible = true; lblEndY.Visible = true;
                comboColor.Visible = true;

                // Временно отключаем события чтобы избежать рекурсии
                numStartX.ValueChanged -= Parameters_ValueChanged;
                numStartY.ValueChanged -= Parameters_ValueChanged;
                numEndX.ValueChanged -= Parameters_ValueChanged;
                numEndY.ValueChanged -= Parameters_ValueChanged;
                numThickness.ValueChanged -= Parameters_ValueChanged;                
                numStartZ.ValueChanged -= Parameters_ValueChanged;
                numEndZ.ValueChanged -= Parameters_ValueChanged;
                if(!(line is LineElement3D))
                {
                    numStartX.Value = (decimal)(line.StartPoint.X); // важно
                    numStartY.Value = (decimal)(line.StartPoint.Y);// важно
                    numEndX.Value = (decimal)(line.EndPoint.X);// важно
                    numEndY.Value = (decimal)(line.EndPoint.Y);// важно
                    //switch (currentAxeName)
                    //{
                    //    case "xoy":
                    //        numStartX.Value = (decimal)(line.StartPoint.X - ZERO_POINT_DIFFERENCE_X); // важно
                    //        numStartY.Value = (decimal)(line.StartPoint.Y - ZERO_POINT_DIFFERENCE_Y);// важно
                    //        numEndX.Value = (decimal)(line.EndPoint.X - ZERO_POINT_DIFFERENCE_X);// важно
                    //        numEndY.Value = (decimal)(line.EndPoint.Y - ZERO_POINT_DIFFERENCE_Y);// важно
                    //        break;
                    //    case "yoz":
                    //        numStartX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numStartY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numEndX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numEndY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;

                    //        numStartX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numStartY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numEndX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numEndY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;

                    //        numStartX.Value = (decimal)(line.StartPoint.X - ZERO_POINT_DIFFERENCE_X); // важно
                    //        numStartY.Value = (decimal)(line.StartPoint.Y - ZERO_POINT_DIFFERENCE_X);// важно
                    //        numEndX.Value = (decimal)(line.EndPoint.X);// важно
                    //        numEndY.Value = (decimal)(line.EndPoint.Y - ZERO_POINT_DIFFERENCE_X);// важно
                    //        break;
                    //    case "xoz":
                    //        numStartX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numStartY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numEndX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numEndY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;

                    //        numStartX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numStartY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numEndX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                    //        numEndY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;

                    //        numStartX.Value = (decimal)(line.StartPoint.X - ZERO_POINT_DIFFERENCE_X); // важно
                    //        numStartY.Value = (decimal)(line.StartPoint.Y);// важно
                    //        numEndX.Value = (decimal)(line.EndPoint.X);// важно
                    //        numEndY.Value = (decimal)(line.EndPoint.Y - ZERO_POINT_DIFFERENCE_X);// важно
                    //        break;
                    //    default:
                    //        break;
                    //}
                }

                
                numThickness.Value = (decimal)line.Thickness;
                if (line is LineElement3D line3D)
                {
                    //ResetSceneToDrawingPlane();
                    //lblEquation.Text = $"Уравнение прямой 3D " + line3D.GetCanonicalEquation3D();
                    lblEquation.Text = "Уравнение прямой 3D";
                    lblEquation3D.Visible = true;
                    lblEquation2D.Visible = false;
                    lblEquation3D.Text = line3D.GetCanonicalEquation3D();

                    //старое - не реальное
                    //numStartZ.Value = (decimal)line3D.StartPoint3D.Z;
                    //numEndZ.Value = (decimal)line3D.EndPoint3D.Z;
                    // Используем реальные координаты
                    var realStart = line3D.GetRealStartPoint();
                    var realEnd = line3D.GetRealEndPoint();

                    //numStartX.Value = (decimal)(realStart.X - ZERO_POINT_DIFFERENCE_X);
                    //numStartY.Value = (decimal)(realStart.Y - ZERO_POINT_DIFFERENCE_Y);
                    numStartZ.Value = (decimal)realStart.Z;
                    //numEndX.Value = (decimal)(realEnd.X - ZERO_POINT_DIFFERENCE_X);
                    //numEndY.Value = (decimal)(realEnd.Y - ZERO_POINT_DIFFERENCE_Y);
                    numEndZ.Value = (decimal)realEnd.Z;

                    switch (currentAxeName)
                    {
                        case "xoy":
                            numStartX.Value = (decimal)(realStart.X); // важно
                            numStartY.Value = (decimal)(realStart.Y);// важно
                            numEndX.Value = (decimal)(realEnd.X);// важно
                            numEndY.Value = (decimal)(realEnd.Y);// важно
                            numStartZ.Value = (decimal)realStart.Z; //важно
                            numEndZ.Value = (decimal)realEnd.Z; //важно

                            //numStartX.Value = (decimal)(realStart.X - ZERO_POINT_DIFFERENCE_X); // важно
                            //numStartY.Value = (decimal)(realStart.Y - ZERO_POINT_DIFFERENCE_Y);// важно
                            //numEndX.Value = (decimal)(realEnd.X - ZERO_POINT_DIFFERENCE_X);// важно
                            //numEndY.Value = (decimal)(realEnd.Y - ZERO_POINT_DIFFERENCE_Y);// важно
                            //numStartZ.Value = (decimal)realStart.Z; //важно
                            //numEndZ.Value = (decimal)realEnd.Z; //важно

                            //numStartX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                            //numStartY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_Y;
                            //numEndX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                            //numEndY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_Y;
                            //numStartX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                            //numStartY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_Y;
                            //numEndX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                            //numEndY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_Y;
                            break;
                        case "yoz":
                            //numStartX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                            //numStartY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                            //numEndX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                            //numEndY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;

                            //numStartX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                            //numStartY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                            //numEndX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                            //numEndY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;

                            numStartX.Value = (decimal)(realStart.X); // важно
                            numStartY.Value = (decimal)(realStart.Y);// важно
                            numEndX.Value = (decimal)(realEnd.X);// важно
                            numEndY.Value = (decimal)(realEnd.Y );// важно
                            numStartZ.Value = (decimal)(realStart.Z ); //важно
                            numEndZ.Value = (decimal)(realEnd.Z); //важно

                            //numStartX.Value = (decimal)(realStart.X); // важно
                            //numStartY.Value = (decimal)(realStart.Y - ZERO_POINT_DIFFERENCE_X);// важно
                            //numEndX.Value = (decimal)(realEnd.X);// важно
                            //numEndY.Value = (decimal)(realEnd.Y - ZERO_POINT_DIFFERENCE_X);// важно
                            //numStartZ.Value = (decimal)(realStart.Z - ZERO_POINT_DIFFERENCE_Y); //важно
                            //numEndZ.Value = (decimal)(realEnd.Z - ZERO_POINT_DIFFERENCE_Y); //важно                            
                            break;
                        case "xoz":
                            //numStartX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                            //numStartY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                            //numEndX.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;
                            //numEndY.Maximum = (decimal)ZERO_POINT_DIFFERENCE_X;

                            //numStartX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                            //numStartY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                            //numEndX.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;
                            //numEndY.Minimum = -(decimal)ZERO_POINT_DIFFERENCE_X;

                            //numStartX.Value = (decimal)(realStart.X - ZERO_POINT_DIFFERENCE_X); // важно
                            //numStartY.Value = (decimal)(realStart.Y);// важно
                            //numEndX.Value = (decimal)(realEnd.X - ZERO_POINT_DIFFERENCE_X);// важно
                            //numEndY.Value = (decimal)(realEnd.Y);// важно
                            //numStartZ.Value = (decimal)(realStart.Z - ZERO_POINT_DIFFERENCE_Y); //важно
                            //numEndZ.Value = (decimal)(realEnd.Z - ZERO_POINT_DIFFERENCE_Y); //важно

                            numStartX.Value = (decimal)(realStart.X); // важно
                            numStartY.Value = (decimal)(realStart.Y);// важно
                            numEndX.Value = (decimal)(realEnd.X);// важно
                            numEndY.Value = (decimal)(realEnd.Y);// важно
                            numStartZ.Value = (decimal)(realStart.Z); //важно
                            numEndZ.Value = (decimal)(realEnd.Z); //важно
                            break;
                        default:
                            break;
                    }

                    lblStartZ.Visible = true;
                    lblEndZ.Visible = true;
                    numStartZ.Visible = true;
                    numEndZ.Visible = true;
                    //line3D.Rotate3DWithScene(CalculateRealSceneCenter(), resetAngleValueX,resetAngleValueY, resetAngleValueZ);
                    //if(totalRotationX!=0 || totalRotationY!=0 || totalRotationZ != 0) 
                    //{
                    //    line3D.Rotate3DWithScene(CalculateRealSceneCenter(), totalRotationX, totalRotationY, totalRotationZ);
                    //}
                }
                else 
                {
                    lblStartZ.Visible = false;
                    lblEndZ.Visible = false;
                    numStartZ.Visible = false;
                    numEndZ.Visible = false;
                    lblEquation.Visible = true;
                    lblEquation3D.Visible = false;
                }

                // Восстанавливаем обработчики
                numStartX.ValueChanged += Parameters_ValueChanged;
                numStartY.ValueChanged += Parameters_ValueChanged;
                numEndX.ValueChanged += Parameters_ValueChanged;
                numEndY.ValueChanged += Parameters_ValueChanged;
                numThickness.ValueChanged += Parameters_ValueChanged;
                numStartZ.ValueChanged += Parameters_ValueChanged;
                numEndZ.ValueChanged += Parameters_ValueChanged;

                comboColor.SelectedItem = line.Color.Name;
            }

            if (selectedFigure is Cube3D cube) 
            {
                //ResetSceneToDrawingPlane();
                parametersPanel.Visible = true;
                UpdateParametersPanelPosition(); // Обновляем позицию

                // Обновляем максимальные значения в соответствии с текущей областью рисования
                var drawingArea = GetDrawingArea();
                //numStartX.Maximum = drawingArea.Width/2;
                //numStartY.Maximum = drawingArea.Height/2;
                //numStartX.Minimum = -drawingArea.Width / 2;
                //numStartY.Minimum = -drawingArea.Height / 2;

                lblCubeCenterPoint.Visible = true;
                lblEquation.Visible = false;
                lblStartPoint.Visible = false;
                lblEndPoint.Visible = false;

                numStartZ.Visible = true;
                numEndX.Visible = false;
                numEndY.Visible = false;
                numThickness.Visible = false;
                lblThickness.Visible = false; lblColor.Visible = false;

                lblStartZ.Visible = true;
                lblEndZ.Visible = false;
                numEndZ.Visible = false;
                
                lblEndX.Visible = false; lblEndY.Visible = false;
                comboColor.Visible = false;



                // Временно отключаем события чтобы избежать рекурсии
                numStartX.ValueChanged -= Parameters_ValueChanged;
                numStartY.ValueChanged -= Parameters_ValueChanged;
                numStartZ.ValueChanged -= Parameters_ValueChanged;

                if ((decimal)cube.Center.X - numStartX.Value > 100) 
                {
                    ;
                }
                
                switch (currentAxeName)
                {
                    case "xoy":
                        numStartX.Value = (decimal)(cube.Center.X ); // важно               
                        numStartY.Value = (decimal)(cube.Center.Y); // важно
                        numStartZ.Value = (decimal)cube.Center.Z;
                        break;
                    case "yoz":
                        numStartX.Value = (decimal)(cube.Center.X); // важно               
                        numStartY.Value = (decimal)(cube.Center.Y); // важно
                        numStartZ.Value = (decimal)(cube.Center.Z);                        
                        break;
                    case "xoz":
                        numStartX.Value = (decimal)(cube.Center.X); // важно               
                        numStartY.Value = (decimal)(cube.Center.Y); // важно
                        numStartZ.Value = (decimal)(cube.Center.Z);
                        break;
                    default:

                        break;
                }


                numStartX.ValueChanged += Parameters_ValueChanged;
                numStartY.ValueChanged += Parameters_ValueChanged;
                numStartZ.ValueChanged += Parameters_ValueChanged;

                //cube.Rotate3DWithScene(resetAngleValueX, resetAngleValueY, resetAngleValueZ, CalculateRealSceneCenter());
                //if (totalRotationX != 0 || totalRotationY != 0 || totalRotationZ != 0)
                //{
                //    cube.Rotate3DWithScene(totalRotationX, totalRotationY, totalRotationZ, CalculateRealSceneCenter());
                //}
            }
        }

        private void Parameters_ValueChanged(object sender, EventArgs e)
        {
            if (selectedFigure is LineElement line && !isDragging && !isResizing)
            {
                //тут флаг если не 3 д, то 2 мерный поинт иначе - 3 мерный
                //а может и без флага просто брать это преропредленно е и все,
                //а из получиь уравнение для 2 д линииивообще убрать z
                if (!(line is LineElement3D))
                {
                    switch (currentAxeName)
                    {
                        case "xoy":
                            line.StartPoint = new PointF((float)numStartX.Value, (float)numStartY.Value); // важно
                            line.EndPoint = new PointF((float)numEndX.Value, (float)numEndY.Value);  // выжно
                            //line.StartPoint = new PointF((float)numStartX.Value + ZERO_POINT_DIFFERENCE_X, (float)numStartY.Value + ZERO_POINT_DIFFERENCE_Y); // важно
                            //line.EndPoint = new PointF((float)numEndX.Value + ZERO_POINT_DIFFERENCE_X, (float)numEndY.Value + ZERO_POINT_DIFFERENCE_Y);  // выжно
                            break;
                        case "yoz":
                            line.StartPoint = new PointF((float)numStartX.Value, (float)numStartY.Value); // важно
                            line.EndPoint = new PointF((float)numEndX.Value, (float)numEndY.Value);  // выжно
                            //line.StartPoint = new PointF((float)numStartX.Value, (float)numStartY.Value + ZERO_POINT_DIFFERENCE_X); // важно
                            //line.EndPoint = new PointF((float)numEndX.Value, (float)numEndY.Value + ZERO_POINT_DIFFERENCE_X);  // выжно
                            break;
                        case "xoz":
                            line.StartPoint = new PointF((float)numStartX.Value, (float)numStartY.Value); // важно
                            line.EndPoint = new PointF((float)numEndX.Value, (float)numEndY.Value);  // выжно
                            //line.StartPoint = new PointF((float)numStartX.Value + ZERO_POINT_DIFFERENCE_X, (float)numStartY.Value); // важно
                            //line.EndPoint = new PointF((float)numEndX.Value + ZERO_POINT_DIFFERENCE_X, (float)numEndY.Value);  // выжно
                            break;
                        default:
                            break;
                    }                    
                }

                line.Thickness = (float)numThickness.Value;
                //(float a, float b, float c, float d) = line.GetEquation();
                //lblEquation.Text = $"Уравнение прямой {a}x+{b}y+{c}=0"; //для 3 д тут z использовать еще
                if(line is LineElement3D line3D) 
                {
                    //line3D.ChangeZ((float)numStartZ.Value - line3D.StartPoint3D.Z, (float)numEndZ.Value- line3D.EndPoint3D.Z); //пооходу ту проблема была в скачках линий
                    //line3D.StartPoint3D = new Point3D(line3D.StartPoint3D.X, line3D.StartPoint3D.Y, (float)numStartZ.Value);
                    //line3D.EndPoint3D = new Point3D(line3D.EndPoint3D.X, line3D.EndPoint3D.Y, (float)numEndZ.Value);
                    //line3D.SetStartZ(GetDrawingArea(), groupManager, (float)numStartZ.Value);
                    //line3D.SetEndZ(GetDrawingArea(), groupManager, (float)numEndZ.Value);
                }
                this.Invalidate();
            }

            if(selectedFigure is LineElement3D line3d && !isDragging && !isResizing) 
            {
                ResetSceneToDrawingPlane();
                //старое не реальное
                //line3d.StartPoint3D = new Point3D((float)numStartX.Value, (float)numStartY.Value, (float)numStartZ.Value);
                //line3d.EndPoint3D = new Point3D((float)numEndX.Value, (float)numEndY.Value, (float)numEndZ.Value);
                // Устанавливаем реальные координаты
                Point3D newStart;
                Point3D newEnd;
                switch (currentAxeName)
                {
                    case "xoy":
                        newStart = new Point3D((float)numStartX.Value, (float)numStartY.Value, (float)numStartZ.Value);
                        newEnd = new Point3D((float)numEndX.Value, (float)numEndY.Value, (float)numEndZ.Value);
                        //newStart = new Point3D((float)numStartX.Value + ZERO_POINT_DIFFERENCE_X, (float)numStartY.Value + ZERO_POINT_DIFFERENCE_Y, (float)numStartZ.Value);
                        //newEnd = new Point3D((float)numEndX.Value + ZERO_POINT_DIFFERENCE_X, (float)numEndY.Value + ZERO_POINT_DIFFERENCE_Y, (float)numEndZ.Value);
                        break;
                    case "yoz":
                        newStart = new Point3D((float)numStartX.Value, (float)numStartY.Value, (float)numStartZ.Value);
                        newEnd = new Point3D((float)numEndX.Value, (float)numEndY.Value, (float)numEndZ.Value);
                        //newStart = new Point3D((float)numStartX.Value, (float)numStartY.Value + ZERO_POINT_DIFFERENCE_X, (float)numStartZ.Value + ZERO_POINT_DIFFERENCE_Y);
                        //newEnd = new Point3D((float)numEndX.Value, (float)numEndY.Value + ZERO_POINT_DIFFERENCE_X, (float)numEndZ.Value + ZERO_POINT_DIFFERENCE_Y);
                        break;
                    case "xoz":
                        newStart = new Point3D((float)numStartX.Value, (float)numStartY.Value, (float)numStartZ.Value);
                        newEnd = new Point3D((float)numEndX.Value, (float)numEndY.Value, (float)numEndZ.Value);
                        //newStart = new Point3D((float)numStartX.Value + ZERO_POINT_DIFFERENCE_X, (float)numStartY.Value, (float)numStartZ.Value + ZERO_POINT_DIFFERENCE_Y);
                        //newEnd = new Point3D((float)numEndX.Value + ZERO_POINT_DIFFERENCE_X, (float)numEndY.Value , (float)numEndZ.Value + ZERO_POINT_DIFFERENCE_Y);
                        break;
                    default:
                        newStart = null;
                        newEnd = null;
                        break;
                }                

                line3d.SetRealPoints(newStart, newEnd, CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC); //ВНИМАНИЕ ВАЖНО
                line3d.Thickness = (float)numThickness.Value;
                line3d.Rotate3DWithScene(CalculateRealSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                if (totalRotationX != 0 || totalRotationY != 0 || totalRotationZ != 0)
                {
                    line3d.Rotate3DWithScene(CalculateRealSceneCenter(), totalRotationX, totalRotationY, totalRotationZ, ZC, currentAxeName);
                }
                this.Invalidate();
            }

            if(selectedFigure is Cube3D cube && !isDragging && !isResizing) 
            {
                ResetSceneToDrawingPlane();
                cube.Rotate3DWithScene(-totalRotationX, -totalRotationY, -totalRotationZ, CalculateSceneCenter(), ZC, currentAxeName);
                cube.Rotate3DWithScene(resetAngleValueX, resetAngleValueY, resetAngleValueZ, CalculateSceneCenter(), ZC, currentAxeName);
                cube.SetMovingCenter(new PointF((float)numStartX.Value - cube.Center.X, (float)numStartY.Value - cube.Center.Y), ZERO_POINT_DIFFERENCE_Y, ZERO_POINT_DIFFERENCE_X, (float)numStartZ.Value - cube.Center.Z, currentAxeName);
                //switch (currentAxeName)
                //{
                //    case "xoy":
                //        cube.SetMovingCenter(new PointF((float)numStartX.Value - cube.Center.X, (float)numStartY.Value - cube.Center.Y), ZERO_POINT_DIFFERENCE_Y, ZERO_POINT_DIFFERENCE_X, (float)numStartZ.Value - cube.Center.Z, currentAxeName);
                //        break;
                //    case "yoz":
                //        cube.Center = new Point3D((float)numStartY.Value, (float)numStartZ.Value /*+ ZERO_POINT_DIFFERENCE_X*/, (float)numStartX.Value /*+ ZERO_POINT_DIFFERENCE_Y*/);
                //        break;
                //    case "xoz":
                //        cube.Center = new Point3D((float)numStartX.Value /*+ ZERO_POINT_DIFFERENCE_X*/, (float)numStartZ.Value, (float)numStartY.Value /*+ ZERO_POINT_DIFFERENCE_Y*/);
                //        break;
                //    default:
                        
                //        break;
                //}
                //cube.Rotate3DWithScene(resetAngleValueX, resetAngleValueY, resetAngleValueZ, CalculateRealSceneCenter());
                //if (totalRotationX != 0 || totalRotationY != 0 || totalRotationZ != 0)
                //{
                //    cube.Rotate3DWithScene(totalRotationX, totalRotationY, totalRotationZ, CalculateRealSceneCenter());
                //}
                cube.Rotate3DWithScene(-totalRotationX, -totalRotationY, -totalRotationZ, CalculateSceneCenter(), ZC, currentAxeName);
                this.Invalidate();
            }
        }
        //колесиком чинить можно - сейчас оно не реагирует, а так можно подключить - ВНИМАНИЕ 
        //    на то каакая плоскость выбрана - в какой-от онужно увеличивать Z, а в какой-т одругую координату
        //    теперь у куба проецирование гавно
        private void ComboColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedFigure is LineElement line)
            {
                Color newColor = Color.Black;

                switch (comboColor.SelectedItem.ToString())
                {
                    case "Черный": newColor = Color.Black; break;
                    case "Красный": newColor = Color.Red; break;
                    case "Зеленый": newColor = Color.Green; break;
                    case "Синий": newColor = Color.Blue; break;
                    case "Оранжевый": newColor = Color.Orange; break;
                    case "Фиолетовый": newColor = Color.Purple; break;
                    case "Желтый": newColor = Color.Yellow; break;
                    case "Розовый": newColor = Color.Pink; break;
                    case "Коричневый": newColor = Color.Brown; break;
                    case "Серый": newColor = Color.Gray; break;
                }

                line.Color = newColor;
                this.Invalidate();
            }
            //if (selectedFigure is LineElement line)
            //{

            //    line.Color = Color.FromName(comboColor.SelectedItem.ToString());
            //    this.Invalidate();
            //}
        }

        private void InitializeStatusBar()
        {
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);

            this.Controls.Add(statusStrip);
            UpdateStatusBar();
        }

        // Обработчики меню
        private void Mode2DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Set2DMode();
        }

        private void Mode3DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Set3DMode();
        }

        private void Set2DMode()
        {
            is3DMode = false;
            UpdateModeVisuals();

            // Снимаем выделение с 3D фигур при переходе в 2D режим
            foreach (var figure in figures.Where(f => f is LineElement3D))
            {
                figure.IsSelected = false;
            }
            selectedFigures.RemoveAll(f => f is LineElement3D);

            this.Invalidate();
        }

        private void Set3DMode()
        {
            is3DMode = true;
            UpdateModeVisuals();

            // Снимаем выделение с 2D фигур при переходе в 3D режим
            foreach (var figure in figures.Where(f => !(f is LineElement3D)))
            {
                figure.IsSelected = false;
            }
            selectedFigures.RemoveAll(f => !(f is LineElement3D));

            this.Invalidate();
        }

        private void UpdateModeVisuals()
        {
            // Обновляем галочки в меню
            mode2DToolStripMenuItem.Checked = !is3DMode;
            mode3DToolStripMenuItem.Checked = is3DMode;

            // Обновляем заголовок окна
            this.Text = $"Графический редактор - {(is3DMode ? "3D" : "2D")} Режим";

            // Обновляем строку состояния
            UpdateStatusBar();

            // Обновляем панель параметров
            UpdateParametersPanel();
            //UpdateModeDependentControls();
        }

        private void UpdateStatusBar()
        {
            if (statusLabel != null)
            {
                string modeText = is3DMode ? "3D Режим" : "2D Режим";
                string hintText = is3DMode ? " | Ctrl+Колесо: изменить Z" : "";
                statusLabel.Text = $"{modeText}{hintText}";
            }
        }

        private void СomboOperation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedFigure is LineElement line || selectedFigure is Cube3D cube)
            {
                switch (comboOperation.SelectedItem.ToString()) 
                {
                    case "Смещение":
                        ApplyTransfer();
                        //месседж боксом получить на сколько по x по y
                        break;
                    case "Вращение":
                        ApplayRotation();
                        //месседж боксом получить угол
                        break;
                    case "Масштабирование":
                        ApplyScaling();
                        //месседж боксом получить на сколько по x по y
                        break;
                    case "Общее масштабирование":
                        ApplyUniformScaling();
                        //месседж боксом получить s
                        break;
                    case "Зеркалирование":
                        ApplyMirroring();
                        //месседж боксом уведомить выбрать линию относительно котоорой будет зеркалирование
                        break;
                    case "Ортографическое проецирование":                        
                        ApplyProjection();
                        //месседж боксом получить на какую ось - тЧТО КОШМАР ВОБЩЕМ
                        break;
                    case "Перспективное проецирование":
                        ApplayProjectioinPerspective();
                        break;
                }
                //масштабирование и вращение могут выкинуть линии за холст - исправить как делали с перемещением
                //UpdateParametersPanel(); //проследить чтобы обновлялось как надо!!!1 координаты выбраной линии после операции
                //либо вензде внурти этих метдв это напихать
                //вроде норм обновляется - проверял чуть 
                //this.Invalidate(); внутри методов делается, чтобы не после подтверждения сообщения отображжался результат
            }
        }

        private void ApplayProjectioinPerspective()
        {
            if (selectedFigure is Cube3D || selectedFigure is LineElement3D)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите расстояние, на котором расположен глаз наблюдателя (Zc):",
                "Перспективное проецирование", "500");
                if (!string.IsNullOrEmpty(input))
                {                   
                    try
                    {
                        float Zc = 0;
                        if (!float.TryParse(input, out Zc))
                        {
                            throw new ArgumentException("Неправильный ввод");
                        }
                        
                        if (selectedFigure is Cube3D cube) 
                        {
                            cube._zc = Zc;
                            figures.Add(cube.TcX);
                            figures.Add(cube.TcY);
                            figures.Add(cube.TcZ);
                            cube.Rotate3DWithScene(0, 0, 0, new Point3D(0, 0, 0), 0, currentAxeName);
                            //PointElement3D TcX = new PointElement3D(,,,);
                        }
                        else 
                        {
                            if (selectedFigure is LineElement3D line3D) 
                            {
                                line3D._zc = Zc;
                                line3D.Rotate3DWithScene(new Point3D(0, 0, 0), 0, 0, 0, 0, currentAxeName);
                            }
                        }
                            string axis = input.ToLower();
                        foreach (var figure in selectedFigures)
                        {
                            figure.Projection(axis);
                        }
                        UpdateParametersPanel();
                        this.Invalidate();
                        MessageBox.Show($"Проецирование применено: ось {axis}", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите число", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool CanPerformRotation(float angle)
        {
            return true;
            throw new NotImplementedException();
            var drawingArea = GetDrawingArea();

            foreach (var figure in selectedFigures)
            {
                if (figure is LineElement line)
                {
                    // Создаем копию линии для проверки
                    var testLine = new LineElement(line.StartPoint, line.EndPoint, line.Color, line.Thickness);
                    testLine.Rotate(angle);

                    if (testLine.EndPoint.X < 0 || testLine.EndPoint.X > drawingArea.Width 
                        || testLine.EndPoint.Y < 0 || testLine.EndPoint.Y > drawingArea.Height 
                        || testLine.StartPoint.X < 0 || testLine.StartPoint.X > drawingArea.Width
                        || testLine.StartPoint.Y < 0 || testLine.StartPoint.Y > drawingArea.Width)
                        return false;
                }
            }
            return true;
        }

        private /*(bool, List<FigureElement>)*/bool CanPerformScaling(float scaleX, float scaleY, float scaleZ = 1)
        {
            return true;
            throw new NotImplementedException();
            var drawingArea = GetDrawingArea();
            //List<FigureElement> 

            foreach (var figure in selectedFigures)
            {
                if (figure is LineElement3D line3d) 
                {
                    // Создаем копию линии для проверки
                    var testLine = new LineElement3D(line3d.ZeroRatatedStartPoint, line3d.ZeroRatatedEndPoint, line3d.Color, line3d.Thickness);

                    Point3D center = new Point3D(
                        (testLine.ZeroRatatedStartPoint.X + testLine.ZeroRatatedEndPoint.X) / 2,
                        (testLine.ZeroRatatedStartPoint.Y + testLine.ZeroRatatedEndPoint.Y) / 2,
                        (testLine.ZeroRatatedStartPoint.Z + testLine.ZeroRatatedEndPoint.Z) / 2
                    );

                    testLine.Scale(center, scaleX, scaleY, scaleZ);
                    testLine.Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);

                    if (testLine.EndPoint.X < -drawingArea.Width / 2 || testLine.EndPoint.X > drawingArea.Width / 2
                        || testLine.EndPoint3D.Y < -drawingArea.Height / 2 || testLine.EndPoint3D.Y > drawingArea.Height / 2
                        || testLine.StartPoint.X < -drawingArea.Width / 2 || testLine.StartPoint.X > drawingArea.Width / 2
                        || testLine.StartPoint.Y < -drawingArea.Height / 2 || testLine.StartPoint.Y > drawingArea.Height / 2)
                        return false;
                        //return (false, null);
                    //return (true, testLine);
                }
                else
                {
                    if (figure is Cube3D cube) 
                    {
                        // Создаем копию линии для проверки
                        var testCube = new Cube3D(cube.Center, cube.Size, cube.Color, currentAxeName, ZC);

                        testCube.Scale(cube.Center, scaleX, scaleY, scaleZ);
                        testCube.Rotate3DWithScene(resetAngleValueX, resetAngleValueY, resetAngleValueZ, CalculateSceneCenter(), ZC, currentAxeName);

                        if (testCube.Center.X < -drawingArea.Width / 2 || testCube.Center.X > drawingArea.Width / 2 || 
                            testCube.Center.Y > drawingArea.Height / 2 || testCube.Center.Y < -drawingArea.Height / 2
                            || testCube.Center.Z > 10000) //нахрен убрать
                            return false;
                            //return (false, null);
                        //return (true, testCube);
                    }
                    else
                    {
                        if (figure is LineElement line)
                        {
                            // Создаем копию линии для проверки
                            var testLine = new LineElement(line.StartPoint, line.EndPoint, line.Color, line.Thickness);

                            PointF center = new PointF(
                                (testLine.StartPoint.X + testLine.EndPoint.X) / 2,
                                (testLine.StartPoint.Y + testLine.EndPoint.Y) / 2
                            );

                            testLine.Scale(center, scaleX, scaleY);

                            if (testLine.EndPoint.X < -drawingArea.Width / 2 || testLine.EndPoint.X > drawingArea.Width / 2
                                || testLine.EndPoint.Y < -drawingArea.Height / 2 || testLine.EndPoint.Y > drawingArea.Height / 2
                                || testLine.StartPoint.X < -drawingArea.Width / 2 || testLine.StartPoint.X > drawingArea.Width/2
                                || testLine.StartPoint.Y < -drawingArea.Height / 2 || testLine.StartPoint.Y > drawingArea.Height/2)
                                return false;
                                //return (false, null);
                            //return (true, testLine);
                        }
                    }
                }
            }
            return true;
            //return (false, null);
        }

        private bool CanPerformMirror(float A, float B, float C)
        {
            var drawingArea = GetDrawingArea();

            //foreach (var figure in selectedFigures)
            //{
                if (selectedFigures[0] is LineElement line)
                {
                    // Создаем копию линии для проверки
                    var testLine = new LineElement(line.StartPoint, line.EndPoint, line.Color, line.Thickness);
                    // Зеркалируем точки линии для проверки
                    testLine.Mirror(A, B, C);

                    // Проверяем, остаются ли точки в пределах области рисования
                    if (!drawingArea.Contains(Point.Round(testLine.StartPoint)) ||
                        !drawingArea.Contains(Point.Round(testLine.EndPoint)))
                    {
                        return false;
                    }
                }
            //}

            return true;
        }


        private void ApplyTransfer()
        {
            if (selectedFigure.Is3D) 
            {
                ApplyTransfer3D();
            }
            else
            {
                ApplyTransfer2D();
            }                
        }

        private void ApplyTransfer3D()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
        "Введите смещение по X, Y и Z через запятую\n(например: 10, -5, 20)\nВ качестве разделителя для дробных чисел\nиспользуйте точку:",
        "3D Смещение", "0, 0, 0");

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    string[] parts = input.Split(',');
                    if (parts.Length != 3)
                        throw new ArgumentException("Необходимо ввести 3 значения: X, Y, Z");

                    float dx = float.Parse(parts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    float dy = float.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    float dz = float.Parse(parts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                    Point3D delta = new Point3D(dx, dy, dz);

                    bool any3DObject = false;
                    var drawingArea = GetDrawingArea();

                    if (isGroupSelected && currentGroupId != null)
                    {
                        // Для групп применяем смещение ко всем элементам
                        var groupElements = groupManager.GetGroupElements(currentGroupId);
                        foreach (var figure in groupElements)
                        {
                            if (figure is LineElement3D line3D)
                            {
                                //line3D.Move3D(delta); это плохой метод
                                line3D.Move(delta.ToPoint2D(), ZERO_POINT_DIFFERENCE_Y, ZERO_POINT_DIFFERENCE_X, dz, currentAxeName);
                                any3DObject = true;
                            }
                            else if (figure is Cube3D cube)
                            {
                                //cube.Move3D(delta);
                                cube.Move3D(delta.ToPoint2D(), ZERO_POINT_DIFFERENCE_Y, ZERO_POINT_DIFFERENCE_X, dz, currentAxeName);
                                any3DObject = true;
                            }
                            else
                            {
                                // Для 2D объектов используем только X,Y
                                figure.Move(new PointF(dx, dy), drawingArea.Height, drawingArea.Width, 0, currentAxeName);
                            }
                        }
                    }
                    else
                    {
                        // Для отдельных фигур
                        foreach (var figure in selectedFigures)
                        {
                            if (figure is LineElement3D line3D)
                            {
                                //line3D.Move3D(delta);
                                line3D.Move(delta.ToPoint2D(), ZERO_POINT_DIFFERENCE_Y, ZERO_POINT_DIFFERENCE_X, dz, currentAxeName);
                                any3DObject = true;
                            }
                            else if (figure is Cube3D cube)
                            {
                                //cube.Move3D(delta); пллохой метод при не плоскости xoy
                                //cube.Move(delta.ToPoint2D(), ZERO_POINT_DIFFERENCE_Y, ZERO_POINT_DIFFERENCE_X, dz, currentAxeName);
                                cube.Move3D(delta.ToPoint2D(), ZERO_POINT_DIFFERENCE_Y, ZERO_POINT_DIFFERENCE_X, dz, currentAxeName);
                                any3DObject = true;
                            }
                            else
                            {
                                // Для 2D объектов используем только X,Y
                                figure.Move(new PointF(dx, dy), drawingArea.Height, drawingArea.Width, 0,currentAxeName);
                            }
                        }
                    }

                    if (!any3DObject)
                    {
                        MessageBox.Show("Смещение применено только по X и Y (выбраны только 2D объекты)",
                                      "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    UpdateParametersPanel();
                    this.Invalidate();
                    MessageBox.Show($"3D смещение применено: X={dx}, Y={dy}, Z={dz}", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите числа в формате: 10, -5, 20", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ApplyTransfer2D() 
        {
            // Ограничиваем область рисования
            var drawingArea = GetDrawingArea();
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите смещение по X и Y через запятую\n(например: 10, -5.)\nВ качестве разделителя для дробных чисел\nиспользуйте точку:",
                "Смещение", "0, 0");

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    string[] parts = input.Split(',');
                    // Используем CultureInfo.InvariantCulture для корректного парсинга дробных чисел
                    float dx = float.Parse(parts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    float dy = float.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                    PointF delta = new PointF(dx, dy);

                    if (isGroupSelected && currentGroupId != null)
                    {
                        groupManager.MoveGroup(currentGroupId, delta, drawingArea.Height, drawingArea.Width, 0, currentAxeName);
                    }
                    else
                    {
                        foreach (var figure in selectedFigures)
                        {
                            figure.Move(delta, drawingArea.Height, drawingArea.Width, 0, currentAxeName);
                        }
                    }
                    UpdateParametersPanel();
                    this.Invalidate();
                    UpdateParametersPanel();
                    MessageBox.Show($"Смещение применено: X={dx}, Y={dy}", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите числа в формате: 10, -5", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ApplayRotation() 
        {
            if (selectedFigure.Is3D)
            {
                ApplayRotation3D();
            }
            else
            {
                ApplayRotation2D();
            }
        }

        private void ApplayRotation3D()
        {
            string byZeroOrLineCenter = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите относительно чего будет вращаться объект: своего центра или начала координат\n(0 - вокруг своего центра, " +
                "1 - начала координат)",
        "3D Вращение", "0");
            if (!string.IsNullOrEmpty(byZeroOrLineCenter))
            {
                int byWhatRotate = -1;
                if (!int.TryParse(byZeroOrLineCenter, out byWhatRotate))
                {
                    throw new ArgumentException("Необходимо ввыбрать относительно чего вращать объект\n(0 - вокруг своего центра, 1 - начала координат)");
                }
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите углы вращения вокруг осей X, Y, Z через запятую в градусах\n(например: 0, 45, 0 для вращения вокруг Y):",
                "3D Вращение", "0, 0, 0");

                if (!string.IsNullOrEmpty(input))
                {
                    try
                    {
                        string[] parts = input.Split(',');
                        if (parts.Length != 3)
                            throw new ArgumentException("Необходимо ввести 3 значения: угол_X, угол_Y, угол_Z");

                        float angleX = float.Parse(parts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        float angleY = float.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        float angleZ = float.Parse(parts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                        bool any3DObject = false;

                        if (isGroupSelected && currentGroupId != null) //черт
                        {
                            // Для групп - вращаем вокруг центра группы
                            var groupElements = groupManager.GetGroupElements(currentGroupId);
                            Point3D groupCenter;
                            if (byWhatRotate == 0)
                                groupCenter = groupManager.GetGroupCenter3D(selectedFigure.GroupId);
                            else
                                groupCenter = CalculateSceneCenter();

                            foreach (var figure in groupElements)
                            {
                                if (figure is LineElement3D line3D)
                                {
                                    line3D.Rotate3D(groupCenter, angleX, angleY, angleZ, ZC);
                                    any3DObject = true;
                                    line3D.Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                                }
                                else if (figure is Cube3D cube)
                                {
                                    cube.Rotate3D(angleX, angleY, angleZ, CalculateSceneCenter(), GetDrawingArea(), ZC);
                                    any3DObject = true;
                                    cube.Rotate3DWithScene(resetAngleValueX, resetAngleValueY, resetAngleValueZ, CalculateSceneCenter(), ZC, currentAxeName);
                                }
                                else
                                {
                                    // Для 2D объектов - только вращение вокруг Z
                                    figure.Rotate(angleZ, groupCenter.ToPoint2D());
                                }
                            }
                        }
                        else
                        {
                            // Для отдельных фигур
                            foreach (var figure in selectedFigures)
                            {
                                if (figure is LineElement3D line3D)
                                {
                                    Point3D lineCenter;
                                    if (byWhatRotate == 0)
                                    // Для линии - вращаем вокруг ее центра
                                    {
                                        lineCenter = new Point3D(
                                        (line3D.StartPoint3D.X + line3D.EndPoint3D.X) / 2,
                                        (line3D.StartPoint3D.Y + line3D.EndPoint3D.Y) / 2,
                                        (line3D.StartPoint3D.Z + line3D.EndPoint3D.Z) / 2
                                    );
                                    }
                                    else
                                    {
                                        lineCenter = CalculateSceneCenter();
                                    }
                                    line3D.Rotate3D(lineCenter, angleX, angleY, angleZ, ZC);
                                    any3DObject = true;
                                    line3D.Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                                }
                                else if (figure is Cube3D cube)
                                {
                                    Point3D cubeCenter;
                                    if (byWhatRotate == 0)
                                    {
                                        cubeCenter = cube.Center;
                                    }
                                    else
                                    {
                                        cubeCenter = CalculateSceneCenter();
                                    }
                                    cube.Rotate3D(angleX, angleY, angleZ, cubeCenter, GetDrawingArea(), ZC);
                                    any3DObject = true;
                                    cube.Rotate3DWithScene(resetAngleValueX, resetAngleValueY, resetAngleValueZ, CalculateSceneCenter(), ZC, currentAxeName);
                                }
                                else
                                {
                                    // Для 2D объектов - только вращение вокруг Z
                                    if (byWhatRotate == 0)
                                        figure.Rotate(angleZ);
                                    else
                                        figure.Rotate(angleZ, CalculateSceneCenter().ToPoint2D());
                                }
                            }
                        }

                        if (!any3DObject)
                        {
                            MessageBox.Show("Вращение применено только вокруг оси Z (выбраны только 2D объекты)",
                                          "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        UpdateParametersPanel();
                        this.Invalidate();
                        MessageBox.Show($"3D вращение применено: X={angleX}°, Y={angleY}°, Z={angleZ}°", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите углы в формате: 0, 45, 0", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ApplayRotation2D()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
        "Введите угол вращения в градусах\n(положительный - по часовой):",
        "Вращение", "0");

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    float angle = float.Parse(input.Trim(), System.Globalization.CultureInfo.InvariantCulture);

                    if (isGroupSelected && currentGroupId != null)
                    {
                        if (!groupManager.RotateGroup(currentGroupId, 0, 0, angle, new Point3D(groupManager.GetGroupCenter(currentGroupId), 0), GetDrawingArea(), ZC))
                        {
                            MessageBox.Show("Ошибка: вращение выносит элементы за границы экрана", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        if (CanPerformRotation(angle))
                        {
                            foreach (var figure in selectedFigures)
                            {
                                figure.Rotate(angle);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Ошибка: вращение выносит элементы за границы экрана", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    UpdateParametersPanel();
                    this.Invalidate();
                    MessageBox.Show($"Вращение применено: {angle}°", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите число", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ApplyScaling()
        {
            if (selectedFigure.Is3D)
            {
                ApplyScaling3D();
            }
            else
            {
                ApplyScaling2D();
            }
        }

        private void ApplyScaling2D()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите масштаб по X и Y через запятую (например: 1.5, 0.8):",
                "Масштабирование", "1, 1");

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    string[] parts = input.Split(',');
                    float sx = float.Parse(parts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    float sy = float.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                    if (isGroupSelected && currentGroupId != null)
                    {
                        // Для группы применяем равномерное масштабирование по минимальному коэффициенту
                        //Почему это? Нет
                        //float uniformScale = Math.Min(sx, sy);
                        //groupManager.ScaleGroup(currentGroupId, uniformScale);
                        if (!groupManager.ScaleGroup(currentGroupId, sx, sy, GetDrawingArea(), ZC, currentAxeName))
                        {
                            MessageBox.Show("Ошибка: масштабирование выносит элементы за границы экрана", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        
                    }
                    else
                    {
                        if (CanPerformScaling(sx, sy))
                        {
                            foreach (var figure in selectedFigures)
                            {
                                // Для отдельных линий - раздельное масштабирование
                                if (figure is LineElement line)
                                {
                                    PointF center = new PointF(
                                        (line.StartPoint.X + line.EndPoint.X) / 2,
                                        (line.StartPoint.Y + line.EndPoint.Y) / 2
                                    );

                                    line.Scale(center, sx, sy);
                                }
                            }
                        }
                        else 
                        {
                            MessageBox.Show("Ошибка: масштабирование выносит элементы за границы экрана", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    UpdateParametersPanel();
                    this.Invalidate();
                    MessageBox.Show($"Масштабирование применено: X={sx}, Y={sy}", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите числа в формате: 1.5, 0.8", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ApplyScaling3D()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите масштаб по X, Y и Z через запятую (например: 1.5, 0.8, 5):",
                "Масштабирование", "1, 1, 1");

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    string[] parts = input.Split(',');
                    float sx = float.Parse(parts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    float sy = float.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    float sz = float.Parse(parts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                    if (isGroupSelected && currentGroupId != null)
                    {
                        // Для группы применяем равномерное масштабирование по минимальному коэффициенту
                        //Почему это? Нет
                        //float uniformScale = Math.Min(sx, sy);
                        //groupManager.ScaleGroup(currentGroupId, uniformScale);
                        if (!groupManager.ScaleGroup(currentGroupId, sx, sy, GetDrawingArea(), ZC, currentAxeName, sz, CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, totalRotationX, totalRotationY, totalRotationZ))
                        {
                            MessageBox.Show("Ошибка: масштабирование выносит элементы за границы экрана", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }
                    else
                    {
                        if (CanPerformScaling(sx, sy, sz) /*|| true*/)
                        {
                            foreach (var figure in selectedFigures)
                            {
                                // Для отдельных линий - раздельное масштабирование
                                if (figure is LineElement3D line3d)
                                {
                                    Point3D center = new Point3D(
                                        (line3d.ZeroRatatedStartPoint.X + line3d.ZeroRatatedEndPoint.X) / 2,
                                        (line3d.ZeroRatatedStartPoint.Y + line3d.ZeroRatatedEndPoint.Y) / 2,
                                        (line3d.ZeroRatatedStartPoint.Z + line3d.ZeroRatatedEndPoint.Z) / 2
                                    );
                                    
                                    line3d.Scale(center, sx, sy, sz);
                                    line3d.Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                                }
                                else
                                {
                                    if (figure is Cube3D cube)
                                    {
                                        cube.Scale(cube.Center, sx, sy, sz);
                                    }
                                }                                
                            }
                        }
                        else
                        {
                            MessageBox.Show("Ошибка: масштабирование выносит элементы за границы экрана", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    UpdateParametersPanel();
                    this.Invalidate();
                    MessageBox.Show($"Масштабирование применено: X={sx}, Y={sy}, Z={sz}", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите числа в формате: 1.5, 0.8, 5", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ApplyUniformScaling()
        {
            if (selectedFigure.Is3D)
            {
                ApplyUniformScaling3D();
            }
            else
            {
                ApplyUniformScaling2D();
            }
        }

        private void ApplyUniformScaling2D()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите коэффициент масштабирования (например: 1.5 для увеличения в 1.5 раза):",
                "Общее масштабирование", "1");

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    float scale = float.Parse(input.Trim(), System.Globalization.CultureInfo.InvariantCulture);

                    if (isGroupSelected && currentGroupId != null)
                    {
                        if (!groupManager.ScaleGroupAverage(currentGroupId, scale, GetDrawingArea(), ZC, currentAxeName))
                        {
                            MessageBox.Show("Ошибка: масштабирование выносит элементы за границы экрана", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        if (CanPerformScaling(scale, scale))
                        { 
                            foreach (var figure in selectedFigures)
                            {
                                figure.ScaleAverage(scale);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Ошибка: масштабирование выносит элементы за границы экрана", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    UpdateParametersPanel();
                    this.Invalidate();
                    MessageBox.Show($"Масштабирование применено: коэффициент {scale}", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите число", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ApplyUniformScaling3D()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите коэффициент масштабирования (например: 1.5 для увеличения в 1.5 раза):",
                "Общее масштабирование", "1");

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    float scale = float.Parse(input.Trim(), System.Globalization.CultureInfo.InvariantCulture);

                    if (isGroupSelected && currentGroupId != null)
                    {
                        if (!groupManager.ScaleGroupAverage(currentGroupId, scale, GetDrawingArea(), ZC, currentAxeName, CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ))
                        {
                            MessageBox.Show("Ошибка: масштабирование выносит элементы за границы экрана", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        if (CanPerformScaling(scale, scale))
                        {
                            foreach (var figure in selectedFigures)
                            {
                                if(figure is LineElement3D line)
                                {
                                    line.ScaleAverage(scale);
                                    line.Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                                    if (totalRotationX != 0 || totalRotationY != 0 || totalRotationZ != 0)
                                        line.Rotate3DWithScene(CalculateSceneCenter(), totalRotationX, totalRotationY, totalRotationZ, ZC, currentAxeName);
                                }
                                if (figure is Cube3D cube)
                                {
                                    cube.ScaleAverage(scale);
                                    cube.Rotate3DWithScene(resetAngleValueX, resetAngleValueY, resetAngleValueZ, CalculateSceneCenter(), ZC, currentAxeName);
                                    if (totalRotationX != 0 || totalRotationY != 0 || totalRotationZ != 0)
                                        cube.Rotate3DWithScene(totalRotationX, totalRotationY, totalRotationZ, CalculateSceneCenter(), ZC, currentAxeName);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Ошибка: масштабирование выносит элементы за границы экрана", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    UpdateParametersPanel();
                    this.Invalidate();
                    MessageBox.Show($"Масштабирование применено: коэффициент {scale}", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите число", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ApplyMirroring()
        {
            if (selectedFigures.Count != 2 & !(isGroupSelected && currentGroupId != null & selectedFigures.Count == groupManager.GetGroupElements(currentGroupId).Count + 1))
            {
                MessageBox.Show($"Ошибка: необходимо выбрать 2 обекта:\n1 - которую зеркалировать,\n2 - относительно которой зеркалировать", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            FigureElement figureToMirror = selectedFigures[0];
            FigureElement mirrorLine = selectedFigures[1];

            if (figureToMirror.IsGrouped) 
            {
                mirrorLine = selectedFigures.Last();
                if (mirrorLine is LineElement mirrorLineElement)
                {
                    (float a, float b, float c, float d) = mirrorLineElement.GetEquation();
                    if (!groupManager.MirrorGroup(currentGroupId, mirrorLineElement, GetDrawingArea(), ZC, currentAxeName, CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, totalRotationX, totalRotationY, totalRotationZ))
                    {
                        MessageBox.Show("Ошибка: зеркалирование выносит элементы за границы экрана", "Ошибка",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    UpdateParametersPanel();
                    this.Invalidate();

                    MessageBox.Show("Зеркалирование выполнено успешно", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else 
            {
                if (mirrorLine is LineElement mirrorLineElement)
                {
                    if (figureToMirror is LineElement3D line3D)
                    {
                        // Зеркалирование 3D линии относительно прямой
                        line3D.Mirror3DRelativeToLine(mirrorLineElement);
                        line3D.Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                        line3D.Rotate3DWithScene(CalculateSceneCenter(), totalRotationX, totalRotationY, totalRotationZ, ZC, currentAxeName);
                    }
                    else if (figureToMirror is Cube3D cube)
                    {
                        // Зеркалирование куба относительно прямой
                        cube.Mirror3DRelativeToLine(mirrorLineElement);

                    }
                    else if (figureToMirror is LineElement line2D)
                    {
                        // Для 2D линии используем старый метод
                        (float A, float B, float C, float tempZ) = mirrorLineElement.GetEquation();
                        line2D.Mirror(A, B, C);
                    }

                    UpdateParametersPanel();
                    this.Invalidate();

                    MessageBox.Show("Зеркалирование выполнено успешно", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else 
                {
                    //если зеркалируем относительно куба
                }
            }

            


            // старое с 2д
            //if (selectedFigures.Count != 2 & !(isGroupSelected && currentGroupId != null & selectedFigures.Count == groupManager.GetGroupElements(currentGroupId).Count + 1)) 
            //{
            //    MessageBox.Show($"Ошибка: необходиом выбрать 2 прямые:\n1 - которую зеркалировать,\n2 - относительно которой зеркалировать", "Ошибка",
            //                      MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            ////FigureElement figure = selectedFigures[0];
            //if (selectedFigures.Last() is LineElement mirrorLine) 
            //{
            //    // Получаем уравнение прямой для зеркалирования
            //    (float A, float B, float C, float tempZ) = mirrorLine.GetEquation(); //а может и без флага

            //    // Проверяем, не выйдут ли фигуры за границы после зеркалирования
            //    if (!CanPerformMirror(A, B, C))
            //    {
            //        MessageBox.Show("Зеркалирование невозможно: изображение выйдет за границы экрана", "Ошибка",
            //                      MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }

            //    // Выполняем зеркалирование
            //    if (isGroupSelected && currentGroupId != null)
            //    {
            //        if (!groupManager.MirrorGroup(currentGroupId, A, B, C, GetDrawingArea()))
            //        {
            //            MessageBox.Show("Ошибка: зеркалиование выносит элементы за границы экрана", "Ошибка",
            //                  MessageBoxButtons.OK, MessageBoxIcon.Error);
            //            return;
            //        }
            //    }
            //    else
            //        //foreach (var figure in selectedFigures)
            //        //{

            //            if (selectedFigures[0] is LineElement line)
            //            {
            //                line.Mirror(A, B, C);
            //            }
            //        //}
            //    }

            //    UpdateParametersPanel();
            //    this.Invalidate();

            //    MessageBox.Show("Зеркалирование выполнено успешно", "Успех",
            //                      MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
        }

        private void ApplyProjection()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите плоскость или ось проецирования (xoy, yoz, xoz) (x, у или z):",
                "Общее масштабирование", "x");
            if (!string.IsNullOrEmpty(input))
            {
                if (input.Length == 1)
                {
                    try
                    {
                        string axis = input.ToLower();
                        foreach (var figure in selectedFigures)
                        {
                            figure.Projection(axis);
                        }
                        UpdateParametersPanel();
                        this.Invalidate();
                        MessageBox.Show($"Проецирование применено: ось {axis}", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите правильное название оси координат", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    try
                    {
                        string plosk = input.ToLower();
                        foreach (var figure in selectedFigures)
                        {
                            if(figure is LineElement3D line3d)
                            { 
                                line3d.Projection3D(plosk);
                                line3d.Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                                if (totalRotationX != 0 || totalRotationY != 0 || totalRotationZ != 0)
                                    line3d.Rotate3DWithScene(CalculateSceneCenter(), totalRotationX, totalRotationY, totalRotationZ, ZC, currentAxeName);
                            }
                            else 
                            {
                                if(figure is Cube3D cube) 
                                {
                                    cube.Projection3D(plosk);
                                }
                            }
                        }
                        UpdateParametersPanel();
                        this.Invalidate();
                        MessageBox.Show($"Проецирование применено: плоскость {plosk}", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите правильное название плоскости", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ApplyProjection3D()
        {
            
        }        

        private void ApplyProjection2D() 
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите ось проецирования (x, у или z):",
                "Общее масштабирование", "1");

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    string axis = input.ToLower();
                    foreach (var figure in selectedFigures)
                    {
                        figure.Projection(axis);
                    }                    
                    UpdateParametersPanel();
                    this.Invalidate();
                    MessageBox.Show($"Проецирование применено: ось {axis}", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка ввода: {ex.Message}\nВведите правильное название оси координат", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //private (float A, float B, float C) GetLineEquation(PointF p1, PointF p2)
        //{
        //    // Уравнение прямой: Ax + By + C = 0
        //    float A = p2.Y - p1.Y;
        //    float B = p1.X - p2.X;
        //    float C = p2.X * p1.Y - p1.X * p2.Y;

        //    return (A, B, C);
        //}

        private bool ResetSceneToDrawingPlane()
        {
            //Сбрасываем вращение всей сцены в ноль
            if (totalRotationX != resetAngleValueX || totalRotationY != resetAngleValueY || totalRotationZ != resetAngleValueZ)
            {
                // Вращаем обратно на накопленные углы
                //RotateAllFigures(-totalRotationX, -totalRotationY, 0); найиг это это старое, актуальное ниже, черт неужели из-за этого

                //пока так
                RotateEntireScene(-totalRotationX, -totalRotationY,  -totalRotationZ);
                RotateEntireScene(resetAngleValueX, resetAngleValueY, resetAngleValueZ);
                //RotateEntireScene(resetAngleValueX - totalRotationX, (resetAngleValueY - totalRotationY), (resetAngleValueZ - totalRotationZ));
                //пока так - работает кристально идеально

                //RotateEntireScene(-totalRotationX, (resetAngleValueY - totalRotationY), 0); //старое, новое следующее ниже нифига не старое они как то вместе идеально рабтали

                //RotateEntireScene(resetAngleValueX, resetAngleValueY, resetAngleValueZ);

                //coordinateAxes.Rotate3D(resetAngleValueX, resetAngleValueY, resetAngleValueZ, CalculateSceneCenter());//это уже есть внтури RotateEntireScene
                totalRotationX = resetAngleValueX;
                totalRotationY = resetAngleValueY;
                totalRotationZ = resetAngleValueZ;
                //!!!!!!!!!!!!!!!!
                //если меняется размер формы - нужно создать новые кроординатные ооси - так как центр области рисования-то поменялся
                //!!!!!!!!!!
                this.Invalidate();
                return true;
            }
            return false;
            //вроде норм рисуется - главное чтобы все вращаалось относительно одной точки - 000б или лучше центра рсовательной местности
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            // ПРЕОБРАЗУЕМ координаты мыши в мировые
            PointF worldMousePos = ScreenToWorld(e.Location);
            lastMousePos = ScreenToWorld(e.Location);

            var drawingArea = GetDrawingArea();
            if (!drawingArea.Contains(e.Location))
                return;            

            if (e.Button == MouseButtons.Left)
            {
                // ЕСЛИ НАЖАТ CTRL - множественное выделение
                if (Control.ModifierKeys == Keys.Control)
                {
                    FigureElement clickedFigure = null;
                    foreach (var figure in figures)
                    {                        
                        // ПРОВЕРЯЕМ в мировых координатах
                        if (figure.ContainsPoint(worldMousePos))
                        {
                            clickedFigure = figure;
                            break;
                        }
                    }

                    if (clickedFigure != null)
                    {
                        // Если кликнули на элемент группы - выделяем всю группу
                        if (clickedFigure.IsGrouped && !string.IsNullOrEmpty(clickedFigure.GroupId))
                        {
                            var groupElements = groupManager.GetGroupElements(clickedFigure.GroupId);
                            foreach (var element in groupElements)
                            {
                                element.IsSelected = true;
                                selectedFigures.Add(element);
                            }
                            currentGroupId = clickedFigure.GroupId;
                            isGroupSelected = true;
                            isDragging = true;
                            selectedFigure = clickedFigure;
                        }
                        else
                        {
                            clickedFigure.IsSelected = !clickedFigure.IsSelected;
                            if (clickedFigure.IsSelected && !selectedFigures.Contains(clickedFigure))
                            { 
                                selectedFigures.Add(clickedFigure);
                                selectedFigure = clickedFigure;
                            }
                            else if (!clickedFigure.IsSelected)
                            { 
                                selectedFigures.Remove(clickedFigure);
                                selectedFigure = null;
                            }
                        }
                        UpdateGroupSelectionState();
                    }

                    UpdateParametersPanel();
                    this.Invalidate();
                }
                else // БЕЗ CTRL - обычное поведение
                {
                    if (ResetSceneToDrawingPlane())
                        return;

                    ClearSelection();

                    // Проверяем клик по элементам в мировых координатах
                    FigureElement clickedFigure = null;
                    foreach (var figure in figures)
                    {
                        if (figure.ContainsPoint(worldMousePos))
                        {
                            clickedFigure = figure;
                            break;
                        }
                    }

                    if (clickedFigure != null)
                    {
                        // Если кликнули на элемент группы - выделяем всю группу
                        if (clickedFigure.IsGrouped && !string.IsNullOrEmpty(clickedFigure.GroupId))
                        {
                            var groupElements = groupManager.GetGroupElements(clickedFigure.GroupId);
                            foreach (var element in groupElements)
                            {
                                element.IsSelected = true;
                                selectedFigures.Add(element);
                            }
                            currentGroupId = clickedFigure.GroupId;
                            isGroupSelected = true;
                            isDragging = true;
                            selectedFigure = clickedFigure;
                        }
                        else
                        {
                            // Одиночный элемент
                            clickedFigure.IsSelected = true;
                            selectedFigures.Add(clickedFigure);
                            selectedFigure = clickedFigure;

                            // ПРОВЕРЯЕМ КЛИК ПО МАРКЕРАМ ИЗМЕНЕНИЯ РАЗМЕРА в мировых координатах
                            if (clickedFigure is LineElement3D line3d)
                            {
                                float handleSize = 8f;
                                RectangleF startHandle = new RectangleF(
                                    line3d.StartPoint.X - handleSize / 2, line3d.StartPoint.Y - handleSize / 2,
                                    handleSize, handleSize);
                                RectangleF endHandle = new RectangleF(
                                    line3d.EndPoint.X - handleSize / 2, line3d.EndPoint.Y - handleSize / 2,
                                    handleSize, handleSize);

                                if (startHandle.Contains(worldMousePos))
                                {
                                    isResizing = true;
                                    resizeStartPoint = true;
                                    UpdateParametersPanel();
                                    this.Invalidate();
                                    return;
                                }
                                else if (endHandle.Contains(worldMousePos))
                                {
                                    isResizing = true;
                                    resizeStartPoint = false;
                                    UpdateParametersPanel();
                                    this.Invalidate();
                                    return;
                                }
                            }
                            else if (clickedFigure is LineElement line)
                            {
                                float handleSize = 8f;
                                RectangleF startHandle = new RectangleF(
                                    line.StartPoint.X - handleSize / 2, line.StartPoint.Y - handleSize / 2,
                                    handleSize, handleSize);
                                RectangleF endHandle = new RectangleF(
                                    line.EndPoint.X - handleSize / 2, line.EndPoint.Y - handleSize / 2,
                                    handleSize, handleSize);

                                if (startHandle.Contains(worldMousePos))
                                {
                                    isResizing = true;
                                    resizeStartPoint = true;
                                    UpdateParametersPanel();
                                    this.Invalidate();
                                    return;
                                }
                                else if (endHandle.Contains(worldMousePos))
                                {
                                    isResizing = true;
                                    resizeStartPoint = false;
                                    UpdateParametersPanel();
                                    this.Invalidate();
                                    return;
                                }
                            }

                            // Если не попали в маркеры - тогда перемещаем
                            isDragging = true;
                        }

                        UpdateParametersPanel();
                        this.Invalidate();
                        return;
                    }

                    // Если не попали на фигуру - начинаем рисовать новую линию
                    ClearSelection();
                    parametersPanel.Visible = false;
                    isDrawing = true;
                    drawingStartPoint = worldMousePos; // сохраняем в мировых координатах
                }
            }
        }

        void AddSelectedFigures(FigureElement clickedFigure) 
        {

        }

        private void ClearSelection()
        {
            // Снимаем выделение со всех фигур
            foreach (var figure in figures)
            {
                figure.IsSelected = false;
            }

            // Очищаем списки выделения
            selectedFigures.Clear();
            selectedFigure = null;

            // Сбрасываем флаги групп
            isGroupSelected = false;
            currentGroupId = null;

            // Сбрасываем флаги операций
            isDragging = false;
            isResizing = false;
            isDrawing = false; //может не надо тут
        }

        private void кубToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetBaseSceneAngle();
            // Получаем область рисования
            var drawingArea = GetDrawingArea();

            // Создаем куб в центре области рисования
            Point3D cubeCenter = new Point3D(
                0/*drawingArea.Width / 2*/,
                0/*drawingArea.Height / 2*/,
                0  // Z координата
            );

            // Размер куба (примерно 1/4 от меньшей стороны области рисования)
            float cubeSize = Math.Min(drawingArea.Width, drawingArea.Height) / 4;
            cubeSize = 100;
            // Цвет куба
            Color cubeColor = Color.Blue;

            // Создаем куб
            Cube3D cube = new Cube3D(cubeCenter, cubeSize, cubeColor, currentAxeName, ZC);

            // Добавляем куб в список фигур
            figures.Add(cube);

            // Выделяем созданный куб
            ClearSelection();
            selectedFigure = cube;
            selectedFigure.IsSelected = true;
            selectedFigures.Add(cube);
            //cube.Rotate3D(30, 30, 30, /*cubeCenter*/CalculateSceneCenter(), GetDrawingArea());
            //cube.Rotate3D(-30, -30, -30, /*cubeCenter*/CalculateSceneCenter(), GetDrawingArea());
            // Обновляем интерфейс
            UpdateParametersPanel();
            this.Invalidate();

            MessageBox.Show($"Куб создан в центре экрана\nРазмер: {cubeSize:F0}px",
                           "Куб создан", MessageBoxButtons.OK, MessageBoxIcon.Information);        
        }

        private void UpdateGroupSelectionState()
        {
            currentGroupId = groupManager.GetSelectedGroupId(selectedFigures);
            isGroupSelected = currentGroupId != null;
        }

        private RectangleF GetGroupBoundingBox(string groupId)
        {
            var groupElements = groupManager.GetGroupElements(groupId);
            if (groupElements.Count == 0)
                return RectangleF.Empty;

            float minX = groupElements.Min(e => e.GetBoundingBox().Left);
            float minY = groupElements.Min(e => e.GetBoundingBox().Top);
            float maxX = groupElements.Max(e => e.GetBoundingBox().Right);
            float maxY = groupElements.Max(e => e.GetBoundingBox().Bottom);

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        private bool IsPointInHandle(PointF point, float handleX, float handleY, float handleSize)
        {
            return point.X >= handleX - handleSize / 2 && point.X <= handleX + handleSize / 2 &&
                   point.Y >= handleY - handleSize / 2 && point.Y <= handleY + handleSize / 2;
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            // ПРЕОБРАЗУЕМ координаты мыши в мировые
            PointF worldMousePos = ScreenToWorld(e.Location);
            isRotatingView = true; //может  вэтом пролема? куда убрать это тогда
            // Вращение сценой
            if (isRotatingView && e.Button == MouseButtons.Middle)
            {
                PointF delta = new PointF(worldMousePos.X - lastMousePos.X, worldMousePos.Y - lastMousePos.Y);

                // Преобразуем движение мыши в углы вращения
                float angleY = delta.X * rotationSensitivity; // Горизонтальное движение = вращение вокруг Y
                float angleX = delta.Y * rotationSensitivity; // Вертикальное движение = вращение вокруг X

                // Вращаем всю сцену
                RotateEntireScene(angleX, angleY, 0);

                lastMousePos = ScreenToWorld(e.Location);
                this.Invalidate();
            }


            //Возможно добавить изменение курсора если на крайние точки попали

            if (e.Button == MouseButtons.Left)
            {
                //ResetBaseSceneAngle();
                // Ограничиваем область рисования
                var drawingArea = GetDrawingArea();


                PointF delta = new PointF(worldMousePos.X - lastMousePos.X, worldMousePos.Y - lastMousePos.Y);

                if (isDragging)
                {
                    // Если есть группа - перемещаем группу
                    if (isGroupSelected && currentGroupId != null)
                    {
                        groupManager.MoveGroup(currentGroupId, delta, drawingArea.Height, drawingArea.Width, 0, currentAxeName);
                    }
                    // Если есть выделенные фигуры - перемещаем все выделенные
                    else if (selectedFigures.Count > 0)
                    {
                        foreach (var figure in selectedFigures)
                        {
                            if(figure is Cube3D cube) 
                            {
                                cube.Move3D(delta, ZERO_POINT_DIFFERENCE_Y, ZERO_POINT_DIFFERENCE_X, 0, currentAxeName);
                            }
                            else
                                selectedFigure.Move(delta, drawingArea.Height, drawingArea.Width, 0, currentAxeName);
                        }
                    }
                    // Иначе перемещаем одиночную фигуру
                    else if (selectedFigure != null)
                    {
                        if (selectedFigure is Cube3D cube)
                        {
                            cube.Move3D(delta, ZERO_POINT_DIFFERENCE_Y, ZERO_POINT_DIFFERENCE_X, 0, currentAxeName);
                        }
                        else
                            selectedFigure.Move(delta, drawingArea.Height, drawingArea.Width, 0, currentAxeName);
                    }
                    
                    UpdateParametersPanel();
                    this.Invalidate();
                }
                else if (isResizing && selectedFigure is LineElement line)
                {
                    //// Ограничиваем область рисования
                    /////Перекочевало наверх
                    //var drawingArea = GetDrawingArea();
                    //PointF newEndPoint = e.Location;

                    // Можно добавить ограничения если нужно:
                    //перекочевало наверх
                    //newEndPoint.X = Math.Max(0, Math.Min(newEndPoint.X, drawingArea.Width));
                    //newEndPoint.Y = Math.Max(0, Math.Min(newEndPoint.Y, drawingArea.Height));

                    // Ограничиваем область рисования в мировых координатах
                    var drawingAr = GetDrawingArea();
                    float maxX = drawingAr.Width / 2;
                    float maxY = drawingAr.Height / 2;
                    float minX = -maxX;
                    float minY = -maxY;

                    PointF newPoint = new PointF(
                        Math.Max(minX, Math.Min(worldMousePos.X, maxX)),
                        Math.Max(minY, Math.Min(worldMousePos.Y, maxY))
                    );

                    if (resizeStartPoint)
                    {
                        // Меняем начальную точку
                        if (line is LineElement3D line3d)
                        {
                            switch (currentAxeName)
                            {
                                case "xoy":
                                    line3d.StartPoint3D = new Point3D(newPoint, line3d.StartPoint3D.Z);
                                    break;
                                case "yoz":
                                    line3d.StartPoint3D = new Point3D(line3d.ZeroRatatedStartPoint.X, newPoint.X, newPoint.Y);
                                    line3d.Rotate3DWithScene(CalculateSceneCenter()/*new Point3D(0, 0, 0)*/, resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName); 
                                    //так центр у вех осей координат в 000Б а не как было - у х и у в 000, а z - центр жэкрана
                                    break;
                                case "xoz":
                                    line3d.StartPoint3D = new Point3D(newPoint.X, line3d.ZeroRatatedStartPoint.Y, newPoint.Y);
                                    line3d.Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                                    break;
                                default:                                    
                                    break;
                            }
                        }
                        else
                            line.StartPoint = newPoint;
                    }
                    else
                    {
                        // Меняем конечную точку
                        if (line is LineElement3D line3d)
                        {
                            switch (currentAxeName)
                            {
                                case "xoy":
                                    line3d.EndPoint3D = new Point3D(newPoint, line3d.EndPoint3D.Z);
                                    break;
                                case "yoz":
                                    line3d.EndPoint3D = new Point3D(line3d.ZeroRatatedEndPoint.X, newPoint.X, newPoint.Y);
                                    line3d.Rotate3DWithScene(CalculateSceneCenter()/*new Point3D(0, 0, 0)*/, resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName); //так центр у вех осей координат в 000Б а не как было - у х и у в 000, а z - центр жэкрана
                                    break;
                                case "xoz":
                                    line3d.EndPoint3D = new Point3D(newPoint.X, line3d.ZeroRatatedEndPoint.Y, newPoint.Y);
                                    line3d.Rotate3DWithScene(CalculateSceneCenter()/*new Point3D(0, 0, 0)*/, resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                                    break;
                                default:
                                    break;
                            }                            
                        }
                        else
                            line.EndPoint = newPoint;
                    }

                    //line.EndPoint = newEndPoint;
                    UpdateParametersPanel();
                    this.Invalidate();
                    //line.EndPoint = e.Location;
                    //UpdateParametersPanel();
                    //this.Invalidate();
                }
                else if (isDrawing)
                {
                    //ResetSceneToDrawingPlane();
                    //lastMousePos = worldMousePos; // обновляем в мировых координатах
                    this.Invalidate(); // Перерисовываем для отображения временной линии
                }
            }            
            lastMousePos = worldMousePos; // сохраняем в мировых координатах;
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // ПРЕОБРАЗУЕМ координаты мыши в мировые
                PointF worldMousePos = ScreenToWorld(e.Location);
                if (isDrawing)
                {
                    // Создаем новую линию только если длина достаточная и в области рисования
                    var drawingAr = GetDrawingArea();
                    if (drawingAr.Contains(e.Location) && Distance(drawingStartPoint, worldMousePos) > 5)
                    {
                        // Ограничиваем область в мировых координатах
                        float maxX = drawingAr.Width / 2;
                        float maxY = drawingAr.Height / 2;
                        float minX = -maxX;
                        float minY = -maxY;

                        PointF drawingEndPoint = new PointF(
                            Math.Max(minX, Math.Min(worldMousePos.X, maxX)),
                            Math.Max(minY, Math.Min(worldMousePos.Y, maxY))
                        );

                        FigureElement newLine;
                        if (is3DMode)
                        {
                            //SimpleCamera.GroupManager = groupManager;
                            //SimpleCamera.DrawingArea = GetDrawingArea();
                            // УЧИТЫВАЕМ поворот системы - применяем обратное вращение
                            // ПРЕОБРАЗУЕМ координаты мыши в мировые
                            Point3D start3D = TransformScreenToWorld((drawingStartPoint), 0);
                            Point3D end3D = TransformScreenToWorld(drawingEndPoint, 0);
                            //Point3D start3D = TransformScreenToWorld(drawingStartPoint, 0);
                            //Point3D end3D = TransformScreenToWorld(drawingEndPoint, 0);
                            //newLine = new LineElement3D(start3D, end3D, Color.Black, 3f);
                            switch (currentAxeName) //в обновлении данных из панели параметров и в паннель параметров также сделать
                            {
//                                тут визуальные координаты так задавать надо, не реальные
//                                а как рисовать в плоскости не понятно
//                                проблема 1 - рисовать в плоскости только надо +чтоб возращалась в исходное после вращения
//    проблема 2 - когда ворочаем то чтобы отобразить меняем координаты -
//    и они поменяные отображаются в свойстах - низя так - разбить координаты для рисования и для публикации как свойтсва


//возможность рисования в плоскости, выбор при совершении операций - относительно линии / группы(если в группе) или начала координат, 
//панель параметров для куба, колесиком изменеие z не отображается потому что в рисвоении в линии в mainform.cs идет через SetZ старое

//                                дипсик предложил создавать относительно центра как-то высчитывать а не просто х=0 задавать

                                //xOy
                                //yoz
                                //xOz
                                case "xoy":
                                    newLine = new LineElement3D(start3D, end3D, Color.Black, 3f);
                                    ((LineElement3D)newLine).Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                                    break;
                                case "yoz":

                                    Point3D tempPoint = new Point3D(drawingStartPoint.X, drawingStartPoint.Y, 0);
                                    start3D.X = 0;
                                    start3D.Y = tempPoint.X;
                                    start3D.Z = tempPoint.Y/*- CalculateSceneCenter().Y*/;
                                    tempPoint = new Point3D(drawingEndPoint.X, drawingEndPoint.Y, 0);
                                    end3D.X = 0;
                                    end3D.Y = tempPoint.X;
                                    end3D.Z = tempPoint.Y/*-CalculateSceneCenter().Y*/;
                                    newLine = new LineElement3D(start3D, end3D, Color.Black, 3f);
                                    // Вращаем новую линию вместе со сценой
                                    //newLine.Rotate3DWithScene();
                                    ((LineElement3D)newLine).Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                                    // Вращаем новую линию вместе со сценой
                                    //newLine.Rotate3DWithScene();
                                    /*((LineElement3D)newLine).Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ);
                                    // Создаем линию в реальных 3D координатах плоскости YOZ
                                    start3D = new Point3D(
                                        0,  // X
                                        drawingStartPoint.X - ZERO_POINT_DIFFERENCE_X,  // Y 
                                        drawingStartPoint.Y - ZERO_POINT_DIFFERENCE_Y   // Z
                                    );

                                     end3D = new Point3D(
                                        0,  // X
                                        drawingEndPoint.X - ZERO_POINT_DIFFERENCE_X,    // Y
                                        drawingEndPoint.Y - ZERO_POINT_DIFFERENCE_Y     // Z
                                    );

                                    newLine = new LineElement3D(start3D, end3D, Color.Black, 3f);
                                    ((LineElement3D)newLine).Rotate3DWithScene(CalculateSceneCenter(), 0, -90, -90);*/
                                    // Создаем линию в плоскости YOZ
                                    /*Point3D start3D = new Point3D(
                                        0,  // X = 0 в плоскости YOZ
                                        drawingStartPoint.X - ZERO_POINT_DIFFERENCE_X,  // Y = экранный X
                                        drawingStartPoint.Y - ZERO_POINT_DIFFERENCE_Y   // Z = экранный Y  
                                    );

                                    Point3D end3D = new Point3D(
                                        0,  // X = 0 в плоскости YOZ
                                        drawingEndPoint.X - ZERO_POINT_DIFFERENCE_X,    // Y = экранный X
                                        drawingEndPoint.Y - ZERO_POINT_DIFFERENCE_Y     // Z = экранный Y
                                    );

                                    newLine = new LineElement3D(start3D, end3D, Color.Black, 3f);

                                    // Вращаем для отображения в плоскости YOZ
                                    ((LineElement3D)newLine).Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ);

                                    // РУЧНАЯ КОРРЕКЦИЯ 2D КООРДИНАТ для правильного отображения в YOZ
                                    // В плоскости YOZ: X игнорируется, Y становится X на экране, Z становится Y на экране
                                    ((LineElement3D)newLine).StartPoint = new PointF(
                                        ((LineElement3D)newLine).ZeroRatatedStartPoint.Y + ZERO_POINT_DIFFERENCE_X,
                                        ((LineElement3D)newLine).ZeroRatatedStartPoint.Z + ZERO_POINT_DIFFERENCE_Y
                                    );

                                    ((LineElement3D)newLine).EndPoint = new PointF(
                                        ((LineElement3D)newLine).ZeroRatatedEndPoint.Y + ZERO_POINT_DIFFERENCE_X,
                                        ((LineElement3D)newLine).ZeroRatatedEndPoint.Z + ZERO_POINT_DIFFERENCE_Y
                                    );*/
                                    //Point3D tempPoint = new Point3D(drawingStartPoint.X, drawingStartPoint.Y, 0);
                                    //start3D.X = 0;
                                    //start3D.Y = tempPoint.X;
                                    //start3D.Z = tempPoint.Y/*- CalculateSceneCenter().Y*/;
                                    //tempPoint = new Point3D(drawingEndPoint.X, drawingEndPoint.Y, 0);
                                    //end3D.X = 0;
                                    //end3D.Y = tempPoint.X;
                                    //end3D.Z = tempPoint.Y/*-CalculateSceneCenter().Y*/;
                                    //newLine = new LineElement3D(start3D, end3D, Color.Black, 3f);
                                    //// Вращаем новую линию вместе со сценой
                                    ////newLine.Rotate3DWithScene();
                                    //((LineElement3D)newLine).Rotate3DWithScene(CalculateSceneCenter(), resetAngleValueX, resetAngleValueY, resetAngleValueZ);

                                    //((LineElement3D)newLine).StartPoint3D.X = ((LineElement3D)newLine).StartPoint3D.X - (ZERO_POINT_DIFFERENCE_X - ZERO_POINT_DIFFERENCE_Y);
                                    //((LineElement3D)newLine).StartPoint3D.Y = ((LineElement3D)newLine).StartPoint3D.Y - ZERO_POINT_DIFFERENCE_Y;
                                    //((LineElement3D)newLine).StartPoint3D.Z = ((LineElement3D)newLine).StartPoint3D.Z + ZERO_POINT_DIFFERENCE_X;
                                    //((LineElement3D)newLine).EndPoint3D.X = ((LineElement3D)newLine).EndPoint3D.X - (ZERO_POINT_DIFFERENCE_X - ZERO_POINT_DIFFERENCE_Y);
                                    //((LineElement3D)newLine).EndPoint3D.Y = ((LineElement3D)newLine).EndPoint3D.Y - ZERO_POINT_DIFFERENCE_Y;
                                    //((LineElement3D)newLine).EndPoint3D.Z = ((LineElement3D)newLine).EndPoint3D.Z + ZERO_POINT_DIFFERENCE_X;
                                    //((LineElement3D)newLine).Update2DProjection();
                                    //так центр у вех осей координат в 000Б а не как было - у х и у в 000, а z - центр жэкрана
                                    break;
                                case "xoz":
                                    /*Point3D*/ tempPoint = new Point3D(drawingStartPoint.X, drawingStartPoint.Y, 0);
                                    start3D.X = tempPoint.X;
                                    start3D.Y = 0;
                                    start3D.Z = tempPoint.Y /*- CalculateSceneCenter().Y*/;
                                    tempPoint = new Point3D(drawingEndPoint.X, drawingEndPoint.Y, 0);
                                    end3D.X = tempPoint.X;
                                    end3D.Y = 0;
                                    end3D.Z = tempPoint.Y;
                                    newLine = new LineElement3D(start3D, end3D, Color.Black, 3f);
                                    ((LineElement3D)newLine).Rotate3DWithScene(CalculateSceneCenter()/*new Point3D(0, 0, 0)*/, resetAngleValueX, resetAngleValueY, resetAngleValueZ, ZC, currentAxeName);
                                    break;
                                default:
                                    newLine = null;
                                    break;
                            }

                            
                            
                            //ГАВНО - Х И У ИМЕЮТ 0 В ЛЕВОМ ВЕРХНЕМ УГЛУ ЭКРАНА, А Z - ПО ЦЕНТРУ
                                //нужно как-то это коректировать при создании линии - и z "не совместими"
                            //newLine = new LineElement3D(
                            //    drawingStartPoint, drawingEndPoint, Color.Black, 3f); //зачем это вообе надо ! 2 раза создается
                            //((LineElement3D)newLine).Rotate3DWithScene(CalculateSceneCenter(), -(resetAngleValueX - totalRotationX), -(resetAngleValueY - totalRotationY), -(resetAngleValueZ - totalRotationZ));
                        }
                        else
                        {
                            newLine = new LineElement(drawingStartPoint, drawingEndPoint, Color.Black, 3f);
                        }
                                                
                        figures.Add(newLine);
                        
                        // АВТОМАТИЧЕСКИ ВЫДЕЛЯЕМ новую линию и добавляем в selectedFigures
                        ClearSelection(); // Сначала сбрасываем старое выделение
                        selectedFigure = newLine;
                        selectedFigure.IsSelected = true;
                        selectedFigures.Add(newLine); //чтобы после создания линии не было необходимости заново ее выделять для создания группы
                        UpdateParametersPanel();
                    }
                    isDrawing = false;
                    this.Invalidate();
                }

                isDragging = false;
                isResizing = false;
                if(Control.ModifierKeys != Keys.Control)
                    ResetSceneToDrawingPlane();
            }
        }

        private Point3D TransformScreenToWorld(PointF screenPoint, float z)
        {
            // Преобразуем экранные координаты в мировые с учетом поворота
            Point3D worldPoint = new Point3D(screenPoint.X, screenPoint.Y, z);

            // Применяем обратное вращение (если система повернута)
            //if (Math.Abs(totalRotationX) > 0.001f || Math.Abs(totalRotationY) > 0.001f)
            //{
            //    Point3D sceneCenter = CalculateSceneCenter();
            //    worldPoint = RotatePoint3D(worldPoint, sceneCenter, -totalRotationX, -totalRotationY, 0);
            //}

            return worldPoint;
        }

        private Point3D RotatePoint3D(Point3D point, Point3D center, float angleX, float angleY, float angleZ)
        {
            // Та же функция вращения, но для обратного преобразования
            float x = point.X - center.X;
            float y = point.Y - center.Y;
            float z = point.Z - center.Z;

            float radX = angleX * (float)Math.PI / 180f;
            float radY = angleY * (float)Math.PI / 180f;
            float radZ = angleZ * (float)Math.PI / 180f;

            float cosX = (float)Math.Cos(radX), sinX = (float)Math.Sin(radX);
            float cosY = (float)Math.Cos(radY), sinY = (float)Math.Sin(radY);
            float cosZ = (float)Math.Cos(radZ), sinZ = (float)Math.Sin(radZ);

            float x1 = x * cosY * cosZ + y * (sinX * sinY * cosZ - cosX * sinZ) + z * (cosX * sinY * cosZ + sinX * sinZ);
            float y1 = x * cosY * sinZ + y * (sinX * sinY * sinZ + cosX * cosZ) + z * (cosX * sinY * sinZ - sinX * cosZ);
            float z1 = x * -sinY + y * sinX * cosY + z * cosX * cosY;

            return new Point3D(x1 + center.X, y1 + center.Y, z1 + center.Z);
        }

        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control && selectedFigures.Count > 0)
            {
                float deltaZ = e.Delta > 0 ? 10f : -10f; // Вверх = +10, вниз = -10
                bool changed = false;

                foreach (var figure in selectedFigures)
                {
                    if (figure is LineElement3D line3D)
                    {
                        if (Control.ModifierKeys == (Keys.Control | Keys.Shift))
                        {
                            // Ctrl+Shift+Колесо - изменяем только начальную точку
                                    //line3D.StartPoint3D.Z = line3D.StartPoint3D.Z + deltaZ; тут не новый поин создавали поэтому и не менялось
                            
                            line3D.ChangeZ(deltaZ, 0);
                            //line3D.SetStartZ(GetDrawingArea(), groupManager, line3D.StartZ + deltaZ);
                        }
                        else if (Control.ModifierKeys == (Keys.Control | Keys.Alt))
                        {
                            // Ctrl+Alt+Колесо - изменяем только конечную точку
                                    //line3D.EndPoint3D = new Point3D(line3D.EndPoint3D.X, line3D.EndPoint3D.Y, line3D.EndPoint3D.Z + deltaZ);
                            line3D.ChangeZ(0, deltaZ);
                            //line3D.SetEndZ(GetDrawingArea(), groupManager, line3D.EndZ + deltaZ);                          
                        }
                        else
                        {
                            // Просто Ctrl+Колесо - изменяем всю линию
                            line3D.ChangeZ(deltaZ, deltaZ);
                                    //line3D.StartPoint3D = new Point3D(line3D.StartPoint3D.X, line3D.StartPoint3D.Y, line3D.StartPoint3D.Z + deltaZ);
                                    //line3D.EndPoint3D = new Point3D(line3D.EndPoint3D.X, line3D.EndPoint3D.Y, line3D.EndPoint3D.Z + deltaZ);
                            //line3D.SetStartZ(GetDrawingArea(), groupManager, line3D.StartZ + deltaZ);
                            //line3D.SetEndZ(GetDrawingArea(), groupManager, line3D.EndZ + deltaZ);
                        }
                        changed = true;
                    }
                }

                if (changed)
                {
                    this.Invalidate();
                    UpdateParametersPanel();
                }
            }
        }

        private float Distance(PointF p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            // Получаем область рисования
            var drawingArea = GetDrawingArea();
            float centerX = drawingArea.X + drawingArea.Width / 2;
            float centerY = drawingArea.Y + drawingArea.Height / 2;

            // Временно отключаем трансформацию для очистки
            e.Graphics.FillRectangle(Brushes.White, drawingArea);

            // Теперь применяем трансформацию (после очистки!)
            e.Graphics.TranslateTransform(centerX, centerY);
            e.Graphics.ScaleTransform(1, -1);

            // ВСЕ остальное рисуется в новых координатах автоматически
            coordinateAxes.Draw(e.Graphics);

            foreach (var figure in figures)
            {
                figure.Draw(e.Graphics);
                if (figure.IsSelected)
                {
                    figure.DrawSelection(e.Graphics);
                }
            }

            if (isGroupSelected && currentGroupId != null && groupManager.GetGroupElements(currentGroupId).Count != 0)
            {
                groupManager.DrawGroupSelection(e.Graphics, currentGroupId);
            }

            // Временная линия - координаты автоматически трансформируются!
            if (isDrawing)
            {
                using (Pen tempPen = new Pen(Color.Gray, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    // Graphics сам преобразует координаты!
                    e.Graphics.DrawLine(tempPen, drawingStartPoint, lastMousePos);
                }
            }


            /*e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TranslateTransform(ZERO_POINT_DIFFERENCE_X, ZERO_POINT_DIFFERENCE_Y);
            // Можем также изменить направление осей (например, сделать Y направленным вверх)
            e.Graphics.ScaleTransform(1, -1);
            // Очищаем только область рисования
            var drawingArea = GetDrawingArea();
            e.Graphics.FillRectangle(Brushes.White, drawingArea);

            // Рисуем оси координат (если включены)
            //if (showCoordinateAxes && coordinateAxes != null)
            //{
                coordinateAxes.Draw(e.Graphics);
            //}

            // Рисуем все фигуры
            foreach (var figure in figures)
            {
                figure.Draw(e.Graphics);

                if (figure.IsSelected)
                {
                    figure.DrawSelection(e.Graphics);
                }
            }

            // Рисуем выделение групп
            if (isGroupSelected && currentGroupId != null && groupManager.GetGroupElements(currentGroupId).Count != 0)
            {
                groupManager.DrawGroupSelection(e.Graphics, currentGroupId);
            }

            // Рисуем временную линию при создании
            if (isDrawing)
            {
                using (Pen tempPen = new Pen(Color.Gray, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    e.Graphics.DrawLine(tempPen, drawingStartPoint, lastMousePos);
                }
            }*/
            // Рисуем границу области рисования (для красоты)
            //using (Pen borderPen = new Pen(Color.LightGray, 1))
            //{
            //    e.Graphics.DrawRectangle(borderPen, drawingArea.X, drawingArea.Y,
            //                           drawingArea.Width - 1, drawingArea.Height - 1);
            //}
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedFigure != null)
            {
                if (selectedFigure.IsGrouped)
                {
                    groupManager.RemoveItem(selectedFigure.GroupId, selectedFigure);
                    //isGroupSelected = false;
                    //currentGroupId = null;
                }
                figures.Remove(selectedFigure);
                if (selectedFigure is Cube3D cube)
                {
                    figures.Remove(cube.TcX);
                    figures.Remove(cube.TcY);
                    figures.Remove(cube.TcZ);
                }
                //selectedFigure = null;
                ClearSelection();
                parametersPanel.Visible = false;
                this.Invalidate();
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                saveDialog.DefaultExt = "json";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    SceneSerializer.SaveToFile(saveDialog.FileName, figures, groupManager);
                    MessageBox.Show("Сцена сохранена!");
                }
            }
        }

        private void загрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                openDialog.DefaultExt = "json";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    // Очищаем текущую сцену
                    figures.Clear();
                    selectedFigures.Clear();
                    selectedFigure = null;

                    // Загружаем из файла
                    var (loadedFigures, loadedGroupManager) =
                        SceneSerializer.LoadFromFile(openDialog.FileName);

                    // Копируем элементы в основной список
                    figures.AddRange(loadedFigures);

                    // ОБНОВЛЯЕМ ГРУПП-МЕНЕДЖЕР
                    groupManager = loadedGroupManager;

                    // СИНХРОНИЗИРУЕМ ГРУППЫ
                    SynchronizeGroupsAfterLoad();

                    // Сбрасываем выделение
                    ClearSelection();
                    isGroupSelected = false;
                    currentGroupId = null;

                    // Обновляем интерфейс
                    UpdateParametersPanel();
                    Invalidate();

                    MessageBox.Show("Сцена загружена!");
                }
            }
        }

        private void SynchronizeGroupsAfterLoad()
        {
            // Проходим по всем группам в groupManager
            foreach (var groupId in groupManager.GetAllGroupIds())
            {
                var groupElements = groupManager.GetGroupElements(groupId);

                // Для каждого элемента в группе ищем соответствующий элемент в figures
                for (int i = 0; i < groupElements.Count; i++)
                {
                    var groupElement = groupElements[i];

                    // Ищем элемент с такими же координатами в figures
                    var matchingFigure = figures.FirstOrDefault(f =>
                        f.GetType() == groupElement.GetType() &&
                        Math.Abs(f.Position.X - groupElement.Position.X) < 0.1f &&
                        Math.Abs(f.Position.Y - groupElement.Position.Y) < 0.1f);

                    if (matchingFigure != null)
                    {
                        // Заменяем элемент в группе на найденный из figures
                        groupElements[i] = matchingFigure;
                        matchingFigure.GroupId = groupId;
                        matchingFigure.IsGrouped = true;
                    }
                }
            }
        }

        private void xOyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDrawingAxe("xOy");
        }

        private void yOzToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetDrawingAxe("yOz");
        }

        private void xOzToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDrawingAxe("xOz");
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && selectedFigure != null)
            {
                if (selectedFigure.IsGrouped)
                { 
                    groupManager.RemoveItem(selectedFigure.GroupId, selectedFigure);
                    //isGroupSelected = false;
                    //currentGroupId = null;                    
                }
                figures.Remove(selectedFigure);
                if (selectedFigure is Cube3D cube)
                {
                    figures.Remove(cube.TcX);
                    figures.Remove(cube.TcY);
                    figures.Remove(cube.TcZ);
                }
                ClearSelection();
                //selectedFigure = null;
                parametersPanel.Visible = false;
                this.Invalidate();
            }
        }

        private void BtnGroup_Click(object sender, EventArgs e)
        {
            if (selectedFigures.Count >= 2)
            {
                string groupId = groupManager.CreateGroup(selectedFigures);
                if (groupId != null)
                {
                    UpdateGroupSelectionState();
                    this.Invalidate();
                }
            }
        }

        private void BtnUngroup_Click(object sender, EventArgs e)
        {
            if (isGroupSelected && currentGroupId != null)
            {
                groupManager.Ungroup(currentGroupId);
                ClearSelection();
                //isGroupSelected = false;
                //currentGroupId = null;
                this.Invalidate();
            }
        }

        private void RotateCoordinateAxes(float angleX, float angleY, float angleZ)
        {
            if (coordinateAxes != null)
            {
                coordinateAxes.Rotate3D(angleX, angleY, angleZ, CalculateSceneCenter());
                this.Invalidate();
            }
        }

        // Пример метода для сброса осей в исходное положение
        private void ResetCoordinateAxes()
        {
            var drawingArea = GetDrawingArea();
            float margin = 50f;
            float length = Math.Min(drawingArea.Width, drawingArea.Height) / 4;

            coordinateAxes = new MainCoordinateAxes(margin, length);
            this.Invalidate();
        }

        // Методы для вращения
        private void MainForm_MouseDownForRotation(object sender, MouseEventArgs e)
        {
            var drawingArea = GetDrawingArea();
            if (!drawingArea.Contains(e.Location))
                return;

            // Зажата кнопка колесика (средняя кнопка мыши)
            if (e.Button == MouseButtons.Middle && is3DMode)
            {
                isRotatingView = true;
                lastMousePos = ScreenToWorld(e.Location);
                this.Cursor = Cursors.SizeAll; // Меняем курсор
            }
        }

        private void MainForm_MouseUpForRotation(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isRotatingView = false;
                this.Cursor = Cursors.Default; // Возвращаем обычный курсор
            }
        }

        private void RotateEntireScene(float angleX, float angleY, float angleZ)
        {
            this.angleX += angleX;
            this.angleY += angleY;
            this.angleZ += angleZ;
            // Обновляем накопленные углы (для информации)
            totalRotationX += angleX;
            totalRotationY += angleY;
            totalRotationZ += angleZ;
            Point3D sceneCenter = CalculateSceneCenter();//new Point3D(0, 0, 0);//CalculateSceneCenter(); ЧЕРТ да вот тут не квокруг того центра крутится
            coordinateAxes.Rotate3D(totalRotationX, totalRotationY, totalRotationZ,/*0,*//*angleX, angleY, angleZ,*/ CalculateSceneCenter()/*new Point3D(0.0,0)*/);
            foreach (var figure in figures)
            {
                if (figure is LineElement3D line3D)
                {
                    // Вращаем каждую линию относительно центра сцены
                    line3D.Rotate3DWithScene(sceneCenter, totalRotationX, totalRotationY, totalRotationZ/*0*//*angleX, angleY, angleZ*/, ZC, currentAxeName);
                }
                if(figure is Cube3D cube) 
                {
                    cube.Rotate3DWithScene(totalRotationX, totalRotationY, totalRotationZ/*0*//*angleX, angleY, angleZ*/, sceneCenter, ZC, currentAxeName);
                }
            }
           
            this.Invalidate();

            return;
            // Вращаем оси координат
            if (coordinateAxes != null)
            {
                coordinateAxes.Rotate3D(angleX, angleY, angleZ, CalculateSceneCenter());
            }

            // Вращаем все фигуры относительно центра сцены
            RotateAllFigures(angleX, angleY, angleZ);

            // Обновляем накопленные углы (для информации)
            totalRotationX += angleX;
            totalRotationY += angleY;

            UpdateStatusBar(); // Обновим строку состояния
        }

        private void RotateAllFigures(float angleX, float angleY, float angleZ)
        {
            if (figures.Count == 0) return;

            // Вычисляем центр всей сцены
            Point3D sceneCenter = CalculateSceneCenter();/*new Point3D(0.0,0)*/ //нужно ли
            //Point3D sceneCenter = new Point3D(0,0,0);

            // Вращаем каждую фигуру относительно центра сцены
            foreach (var figure in figures)
            {
                if (figure is LineElement3D line3D)
                {
                    line3D.Rotate3D(sceneCenter, angleX, angleY, angleZ, ZC);
                }
                else if (figure is Cube3D cube)
                {
                    // Для куба вращаем вокруг его центра
                    cube.Rotate3D(angleX, angleY, angleZ, CalculateSceneCenter(), GetDrawingArea()/*new Point3D(0.0,0)*/, ZC);
                }
                // 2D фигуры не вращаем в 3D
            }
        }

        private PointF GetDrawingAreaCenter() 
        {
            Rectangle drawArea = GetDrawingArea();
            return new PointF(drawArea.Width / 2, drawArea.Height / 2);
        }

        private Point3D CalculateRealSceneCenter() { return new Point3D(0, 0, 0); }

        private Point3D CalculateSceneCenter()
        {
            // Центр сцены с учетом смещения от меню
            //float offsetY = ZERO_POINT_DIFFERENCE_Y;//menuStrip.Height + 50f;
            //float offsetX = ZERO_POINT_DIFFERENCE_X;//50f;
            //return new Point3D(offsetX, offsetY, 0);
            //return new Point3D(offsetX, offsetY, 0);
            return new Point3D(0, 0, 0);
            Rectangle drawArea = GetDrawingArea();
            return new Point3D(drawArea.Width/2, drawArea.Height/2, 0);

            if (figures.Count == 0) return new Point3D(0, 0, 0);

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            foreach (var figure in figures)
            {
                if (figure is LineElement3D line3D)
                {
                    minX = Math.Min(minX, Math.Min(line3D.StartPoint3D.X, line3D.EndPoint3D.X));
                    maxX = Math.Max(maxX, Math.Max(line3D.StartPoint3D.X, line3D.EndPoint3D.X));
                    minY = Math.Min(minY, Math.Min(line3D.StartPoint3D.Y, line3D.EndPoint3D.Y));
                    maxY = Math.Max(maxY, Math.Max(line3D.StartPoint3D.Y, line3D.EndPoint3D.Y));
                    minZ = Math.Min(minZ, Math.Min(line3D.StartPoint3D.Z, line3D.EndPoint3D.Z));
                    maxZ = Math.Max(maxZ, Math.Max(line3D.StartPoint3D.Z, line3D.EndPoint3D.Z));
                }
            }

            return new Point3D(
                (minX + maxX) / 2,
                (minY + maxY) / 2,
                (minZ + maxZ) / 2
            );

            //if (figures.Count == 0) return new Point3D(0, 0, 0);

            //var drawingArea = GetDrawingArea();
            //return new Point3D(
            //    drawingArea.Width / 2,
            //    drawingArea.Height / 2,
            //    0
            //);
        }

        private void ResetBaseSceneAngle() 
        {
            //float tempX = angleX;
            //float tempY = angleY;
            //float tempZ = angleZ;
            //angleX = 0;
            //angleY = 0;
            //angleZ = 0;
            //RotateEntireScene(-tempX, -tempY, -tempZ);
            //angleX = 0;
            //angleY = 0;
            //angleZ = 0;

        }

        private void SetDrawingAxe(string axeName, int axeMargin = 0) 
        {
            axeName = axeName.ToLower();
            switch (axeName) 
            {
                case "xoy":
                    currentAxeName = axeName;
                    resetAngleValueX = 0;
                    resetAngleValueY = 0;
                    resetAngleValueZ = 0;
                    ResetSceneToDrawingPlane();
                    //RotateEntireScene(resetAngleValueX, resetAngleValueY, 0);
                    break;
                //xOy
                //xOz
                case "yoz":
                    currentAxeName = axeName;
                    resetAngleValueX = 0;
                    resetAngleValueY = -90;
                    resetAngleValueZ = -90;
                    ResetSceneToDrawingPlane();
                    //RotateEntireScene(resetAngleValueX, resetAngleValueY, 0);
                    break;

                case "xoz":
                    currentAxeName = axeName;
                    resetAngleValueX = -90;
                    resetAngleValueY = 0;
                    resetAngleValueZ = 0;
                    ResetSceneToDrawingPlane();
                    //RotateEntireScene(resetAngleValueX, resetAngleValueY, 0);
                    break;
            }
        }
        //public PointF ScreenToWorldStatic(Point screenPoint)
        //{
        //    var drawingArea = GetDrawingArea();
        //    float centerX = drawingArea.X + drawingArea.Width / 2;
        //    float centerY = drawingArea.Y + drawingArea.Height / 2;

        //    return new PointF(
        //        screenPoint.X - centerX,
        //        -(screenPoint.Y - centerY)
        //    );
        //}

        private PointF ScreenToWorld(Point screenPoint)
        {
            ZERO_POINT_DIFFERENCE_X = GetDrawingAreaCenter().X;
            ZERO_POINT_DIFFERENCE_Y = GetDrawingAreaCenter().Y;
            var drawingArea = GetDrawingArea();
            float centerX = drawingArea.X + ZERO_POINT_DIFFERENCE_X;//drawingArea.Width / 2;
            float centerY = drawingArea.Y + ZERO_POINT_DIFFERENCE_Y;//drawingArea.Height / 2;

            // Преобразуем экранные координаты в мировые
            return new PointF(
                screenPoint.X - centerX,                    // X: смещаем относительно центра
                -(screenPoint.Y - centerY)                 // Y: смещаем и инвертируем
            );
        }

        private float WorldToScreen(float screenPoint)
        {
            var drawingArea = GetDrawingArea();
            float centerX = drawingArea.X + ZERO_POINT_DIFFERENCE_X;//drawingArea.Width / 2;
            float centerY = drawingArea.Y + ZERO_POINT_DIFFERENCE_Y;//drawingArea.Height / 2;

            // Преобразуем экранные координаты в мировые
            return
                screenPoint + centerX;                   // X: смещаем относительно центр               // Y: смещаем и инвертируем
            
        }
    }
}