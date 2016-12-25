using System;

namespace Ch3_3
{
    class Program
    {
        static void Test()
        {
            var rand = new Random(123);

            for (var itest = 0; itest < 10000; itest++)
            {
                var numpts = rand.Next(3, 6);
                var n = MISNode.CreateNetwork(100, numpts, 0.5, rand);

                MISNode.Run(n, null);

                if (MISNode.Verify(n, Console.Out))
                {
                    Console.WriteLine(": Verification successful.");
                }
                else
                {
                    MISNode.Run(n, Console.Out);    // run again
                    break;
                }

                //Console.WriteLine(": Press any key to continue ...");
                //Console.ReadKey(true);
            }
        }

        static void Main(string[] args)
        {
            Test();
        }
    }
}
