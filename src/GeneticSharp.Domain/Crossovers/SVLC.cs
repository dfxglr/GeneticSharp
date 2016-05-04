using System;
using GeneticSharp.Domain.Chromosomes;
using System.Collections.Generic;




namespace GeneticSharp.Domain.Crossovers
{


    public class SVLC : CrossoverBase
    {


        private const int MinimumCommonLength = 4;
        private List<int[]> CommonParts;


        public SVLC(int parentsNumber, int childrenNumber) : base(parentsNumber, childrenNumber)
        {

        }


       public override IList<IChromosome> PerformCross(IList<IChromosome> parents)
       {
           List<IChromosome> children = new List<IChromosome>();
           // Find LCSS of at least n length
           CommonParts = GetSynapsingParts(parents);

           if(CommonParts.Count == 0)
           {
               // No LCSS found - what then?
               return children;
           }

           // Select n random points
                // order by length and choose those more often? Choose all?

           // Number of children (yah, it's a crapload.... :S )
           int listSize = (int) Math.Pow(2, CommonParts.Count + 1);

           for( int i = 1; i < listSize-1; i++)
           {
               // 0 and listSize are 00000 and 11111 meaning exact copies
               //
               //
               // Create offspring #i
               IChromosome offspring = CreateOffspring(parents[0], parents[1], i);

               // Check if it equals parents?  will it ever?


               // Add to children
               children.Add(offspring);
           }

           return children;
       }

       private IChromosome CreateOffspring(IChromosome p1, IChromosome p2, int bitPattern)
       {
            // LSB signifies which parent the first variable part comes from
            // 0 = p1, 1 = p2
            // Shift right and continue, n+1 times
            // Return  resulting chromosome
            var p1_genes = p1.GetGenes();
            var p2_genes = p2.GetGenes();

            List<Gene> child = new List<Gene>();

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
                        // Copy from 0 to first i
                        index_1 = 0;
                        index_2 = CommonParts[i][2];
                    }
                    else
                    {
                        //          index           +    len
                        index_1 = CommonParts[i-1][2] + CommonParts[i-1][0];

                        // at end
                        if(i == CommonParts.Count)
                            index_2 = p2_genes.Length - 1;
                        else
                            index_2 = CommonParts[i][2];
                    }

                    // Only copy if still inside array
                    if(index_1 < p2_genes.Length && index_2 < p2_genes.Length)
                        child.AddRange(new ArraySegment<Gene>(p2_genes, index_1, index_2 - index_1));
                }
                else
                {
                    // Take from p1
                    if(i == 0)
                    {
                        // at start
                        // Copy from 0 to first i
                        index_1 = 0;
                        index_2 = CommonParts[i][1];
                    }
                    else
                    {
                        //          index           +    len
                        index_1 = CommonParts[i-1][1] + CommonParts[i-1][0];

                        // at end
                        if(i == CommonParts.Count)
                            index_2 = p1_genes.Length - 1;
                        else
                            index_2 = CommonParts[i][1];
                    }

                    // Only copy if still inside array
                    if(index_1 < p1_genes.Length && index_2 < p1_genes.Length)
                        child.AddRange(new ArraySegment<Gene>(p1_genes, index_1, index_2 - index_1));
                }

                // shift pattern right
                bitPattern = bitPattern >> 1;
            }


            // Create chromosomes from genes
            IChromosome rchild = p1.CreateNew();
            rchild.Resize(child.Count);
            rchild.ReplaceGenes(0, child.ToArray());
            return rchild;
       }


       private List<int[]> GetSynapsingParts( IList<IChromosome> parents)
       {
           // Calculate common substring
           var table = LCSSTable(parents);

           // Extract, recursively, the locations (in both parent 0 and parent 1) and lengths {len, loc 0, loc 1}
           var parts = SynapsingParts(table);


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
                   if(Table[i,o] > maxLen)
                   {
                       maxLen = Table[i,o];
                       maxI = i;
                       maxO = o;
                   }

               }
           }

           if(maxLen > MinimumCommonLength)
           {

               // Check if Left side is big enough for recursion
               if(maxI > MinimumCommonLength && maxO > MinimumCommonLength)
               {
                   // Copy top-left part of table
                   int[,] leftTable = new int[maxI, maxO];
                   for(int i = 0; i < maxI; i++)
                   {
                       for(int o = 0; o < maxO; o++)
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
               int remainingI = (Table.GetLength(0) - maxI + 1);
               int remainingO = (Table.GetLength(1) - maxO + 1);
               if(remainingI > MinimumCommonLength
                   && remainingO > MinimumCommonLength)
               {
                   // Copy bottom-right part of table
                   int[,] rightTable = new int[remainingI,remainingO];
                   for(int i = 0; i < remainingI; i++)
                   {
                       for(int o = 0; o < remainingO; o++)
                       {
                           rightTable[i,o] = Table[maxI + i,maxO + o];
                       }
                   }

                   // Append recursively
                   results.AddRange(SynapsingParts(rightTable));
               }
           }

           return results;

       }

    }




}
