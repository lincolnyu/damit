namespace Ch1_2_2
{
    class Program
    {
        static void Main(string[] args)
        {
            var n = LCProcess.CreateNetwork(100, 2);
            LCProcess.Run(n);
        }
    }
}
