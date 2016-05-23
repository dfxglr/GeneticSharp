using System;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using System.Collections.Generic;
using GeneticSharp.Domain.Randomizations;



namespace GeneticSharp.Domain.Crossovers
{

	/// <summary>
	/// Synapsing Variable Length Crossover
	/// </summary>
    public class SVLC : CrossoverBase
    {

		#pragma warning disable 162

		private bool _debugging = false;
		private System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        private const int MinimumCommonLength = 10;
		private const int MaxNumCommonParts = 5;

		private const int MinimumCrossoverPoints = 2;
		private const int MaximumCrossoverPoints = 5;
        private List<int[]> CommonParts;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneticSharp.Domain.Crossovers.SVLC"/> class.
		/// </summary>
		/// <param name="parentsNumber">Parents number.</param>
		/// <param name="childrenNumber">Children number.</param>
        public SVLC(int parentsNumber, int childrenNumber) : base(parentsNumber, childrenNumber)
        {

        }

		/// <summary>
		/// Performs the cross with specified parents generating the children.
		/// </summary>
		/// <param name="parents">The parents chromosomes.</param>
		/// <returns>The offspring (children) of the parents.</returns>
       protected override IList<IChromosome> PerformCross(IList<IChromosome> parents)
		{
			if (_debugging) {
				Console.WriteLine ("Performing SVLC on {0} parents", parents.Count);

				if(parents.Count == 2)
					Console.WriteLine ("Getting synapsing parts on parent lengths: {0} {1}", parents[0].Length, parents[1].Length);
				timer.Start ();
			}
           List<IChromosome> children = new List<IChromosome>();
           // Find LCSS of at least n length

           CommonParts = GetSynapsingParts(parents);

			if (_debugging) {
				Console.WriteLine ("Got {0} synapsing parts. Took {1}", CommonParts.Count, timer.Elapsed);
				timer.Stop ();
			}


           if(CommonParts.Count == 0)
           {
               // No LCSS found - what then?
               //return parents;

				if (_debugging) {
					Console.WriteLine ("# parents: {0}", parents.Count);

					if(parents.Count == 2)
					{
						Console.Write("p0 ({0}) : ", parents [0].Length);
						foreach(Gene _g in parents[0].GetGenes())
						{
							List<int> _gl = _g.Value as List<int>;
							if(_gl == null)
								Console.Write("{0} ", _g.Value);
							else
								_gl.ForEach(h => Console.Write("|{0}",h));
						}
						Console.WriteLine (" ");
						Console.Write("p1 ({0})", parents [1].Length);
						foreach(Gene _g in parents[1].GetGenes())
						{
							List<int> _gl = _g.Value as List<int>;
							if(_gl == null)
								Console.Write("{0} ", _g.Value);
							else
								_gl.ForEach(h => Console.Write("|{0}",h));
						}
						Console.WriteLine (" ");
					}
				}


				int randomLength = MinimumCommonLength;


				if (MinimumCommonLength >= Math.Min (parents [0].Length, parents [1].Length)) {
					randomLength = 1;
				}



				int rpoint1 = RandomizationProvider.Current.GetInt (0, parents [0].Length - randomLength - 1);
				int rpoint2 = RandomizationProvider.Current.GetInt (0, parents [1].Length - randomLength - 1);
				int[] rs = new int[3];
				rs [0] = randomLength;
				rs [1] = rpoint1;
				rs [2] = rpoint2;

				CommonParts.Add (rs);

				// Generate X random crossover points
				//int cpoints = RandomizationProvider.Current.GetInt(MinimumCrossoverPoints, MaximumCrossoverPoints);

				//for (int i = 0; i < cpoints; i++) {
				//	
				//}
				if(_debugging)
					Console.WriteLine("Found no synapsing parts. Added |l:{0},i:{1},o:{2}|", randomLength, rpoint1, rpoint2);
           }


			if (CommonParts.Count > MaxNumCommonParts) {
				if (_debugging) {
					Console.WriteLine ("More common parts than maximum. What do?");
				}

				// remove from smallest parts
				//List<int[]> sorted = CommonParts.Sort(
				while (CommonParts.Count > MaxNumCommonParts) {
					int rand = RandomizationProvider.Current.GetInt (0, CommonParts.Count - 1);
					CommonParts.RemoveAt (rand);
				}
			}
           // Select n random points
                // order by length and choose those more often? Choose all?

           // Number of children (yah, it's a crapload.... :S )
           int listSize = (int) Math.Pow(2, CommonParts.Count + 1);

			if (_debugging)
				Console.WriteLine ("Creating {0} children", listSize);

           for( int i = 0; i < listSize; i++)
           {
               // 0 and listSize are 00000 and 11111 meaning exact copies
               //
               //
               // Create offspring #i
               IChromosome offspring = CreateOffspring(parents[0], parents[1], i);

               // Check if it equals parents?  will it ever?


               // Add to children
				if(offspring != null)
               		children.Add(offspring);
           }
								// min pop size
//			while (children.Count < 10) {
//				children.Add (parents [RandomizationProvider.Current.GetInt (0, parents.Count - 1)]);
//			}

			if (_debugging)
				Console.WriteLine ("SVLC returning {0} children", children.Count);

           return children;
       }

