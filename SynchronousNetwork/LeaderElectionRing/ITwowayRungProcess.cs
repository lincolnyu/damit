using SynchronousNetwork;

namespace LeaderElectionRing
{
    public interface ITwowayRungProcess : IReporter
    {
        Link InboundLeftNbr { get; }
        Link InboundRightNbr { get; }
        Link OutboundLeftNbr { get; }
        Link OutboundRightNbr { get; }
    }
}
