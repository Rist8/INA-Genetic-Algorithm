using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilityNamespace;
using static UtilityNamespace.Utility;

namespace GeneticAlgorithm
{
    public class Population
    {
        private ThreadLocal<Random> threadLocalRandom;
        public double a, b, d, pk, pm;
        public int N, l, prec, T;
        public bool elite;
        private double eliteVal, eliteRate;
        public enum PopulationStagesNames
        {
            InitialVals,
            GoalVals,
            RatingVals,
            SelectProbs,
            DistributionVals,
            SelectRandomVals,
            SelectedVals,
            SelectedValsBin,
            ParentsVals,
            ChildrenVals,
            OffspringVals,
            ResultVals,
            FinalGoalVals
        }
        public Population(Population population, double _a, double _b, double _d,
            int _N, double _pk, double _pm, int _T, bool _elite, int _l, int _prec)
        {
            a = _a; b = _b; d = _d; N = _N; pk = _pk; pm = _pm;
            T = _T; elite = _elite; l = _l; prec = _prec;
            threadLocalRandom = new ThreadLocal<Random>(
                () => new Random(Environment.TickCount * Thread.CurrentThread.ManagedThreadId));

            _populationStages = new List<object>(Enum.GetNames(typeof(PopulationStagesNames)).Length);

            if (population == null)
            {
                Populate();
            }
            else
            {
                _populationStages.Add(population._populationStages[(int)PopulationStagesNames.ResultVals]);
            }
            Evaluate();
        }

        public void Evaluate()
        {
            double[] valuesStage = (double[])_populationStages[(int)PopulationStagesNames.InitialVals];
            double[] goalStage = new double[N];
            double maxTmp = double.MinValue;
            double maxVal = 0;
            _populationStages.Add(goalStage);
            for (int i = 0; i < N; ++i)
            {
                goalStage[i] = InputProcessing.f(valuesStage[i]);
                if (elite && goalStage[i] > maxTmp)
                {
                    maxTmp = goalStage[i];
                    maxVal = valuesStage[i];
                }
            }

            double[] rateStage = new double[N];
            double gsum = 0;
            double minValue = goalStage.Min();
            _populationStages.Add(rateStage);
            for (int i = 0; i < N; ++i)
            {
                rateStage[i] = InputProcessing.g(valuesStage[i], minValue, d);
                gsum += rateStage[i];
            }

            double[] probStage = new double[N];
            _populationStages.Add(probStage);
            for (int i = 0; i < N; ++i)
            {
                probStage[i] = rateStage[i] / gsum;
            }

            double[] distributionStage = new double[N];
            _populationStages.Add(distributionStage);
            for (int i = 0; i < N; ++i)
            {
                distributionStage[i] = probStage[i] + (i > 0 ? distributionStage[i - 1] : 0);
            }

            if (elite)
            {
                eliteVal = maxVal;
                eliteRate = maxTmp;
            }
        }

        public void Select()
        {
            double[] distributionStage = (double[])_populationStages[(int)PopulationStagesNames.DistributionVals];
            double[] valuesStage = (double[])_populationStages[(int)PopulationStagesNames.InitialVals];
            double[] randomStage = new double[N];
            _populationStages.Add(randomStage);
            for (int i = 0; i < N; ++i)
            {
                randomStage[i] = threadLocalRandom.Value.NextDouble();
            }

            double[] selectStage = new double[N];
            _populationStages.Add(selectStage);
            for(int i = 0; i < N; ++i)
            {
                int j = Array.BinarySearch(distributionStage, randomStage[i]);
                // BinarySearch returns a negative value if not an exact match, indicating the insertion point
                if (j < 0)
                {
                    j = ~j; // Bitwise complement to get the insertion index
                }
                selectStage[i] = valuesStage[j];
            }

            BitArray[] binStage = new BitArray[N];
            _populationStages.Add(binStage);
            for (int i = 0; i < N; ++i)
            {
                binStage[i] = selectStage[i].ToLong(a, b, l).ToBin(l);
            }
        }

