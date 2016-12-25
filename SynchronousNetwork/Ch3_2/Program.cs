using System;
using System.IO;
using QSharpTest.Scheme.Classical.Graphs.Mocks;

namespace Ch3_2
{
    /// <summary>
    ///  The program that tests distributed MST
    /// </summary>
    class Program
    {
        static void Test(TextWriter tw)
        {
            //var n = MSTNode.CreateNetwork(100, 5);
            //MSTNode.Run(n);

            var rand = new Random(123);

            for (var itest = 0; itest < 10000; itest++)
            {
                var numpts = rand.Next(3, 200);
                var n = MSTNode.CreateNetwork(100, numpts, 1, 50, 0.5, rand);

                var g = MSTTest.GetGraph(n);    // generates the graph to feed to the tree markers
                var tmda = new TreeMarker(g);   // the treemarker for the da-MST
                var tmseq = new TreeMarker(g);  // the treemarkder for the sequantial MST, the reference solver
                

                MSTTest.Run(n, tmda);               // run the da-MST
                MSTTest.MSTSequential(g, tmseq);    // runs the reference solver
                var succ = true;

                if (tmda.ContainsLoop)
                {
                    tw.WriteLine("da contains loop, test {0} failed", itest);
                    succ = false;
                }
                if (tmda.TotalWeight.Value != tmseq.TotalWeight.Value)
                {
                    tw.WriteLine("da {0} vs seq {1}, test {2} failed", tmda.TotalWeight.Value,
                        tmseq.TotalWeight.Value, itest);
                    succ = false;
                }
                if (succ)
                {
                    tw.WriteLine("Test {0} passed, totalweight {1}", itest, tmda.TotalWeight.Value);
                }
            }
        }

        /// <summary>
        ///  The program entry
        /// </summary>
        static void Main()
        {
#if true
            Test(Console.Out);
#else
            using (var sw = new StreamWriter(@"c:\temp\dbg.txt"))
            {
                Test(sw);
            }
#endif
        }
    }
}
