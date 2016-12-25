using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Graph;
using SynchronousNetwork;
using Process = SynchronousNetwork.Process;

namespace Ch3_2
{
    /// <summary>
    ///  The class that represents a node in a graph for which Minimum Spanning Tree is to be found
    /// </summary>
    /// <remarks>
    ///  Some caveats,
    ///   The graph is undirectional, meaning it's bidirectionally linked
    ///   All links in 'OutNbrs' have to be purely links in the graph
    /// </remarks>
    public class MSTNode : Process<MSTState>
    {
        #region Nested types

        /// <summary>
        ///  Network whose Minimum Spanning Tree is to be found
        /// </summary>
        public class MSTNetwork : Network
        {
            #region Constructors

            /// <summary>
            ///  Instantiates a MST network with specified uid
            /// </summary>
            /// <param name="uid">The unique id assigned to the network</param>
            public MSTNetwork(uint uid) : base(uid)
            {
            }

            #endregion
        }

        /// <summary>
        ///  The information contained with MinimumToReport message
        /// </summary>
        private class MinOutboundInfo
        {
            #region Properties

            /// <summary>
            ///  The minimum outward link so far found with passing around the message
            /// </summary>
            public WeightedLink MinOut { get; private set; }

            /// <summary>
            ///  The maximum depth of the side trees up to the leader (including the side tree from the leader)
            /// </summary>
            public int MaxSideTreeDepth { get; private set; }

            #endregion

            #region Constructors

