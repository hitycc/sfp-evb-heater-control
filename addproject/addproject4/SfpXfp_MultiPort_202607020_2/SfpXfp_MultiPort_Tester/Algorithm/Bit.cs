using System;
using System.Drawing;
using System.Windows.Forms;

namespace FibertopTest_Common
{
    public class Bit
    {
        //index从0开始　　
        //获取取第index是否为1　　
        public static bool GetBit(byte b, int index)
        {
            return (b & (1 << index)) > 0;
        }
        //将第index位设为1　　
        public static byte SetBit(byte b, int index)
        {
            b |= (byte)(1 << index);
            return b;
        }
        //将第index位设为0　　
        public static byte ClearBit(byte b, int index)
        {
            b &= (byte)((1 << 8) - 1 - (1 << index));
            return b;
        }
        //将第index位取反　　
        public static byte ReverseBit(byte b, int index)
        {
            b ^= (byte)(1 << index);
            return b;
        }
        //比较数组是否相等
        public static bool ByteEquals(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length) return false;
            if (b1 == null || b2 == null) return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                {
                    /*MessageBox.Show($"b1[i] != b2[i]",
                "通信故障",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);*/
                    return false;
                }
            return true;
        }

        //像素对比
        public static bool IsSameImage(Image a, Image b)
        {
            if (a.Width != b.Width || a.Height != b.Height) return false;
            Bitmap p1 = a as Bitmap;
            Bitmap p2 = b as Bitmap;
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    if (p1.GetPixel(x, y) != p2.GetPixel(x, y)) return false;
                }
            }
            return true;
        }

        /* 最小二乘曲线拟合 
       P(x)=a0+a1(x-z)+a2(x-z)^2+...+am-1(x-z)^m-1
       z=(x1+x2+...+xn)/n
       m : 变量个数	n : 数据点个数
       */
        public static void iapcir(double[] x, double[] y, short n, double[] a, short m, double[] dt)
        {
            int i, j, k;
            double z, p, c, g, q, d1, d2;
            double[] s, t, b;
            s = new double[20];
            t = new double[20];
            b = new double[20];

            for (i = 0; i <= m - 1; i++) a[i] = 0.0;
            if (m > n) m = n;
            if (m > 20) m = 20;
            z = 0.0;
            //for (i = 0; i <= n - 1; i++) z = z + x[i] / (1.0 * n);//求均值
            b[0] = 1.0; d1 = 1.0 * n; p = 0.0; c = 0.0; q = 0.0;
            for (i = 0; i <= n - 1; i++)
            { p = p + (x[i] - z); c = c + y[i]; }
            c = c / d1; p = p / d1;
            a[0] = c * b[0];
            if (m > 1)
            {
                t[1] = 1.0; t[0] = -p;
                d2 = 0.0; c = 0.0; g = 0.0;
                for (i = 0; i <= n - 1; i++)
                {
                    q = x[i] - z - p; d2 = d2 + q * q;
                    c = c + y[i] * q;
                    g = g + (x[i] - z) * q * q;
                }
                c = c / d2; p = g / d2; q = d2 / d1;
                d1 = d2;
                a[1] = c * t[1]; a[0] = c * t[0] + a[0];
            }
            for (j = 2; j <= m - 1; j++)
            {
                s[j] = t[j - 1];
                s[j - 1] = -p * t[j - 1] + t[j - 2];
                if (j >= 3)
                    for (k = j - 2; k >= 1; k--)
                        s[k] = -p * t[k] + t[k - 1] - q * b[k];
                s[0] = -p * t[0] - q * b[0];
                d2 = 0.0; c = 0.0; g = 0.0;
                for (i = 0; i <= n - 1; i++)
                {
                    q = s[j];
                    for (k = j - 1; k >= 0; k--)
                        q = q * (x[i] - z) + s[k];
                    d2 = d2 + q * q; c = c + y[i] * q;
                    g = g + (x[i] - z) * q * q;
                }
                c = c / d2; p = g / d2; q = d2 / d1;
                d1 = d2;
                a[j] = c * s[j]; t[j] = s[j];
                for (k = j - 1; k >= 0; k--)
                {
                    a[k] = c * s[k] + a[k];
                    b[k] = t[k]; t[k] = s[k];
                }
            }
            dt[0] = 0.0; dt[1] = 0.0; dt[2] = 0.0;
            for (i = 0; i <= n - 1; i++)
            {
                q = a[m - 1];
                for (k = m - 2; k >= 0; k--)
                    q = a[k] + q * (x[i] - z);
                p = (q - y[i]) / y[i];
                if (Math.Abs(p) > dt[2]) dt[2] = Math.Abs(p);
                dt[0] = dt[0] + p * p;
                dt[1] = dt[1] + Math.Abs(p);
            }
            dt[3] = Math.Sqrt(dt[0] / n);
            dt[4] = dt[1] / n;
            return;
        }
    }

    public class SaveException : ApplicationException
    {
        public SaveException(string msg) : base(msg) { }
    }
}
