using System;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using System.Collections.Generic;
using System.Linq;



namespace GeneticSharp.Domain.Mutations
{
	/// <summary>
	/// Multiple mutations.
	/// </summary>
    public class MultipleMutations :  MutationBase
    {
		/// <summary>
		/// The mutations.
		/// </summary>
		public List<IMutation> Mutations;
        
		/// <summary>
		/// The relative probabilites.
		/// </summary>
		public List<float> RelativeProbabilites;


		public MultipleMutations() : base()
		{
			Mutations = new List<IMutation> ();
			RelativeProbabilites = new List<float> ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneticSharp.Domain.Mutations.MultipleMutations"/> class.
		/// </summary>
		/// <param name="mutations">Mutations.</param>
		/// <param name="relprobs">Relprobs.</param>
		public MultipleMutations(List<IMutation> mutations, List<float> relprobs) : base()
		{
			Mutations = mutations;
			RelativeProbabilites = relprobs;
		}

		/// <summary>
		/// Mutate the specified chromosome.
		/// </summary>
		/// <param name="chromosome">The chromosome.</param>
		/// <param name="probability">The probability to mutate each chromosome.</param>
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
                    
					try
					{
						Mutations[i].Mutate(chromosome, 1.0f);
					}
					catch {
						Console.WriteLine ("Mutation broke. This is a bp");
					};
                    break;
                }

                SumSoFar += RelativeProbabilites[i];
            }


        }
    }
}
