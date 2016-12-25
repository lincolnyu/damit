using System;
using Graph;
using SynchronousNetwork;

namespace Ch3_1
{
    public class FordNode : Process<FordState>
    {
        #region Nested types

        public class FordNetwork : Network
        {
            public DummyNode Dummy { get; private set; }

            public FordNetwork(uint uid, DummyNode dummy)
                : base(uid)
            {
                Dummy = dummy;
                Processes.Add(dummy);
            }
        }

        #endregion

        #region Constructors

        public FordNode(uint uid, bool isTarget)
        {
            State = new FordState(uid, isTarget);
        }

        #endregion

        #region Methods

        public override void Init()
        {
            if (State.IsTarget)
            {
                State.Status = FordState.Statuses.IsTarget;
                State.BestDist = 0;
            }
            else
            {
                State.Status = FordState.Statuses.Listening;
                State.BestDist = double.PositiveInfinity;
            }
        }

        public override void Msgs()
        {
            switch (State.Status)
            {
                case FordState.Statuses.Updating:
                case FordState.Statuses.IsTarget:
                    // As the mesage is readonly and won't be passed on
                    // it can have only one copy and shared
                    foreach (var output in OutNbrs)
                    {
                        var dummyOut = output.DownstreamProcess as DummyNode;
                        if (dummyOut != null)
                        {
                            var message = new DummyMessage(State.Own, State.BestDist, State.Parent);
                            output.SendMessage(message);
                            continue;
                        }
                        var fordNode = output.DownstreamProcess as FordNode;
                        if (fordNode != null && !State.Excluded.Contains(fordNode))
                        {
                            var message = new FordMessage(State.Own, State.BestDist);
                            output.SendMessage(message);
                        }
                    }
                    break;
            }
        }

        public override void Trans()
        {
            switch (State.Status)
            {
                case FordState.Statuses.IsTarget:
                    State.Status = FordState.Statuses.TargetReported;
                    break;
                case FordState.Statuses.Listening:
                case FordState.Statuses.Updating:
                    var needUpdate = false;
                    foreach (var input in InNbrs)
                    {
                        var weighted = (WeightedLink)input;
                        var msg = weighted.Message as FordMessage;
                        if (msg == null) continue;
                        var weight = weighted.Weight;
                        var dist = weight + msg.BestDist;
                        if (!(dist < State.BestDist)) continue;
                        State.BestDist = dist;
                        var usproc = (FordNode) weighted.UpstreamProcess;
                        State.Parent = usproc.State.Own;
                        needUpdate = true;
                    }
                    if (needUpdate)
                    {
                        // this is to optimize the amount of messages sent
                        State.Excluded.Clear();
                        foreach (var input in InNbrs)
                        {
                            var weighted = (WeightedLink)input;
                            var msg = weighted.Message as FordMessage;
                            if (msg == null) continue;
                            var weight = weighted.Weight;
                            var dist = msg.BestDist;
                            if (State.BestDist + weight >= dist)
                            {
                                State.Excluded.Add((FordNode)input.UpstreamProcess);
                            }
                        }
                    }
                    State.Status = needUpdate
                                       ? FordState.Statuses.Updating
                                       : FordState.Statuses.Listening;
                    break;
            }
        }

        #endregion

        #region Static methods

        public static FordNetwork CreateNetwork(uint networkUid, int count, double connectRate, double sparseRate = 0.5)
        {
            var dummyNode = new DummyNode(count);
            var network = new FordNetwork(networkUid, dummyNode);
            var rand = new Random();
            var selected = rand.Next(0, count);
            for (var i = 0; i < count; i++)
            {
                var uid = (uint)i + 1;
                var process = new FordNode(uid, i == selected);
                network.Processes.Add(process);
            }
            var maxLinks = count * (count - 1) / 2;
            var avgLinks = (int)(maxLinks * connectRate);
            var maxWeight = (int)(avgLinks * sparseRate);
            for (var i = 0; i < network.Processes.Count; i++) // note the iteration includes the dummy node
            {
                var proci = network.Processes[i];
                if (proci is DummyNode) continue;
                for (var j = i+1; j < network.Processes.Count; j++)
                {
                    var procj = network.Processes[j];
                    if (procj is DummyNode) continue;
                    var rd = rand.NextDouble();
                    if (rd > connectRate)
                    {
                        continue;
                    }
                    var weight = rand.Next(1, 100);
                    var link1 = WeightedLink.Connect(proci, procj, weight);
                    var link2 = WeightedLink.Connect(procj, proci, weight);
                    network.Links.Add(link1);
                    network.Links.Add(link2);
                }
                var dummyLink = Link.Connect(proci, dummyNode);
                network.Links.Add(dummyLink);
            }
            return network;
        }

        public static void Run(FordNetwork network)
        {            
            Utility.DisplayNetwork(network, check => check is DummyNode);

            network.Initialize();

            for (; !network.Dummy.State.Finished; )
            {
                network.RunOneRound();
            }

            DisplayReports(network);
        }

        public static void DisplayReports(FordNetwork network)
        {
            Console.WriteLine("Reports ...");

            foreach (var msgInfo in network.Dummy.State.Messages)
            {
                var dummyMsg = msgInfo.Message as DummyMessage;
                if (dummyMsg == null) continue; // actually it's an unexpected error
                
                Console.WriteLine(" {0}: {1}->{2}, {3:0.000}", msgInfo.Round, dummyMsg.Source, dummyMsg.Parent, dummyMsg.BestDist);
            }
        }

        #endregion
    }
}