       private IChromosome CreateOffspring(IChromosome p1, IChromosome p2, int bitPattern)
       {
            // LSB signifies which parent the first variable part comes from
            // 0 = p1, 1 = p2
            // Shift right and continue, n+1 times
            // Return  resulting chromosome
			Gene[] p1_genes = p1.GetGenes();
			Gene[] p2_genes = p2.GetGenes();

            List<Gene> child = new List<Gene>();


			if (_debugging && false) {
				Console.WriteLine ("creating offspring from parents. Bitpattern: {0}", bitPattern);
			}
            // Copy the correct parts from either parent into child
            for(int i = 0; i <= CommonParts.Count; i++)
            {
                // indices where we cut (crossover points)
                int index_1;
                int index_2;

                // Is LSB 1?
                if((bitPattern & 1) > 0)
                {
                    // Take from p2
                    if(i == 0)
                    {
                        // at start
                        // Copy from 0 to first synapsing parts
                        index_1 = 0;		// First item in non-synapsing part
					}
                    else
                    {
                        //end of last synapsing part + 1 = fist item in non-synapsing part
                        index_1 = CommonParts[i-1][2] + 1;

						if (index_1 >= p2_genes.GetLength(0)) {
							// We're done here, no more genes to add
							break;
						}
					}

					if (i < CommonParts.Count) {
						// First item in synapsing part
						index_2 = Math.Max(CommonParts[i][2] - CommonParts[i][0] + 1,0);	
					} else {
						// We're beyond the list of common parts, copy till end
						index_2 = p2_genes.GetLength(0);
					}

					// Get slice of p2
					ArraySegment<Gene> a = new ArraySegment<Gene> (p2_genes, index_1, index_2 - index_1); 

					// Put it into a list
					List<Gene> tl = new List<Gene>();
					foreach (Gene _g in a.Array) {
						tl.Add (_g);
					}

					// Add them to the child
					child.AddRange (tl);
					if (_debugging && false) {
						Console.Write ("Added to child ({0},{1}):", index_1, index_2 - index_1);
						foreach(Gene _g in tl)
						{
							List<int> _gl = _g.Value as List<int>;
							if(_gl == null)
								Console.Write("{0} ", _g.Value);
							else
								_gl.ForEach(h => Console.Write("|{0}",h));
						}
						Console.WriteLine (" ");
					}
                }
                else
				{
					// Take from p1
					if(i == 0)
					{
						// at start
						// Copy from 0 to first synapsing parts
						index_1 = 0;		// First item in non-synapsing part
					}
					else
					{
						//end of last synapsing part + 1 = fist item in non-synapsing part
						index_1 = CommonParts[i-1][1] + 1;

						if (index_1 >= p1_genes.GetLength(0)) {
							// We're done here, no more genes to add
							break;
						}
					}

					if (i < CommonParts.Count) {
						// First item in synapsing part
						index_2 = Math.Max(CommonParts[i][1] - CommonParts[i][0] + 1,0);	
					} else {
						// We're beyond the list of common parts, copy till end
						index_2 = p1_genes.GetLength(0);
					}

					// Get slice of p1
					ArraySegment<Gene> a = new ArraySegment<Gene> (p1_genes, index_1, index_2 - index_1); 

					// Put it into a list
					List<Gene> tl = new List<Gene>();
					foreach (Gene _g in a.Array) {
						tl.Add (_g);
					}

					// Add them to the child
					child.AddRange (tl);
					if (_debugging && false) {
						Console.Write ("Added to child ({0},{1}):", index_1, index_2 - index_1);
						foreach(Gene _g in tl)
						{
							List<int> _gl = _g.Value as List<int>;
							if(_gl == null)
								Console.Write("{0} ", _g.Value);
							else
								_gl.ForEach(h => Console.Write("|{0}",h));
						}
						Console.WriteLine (" ");
					}
                }

                // shift pattern right
                bitPattern = bitPattern >> 1;

				if (_debugging && false) {
					Console.Write("Child so far:");
					foreach(Gene _g in child)
					{
						List<int> _gl = _g.Value as List<int>;
						if(_gl == null)
							Console.Write("{0} ", _g.Value);
						else
							_gl.ForEach(h => Console.Write("|{0}",h));
					}
					Console.WriteLine (" ");
				}
            }


			if (child.Count < 2)
				return null;

            // Create chromosomes from genes
            IChromosome rchild = p1.CreateNew();
			if (child.Count < 2 && _debugging) {
				Console.WriteLine ("About to break. Added this as bp");
			}
            rchild.Resize(child.Count);
            rchild.ReplaceGenes(0, child.ToArray());
            return rchild;
       }


