using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
namespace GrafRedactor
{
    public partial class MainForm : Form
    {
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
        private Label lblStartPoint, lblEndPoint, lblThickness, lblColor, lblOperations, lblEquation, lblStartZ, lblEndZ;

        // Константы для размеров
        private const int PARAMETERS_PANEL_WIDTH = 450;
        private const int MIN_DRAWING_WIDTH = 500;

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

            InitializeStatusBar(); // Добавляем строку состояния
            UpdateModeVisuals(); // Обновляем интерфейс под текущий режим
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
                Size = new Size(400, 25),
                Font = new Font("Arial", 12),
                //ForeColor = Color.Black
            };
            parametersPanel.Controls.Add(lblEquation);
            y += 40;

            // Координаты начальной точки
            lblStartPoint = AddLabel("Начальная точка:", 20, y);
            y += 25;
            AddLabel("X:", 40, y);
            numStartX = AddNumericUpDown(70, y, 0, 1000, 100);
            numStartX.ValueChanged += Parameters_ValueChanged;

            AddLabel("Y:", 160, y);
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
            AddLabel("X:", 40, y);
            numEndX = AddNumericUpDown(70, y, 0, 1000, 200);
            numEndX.ValueChanged += Parameters_ValueChanged;

            AddLabel("Y:", 160, y);
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
            comboOperation = new ComboBox
            {
                Location = new Point(180, y - 40),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboOperation.Items.AddRange(new object[] { "", "Смещение", "Вращение", "Масштабирование", "Общее масштабирование", "Зеркалирование", "Проецирование" });
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
            comboColor.Items.AddRange(new object[] { "Black", "Red", "Green", "Blue", "Orange", "Purple" });
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
            if (parametersPanel != null)
            {
                parametersPanel.Location = new Point(this.ClientSize.Width - PARAMETERS_PANEL_WIDTH, 0);
                parametersPanel.Height = this.ClientSize.Height;
            }
        }

        // Получение области рисования (вся форма минус панель параметров)
        private Rectangle GetDrawingArea()
        {
            int drawingWidth = this.ClientSize.Width - (/*parametersPanel.Visible ?*/ PARAMETERS_PANEL_WIDTH/* : 0*/);
            return new Rectangle(0, 0, drawingWidth, this.ClientSize.Height);
        }

