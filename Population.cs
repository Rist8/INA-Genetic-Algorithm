using System;
using System.Collections;
using System.Linq;
using static GeneticAlgorithm.InputProcessing;
using static UtilityNamespace.Utility;

namespace GeneticAlgorithm
{
    public class Population
    {
        public enum PopulationStagesNames
        {
            Indexes,
            InitialVals,
            GoalVals,
            RatingVals,
            SelectProbs,
            DistributionVals,
            SelectRandomVals,
            SelectedVals,
            SelectedValsBin,
            ParentsVals,
            PairsCuts,
            ChildrenVals,
            OffspringVals,
            MutateVals,
            ResultVals,
            FinalGoalVals
        }
        public Population(Population population = null) 
        {
            ArrayList indexColumn;
            _populationStages.Add(indexColumn = new ArrayList());
            for (int i = 0; i < InputProcessing.N; ++i)
            {
                indexColumn.Add(i + 1);
            }

            if (population == null)
            {
                Populate();
            }
            else
            {
                _populationStages.Add(population._populationStages.ToArray().Last());
            }
            Evaluate();
        }

        public void Evaluate()
        {
            ArrayList valuesStage = (ArrayList)_populationStages[(int)PopulationStagesNames.InitialVals];
            ArrayList goalStage;
            _populationStages.Add(goalStage = new ArrayList());
            for(int i = 0; i < N; ++i)
            {
                goalStage.Add(f((Double)valuesStage[i]));
            }
            ArrayList rateStage;
            _populationStages.Add(rateStage = new ArrayList());
            double gsum = 0;
            for (int i = 0; i < N; ++i)
            {
                rateStage.Add(g((Double)valuesStage[i]));
                gsum += (Double)rateStage[i];
            }
            ArrayList probStage;
            _populationStages.Add(probStage = new ArrayList());
            for (int i = 0; i < N; ++i)
            {
                probStage.Add(g((Double)valuesStage[i]) / gsum);
            }
            ArrayList distributionStage;
            _populationStages.Add(distributionStage = new ArrayList());
            for (int i = 0; i < N; ++i)
            {
                distributionStage.Add((Double)probStage[i] + ((i != 0) ? (Double)distributionStage[i - 1] : 0));
            }
        }

        public void Select()
        {
            ArrayList distributionStage = (ArrayList)_populationStages[(int)PopulationStagesNames.DistributionVals];
            ArrayList valuesStage = (ArrayList)_populationStages[(int)PopulationStagesNames.InitialVals];
            ArrayList randomStage;
            _populationStages.Add(randomStage = new ArrayList());
            for (int i = 0; i < N; ++i)
            {
                randomStage.Add(myRandom.NextDouble());
            }
            ArrayList selectStage;
            _populationStages.Add(selectStage = new ArrayList());
            for (int i = 0; i < N; ++i)
            {
                for (int j = 0; j < N; ++j)
                {
                    if ((Double)randomStage[i] <= (Double)distributionStage[j])
                    {
                        selectStage.Add(valuesStage[j]);
                        break;
                    }
                }
            }
            ArrayList binStage;
            _populationStages.Add(binStage = new ArrayList());
            for (int i = 0; i < N; ++i)
            {
                binStage.Add(((Double)selectStage[i]).ToLong().ToBin());
            }
        }

        public void Cross()
        {
            ArrayList binStage = (ArrayList)_populationStages[(int)PopulationStagesNames.SelectedValsBin];
            ArrayList parentsStage;
            _populationStages.Add(parentsStage = new ArrayList());
            for (int i = 0; i < N; ++i)
            {
                if (myRandom.NextDouble() <= pk)
                {
                    parentsStage.Add(binStage[i]);
                } 
                else
                {
                    parentsStage.Add(null);
                }
            }
            ArrayList pairsStage, childrenStage;
            _populationStages.Add(pairsStage = new ArrayList());
            _populationStages.Add(childrenStage = new ArrayList());
            bool paired = true;
            int pc = 0, first = 0, last = 0;
            for (int i = 0; i < N; ++i)
            {
                if (parentsStage[i] != null)
                {
                    childrenStage.Add(new BitArray((BitArray)parentsStage[i]));
                    last = i;
                    if (paired)
                    {
                        pc = (int)Math.Round(myRandom.NextDouble() * (l - 2) + 1);
                        pairsStage.Add(pc);
                        first = i;
                        paired = false;
                    }
                    else
                    {
                        pairsStage.Add(pc);
                        BitArray firstChild = (BitArray)childrenStage[first];
                        for(int j = pc + 1; j <= l; ++j)
                        {
                            firstChild[l - j] = ((BitArray)parentsStage[last])[l - j];
                        }
                        BitArray secondChild = (BitArray)childrenStage[last];
                        for (int j = pc + 1; j <= l; ++j)
                        {
                            secondChild[l - j] = ((BitArray)parentsStage[first])[l - j];
                        }
                        paired = true;
                    }
                }
                else
                {
                    childrenStage.Add(null);
                    pairsStage.Add(null);
                }
            }
            if (!paired)
            {
                parentsStage[last] = null;
                childrenStage[last] = null;
                pairsStage[last] = null;
            }

            ArrayList offspringStage;
            _populationStages.Add(offspringStage = new ArrayList());

            for (int i = 0; i < N; ++i)
            {
                if (childrenStage[i] == null)
                {
                    offspringStage.Add(binStage[i]);
                }
                else
                {
                    offspringStage.Add(childrenStage[i]);
                }
            }

        }

        public void Mutate()
        {
            ArrayList mutateStage;
            ArrayList offspringStage = (ArrayList)_populationStages[(int)PopulationStagesNames.OffspringVals];
            _populationStages.Add(mutateStage = new ArrayList());
            for(int i = 0; i < N; ++i)
            {
                BitArray currentSpecimen;
                mutateStage.Add(currentSpecimen = new BitArray((BitArray)offspringStage[i]));
                BitArray changedGenes;
                mutateStage.Add(changedGenes = new BitArray(l));
                changedGenes.SetAll(false);
                for(int j = 0; j < l; ++j)
                {
                    if(myRandom.NextDouble() <= pm)
                    {
                        currentSpecimen[j] = !currentSpecimen[j];
                        changedGenes[l - j - 1] = true;
                    }
                }
            }
            ArrayList resultStage, finalGoalStage;
            _populationStages.Add(resultStage = new ArrayList());
            _populationStages.Add(finalGoalStage = new ArrayList());
            for(int i = 0;i < N; ++i)
            {
                resultStage.Add(((BitArray)mutateStage[2 * i]).ToLong().ToReal());
                finalGoalStage.Add(f((Double)resultStage[i]));
            }
        }

        public ArrayList GetStages()
        {
            return _populationStages;
        }

        private void Populate()
        {
            ArrayList generateStage;
            _populationStages.Add(generateStage = new ArrayList());
            for (int i = 0; i < N; ++i)
            {
                generateStage.Add(
                    Math.Round(
                        myRandom.NextDouble() * (b - a) + a,
                        prec,
                        MidpointRounding.AwayFromZero)
                    );
            }
        }
        private ArrayList _populationStages = new ArrayList();
    }
}
