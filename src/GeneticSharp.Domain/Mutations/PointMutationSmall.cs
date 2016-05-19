using System;
using System.Collections;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;


namespace GeneticSharp.Domain.Mutations
{
	/// <summary>
	/// Point mutation small.
	/// </summary>
    public class PointMutationSmall : MutationBase
    {
        // Perform a small mutation at a random point

        // We kind of assume we are dealing with ints....
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


                // Copy gene
                Gene g = chromosome.GetGene(randIndex);

                // Apply changes

                // Get whether we add or subtract 1
                float plusOrMinus = RandomizationProvider.Current.GetFloat();

                // Check if enumerable (list)
                var asIEnum = g.Value as IEnumerable;
                if(asIEnum == null)
                {
                    // Nope. Just change the value a little
                    if(plusOrMinus < 0.5f)
                        g = new Gene((int)g.Value + 1);
                    else
                        g = new Gene((int)g.Value - 1);
                }
                else
                {
                    List<int> newValues = new List<int>();
                    var vi = asIEnum.GetEnumerator();

                    // Loop to last item
                    while(vi.MoveNext())
                    {
                    }

                    // Change the value a little
                    if(plusOrMinus < 0.5f)
                        newValues.Add((int) vi.Current + 1);
                    else
                        newValues.Add((int) vi.Current - 1);

                    g = new Gene(newValues);
                }


                // Reinsert
                chromosome.ReplaceGene(randIndex, g);

            }
        }

    }
}
