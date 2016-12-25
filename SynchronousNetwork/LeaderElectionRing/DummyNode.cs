using SynchronousNetwork;

namespace LeaderElectionRing
{
    public class DummyNode : Process
    {
        #region Nested types

        public class DummyState : State
        {
            public bool IsConcluded { get; set; }
            public uint Uid { get; set; }
        }

        #endregion

        #region Properties

        public new DummyState State
        {
            get { return (DummyState)base.State; }
            set { base.State = value; }
        }

        #endregion

        #region Constructors

        public DummyNode()
        {
            State = new DummyState
                {
                    IsConcluded = false,
                    Uid = 0
                };
        }

        #endregion

        #region Methods

        public override void Init()
        {
        }

        public override void Msgs()
        {
        }

        public override void Trans()
        {
            foreach (var inNbr in InNbrs)
            {
                var m = (UidMessage) inNbr.ReceiveMessage();
                if (m != null)
                {
                    State.IsConcluded = true;
                    State.Uid = m.Uid;
                    break;
                }
            }
        }

        #endregion
    }
}
