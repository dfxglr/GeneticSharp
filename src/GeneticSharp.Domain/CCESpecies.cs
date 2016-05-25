using System;
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

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name {get;set;}

		/// <summary>
		/// Gets or sets the I.
		/// </summary>
		/// <value>The I.</value>
		public int ID { get; set;}

		/// <summary>
		/// The offspring.
		/// </summary>
		public IList<IChromosome> Offspring;

		/// <summary>
		///  The parents
		/// </summary>
		public IList<IChromosome> Parents;


		private bool _debugging = false;
		private System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

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
			if (_debugging) {
				Console.WriteLine ("Species '{0}' generating children", Name);
				timer.Start ();
			}
			OrderChromosomes ();

            // Do the whole selection, crossover, etc
            Parents = SelectParents();

			if (_debugging) {
				Console.WriteLine ("Species '{0}' selected parents. Took {1}", Name, timer.Elapsed.ToString ());
				timer.Stop ();
				timer.Start ();
			}

            Offspring = Cross(Parents);

			if (_debugging) {
				Console.WriteLine ("Species '{0}' did crossover. Took {1}", Name, timer.Elapsed.ToString ());
				timer.Stop ();
				timer.Start ();
			}

            Mutate(Offspring);

			if (_debugging) {
				Console.WriteLine ("Species '{0}' mutated. Took {1}", Name, timer.Elapsed.ToString ());
				timer.Stop ();
			}

			if (_debugging) {
				Console.WriteLine ("Species '{0}' repairing.", Name);
				timer.Start ();
			}

			Repair (Offspring);

			if (_debugging) {
				Console.WriteLine ("Species '{0}' repaired. Took {1}", Name, timer.Elapsed);
				timer.Stop ();
			}


        }

		/// <summary>
		/// Ends the current generation.
		/// </summary>
		public void EndCurrentGeneration()
		{            
			if (_debugging) {
				timer.Start ();
			}							

			//var coll =  Population.CurrentGeneration.Chromosomes == null ? Offspring : Population.CurrentGeneration.Chromosomes;// parents not used
			var coll = Offspring;

			// null in first round I think
			if (coll == null)
				coll = Population.CurrentGeneration.Chromosomes;


			if (_debugging) {
				if (coll == Offspring) {
					Console.WriteLine ("Collection is Offspring");
					if (coll == null)
						Console.WriteLine ("IT IS NULL");
				} else if (coll == Population.CurrentGeneration.Chromosomes) {
					Console.WriteLine ("Collection is CurrentGeneration.Chromosomes");
				}
				Console.WriteLine ("{0} in the coll list", coll.Count);
			}

			var newGenerationChromosomes = Reinsert(coll, Parents);


			if (_debugging) {
				Console.WriteLine ("{0} new chromosomes returned from reinsertion", newGenerationChromosomes.Count);
				Console.WriteLine ("Species '{0}' reinserted. Took {1}", Name, timer.Elapsed.ToString ());
				timer.Stop ();
				timer.Start ();
			}

			OrderChromosomes ();

			Population.EndCurrentGeneration ();


			Population.CreateNewGeneration(newGenerationChromosomes);

			if (_debugging) {
				Console.WriteLine ("Species '{0}' created new generation. Took {1}", Name, timer.Elapsed.ToString ());
				timer.Stop ();
				//timer.Start ();
			}


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
			if(_debugging)
				Console.WriteLine("Selecting parents from");
			
			var _selection = Selection.SelectChromosomes(Population.MinSize, Population.CurrentGeneration);

			if (_debugging)
				Console.WriteLine ("Selected {0} parents", _selection.Count);
			
			return _selection; 
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

			if (_debugging) {
				Console.WriteLine ("{0} parents created {1} children", parents.Count, offspring.Count);
				if (offspring.Count < parents.Count) {
					Console.WriteLine ("Time for debugging! This is a bp");
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


		private void Repair(IList<IChromosome> chromosomes)
		{
			foreach (var c in chromosomes) {
				Reparation.Repair (c);
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
			
			try{
			while (offspring.Count < Population.MinSize)
			{
				offspring.Add(parents [RandomizationProvider.Current.GetInt (0, parents.Count - 1)]);
//				if(offspring.Count == 0)
//					offspring.Add(parents.First().CreateNew());
//				else
//					offspring.Add(offspring.First().CreateNew());
			}

			if(parents == null)
				parents = new List<IChromosome>();

			return Reinsertion.SelectChromosomes(Population, offspring, parents);
			}
			catch {
				Console.WriteLine ("GET ON THE GROUND FUCKO THIS IS A BP");
				return null;
			}
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
		/// Gets or sets the reparation.
		/// </summary>
		/// <value>The reparation.</value>
		public IReparation Reparation { get; set; }

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
