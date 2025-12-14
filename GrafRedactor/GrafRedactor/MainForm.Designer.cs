namespace GrafRedactor
{
        partial class MainForm
        {
            /// <summary>
            /// Required designer variable.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem modeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mode2DToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mode3DToolStripMenuItem;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Windows Form Designer generated code

            /// <summary>
            /// Required method for Designer support - do not modify
            /// the contents of this method with the code editor.
            /// </summary>
            private void InitializeComponent()
            {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.modeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mode2DToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mode3DToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.дФигураToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.кубToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.плоскостьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.yOzToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.yOzToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.xOzToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.загрузитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modeToolStripMenuItem,
            this.дФигураToolStripMenuItem,
            this.плоскостьToolStripMenuItem,
            this.файлToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(1050, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // modeToolStripMenuItem
            // 
            this.modeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mode2DToolStripMenuItem,
            this.mode3DToolStripMenuItem});
            this.modeToolStripMenuItem.Name = "modeToolStripMenuItem";
            this.modeToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.modeToolStripMenuItem.Text = "Режим";
            // 
            // mode2DToolStripMenuItem
            // 
            this.mode2DToolStripMenuItem.Name = "mode2DToolStripMenuItem";
            this.mode2DToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.mode2DToolStripMenuItem.Text = "2D Режим";
            this.mode2DToolStripMenuItem.Click += new System.EventHandler(this.Mode2DToolStripMenuItem_Click);
            // 
            // mode3DToolStripMenuItem
            // 
            this.mode3DToolStripMenuItem.Name = "mode3DToolStripMenuItem";
            this.mode3DToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.mode3DToolStripMenuItem.Text = "3D Режим";
            this.mode3DToolStripMenuItem.Click += new System.EventHandler(this.Mode3DToolStripMenuItem_Click);
            // 
            // дФигураToolStripMenuItem
            // 
            this.дФигураToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.кубToolStripMenuItem});
            this.дФигураToolStripMenuItem.Name = "дФигураToolStripMenuItem";
            this.дФигураToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.дФигураToolStripMenuItem.Text = "3Д Фигура";
            // 
            // кубToolStripMenuItem
            // 
            this.кубToolStripMenuItem.Name = "кубToolStripMenuItem";
            this.кубToolStripMenuItem.Size = new System.Drawing.Size(94, 22);
            this.кубToolStripMenuItem.Text = "Куб";
            this.кубToolStripMenuItem.Click += new System.EventHandler(this.кубToolStripMenuItem_Click);
            // 
            // плоскостьToolStripMenuItem
            // 
            this.плоскостьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.yOzToolStripMenuItem,
            this.yOzToolStripMenuItem1,
            this.xOzToolStripMenuItem});
            this.плоскостьToolStripMenuItem.Name = "плоскостьToolStripMenuItem";
            this.плоскостьToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
            this.плоскостьToolStripMenuItem.Text = "Плоскость";
            // 
            // yOzToolStripMenuItem
            // 
            this.yOzToolStripMenuItem.Name = "yOzToolStripMenuItem";
            this.yOzToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.yOzToolStripMenuItem.Text = "xOY";
            this.yOzToolStripMenuItem.Click += new System.EventHandler(this.xOyToolStripMenuItem_Click);
            // 
            // yOzToolStripMenuItem1
            // 
            this.yOzToolStripMenuItem1.Name = "yOzToolStripMenuItem1";
            this.yOzToolStripMenuItem1.Size = new System.Drawing.Size(96, 22);
            this.yOzToolStripMenuItem1.Text = "yOz";
            this.yOzToolStripMenuItem1.Click += new System.EventHandler(this.yOzToolStripMenuItem1_Click);
            // 
            // xOzToolStripMenuItem
            // 
            this.xOzToolStripMenuItem.Name = "xOzToolStripMenuItem";
            this.xOzToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.xOzToolStripMenuItem.Text = "xOz";
            this.xOzToolStripMenuItem.Click += new System.EventHandler(this.xOzToolStripMenuItem_Click);
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.сохранитьToolStripMenuItem,
            this.загрузитьToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // сохранитьToolStripMenuItem
            // 
            this.сохранитьToolStripMenuItem.Name = "сохранитьToolStripMenuItem";
            this.сохранитьToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.сохранитьToolStripMenuItem.Text = "Сохранить";
            this.сохранитьToolStripMenuItem.Click += new System.EventHandler(this.сохранитьToolStripMenuItem_Click);
            // 
            // загрузитьToolStripMenuItem
            // 
            this.загрузитьToolStripMenuItem.Name = "загрузитьToolStripMenuItem";
            this.загрузитьToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.загрузитьToolStripMenuItem.Text = "Загрузить";
            this.загрузитьToolStripMenuItem.Click += new System.EventHandler(this.загрузитьToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1050, 488);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(604, 414);
            this.Name = "MainForm";
            this.Text = "Графический редактор";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem дФигураToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem кубToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem плоскостьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem yOzToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem yOzToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem xOzToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сохранитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem загрузитьToolStripMenuItem;
    }
    }