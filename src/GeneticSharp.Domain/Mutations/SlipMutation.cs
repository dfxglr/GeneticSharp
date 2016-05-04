using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;


namespace GeneticSharp.Domain.Mutations
{


    public class SlipMutation
    {
        public int MinSlipSize = 1;
        public int MaxSlipSize = 5;


        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            if(RandomizationProvider.Current.GetFloat() < probability)
            {
                // Get slip size, source and destination indicies
                int slipSize = RandomizationProvider.Current.GetInt(MinSlipSize, MaxSlipSize);
                int index = RandomizationProvider.Current.GetInt(0, chromosome.Length - slipSize - 1);

                float insertAfter = RandomizationProvider.Current.GetFloat();


                // Get the genes in list form
                List<Gene> genes = new List<Gene>();
                genes.AddRange(chromosome.GetGenes());

                // Copy and insert a range as per above parameters
                if(insertAfter < 0.5f)
                    genes.InsertRange(index, genes.GetRange(index, slipSize));
                else
                    genes.InsertRange(index+slipSize, genes.GetRange(index, slipSize));

                // Resize and replace with new genome
                chromosome.Resize(chromosome.Length + slipSize);
                chromosome.ReplaceGenes(0, genes.ToArray());
            }
        }
    }
}


