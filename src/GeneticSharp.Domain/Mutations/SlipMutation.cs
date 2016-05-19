using System.Collections.Generic;
using System;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;


namespace GeneticSharp.Domain.Mutations
{

	/// <summary>
	/// Slip mutation.
	/// </summary>
    public class SlipMutation : MutationBase
    {
		/// <summary>
		/// The minimum size of the slip.
		/// </summary>
        public int MinSlipSize = 1;
        
		/// <summary>
		/// The size of the max slip.
		/// </summary>
		public int MaxSlipSize = 5;

		/// <summary>
		/// Mutate the specified chromosome.
		/// </summary>
		/// <param name="chromosome">The chromosome.</param>
		/// <param name="probability">The probability to mutate each chromosome.</param>
        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            if(RandomizationProvider.Current.GetFloat() < probability)
            {
                // Get slip size, source and destination indicies
				int slipSize = RandomizationProvider.Current.GetInt(MinSlipSize, Math.Min(MaxSlipSize,chromosome.Length));
				int index = RandomizationProvider.Current.GetInt(0, chromosome.Length - (slipSize - 1));

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
				chromosome.Resize(genes.Count);
                chromosome.ReplaceGenes(0, genes.ToArray());
            }
        }
    }
}


