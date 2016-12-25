using System;
using System.Collections.Generic;
using System.Text;
using Graph;
using SynchronousNetwork;

namespace Ch3_2
{
    /// <summary>
    ///  A class that contains all the state information of an MST node
    /// </summary>
    /// <remarks>
    ///  It's all the conceptual elemenary data for the algorithm, there may be other data 
    ///  inferred from this held by the node to enhance runtime performance
    /// </remarks>
    public class MSTState : State
    {
        #region Enumerations

        /// <summary>
        ///  All the statuses an MST node can be in
        /// </summary>
        public enum Statuses
        {
            Init = 0,
            Passive,
            LeaderToAnnounce,
            LeaderAnnounced,
            ToLoopBack,
            LoopedBack,
            MinimumToReport,    // to report the minimum length output of self, initiated by the leader when it receives loop back message
            MinimumToPass,      // minimum of self has been reported, now need to passing an overwhelming minimum to the link specified by 'PassMinimumThru'
            MinimumSilent,      // minimum of self has been reported, nothing to pass at the moment
            MinimumSelected,    // knowing that the minimum length exchange process has finished and the minimum of the node is selected
            FlipDirection,      // knowing that the minimum length exchange process has finished and the minimum of the node is not selected
        }

        #endregion

        #region Properties

        /// <summary>
        ///  The owner of the state
        /// </summary>
        public MSTNode Owner { get; private set; }

        /// <summary>
        ///  The current status of the owner
        /// </summary>
        public Statuses Status { get; set; }

        /// <summary>
        ///  The ID of the owner
        /// </summary>
        public uint Own { get; private set; }

        /// <summary>
        ///  The ID of the group the owner is in
        /// </summary>
        public uint GroupUid { get; set; }

        /// <summary>
        ///  The tick that increases by one at the end of each synchronisation unit
        /// </summary>
        public int Tick { get; set; }

        /// <summary>
        ///  The tich at which the LeaderAnnouncement is made at the node, used for
        ///  measuring the depth from the node down to its deepest leaves
        /// </summary>
        public int TickLeaderAnnouncement { get; set; }

        /// <summary>
        ///  A flag indicating if the node is currently the leader of the tree
        /// </summary>
        public bool IsLeader
        {
            get { return Own == GroupUid; }
        }

        /// <summary>
        ///  Outbound link to the parent in the current tree
        /// </summary>
        public WeightedLink LinkToParent { get; set; }

        /// <summary>
        ///  A flag that indicates if at the current stage the connection requests 
        ///  are to be considered part of the current tree
        /// </summary>
        /// <remarks>
        ///   - from LeaderToAnnounce (or leaf nodes' ToLoopBack) on, the intake of connections pauses
        ///     connection requests are buffered, only consolidated connections are considered part of the tree
        ///   - from the end of Minimum outbound election on, the intake resumes
        /// </remarks>
        public bool TakingConnections { get; set; }

        /// <summary>
        ///  This keeps all the incoming connection requests that remain unconsolidated yet in this round
        /// </summary>
        public HashSet<MSTNode> ConnectionRequests { get; private set; }

        /// <summary>
        ///  This keeps all the consolidated connections (connections considered part of the tree)
        /// </summary>
        /// <remarks>
        ///  Since subnodes considered part of the tree are always added to the ConnectionsConsolidated collection
        ///  The algorithm always requests this collection for subnodes
        /// </remarks>
        public HashSet<MSTNode> Children { get; private set; }

        /// <summary>
        ///  Returns all current connections including the parent
        /// </summary>
        public IEnumerable<MSTNode> AllConnections
        {
            get
            {
                if (LinkToParent != null)
                {
                    yield return (MSTNode)LinkToParent.DownstreamProcess;                    
                }
                foreach (var connection in Children)
                {
                    yield return connection;
                }
            }
        }

        /// <summary>
        ///  Moves connection requests to the list of consolidated collection
        ///  Normally it's peformed when the node is re-opened to connection requests
        /// </summary>
        public void ConsolidateConnectionRequests()
        {
            foreach (var conn in ConnectionRequests)
            {
                Children.Add(conn);
            }
            ConnectionRequests.Clear();
        }

        /// <summary>
        ///  Adds connection request
        /// </summary>
        /// <param name="node"></param>
        public void AddConnectionRequestFrom(MSTNode node)
        {
            var conn = TakingConnections ? Children : ConnectionRequests;
            conn.Add(node);
        }

        /// <summary>
        ///  Removes a node from the connections of the current node
        /// </summary>
        /// <param name="node">The node to remove</param>
        public void DeleteConnection(MSTNode node)
        {
            if (!Children.Remove(node))
            {
                // normally it should be removed from 'ConnectionConsolidated', however
                // we here added removal from 'ConnectionRequests' as a second attempt
                ConnectionRequests.Remove(node);
            }
        }