        // Обработчик изменения размера формы
        private void MainForm_Resize(object sender, EventArgs e)
        {
            UpdateParametersPanelPosition();

            // Обновляем максимальные значения координат в соответствии с новой областью рисования
            var drawingArea = GetDrawingArea();
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
            if (selectedFigure is LineElement line)
            {
                parametersPanel.Visible = true;
                UpdateParametersPanelPosition(); // Обновляем позицию

                // Обновляем максимальные значения в соответствии с текущей областью рисования
                var drawingArea = GetDrawingArea();
                numStartX.Maximum = drawingArea.Width;
                numStartY.Maximum = drawingArea.Height;
                numEndX.Maximum = drawingArea.Width;
                numEndY.Maximum = drawingArea.Height;
                (float a, float b, float c, float d) = line.GetEquation();
                lblEquation.Text = $"Уравнение прямой {a}x+{b}y+{c}=0"; //для 3 д тут z использовать еще

                // Временно отключаем события чтобы избежать рекурсии
                numStartX.ValueChanged -= Parameters_ValueChanged;
                numStartY.ValueChanged -= Parameters_ValueChanged;
                numEndX.ValueChanged -= Parameters_ValueChanged;
                numEndY.ValueChanged -= Parameters_ValueChanged;
                numThickness.ValueChanged -= Parameters_ValueChanged;                
                numStartZ.ValueChanged -= Parameters_ValueChanged;
                numEndZ.ValueChanged -= Parameters_ValueChanged;

                numStartX.Value = (decimal)line.StartPoint.X;
                numStartY.Value = (decimal)line.StartPoint.Y;
                numEndX.Value = (decimal)line.EndPoint.X;
                numEndY.Value = (decimal)line.EndPoint.Y;
                numThickness.Value = (decimal)line.Thickness;
                if (line is LineElement3D line3D)
                {
                    numStartZ.Value = (decimal)line3D.StartZ;
                    numEndZ.Value = (decimal)line3D.EndZ;

                    lblStartZ.Visible = true;
                    lblEndZ.Visible = true;
                    numStartZ.Visible = true;
                    numEndZ.Visible = true;
                }
                else 
                {
                    lblStartZ.Visible = false;
                    lblEndZ.Visible = false;
                    numStartZ.Visible = false;
                    numEndZ.Visible = false;
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
        }

        private void Parameters_ValueChanged(object sender, EventArgs e)
        {
            if (selectedFigure is LineElement line && !isDragging && !isResizing)
            {
                //тут флаг если не 3 д, то 2 мерный поинт иначе - 3 мерный
                //а может и без флага просто брать это преропредленно е и все,
                //а из получиь уравнение для 2 д линииивообще убрать z
                line.StartPoint = new PointF((float)numStartX.Value, (float)numStartY.Value);
                line.EndPoint = new PointF((float)numEndX.Value, (float)numEndY.Value);
                line.Thickness = (float)numThickness.Value;
                //(float a, float b, float c, float d) = line.GetEquation();
                //lblEquation.Text = $"Уравнение прямой {a}x+{b}y+{c}=0"; //для 3 д тут z использовать еще
                if(line is LineElement3D line3D) 
                {
                    line3D.SetStartZ(GetDrawingArea(), groupManager, (float)numStartZ.Value);
                    line3D.SetEndZ(GetDrawingArea(), groupManager, (float)numEndZ.Value);
                }
                this.Invalidate();
            }
        }

        private void ComboColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedFigure is LineElement line)
            {

                line.Color = Color.FromName(comboColor.SelectedItem.ToString());
                this.Invalidate();
            }
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
            if (selectedFigure is LineElement line)
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
                    case "Проецирование":
                        ApplyProjection();
                        //месседж боксом получить на какую ось - тЧТО КОШМАР ВОБЩЕМ
                        break;
                }
                //масштабирование и вращение могут выкинуть линии за холст - исправить как делали с перемещением
                //UpdateParametersPanel(); //проследить чтобы обновлялось как надо!!!1 координаты выбраной линии после операции
                //либо вензде внурти этих метдв это напихать
                //вроде норм обновляется - проверял чуть 
                //this.Invalidate(); внутри методов делается, чтобы не после подтверждения сообщения отображжался результат
            }
        }

        private bool CanPerformRotation(float angle)
        {
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

        private bool CanPerformScaling(float scaleX, float scaleY)
        {
            var drawingArea = GetDrawingArea();

            foreach (var figure in selectedFigures)
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

                    if (testLine.EndPoint.X < 0 || testLine.EndPoint.X > drawingArea.Width
                        || testLine.EndPoint.Y < 0 || testLine.EndPoint.Y > drawingArea.Height
                        || testLine.StartPoint.X < 0 || testLine.StartPoint.X > drawingArea.Width
                        || testLine.StartPoint.Y < 0 || testLine.StartPoint.Y > drawingArea.Width)
                        return false;
                }
            }
            return true;
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
                        groupManager.MoveGroup(currentGroupId, delta, drawingArea.Height, drawingArea.Width);
                    }
                    else
                    {
                        foreach (var figure in selectedFigures)
                        {
                            figure.Move(delta, drawingArea.Height, drawingArea.Width);
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
                        if (!groupManager.RotateGroup(currentGroupId, angle, GetDrawingArea()))
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
                        if (!groupManager.ScaleGroup(currentGroupId, sx, sy, GetDrawingArea()))
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

        private void ApplyUniformScaling()
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
                        if (!groupManager.ScaleGroupAverage(currentGroupId, scale, GetDrawingArea()))
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

        private void ApplyMirroring() 
        {
            if (selectedFigures.Count != 2 & !(isGroupSelected && currentGroupId != null & selectedFigures.Count == groupManager.GetGroupElements(currentGroupId).Count + 1)) 
            {
                MessageBox.Show($"Ошибка: необходиом выбрать 2 прямые:\n1 - которую зеркалировать,\n2 - относительно которой зеркалировать", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //FigureElement figure = selectedFigures[0];
            if (selectedFigures.Last() is LineElement mirrorLine) 
            {
                // Получаем уравнение прямой для зеркалирования
                (float A, float B, float C, float tempZ) = mirrorLine.GetEquation(); //а может и без флага

                // Проверяем, не выйдут ли фигуры за границы после зеркалирования
                if (!CanPerformMirror(A, B, C))
                {
                    MessageBox.Show("Зеркалирование невозможно: изображение выйдет за границы экрана", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Выполняем зеркалирование
                if (isGroupSelected && currentGroupId != null)
                {
                    if (!groupManager.MirrorGroup(currentGroupId, A, B, C, GetDrawingArea()))
                    {
                        MessageBox.Show("Ошибка: зеркалиование выносит элементы за границы экрана", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    //foreach (var figure in selectedFigures)
                    //{

                        if (selectedFigures[0] is LineElement line)
                        {
                            line.Mirror(A, B, C);
                        }
                    //}
                }

                UpdateParametersPanel();
                this.Invalidate();

                MessageBox.Show("Зеркалирование выполнено успешно", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ApplyProjection() 
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите ось проецирования (x или y):",
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



        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            var drawingArea = GetDrawingArea();
            if (!drawingArea.Contains(e.Location))
                return;

            lastMousePos = e.Location;

            if (e.Button == MouseButtons.Left)
            {
                // ЕСЛИ НАЖАТ CTRL - множественное выделение
                if (Control.ModifierKeys == Keys.Control)
                {
                    FigureElement clickedFigure = null;
                    foreach (var figure in figures)
                    {
                        if (figure.ContainsPoint(e.Location))
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
                            selectedFigure = clickedFigure; //чтобы отображалась панель параметров при выборе линии в группе
                        }                                  //операции примененные к линии в группе будут применяться ко всей группе
                        else                    
                        {
                            clickedFigure.IsSelected = !clickedFigure.IsSelected;
                            if (clickedFigure.IsSelected && !selectedFigures.Contains(clickedFigure))
                                selectedFigures.Add(clickedFigure);
                            else if (!clickedFigure.IsSelected)
                                selectedFigures.Remove(clickedFigure);
                        }
                        UpdateGroupSelectionState();
                    }

                    UpdateParametersPanel();
                    this.Invalidate();
                }
                else // БЕЗ CTRL - обычное поведение
                {
                    // Сбрасываем ВСЕ состояния выделения
                    ClearSelection();

                    // Сначала проверяем клик по групповым маркерам
                    // Нафиг надо проверять  вообще, пусть не изменяется размер группы при растягивании за края ограничивающего группу прямоугольника
                    //foreach (var groupId in groupManager.GetAllGroupIds())
                    //{
                    //    var groupBbox = GetGroupBoundingBox(groupId);
                    //    float handleSize = 8f;

                    //    if (IsPointInHandle(e.Location, groupBbox.Left, groupBbox.Top, handleSize) ||
                    //        IsPointInHandle(e.Location, groupBbox.Right, groupBbox.Top, handleSize) ||
                    //        IsPointInHandle(e.Location, groupBbox.Left, groupBbox.Bottom, handleSize) ||
                    //        IsPointInHandle(e.Location, groupBbox.Right, groupBbox.Bottom, handleSize))
                    //    {
                    //        // Выделяем всю группу
                    //        var groupElements = groupManager.GetGroupElements(groupId);
                    //        foreach (var element in groupElements)
                    //        {
                    //            element.IsSelected = true;
                    //            selectedFigures.Add(element);
                    //        }
                    //        currentGroupId = groupId;
                    //        isGroupSelected = true;
                    //        isResizing = true;
                    //        UpdateParametersPanel(); //необхдимо определить все таки выбранную линию и для нее рисовать панель - а если операции будут применяться к линии в группе - то ко всей группе
                    //          //тут были спорности
                    //        this.Invalidate();
                    //        return;
                    //    }
                    //}

                    // Проверяем клик по элементам
                    FigureElement clickedFigure = null;
                    foreach (var figure in figures)
                    {
                        if (figure.ContainsPoint(e.Location))
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
                            selectedFigure = clickedFigure; //чтобы отображалась панель параметров при выборе линии в группе
                            //операции примененные к линии в группе будут применяться ко всей группе
                        }
                        else
                        {
                            // Одиночный элемент
                            clickedFigure.IsSelected = true;
                            selectedFigures.Add(clickedFigure);
                            selectedFigure = clickedFigure;

                            // ПРОВЕРЯЕМ КЛИК ПО МАРКЕРАМ ИЗМЕНЕНИЯ РАЗМЕРА ПЕРЕД установкой isDragging
                            if (clickedFigure is LineElement line)
                            {
                                float handleSize = 8f;
                                RectangleF startHandle = new RectangleF(
                                    line.StartPoint.X - handleSize / 2, line.StartPoint.Y - handleSize / 2,
                                    handleSize, handleSize);
                                RectangleF endHandle = new RectangleF(
                                    line.EndPoint.X - handleSize / 2, line.EndPoint.Y - handleSize / 2,
                                    handleSize, handleSize);

                                if (startHandle.Contains(e.Location))
                                {
                                    isResizing = true;
                                    resizeStartPoint = true;
                                    UpdateParametersPanel();
                                    this.Invalidate();
                                    return; // ВАЖНО: выходим, чтобы не установить isDragging
                                }
                                else if (endHandle.Contains(e.Location))
                                {
                                    isResizing = true;
                                    resizeStartPoint = false;
                                    UpdateParametersPanel();
                                    this.Invalidate();
                                    return; // ВАЖНО: выходим, чтобы не установить isDragging
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
                    drawingStartPoint = e.Location;
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
            //Возможно добавить изменение курсора если на крайние точки попали

            if (e.Button == MouseButtons.Left)
            {
                // Ограничиваем область рисования
                var drawingArea = GetDrawingArea();
                

                PointF delta = new PointF(e.X - lastMousePos.X, e.Y - lastMousePos.Y);

                if (isDragging)
                {
                    // Если есть группа - перемещаем группу
                    if (isGroupSelected && currentGroupId != null)
                    {
                        groupManager.MoveGroup(currentGroupId, delta, drawingArea.Height, drawingArea.Width);
                    }
                    // Если есть выделенные фигуры - перемещаем все выделенные
                    else if (selectedFigures.Count > 0)
                    {
                        foreach (var figure in selectedFigures)
                        {
                            selectedFigure.Move(delta, drawingArea.Height, drawingArea.Width);
                        }
                    }
                    // Иначе перемещаем одиночную фигуру
                    else if (selectedFigure != null)
                    {
                        selectedFigure.Move(delta, drawingArea.Height, drawingArea.Width); ;
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

                    PointF newPoint = e.Location;
                    newPoint.X = Math.Max(0, Math.Min(newPoint.X, drawingArea.Width));
                    newPoint.Y = Math.Max(0, Math.Min(newPoint.Y, drawingArea.Height));

                    if (resizeStartPoint)
                    {
                        // Меняем начальную точку
                        line.StartPoint = newPoint;
                    }
                    else
                    {
                        // Меняем конечную точку
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
                    this.Invalidate(); // Перерисовываем для отображения временной линии
                }
            }

            lastMousePos = e.Location;
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (isDrawing)
                {
                    // Создаем новую линию только если длина достаточная и в области рисования
                    var drawingArea = GetDrawingArea();
                    if (drawingArea.Contains(e.Location) && Distance(drawingStartPoint, e.Location) > 5)
                    {
                        PointF drawingEndPoint = e.Location;
                        drawingEndPoint.X = Math.Max(0, Math.Min(drawingEndPoint.X, drawingArea.Width));
                        drawingEndPoint.Y = Math.Max(0, Math.Min(drawingEndPoint.Y, drawingArea.Height));
                        FigureElement newLine;
                        if (is3DMode)
                        {
                            newLine = new LineElement3D(
                                drawingStartPoint, drawingEndPoint, Color.Black, 3f);
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
            }
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
                            line3D.SetStartZ(GetDrawingArea(), groupManager, line3D.StartZ + deltaZ);
                        }
                        else if (Control.ModifierKeys == (Keys.Control | Keys.Alt))
                        {
                            // Ctrl+Alt+Колесо - изменяем только конечную точку
                            line3D.SetEndZ(GetDrawingArea(), groupManager, line3D.EndZ + deltaZ);                          
                        }
                        else
                        {
                            // Просто Ctrl+Колесо - изменяем всю линию
                            line3D.SetStartZ(GetDrawingArea(), groupManager, line3D.StartZ + deltaZ);
                            line3D.SetEndZ(GetDrawingArea(), groupManager, line3D.EndZ + deltaZ);
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
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Очищаем только область рисования
            var drawingArea = GetDrawingArea();
            e.Graphics.FillRectangle(Brushes.White, drawingArea);

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
            }
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
                //selectedFigure = null;
                ClearSelection();
                parametersPanel.Visible = false;
                this.Invalidate();
            }
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
    }
}