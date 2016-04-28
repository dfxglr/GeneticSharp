using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Domain.Reinsertions;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Threading;
using GeneticSharp.Infrastructure.Framework.Threading;
using HelperSharp;

namespace GeneticSharp.Domain
{


    /// <summary>
    /// A genetic algorithm (GA) is a search heuristic that mimics the process of natural selection.
    /// This heuristic (also sometimes called a metaheuristic) is routinely used to generate useful solutions
    /// to optimization and search problems. Genetic algorithms belong to the larger class of evolutionary
    /// algorithms (EA), which generate solutions to optimization problems using techniques inspired by natural evolution,
    /// such as inheritance, mutation, selection, and crossover.
    /// <para>
    /// Genetic algorithms find application in bioinformatics, phylogenetics, computational science, engineering,
    /// economics, chemistry, manufacturing, mathematics, physics, pharmacometrics, game development and other fields.
    /// </para>
    /// <see href="http://http://en.wikipedia.org/wiki/Genetic_algorithm">Wikipedia</see>
    /// </summary>
    public sealed class GeneticAlgorithmCCE : IGeneticAlgorithm
    {

		/// <summary>
		///  dummy
		/// </summary>
		public IChromosome BestChromosome { get { return BestChromosomeSet.First(); } }
		// Might wanna fix this


        #region Fields
        private bool m_stopRequested;
        private object m_lock = new object();
        private GeneticAlgorithmState m_state;
        #endregion

		/// <summary>
		/// List of species
		/// </summary>
        public List<CCESpecies> Species { get; set;}

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneticSharp.Domain.GeneticAlgorithmCCE"/> class.
        /// </summary>
        /// <param name="species">The species for this CCE</param>
        public GeneticAlgorithmCCE(List<CCESpecies> species)
        {

            Species = species;
            TimeEvolving = TimeSpan.Zero;
            State = GeneticAlgorithmState.NotStarted;
			TaskExecutorFit = new SmartThreadPoolTaskExecutor()
			{
				MinThreads = 10,
				MaxThreads = 20
			};       
			TaskExecutorGen = new SmartThreadPoolTaskExecutor()
			{
				MinThreads = 10,
				MaxThreads = 20
			};       
        }

        #endregion

        #region Events
        /// <summary>
        /// Occurs when generation ran.
        /// </summary>
        public event EventHandler GenerationRan;

        /// <summary>
        /// Occurs when termination reached.
        /// </summary>
        public event EventHandler TerminationReached;

        /// <summary>
        /// Occurs when stopped.
        /// </summary>
        public event EventHandler Stopped;
        #endregion


		/// <summary>
		/// Gets the generations number.
		/// </summary>
		/// <value>The generations number.</value>
        public int GenerationsNumber {
			get; set;
        }

        /* ChromosomeSet** */
        /// <summary>
        /// Gets the best chromosome.
        /// </summary>
        /// <value>The best chromosome.</value>
        public List<IChromosome> BestChromosomeSet
        {
            get
            {
                return Species.Select(p => p.BestChromosome).ToList();
            }
        }

