using System;
using Graph;
using SynchronousNetwork;

namespace Ch2_3
{
    class BFSNode : Process<BFSState>
    {
        #region Nested types

        public class BFSNetwork : Network
        {
            public DummyNode Dummy { get; private set; }

            public BFSNetwork(uint uid, DummyNode dummy)
                : base(uid)
            {
                Dummy = dummy;
                Processes.Add(dummy);
            }
        }

        #endregion

        #region Constructors

        public BFSNode(uint uid, bool isIni)
        {
            State = new BFSState(uid, isIni);
        }

        #endregion

        #region Methods

        public override void Init()
        {
            if (State.IsRoot)
            {
                State.Status = BFSState.Statuses.Reporting;
            }
            else
            {
                State.Status = BFSState.Statuses.Waiting;
            }
        }

        public override void Msgs()
        {
            switch (State.Status)
            {
                case BFSState.Statuses.Reporting:
                    foreach (var output in OutNbrs)
                    {
                        var outBfs = output.DownstreamProcess as BFSNode;
                        if (outBfs == null)
                        {
                            // must be the dummy node
                            output.SendMessage(new DummyMessage(State.Own, State.Parent));
                            continue;
                        }
                        if (State.Inputs.Contains(outBfs))
                        {
                            continue;
                        }
                        output.SendMessage(BFSMessage.Instance);
                    }
                    break;
                default:
                    ClearAllOutputMessages();
                    break;
            }
        }

        public override void Trans()
        {
            switch (State.Status)
            {
                case BFSState.Statuses.Waiting:
                    var parentSet = false;
                    foreach (var input in InNbrs)
                    {
                        if (input.Message == null) continue;
                        State.Inputs.Add((BFSNode)input.UpstreamProcess);
                        if (parentSet) continue;
                        State.Parent = ((BFSNode)input.UpstreamProcess).State.Own;
                        parentSet = true;
                    }
                    if (State.Inputs.Count > 0)
                    {
                        State.Status = BFSState.Statuses.Reporting;
                    }
                    break;
                case BFSState.Statuses.Reporting:
                    State.Status = BFSState.Statuses.Reported;
                    break;
            }
        }

        #endregion

        #region Static Methods

        public static BFSNetwork CreateNetwork(uint networkUid, int count, double connectRate)
        {
            var dummyNode = new DummyNode(count);
            var network = new BFSNetwork(networkUid, dummyNode);
            var rand = new Random();
            var selected = rand.Next(0, count);
            for (var i = 0; i < count; i++)
            {
                var uid = (uint)i + 1;
                var process = new BFSNode(uid, i == selected);
                network.Processes.Add(process);
            }
            for (var i = 0; i < network.Processes.Count; i++)    // note the iteration includes the dummy node
            {
                var proci = network.Processes[i];
                if (proci is DummyNode) continue;
                for (var j = 0; j < network.Processes.Count; j++)
                {
                    var procj = network.Processes[j];
                    if (procj is DummyNode) continue;
                    if (j == i) continue;
                    var rd = rand.NextDouble();
                    if (rd > connectRate)
                    {
                        continue;
                    }
                    var link = Link.Connect(proci, procj);
                    network.Links.Add(link);
                }
                var dummyLink = Link.Connect(proci, dummyNode);
                network.Links.Add(dummyLink);
            }
            return network;
        }

        public static void Run(BFSNetwork network)
        {
            DisplayNetwork(network);

            network.Initialize();

            for (; !network.Dummy.State.Finished; )
            {
                network.RunOneRound();
            }

            DisplayReports(network);
        }

        public static void DisplayNetwork(BFSNetwork network)
        {
            Console.WriteLine("Network ...");

            foreach (var proc in network.Processes)
            {
                var bfsProc = proc as BFSNode;
                if (bfsProc == null) continue;
                Console.WriteLine(" Process {0}", bfsProc.State.Own);
            }

            foreach (var link in network.Links)
            {
                var usProc = link.UpstreamProcess as BFSNode;
                var dsProc = link.DownstreamProcess as BFSNode;
                if (usProc == null || dsProc == null)
                {
                    continue;
                }
                Console.WriteLine(" Link {0} -> {1}", usProc.State.Own, dsProc.State.Own);
            }
        }

        public static void DisplayReports(BFSNetwork network)
        {
            Console.WriteLine("Reports ...");

            foreach (var msgInfo in network.Dummy.State.Messages)
            {
                var dummyMsg = msgInfo.Message as DummyMessage;
                if (dummyMsg == null) continue; // actually it's an unexpected error
                Console.WriteLine(" {0}: {1}->{2}", msgInfo.Round, dummyMsg.ChildUid, dummyMsg.ParentUid);
            }
        }

        #endregion
    }
}
