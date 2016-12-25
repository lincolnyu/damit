using System.Collections.Generic;
using SynchronousNetwork;

namespace Graph
{
    public class DummyState : State
    {
        #region Properties

        public List<DummyNode.MessageInfo> Messages { get; private set; }

        public int TotalNodes { get; set; }

        public bool Finished { get; set; }

        public int Round { get; set; }

        #endregion

        #region Constructors

        public DummyState()
        {
            Messages = new List<DummyNode.MessageInfo>();
        }

        #endregion
    }
}
