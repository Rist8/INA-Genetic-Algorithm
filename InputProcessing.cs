using System;
using System.Collections;

namespace GeneticAlgorithm
{
    public static class InputProcessing
    {
        public static double a, b, d, pk, pm;
        public static int N, l, prec;
        public static ArrayList Process(double _a, double _b, double _d, int _N, double _pk, double _pm)
        {

            a = _a; b = _b; d = _d; N = _N; pk = _pk; pm = _pm;

            l = (int)Math.Ceiling(Math.Log((b - a) / d + 1, 2));
            prec = (int)Math.Log(d, 0.1);

            Population population = new Population(null);
            population.Select();
            population.Cross();
            population.Mutate();

            return population.GetStages();
        }

        public static long ToLong(this double x)
        {
            return (long)Math.Round((1 / (b - a)) * (x - a)
                * (Math.Pow(2, l) - 1));
        }

        public static BitArray ToBin(this long x)
        {
            BitArray binFormat = new BitArray(BitConverter.GetBytes(x));
            binFormat.Length = l;
            return binFormat;
        }

        public static long ToLong(this BitArray x)
        {
            var array = new int[2];
            x.CopyTo(array, 0);
            return (uint)array[0] + ((long)(uint)array[1] << 32);
        }

        public static double ToReal(this long x)
        {
            return Math.Round(x * (b - a) / (Math.Pow(2, l) - 1)
                + a, prec);
        }

        public static double f(double x)
        {
            return x % 1 * (Math.Cos(20 * Math.PI * x) - Math.Sin(x));
        }

        public static double g(double x)
        {
            return f(x) + 2 + d;
        }
    }
}
