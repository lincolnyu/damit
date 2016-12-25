using SynchronousNetwork;

namespace Ch3_1
{
    class FordMessage : Message
    {
        public uint Source { get; private set; }
        public double BestDist { get; private set; }

        public FordMessage(uint source, double bestDist)
        {
            Source = source;
            BestDist = bestDist;
        }
    }
}
