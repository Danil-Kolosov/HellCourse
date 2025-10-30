using FamilyTree;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FamilyTree
{
    public partial class SearchPersonForm : Form
    {
        public Person SelectedPerson { get; private set; }
        private PersonRepository _personRepo;
        private List<Person> _searchResults;

        public SearchPersonForm(PersonRepository personRepo)
        {
            _personRepo = personRepo;
            InitializeComponent();
            _searchResults = new List<Person>();
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            lblSurname = new Label();
            txtSurname = new TextBox();
            lblName = new Label();
            txtName = new TextBox();
            lblLastName = new Label();
            txtLastName = new TextBox();
            btnSearch = new Button();
            dgvResults = new DataGridView();
            btnSelect = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            SuspendLayout();
            // 
            // lblSurname
            // 
            lblSurname.AutoSize = true;
            lblSurname.Location = new Point(12, 15);
            lblSurname.Name = "lblSurname";
            lblSurname.Size = new Size(61, 15);
            lblSurname.TabIndex = 0;
            lblSurname.Text = "Фамилия:";
            // 
            // txtSurname
            // 
            txtSurname.Location = new Point(80, 12);
            txtSurname.Name = "txtSurname";
            txtSurname.Size = new Size(120, 23);
            txtSurname.TabIndex = 0;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(210, 15);
            lblName.Name = "lblName";
            lblName.Size = new Size(34, 15);
            lblName.TabIndex = 1;
            lblName.Text = "Имя:";
            // 
            // txtName
            // 
            txtName.Location = new Point(250, 12);
            txtName.Name = "txtName";
            txtName.Size = new Size(120, 23);
            txtName.TabIndex = 1;
            // 
            // lblLastName
            // 
            lblLastName.AutoSize = true;
            lblLastName.Location = new Point(373, 15);
            lblLastName.Name = "lblLastName";
            lblLastName.Size = new Size(61, 15);
            lblLastName.TabIndex = 2;
            lblLastName.Text = "Отчество:";
            // 
            // txtLastName
            // 
            txtLastName.Location = new Point(440, 12);
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(120, 23);
            txtLastName.TabIndex = 2;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(570, 10);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 23);
            btnSearch.TabIndex = 3;
            btnSearch.Text = "Поиск";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // dgvResults
            // 
            dgvResults.AllowUserToAddRows = false;
            dgvResults.AllowUserToDeleteRows = false;
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResults.Location = new Point(12, 45);
            dgvResults.Name = "dgvResults";
            dgvResults.ReadOnly = true;
            dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResults.Size = new Size(633, 200);
            dgvResults.TabIndex = 4;
            dgvResults.CellDoubleClick += dgvResults_CellDoubleClick;
            // 
            // btnSelect
            // 
            btnSelect.Enabled = false;
            btnSelect.Location = new Point(489, 251);
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new Size(75, 23);
            btnSelect.TabIndex = 5;
            btnSelect.Text = "Выбрать";
            btnSelect.UseVisualStyleBackColor = true;
            btnSelect.Click += btnSelect_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(570, 251);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Отмена";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // SearchPersonForm
            // 
            AcceptButton = btnSearch;
            CancelButton = btnCancel;
            ClientSize = new Size(657, 286);
            Controls.Add(lblSurname);
            Controls.Add(txtSurname);
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblLastName);
            Controls.Add(txtLastName);
            Controls.Add(btnSearch);
            Controls.Add(dgvResults);
            Controls.Add(btnSelect);
            Controls.Add(btnCancel);
            //FormBorderStyle = FormBorderStyle.FixedDialog;
            //MaximizeBox = false;
            //MinimizeBox = false;
            Name = "SearchPersonForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Поиск персоны";
            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblSurname;
        private TextBox txtSurname;
        private Label lblName;
        private TextBox txtName;
        private Label lblLastName;
        private TextBox txtLastName;
        private Button btnSearch;
        private DataGridView dgvResults;
        private Button btnSelect;
        private Button btnCancel;

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                _searchResults = _personRepo.FindPerson(
                    string.IsNullOrWhiteSpace(txtSurname.Text) ? null : txtSurname.Text,
                    string.IsNullOrWhiteSpace(txtName.Text) ? null : txtName.Text,
                    string.IsNullOrWhiteSpace(txtLastName.Text) ? null : txtLastName.Text
                );

                dgvResults.DataSource = _searchResults.Select(p => new
                {
                    p.PersonId,
                    Фамилия = p.Surname,
                    Имя = p.Name,
                    Отчество = p.LastName,
                    Пол = p.GenderName,
                    Дата_рождения = p.BirthDate.ToString("dd.MM.yyyy"),
                    Дата_смерти = p.DeathDate?.ToString("dd.MM.yyyy") ?? "н/д",
                    Биография = p.Biography,
                    Последняя_дата_изменения = p.LastModifiedDate
                }).ToList();

                btnSelect.Enabled = _searchResults.Any();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvResults_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _searchResults.Count)
            {
                SelectedPerson = _searchResults[e.RowIndex];
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (dgvResults.CurrentRow?.Index >= 0 && dgvResults.CurrentRow.Index < _searchResults.Count)
            {
                SelectedPerson = _searchResults[dgvResults.CurrentRow.Index];
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void lblLastName_Click(object sender, EventArgs e)
        {

        }
    }
}