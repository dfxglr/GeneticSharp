
using GeneticSharp.Domain.Randomizations;
using System.Linq;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Reinsertions;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Infrastructure.Framework.Threading;



namespace GeneticSharp.Domain
{

	/// <summary>
	/// A Speecies fr use in CCE
	/// </summary>
    public sealed class CCESpecies
    {
        // This class containts everything a species needs in CCE
		public string Name {get;set;}

        #region Constants
        /// <summary>
        /// The default crossover probability.
        /// </summary>
        public const float DefaultCrossoverProbability = 0.75f;

        /// <summary>
        /// The default mutation probability.
        /// </summary>
        public const float DefaultMutationProbability = 0.1f;
        #endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneticSharp.Domain.CCESpecies"/> class.
		/// </summary>
        public CCESpecies()
        {
            CrossoverProbability = DefaultCrossoverProbability;
            MutationProbability = DefaultMutationProbability;


        }

		/// <summary>
		/// Creates the new generation of children.
		/// </summary>
        public void GenerateChildren()
        {
            // Do the whole selection, crossover, etc
            var parents = SelectParents();
            var offspring = Cross(parents);
            Mutate(offspring);
            var newGenerationChromosomes = Reinsert(offspring, parents);
            Population.CreateNewGeneration(newGenerationChromosomes);
        }

		/// <summary>
		/// Orders the chromosomes.
		/// </summary>
        public void OrderChromosomes()
        {
            Population.CurrentGeneration.Chromosomes = Population.CurrentGeneration.Chromosomes.OrderByDescending(c => c.Fitness.Value).ToList();
        }

        /// <summary>
        /// Selects the parents.
        /// </summary>
        /// <returns>The parents.</returns>
        private IList<IChromosome> SelectParents()
        {
            return Selection.SelectChromosomes(Population.MinSize, Population.CurrentGeneration);
        }

        /// <summary>
        /// Crosses the specified parents.
        /// </summary>
        /// <param name="parents">The parents.</param>
        /// <returns>The result chromosomes.</returns>
        private IList<IChromosome> Cross(IList<IChromosome> parents)
        {
            var offspring = new List<IChromosome>();

            for (int i = 0; i < Population.MinSize; i += Crossover.ParentsNumber)
            {
                var selectedParents = parents.Skip(i).Take(Crossover.ParentsNumber).ToList();

                // If match the probability cross is made, otherwise the offspring is an exact copy of the parents.
                // Checks if the number of selected parents is equal which the crossover expect, because the in the end of the list we can
                // have some rest chromosomes.
                if (selectedParents.Count == Crossover.ParentsNumber && RandomizationProvider.Current.GetDouble() <= CrossoverProbability)
                {
                    offspring.AddRange(Crossover.Cross(selectedParents));
                }
            }

            return offspring;
        }


        /// <summary>
        /// Mutate the specified chromosomes.
        /// </summary>
        /// <param name="chromosomes">The chromosomes.</param>
        private void Mutate(IList<IChromosome> chromosomes)
        {
            foreach (var c in chromosomes)
            {
                Mutation.Mutate(c, MutationProbability);
            }
        }

        /// <summary>
        /// Reinsert the specified offspring and parents.
        /// </summary>
        /// <param name="offspring">The offspring chromosomes.</param>
        /// <param name="parents">The parents chromosomes.</param>
        /// <returns>
        /// The reinserted chromosomes.
        /// </returns>
        private IList<IChromosome> Reinsert(IList<IChromosome> offspring, IList<IChromosome> parents)
        {
            return Reinsertion.SelectChromosomes(Population, offspring, parents);
        }
        /// <summary>
        /// Gets the population.
        /// </summary>
        /// <value>The population.</value>
        public IPopulation Population { get; set; }

        /// <summary>
        /// Gets or sets the selection operator.
        /// </summary>
        public ISelection Selection { get; set; }

        /// <summary>
        /// Gets or sets the crossover operator.
        /// </summary>
        /// <value>The crossover.</value>
        public ICrossover Crossover { get; set; }

        /// <summary>
        /// Gets or sets the crossover probability.
        /// </summary>
        public float CrossoverProbability { get; set; }

        /// <summary>
        /// Gets or sets the mutation operator.
        /// </summary>
        public IMutation Mutation { get; set; }

        /// <summary>
        /// Gets or sets the mutation probability.
        /// </summary>
        public float MutationProbability { get; set; }

        /// <summary>
        /// Gets or sets the reinsertion operator.
        /// </summary>
        public IReinsertion Reinsertion { get; set; }


        /// <summary>
        /// Gets the best chromosome.
        /// </summary>
        /// <value>The best chromosome.</value>
        public IChromosome BestChromosome
        {
            get
            {
                return Population.BestChromosome;
            }
        }



    }
}
