using System.Windows.Forms;

namespace FamilyTree
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem добавитьToolStripMenuItem;
        private ToolStripMenuItem добавитьПерсонуToolStripMenuItem;
        private ToolStripMenuItem добавитьСвязьToolStripMenuItem;
        private ToolStripMenuItem получитьToolStripMenuItem;
        private ToolStripMenuItem всеПерсоныToolStripMenuItem;
        private ToolStripMenuItem всеСвязиToolStripMenuItem;
        private ToolStripMenuItem найтиПерсонуToolStripMenuItem;
        private ToolStripMenuItem изменитьToolStripMenuItem;
        private ToolStripMenuItem изменитьПерсонуToolStripMenuItem;
        private ToolStripMenuItem удалитьToolStripMenuItem;
        private ToolStripMenuItem удалитьПерсонуToolStripMenuItem;
        private ToolStripMenuItem удалитьСвязьToolStripMenuItem;
        private DataGridView dgvPersons;
        private DataGridView dgvConnections;
        private TabControl tabControl1;
        private TabPage tabPagePersons;
        private TabPage tabPageConnections;
        private Button btnRefresh;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.добавитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.добавитьПерсонуToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.добавитьСвязьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.получитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.всеПерсоныToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.всеСвязиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.найтиПерсонуToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.изменитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.изменитьПерсонуToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьПерсонуToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьСвязьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPagePersons = new System.Windows.Forms.TabPage();
            this.dgvPersons = new System.Windows.Forms.DataGridView();
            this.tabPageConnections = new System.Windows.Forms.TabPage();
            this.dgvConnections = new System.Windows.Forms.DataGridView();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();

            this.menuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPagePersons.SuspendLayout();
            this.tabPageConnections.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();

            // menuStrip1
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.добавитьToolStripMenuItem,
            this.получитьToolStripMenuItem,
            this.изменитьToolStripMenuItem,
            this.удалитьToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(884, 24);
            this.menuStrip1.TabIndex = 0;

            // добавитьToolStripMenuItem
            this.добавитьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.добавитьПерсонуToolStripMenuItem,
            this.добавитьСвязьToolStripMenuItem});
            this.добавитьToolStripMenuItem.Name = "добавитьToolStripMenuItem";
            this.добавитьToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.добавитьToolStripMenuItem.Text = "Добавить";

            // добавитьПерсонуToolStripMenuItem
            this.добавитьПерсонуToolStripMenuItem.Name = "добавитьПерсонуToolStripMenuItem";
            this.добавитьПерсонуToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.добавитьПерсонуToolStripMenuItem.Text = "Персону";
            this.добавитьПерсонуToolStripMenuItem.Click += new System.EventHandler(this.добавитьПерсонуToolStripMenuItem_Click);

            // добавитьСвязьToolStripMenuItem
            this.добавитьСвязьToolStripMenuItem.Name = "добавитьСвязьToolStripMenuItem";
            this.добавитьСвязьToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.добавитьСвязьToolStripMenuItem.Text = "Родственную связь";
            this.добавитьСвязьToolStripMenuItem.Click += new System.EventHandler(this.добавитьСвязьToolStripMenuItem_Click);

            // получитьToolStripMenuItem
            this.получитьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.всеПерсоныToolStripMenuItem,
            this.всеСвязиToolStripMenuItem,
            this.найтиПерсонуToolStripMenuItem});
            this.получитьToolStripMenuItem.Name = "получитьToolStripMenuItem";
            this.получитьToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.получитьToolStripMenuItem.Text = "Получить";

            // всеПерсоныToolStripMenuItem
            this.всеПерсоныToolStripMenuItem.Name = "всеПерсоныToolStripMenuItem";
            this.всеПерсоныToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.всеПерсоныToolStripMenuItem.Text = "Всех персон";
            this.всеПерсоныToolStripMenuItem.Click += new System.EventHandler(this.всеПерсоныToolStripMenuItem_Click);

            // всеСвязиToolStripMenuItem
            this.всеСвязиToolStripMenuItem.Name = "всеСвязиToolStripMenuItem";
            this.всеСвязиToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.всеСвязиToolStripMenuItem.Text = "Все связи";
            this.всеСвязиToolStripMenuItem.Click += new System.EventHandler(this.всеСвязиToolStripMenuItem_Click);

            // найтиПерсонуToolStripMenuItem
            this.найтиПерсонуToolStripMenuItem.Name = "найтиПерсонуToolStripMenuItem";
            this.найтиПерсонуToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.найтиПерсонуToolStripMenuItem.Text = "Найти персону";
            this.найтиПерсонуToolStripMenuItem.Click += new System.EventHandler(this.найтиПерсонуToolStripMenuItem_Click);

            // изменитьToolStripMenuItem
            this.изменитьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.изменитьПерсонуToolStripMenuItem});
            this.изменитьToolStripMenuItem.Name = "изменитьToolStripMenuItem";
            this.изменитьToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.изменитьToolStripMenuItem.Text = "Изменить";

            // изменитьПерсонуToolStripMenuItem
            this.изменитьПерсонуToolStripMenuItem.Name = "изменитьПерсонуToolStripMenuItem";
            this.изменитьПерсонуToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.изменитьПерсонуToolStripMenuItem.Text = "Сведения о персоне";
            this.изменитьПерсонуToolStripMenuItem.Click += new System.EventHandler(this.изменитьПерсонуToolStripMenuItem_Click);

            // удалитьToolStripMenuItem
            this.удалитьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.удалитьПерсонуToolStripMenuItem,
            this.удалитьСвязьToolStripMenuItem});
            this.удалитьToolStripMenuItem.Name = "удалитьToolStripMenuItem";
            this.удалитьToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.удалитьToolStripMenuItem.Text = "Удалить";

            // удалитьПерсонуToolStripMenuItem
            this.удалитьПерсонуToolStripMenuItem.Name = "удалитьПерсонуToolStripMenuItem";
            this.удалитьПерсонуToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.удалитьПерсонуToolStripMenuItem.Text = "Персону";
            this.удалитьПерсонуToolStripMenuItem.Click += new System.EventHandler(this.удалитьПерсонуToolStripMenuItem_Click);

            // удалитьСвязьToolStripMenuItem
            this.удалитьСвязьToolStripMenuItem.Name = "удалитьСвязьToolStripMenuItem";
            this.удалитьСвязьToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.удалитьСвязьToolStripMenuItem.Text = "Связь";
            this.удалитьСвязьToolStripMenuItem.Click += new System.EventHandler(this.удалитьСвязьToolStripMenuItem_Click);

            // tabControl1
            this.tabControl1.Controls.Add(this.tabPagePersons);
            this.tabControl1.Controls.Add(this.tabPageConnections);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 24);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(884, 437);
            this.tabControl1.TabIndex = 1;

            // tabPagePersons
            this.tabPagePersons.Controls.Add(this.dgvPersons);
            this.tabPagePersons.Location = new System.Drawing.Point(4, 22);
            this.tabPagePersons.Name = "tabPagePersons";
            this.tabPagePersons.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePersons.Size = new System.Drawing.Size(876, 411);
            this.tabPagePersons.TabIndex = 0;
            this.tabPagePersons.Text = "Персоны";
            this.tabPagePersons.UseVisualStyleBackColor = true;

            // dgvPersons
            this.dgvPersons.AllowUserToAddRows = false;
            this.dgvPersons.AllowUserToDeleteRows = false;
            this.dgvPersons.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPersons.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPersons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPersons.Location = new System.Drawing.Point(3, 3);
            this.dgvPersons.Name = "dgvPersons";
            this.dgvPersons.ReadOnly = true;
            this.dgvPersons.Size = new System.Drawing.Size(870, 405);
            this.dgvPersons.TabIndex = 0;

            // tabPageConnections
            this.tabPageConnections.Controls.Add(this.dgvConnections);
            this.tabPageConnections.Location = new System.Drawing.Point(4, 22);
            this.tabPageConnections.Name = "tabPageConnections";
            this.tabPageConnections.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConnections.Size = new System.Drawing.Size(876, 411);
            this.tabPageConnections.TabIndex = 1;
            this.tabPageConnections.Text = "Родственные связи";
            this.tabPageConnections.UseVisualStyleBackColor = true;

            // dgvConnections
            this.dgvConnections.AllowUserToAddRows = false;
            this.dgvConnections.AllowUserToDeleteRows = false;
            this.dgvConnections.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvConnections.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvConnections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvConnections.Location = new System.Drawing.Point(3, 3);
            this.dgvConnections.Name = "dgvConnections";
            this.dgvConnections.ReadOnly = true;
            this.dgvConnections.Size = new System.Drawing.Size(870, 405);
            this.dgvConnections.TabIndex = 0;

            // btnRefresh
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(797, 0);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Обновить";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // statusStrip1
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 461);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(884, 22);
            this.statusStrip1.TabIndex = 3;

            // lblStatus
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(79, 17);
            this.lblStatus.Text = "Загрузка...";

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 483);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Генеалогическое дерево";
            this.Load += new System.EventHandler(this.MainForm_Load);

            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPagePersons.ResumeLayout(false);
            this.tabPageConnections.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}