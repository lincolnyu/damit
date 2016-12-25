using Graph;
using QSharp.Scheme.Classical.Graphs.MinimumSpanningTree;
using QSharpTest.Scheme.Classical.Graphs.Mocks;

namespace Ch3_2
{
    public static class MSTTest
    {
        #region Nested types

        public class DaMSTVertex : IIndexedVertex
        {
            #region Properties

            public MSTNode Node { get; private set; }

            public int Index
            {
                get
                {
                    return (int)Node.State.Own-1;
                }
            }

            #endregion

            #region Constructors

            public DaMSTVertex(MSTNode node)
            {
                Node = node;
            }

            #endregion
        }

        #endregion

        #region Methods

        public static void MarkEdges(MSTNode.MSTNetwork network, ITreeMarker treeMarker)
        {
            foreach (var node in network.Processes)
            {
                var mstNode = node as MSTNode;
                System.Diagnostics.Trace.Assert(mstNode != null);
                var linkToParent = mstNode.State.LinkToParent;
                if (linkToParent != null)
                {
                    var v1 = new DaMSTVertex((MSTNode)linkToParent.DownstreamProcess);
                    var v2 = new DaMSTVertex((MSTNode)node);
                    treeMarker.Connect(v1, v2);
                }
            }
        }

        /// <summary>
        ///  Runs the da-MST
        /// </summary>
        /// <param name="network">The network to solve</param>
        /// <param name="treeMarker">The treemarker that records the solving process</param>
        public static void Run(MSTNode.MSTNetwork network, ITreeMarker treeMarker)
        {
            //MSTNode.DisplayNetwork(network);

            network.Initialize();

            var allPassive = MSTNode.AllPassive(network);
            bool lastAllPassive;

            //MSTNode.DebugStep(network);

            do
            {
                lastAllPassive = allPassive;
                network.RunOneRound();
                //MSTNode.DebugStep(network);
                allPassive = MSTNode.AllPassive(network);
            } while (!(lastAllPassive && allPassive));  // just to avoid the stop on the initial state transfer to all passive

            //MSTNode.DisplayReports(network);

            MarkEdges(network, treeMarker);
        }

        public static SampleWeightOrderedDigraph GetGraph(MSTNode.MSTNetwork network)
        {
            var n = network.Processes.Count;
            var wtab = new int[n, n];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    wtab[i, j] = int.MaxValue;
                }
            }
            foreach (WeightedLink link in network.Links)
            {
                var w = link.Weight;
                // Note vertex id starts from 1
                var v1 = ((MSTNode)link.UpstreamProcess).State.Own;
                var v2 = ((MSTNode)link.DownstreamProcess).State.Own;
                wtab[v1 - 1, v2 - 1] = wtab[v2 - 1, v1 - 1] = w;
            }
            var g = new SampleWeightOrderedDigraph(wtab);
            return g;
        }

        public static void MSTSequential(SampleWeightOrderedDigraph g, ITreeMarker treeMarker)
        {
            Prim.Solve(g, treeMarker);
        }

        #endregion
    }
}