        /// <summary>
        /// Gets the time evolving.
        /// </summary>
        public TimeSpan TimeEvolving { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        public GeneticAlgorithmState State
        {
            get
            {
                return m_state;
            }

            private set
            {
                var shouldStop = Stopped != null && m_state != value && value == GeneticAlgorithmState.Stopped;

                m_state = value;

                if (shouldStop)
                {
                    Stopped(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
        public bool IsRunning
        {
            get
            {
                return State == GeneticAlgorithmState.Started || State == GeneticAlgorithmState.Resumed;
            }
        }

        /// <summary>
        /// Gets or sets the task executor which will be used to execute generation
        /// </summary>
        public ITaskExecutor TaskExecutorGen { get; set; }

		/// <summary>
		/// Gets or sets the task executor which will be used to execute fitness evaluation.
		/// </summary>
        public ITaskExecutor TaskExecutorFit { get; set; }

		/// <summary>
		/// Termination
		/// </summary>
        public ITermination Termination { get; set; }

		/// <summary>
		/// Fitness
		/// </summary>
        public ICCEFitness Fitness { get; set;}


        #region Methods

        /// <summary>
        /// Starts the genetic algorithm using population, fitness, selection, crossover, mutation and termination configured.
        /// </summary>
        public void Start()
        {
            lock (m_lock)
            {
                State = GeneticAlgorithmState.Started;
                var startDateTime = DateTime.Now;

                // Create initial generations for all species
                foreach(var spec in Species)
                {
                    spec.Population.CreateInitialGeneration();
                }

                TimeEvolving = DateTime.Now - startDateTime;
            }

            Resume();
        }

        /// <summary>
        /// Resumes the last evolution of the genetic algorithm.
        /// <remarks>
        /// If genetic algorithm was not explicit Stop (calling Stop method), you will need provide a new extended Termination.
        /// </remarks>
        /// </summary>
        public void Resume()
        {
            try
            {
                lock (m_lock)
                {
                    m_stopRequested = false;
                }

				if (Species.First().Population.GenerationsNumber == 0)
                {
                    throw new InvalidOperationException("Attempt to resume a genetic algorithm which was not yet started.");
                }
				else if (Species.First().Population.GenerationsNumber > 1)
                {
                    if (Termination.HasReached(this))
                    {
                        throw new InvalidOperationException("Attempt to resume a genetic algorithm with a termination ({0}) already reached. Please, specify a new termination or extend the current one.".With(Termination));
                    }

                    State = GeneticAlgorithmState.Resumed;
                }

                if (EndCurrentGeneration())
                {
                    return;
                }

                bool terminationConditionReached = false;
                DateTime startDateTime;

                do
                {
                    if (m_stopRequested)
                    {
                        break;
                    }

                    startDateTime = DateTime.Now;
					terminationConditionReached = EvolveOneGeneration();
                    //EvolveOneGeneration();
                    TimeEvolving += DateTime.Now - startDateTime;
                }
                while (!terminationConditionReached);
            }
            catch
            {
                State = GeneticAlgorithmState.Stopped;
                throw;
            }
        }

        /// <summary>
        /// Stops the genetic algorithm..
        /// </summary>
        public void Stop()
        {
			if (Species.First().Population.GenerationsNumber == 0)
            {
                throw new InvalidOperationException("Attempt to stop a genetic algorithm which was not yet started.");
            }

            lock (m_lock)
            {
                m_stopRequested = true;
            }
        }

        /// <summary>
        /// Evolve one generation.
        /// </summary>
        /// <returns>True if termination has been reached, otherwise false.</returns>
        private bool EvolveOneGeneration()
        {
            try
            {
                foreach(var spec in Species)
                {
                    TaskExecutorGen.Add( () =>
                            {
                                spec.GenerateChildren();
                            });
                }
                if (!TaskExecutorGen.Start())
                {
                    throw new TimeoutException("The fitness evaluation rech the {0} timeout.".With(TaskExecutorGen.Timeout));
                }
            }
            finally
            {
                TaskExecutorGen.Stop();
                TaskExecutorGen.Clear();
            }

            return EndCurrentGeneration();
        }

        /// <summary>
        /// Ends the current generation.
        /// </summary>
        /// <returns><c>true</c>, if current generation was ended, <c>false</c> otherwise.</returns>
        private bool EndCurrentGeneration()
        {
            foreach(var spec in Species)
            {
                EvaluateFitness(spec);
                spec.Population.EndCurrentGeneration();
            }
			GenerationsNumber++;

            if (GenerationRan != null)
            {
                GenerationRan(this, EventArgs.Empty);
            }

            if (Termination.HasReached(this))
            {
                State = GeneticAlgorithmState.TerminationReached;

                if (TerminationReached != null)
                {
                    TerminationReached(this, EventArgs.Empty);
                }

                return true;
            }

            if (m_stopRequested)
            {
                TaskExecutorGen.Stop();
                TaskExecutorFit.Stop();
                State = GeneticAlgorithmState.Stopped;
            }

            return false;
        }

        /// <summary>
        /// Evaluates the fitness.
        /// </summary>
        private void EvaluateFitness(CCESpecies spec)
        {
            // For each individual we run fitness with a set
            // of best chromosomes, and a random set
            //
            // BestChromosomeList can be used
            // Get random from spec.Population.CurrentGeneration.Chromosomes
            // -- also verify that it is not the same as the best
            //    --- just take any but the first in list (it is ordered)
            //
            //    Use fitness function to get fitness for both sets
            //     Fitness given is best of the two
            try
            {

                foreach(var individual in spec.Population.CurrentGeneration.Chromosomes)
                {
                    List<IChromosome> RandomList = new List<IChromosome>();

                    for(int o = 0; o < Species.Count; o++)
                    {
                        if(Object.ReferenceEquals(Species[o], spec))
                        {
                            // this is to keep it in the same order as the species list, always
                            RandomList.Add(individual);
                            continue;
                        }

						int randIndex;

						if(Species[o].Population.CurrentGeneration.Chromosomes.Count > 1)
                        	randIndex = RandomizationProvider.Current.GetInt(1,Species[o].Population.CurrentGeneration.Chromosomes.Count-1);
						else
							randIndex = 0;

                        RandomList.Add(Species[o].Population.CurrentGeneration.Chromosomes[randIndex]);

                    }


                    TaskExecutorFit.Add(() =>
                            {
                                RunEvaluateFitness(individual, RandomList);
                            });

                }


                if (!TaskExecutorFit.Start())
                {
                    throw new TimeoutException("The fitness evaluation rech the {0} timeout.".With(TaskExecutorFit.Timeout));
                }
            }
            finally
            {
                TaskExecutorFit.Stop();
                TaskExecutorFit.Clear();
            }

            spec.OrderChromosomes();
        }



        /// <summary>
        /// Runs the evaluate fitness.
        /// </summary>
        /// <returns>The evaluate fitness.</returns>
		/// <param name="individual">The chromosome.</param>
		/// <param name="randomSet">The chromosome.</param>
        private object RunEvaluateFitness(IChromosome individual, List<IChromosome> randomSet)
        {

            try
            {
				if(BestChromosomeSet == null || BestChromosomeSet.Count == 0 || BestChromosomeSet.Contains(null))
					individual.Fitness = Fitness.Evaluate(randomSet);
				else
                	individual.Fitness = Math.Max(Fitness.Evaluate(randomSet),
                                         Fitness.Evaluate(BestChromosomeSet));
            }
            catch (Exception ex)
            {
                throw new FitnessException("Error executing Fitness.Evaluate for chromosome: {0}".With(ex.Message), ex);
            }

            return individual.Fitness;
        }



        #endregion
    }
}
