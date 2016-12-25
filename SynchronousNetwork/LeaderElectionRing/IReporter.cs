using SynchronousNetwork;

namespace LeaderElectionRing
{
    public interface IReporter
    {
        Link DummyOutput { get; }

        string ToShortString();
    }
}
