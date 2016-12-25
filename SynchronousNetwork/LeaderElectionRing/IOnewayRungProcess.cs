using SynchronousNetwork;

namespace LeaderElectionRing
{
    public interface IOnewayRungProcess : IReporter
    {
        Link InboundNbr { get; }
        Link OutboundNbr { get; }
    }
}
