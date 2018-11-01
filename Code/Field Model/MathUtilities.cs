//**************************************************************//
//**************************************************************//
//*******           Copyright (C) 2010-2018              *******//
//*******           Team: Wuhan University               *******//
//*******           Developer:Xuequan Zhang              *******//
//*******           EMail:brucezhang@whu.edu.cn          *******//
//*******           Address: 129 Luoyu Road, Wuhan,      *******//
//*******                    Hubei, 430079, China        *******//
//**************************************************************//
//**************************************************************//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;

namespace FieldModel
{
    public static class MathUitilities
    {
        public static Matrix RotationV1ToV2(Vector3 v1, Vector3 v2)
        {
            v1.Normalize();
            v2.Normalize();
            Vector3 v = Vector3.Cross(v1, v2);
            float e = Vector3.Dot(v1, v2);
            float h = (1 - e) / (Vector3.Dot(v, v));

            Matrix m1 = new Matrix();
            m1.M11 = e + h * v.X * v.X;
            m1.M12 = h * v.X * v.Y - v.Z;
            m1.M13 = h * v.X * v.Z + v.Y;
            m1.M21 = h * v.X * v.Y + v.Z;
            m1.M22 = e + h * v.Y * v.Y;
            m1.M23 = h * v.Y * v.Z - v.X;
            m1.M31 = h * v.X * v.Z - v.Y;
            m1.M32 = h * v.Y * v.Z + v.X;
            m1.M33 = e + h * v.Z * v.Z;
            m1.M44 = 1;
            return m1;
        }

        /// <summary>
        /// 比较两个浮点型数据的大小
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>返回0表示相等，1表示前者比后者大，-1表示前者比后者小 </returns>
        public static int IsEqual(double x, double y)
        {
            if (Math.Abs(x - y) <= 0.00001)
                return 0;
            else if ((x - y) > 0)
                return 1;
            else
                return -1;
        }

        public static void WriteLog(string value)
        {
            //FileStream fs;
            string sPath = @"C:ThreadLog.txt";
            if (File.Exists(sPath))
            {
                using (FileStream fs = new FileStream(sPath, FileMode.Append))
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode))
                {
                    sw.Write(value + " ");
                }
            }
        }

        public static HSV RGBtoHSV(int rgb)
        {
            float r, g, b, max, min, h, s, v;  //r,g,b belong to  (0,1)
            r = (float)((rgb >> 16) & 255) / (float)255;   
            g = (float)((rgb >> 8) & 255) / (float)255;
            b = (float)(rgb & 255) / (float)255;
            max = Math.Max(Math.Max(r, g), b);
            min = Math.Min(Math.Min(r, g), b);
            if (max == min)
                h = 0;
            else
            {
                if (r == max)
                    h = (g - b) / (max - min);
                else if (g == max)
                    h = 2 + (b - r) / (max - min);
                else
                    h = 4 + (r - g) / (max - min);

                h = h * 60;
                if (h < 0)
                    h = h + 360;
            }
            v = max;
            s = (max - min) / max;

            HSV hsv = new HSV();
            //if (h == float.NaN)
            //    h = 0;  //h为NaN的时候为白色，这里假3定为0度
            //if (s == float.NaN)
            //    s = 0;  //h s=NaN时，为黑色，这里也假定h s=0
            hsv.H = h;
            hsv.S = s;
            hsv.V = v;
            return hsv;
        }

        public static int HSVtoRGB(HSV hsv)
        {
            float R, G, B;
            float H = hsv.H;
            float S = hsv.S;
            float V = hsv.V;

            if (S == 0)
                R = G = B = V;
            else
            {
                H /= 60;
                int i = (int)H;
                float f, a, b, c;
                f = H - i;
                a = V * (1 - S);
                b = V * (1 - S * f);
                c = V * (1 - S * (1 - f));
                switch (i)
                {
                    case 0:
                        R = V;
                        G = c;
                        B = a;
                        break;
                    case 1:
                        R = b;
                        G = V;
                        B = a;
                        break;
                    case 2:
                        R = a;
                        G = V;
                        B = c;
                        break;
                    case 3:
                        R = a;
                        G = b;
                        B = V;
                        break;
                    case 4:
                        R = c;
                        G = a;
                        B = V;
                        break;
                    default:   //case: 5
                        R = V;
                        G = a;
                        B = b;
                        break;
                }
            }
            return ((int)(R * 255) << 16) | ((int)(G * 255)) << 8 | (int)(B * 255);
        }
    }

    public struct HSV
    {
        public float H;
        public float S;
        public float V;
    }
}