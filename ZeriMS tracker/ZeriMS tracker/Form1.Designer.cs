namespace ZeriMS_tracker
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(11, 11);
            label1.Margin = new Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Padding = new Padding(5);
            label1.Size = new Size(107, 28);
            label1.TabIndex = 0;
            label1.Text = "PMS: -----";
            label1.Click += label1_Click_1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(11, 46);
            label2.Margin = new Padding(5, 0, 5, 0);
            label2.Name = "label2";
            label2.Padding = new Padding(5);
            label2.Size = new Size(70, 28);
            label2.TabIndex = 1;
            label2.Text = "label2";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(106, 11);
            label3.Margin = new Padding(5, 0, 5, 0);
            label3.Name = "label3";
            label3.Padding = new Padding(5);
            label3.Size = new Size(70, 28);
            label3.TabIndex = 2;
            label3.Text = "label3";
            label3.Click += label3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(106, 47);
            label4.Margin = new Padding(5, 0, 5, 0);
            label4.Name = "label4";
            label4.Padding = new Padding(5);
            label4.Size = new Size(70, 28);
            label4.TabIndex = 3;
            label4.Text = "label4";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = Color.FromArgb(19, 35, 33);
            label5.Location = new Point(207, 11);
            label5.Margin = new Padding(5, 0, 5, 0);
            label5.Name = "label5";
            label5.Padding = new Padding(5);
            label5.Size = new Size(70, 28);
            label5.TabIndex = 4;
            label5.Text = "label5";
            label5.Click += Label5_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(207, 46);
            label6.Margin = new Padding(5, 0, 5, 0);
            label6.Name = "label6";
            label6.Padding = new Padding(5);
            label6.Size = new Size(70, 28);
            label6.TabIndex = 5;
            label6.Text = "label6";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(19, 35, 33);
            BackgroundImageLayout = ImageLayout.None;
            ClientSize = new Size(308, 82);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Verdana", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ForeColor = Color.FromArgb(248, 241, 149);
            FormBorderStyle = FormBorderStyle.None;
            Location = new Point(235, 1046);
            Margin = new Padding(5, 4, 5, 4);
            Name = "Form1";
            Opacity = 0.9D;
            StartPosition = FormStartPosition.Manual;
            Text = "Zeri MS Tracker";
            TopMost = true;
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        private void Label5_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
    }
}
