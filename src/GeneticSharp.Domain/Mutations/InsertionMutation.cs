using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;


namespace GeneticSharp.Domain.Mutations
{


    public class InsertionMutation
    {
        public int MinInsertionSize = 1;
        public int MaxInsertionSize = 5;


        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            if(RandomizationProvider.Current.GetFloat() < probability)
            {
                // Get random insertion size and index
                int insertionSize = RandomizationProvider.Current.GetInt(MinInsertionSize, MaxInsertionSize);
                int index = RandomizationProvider.Current.GetInt(0,chromosome.Length - insertionSize - 1);


                // Generate new genes for insertion
                List<Gene> newGenes = new List<Gene>();

                for(int i = 0; i < insertionSize; i++)
                {
                    newGenes.Add(chromosome.GenerateGene(index + i));
                }

                // Get genes from chromosome and insert generated ones
                List<Gene> genes = new List<Gene>();
                genes.AddRange(chromosome.GetGenes());
                genes.InsertRange(index, newGenes);


                // Resize chromosome and replace with new
                chromosome.Resize(chromosome.Length + insertionSize);

                chromosome.ReplaceGenes(index, genes.ToArray());
            }
        }
    }


}
