using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FamilyTree
{
    public partial class SearchConnectionForm : Form
    {
        public FamilyConnection SelectedConnection { get; private set; }
        private FamilyConnectionRepository _connectionRepo;
        private PersonRepository _personRepo;
        private List<FamilyConnection> _searchResults;
        private bool _selectionMode;

        // Основные элементы управления
        private Label lblPersonId;
        private TextBox txtPersonId;
        private Label lblConnectionType;
        private ComboBox cmbConnectionType;
        private Button btnSearch;
        private DataGridView dgvResults;
        private Button btnSelect;
        private Button btnCancel;
        private Button btnSearchPerson;

        // Элементы для поиска цепочки
        private Label lblPerson1;
        private TextBox txtPerson1Id;
        private Button btnSearchPerson1;
        private Label lblPerson2;
        private TextBox txtPerson2Id;
        private Button btnSearchPerson2;
        private Button btnFindChain;

        // Элементы для поиска предков/потомков
        private Label lblTargetPerson;
        private TextBox txtTargetPersonId;
        private Button btnSearchTargetPerson;
        private Button btnFindAncestors;
        private Button btnFindDescendants;

        public SearchConnectionForm(FamilyConnectionRepository connectionRepo, PersonRepository personRepo, bool selectionMode = true)
        {
            _connectionRepo = connectionRepo;
            _personRepo = personRepo;
            _selectionMode = selectionMode;
            _searchResults = new List<FamilyConnection>();

            InitializeComponent();
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            LoadConnectionTypes();

            if (!_selectionMode)
            {
                this.Text = "Поиск связей";
                btnSelect.Visible = false;
                btnCancel.Text = "Закрыть";
            }
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            // Основные элементы
            lblPersonId = new Label();
            txtPersonId = new TextBox();
            lblConnectionType = new Label();
            cmbConnectionType = new ComboBox();
            btnSearch = new Button();
            dgvResults = new DataGridView();
            btnSelect = new Button();
            btnCancel = new Button();
            btnSearchPerson = new Button();

            // Элементы для поиска цепочки
            lblPerson1 = new Label();
            txtPerson1Id = new TextBox();
            btnSearchPerson1 = new Button();
            lblPerson2 = new Label();
            txtPerson2Id = new TextBox();
            btnSearchPerson2 = new Button();
            btnFindChain = new Button();

            // Элементы для поиска предков/потомков
            lblTargetPerson = new Label();
            txtTargetPersonId = new TextBox();
            btnSearchTargetPerson = new Button();
            btnFindAncestors = new Button();
            btnFindDescendants = new Button();

            SuspendLayout();

            // Основные элементы (без изменений)
            lblPersonId.AutoSize = true;
            lblPersonId.Location = new Point(12, 15);
            lblPersonId.Name = "lblPersonId";
            lblPersonId.Size = new Size(73, 15);
            lblPersonId.TabIndex = 0;
            lblPersonId.Text = "ID персоны:";

            txtPersonId.Location = new Point(95, 12);
            txtPersonId.Name = "txtPersonId";
            txtPersonId.Size = new Size(80, 23);
            txtPersonId.TabIndex = 1;

            btnSearchPerson.Location = new Point(181, 11);
            btnSearchPerson.Name = "btnSearchPerson";
            btnSearchPerson.Size = new Size(75, 23);
            btnSearchPerson.TabIndex = 2;
            btnSearchPerson.Text = "Поиск...";
            btnSearchPerson.UseVisualStyleBackColor = true;
            btnSearchPerson.Click += btnSearchPerson_Click;

            lblConnectionType.AutoSize = true;
            lblConnectionType.Location = new Point(262, 15);
            lblConnectionType.Name = "lblConnectionType";
            lblConnectionType.Size = new Size(63, 15);
            lblConnectionType.TabIndex = 3;
            lblConnectionType.Text = "Тип связи:";

            cmbConnectionType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbConnectionType.FormattingEnabled = true;
            cmbConnectionType.Location = new Point(331, 12);
            cmbConnectionType.Name = "cmbConnectionType";
            cmbConnectionType.Size = new Size(150, 23);
            cmbConnectionType.TabIndex = 4;
            cmbConnectionType.SelectedIndexChanged += cmbConnectionType_SelectedIndexChanged;

            btnSearch.Location = new Point(487, 11);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 23);
            btnSearch.TabIndex = 5;
            btnSearch.Text = "Поиск";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;

            dgvResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvResults.AllowUserToAddRows = false;
            dgvResults.AllowUserToDeleteRows = false;
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResults.Location = new Point(12, 100);
            dgvResults.Name = "dgvResults";
            dgvResults.ReadOnly = true;
            dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResults.Size = new Size(550, 220);
            dgvResults.TabIndex = 6;
            dgvResults.CellDoubleClick += dgvResults_CellDoubleClick;

            btnSelect.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSelect.Enabled = false;
            btnSelect.Location = new Point(406, 326);
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new Size(75, 23);
            btnSelect.TabIndex = 7;
            btnSelect.Text = "Выбрать";
            btnSelect.UseVisualStyleBackColor = true;
            btnSelect.Click += btnSelect_Click;

            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(487, 326);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Отмена";
            btnCancel.UseVisualStyleBackColor = true;

            // Элементы для поиска цепочки
            lblPerson1.AutoSize = true;
            lblPerson1.Location = new Point(12, 45);
            lblPerson1.Name = "lblPerson1";
            lblPerson1.Size = new Size(64, 15);
            lblPerson1.TabIndex = 9;
            lblPerson1.Text = "Персона 1:";
            lblPerson1.Visible = false;

            txtPerson1Id.Location = new Point(95, 42);
            txtPerson1Id.Name = "txtPerson1Id";
            txtPerson1Id.Size = new Size(80, 23);
            txtPerson1Id.TabIndex = 10;
            txtPerson1Id.Visible = false;

            btnSearchPerson1.Location = new Point(181, 41);
            btnSearchPerson1.Name = "btnSearchPerson1";
            btnSearchPerson1.Size = new Size(75, 23);
            btnSearchPerson1.TabIndex = 11;
            btnSearchPerson1.Text = "Выбрать...";
            btnSearchPerson1.UseVisualStyleBackColor = true;
            btnSearchPerson1.Visible = false;
            btnSearchPerson1.Click += btnSearchPerson1_Click;

            lblPerson2.AutoSize = true;
            lblPerson2.Location = new Point(262, 45);
            lblPerson2.Name = "lblPerson2";
            lblPerson2.Size = new Size(64, 15);
            lblPerson2.TabIndex = 12;
            lblPerson2.Text = "Персона 2:";
            lblPerson2.Visible = false;

            txtPerson2Id.Location = new Point(331, 42);
            txtPerson2Id.Name = "txtPerson2Id";
            txtPerson2Id.Size = new Size(80, 23);
            txtPerson2Id.TabIndex = 13;
            txtPerson2Id.Visible = false;

            btnSearchPerson2.Location = new Point(417, 41);
            btnSearchPerson2.Name = "btnSearchPerson2";
            btnSearchPerson2.Size = new Size(75, 23);
            btnSearchPerson2.TabIndex = 14;
            btnSearchPerson2.Text = "Выбрать...";
            btnSearchPerson2.UseVisualStyleBackColor = true;
            btnSearchPerson2.Visible = false;
            btnSearchPerson2.Click += btnSearchPerson2_Click;

            btnFindChain.Location = new Point(498, 41);
            btnFindChain.Name = "btnFindChain";
            btnFindChain.Size = new Size(64, 23);
            btnFindChain.TabIndex = 15;
            btnFindChain.Text = "Найти";
            btnFindChain.UseVisualStyleBackColor = true;
            btnFindChain.Visible = false;
            btnFindChain.Click += btnFindChain_Click;

            // Элементы для поиска предков/потомков
            lblTargetPerson.AutoSize = true;
            lblTargetPerson.Location = new Point(0, 15);
            lblTargetPerson.Name = "lblTargetPerson";
            lblTargetPerson.Size = new Size(77, 15);
            lblTargetPerson.TabIndex = 0;
            lblTargetPerson.Text = "Целевая персона:";
            lblTargetPerson.Visible = false;

            txtTargetPersonId.Location = new Point(95, 42);
            txtTargetPersonId.Name = "txtTargetPersonId";
            txtTargetPersonId.Size = new Size(80, 23);
            txtTargetPersonId.TabIndex = 17;
            txtTargetPersonId.Visible = false;

            btnSearchTargetPerson.Location = new Point(181, 41);
            btnSearchTargetPerson.Name = "btnSearchTargetPerson";
            btnSearchTargetPerson.Size = new Size(75, 23);
            btnSearchTargetPerson.TabIndex = 18;
            btnSearchTargetPerson.Text = "Выбрать...";
            btnSearchTargetPerson.UseVisualStyleBackColor = true;
            btnSearchTargetPerson.Visible = false;
            btnSearchTargetPerson.Click += btnSearchTargetPerson_Click;

            btnFindAncestors.Location = new Point(262, 41);
            btnFindAncestors.Name = "btnFindAncestors";
            btnFindAncestors.Size = new Size(120, 23);
            btnFindAncestors.TabIndex = 19;
            btnFindAncestors.Text = "Найти предков";
            btnFindAncestors.UseVisualStyleBackColor = true;
            btnFindAncestors.Visible = false;
            btnFindAncestors.Click += btnFindAncestors_Click;

            btnFindDescendants.Location = new Point(388, 41);
            btnFindDescendants.Name = "btnFindDescendants";
            btnFindDescendants.Size = new Size(120, 23);
            btnFindDescendants.TabIndex = 20;
            btnFindDescendants.Text = "Найти потомков";
            btnFindDescendants.UseVisualStyleBackColor = true;
            btnFindDescendants.Visible = false;
            btnFindDescendants.Click += btnFindDescendants_Click;

            // SearchConnectionForm
            AcceptButton = btnSearch;
            CancelButton = btnCancel;
            ClientSize = new Size(574, 361);
            Controls.Add(btnFindDescendants);
            Controls.Add(btnFindAncestors);
            Controls.Add(btnSearchTargetPerson);
            Controls.Add(txtTargetPersonId);
            Controls.Add(lblTargetPerson);
            Controls.Add(btnFindChain);
            Controls.Add(btnSearchPerson2);
            Controls.Add(txtPerson2Id);
            Controls.Add(lblPerson2);
            Controls.Add(btnSearchPerson1);
            Controls.Add(txtPerson1Id);
            Controls.Add(lblPerson1);
            Controls.Add(btnCancel);
            Controls.Add(btnSelect);
            Controls.Add(dgvResults);
            Controls.Add(btnSearch);
            Controls.Add(cmbConnectionType);
            Controls.Add(lblConnectionType);
            Controls.Add(btnSearchPerson);
            Controls.Add(txtPersonId);
            Controls.Add(lblPersonId);
            Name = "SearchConnectionForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Поиск связей";
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ResumeLayout(false);
            PerformLayout();
        }

        private void LoadConnectionTypes()
        {
            cmbConnectionType.Items.Clear();
            cmbConnectionType.Items.Add("Все типы");
            cmbConnectionType.Items.Add("Мужья-жены");
            cmbConnectionType.Items.Add("Родители-дети");
            cmbConnectionType.Items.Add("Братья-сестры");
            cmbConnectionType.Items.Add("Родственная цепочка");
            cmbConnectionType.Items.Add("Поиск предков");
            cmbConnectionType.Items.Add("Поиск потомков");
            cmbConnectionType.SelectedIndex = 0;
        }

        private void cmbConnectionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = cmbConnectionType.SelectedIndex;
            bool isChainSearch = selectedIndex == 4;
            bool isAncestorsSearch = selectedIndex == 5;
            bool isDescendantsSearch = selectedIndex == 6;

            // Сбрасываем видимость всех специализированных элементов
            lblPerson1.Visible = false;
            txtPerson1Id.Visible = false;
            btnSearchPerson1.Visible = false;
            lblPerson2.Visible = false;
            txtPerson2Id.Visible = false;
            btnSearchPerson2.Visible = false;
            btnFindChain.Visible = false;

            lblTargetPerson.Visible = false;
            txtTargetPersonId.Visible = false;
            btnSearchTargetPerson.Visible = false;
            btnFindAncestors.Visible = false;
            btnFindDescendants.Visible = false;

            // Показываем элементы в зависимости от выбранного типа
            if (isChainSearch)
            {
                lblPerson1.Visible = true;
                txtPerson1Id.Visible = true;
                btnSearchPerson1.Visible = true;
                lblPerson2.Visible = true;
                txtPerson2Id.Visible = true;
                btnSearchPerson2.Visible = true;
                btnFindChain.Visible = true;
            }
            else if (isAncestorsSearch || isDescendantsSearch)
            {
                lblTargetPerson.Visible = true;
                txtTargetPersonId.Visible = true;
                btnSearchTargetPerson.Visible = true;
                btnFindAncestors.Visible = isAncestorsSearch;
                btnFindDescendants.Visible = isDescendantsSearch;
            }

            // Показываем/скрываем элементы обычного поиска
            bool showStandardSearch = selectedIndex >= 0 && selectedIndex <= 3;
            lblPersonId.Visible = showStandardSearch;
            txtPersonId.Visible = showStandardSearch;
            btnSearchPerson.Visible = showStandardSearch;
            btnSearch.Visible = showStandardSearch;

            // Очищаем результаты при смене типа поиска
            if (isChainSearch || isAncestorsSearch || isDescendantsSearch)
            {
                _searchResults.Clear();
                dgvResults.DataSource = null;
                btnSelect.Enabled = false;
            }
        }

        private void btnSearchTargetPerson_Click(object sender, EventArgs e)
        {
            using (var searchForm = new SearchPersonForm(_personRepo))
            {
                if (searchForm.ShowDialog() == DialogResult.OK && searchForm.SelectedPerson != null)
                {
                    txtTargetPersonId.Text = searchForm.SelectedPerson.PersonId.ToString();
                }
            }
        }

        private void btnFindAncestors_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTargetPersonId.Text))
            {
                MessageBox.Show("Выберите персону для поиска предков", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtTargetPersonId.Text, out int personId))
            {
                MessageBox.Show("ID персоны должен быть числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var ancestors = _connectionRepo.GetAllAncestorsOfPerson(personId);

                if (ancestors.Any())
                {
                    _searchResults = ancestors;
                    DisplayAncestorsResults(ancestors, personId);
                    btnSelect.Enabled = _selectionMode;
                    this.Text = $"Поиск предков - Найдено {ancestors.Count} связей";
                }
                else
                {
                    MessageBox.Show("Предки для выбранной персоны не найдены", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _searchResults.Clear();
                    dgvResults.DataSource = null;
                    btnSelect.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска предков: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFindDescendants_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTargetPersonId.Text))
            {
                MessageBox.Show("Выберите персону для поиска потомков", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtTargetPersonId.Text, out int personId))
            {
                MessageBox.Show("ID персоны должен быть числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var descendants = _connectionRepo.GetAllDescendantsOfPerson(personId);

                if (descendants.Any())
                {
                    _searchResults = descendants;
                    DisplayDescendantsResults(descendants, personId);
                    btnSelect.Enabled = _selectionMode;
                    this.Text = $"Поиск потомков - Найдено {descendants.Count} связей";
                }
                else
                {
                    MessageBox.Show("Потомки для выбранной персоны не найдены", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _searchResults.Clear();
                    dgvResults.DataSource = null;
                    btnSelect.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска потомков: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayAncestorsResults(List<FamilyConnection> ancestors, int targetPersonId)
        {
            var displayData = new List<object>();
            int generation = 1;

            // Группируем связи по поколениям
            var connectionsByGeneration = GroupConnectionsByGeneration(ancestors, targetPersonId, true);

            foreach (var generationConnections in connectionsByGeneration)
            {
                foreach (var connection in generationConnections)
                {
                    var parent = _personRepo.GetPerson(connection.PersonId1);
                    var child = _personRepo.GetPerson(connection.PersonId2);

                    displayData.Add(new
                    {
                        Поколение = generation,
                        Предок = GetPersonFullName(parent),
                        Потомок = GetPersonFullName(child),
                        Тип_связи = "Родители-дети",
                        ID_предка = connection.PersonId1,
                        ID_потомка = connection.PersonId2
                    });
                }
                generation++;
            }

            dgvResults.DataSource = displayData;
            AdjustColumnsForGenealogy();
        }

        private void DisplayDescendantsResults(List<FamilyConnection> descendants, int targetPersonId)
        {
            var displayData = new List<object>();
            int generation = 1;

            // Группируем связи по поколениям
            var connectionsByGeneration = GroupConnectionsByGeneration(descendants, targetPersonId, false);

            foreach (var generationConnections in connectionsByGeneration)
            {
                foreach (var connection in generationConnections)
                {
                    var parent = _personRepo.GetPerson(connection.PersonId1);
                    var child = _personRepo.GetPerson(connection.PersonId2);

                    displayData.Add(new
                    {
                        Поколение = generation,
                        Родитель = GetPersonFullName(parent),
                        Потомок = GetPersonFullName(child),
                        Тип_связи = "Родители-дети",
                        ID_родителя = connection.PersonId1,
                        ID_потомка = connection.PersonId2
                    });
                }
                generation++;
            }

            dgvResults.DataSource = displayData;
            AdjustColumnsForGenealogy();
        }

        private List<List<FamilyConnection>> GroupConnectionsByGeneration(List<FamilyConnection> connections, int startPersonId, bool isAncestors)
        {
            var result = new List<List<FamilyConnection>>();
            var currentLevel = new HashSet<int> { startPersonId };
            var visited = new HashSet<int> { startPersonId };

            while (currentLevel.Any())
            {
                var levelConnections = new List<FamilyConnection>();
                var nextLevel = new HashSet<int>();

                foreach (var personId in currentLevel)
                {
                    var personConnections = connections.Where(c =>
                        isAncestors ? c.PersonId2 == personId : c.PersonId1 == personId);

                    foreach (var connection in personConnections)
                    {
                        var relatedPersonId = isAncestors ? connection.PersonId1 : connection.PersonId2;

                        if (!visited.Contains(relatedPersonId))
                        {
                            levelConnections.Add(connection);
                            nextLevel.Add(relatedPersonId);
                            visited.Add(relatedPersonId);
                        }
                    }
                }

                if (levelConnections.Any())
                {
                    result.Add(levelConnections);
                }

                currentLevel = nextLevel;
            }

            return result;
        }

        private void AdjustColumnsForGenealogy()
        {
            if (dgvResults.Columns.Count > 0)
            {
                dgvResults.Columns["Поколение"].Width = 70;
                dgvResults.Columns["Тип_связи"].Width = 120;
                // Остальные колонки автоматически распределят пространство
            }
        }

        private void btnSearchPerson_Click(object sender, EventArgs e)
        {
            using (var searchForm = new SearchPersonForm(_personRepo))
            {
                if (searchForm.ShowDialog() == DialogResult.OK && searchForm.SelectedPerson != null)
                {
                    txtPersonId.Text = searchForm.SelectedPerson.PersonId.ToString();
                }
            }
        }

        private void btnSearchPerson1_Click(object sender, EventArgs e)
        {
            using (var searchForm = new SearchPersonForm(_personRepo))
            {
                if (searchForm.ShowDialog() == DialogResult.OK && searchForm.SelectedPerson != null)
                {
                    txtPerson1Id.Text = searchForm.SelectedPerson.PersonId.ToString();
                }
            }
        }

        private void btnSearchPerson2_Click(object sender, EventArgs e)
        {
            using (var searchForm = new SearchPersonForm(_personRepo))
            {
                if (searchForm.ShowDialog() == DialogResult.OK && searchForm.SelectedPerson != null)
                {
                    txtPerson2Id.Text = searchForm.SelectedPerson.PersonId.ToString();
                }
            }
        }

        private void btnFindChain_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPerson1Id.Text) || string.IsNullOrWhiteSpace(txtPerson2Id.Text))
            {
                MessageBox.Show("Выберите обе персоны для поиска родственной цепочки", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtPerson1Id.Text, out int personId1) || !int.TryParse(txtPerson2Id.Text, out int personId2))
            {
                MessageBox.Show("ID персон должны быть числами", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (personId1 == personId2)
            {
                MessageBox.Show("Выберите разных персон для поиска цепочки", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Получаем цепочку связей
                var chain = _connectionRepo.GetChainOfFamilyConnections(personId1, personId2);

                if (chain != null && chain.Any())
                {
                    _searchResults = chain;
                    DisplayChainResults(chain);
                    btnSelect.Enabled = _selectionMode;
                    this.Text = $"Поиск связей - Найдена цепочка из {chain.Count} связей";
                }
                else
                {
                    MessageBox.Show("Родственная цепочка между выбранными персонами не найдена", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _searchResults.Clear();
                    dgvResults.DataSource = null;
                    btnSelect.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска цепочки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayChainResults(List<FamilyConnection> chain)
        {
            var displayData = new List<object>();
            int step = 1;

            foreach (var connection in chain)
            {
                var person1 = _personRepo.GetPerson(connection.PersonId1);
                var person2 = _personRepo.GetPerson(connection.PersonId2);

                displayData.Add(new
                {
                    Шаг = step++,
                    ID1 = connection.PersonId1,
                    Персона1 = GetPersonFullName(person1),
                    ID2 = connection.PersonId2,
                    Персона2 = GetPersonFullName(person2),
                    Тип_связи = ConnectionType.GetConnectionTypeName(connection.ConnectionTypeId),
                    Описание = GetConnectionDescription(connection, person1, person2)
                });
            }

            dgvResults.DataSource = displayData;

            // Настраиваем ширину столбцов
            if (dgvResults.Columns.Count > 0)
            {
                dgvResults.AutoResizeColumns();
                //dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                //dgvResults.Columns["Шаг"].Width = 50;
                //dgvResults.Columns["ID1"].Width = 60;
                //dgvResults.Columns["Персона1"].Width = 150;
                //dgvResults.Columns["ID2"].Width = 60;
                //dgvResults.Columns["Персона2"].Width = 150;
                //dgvResults.Columns["Тип_связи"].Width = 120;
                //dgvResults.Columns["Описание"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private string GetPersonFullName(Person person)
        {
            return person != null ? $"{person.Surname} {person.Name} {person.LastName}" : "Не найдено";
        }

        private string GetConnectionDescription(FamilyConnection connection, Person person1, Person person2)
        {
            if (person1 == null || person2 == null)
                return "Ошибка: персона не найдена";

            switch (connection.ConnectionTypeId)
            {
                case 1: // Мужья-жены
                    return $"{person1.Surname} {person1.Name} является супругом/ой {person2.Surname} {person2.Name}";
                case 2: // Родители-дети
                    return $"{person1.Surname} {person1.Name} является родителем {person2.Surname} {person2.Name}";
                case 3: // Братья-сестры
                    return $"{person1.Surname} {person1.Name} является братом/сестрой {person2.Surname} {person2.Name}";
                default:
                    return $"{person1.Surname} {person1.Name} связан(а) с {person2.Surname} {person2.Name}";
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                int? personId = null;
                if (!string.IsNullOrWhiteSpace(txtPersonId.Text) && int.TryParse(txtPersonId.Text, out int id))
                {
                    personId = id;
                }

                int? connectionTypeId = null;
                if (cmbConnectionType.SelectedIndex > 0 && cmbConnectionType.SelectedIndex < 4)
                {
                    connectionTypeId = cmbConnectionType.SelectedIndex;
                }

                if (personId.HasValue)
                {
                    if (connectionTypeId.HasValue)
                    {
                        _searchResults = _connectionRepo.FindConnectionsByPersonType(personId.Value, connectionTypeId.Value);
                    }
                    else
                    {
                        _searchResults = _connectionRepo.FindConnectionsByPerson(personId.Value);
                    }
                }
                else
                {
                    _searchResults = _connectionRepo.GetAllConnections();
                }

                DisplayResults();
                btnSelect.Enabled = _searchResults.Any() && _selectionMode;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayResults()
        {
            var displayData = _searchResults.Select(c => new
            {
                ID1 = c.PersonId1,
                Персона1 = GetPersonFullName(_personRepo.GetPerson(c.PersonId1)),
                ID2 = c.PersonId2,
                Персона2 = GetPersonFullName(_personRepo.GetPerson(c.PersonId2)),
                Тип_связи = ConnectionType.GetConnectionTypeName(c.ConnectionTypeId)
            }).ToList();

            dgvResults.DataSource = displayData;
        }

        private void dgvResults_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _searchResults.Count)
            {
                SelectedConnection = _searchResults[e.RowIndex];
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (dgvResults.CurrentRow?.Index >= 0 && dgvResults.CurrentRow.Index < _searchResults.Count)
            {
                SelectedConnection = _searchResults[dgvResults.CurrentRow.Index];
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}