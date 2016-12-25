using System;
using System.Collections.Generic;
using LeaderElectionRing;
using SynchronousNetwork;

namespace Ch1_2_2
{
    class LCProcess : Process<LCState>, Network.IToShortString, IOnewayRungProcess
    {
        #region Nested types

        public class LCNetwork : Ring
        {
            #region Constructors

            public LCNetwork(uint uid, DummyNode dummy) 
                : base(uid, dummy)
            {
            }

            #endregion
        }

        #endregion

        #region Properties

        public Link InboundNbr
        {
            get { return InNbrs[0]; }
        }

        public Link OutboundNbr
        {
            get { return OutNbrs[0]; }
        }

        public Link DummyOutput
        {
            get { return OutNbrs[1]; }
        }

        #endregion

        #region Constructors

        public LCProcess(uint uid)
        {
            State = new LCState(uid);
        }

        #endregion

        #region Methods

        public override void Init()
        {
            State.Send = State.Own;
            State.Status = LCState.Statuses.Unknown;
        }

        public override void Msgs()
        {
            if (State.Status == LCState.Statuses.Chosen)
            {
                DummyOutput.SendMessage(new UidMessage(State.Own));
            }
            else if (State.Send != null)
            {
                OutboundNbr.SendMessage(new UidMessage(State.Send.Value));
            }
        }

        public override void Trans()
        {
            var m = (UidMessage) InboundNbr.ReceiveMessage();

            State.Send = null;
            if (State.Status == LCState.Statuses.Chosen)
            {
                State.Status = LCState.Statuses.Reported;
            }
            else if (m != null)
            {
                if (m.Uid > State.Own)
                {
                    State.Send = m.Uid;
                }
                else if (m.Uid == State.Own)
                {
                    State.Status = LCState.Statuses.Chosen;
                    State.Send = null;
                }
                else
                {
                    State.Send = State.Own;
                }
            }
        }

        public string ToShortString()
        {
            var s = "{";
            s += string.Format("{0}-", State.Own);
            switch (State.Status)
            {
                case LCState.Statuses.Unknown:
                    s += "U";
                    break;
                case LCState.Statuses.Chosen:
                    s += "C";
                    break;
                case LCState.Statuses.Reported:
                    s += "R";
                    break;
            }
            s += "}";
            return s;
        }

        #endregion

        #region Static Methods

        public static LCNetwork CreateNetwork(uint networkUid, int count)
        {
            var rand = new Random();
            LCProcess prevProcess = null;
            LCProcess firstProcess = null;
            var dummyNode = new DummyNode();
            var network = new LCNetwork(networkUid, dummyNode);
            var usedIds = new HashSet<uint> {0};
            for (var i = 0; i < count; i++)
            {
                uint uid;
                do
                {
                    uid = (uint) (rand.Next()%count + 1);
                } while (usedIds.Contains(uid));
                usedIds.Add(uid);
                var process = new LCProcess(uid);
                if (prevProcess != null)
                {
                    var link = Link.Connect(prevProcess, process);
                    network.Links.Add(link);
                }
                else
                {
                    firstProcess = process;
                }
                network.Processes.Add(process);
                prevProcess = process;
            }
            if (count > 1)
            {
                var link = Link.Connect(prevProcess, firstProcess);
                network.Links.Add(link);
            }

            foreach (var process in network.Processes)
            {
                var link = Link.Connect(process, dummyNode);
                network.Links.Add(link);
            }
            return network;
        }

        public static void Run(LCNetwork network)
        {
            network.Initialize();
            var round = 0;
            for (; !network.Dummy.State.IsConcluded; round++)
            {
                network.RunOneRound(() => Console.WriteLine(network.ToShortString()));
            }

            Console.WriteLine("The leader {0} is elected after {1} round(s) with {2} message(s) sent.", 
                network.Dummy.State.Uid, round, network.TotalMessages);
        }

        #endregion
    }
}
