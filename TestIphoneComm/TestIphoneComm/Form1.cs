using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using IphoneComm;

namespace TestIphoneComm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            Listener = new CoordListener(666);
            ChangeButtonStatuses();
            timer1.Enabled = true;
        }

        private void EndButton_Click(object sender, EventArgs e)
        {
            Listener.Stop();
            ChangeButtonStatuses();
            timer1.Enabled = false;
        }

        private void ChangeButtonStatuses()
        {
            StartButton.Enabled = !StartButton.Enabled;
            EndButton.Enabled = !EndButton.Enabled;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Listener.Connected)
            {
                if (ConnStatus.Text!="Connected")
                    ConnStatus.Text = "Connected";
                
                IphoneComm.Vector vAccel = Listener.AccelerometerVector;
                IphoneComm.Orientation oComp = Listener.CompassData;

                AccelX.Text = vAccel.x.ToString();
                AccelY.Text = vAccel.y.ToString();
                AccelZ.Text = vAccel.z.ToString();

                CompX.Text = oComp.Angle.ToString();
                CompY.Text = oComp.Accuracy.ToString();
            }
            else
                if (ConnStatus.Text!="Not connected")
                    ConnStatus.Text = "Not connected";
        }

        CoordListener Listener = null;
    }
}