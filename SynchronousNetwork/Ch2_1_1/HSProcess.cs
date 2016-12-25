using System;
using System.Collections.Generic;
using LeaderElectionRing;
using SynchronousNetwork;

namespace Ch2_1_1
{
    class HSProcess : Process<HSState>, ITwowayRungProcess
    {
        #region Nested types

        public class HSNetwork : Ring
        {
            #region Constructors

            public HSNetwork(uint uid, DummyNode dummy) 
                : base(uid, dummy)
            {
            }

            #endregion
        }

        #endregion

        #region Properties

        public Link InboundLeftNbr
        {
            get { return InNbrs[0]; }
        }

        public Link InboundRightNbr
        {
            get { return InNbrs[1]; }
        }

        public Link OutboundLeftNbr
        {
            get { return OutNbrs[1]; }
        }

        public Link OutboundRightNbr
        {
            get { return OutNbrs[0]; }
        }

        public Link DummyOutput
        {
            get { return OutNbrs[2]; }
        }

        #endregion

        #region Constructors

        public HSProcess(uint uid)
        {
            State = new HSState(uid);
        }

        #endregion

        #region Static methods

        #region Members of Process

        public override void Init()
        {
            State.Status = HSState.Statuses.IniMsg;
            State.Send = State.Own;
        }

        public override void Msgs()
        {
            switch (State.Status)
            {
                case HSState.Statuses.IniMsg:
                    OutboundLeftNbr.SendMessage(new UidMessage(State.Send));
                    OutboundRightNbr.SendMessage(new UidMessage(State.Send));
                    break;
                case HSState.Statuses.PassingLeft:
                    OutboundLeftNbr.SendMessage(new UidMessage(State.Send));
                    break;
                case HSState.Statuses.PassingRight:
                    OutboundRightNbr.SendMessage(new UidMessage(State.Send));
                    break;
                case HSState.Statuses.Chosen:
                    DummyOutput.SendMessage(new UidMessage(State.Own));
                    break;
            }
        }

        public override void Trans()
        {
            var ml = (UidMessage)InboundLeftNbr.ReceiveMessage();
            var mr = (UidMessage)InboundRightNbr.ReceiveMessage();
            if (ml != null && mr != null)
            {
                var maxUid = State.Own;
                if (ml.Uid > maxUid)
                {
                    maxUid = ml.Uid;
                }
                if (mr.Uid > maxUid)
                {
                    maxUid = mr.Uid;
                }

                if (maxUid > State.Own)
                {
                    // NOTE only passing to one side to minimize the messages
                    State.Status = ml.Uid > mr.Uid ? HSState.Statuses.PassingRight
                        : HSState.Statuses.PassingLeft;
                    State.Send = maxUid;
                }
                else if (ml.Uid == State.Own || mr.Uid == State.Own)
                {
                    State.Status = HSState.Statuses.Chosen;
                }
                else if (State.Status == HSState.Statuses.IniMsg)
                {
                    State.Status = HSState.Statuses.Challenging;
                }
                /* else maxUid < State.Own do nothing*/
            }
            else if (ml != null)
            {
                if (ml.Uid > State.Own)
                {
                    State.Status = HSState.Statuses.PassingRight;
                    State.Send = ml.Uid;
                }
                if (ml.Uid == State.Own)
                {
                    State.Status = HSState.Statuses.Chosen;
                }
                else if (State.Status == HSState.Statuses.IniMsg)
                {
                    State.Status = HSState.Statuses.Challenging;
                }
                else
                {
                    State.Status = HSState.Statuses.Passing;
                }
            }
            else if (mr != null)
            {
                if (mr.Uid > State.Own)
                {
                    State.Status = HSState.Statuses.PassingLeft;
                    State.Send = mr.Uid;
                }
                else if (mr.Uid == State.Own)
                {
                    State.Status = HSState.Statuses.Chosen;
                }
                else if (State.Status == HSState.Statuses.IniMsg)
                {
                    State.Status = HSState.Statuses.Challenging;
                }
                else
                {
                    State.Status = HSState.Statuses.Passing;
                }
            }
            else if (State.Status == HSState.Statuses.IniMsg)
            {
                State.Status = HSState.Statuses.Challenging;
            }
            else if (State.Status != HSState.Statuses.Challenging)
            {
                State.Status = HSState.Statuses.Passing;
            }
        }

        #endregion

        public string ToShortString()
        {
            var s = "{";
            s += string.Format("{0}-", State.Own);
            switch (State.Status)
            {
                case HSState.Statuses.IniMsg:
                    s += "I";
                    break;
                case HSState.Statuses.Chosen:
                    s += "C";
                    break;
                case HSState.Statuses.Challenging:
                    s += "H";
                    break;
                case HSState.Statuses.Passing:
                    s += "P";
                    break;
                case HSState.Statuses.PassingLeft:
                    s += "<";
                    break;
                case HSState.Statuses.PassingRight:
                    s += ">";
                    break;
            }
            s += "}";
            return s;
        }

        #endregion

        #region Static Methods

        public static HSNetwork CreateNetwork(uint networkUid, int count)
        {
            var rand = new Random();
            HSProcess prevProcess = null;
            HSProcess firstProcess = null;
            var dummyNode = new DummyNode();
            var network = new HSNetwork(networkUid, dummyNode);
            var usedIds = new HashSet<uint> { 0 };
            for (var i = 0; i < count; i++)
            {
                uint uid;
                do
                {
                    uid = (uint) (rand.Next()%count + 1);
                } while (usedIds.Contains(uid));
                usedIds.Add(uid);
                var process = new HSProcess(uid);
                if (prevProcess != null)
                {
                    var linkLR = Link.Connect(prevProcess, 0, process, 0);
                    var linkRL = Link.Connect(process, 1, prevProcess, 1);
                    network.Links.Add(linkLR);
                    network.Links.Add(linkRL);
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
                var linkLR = Link.Connect(prevProcess, 0, firstProcess, 0);
                var linkRL = Link.Connect(firstProcess, 1, prevProcess, 1);

                network.Links.Add(linkLR);
                network.Links.Add(linkRL);
            }

            foreach (var process in network.Processes)
            {
                var linkToDummy = Link.Connect(process, dummyNode);
                network.Links.Add(linkToDummy);
            }
            return network;
        }

        public static void Run(HSNetwork network)
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
