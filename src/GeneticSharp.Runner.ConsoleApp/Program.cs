using System;
using System.IO;
using System.Collections.Generic;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Extensions;
using GeneticSharp.Domain.Reinsertions;
using GeneticSharp.Infrastructure.Framework.Reflection;
using GeneticSharp.Infrastructure.Threading;
using GeneticSharp.Runner.ConsoleApp.Samples;

namespace GeneticSharp.Runner.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Run();
        }

        private static void Run()
        {
            Console.SetError(TextWriter.Null);
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("GeneticSharp - ConsoleApp");
            Console.ResetColor();
            Console.WriteLine("Select the sample:");

            var sampleNames = TypeHelper.GetDisplayNamesByInterface<ISampleController>();

            for (int i = 0; i < sampleNames.Count; i++)
            {
                Console.WriteLine("{0}) {1}", i + 1, sampleNames[i]);
            }

            int sampleNumber = 0;
            string selectedSampleName = string.Empty;

            try
            {
                sampleNumber = Convert.ToInt32(Console.ReadLine());
                selectedSampleName = sampleNames[sampleNumber - 1];
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid option.");
            }

            // Do it twice, with CCE
            List<CCESpecies> sp = new List<CCESpecies>();
            sp.Add(new CCESpecies());
            sp.Add(new CCESpecies());

            
            var sampleController1 = TypeHelper.CreateInstanceByName<ISampleController>(selectedSampleName);
            var sampleController2 = TypeHelper.CreateInstanceByName<ISampleController>(selectedSampleName);
            DrawSampleName(selectedSampleName);
            sampleController1.Initialize();
            sampleController2.Initialize();

            Console.WriteLine("Starting...");

            sp[0].Selection = sampleController1.CreateSelection();
            sp[0].Crossover = sampleController1.CreateCrossover();
            sp[0].Mutation = sampleController1.CreateMutation();
			sp [0].Reinsertion = new ElitistReinsertion ();
            sp[0].Population = new Population(100, 200, sampleController1.CreateChromosome());
            sp[0].Population.GenerationStrategy = new PerformanceGenerationStrategy();

			sp[1].Selection = sampleController2.CreateSelection();
			sp[1].Crossover = sampleController2.CreateCrossover();
			sp[1].Mutation = sampleController2.CreateMutation();
			sp [1].Reinsertion = new ElitistReinsertion ();
			sp[1].Population = new Population(100, 200, sampleController2.CreateChromosome());
			sp[1].Population.GenerationStrategy = new PerformanceGenerationStrategy();

            var ga = new GeneticAlgorithmCCE(sp);
			ga.Fitness = new CCEDoubleFitness (sampleController1.CreateFitness(), sampleController2.CreateFitness());
            ga.Termination = sampleController1.CreateTermination();            

            var terminationName = ga.Termination.GetType().Name;

            ga.GenerationRan += delegate
            {
				Console.Clear();
                DrawSampleName(selectedSampleName);

                var bestChromosome = ga.Species[0].Population.BestChromosome;
                Console.WriteLine("Termination: {0}", terminationName);
				Console.WriteLine("Generations: {0}", ga.Species[0].Population.GenerationsNumber);
                Console.WriteLine("Fitness: {0,10}", bestChromosome.Fitness);
                Console.WriteLine("Time: {0}", ga.TimeEvolving);
                sampleController1.Draw(bestChromosome);
            };
			ga.GenerationRan += delegate
			{
				DrawSampleName(selectedSampleName);

				var bestChromosome = ga.Species[1].Population.BestChromosome;
				Console.WriteLine("Termination: {0}", terminationName);
				Console.WriteLine("Generations: {0}", ga.Species[1].Population.GenerationsNumber);
				Console.WriteLine("Fitness: {0,10}", bestChromosome.Fitness);
				Console.WriteLine("Time: {0}", ga.TimeEvolving);
				sampleController2.Draw(bestChromosome);
			};

            try
            {
				//sampleController1.ConfigGA(ga);
				//sampleController2.ConfigGA(ga);
                ga.Start();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine();
                Console.WriteLine("Error: {0}", ex.Message);
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine();
            Console.WriteLine("Evolved.");
            Console.ResetColor();
            Console.ReadKey();
            Run();
        }

        private static void DrawSampleName(string selectedSampleName)
        {
            //Console.Clear();

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("GeneticSharp - ConsoleApp");
            Console.WriteLine();
            Console.WriteLine(selectedSampleName);
            Console.ResetColor();
        }
    }
}
