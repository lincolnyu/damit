using System;
using System.IO;
using SynchronousNetwork;

namespace Ch3_3
{
    /// <summary>
    ///  A node of Maxmimal Independent Set
    /// </summary>
    public class MISNode : Process<MISState>
    {
        #region Nested types

        /// <summary>
        ///  Network whose Minimum Spanning Tree is to be found
        /// </summary>
        public class MISNetwork : Network
        {
            #region Constructors

            /// <summary>
            ///  Instantiates a MST network with specified uid
            /// </summary>
            /// <param name="uid">The unique id assigned to the network</param>
            public MISNetwork(uint uid) : base(uid)
            {
            }

            #endregion
        }

        #endregion

        #region Fields

        private static Random _random;

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a node
        /// </summary>
        public MISNode(uint uid)
        {
            Actions.Add(Int);
            Actions.Add(Msgs);
            Actions.Add(Trans);

            State = new MISState(uid);
        }

        #endregion

        #region Properties

        /// <summary>
        ///  The number of neighbours of this node
        /// </summary>
        public int NumberNeighbours
        {
            get { return InNbrs.Count; }
        }

        /// <summary>
        ///  Random number generator
        /// </summary>
        private static Random Random
        {
            get { return _random ?? (_random = new Random(123)); }
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Creates a network of MISNodes with specified parameters
        /// </summary>
        /// <param name="networkUid">The network uid</param>
        /// <param name="count">The number of nodes to create</param>
        /// <param name="connectionRate">The ratio of connections to all possible links</param>
        /// <param name="rand">The random number generator</param>
        /// <returns>The network</returns>
        public static MISNetwork CreateNetwork(uint networkUid, int count, double connectionRate = 0.5, Random rand = null)
        {
            if (rand != null)
            {
                _random = rand;
            }

            var network = new MISNetwork(networkUid);

            for (var i = 0; i < count; i++)
            {
                var uid = (uint) i + 1;
                var process = new MISNode(uid);
                network.Processes.Add(process);
            }

            for (var i = 0; i < count; i++)
            {
                for (var j = i + 1; j < count; j++)
                {
                    var rn = _random.Next(100);
                    if (rn > connectionRate * 100) continue;  // not connected
                    var ni = network.Processes[i];
                    var nj = network.Processes[j];
                    var linkIj = Link.Connect(ni, nj);
                    var linkJi = Link.Connect(nj, ni);
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
        public static void Run(MISNetwork network, TextWriter tw)
        {
            network.Initialize();

            DisplayNetwork(network, tw, true);

            var allProcessed = AllProcessed(network);

            while (!allProcessed)
            {
                network.RunOneRound();
                DisplayNetwork(network, tw, false);
                allProcessed = AllProcessed(network);
            }
        }

        /// <summary>
        ///  Verifies if the solution is an independent set and possibly a minimal
        /// </summary>
        /// <param name="network">The network to verify</param>
        /// <param name="tw">The text writer to right resultant information to</param>
        /// <returns>True if it's passed the verification</returns>
        /// <remarks>
        ///  It assumes that links come in as pairs of opposite links on the same edge
        /// </remarks>
        public static bool Verify(MISNetwork network, TextWriter tw)
        {
            // check if it's independent

            for (var i = 0; i < network.Links.Count; i+=2)  // see the assumption
            {
                var link = network.Links[i];
                var us = (MISNode)link.UpstreamProcess;
                var ds = (MISNode)link.DownstreamProcess;
                if (us.State.Status == MISState.Statuses.Selected && ds.State.Status == MISState.Statuses.Selected)
                {
                    tw.WriteLine("! It's not an independent set");
                    return false;
                }
            }

            // check if any unselected nodes have no selected neighbours

            foreach (MISNode node in network.Processes)
            {
                if (node.State.Status != MISState.Statuses.Unselected) continue;

                if (node.InNbrs.Count == 0) continue;

                var isMinimal = false;
                foreach (var inblink in node.InNbrs)
                {
                    var nbr = (MISNode) inblink.UpstreamProcess;
                    if (nbr.State.Status == MISState.Statuses.Selected)
                    {
                        isMinimal = true;
                        break;
                    }
                }
                if (!isMinimal)
                {
                    tw.WriteLine("! It's not a minimal independent set");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///  Returns if all nodes in the network has been processed so the algorithm ends
        /// </summary>
        /// <param name="network">The network to examine</param>
        /// <returns>True if all nodes in the network has been processed</returns>
        private static bool AllProcessed(MISNetwork network)
        {
            foreach (MISNode n in network.Processes)
            {
                if (!(n.State.Status == MISState.Statuses.Selected || n.State.Status == MISState.Statuses.Unselected))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///  Displays the statuses of the nodes in the network
        /// </summary>
        /// <param name="network">The network the independent set is to be found from</param>
        /// <param name="tw">The text writer to write the information to</param>
        /// <param name="displayLinks">True if links are to be displayed</param>
        public static void DisplayNetwork(MISNetwork network, TextWriter tw, bool displayLinks)
        {
            WriteLine(tw, "Network ...");

            foreach (MISNode node in network.Processes)
            {
                WriteLine(tw, string.Format(" Process {0}: {1}", node.State.Own, node.State.Status.ToString()));
            }

            if (displayLinks)
            {
                foreach (var link in network.Links)
                {
                    WriteLine(tw, string.Format(" Links {0} <-> {1}", 
                        ((MISState)link.UpstreamProcess.State).Own, ((MISState)link.DownstreamProcess.State).Own));
                }
            }
        }

        /// <summary>
        ///  Writes a line to the specified text writer and to console as well if not equal
        /// </summary>
        /// <param name="tw">The text writer to write to</param>
        /// <param name="line">The line to write</param>
        private static void WriteLine(TextWriter tw, string line)
        {
            if (tw == null) return; // not to write anything
            tw.WriteLine(line);
            if (tw != Console.Out)
            {
                Console.WriteLine(line);
            }
        }


        #region Process<MITState> members

        /// <summary>
        ///  Initializes the process to its start state
        /// </summary>
        public override void Init()
        {
            State.Status = MISState.Statuses.Initial;
            State.UnselectedNeighbours = 0;
            base.Init();
        }

        /// <summary>
        ///  Int method of the node that prepares initial state for each round
        /// </summary>
        public void Int()
        {
            if (State.Status == MISState.Statuses.Initial)
            {
                // returns a random magnitude 
                // In the book the upper bound of the random number is set to amounting to 4*N
                // where N is the number of total nodes
                State.Magnitude = Random.Next();
            }
        }

        /// <summary>
        ///  Msgs method of the node that Produces the output messages according to the state
        /// </summary>
        public override void Msgs()
        {
            switch (State.Status)
            {
                case MISState.Statuses.Initial:
                    foreach (var outNbr in OutNbrs)
                    {
                        outNbr.SendMessage(new MISMessage(MISMessage.Types.Magnitude, State.Magnitude));
                    }
                    break;
                case MISState.Statuses.JustSelected:
                    foreach (var outNbr in OutNbrs)
                    {
                        outNbr.SendMessage(new MISMessage(MISMessage.Types.Selected));   
                    }
                    break;
                case MISState.Statuses.JustUnselected:
                    foreach (var outNbr in OutNbrs)
                    {
                        outNbr.SendMessage(new MISMessage(MISMessage.Types.UnSelected));   
                    }
                    break;
            }
        }

        /// <summary>
        ///  Trans method of the node that transfers the state of the process according to the input messages
        /// </summary>
        public override void Trans()
        {
            if (State.Status == MISState.Statuses.Selected || State.Status == MISState.Statuses.Unselected)
            {
                return;
            }

            switch (State.Status)
            {
                case MISState.Statuses.JustSelected:
                    State.Status = MISState.Statuses.Selected;
                    return;
                case MISState.Statuses.JustUnselected:
                    State.Status = MISState.Statuses.Unselected;
                    return;
            }

            var maxMag = int.MinValue;
            foreach (var inNbr in InNbrs)
            {
                var msg = (MISMessage)inNbr.ReceiveMessage();
                if (msg == null) continue;
                if (msg.Type == MISMessage.Types.Selected)
                {
                    State.Status = MISState.Statuses.JustUnselected;
                    return;
                }
                if (msg.Type == MISMessage.Types.UnSelected)
                {
                    State.UnselectedNeighbours++;
                }
                else if (msg.Type == MISMessage.Types.Magnitude)
                {
                    if (msg.Magnitude > maxMag)
                    {
                        maxMag = msg.Magnitude;
                    }
                }
            }

            if (State.UnselectedNeighbours == NumberNeighbours)
            {
                // if all neighbours are set to unselected the node should be added to the independent set
                // as the neighbours don't expect message from this node, the status can be just set to 'Selected' 
                State.Status = MISState.Statuses.Selected;
                return;
            }

            if (State.Status == MISState.Statuses.Initial)
            {
                // all active (non-unselected) neighbours have less magnitude than this one
                if (maxMag < State.Magnitude)
                {
                    State.Status = MISState.Statuses.JustSelected;
                    return;
                }
            }

           
        }

        #endregion

        #endregion
    }
}
