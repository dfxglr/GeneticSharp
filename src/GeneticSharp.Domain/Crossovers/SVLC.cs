using GeneticSharp.Domain.Chromosomes;
using System.Collections.Generic;




namespace GeneticSharp.Domain.Crossovers
{


    public class SVLC : CrossoverBase
    {


        private const int MinimumCommonLength = 4;


        public SVLC(int parentsNumber, int childrenNumber) : base(parentsNumber, childrenNumber)
        {

        }


       public override IList<IChromosome> PerformCross(IList<IChromosome> parents)
       {
           List<IChromosome> children = new List<IChromosome>();
           // Find LCSS of at least n length
           List<int[]> CommonParts = GetSynapsingParts(parents);

           if(CommonParts.Count == 0)
           {
               // No LCSS found - what then?
           }

           // Select n random points
                // order by length and choose those more often? Choose all?


           // Crossover at those points
           //       // Use one/two point crossover?

           return children;
       }


       private List<int[]> GetSynapsingParts( IList<IChromosome> parents)
       {
           // Calculate common substring
           var table = LCSSTable(parents);

           // Extract, recursively, the locations (in both parent 0 and parent 1) and lengths {len, loc 0, loc 1}
           var parts = SynapsingParts(table);


           return parts;
       }

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
