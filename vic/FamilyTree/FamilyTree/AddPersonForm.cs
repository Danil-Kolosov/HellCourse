using FamilyTree;
using System;
using System.Windows.Forms;

namespace FamilyTree
{
    public partial class AddPersonForm : Form
    {
        public Person Person { get; private set; }

        public AddPersonForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
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
            btnSave = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // lblSurname
            lblSurname.AutoSize = true;
            lblSurname.Location = new Point(12, 15);
            lblSurname.Name = "lblSurname";
            lblSurname.Size = new Size(59, 13);
            lblSurname.Text = "Фамилия:";
            // txtSurname
            txtSurname.Location = new Point(100, 12);
            txtSurname.Name = "txtSurname";
            txtSurname.Size = new Size(200, 20);
            txtSurname.TabIndex = 0;
            // lblName
            lblName.AutoSize = true;
            lblName.Location = new Point(12, 41);
            lblName.Name = "lblName";
            lblName.Size = new Size(32, 13);
            lblName.Text = "Имя:";
            // txtName
            txtName.Location = new Point(100, 38);
            txtName.Name = "txtName";
            txtName.Size = new Size(200, 20);
            txtName.TabIndex = 1;
            // lblLastName
            lblLastName.AutoSize = true;
            lblLastName.Location = new Point(12, 67);
            lblLastName.Name = "lblLastName";
            lblLastName.Size = new Size(57, 13);
            lblLastName.Text = "Отчество:";
            // txtLastName
            txtLastName.Location = new Point(100, 64);
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(200, 20);
            txtLastName.TabIndex = 2;
            // lblGender
            lblGender.AutoSize = true;
            lblGender.Location = new Point(12, 93);
            lblGender.Name = "lblGender";
            lblGender.Size = new Size(30, 13);
            lblGender.Text = "Пол:";
            // cmbGender
            cmbGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGender.FormattingEnabled = true;
            cmbGender.Items.AddRange(new object[] { "Мужской", "Женский" });
            cmbGender.Location = new Point(100, 90);
            cmbGender.Name = "cmbGender";
            cmbGender.Size = new Size(200, 21);
            cmbGender.TabIndex = 3;
            // lblBirthDate
            lblBirthDate.AutoSize = true;
            lblBirthDate.Location = new Point(12, 120);
            lblBirthDate.Name = "lblBirthDate";
            lblBirthDate.Size = new Size(89, 13);
            lblBirthDate.Text = "Дата рождения:";
            // dtpBirthDate
            dtpBirthDate.Location = new Point(100, 117);
            dtpBirthDate.Name = "dtpBirthDate";
            dtpBirthDate.Size = new Size(200, 20);
            dtpBirthDate.TabIndex = 4;
            // lblDeathDate
            lblDeathDate.AutoSize = true;
            lblDeathDate.Location = new Point(12, 146);
            lblDeathDate.Name = "lblDeathDate";
            lblDeathDate.Size = new Size(70, 13);
            lblDeathDate.Text = "Дата смерти:";
            // dtpDeathDate
            dtpDeathDate.Enabled = false;
            dtpDeathDate.Location = new Point(100, 143);
            dtpDeathDate.Name = "dtpDeathDate";
            dtpDeathDate.Size = new Size(200, 20);
            dtpDeathDate.TabIndex = 6;
            // chkHasDeathDate
            chkHasDeathDate.AutoSize = true;
            chkHasDeathDate.Location = new Point(306, 145);
            chkHasDeathDate.Name = "chkHasDeathDate";
            chkHasDeathDate.Size = new Size(15, 14);
            chkHasDeathDate.TabIndex = 5;
            chkHasDeathDate.CheckedChanged += chkHasDeathDate_CheckedChanged;
            // lblBiography
            lblBiography.AutoSize = true;
            lblBiography.Location = new Point(12, 172);
            lblBiography.Name = "lblBiography";
            lblBiography.Size = new Size(59, 13);
            lblBiography.Text = "Биография:";
            // txtBiography
            txtBiography.Location = new Point(100, 169);
            txtBiography.Multiline = true;
            txtBiography.Name = "txtBiography";
            txtBiography.Size = new Size(200, 60);
            txtBiography.TabIndex = 7;            
            // btnSave
            btnSave.Location = new Point(144, 273);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 9;
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // btnCancel
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(225, 273);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "Отмена";
            btnCancel.UseVisualStyleBackColor = true;
            // AddPersonForm
            AcceptButton = btnSave;
            CancelButton = btnCancel;
            ClientSize = new Size(314, 308);
            Controls.AddRange(new Control[] { lblSurname, txtSurname, lblName, txtName, lblLastName, txtLastName, lblGender, cmbGender, lblBirthDate, dtpBirthDate, lblDeathDate, dtpDeathDate, chkHasDeathDate, lblBiography, txtBiography, btnSave, btnCancel });
            //FormBorderStyle = FormBorderStyle.FixedDialog;
            //MaximizeBox = false;
            //MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Добавить персону";
            ResumeLayout(false);
            PerformLayout();
        }

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

        private void chkHasDeathDate_CheckedChanged(object sender, EventArgs e)
        {
            dtpDeathDate.Enabled = chkHasDeathDate.Checked;
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
                Person = new Person(
                    txtSurname.Text,
                    txtName.Text,
                    txtLastName.Text,
                    cmbGender.SelectedIndex + 1, // 1=Мужской, 2=Женский
                    DateOnly.FromDateTime(dtpBirthDate.Value),
                    chkHasDeathDate.Checked ? DateOnly.FromDateTime(dtpDeathDate.Value) : null,
                    txtBiography.Text
                );

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания персоны: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}