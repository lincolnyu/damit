using SynchronousNetwork;

namespace Graph
{
    public class DummyNode : Process<DummyState>
    {
        #region Nested types

        public class MessageInfo
        {
            public Message Message { get; set; }
            public int Round { get; set; }
        }

        #endregion

        #region Constructors

        public DummyNode(int totalNodes)
        {
            State = new DummyState { TotalNodes = totalNodes };
        }

        #endregion

        #region Methods

        public override void Init()
        {
            State.Round = 0;
            State.Finished = false;
        }

        public override void Msgs()
        {
            
        }

        public override void Trans()
        {
            var reportCount = 0;
            foreach (var input in InNbrs)
            {
                var msg = input.ReceiveMessage();
                if (msg == null) continue;
                var msgInfo = new MessageInfo { Message = msg, Round = State.Round };
                State.Messages.Add(msgInfo);
                reportCount++;
            }
            State.Round++;
            if (reportCount == 0)
            {
                State.Finished = true;
            }
        }

        #endregion
    }
}
