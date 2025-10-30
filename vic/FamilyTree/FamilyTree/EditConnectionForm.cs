using FamilyTree;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GenealogySystem
{
    public partial class EditConnectionForm : Form
    {
        public FamilyConnection UpdatedConnection { get; private set; }
        private FamilyConnection _originalConnection;
        private PersonRepository _personRepo;
        private Person _selectedPerson1;
        private Person _selectedPerson2;

        public EditConnectionForm(FamilyConnection connection, PersonRepository personRepo)
        {
            _originalConnection = connection;
            _personRepo = personRepo;
            InitializeComponent();
            LoadConnectionData();
            LoadPersonsComboBox();
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.lblPerson1 = new Label();
            this.cmbPerson1 = new ComboBox();
            this.lblPerson2 = new Label();
            this.cmbPerson2 = new ComboBox();
            this.lblConnectionType = new Label();
            this.cmbConnectionType = new ComboBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.lblPerson1Info = new Label();
            this.lblPerson2Info = new Label();
            this.btnSearchPerson1 = new Button();
            this.btnSearchPerson2 = new Button();
            this.lblConnectionId = new Label();
            this.lblConnectionIdValue = new Label();

            this.SuspendLayout();

            // lblConnectionId
            this.lblConnectionId.AutoSize = true;
            this.lblConnectionId.Location = new System.Drawing.Point(12, 15);
            this.lblConnectionId.Name = "lblConnectionId";
            this.lblConnectionId.Size = new System.Drawing.Size(64, 13);
            this.lblConnectionId.Text = "ID связи:";

            // lblConnectionIdValue
            this.lblConnectionIdValue.AutoSize = true;
            this.lblConnectionIdValue.Location = new System.Drawing.Point(100, 15);
            this.lblConnectionIdValue.Name = "lblConnectionIdValue";
            this.lblConnectionIdValue.Size = new System.Drawing.Size(0, 13);
            this.lblConnectionIdValue.Text = "";

            // lblPerson1
            this.lblPerson1.AutoSize = true;
            this.lblPerson1.Location = new System.Drawing.Point(12, 45);
            this.lblPerson1.Name = "lblPerson1";
            this.lblPerson1.Size = new System.Drawing.Size(63, 13);
            this.lblPerson1.Text = "Персона 1:";

            // cmbPerson1
            this.cmbPerson1.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbPerson1.FormattingEnabled = true;
            this.cmbPerson1.Location = new System.Drawing.Point(100, 42);
            this.cmbPerson1.Name = "cmbPerson1";
            this.cmbPerson1.Size = new System.Drawing.Size(150, 21);
            this.cmbPerson1.TabIndex = 1;
            this.cmbPerson1.SelectedIndexChanged += new EventHandler(this.cmbPerson1_SelectedIndexChanged);

            // btnSearchPerson1
            this.btnSearchPerson1.Location = new System.Drawing.Point(256, 41);
            this.btnSearchPerson1.Name = "btnSearchPerson1";
            this.btnSearchPerson1.Size = new System.Drawing.Size(75, 23);
            this.btnSearchPerson1.TabIndex = 2;
            this.btnSearchPerson1.Text = "Поиск...";
            this.btnSearchPerson1.UseVisualStyleBackColor = true;
            this.btnSearchPerson1.Click += new EventHandler(this.btnSearchPerson1_Click);

            // lblPerson1Info
            this.lblPerson1Info.AutoSize = true;
            this.lblPerson1Info.ForeColor = SystemColors.GrayText;
            this.lblPerson1Info.Location = new System.Drawing.Point(97, 66);
            this.lblPerson1Info.Name = "lblPerson1Info";
            this.lblPerson1Info.Size = new System.Drawing.Size(0, 13);
            this.lblPerson1Info.Text = "";

            // lblPerson2
            this.lblPerson2.AutoSize = true;
            this.lblPerson2.Location = new System.Drawing.Point(12, 95);
            this.lblPerson2.Name = "lblPerson2";
            this.lblPerson2.Size = new System.Drawing.Size(63, 13);
            this.lblPerson2.Text = "Персона 2:";

            // cmbPerson2
            this.cmbPerson2.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbPerson2.FormattingEnabled = true;
            this.cmbPerson2.Location = new System.Drawing.Point(100, 92);
            this.cmbPerson2.Name = "cmbPerson2";
            this.cmbPerson2.Size = new System.Drawing.Size(150, 21);
            this.cmbPerson2.TabIndex = 3;
            this.cmbPerson2.SelectedIndexChanged += new EventHandler(this.cmbPerson2_SelectedIndexChanged);

            // btnSearchPerson2
            this.btnSearchPerson2.Location = new System.Drawing.Point(256, 91);
            this.btnSearchPerson2.Name = "btnSearchPerson2";
            this.btnSearchPerson2.Size = new System.Drawing.Size(75, 23);
            this.btnSearchPerson2.TabIndex = 4;
            this.btnSearchPerson2.Text = "Поиск...";
            this.btnSearchPerson2.UseVisualStyleBackColor = true;
            this.btnSearchPerson2.Click += new EventHandler(this.btnSearchPerson2_Click);

            // lblPerson2Info
            this.lblPerson2Info.AutoSize = true;
            this.lblPerson2Info.ForeColor = SystemColors.GrayText;
            this.lblPerson2Info.Location = new System.Drawing.Point(97, 116);
            this.lblPerson2Info.Name = "lblPerson2Info";
            this.lblPerson2Info.Size = new System.Drawing.Size(0, 13);
            this.lblPerson2Info.Text = "";

            // lblConnectionType
            this.lblConnectionType.AutoSize = true;
            this.lblConnectionType.Location = new System.Drawing.Point(12, 145);
            this.lblConnectionType.Name = "lblConnectionType";
            this.lblConnectionType.Size = new System.Drawing.Size(68, 13);
            this.lblConnectionType.Text = "Тип связи:";

            // cmbConnectionType
            this.cmbConnectionType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbConnectionType.FormattingEnabled = true;
            this.cmbConnectionType.Items.AddRange(new object[] {
                "Мужья-жены",
                "Родители-дети",
                "Братья-сестры"
            });
            this.cmbConnectionType.Location = new System.Drawing.Point(100, 142);
            this.cmbConnectionType.Name = "cmbConnectionType";
            this.cmbConnectionType.Size = new System.Drawing.Size(150, 21);
            this.cmbConnectionType.TabIndex = 5;

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(100, 180);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);

            // btnCancel
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(181, 180);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;

            // EditConnectionForm
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(343, 215);
            this.Controls.AddRange(new Control[] {
                this.lblConnectionId, this.lblConnectionIdValue,
                this.lblPerson1, this.cmbPerson1, this.btnSearchPerson1, this.lblPerson1Info,
                this.lblPerson2, this.cmbPerson2, this.btnSearchPerson2, this.lblPerson2Info,
                this.lblConnectionType, this.cmbConnectionType,
                this.btnSave, this.btnCancel
            });
            //this.FormBorderStyle = FormBorderStyle.FixedDialog;
            //this.MaximizeBox = false;
            //this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Редактирование родственной связи";

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Label lblConnectionId;
        private Label lblConnectionIdValue;
        private Label lblPerson1;
        private ComboBox cmbPerson1;
        private Label lblPerson2;
        private ComboBox cmbPerson2;
        private Label lblConnectionType;
        private ComboBox cmbConnectionType;
        private Button btnSave;
        private Button btnCancel;
        private Label lblPerson1Info;
        private Label lblPerson2Info;
        private Button btnSearchPerson1;
        private Button btnSearchPerson2;

        private void LoadConnectionData()
        {
            // Создаем уникальный идентификатор связи для отображения
            lblConnectionIdValue.Text = $"{_originalConnection.PersonId1}-{_originalConnection.PersonId2}-{_originalConnection.ConnectionTypeId}";

            // Устанавливаем тип связи
            cmbConnectionType.SelectedIndex = _originalConnection.ConnectionTypeId - 1;
        }

        private void LoadPersonsComboBox()
        {
            var persons = _personRepo.GetAllPersons();
            cmbPerson1.Items.Clear();
            cmbPerson2.Items.Clear();

            foreach (var person in persons)
            {
                cmbPerson1.Items.Add(new PersonComboBoxItem(person));
                cmbPerson2.Items.Add(new PersonComboBoxItem(person));
            }

            // Выбираем оригинальных персон
            SelectPersonInComboBox(cmbPerson1, _originalConnection.PersonId1);
            SelectPersonInComboBox(cmbPerson2, _originalConnection.PersonId2);
        }

        private void SelectPersonInComboBox(ComboBox comboBox, int personId)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is PersonComboBoxItem item && item.Person.PersonId == personId)
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void cmbPerson1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPerson1.SelectedItem is PersonComboBoxItem item)
            {
                _selectedPerson1 = item.Person;
                lblPerson1Info.Text = $"{item.Person.GenderName}, {item.Person.BirthDate:dd.MM.yyyy}";
            }
        }

        private void cmbPerson2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPerson2.SelectedItem is PersonComboBoxItem item)
            {
                _selectedPerson2 = item.Person;
                lblPerson2Info.Text = $"{item.Person.GenderName}, {item.Person.BirthDate:dd.MM.yyyy}";
            }
        }

        private void btnSearchPerson1_Click(object sender, EventArgs e)
        {
            using (var searchForm = new SearchPersonForm(_personRepo))
            {
                if (searchForm.ShowDialog() == DialogResult.OK && searchForm.SelectedPerson != null)
                {
                    SelectPersonInComboBox(cmbPerson1, searchForm.SelectedPerson.PersonId);
                }
            }
        }

        private void btnSearchPerson2_Click(object sender, EventArgs e)
        {
            using (var searchForm = new SearchPersonForm(_personRepo))
            {
                if (searchForm.ShowDialog() == DialogResult.OK && searchForm.SelectedPerson != null)
                {
                    SelectPersonInComboBox(cmbPerson2, searchForm.SelectedPerson.PersonId);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_selectedPerson1 == null || _selectedPerson2 == null || cmbConnectionType.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите обе персоны и тип связи", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_selectedPerson1.PersonId == _selectedPerson2.PersonId)
            {
                MessageBox.Show("Нельзя создать связь человека с самим собой", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                int connectionTypeId = cmbConnectionType.SelectedIndex + 1;

                UpdatedConnection = new FamilyConnection(
                    _selectedPerson1.PersonId,
                    _selectedPerson2.PersonId,
                    connectionTypeId
                );

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления связи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Вспомогательный класс для комбобокса
        private class PersonComboBoxItem
        {
            public Person Person { get; }

            public PersonComboBoxItem(Person person)
            {
                Person = person;
            }

            public override string ToString()
            {
                return $"{Person.PersonId}: {Person.Surname} {Person.Name} {Person.LastName}";
            }
        }
    }
}