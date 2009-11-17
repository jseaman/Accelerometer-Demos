namespace TestIphoneComm
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            this.StartButton = new System.Windows.Forms.Button();
            this.EndButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.AccelX = new System.Windows.Forms.TextBox();
            this.AccelY = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.AccelZ = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.CompZ = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.CompY = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.CompX = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ConnStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(22, 38);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(224, 23);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "START";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // EndButton
            // 
            this.EndButton.Enabled = false;
            this.EndButton.Location = new System.Drawing.Point(22, 87);
            this.EndButton.Name = "EndButton";
            this.EndButton.Size = new System.Drawing.Size(224, 23);
            this.EndButton.TabIndex = 1;
            this.EndButton.Text = "END";
            this.EndButton.UseVisualStyleBackColor = true;
            this.EndButton.Click += new System.EventHandler(this.EndButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 133);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "X";
            // 
            // AccelX
            // 
            this.AccelX.Location = new System.Drawing.Point(42, 130);
            this.AccelX.Name = "AccelX";
            this.AccelX.ReadOnly = true;
            this.AccelX.Size = new System.Drawing.Size(129, 20);
            this.AccelX.TabIndex = 3;
            // 
            // AccelY
            // 
            this.AccelY.Location = new System.Drawing.Point(210, 130);
            this.AccelY.Name = "AccelY";
            this.AccelY.ReadOnly = true;
            this.AccelY.Size = new System.Drawing.Size(129, 20);
            this.AccelY.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(190, 133);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Y";
            // 
            // AccelZ
            // 
            this.AccelZ.Location = new System.Drawing.Point(376, 130);
            this.AccelZ.Name = "AccelZ";
            this.AccelZ.ReadOnly = true;
            this.AccelZ.Size = new System.Drawing.Size(129, 20);
            this.AccelZ.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(356, 133);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Z";
            // 
            // CompZ
            // 
            this.CompZ.Location = new System.Drawing.Point(376, 173);
            this.CompZ.Name = "CompZ";
            this.CompZ.ReadOnly = true;
            this.CompZ.Size = new System.Drawing.Size(129, 20);
            this.CompZ.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(356, 176);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Z";
            // 
            // CompY
            // 
            this.CompY.Location = new System.Drawing.Point(210, 173);
            this.CompY.Name = "CompY";
            this.CompY.ReadOnly = true;
            this.CompY.Size = new System.Drawing.Size(129, 20);
            this.CompY.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(190, 176);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Y";
            // 
            // CompX
            // 
            this.CompX.Location = new System.Drawing.Point(42, 173);
            this.CompX.Name = "CompX";
            this.CompX.ReadOnly = true;
            this.CompX.Size = new System.Drawing.Size(129, 20);
            this.CompX.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 176);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(14, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "X";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ConnStatus
            // 
            this.ConnStatus.AutoSize = true;
            this.ConnStatus.Location = new System.Drawing.Point(25, 239);
            this.ConnStatus.Name = "ConnStatus";
            this.ConnStatus.Size = new System.Drawing.Size(78, 13);
            this.ConnStatus.TabIndex = 14;
            this.ConnStatus.Text = "Not connected";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 382);
            this.Controls.Add(this.ConnStatus);
            this.Controls.Add(this.CompZ);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.CompY);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.CompX);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.AccelZ);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.AccelY);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.AccelX);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.EndButton);
            this.Controls.Add(this.StartButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button EndButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox AccelX;
        private System.Windows.Forms.TextBox AccelY;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox AccelZ;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox CompZ;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox CompY;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox CompX;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label ConnStatus;
    }
}

