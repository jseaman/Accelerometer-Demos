using System;
using System.Collections.Generic;
using System.Text;

namespace IphoneComm
{
    public class Orientation
    {
        public Orientation()
        {
            Angle = Accuracy = 0;
        }

        public Orientation(double Angle, double Accuracy)
        {
            this.Angle = Angle;
            this.Accuracy = Accuracy;
        }

        public double Angle, Accuracy;
    }
}
