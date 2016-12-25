namespace Ch3_1
{
    class DummyMessage : FordMessage
    {
        public uint Parent { get; private set; }

        public DummyMessage(uint source, double bestDist, uint parent) 
            : base(source, bestDist)
        {
            Parent = parent;
        }
    }
}
