using System;

namespace GeneticSharp.Domain.Chromosomes
{
	/// <summary>
	/// I reparation.
	/// </summary>
	public interface IReparation
	{

		/// <summary>
		/// Repair the specified chromosome.
		/// </summary>
		/// <param name="chromosome">Chromosome.</param>
		void Repair (IChromosome chromosome);
	}
}