       private List<int[]> GetSynapsingParts( IList<IChromosome> parents)
       {
			if (_debugging) {
				Console.WriteLine ("Creating LCSS table");
			}
           // Calculate common substring
			var table = LCSSTable(parents);
			if (_debugging) {
				Console.WriteLine ("Getting synapsing parts from LCSS table");
			}

           // Extract, recursively, the locations (in both parent 0 and parent 1) and lengths {len, loc 0, loc 1}
           var parts = SynapsingParts(table);


			if (_debugging) {
				Console.WriteLine ("Got synapsing parts!");
			}
           return parts;
       }

       // Calculate table used for finding longest common substring
       private int[,] LCSSTable(IList<IChromosome> parents)
       {

		   
           int[,] table = new int[parents[0].Length, parents[1].Length];


           for(int i = 0; i < parents[0].Length; i++)
           {
               for(int o = 0; o < parents[1].Length; o++)
               {

                   if(parents[0].GetGene(i) != parents[1].GetGene(o))
                   {
                       table[i,o] = 0;
                   }
                   else
                   {
                       if(i == 0 || o == 0)
                       {
                           table[i,o] = 0;
                       }
                       else
                       {
                           table[i,o] = 1 + table[i-1,o-1];
                       }
                   }
               }

           }

			return table;

       }

       // Recursive function to find similar parts. Returns list
       // ordered so the first parts are first in the last
       private List<int[]> SynapsingParts( int[,] Table)
       {
           int maxI = 0;
           int maxO = 0;
           int maxLen = 0;
           List<int[]> results = new List<int[]>();

           for(int i = 0; i < Table.GetLength(0); i++)
           {
               for(int o = 0; o < Table.GetLength(1); o++)
               {

					// correct value, so it holds true for subtables too
					var tValue = Math.Min (Table [i, o],
						             		Math.Min (i, o) + 1);
                   if(tValue > maxLen)
                   {
                       maxLen = tValue;
                       maxI = i;
                       maxO = o;
                   }

               }
           }



           if(maxLen > MinimumCommonLength)
           {

				int LTSize_i = maxI - maxLen + 1;
				int LTSize_o = maxO - maxLen + 1;

               // Check if Left side is big enough for recursion
               if(LTSize_i >= MinimumCommonLength && LTSize_o >= MinimumCommonLength)
               {
                   // Copy top-left part of table
                   int[,] leftTable = new int[LTSize_i, LTSize_o];

                   for(int i = 0; i < LTSize_i; i++)
                   {
                       for(int o = 0; o < LTSize_o; o++)
                       {
                           leftTable[i,o] = Table[i,o];
                       }
                   }

                   // Append recursively
                   results.AddRange(SynapsingParts(leftTable));
               }

               // Add the longest part from the "middle"
               results.Add(new int[3] {maxLen, maxI, maxO});

               // Check if Right side is big enough for recursion
				int RTSize_i = Table.GetLength(0) - maxI - 1;
				int RTSize_o = Table.GetLength (1) - maxO - 1;

               if(RTSize_i >= MinimumCommonLength
                   && RTSize_o >= MinimumCommonLength)
               {
                   // Copy bottom-right part of table
                   int[,] rightTable = new int[RTSize_i,RTSize_o];


					for(int i = 0; i < RTSize_i; i++)
                   {
						for(int o = 0; o < RTSize_o; o++)
                       {
							try
							{
                           		rightTable[i,o] = Table[maxI + i + 1,maxO + o + 1];
							}
							catch{
								Console.WriteLine ("Broke. This is a bp");
							};
                       }
					}

                   // Append recursively - correct to fit within this table coordinae
					var RTableResults = SynapsingParts(rightTable);
					foreach (var RTres in RTableResults) {
						//RTres[0] = .. same
						RTres [1] += maxI + 1;	// RTable pos + the range up till that (same as loop above)
						RTres [2] += maxO + 1;	//  ---- || ----
					}
                   results.AddRange(RTableResults);
               }
           }

           return results;

       }

    }




}
