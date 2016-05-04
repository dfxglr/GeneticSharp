using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;



namespace GeneticSharp.Domain.Mutations
{

    public class DeletionMutation
    {

        public int MinDeletionSize = 1;
        public int MaxDeletionSize = 5;


        public override void PerformMutate(IChromosome chromosome, float probability)
        {
            if(RandomizationProvider.Current.GetFloat() < probability)
            {
                // Get size of deletion block and index of it
                int delSize = RandomizationProvider.Current.GetInt(MinDeletionSize, MaxDeletionSize);

                int index = RandomizationProvider.Current.GetInt(0, chromosome.Length - delSize - 1);


                // Get the genes as a list
                List<Gene> genes = new List<Gene>();
                genes.AddRange(chromosome.GetGenes());

                // Remove the given range
                genes.RemoveRange(index, delSize);

                // Resize the chromosome and replace with the mutated one
                chromosome.Resize(chromosome.Length - delSize);
                chromosome.ReplaceGenes(0, genes.ToArray());
            }
        }




    }
}
