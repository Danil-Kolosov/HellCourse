using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GrafRedactor
{
    public partial class MainForm : Form
    {
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
        private NumericUpDown numStartX, numStartY, numEndX, numEndY;
        private NumericUpDown numThickness;
        private ComboBox comboColor;
        private ComboBox comboOperation;
        private Button btnDelete;
        private Label lblStartPoint, lblEndPoint, lblThickness, lblColor, lblOperations;

        // Константы для размеров
        private const int PARAMETERS_PANEL_WIDTH = 350;
        private const int MIN_DRAWING_WIDTH = 500;

        public MainForm()
        {
            InitializeComponent(); // Вызов метода из Designer
            InitializeCustomComponents(); // Своя инициализация
            selectedFigures = new List<FigureElement>();
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
        }

        private void InitializeParametersPanel()
        {
            parametersPanel = new Panel
            {
                Location = new Point(650, 0),
                Size = new Size(350, 600),
                BackColor = Color.LightGray,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right // Якоря для автоматического изменения размера
            };

            int y = 20;

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
            comboOperation.Items.AddRange(new object[] { "", "Смещение", "Вращение", "Масштабирование", "Общ масшт", "Зеркалирование", "Проецирование" });
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

                // Временно отключаем события чтобы избежать рекурсии
                numStartX.ValueChanged -= Parameters_ValueChanged;
                numStartY.ValueChanged -= Parameters_ValueChanged;
                numEndX.ValueChanged -= Parameters_ValueChanged;
                numEndY.ValueChanged -= Parameters_ValueChanged;
                numThickness.ValueChanged -= Parameters_ValueChanged;

                numStartX.Value = (decimal)line.StartPoint.X;
                numStartY.Value = (decimal)line.StartPoint.Y;
                numEndX.Value = (decimal)line.EndPoint.X;
                numEndY.Value = (decimal)line.EndPoint.Y;
                numThickness.Value = (decimal)line.Thickness;

                // Восстанавливаем обработчики
                numStartX.ValueChanged += Parameters_ValueChanged;
                numStartY.ValueChanged += Parameters_ValueChanged;
                numEndX.ValueChanged += Parameters_ValueChanged;
                numEndY.ValueChanged += Parameters_ValueChanged;
                numThickness.ValueChanged += Parameters_ValueChanged;

                comboColor.SelectedItem = line.Color.Name;
            }
        }

        private void Parameters_ValueChanged(object sender, EventArgs e)
        {
            if (selectedFigure is LineElement line && !isDragging && !isResizing)
            {
                line.StartPoint = new PointF((float)numStartX.Value, (float)numStartY.Value);
                line.EndPoint = new PointF((float)numEndX.Value, (float)numEndY.Value);
                line.Thickness = (float)numThickness.Value;
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

        private void СomboOperation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedFigure is LineElement line)
            {
                switch (comboOperation.SelectedItem.ToString()) 
                {
                    case "Смещение":
                        //месседж боксом получить на сколько по x по y
                        break;
                    case "Вращение":
                        //месседж боксом получить угол
                        break;
                    case "Масштабирование":
                        //месседж боксом получить на сколько по x по y
                        break;
                    case "Общ масшт":
                        //месседж боксом получить s
                        break;
                    case "Зеркалирование":
                        //месседж боксом уведомить выбрать линию относительно котоорой будет зеркалирование
                        break;
                    case "Проецирование":
                        //месседж боксом получить на какую ось - тЧТО КОШМАР ВОБЩЕМ
                        break;
                }
                this.Invalidate();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedFigure != null)
            {
                figures.Remove(selectedFigure);
                selectedFigure = null;
                parametersPanel.Visible = false;
                this.Invalidate();
            }
        }

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
                        clickedFigure.IsSelected = !clickedFigure.IsSelected;
                        if (clickedFigure.IsSelected && !selectedFigures.Contains(clickedFigure))
                            selectedFigures.Add(clickedFigure);
                        else if (!clickedFigure.IsSelected)
                            selectedFigures.Remove(clickedFigure);

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
                    foreach (var groupId in groupManager.GetAllGroupIds())
                    {
                        var groupBbox = GetGroupBoundingBox(groupId);
                        float handleSize = 8f;

                        if (IsPointInHandle(e.Location, groupBbox.Left, groupBbox.Top, handleSize) ||
                            IsPointInHandle(e.Location, groupBbox.Right, groupBbox.Top, handleSize) ||
                            IsPointInHandle(e.Location, groupBbox.Left, groupBbox.Bottom, handleSize) ||
                            IsPointInHandle(e.Location, groupBbox.Right, groupBbox.Bottom, handleSize))
                        {
                            // Выделяем всю группу
                            var groupElements = groupManager.GetGroupElements(groupId);
                            foreach (var element in groupElements)
                            {
                                element.IsSelected = true;
                                selectedFigures.Add(element);
                            }
                            currentGroupId = groupId;
                            isGroupSelected = true;
                            isResizing = true;
                            UpdateParametersPanel();
                            this.Invalidate();
                            return;
                        }
                    }

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
                        var newLine = new LineElement(drawingStartPoint, drawingEndPoint, Color.Black, 3f);
                        figures.Add(newLine);
                        // АВТОМАТИЧЕСКИ ВЫДЕЛЯЕМ новую линию и добавляем в selectedFigures
                        ClearSelection(); // Сначала сбрасываем старое выделение
                        selectedFigure = newLine;
                        selectedFigure.IsSelected = true;
                        UpdateParametersPanel();
                    }
                    isDrawing = false;
                    this.Invalidate();
                }

                isDragging = false;
                isResizing = false;
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
            if (isGroupSelected && currentGroupId != null)
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
            // Рисуем границу области рисования (опционально)
            using (Pen borderPen = new Pen(Color.LightGray, 1))
            {
                e.Graphics.DrawRectangle(borderPen, drawingArea.X, drawingArea.Y,
                                       drawingArea.Width - 1, drawingArea.Height - 1);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && selectedFigure != null)
            {
                figures.Remove(selectedFigure);
                selectedFigure = null;
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
                selectedFigures.Clear();
                isGroupSelected = false;
                currentGroupId = null;
                this.Invalidate();
            }
        }
    }
}