using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Chromosomes;

namespace GeneticSharp.Extensions
{
	public class CCEDoubleFitness : ICCEFitness
	{
		public IFitness f1;
		public IFitness f2;

		public CCEDoubleFitness(IFitness _f1, IFitness _f2)
		{
			f1 = _f1;
			f2 = _f2;
		}

		public double Evaluate(List<IChromosome> chromosomes)
		{
			return f1.Evaluate (chromosomes [0]) + f2.Evaluate (chromosomes [1]);
		}
	}
}

