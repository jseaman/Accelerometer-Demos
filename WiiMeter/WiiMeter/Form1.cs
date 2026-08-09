using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WiimoteLib;


namespace WiiMeter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private readonly Wiimote Wii = new Wiimote();
        private bool wiimoteConnected;

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Wii.Connect();
                wiimoteConnected = true;
                Wii.WiimoteChanged += Wii_WiimoteChanged;
                Wii.SetReportType(InputReport.ExtensionAccel, true);
                Wii.SetLEDs(1);

                if (Nunchuck_Present())
                    this.Height = 650;
                else
                    this.Height = 350;

                WiimoteGaugeX.ThresholdPercent = 20f;
                WiimoteGaugeY.ThresholdPercent = 20f;
                WiimoteGaugeZ.ThresholdPercent = 20f;

                NunchuckGaugeX.ThresholdPercent = 20f;
                NunchuckGaugeY.ThresholdPercent = 20f;
                NunchuckGaugeZ.ThresholdPercent = 20f;

            }
            catch (Exception E)
            {
                MessageBox.Show(
                    "Could not connect to a Wii Remote.\n\n" +
                    "Close Dolphin completely, wake the remote by pressing a button, and make sure " +
                    "Windows shows Nintendo RVL-CNT-01 or RVL-CNT-01-TR as connected. Then restart WiiMeter.\n\n" +
                    "Details: " + E.Message,
                    "Wii Remote not available",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
            }
        }

        protected bool Nunchuck_Present()
        {
            return !(Wii.WiimoteState.NunchukState.AccelState.Values.X == 0 && Wii.WiimoteState.NunchukState.AccelState.Values.Y == 0 && Wii.WiimoteState.NunchukState.AccelState.Values.Z == 0);
        }

        protected delegate void GaugeDelegate(AquaControls.AquaGauge Gauge, float val);

        protected void ChangeGaugeVal(AquaControls.AquaGauge Gauge, float val)
        {
            Gauge.Value = val;
            Gauge.ThresholdPercent *= 1;

            if (val > 1.2 || val < -1.2)
                Gauge.DialColor = Color.Red;
            else
                Gauge.DialColor = Color.Lavender;

            Application.DoEvents();
        }

        void Wii_WiimoteChanged(object sender, WiimoteChangedEventArgs e)
        {
            try
            {
                GaugeDelegate Func = new GaugeDelegate(ChangeGaugeVal);

                Console.WriteLine("Wiimote: X={0}, Y={1}, Z={2}", e.WiimoteState.AccelState.Values.X, e.WiimoteState.AccelState.Values.Y, e.WiimoteState.AccelState.Values.Z);

                WiimoteGaugeX.Invoke(Func, new object[] { WiimoteGaugeX, e.WiimoteState.AccelState.Values.X });
                WiimoteGaugeY.Invoke(Func, new object[] { WiimoteGaugeY, e.WiimoteState.AccelState.Values.Y });
                WiimoteGaugeZ.Invoke(Func, new object[] { WiimoteGaugeZ, e.WiimoteState.AccelState.Values.Z });

                if (Nunchuck_Present())
                {
                    NunchuckGaugeX.Invoke(Func, new object[] { NunchuckGaugeX, e.WiimoteState.NunchukState.AccelState.Values.X });
                    NunchuckGaugeY.Invoke(Func, new object[] { NunchuckGaugeY, e.WiimoteState.NunchukState.AccelState.Values.Y });
                    NunchuckGaugeZ.Invoke(Func, new object[] { NunchuckGaugeZ, e.WiimoteState.NunchukState.AccelState.Values.Z });
                }
            }
            catch (Exception)
            {
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (wiimoteConnected)
                {
                    Wii.WiimoteChanged -= Wii_WiimoteChanged;
                    Wii.SetLEDs(0);
                    Wii.Disconnect();
                    wiimoteConnected = false;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
