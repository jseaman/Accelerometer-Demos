using System;
using System.Collections.Generic;
using System.Text;

namespace HTC_GSensor_Demo
{
    class Vector2
    {
        public Vector2()
        {
        }

        public Vector2(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }
        
        public double X = 0;
        public double Y = 0;

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public static double MiddleAngle(Vector2 v1, Vector2 v2)
        {
            double val1 = Vector2.Dot(v1, v2);
            double val2 = v1.Length() * v2.Length();

            if (val2 == 0)
                val2 = 0.0001;

            double val3 = val1 / val2;
            double val4 = Math.Acos(val3);

            double angle = val4;

            if (angle.Equals(double.NaN))
                angle = 0;

            return angle;
        }

        public static Vector2 Sum(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2 Subtract(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static double Dot(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        
    }
}
