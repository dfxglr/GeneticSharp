using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;



namespace GeneticSharp.Domain.Mutations
{
	/// <summary>
	/// Deletion mutation.
	/// </summary>
	public class DeletionMutation : MutationBase
    {
		/// <summary>
		/// The minimum size of the deletion.
		/// </summary>
        public int MinDeletionSize = 1;
       
		/// <summary>
		/// The size of the max deletion.
		/// </summary>
		public int MaxDeletionSize = 5;

		/// <summary>
		/// Mutate the specified chromosome.
		/// </summary>
		/// <param name="chromosome">The chromosome.</param>
		/// <param name="probability">The probability to mutate each chromosome.</param>
        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            if(RandomizationProvider.Current.GetFloat() < probability)
            {
                // Get size of deletion block and index of it
                int delSize = RandomizationProvider.Current.GetInt(MinDeletionSize, MaxDeletionSize);


				if (chromosome.Length - delSize <= 2)
					return;

                int index = RandomizationProvider.Current.GetInt(0, chromosome.Length - delSize - 1);


                // Get the genes as a list
                List<Gene> genes = new List<Gene>();
                genes.AddRange(chromosome.GetGenes());

                // Remove the given range
                genes.RemoveRange(index, delSize);

                // Resize the chromosome and replace with the mutated one
				chromosome.Resize(genes.Count);
                chromosome.ReplaceGenes(0, genes.ToArray());
            }
        }




    }
}
