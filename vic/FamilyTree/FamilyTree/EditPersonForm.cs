using FamilyTree;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FamilyTree
{
    public partial class EditPersonForm : Form
    {
        public Person UpdatedPerson { get; private set; }
        private Person _originalPerson;

        public EditPersonForm(Person person)
        {
            _originalPerson = person;
            InitializeComponent();
            LoadPersonData();
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
            lblGender = new Label();
            cmbGender = new ComboBox();
            lblBirthDate = new Label();
            dtpBirthDate = new DateTimePicker();
            lblDeathDate = new Label();
            dtpDeathDate = new DateTimePicker();
            chkHasDeathDate = new CheckBox();
            lblBiography = new Label();
            txtBiography = new TextBox();
            lblPersonId = new Label();
            txtPersonId = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblSurname
            // 
            lblSurname.AutoSize = true;
            lblSurname.Location = new Point(1, 44);
            lblSurname.Name = "lblSurname";
            lblSurname.Size = new Size(61, 15);
            lblSurname.TabIndex = 1;
            lblSurname.Text = "Фамилия:";
            // 
            // txtSurname
            // 
            txtSurname.Location = new Point(100, 38);
            txtSurname.Name = "txtSurname";
            txtSurname.Size = new Size(200, 23);
            txtSurname.TabIndex = 1;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(1, 70);
            lblName.Name = "lblName";
            lblName.Size = new Size(34, 15);
            lblName.TabIndex = 2;
            lblName.Text = "Имя:";
            // 
            // txtName
            // 
            txtName.Location = new Point(100, 64);
            txtName.Name = "txtName";
            txtName.Size = new Size(200, 23);
            txtName.TabIndex = 2;
            // 
            // lblLastName
            // 
            lblLastName.AutoSize = true;
            lblLastName.Location = new Point(1, 96);
            lblLastName.Name = "lblLastName";
            lblLastName.Size = new Size(61, 15);
            lblLastName.TabIndex = 3;
            lblLastName.Text = "Отчество:";
            // 
            // txtLastName
            // 
            txtLastName.Location = new Point(100, 90);
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(200, 23);
            txtLastName.TabIndex = 3;
            // 
            // lblGender
            // 
            lblGender.AutoSize = true;
            lblGender.Location = new Point(1, 122);
            lblGender.Name = "lblGender";
            lblGender.Size = new Size(33, 15);
            lblGender.TabIndex = 4;
            lblGender.Text = "Пол:";
            // 
            // cmbGender
            // 
            cmbGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGender.FormattingEnabled = true;
            cmbGender.Items.AddRange(new object[] { "Мужской", "Женский" });
            cmbGender.Location = new Point(100, 116);
            cmbGender.Name = "cmbGender";
            cmbGender.Size = new Size(200, 23);
            cmbGender.TabIndex = 4;
            // 
            // lblBirthDate
            // 
            lblBirthDate.AutoSize = true;
            lblBirthDate.Location = new Point(1, 149);
            lblBirthDate.Name = "lblBirthDate";
            lblBirthDate.Size = new Size(93, 15);
            lblBirthDate.TabIndex = 5;
            lblBirthDate.Text = "Дата рождения:";
            // 
            // dtpBirthDate
            // 
            dtpBirthDate.Location = new Point(100, 143);
            dtpBirthDate.Name = "dtpBirthDate";
            dtpBirthDate.Size = new Size(200, 23);
            dtpBirthDate.TabIndex = 5;
            // 
            // lblDeathDate
            // 
            lblDeathDate.AutoSize = true;
            lblDeathDate.Location = new Point(1, 175);
            lblDeathDate.Name = "lblDeathDate";
            lblDeathDate.Size = new Size(78, 15);
            lblDeathDate.TabIndex = 6;
            lblDeathDate.Text = "Дата смерти:";
            // 
            // dtpDeathDate
            // 
            dtpDeathDate.Enabled = false;
            dtpDeathDate.Location = new Point(100, 169);
            dtpDeathDate.Name = "dtpDeathDate";
            dtpDeathDate.Size = new Size(200, 23);
            dtpDeathDate.TabIndex = 7;
            // 
            // chkHasDeathDate
            // 
            chkHasDeathDate.AutoSize = true;
            chkHasDeathDate.Location = new Point(306, 171);
            chkHasDeathDate.Name = "chkHasDeathDate";
            chkHasDeathDate.Size = new Size(15, 14);
            chkHasDeathDate.TabIndex = 6;
            chkHasDeathDate.CheckedChanged += chkHasDeathDate_CheckedChanged;
            // 
            // lblBiography
            // 
            lblBiography.AutoSize = true;
            lblBiography.Location = new Point(1, 201);
            lblBiography.Name = "lblBiography";
            lblBiography.Size = new Size(71, 15);
            lblBiography.TabIndex = 8;
            lblBiography.Text = "Биография:";
            // 
            // txtBiography
            // 
            txtBiography.Location = new Point(100, 195);
            txtBiography.Multiline = true;
            txtBiography.Name = "txtBiography";
            txtBiography.Size = new Size(200, 60);
            txtBiography.TabIndex = 8;
            // 
            // lblPersonId
            // 
            lblPersonId.AutoSize = true;
            lblPersonId.Location = new Point(1, 18);
            lblPersonId.Name = "lblPersonId";
            lblPersonId.Size = new Size(21, 15);
            lblPersonId.TabIndex = 0;
            lblPersonId.Text = "ID:";
            // 
            // txtPersonId
            // 
            txtPersonId.BackColor = SystemColors.Control;
            txtPersonId.Location = new Point(100, 12);
            txtPersonId.Name = "txtPersonId";
            txtPersonId.ReadOnly = true;
            txtPersonId.Size = new Size(100, 23);
            txtPersonId.TabIndex = 0;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(144, 270);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 9;
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(225, 270);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "Отмена";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // EditPersonForm
            // 
            AcceptButton = btnSave;
            CancelButton = btnCancel;
            ClientSize = new Size(314, 305);
            Controls.Add(lblPersonId);
            Controls.Add(txtPersonId);
            Controls.Add(lblSurname);
            Controls.Add(txtSurname);
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblLastName);
            Controls.Add(txtLastName);
            Controls.Add(lblGender);
            Controls.Add(cmbGender);
            Controls.Add(lblBirthDate);
            Controls.Add(dtpBirthDate);
            Controls.Add(lblDeathDate);
            Controls.Add(dtpDeathDate);
            Controls.Add(chkHasDeathDate);
            Controls.Add(lblBiography);
            Controls.Add(txtBiography);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            //FormBorderStyle = FormBorderStyle.FixedDialog;
            //MaximizeBox = false;
            //MinimizeBox = false;
            Name = "EditPersonForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Редактирование персоны";
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblPersonId;
        private TextBox txtPersonId;
        private Label lblSurname;
        private TextBox txtSurname;
        private Label lblName;
        private TextBox txtName;
        private Label lblLastName;
        private TextBox txtLastName;
        private Label lblGender;
        private ComboBox cmbGender;
        private Label lblBirthDate;
        private DateTimePicker dtpBirthDate;
        private Label lblDeathDate;
        private DateTimePicker dtpDeathDate;
        private CheckBox chkHasDeathDate;
        private Label lblBiography;
        private TextBox txtBiography;
        private Button btnSave;
        private Button btnCancel;

        private void LoadPersonData()
        {
            txtPersonId.Text = _originalPerson.PersonId.ToString();
            txtSurname.Text = _originalPerson.Surname;
            txtName.Text = _originalPerson.Name;
            txtLastName.Text = _originalPerson.LastName;
            cmbGender.SelectedIndex = _originalPerson.GenderId - 1; // 1=Мужской, 2=Женский
            dtpBirthDate.Value = _originalPerson.BirthDate.ToDateTime(TimeOnly.MinValue);

            if (_originalPerson.DeathDate.HasValue)
            {
                chkHasDeathDate.Checked = true;
                dtpDeathDate.Value = _originalPerson.DeathDate.Value.ToDateTime(TimeOnly.MinValue);
            }
            else
            {
                chkHasDeathDate.Checked = false;
            }

            txtBiography.Text = _originalPerson.Biography;
        }

        private void chkHasDeathDate_CheckedChanged(object sender, EventArgs e)
        {
            dtpDeathDate.Enabled = chkHasDeathDate.Checked;
            if (!chkHasDeathDate.Checked)
            {
                dtpDeathDate.Value = DateTime.Today;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSurname.Text) ||
                string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                cmbGender.SelectedIndex == -1)
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                UpdatedPerson = new Person(                    
                    txtSurname.Text,
                    txtName.Text,
                    txtLastName.Text,
                    cmbGender.SelectedIndex + 1,
                    DateOnly.FromDateTime(dtpBirthDate.Value),
                    chkHasDeathDate.Checked ? DateOnly.FromDateTime(dtpDeathDate.Value) : null,
                    txtBiography.Text
                );
                UpdatedPerson.PersonId = _originalPerson.PersonId; // Сохраняем оригинальный ID

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления персоны: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}