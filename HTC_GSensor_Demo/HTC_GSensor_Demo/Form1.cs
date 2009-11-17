using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Microsoft.WindowsCE.Forms;
using System.Reflection;
using System.IO;
using Sensors;

namespace HTC_GSensor_Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SystemSettings.ScreenOrientation = Microsoft.WindowsCE.Forms.ScreenOrientation.Angle270;
            
            String strAppDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            String FilePath = Path.Combine(strAppDir, "Images\\ppic2.jpg");

            Img = new Bitmap(FilePath);
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            SystemSettings.ScreenOrientation = Microsoft.WindowsCE.Forms.ScreenOrientation.Angle270;
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            SystemSettings.ScreenOrientation = Microsoft.WindowsCE.Forms.ScreenOrientation.Angle0;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //Graphics g = Graphics.FromImage(Img);

            if (BackBuffer == null)
            {
                BackBuffer = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format16bppRgb565);
                BackBufferGraphics = Graphics.FromImage(BackBuffer);
            }
            
            //BackBufferGraphics.Clear(SystemColors.Window);
            BackBufferGraphics.DrawImage(Img, 0, 0, new Rectangle(0, yProp, ClientSize.Width, ClientSize.Height), GraphicsUnit.Pixel);

            int x = ClientSize.Width;
            int y = ClientSize.Height;

            GVector gvector = Sensor.GetGVector();
            BackBufferGraphics.DrawString(gvector.Normalize().ToString(), myFont, myTextBrush, 0, 200);

            e.Graphics.DrawImage(BackBuffer, 0, 0);

            yLast = yProp;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            yProp = (int)(AngleProportion() + 0.5);

            if (Math.Abs(yProp - yLast) >= 10)
                //Invalidate();
                Refresh();
        }

        private int yLast = 0;
        private int yProp = 0;

        protected double AngleProportion ()
        {
            Vector2 v1 = new Vector2(0, 1);
            GVector gv = Sensor.GetGVector().Normalize();
            Vector2 v2 = new Vector2(gv.X, gv.Z);

            double Ang = Vector2.MiddleAngle(v1, v2);
            double m = (Img.Height - ClientSize.Height) / Math.PI;

            return Ang * m;
        }

        private Bitmap Img = null;
        private Bitmap BackBuffer = null;
        private Graphics BackBufferGraphics = null;
        IGSensor Sensor = GSensorFactory.CreateGSensor();
        Font myFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
        SolidBrush myTextBrush = new SolidBrush(Color.CornflowerBlue);
    }
}