        public void Cross()
        {
            BitArray[] binStage = (BitArray[])_populationStages[(int)PopulationStagesNames.SelectedValsBin];
            BitArray[] parentsStage = new BitArray[N];
            _populationStages.Add(parentsStage);

            for (int i = 0; i < N; ++i)
            {
                parentsStage[i] = (threadLocalRandom.Value.NextDouble() <= pk ? binStage[i] : null);
            }

            BitArray[] childrenStage = new BitArray[N];
            _populationStages.Add(childrenStage);

            bool paired = true;
            int pc = 0, first = 0, last = 0;

            for (int i = 0; i < N; ++i)
            {
                if (parentsStage[i] != null)
                {
                    childrenStage[i] = new BitArray((BitArray)parentsStage[i]);
                    last = i;

                    if (paired)
                    {
                        pc = (int)Math.Round(threadLocalRandom.Value.NextDouble() * (l - 2) + 1);
                        first = i;
                        paired = false;
                    }
                    else
                    {
                        for (int j = pc + 1; j < l; ++j)
                        {
                            childrenStage[first][l - j] = parentsStage[last][l - j];
                            childrenStage[last][l - j] = parentsStage[first][l - j];
                        }
                        paired = true;
                    }
                }
                else
                {
                    childrenStage[i] = null;
                }
            }

            if (!paired)
            {
                parentsStage[last] = null;
                childrenStage[last] = null;
            }

            BitArray[] offspringStage = new BitArray[N];
            _populationStages.Add(offspringStage);
            for (int i = 0; i < N; ++i)
            {
                offspringStage[i] = childrenStage[i] ?? binStage[i];
            }
        }

        public void Mutate()
        {
            BitArray[] offspringStage = (BitArray[])_populationStages[(int)PopulationStagesNames.OffspringVals];

            double[] randomPool = new double[N * l];  // Pre-generate random values
                                                      // Define a thread-local random generator to prevent contention.
                                                      // Use parallel processing for mutation directly

            Parallel.For(0, N, i =>
            {
                var rand = threadLocalRandom.Value;
                for (int j = 0; j < l; j++)
                {
                    if (rand.NextDouble() <= pm)
                    {
                        offspringStage[i][j] = !offspringStage[i][j];
                    }
                }
            });

            BitArrayComparer comp = null;
            if (!InputProcessing.tests)
            {
                Array.Sort(offspringStage, comp = new BitArrayComparer(a, b, prec, l));
            }

            double[] resultStage = new double[N];
            double[] finalGoalStage = new double[N];
            _populationStages.Add(resultStage);
            _populationStages.Add(finalGoalStage);

            for(int i = 0; i < N; ++i)
            {
                double res = (resultStage[i] = offspringStage[i].ToLong().ToReal(a, b, prec, l));
                finalGoalStage[i] = InputProcessing.f(res);
            }

            if (elite)
            {
                int randIndex = threadLocalRandom.Value.Next(0, N);
                if (finalGoalStage[randIndex] < eliteRate)
                {
                    finalGoalStage[randIndex] = eliteRate;
                    resultStage[randIndex] = eliteVal;
                    offspringStage[randIndex] = eliteVal.ToLong(a, b, l).ToBin(l);
                    if (!InputProcessing.tests)
                    {
                        Array.Sort(offspringStage, comp);
                        Array.Sort(resultStage, new RealComparer());
                        Array.Sort(finalGoalStage);
                        Array.Reverse(finalGoalStage);
                    }
                }
            }
        }

        public List<object> GetStages()
        {
            return _populationStages;
        }

        private void Populate()
        {
            double[] generateStage = new double[N];
            _populationStages.Add(generateStage);
            for (int i = 0; i < N; ++i)
            {
                generateStage[i] = Math.Round(threadLocalRandom.Value.NextDouble() * (b - a) + a,
                    prec, MidpointRounding.AwayFromZero);
            }
        }

        public class BitArrayComparer : IComparer
        {
            public BitArrayComparer(double _a, double _b, int _prec, int _l)
            {
                a = _a; b = _b; prec = _prec; l = _l;
            }
            private double a, b;
            private int prec, l;
            public int Compare(Object x, Object y)
            {
                // Calculate f values once for each object
                double fx = InputProcessing.f(((BitArray)x).ToLong().ToReal(a, b, prec, l));
                double fy = InputProcessing.f(((BitArray)y).ToLong().ToReal(a, b, prec, l));

                // Use precomputed values for comparison
                if (fx == fy)
                    return 0;

                return (fx < fy) ? 1 : -1;
            }
        }
        public class RealComparer : IComparer
        {
            public int Compare(Object x, Object y)
            {
                if (InputProcessing.f((double)x) == InputProcessing.f((double)y))
                    return 0;
                return ((InputProcessing.f((double)x) < InputProcessing.f((double)y)) ? 1 : -1);
            }
        }


        private List<object> _populationStages;
    }
}
