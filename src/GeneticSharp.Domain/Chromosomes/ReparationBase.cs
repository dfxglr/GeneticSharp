using System;
using HelperSharp;

namespace GeneticSharp.Domain.Chromosomes
{
	/// <summary>
	/// Reparation base.
	/// </summary>
	public abstract class ReparationBase : IReparation
	{
		/// <summary>
		/// Repair the specified chromosome.
		/// </summary>
		/// <param name="chromosome">Chromosome.</param>
		public void Repair(IChromosome chromosome)
		{
			ExceptionHelper.ThrowIfNull ("chromosome", chromosome);

			PerformRepair (chromosome);
		}

		/// <summary>
		/// Performs the repair.
		/// </summary>
		/// <param name="chromosome">Chromosome.</param>
		protected abstract void PerformRepair(IChromosome chromosome);

	}
}

