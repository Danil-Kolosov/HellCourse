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

            // MenuStrip
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();

            // menuStrip
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modeToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(500, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";

            // modeToolStripMenuItem
            this.modeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mode2DToolStripMenuItem,
            this.mode3DToolStripMenuItem});
            this.modeToolStripMenuItem.Text = "Режим";

            // mode2DToolStripMenuItem
            this.mode2DToolStripMenuItem.Text = "2D Режим";
            this.mode2DToolStripMenuItem.Click += new System.EventHandler(this.Mode2DToolStripMenuItem_Click);

            // mode3DToolStripMenuItem
            this.mode3DToolStripMenuItem.Text = "3D Режим";
            this.mode3DToolStripMenuItem.Click += new System.EventHandler(this.Mode3DToolStripMenuItem_Click);

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 600);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MainForm";
            this.Text = "Графический редактор";

            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

            #endregion
        }
    }