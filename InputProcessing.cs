using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GeneticAlgorithm
{
    public static class InputProcessing
    {
        public static bool tests = false;
        public static double a, b, d, pk, pm;
        public static int N, l, prec, T;
        public static bool elite;
        public static List<double> plotDataMin;
        public static List<double> plotDataAvg;
        public static List<double> plotDataMax;
        public static List<object> Process(double _a, double _b, double _d, int _N, double _pk, double _pm, int _T, bool _elite)
        {
            plotDataMin = new List<double>();
            plotDataAvg = new List<double>();
            plotDataMax = new List<double>();
            double a, b, d, pk, pm;
            int N, l, prec, T;
            bool elite;

            a = _a; b = _b; d = _d; N = _N; pk = _pk; pm = _pm; T = _T; elite = _elite;

            l = (int)Math.Ceiling(Math.Log((b - a) / d + 1, 2));
            prec = (int)Math.Log(d, 0.1);

            Population population = null;

            for (int i = 0; i < T; ++i)
            {
                population = new Population(population, a, b, d, N, pk, pm, T, elite, l, prec);
                population.Select();
                population.Cross();
                population.Mutate();
                if (!tests)
                {
                    plotDataMin.Add(
                        ((double[])(population.GetStages()[(int)Population.PopulationStagesNames.FinalGoalVals]))
                            .ToArray().Min()

                    );
                    plotDataAvg.Add(
                        ((double[])(population.GetStages()[(int)Population.PopulationStagesNames.FinalGoalVals]))
                            .ToArray().Average()
                    );
                    plotDataMax.Add(
                        ((double[])(population.GetStages()[(int)Population.PopulationStagesNames.FinalGoalVals]))
                            .ToArray().Max()
                    );
                }
            }
            if (!tests)
            {
                InputProcessing.a = a;
                InputProcessing.b = b;
                InputProcessing.d = d;
                InputProcessing.l = l;
                InputProcessing.prec = prec;
                InputProcessing.N = N;
                InputProcessing.pk = pk;
                InputProcessing.pm = pm;
                InputProcessing.T = T;
                InputProcessing.elite = elite;
                return population.GetStages();
            }
            else
            {
                List<object> res = new List<object>(1);
                res.Add(population.GetStages()[(int)Population.PopulationStagesNames.FinalGoalVals]);
                return res;
            }
        }

        public static long ToLong(this double x, double a, double b, int l)
        {
            // Precompute the factor outside if possible for repeated calls
            double factor = (Math.Pow(2, l) - 1) / (b - a);
            return (long)Math.Round((x - a) * factor);
        }

        public static BitArray ToBin(this long x, int l)
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

        public static double ToReal(this long x, double a, double b, int prec, int l)
        {
            // Precompute the denominator if used repeatedly
            double range = b - a;
            double maxVal = (1L << l) - 1;
            return Math.Round((x * range / maxVal) + a, prec);
        }

        private static readonly double PrecomputedConstant = 20 * Math.PI;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double f(double x)
        {
            return x % 1 * (Math.Cos(PrecomputedConstant * x) - Math.Sin(x));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double g(double x, double min, double d)
        {
            return f(x) + (0 - min) + d;
        }
    }
}
