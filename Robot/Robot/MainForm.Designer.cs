using System.Windows.Forms;

namespace Robot
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм

        private void InitializeComponent()
        {
            this.trackBarMove = new System.Windows.Forms.TrackBar();
            this.trackBarBoomLength = new System.Windows.Forms.TrackBar();
            this.trackBarSubArmAngle = new System.Windows.Forms.TrackBar();
            this.labelMove = new System.Windows.Forms.Label();
            this.labelBoomLength = new System.Windows.Forms.Label();
            this.labelSubArmAngle = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.labelArmAngle = new System.Windows.Forms.Label();
            this.trackBarArmAngle = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Башня = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.labelArmLength = new System.Windows.Forms.Label();
            this.trackBarArmLength = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMove)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBoomLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSubArmAngle)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarArmAngle)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.Башня.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarArmLength)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBarMove
            // 
            this.trackBarMove.Location = new System.Drawing.Point(25, 35);
            this.trackBarMove.Maximum = 950;
            this.trackBarMove.Minimum = 200;
            this.trackBarMove.Name = "trackBarMove";
            this.trackBarMove.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarMove.Size = new System.Drawing.Size(45, 200);
            this.trackBarMove.TabIndex = 0;
            this.trackBarMove.TickFrequency = 10;
            this.trackBarMove.Value = 600;
            this.trackBarMove.Scroll += new System.EventHandler(this.TrackBar_Scroll);
            // 
            // trackBarBoomLength
            // 
            this.trackBarBoomLength.Location = new System.Drawing.Point(22, 35);
            this.trackBarBoomLength.Maximum = 200;
            this.trackBarBoomLength.Minimum = 50;
            this.trackBarBoomLength.Name = "trackBarBoomLength";
            this.trackBarBoomLength.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarBoomLength.Size = new System.Drawing.Size(45, 200);
            this.trackBarBoomLength.TabIndex = 1;
            this.trackBarBoomLength.TickFrequency = 10;
            this.trackBarBoomLength.Value = 100;
            this.trackBarBoomLength.Scroll += new System.EventHandler(this.TrackBar_Scroll);
            // 
            // trackBarSubArmAngle
            // 
            this.trackBarSubArmAngle.Location = new System.Drawing.Point(35, 35);
            this.trackBarSubArmAngle.Maximum = 90;
            this.trackBarSubArmAngle.Minimum = -90;
            this.trackBarSubArmAngle.Name = "trackBarSubArmAngle";
            this.trackBarSubArmAngle.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarSubArmAngle.Size = new System.Drawing.Size(45, 200);
            this.trackBarSubArmAngle.TabIndex = 2;
            this.trackBarSubArmAngle.TickFrequency = 5;
            this.trackBarSubArmAngle.Value = -30;
            this.trackBarSubArmAngle.Scroll += new System.EventHandler(this.TrackBar_Scroll);
            // 
            // labelMove
            // 
            this.labelMove.Location = new System.Drawing.Point(9, 247);
            this.labelMove.Name = "labelMove";
            this.labelMove.Size = new System.Drawing.Size(80, 20);
            this.labelMove.TabIndex = 3;
            this.labelMove.Text = "Позиция: 600";
            // 
            // labelBoomLength
            // 
            this.labelBoomLength.Location = new System.Drawing.Point(6, 247);
            this.labelBoomLength.Name = "labelBoomLength";
            this.labelBoomLength.Size = new System.Drawing.Size(100, 23);
            this.labelBoomLength.TabIndex = 4;
            this.labelBoomLength.Text = "Длина: 100";
            // 
            // labelSubArmAngle
            // 
            this.labelSubArmAngle.Location = new System.Drawing.Point(9, 247);
            this.labelSubArmAngle.Name = "labelSubArmAngle";
            this.labelSubArmAngle.Size = new System.Drawing.Size(71, 20);
            this.labelSubArmAngle.TabIndex = 5;
            this.labelSubArmAngle.Text = "Угол: 30°";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.groupBox6);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.Башня);
            this.groupBox1.Controls.Add(this.groupBox5);
            this.groupBox1.Location = new System.Drawing.Point(3, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(560, 302);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Панель управления роботом";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.labelSubArmAngle);
            this.groupBox4.Controls.Add(this.trackBarSubArmAngle);
            this.groupBox4.Location = new System.Drawing.Point(228, 19);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(105, 280);
            this.groupBox4.TabIndex = 9;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Поворот башни";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.labelArmAngle);
            this.groupBox6.Controls.Add(this.trackBarArmAngle);
            this.groupBox6.Location = new System.Drawing.Point(450, 19);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(105, 280);
            this.groupBox6.TabIndex = 11;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Поворот стрелы";
            // 
            // labelArmAngle
            // 
            this.labelArmAngle.Location = new System.Drawing.Point(6, 247);
            this.labelArmAngle.Name = "labelArmAngle";
            this.labelArmAngle.Size = new System.Drawing.Size(71, 20);
            this.labelArmAngle.TabIndex = 6;
            this.labelArmAngle.Text = "Угол: 0°";
            // 
            // trackBarArmAngle
            // 
            this.trackBarArmAngle.Location = new System.Drawing.Point(21, 35);
            this.trackBarArmAngle.Maximum = 180;
            this.trackBarArmAngle.Minimum = 0;
            this.trackBarArmAngle.Name = "trackBarArmAngle";
            this.trackBarArmAngle.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarArmAngle.Size = new System.Drawing.Size(45, 200);
            this.trackBarArmAngle.TabIndex = 2;
            this.trackBarArmAngle.TickFrequency = 5;
            this.trackBarArmAngle.Value = 0;
            this.trackBarArmAngle.Scroll += new System.EventHandler(this.TrackBar_Scroll);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.trackBarMove);
            this.groupBox2.Controls.Add(this.labelMove);
            this.groupBox2.Location = new System.Drawing.Point(6, 19);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(105, 280);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Передвижение робота";
            // 
            // Башня
            // 
            this.Башня.Controls.Add(this.trackBarBoomLength);
            this.Башня.Controls.Add(this.labelBoomLength);
            this.Башня.Location = new System.Drawing.Point(117, 19);
            this.Башня.Name = "Башня";
            this.Башня.Size = new System.Drawing.Size(105, 280);
            this.Башня.TabIndex = 8;
            this.Башня.TabStop = false;
            this.Башня.Text = "Длина башни";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.labelArmLength);
            this.groupBox5.Controls.Add(this.trackBarArmLength);
            this.groupBox5.Location = new System.Drawing.Point(339, 19);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(105, 280);
            this.groupBox5.TabIndex = 10;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Длина стрелы";
            // 
            // labelArmLength
            // 
            this.labelArmLength.Location = new System.Drawing.Point(6, 247);
            this.labelArmLength.Name = "labelArmLength";
            this.labelArmLength.Size = new System.Drawing.Size(74, 20);
            this.labelArmLength.TabIndex = 12;
            this.labelArmLength.Text = "Длина: 100";
            // 
            // trackBarArmLength
            // 
            this.trackBarArmLength.Location = new System.Drawing.Point(19, 35);
            this.trackBarArmLength.Maximum = 200;
            this.trackBarArmLength.Minimum = 50;
            this.trackBarArmLength.Name = "trackBarArmLength";
            this.trackBarArmLength.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarArmLength.Size = new System.Drawing.Size(45, 200);
            this.trackBarArmLength.TabIndex = 2;
            this.trackBarArmLength.TickFrequency = 10;
            this.trackBarArmLength.Value = 100;
            this.trackBarArmLength.Scroll += new System.EventHandler(this.TrackBar_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(400, 550);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "label1";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1073, 721);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.DoubleBuffered = true;
            this.Name = "MainForm";
            this.Text = "Симулятор Экскаватора";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMove)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBoomLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSubArmAngle)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarArmAngle)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.Башня.ResumeLayout(false);
            this.Башня.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarArmLength)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar trackBarMove;
        private System.Windows.Forms.TrackBar trackBarBoomLength;
        private System.Windows.Forms.TrackBar trackBarSubArmAngle;
        private System.Windows.Forms.Label labelMove;
        private System.Windows.Forms.Label labelBoomLength;
        private System.Windows.Forms.Label labelSubArmAngle;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        private TrackBar trackBarArmLength;
        private GroupBox Башня;
        private GroupBox groupBox6;
        private TrackBar trackBarArmAngle;
        private Label labelArmAngle;
        private Label labelArmLength;
        private Label label1;
    }
}