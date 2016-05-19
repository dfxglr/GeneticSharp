using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;


namespace GeneticSharp.Domain.Mutations
{

	/// <summary>
	/// Replication mutation.
	/// </summary>
    public class ReplicationMutation : MutationBase
    {
		/// <summary>
		/// The minimum size of the replication.
		/// </summary>
        public int MinReplicationSize = 1;
        
		/// <summary>
		/// The size of the max replication.
		/// </summary>
		public int MaxReplicationSize = 5;

		/// <summary>
		/// Mutate the specified chromosome.
		/// </summary>
		/// <param name="chromosome">The chromosome.</param>
		/// <param name="probability">The probability to mutate each chromosome.</param>
        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            if(RandomizationProvider.Current.GetFloat() < probability)
            {
                // Get replication size, source and destination indicies
				int replSize = RandomizationProvider.Current.GetInt(MinReplicationSize, Math.Min(MaxReplicationSize, chromosome.Length));
				int index = RandomizationProvider.Current.GetInt(0, chromosome.Length - (replSize - 1));

                int insertionIndex = RandomizationProvider.Current.GetInt(0, chromosome.Length);


                // Get the genes in list form
                List<Gene> genes = new List<Gene>();
                genes.AddRange(chromosome.GetGenes());

                // Copy and insert a range as per above parameters
                genes.InsertRange(insertionIndex, genes.GetRange(index, replSize));

                // Resize and replace with new genome
				chromosome.Resize(genes.Count);
                chromosome.ReplaceGenes(0, genes.ToArray());
            }
        }
    }
}


