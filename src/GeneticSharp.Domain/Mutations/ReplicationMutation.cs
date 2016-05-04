using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;


namespace GeneticSharp.Domain.Mutations
{


    public class ReplicationMutation
    {
        public int MinReplicationSize = 1;
        public int MaxReplicationSize = 5;


        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            if(RandomizationProvider.Current.GetFloat() < probability)
            {
                // Get replication size, source and destination indicies
                int replSize = RandomizationProvider.Current.GetInt(MinReplicationSize, MaxReplicationSize);
                int index = RandomizationProvider.Current.GetInt(0, chromosome.Length - replSize - 1);

                int insertionIndex = RandomizationProvider.Current.GetInt(0, chromosome.Length);


                // Get the genes in list form
                List<Gene> genes = new List<Gene>();
                genes.AddRange(chromosome.GetGenes());

                // Copy and insert a range as per above parameters
                genes.InsertRange(insertionIndex, genes.GetRange(index, replSize));

                // Resize and replace with new genome
                chromosome.Resize(chromosome.Length + replSize);
                chromosome.ReplaceGenes(0, genes.ToArray());
            }
        }
    }
}


