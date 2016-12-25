using System.Text;
using SynchronousNetwork;

namespace LeaderElectionRing
{
    public class Ring : Network
    {
        #region Delegates

        public delegate void Injection();

        #endregion

        #region Properties

        public DummyNode Dummy { get; private set; }

        public int TotalMessages { get; private set; }

        #endregion

        #region Constructors

        public Ring(uint uid, DummyNode dummy)
            : base(uid)
        {
            Dummy = dummy;
            Processes.Add(Dummy);
            TotalMessages = 0;
        }

        #endregion

        #region Methods

        public new void Initialize()
        {
            base.Initialize();
            TotalMessages = 0;
        }

        public void RunOneRound(Injection inject = null)
        {
            foreach (var process in Processes)
            {
                process.Msgs();
            }

            // Counts the number of total messages sent this round
            foreach (var link in Links)
            {
                if (link.Message != null)
                {
                    TotalMessages++;
                }
            }

            if (inject != null)
            {
                inject();
            }

            foreach (var process in Processes)
            {
                process.Trans();
            }
        }

        public override string ToShortString()
        {
            var sb = new StringBuilder();
            foreach (var t in Processes)
            {
                var onewayProcess = t as IOnewayRungProcess;
                if (onewayProcess != null)
                {
                    sb.Append(onewayProcess.ToShortString());
                    sb.Append("='");
                    var arrow = onewayProcess.OutboundNbr;
                    var m = (UidMessage)arrow.Message;
                    if (m != null)
                    {
                        sb.Append(m.Uid);
                    }
                    else
                    {
                        sb.Append("null");
                    }
                    sb.Append("'=>");
                }

                var twowayProcess = t as ITwowayRungProcess;
                if (twowayProcess != null)
                {
                    sb.Append(twowayProcess.ToShortString());
                    sb.Append("<='");
                    var outRight = twowayProcess.OutboundRightNbr;
                    var inRight = twowayProcess.InboundRightNbr;
                    var mOut = (UidMessage)outRight.Message;
                    var mIn = (UidMessage) inRight.Message;
                    if (mIn != null)
                    {
                        sb.Append(mIn.Uid);
                    }
                    else
                    {
                        sb.Append("null");
                    }
                    sb.Append("'|'");
                    if (mOut != null)
                    {
                        sb.Append(mOut.Uid);
                    }
                    else
                    {
                        sb.Append("null");
                    }
                    sb.Append("'=>");                     
                }
            }
            sb.Append("\r\n");
            return sb.ToString();
        }

        #endregion
    }
}
