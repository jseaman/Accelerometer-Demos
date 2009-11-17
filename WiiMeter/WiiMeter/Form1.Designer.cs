namespace WiiMeter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.WiimoteGaugeX = new AquaControls.AquaGauge();
            this.WiimoteGaugeY = new AquaControls.AquaGauge();
            this.WiimoteGaugeZ = new AquaControls.AquaGauge();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.NunchuckGaugeZ = new AquaControls.AquaGauge();
            this.NunchuckGaugeY = new AquaControls.AquaGauge();
            this.NunchuckGaugeX = new AquaControls.AquaGauge();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(1, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(248, 248);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // WiimoteGaugeX
            // 
            this.WiimoteGaugeX.BackColor = System.Drawing.Color.Transparent;
            this.WiimoteGaugeX.DialColor = System.Drawing.Color.Lavender;
            this.WiimoteGaugeX.DialText = "Wiimote - X";
            this.WiimoteGaugeX.Glossiness = 11.36364F;
            this.WiimoteGaugeX.Location = new System.Drawing.Point(255, 2);
            this.WiimoteGaugeX.MaxValue = 5F;
            this.WiimoteGaugeX.MinValue = -5F;
            this.WiimoteGaugeX.Name = "WiimoteGaugeX";
            this.WiimoteGaugeX.RecommendedValue = 0F;
            this.WiimoteGaugeX.Size = new System.Drawing.Size(248, 248);
            this.WiimoteGaugeX.TabIndex = 2;
            this.WiimoteGaugeX.ThresholdPercent = 0F;
            this.WiimoteGaugeX.Value = 0F;
            // 
            // WiimoteGaugeY
            // 
            this.WiimoteGaugeY.BackColor = System.Drawing.Color.Transparent;
            this.WiimoteGaugeY.DialColor = System.Drawing.Color.Lavender;
            this.WiimoteGaugeY.DialText = "Wiimote - Y";
            this.WiimoteGaugeY.Glossiness = 11.36364F;
            this.WiimoteGaugeY.Location = new System.Drawing.Point(509, -2);
            this.WiimoteGaugeY.MaxValue = 5F;
            this.WiimoteGaugeY.MinValue = -5F;
            this.WiimoteGaugeY.Name = "WiimoteGaugeY";
            this.WiimoteGaugeY.RecommendedValue = 0F;
            this.WiimoteGaugeY.Size = new System.Drawing.Size(248, 248);
            this.WiimoteGaugeY.TabIndex = 3;
            this.WiimoteGaugeY.ThresholdPercent = 0F;
            this.WiimoteGaugeY.Value = 0F;
            // 
            // WiimoteGaugeZ
            // 
            this.WiimoteGaugeZ.BackColor = System.Drawing.Color.Transparent;
            this.WiimoteGaugeZ.DialColor = System.Drawing.Color.Lavender;
            this.WiimoteGaugeZ.DialText = "Wiimote - Z";
            this.WiimoteGaugeZ.Glossiness = 11.36364F;
            this.WiimoteGaugeZ.Location = new System.Drawing.Point(763, 2);
            this.WiimoteGaugeZ.MaxValue = 5F;
            this.WiimoteGaugeZ.MinValue = -5F;
            this.WiimoteGaugeZ.Name = "WiimoteGaugeZ";
            this.WiimoteGaugeZ.RecommendedValue = 0F;
            this.WiimoteGaugeZ.Size = new System.Drawing.Size(248, 248);
            this.WiimoteGaugeZ.TabIndex = 4;
            this.WiimoteGaugeZ.ThresholdPercent = 0F;
            this.WiimoteGaugeZ.Value = 0F;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(1, 268);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(248, 248);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 5;
            this.pictureBox2.TabStop = false;
            // 
            // NunchuckGaugeZ
            // 
            this.NunchuckGaugeZ.BackColor = System.Drawing.Color.Transparent;
            this.NunchuckGaugeZ.DialColor = System.Drawing.Color.Lavender;
            this.NunchuckGaugeZ.DialText = "Nunchuck - Z";
            this.NunchuckGaugeZ.Glossiness = 11.36364F;
            this.NunchuckGaugeZ.Location = new System.Drawing.Point(763, 268);
            this.NunchuckGaugeZ.MaxValue = 5F;
            this.NunchuckGaugeZ.MinValue = -5F;
            this.NunchuckGaugeZ.Name = "NunchuckGaugeZ";
            this.NunchuckGaugeZ.RecommendedValue = 0F;
            this.NunchuckGaugeZ.Size = new System.Drawing.Size(248, 248);
            this.NunchuckGaugeZ.TabIndex = 8;
            this.NunchuckGaugeZ.ThresholdPercent = 0F;
            this.NunchuckGaugeZ.Value = 0F;
            // 
            // NunchuckGaugeY
            // 
            this.NunchuckGaugeY.BackColor = System.Drawing.Color.Transparent;
            this.NunchuckGaugeY.DialColor = System.Drawing.Color.Lavender;
            this.NunchuckGaugeY.DialText = "Nunchuck - Y";
            this.NunchuckGaugeY.Glossiness = 11.36364F;
            this.NunchuckGaugeY.Location = new System.Drawing.Point(509, 264);
            this.NunchuckGaugeY.MaxValue = 5F;
            this.NunchuckGaugeY.MinValue = -5F;
            this.NunchuckGaugeY.Name = "NunchuckGaugeY";
            this.NunchuckGaugeY.RecommendedValue = 0F;
            this.NunchuckGaugeY.Size = new System.Drawing.Size(248, 248);
            this.NunchuckGaugeY.TabIndex = 7;
            this.NunchuckGaugeY.ThresholdPercent = 0F;
            this.NunchuckGaugeY.Value = 0F;
            // 
            // NunchuckGaugeX
            // 
            this.NunchuckGaugeX.BackColor = System.Drawing.Color.Transparent;
            this.NunchuckGaugeX.DialColor = System.Drawing.Color.Lavender;
            this.NunchuckGaugeX.DialText = "Nunchuck - X";
            this.NunchuckGaugeX.Glossiness = 11.36364F;
            this.NunchuckGaugeX.Location = new System.Drawing.Point(255, 268);
            this.NunchuckGaugeX.MaxValue = 5F;
            this.NunchuckGaugeX.MinValue = -5F;
            this.NunchuckGaugeX.Name = "NunchuckGaugeX";
            this.NunchuckGaugeX.RecommendedValue = 0F;
            this.NunchuckGaugeX.Size = new System.Drawing.Size(248, 248);
            this.NunchuckGaugeX.TabIndex = 6;
            this.NunchuckGaugeX.ThresholdPercent = 0F;
            this.NunchuckGaugeX.Value = 0F;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1046, 535);
            this.Controls.Add(this.NunchuckGaugeZ);
            this.Controls.Add(this.NunchuckGaugeY);
            this.Controls.Add(this.NunchuckGaugeX);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.WiimoteGaugeZ);
            this.Controls.Add(this.WiimoteGaugeY);
            this.Controls.Add(this.WiimoteGaugeX);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Wiimeter";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private AquaControls.AquaGauge WiimoteGaugeX;
        private AquaControls.AquaGauge WiimoteGaugeY;
        private AquaControls.AquaGauge WiimoteGaugeZ;
        private System.Windows.Forms.PictureBox pictureBox2;
        private AquaControls.AquaGauge NunchuckGaugeZ;
        private AquaControls.AquaGauge NunchuckGaugeY;
        private AquaControls.AquaGauge NunchuckGaugeX;
    }
}

