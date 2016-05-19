using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Domain.Chromosomes;




namespace GeneticSharp.Domain.Mutations
{
	/// <summary>
	/// Point mutation large.
	/// </summary>
    public class PointMutationLarge : MutationBase
    {

		/// <summary>
		/// Mutate the specified chromosome.
		/// </summary>
		/// <param name="chromosome">The chromosome.</param>
		/// <param name="probability">The probability to mutate each chromosome.</param>
        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            if(RandomizationProvider.Current.GetFloat() < probability)
            {
                int randIndex = RandomizationProvider.Current.GetInt(0,chromosome.Length - 1);

                // Create a new randon gene in place of the other
                chromosome.ReplaceGene(randIndex, chromosome.GenerateGene(randIndex));
            }
        }
    }
}
