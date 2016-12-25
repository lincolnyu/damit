using SynchronousNetwork;

namespace Graph
{
    public class WeightedLink : Link
    {
        #region Properties

        public int Weight { get; private set; } 

        #endregion

        #region Constructors

        public WeightedLink(int weight)
        {
            Weight = weight;
        }

        #endregion

        #region Static methods

        public static WeightedLink Connect(Process upstreamProcess, Process downstreamProcess, int weight)
        {
            var link = new WeightedLink(weight);
            AddLink(link, upstreamProcess, downstreamProcess);
            return link;
        }

        public static WeightedLink Connect(Process upstreamProcess, int indexUpstream,
                                          Process downstreamProcess, int indexDownstream, int weight)
        {
            var link = new WeightedLink(weight);
            InsertLink(link, upstreamProcess, indexUpstream, downstreamProcess, indexDownstream);
            return link;
        }

        #endregion

    }
}
