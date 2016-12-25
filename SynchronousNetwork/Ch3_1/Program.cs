namespace Ch3_1
{
    class Program
    {
        static void Main(string[] args)
        {
            var n = FordNode.CreateNetwork(100, 8, 0.8);
            FordNode.Run(n);
        }
    }
}