            /// <summary>
            ///  Instantiates a MinOutBoundInfo with the minimum outboiund link data and information about the tree
            /// </summary>
            /// <param name="minOut">The minimum outward link so far found with passing around the message</param>
            /// <param name="maxSideTreeDepth">The maximum depth of the side trees up to the leader (including the side tree from the leader)</param>
            public MinOutboundInfo(WeightedLink minOut, int maxSideTreeDepth = -1)
            {
                MinOut = minOut;
                MaxSideTreeDepth = maxSideTreeDepth;
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///  A map from a node to the outbound link with this node that links to the node
        /// </summary>
        private readonly Dictionary<Process, Link> _nodeToOutboundLink = new Dictionary<Process, Link>();

        /// <summary>
        ///  A map from a node to the inbound link with this node that links from the node
        /// </summary>
        private readonly Dictionary<Process, Link> _nodeToInboundLink = new Dictionary<Process, Link>();

        #endregion

        #region Properties

        /// <summary>
        ///  Returns the parent of the current node if any for quick reference
        /// </summary>
        public MSTNode Parent
        {
            get { return State.LinkToParent != null ? (MSTNode)State.LinkToParent.DownstreamProcess : null; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  Constructs a MST node with specified id
        /// </summary>
        /// <param name="uid"></param>
        public MSTNode(uint uid)
        {
            State = new MSTState(this, uid);
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Creates a MST-network 
        /// </summary>
        /// <param name="networkUid">The unique ID for the network to create</param>
        /// <param name="count">The number of nodes in the network to create</param>
        /// <param name="minWeight">The mininum possible weight of the links in the network to create</param>
        /// <param name="maxWeight">The maximum possible weight of the links in the network to create</param>
        /// <param name="connectionRate">The approximate ratio of connections to the total possible links</param>
        /// <param name="rand">The random number generator to use to create the network</param>
        /// <returns>The network that has been created</returns>
        public static MSTNetwork CreateNetwork(uint networkUid, int count, int minWeight = 1, int maxWeight = int.MaxValue,
            double connectionRate = 0.5, Random rand = null)
        {
            if (rand == null)
            {
                rand = new Random(123);
            }

            var network = new MSTNetwork(networkUid);

            for (var i = 0; i < count; i++)
            {
                var uid = (uint)i + 1;
                var process = new MSTNode(uid);
                network.Processes.Add(process);
            }

            for (var i = 0; i < count; i++)
            {
                for (var j = i + 1; j < count; j++)
                {
                    var rn = rand.Next(100);
                    if (rn > connectionRate * 100) continue;  // not connected
                    var weight = rand.Next(minWeight, maxWeight + 1);
                    var ni = network.Processes[i];
                    var nj = network.Processes[j];
                    var linkIj = WeightedLink.Connect(ni, nj, weight);
                    var linkJi = WeightedLink.Connect(nj, ni, weight);
                    network.Links.Add(linkIj);
                    network.Links.Add(linkJi);
                }
            }

            return network;
        }

        /// <summary>
        ///  Runs the network till the end
        /// </summary>
        /// <param name="network">The network to run</param>
        /// <param name="tw">The text writer to write the debug information to</param>
        public static void Run(MSTNetwork network, TextWriter tw)
        {
            DisplayNetwork(network, tw);

            network.Initialize();

            var allPassive = AllPassive(network);
            var lastAllPassive = false;
            var cont = DebugStep(network, tw);

            while (!(lastAllPassive && allPassive) && cont) // just to avoid the stop on initial state transfer to all passive
            {
                lastAllPassive = allPassive;
                network.RunOneRound();
                cont = DebugStep(network, tw);
                allPassive = AllPassive(network);
            } 

            DisplayReports(network, tw);
        }

        /// <summary>
        ///  Displays the network
        /// </summary>
        /// <param name="network">The network to display</param>
        /// <param name="tw">The text writer to write to</param>
        public static void DisplayNetwork(MSTNetwork network, TextWriter tw)
        {
            WriteLine(tw, "Network ...");

            foreach (var proc in network.Processes)
            {
                var bfsProc = proc as MSTNode;
                if (bfsProc == null) continue;
                WriteLine(tw, string.Format(" Process {0}", bfsProc.State.Own));
            }

            for (var i = 0; i < network.Links.Count; i += 2)
            {
                var link = network.Links[i];
                var weightedLink = link as WeightedLink;
                if (weightedLink == null)
                {
                    continue;
                }
                var usProc = link.UpstreamProcess as MSTNode;
                var dsProc = link.DownstreamProcess as MSTNode;
                if (usProc == null || dsProc == null)
                {
                    continue;
                }
                WriteLine(tw, string.Format(" Link: {0} <-- {1} --> {2}", usProc.State.Own, weightedLink.Weight,
                    dsProc.State.Own));
            }
        }

        /// <summary>
        ///  Checks if all the nodes in the network is in passive mode which indicates the process has finished
        /// </summary>
        /// <param name="network">The network to check</param>
        /// <returns>true if all nodes are passive or false</returns>
        public static bool AllPassive(MSTNetwork network)
        {
            // if all the nodes are in passive mode
// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var node in network.Processes)
// ReSharper restore LoopCanBeConvertedToQuery
            {
                var mstNode = node as MSTNode;
                if (mstNode == null) continue;
                if (mstNode.State.Status != MSTState.Statuses.Passive)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///  Debug (run) one step on the network
        /// </summary>
        /// <param name="network">The network to debug</param>
        /// <param name="tw">Text writer to write the debug information to</param>
        /// <returns>true if to debug further</returns>
        public static bool DebugStep(MSTNetwork network, TextWriter tw)
        {
            foreach (var node in network.Processes)
            {
                var mstNode = node as MSTNode;
                if (mstNode == null) continue;
                Write(tw, string.Format("{0}:{1} ", mstNode.State.Own, mstNode.State));
            }
            WriteLine(tw, "");
            var key = Console.ReadKey(true);
            return key.KeyChar != 'q';
        }

        /// <summary>
        ///  Displays the MST solution to the network
        /// </summary>
        /// <param name="network">The network to display</param>
        /// <param name="tw">Text writer to write the debug information to</param>
        public static void DisplayReports(MSTNetwork network, TextWriter tw)
        {
            WriteLine(tw, "MST Solution ...");

            foreach (var node in network.Processes)
            {
                var mstNode = node as MSTNode;
                System.Diagnostics.Trace.Assert(mstNode != null);
                var linkToParent = mstNode.State.LinkToParent;
                if (linkToParent != null)
                {
                    var parent = linkToParent.DownstreamProcess;
                    var mstParent = parent as MSTNode;
                    System.Diagnostics.Trace.Assert(mstParent != null);
                    WriteLine(tw, string.Format(" Node {0} links to Node {1}", mstNode.State.Own, mstParent.State.Own));
                }
                else
                {
                    WriteLine(tw, string.Format(" Node {0} links is the root of the tree", mstNode.State.Own));
                }
            }

            var sum = Sum(network);
            WriteLine(tw, string.Format(" The total weight of the tree is {0}", sum));
        }

        /// <summary>
        ///  Returns the total weight of links in the network
        /// </summary>
        /// <param name="network">The network of which the total weight of links is to return</param>
        /// <returns>The total weight of links</returns>
        public static int Sum(MSTNetwork network)
        {
            var sum = 0;
            foreach (var node in network.Processes)
            {
                var mstNode = node as MSTNode;
                System.Diagnostics.Trace.Assert(mstNode != null);
                var linkToParent = mstNode.State.LinkToParent;
                if (linkToParent != null)
                {
                    sum += linkToParent.Weight;
                }
            }
            return sum;
        }

        /// <summary>
        ///  Writes a line to the specified text writer and to console as well if not equal
        /// </summary>
        /// <param name="tw">The text writer to write to</param>
        /// <param name="line">The line to write</param>
        private static void WriteLine(TextWriter tw, string line)
        {
            tw.WriteLine(line);
            if (tw != Console.Out)
            {
                Console.WriteLine(line);
            }
        }

        /// <summary>
        ///  Writes a string to the specified text writer and to console as well if not equal
        /// </summary>
        /// <param name="tw">The text writer to write to</param>
        /// <param name="str">The string  to write</param>
        private static void Write(TextWriter tw, string str)
        {
            tw.Write(str);
            if (tw != Console.Out)
            {
                Console.Write(str);
            }
        }

        /// <Summary>
        ///  Compares two links in terms of their order of being selected
        /// </Summary>
        /// <param name="link1">The first link to compare</param>
        /// <param name="link2">The second link to compare</param>
        /// <returns>The comparison result indicator</returns>
        private static int CompareLinks(Link link1, Link link2)
        {
            if (link1 == null && link2 == null)
            {
                return 0;
            }
            if (link1 == null)
            {
                return 1;
            }
            if (link2 == null)
            {
                return -1;
            }
            var wlink1 = (WeightedLink)link1;
            var wlink2 = (WeightedLink)link2;
            var comp = wlink1.Weight.CompareTo(wlink2.Weight);
            if (comp != 0)
            {
                return comp;
            }
            var us1 = (MSTNode)wlink1.UpstreamProcess;
            var ds1 = (MSTNode)wlink1.DownstreamProcess;
            var us2 = (MSTNode)wlink2.UpstreamProcess;
            var ds2 = (MSTNode)wlink2.DownstreamProcess;

            var min1 = Math.Min(us1.State.Own, ds1.State.Own);
            var min2 = Math.Min(us2.State.Own, ds2.State.Own);
            comp = min1.CompareTo(min2);
            if (comp != 0)
            {
                return comp;
            }

            var max1 = Math.Max(us1.State.Own, ds1.State.Own);
            var max2 = Math.Max(us2.State.Own, ds2.State.Own);
            comp = max1.CompareTo(max2);
            return comp;
        }

        #region Process<MSTNode> members

        /// <summary>
        ///  Initializes the process to its start state
        /// </summary>
        public override void Init()
        {
            State.Status = MSTState.Statuses.Init;
            State.GroupUid = State.Own;

            State.LinkToParent = null;
            State.ConnectionRequests.Clear();
            State.Children.Clear();
            State.ReverseTreeDepth.Clear();
            State.TakingConnections = true;

            State.Tick = 0;

            // initialisation of facilities that assist the MST searching process
            StructuralInit();
        }

        /// <summary>
        ///  Msgs method of the node that Produces the output messages according to the state
        /// </summary>
        public override void Msgs()
        {
            switch (State.Status)
            {
                case MSTState.Statuses.Init:
                    MsgsInit();
                    break;
                case MSTState.Statuses.Passive:
                    MsgsPassive();
                    break;
                case MSTState.Statuses.LeaderToAnnounce:
                    MsgsLeaderToAnnounce();
                    break;
                case MSTState.Statuses.LeaderAnnounced:
                    MsgsLeaderAnnounced();
                    break;
                case MSTState.Statuses.ToLoopBack:
                    MsgsToLoopBack();
                    break;
                case MSTState.Statuses.LoopedBack:
                    MsgsLoopedBack();
                    break;
                case MSTState.Statuses.MinimumToReport:
                    MsgsMinimumToReport();
                    break;
                case MSTState.Statuses.MinimumToPass:
                    MsgsMinimumToPass();
                    break;
                case MSTState.Statuses.MinimumSilent:
                    MsgsMinimumSilent();
                    break;
                case MSTState.Statuses.MinimumSelected:
                    MsgsMinimumSelected();
                    break;
                case MSTState.Statuses.FlipDirection:
                    MsgsFlipDirection();
                    break;
            }
        }

        /// <summary>
        ///  Trans method of the node that transfers the state of the process according to the input messages
        /// </summary>
        public override void Trans()
        {
            CollectConnectionRequests();

            switch (State.Status)
            {
                case MSTState.Statuses.Init:
                    TransInit();
                    break;
                case MSTState.Statuses.Passive:
                    TransPassive();
                    break;
                case MSTState.Statuses.LeaderToAnnounce:
                    TransLeaderToAnnounce();
                    break;
                case MSTState.Statuses.LeaderAnnounced:
                    TransLeaderAnnounced();
                    break;
                case MSTState.Statuses.ToLoopBack:
                    TransToLoopBack();
                    break;
                case MSTState.Statuses.LoopedBack:
                    TransLoopedBack();
                    break;
                case MSTState.Statuses.MinimumToReport:
                    TransMinimumToReport();
                    break;
                case MSTState.Statuses.MinimumToPass:
                    TransMinimumToPass();
                    break;
                case MSTState.Statuses.MinimumSilent:
                    TransMininumSilent();
                    break;
                case MSTState.Statuses.MinimumSelected:
                    TransMinimumSelected();
                    break;
                case MSTState.Statuses.FlipDirection:
                    TransFlipDirection();
                    break;
            }

            State.Tick++;
        }

        #endregion

        /// <summary>
        ///  Initialises the facilities that optimise the process by accelerating the data access but should not be
        ///  regarded as part of the algorithmic structure
        /// </summary>
        private void StructuralInit()
        {
            foreach (var outbound in OutNbrs)
            {
                _nodeToOutboundLink[outbound.DownstreamProcess] = outbound;
            }
            foreach (var inbound in InNbrs)
            {
                _nodeToInboundLink[inbound.UpstreamProcess] = inbound;
            }
        }

        /// <summary>
        ///  Each tick the connection requests should either be taken into the tree being formed or
        ///  queued for the next round
        /// </summary>
        private void CollectConnectionRequests()
        {
            foreach (var inbound in InNbrs)
            {
                var msg = inbound.Message as MSTMessage;
                if (msg == null || msg.Type != MSTMessage.Types.ConnectionRequest)
                {
                    continue;
                }
                inbound.ReceiveMessage(); // remove the message from the buffer

                State.AddConnectionRequestFrom((MSTNode)inbound.UpstreamProcess);
            }
        }

        /// <summary>
        ///  Msgs method for the Init state
        /// </summary>
        private void MsgsInit()
        {
            State.LinkToParent = GetMinOut();

            if (State.LinkToParent == null)
            {
                return;
            }

            var msg = new MSTMessage(MSTMessage.Types.ConnectionRequest);
            State.LinkToParent.SendMessage(msg);
        }

        /// <summary>
        ///  Trans method for the Init state
        /// </summary>
        private void TransInit()
        {
            State.Status = MSTState.Statuses.Passive;   
            // If State.LinkToParent is null it's an isolated node which may not be allowed
        }

        /// <summary>
        ///  Msgs method for the Passive state
        /// </summary>
        private void MsgsPassive()
        {
            // no message to send
        }

        /// <summary>
        ///  Trans method for the Passive state
        /// </summary>
        private void TransPassive()
        {
            // check if it's post-minimum-finding stage and flip is needed
            foreach (var child in State.Children)
            {
                var msg = PopMessageFrom(child);
                if (msg == null || msg.Type != MSTMessage.Types.MinimumAnnouncement) continue;
                
                // use 'CurrentMinimumFrom' for keeping the the new parent
                State.CurrentMinimumFrom = child;

                State.Status = MSTState.Statuses.FlipDirection;

                // The node won't be leader the coming round 
                // and there must not be leader announcement this tick
                return;
            }

            TransLeaderDeal();    
        }

        private void TransLeaderDeal()
        {
            var parent = Parent;
            if (parent == null)
            {
                // NOTE this node must be a leader with its tree created
                return;
            }

            // NOTE that this is legal as conceptually it can be achieved
            // by comparing the links rather than the neighbouring nodes
            if (State.Children.Contains(parent))
            {
                // mutually connected
                // NOTE again this is legal
                var thatId = parent.State.Own;
                State.GroupUid = State.Own < thatId ? State.Own : thatId;

                if (State.IsLeader) // It is elected as the leader of the two mutually connected
                {
                    // obviously the leader has at least one node to announce to
                    State.Status = MSTState.Statuses.LeaderToAnnounce;
                    // once the leader is announced connection taking is closed
                    // and all existing connections (requests) are consolidated
                    State.TakingConnections = false;

                    State.RepliesReceived = 0;
                    State.MaxSideTreeDepth = 0;
                    State.TickLeaderAnnouncement = State.Tick;
                    State.LinkToParent = null;  // leader's parent should be null until flipping
                }
                else   // It's the one other than the leader of the two mutually connnected
                {
                    // The node should remain Passive and wait until the next round
                    // it should remove the leader from its list of children
                    // it's both conceptually required and physically as it won't send leader 
                    // initiated messages back to the leader
                    // 
                    // NOTE parent is in the Children list as it's been consolidated or added when 
                    // connection intake was open
                    State.Children.Remove(parent);
                }
            }
            else  // it's not a mutually connected node
            {
                // first message to be expected is the LeaderAnnouncement
                // which should be from the parent and sent only once
                var mstmsg = PopMessageFrom(parent);
                if (mstmsg != null)
                {
                    if (mstmsg.Type == MSTMessage.Types.LeaderAnnouncment)
                    {   // pass on the LeaderAnnouncement message
                        State.Status = MSTState.Statuses.LeaderToAnnounce;
                        // once the leader is announced connection taking is closed
                        // and all existing connections (requests) are consolidated
                        State.TakingConnections = false;

                        State.RepliesReceived = 0;
                        State.TickLeaderAnnouncement = State.Tick;
                        State.GroupUid = (uint)mstmsg.Value;

                        if (State.Children.Count == 0)
                        {
                            // it's an end point (leaf)
                            State.Status = MSTState.Statuses.ToLoopBack;
                            State.TakingConnections = false;
                            // Note even if it's the leader's partner, it's ok to transfer to ToLoopBack at this round
                            // since the leader can handle a non-delayed reply from its partner
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Msgs method for the LeaderToAnnounce state
        /// </summary>
        private void MsgsLeaderToAnnounce()
        {
            foreach (var connector in State.Children)
            {
                var msg = new MSTMessage(MSTMessage.Types.LeaderAnnouncment, State.GroupUid);
                var outlink = _nodeToOutboundLink[connector];
                outlink.SendMessage(msg);
            }
        }

        /// <summary>
        ///  Trans method for the LeaderToAnnounce state
        /// </summary>
        private void TransLeaderToAnnounce()
        {
            // clear the ReverseTreeDepth collection for the use shortly after
            State.ReverseTreeDepth.Clear();

            State.Status = MSTState.Statuses.LeaderAnnounced;
        }

        /// <summary>
        ///  Msgs method for the LeaderAnnounced state
        /// </summary>
        private void MsgsLeaderAnnounced()
        {
            // no message to send
        }

        /// <summary>
        ///  Trans method for the LeaderAnnounced state
        /// </summary>
        /// <remarks>
        ///  reverse-tree-depth from a node against one of its child is defined as the depth of 
        ///  the sub-tree of the current confirmed connected tree which has the current node as 
        ///  its root and exclude the subtree starting from the child
        ///  NOTE for leader the follwing is to work out its child nodes' reverse-tree-depth
        /// 
        ///  This state is applicable to both leader and non-leader
        /// </remarks>
        private void TransLeaderAnnounced()
        {
            var currDepth = GetTreeDepth();
            var currReceived = 0;   // number of reply received this tick

            foreach (var connector in State.Children)
            {
                var mstmsg = PopMessageFrom(connector);
                if (mstmsg == null || mstmsg.Type != MSTMessage.Types.ReplyToLeader) continue;

                // This is temporarily used for keeping the depth of the tree
                // from the leader via the connector
                State.ReverseTreeDepth[connector] = currDepth;
                State.RepliesReceived++;
                currReceived++;
            }

            if (State.RepliesReceived < State.Children.Count) 
            {
                return; // not all replies have been collected
            }
            // all replies have been collected

            // this is to work out the ReverseTreeDepth
            // both the leader and nonleaders need reverse tree depth list
            var keys = State.ReverseTreeDepth.Keys.ToArray();
            if (currReceived > 1)
            {
                // as there are more than one links extend to the max depth (deduced in this tick)
                // the depth of a subtree from the current node via any set of its neighbouring nodes 
                // minus a specified neighbouring node has to be greater than the max depth
                foreach (var key in keys)
                {
                    State.ReverseTreeDepth[key] = currDepth;
                }
            }
            else
            {
                var secondMax = 0;
                MSTNode maxDepthNode = null;
                foreach (var key in keys)
                {
                    var depth = State.ReverseTreeDepth[key];
                    if (depth < currDepth)
                    {
                        State.ReverseTreeDepth[key] = currDepth;
                        if (secondMax < depth)
                        {
                            secondMax = depth;
                        }
                    }
                    else
                    {
                        maxDepthNode = key;
                    }
                }
                System.Diagnostics.Trace.Assert(maxDepthNode != null);
                State.ReverseTreeDepth[maxDepthNode] = secondMax;
            }

            if (State.IsLeader)
            {
                State.CurrentMinimum = State.MyMinimum = GetMinOut();
                State.Status = MSTState.Statuses.MinimumToReport;
                State.TickToReportMinimum = State.Tick;
                State.FirstMinimumMessage = true;
            }
            else
            {
                // the radius will be this maximum of the depth to leaf and the depth via parent
                State.Status = MSTState.Statuses.ToLoopBack;
            }

            State.MaxDepthToLeaf = currDepth;
        }

        /// <summary>
        ///  Msgs method for the ToLoopBack state
        /// </summary>
        private void MsgsToLoopBack()
        {
            var msg = new MSTMessage(MSTMessage.Types.ReplyToLeader);
            var linkToParent = State.LinkToParent;
            linkToParent.SendMessage(msg);
        }

        /// <summary>
        ///  Trans method for the ToLoopBack state
        /// </summary>
        /// <remarks>
        ///  Note only non-leader goes through this state
        /// </remarks>
        private void TransToLoopBack()
        {
            State.Status = MSTState.Statuses.LoopedBack;
        }

        /// <summary>
        ///  Msgs method for the LoopedBack state
        /// </summary>
        private void MsgsLoopedBack()
        {
            // no message to send
        }

        /// <summary>
        ///  Trans method for the Looped back state
        /// </summary>
        /// <remarks>
        ///  This state is always and only applicable to non-leaders
        /// </remarks>
        private void TransLoopedBack()
        {
            var mstmsg = PopMessageFrom(Parent);
            
            // if the incoming message is not minimum outbound it stays in LoopedBack state
            if (mstmsg == null || mstmsg.Type != MSTMessage.Types.MinimumOutbound)
            {
                return;
            }

            // incoming message is a minimum outbound first time sent
            // the payload of the incoming message is a full-fledged one containing
            // - minimum outbound
            // - the maximum side depth

// ReSharper disable PossibleInvalidCastException
            var payload = (MinOutboundInfo) mstmsg.Value;
// ReSharper restore PossibleInvalidCastException
            var min = payload.MinOut;
            State.MaxSideTreeDepth = payload.MaxSideTreeDepth;

            // switching from LoopedBack to first minimum update state
            // instructs the Msgs to send updates downwards
            State.FirstMinimumMessage = true;

            var myMinOut = GetMinOut();
            State.MyMinimum = myMinOut;

            var cmp = CompareLinks(min, myMinOut);
            if (cmp <= 0)
            {
                // the node doesn't have the minimum compared with the incoming
                State.CurrentMinimum = min;
                State.CurrentMinimumFrom = Parent;
                State.Status = MSTState.Statuses.MinimumToPass;
            }
            else
            {
                System.Diagnostics.Trace.Assert(myMinOut != null);

                // the node has the minimum compared with the incoming
                State.CurrentMinimum = myMinOut;
                State.Status = MSTState.Statuses.MinimumToReport;
            }

            // first time the node takes up the minimum outbound message passing for this round
            State.TickToReportMinimum = State.Tick;
        }

        /// <summary>
        ///  Msgs method for the MinimumToReport status
        ///  which passes the minimum to all neighbours in the tree
        /// </summary>
        private void MsgsMinimumToReport()
        {
            System.Diagnostics.Trace.Assert(State.CurrentMinimum == State.MyMinimum);
            // the minimum of this node is going to overrule all around for this tick

            MsgsMinimum(false);
        }

        /// <summary>
        ///  Trans method for the MinimumToReport status
        /// </summary>
        private void TransMinimumToReport()
        {
            TransMinimum();
        }

        /// <summary>
        ///  Msgs method for the MinimumToPass status
        ///  This passes minimum to neighbours other than the one that sent the minimum
        /// </summary>
        void MsgsMinimumToPass()
        {
            MsgsMinimum(true);
        }

        /// <Summary>
        ///  Trans method for MinimumToPass state
        /// </Summary>
        void TransMinimumToPass()
        {
            TransMinimum();
        }

        /// <Summary>
        ///  Msgs method for MinimumSilent state
        /// </Summary>
        private void MsgsMinimumSilent()
        {
            // no message to send
        }

        /// <summary>
        ///  Trans method for MinimumSilent state
        /// </summary>
        private void TransMininumSilent()
        {
            TransMinimum();
        }

        /// <summary>
        ///  Msgs method shared by MinimumToPass and MinimumToReport
        /// </summary>
        /// <param name="checkMinFrom">
        ///  true to check if the node is where the current minimum from. This is more for minimizing the messages to send.
        /// </param>
        void MsgsMinimum(bool checkMinFrom)
        {
            var min = State.CurrentMinimum;

            foreach (var conn in State.AllConnections)
            {
                if (checkMinFrom && conn == State.CurrentMinimumFrom)
                {
                    continue;   // skip the node from which a passing message came
                }

                var weightedLink = (WeightedLink)_nodeToOutboundLink[conn];
                MinOutboundInfo payload;
                if (State.FirstMinimumMessage && conn != Parent)
                {
                    // first time to send out a post-loop-back message, 
                    // it should contain a parent tree span update for the children
                    var sideTreeDepth = Math.Max(State.MaxSideTreeDepth, State.ReverseTreeDepth[conn]);
                    payload = new MinOutboundInfo(min, sideTreeDepth);
                }
                else
                {
                    payload = new MinOutboundInfo(min);
                }
                var msg = new MSTMessage(MSTMessage.Types.MinimumOutbound, payload);
                weightedLink.SendMessage(msg);
            }

            State.FirstMinimumMessage = false;
        }

        /// <summary>
        ///  Trans method for multiple statuses related to Mininum extension exchange
        /// </summary>
        private void TransMinimum()
        {
            var minChanged = false;
            foreach (var connection in State.AllConnections)
            {
                var msgFromNbr = PeekMessageFrom(connection);
                if (msgFromNbr == null) continue;
                switch (msgFromNbr.Type)
                {
                    case MSTMessage.Types.LeaderAnnouncment:
                        continue;
                    case MSTMessage.Types.MinimumOutbound:
                        {
                            var minOutInfo = (MinOutboundInfo)msgFromNbr.Value;
                            // this should not be the first minimum outbound message sent
                            var min = minOutInfo.MinOut;
                            if (State.CurrentMinimum == null || CompareLinks(min, State.CurrentMinimum) < 0)
                            {
                                State.CurrentMinimum = min;
                                State.CurrentMinimumFrom = connection;
                                minChanged = true;
                            }
                        }
                        break;
                    case MSTMessage.Types.MinimumAnnouncement:
                        /* the announcement comes from the new leader before the node realises the election has finished */
                        // use 'CurrentMinimumFrom' for keeping the the new parent
                        State.CurrentMinimumFrom = connection;
                        State.Status = MSTState.Statuses.FlipDirection;

                        // at the mean time start to be open to connection requests
                        State.ConsolidateConnectionRequests();
                        State.TakingConnections = true;

                        // The node won't be leader the coming round 
                        // and there must not be leader announcement this tick
                        PopMessageFrom(connection); // pop out the message
                        return;
                }
                PopMessageFrom(connection); // pop out the message
            }

            State.Status = minChanged ? MSTState.Statuses.MinimumToPass : MSTState.Statuses.MinimumSilent;

            if (minChanged) return;
            
            // check if this node can be sure that the minimum has been elected
            if (State.Tick - State.TickToReportMinimum < State.WaitingPeriod) return;

            State.Status = State.CurrentMinimum == State.MyMinimum && State.MyMinimum != null
                               ? MSTState.Statuses.MinimumSelected
                               : MSTState.Statuses.Passive;
            // start to be open to connection requests
            State.ConsolidateConnectionRequests();
            State.TakingConnections = true;

            if (State.Status == MSTState.Statuses.Passive)
            {
                TransLeaderDeal();
            }
        }

        /// <summary>
        ///  Msgs method for the MinimumSelected state
        /// </summary>
        private void MsgsMinimumSelected()
        {
            // send connection request to the partner to be
            var connectionRequest = new MSTMessage(MSTMessage.Types.ConnectionRequest);
            State.CurrentMinimum.SendMessage(connectionRequest);

            if (State.LinkToParent == null) return;

            // sends orientation rectification notice to neighbours
            // who should be in passive state
            var minAnnouncment = new MSTMessage(MSTMessage.Types.MinimumAnnouncement);
            State.LinkToParent.SendMessage(minAnnouncment);
        }

        /// <summary>
        ///  Trans method for the MinimumSelected state
        /// </summary>
        private void TransMinimumSelected()
        {
            // rectify the connection
            if (State.LinkToParent != null)
            {
                State.Children.Add((MSTNode)State.LinkToParent.DownstreamProcess);
            }
            State.LinkToParent = State.CurrentMinimum;

            State.Status = MSTState.Statuses.Passive;

            // leader negotiation can be performed here
            TransLeaderDeal();
        }

        /// <summary>
        ///  Msgs method for the FlipDirection state
        /// </summary>
        private void MsgsFlipDirection()
        {
            // send orientation rectification notice to neighbours
            if (State.LinkToParent == null) return;

            var msgMinAnn = new MSTMessage(MSTMessage.Types.MinimumAnnouncement);
            State.LinkToParent.SendMessage(msgMinAnn);
        }

        /// <summary>
        ///  Trans method for the FlipDirection state
        /// </summary>
        private void TransFlipDirection()
        {
            if (State.LinkToParent != null)
            {
                var oldParent = State.LinkToParent.DownstreamProcess;
                State.Children.Add((MSTNode)oldParent);
            }

            var newParent = State.CurrentMinimumFrom;
            // it must be in ConnectionConsolidated
            State.Children.Remove(newParent);
            State.LinkToParent = (WeightedLink)_nodeToOutboundLink[newParent];

            // once the direction is flipped the node should get back to Passive mode
            State.Status = MSTState.Statuses.Passive;

            // start dealing with passive state message here
            TransLeaderDeal();
        }

        /// <summary>
        ///  Returns the minimum outbound link to node in a different group
        /// </summary>
        /// <returns>The minimum outbound link</returns>
        private WeightedLink GetMinOut()
        {
            WeightedLink min = null;
            foreach (var outbound in OutNbrs)
            {
                if (State.GroupUid == ((MSTState)outbound.DownstreamProcess.State).GroupUid)
                {
                    continue;
                }
                if (min == null || CompareLinks(outbound, min) < 0)
                {
                    min = (WeightedLink)outbound;
                }
            }
            return min;
        }
        
        /// <summary>
        ///  Returns the message from the specified neigbouring node
        /// </summary>
        /// <param name="node">The node from which the message is to be returned</param>
        /// <returns>The message received</returns>
        private MSTMessage PopMessageFrom(Process node)
        {
            var inboundLink = _nodeToInboundLink[node];
            var msg = inboundLink.ReceiveMessage();
            var mstmsg = (MSTMessage) msg;
            return mstmsg;
        }

        /// <summary>
        ///  Returns the message from the specified neighbouring node without removing the message
        /// </summary>
        /// <param name="node">The node from which the message is to be returned</param>
        /// <returns>The message received</returns>
        private MSTMessage PeekMessageFrom(Process node)
        {
            var inboundLink = _nodeToInboundLink[node];
            var msg = inboundLink.Message;
            var mstmsg = (MSTMessage)msg;
            return mstmsg;
        }

        /// <summary>
        ///  Returns the depth of the sub-tree from this node broken up from the its parent
        /// </summary>
        /// <returns>The depth</returns>
        private int GetTreeDepth()
        {
            var tickAnnounce = State.TickLeaderAnnouncement;
            var tickAllReplied = State.Tick;
            return (tickAllReplied - tickAnnounce) / 2;
        }

        #endregion
    }
}
