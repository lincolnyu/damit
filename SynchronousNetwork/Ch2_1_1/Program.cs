namespace Ch2_1_1
{
    class Program
    {
        static void Main(string[] args)
        {
            var n = HSProcess.CreateNetwork(100, 2);
            HSProcess.Run(n);
        }
    }
}
