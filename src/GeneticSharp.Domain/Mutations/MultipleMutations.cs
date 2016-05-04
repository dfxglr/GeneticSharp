using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using System.Collections.Generic;
using System.Linq;



namespace GeneticSharp.Domain.Mutations
{
    public class MultipleMutations :  MutationBase
    {
        public List<IMutation> Mutations;
        public List<float> RelativeProbabilites;


        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            if(RandomizationProvider.Current.GetFloat() > probability)
                return;

            // Randomly choose between multiple mutations
            float ProbSum = RelativeProbabilites.Sum();
            float SumSoFar = 0f;

            float R = RandomizationProvider.Current.GetFloat();

            for(int i=0; i < RelativeProbabilites.Count; i++)
            {
                if(R < SumSoFar/ProbSum)
                {
                    // Do the mutation (100% chance, as we already covered that part in this function (see top)
                    Mutations[i].Mutate(chromosome, 1.0f);
                    break;
                }

                SumSoFar += RelativeProbabilites[i];
            }


        }
    }
}