        /// <summary>
        ///  Neighbours including all items in the Children collection and the parent
        /// </summary>
        public IEnumerable<MSTNode> GroupNeighbours
        {
            get
            {
                foreach (var connection in Children)
                {
                    yield return connection;
                }

                if (LinkToParent != null)
                {
                    yield return (MSTNode)LinkToParent.DownstreamProcess;
                }
            }
        }

        /// <summary>
        ///  The maximum depth of the sub-trees from the current node via all nodes but the given node
        /// </summary>
        /// <remarks>
        ///  equivalently it's also the depth of the sub-tree of the current confirmed connected tree 
        ///  which has the current node as its root and excludes the subtree starting from the specified child
        /// </remarks>
        public Dictionary<MSTNode, int> ReverseTreeDepth { get; private set; }

        /// <summary>
        ///  Flag that indicates the next minimum outbound message is the first time sent this round
        /// </summary>
        public bool FirstMinimumMessage { get; set; }

        /// <summary>
        ///  The maximum distance to leaf opposite the leader
        /// </summary>
        /// <remarks>
        ///  For leader it's its radius; for non-leader it's its depth to leaves down the tree
        /// </remarks>
        public int MaxDepthToLeaf { get; set; }

        /// <summary>
        ///  The distance from the current node to the current leader
        /// </summary>
        public int DistanceToLeader { get; set; }

        /// <summary>
        ///  The maximum depth of side trees
        /// </summary>
        public int MaxSideTreeDepth { get; set; }

        /// <summary>
        ///  The time for the node to pass to know if a new leader has been elected
        /// </summary>
        public int WaitingPeriod
        {
            get { return Math.Max(MaxSideTreeDepth, MaxDepthToLeaf) * 2; }
        }

        /// <summary>
        ///  Number of replies received since the announcement of leader from the current
        /// </summary>
        public int RepliesReceived { get; set; }

        /// <summary>
        ///  Link with minimum weight from this node at the current round
        /// </summary>
        public WeightedLink MyMinimum { get; set; }

        /// <summary>
        ///  Link with minimum weight so far passed to this node
        /// </summary>
        public WeightedLink CurrentMinimum { get; set; }

        /// <summary>
        ///  The node from which the current minimum bound message came
        /// </summary>
        public MSTNode CurrentMinimumFrom { get; set; }

        /// <summary>
        ///  The tick at which the recent minimum reporting/passing happened
        /// </summary>
        public int TickToReportMinimum { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a state info object
        /// </summary>
        /// <param name="owner">The owner node of the state</param>
        /// <param name="uid">The id of the owner node as part of the state</param>
        public MSTState(MSTNode owner, uint uid)
        {
            Owner = owner;
            Own = uid;

            ConnectionRequests = new HashSet<MSTNode>();
            Children = new HashSet<MSTNode>();

            ReverseTreeDepth = new Dictionary<MSTNode, int>();
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Converts a state to a string that represents it
        /// </summary>
        /// <returns>The string that represents the state</returns>
        public override string ToString()
        {
            var map = new [] {"I", "P", "LT", "LD", "OT", "OD", "MR", "MP", "ML", "MS", "F"};
            var sb = new StringBuilder(map[(int) Status]);
            sb.Append("(");

            sb.AppendFormat("{0}", GroupUid);
            if (Status == Statuses.LeaderAnnounced)
            {
                sb.Append(",");
                sb.AppendFormat("ChCnt={0}", Children.Count);
            }
            if (Status == Statuses.MinimumToReport || Status == Statuses.MinimumToPass)
            {
                sb.Append(",");

                if (CurrentMinimum != null)
                {
                    sb.AppendFormat("CurMin={0}:{1}", CurrentMinimum.Weight, ((MSTState)CurrentMinimum.DownstreamProcess.State).Own);
                }
                else
                {
                    sb.AppendFormat("CurMin=<null>");
                }
            }
            if (Status == Statuses.MinimumSilent || Status == Statuses.MinimumToPass)
            {
                sb.Append(",");
                sb.AppendFormat("W={0}", WaitingPeriod);
            }
            if (Status == Statuses.MinimumToReport || Status == Statuses.ToLoopBack)
            {
                sb.Append(",");
                sb.Append("{RevDpt=");
                var firstArg = true;
                foreach (var child in Children)
                {
                    if (!firstArg) sb.Append(",");
                    var revdepth = ReverseTreeDepth[child];
                    sb.AppendFormat("{0}:{1}", child.State.Own, revdepth);
                    firstArg = false;
                }
                sb.Append("}");
            }

            if (LinkToParent != null)
            {
                var parent = ((MSTNode)LinkToParent.DownstreamProcess).State.Own;
                sb.Append(",");
                sb.AppendFormat("->{0}", parent);
            }
            sb.AppendFormat(")");
            return sb.ToString();
        }

        #endregion
    }
}
