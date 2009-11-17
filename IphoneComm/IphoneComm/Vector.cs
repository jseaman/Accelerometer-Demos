using System;
using System.Collections.Generic;
using System.Text;

namespace IphoneComm
{
    public class Vector
    {
        public Vector()
        {
            x = y = z = 0;
        }

        public Vector (double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double x, y, z;
    }
}
