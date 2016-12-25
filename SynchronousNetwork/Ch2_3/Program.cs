namespace Ch2_3
{
    class Program
    {
        static void Main(string[] args)
        {
            var n = BFSNode.CreateNetwork(100, 8, 0.2);
            BFSNode.Run(n);
        }
    }
}
