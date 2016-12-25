using System.Collections.Generic;
using SynchronousNetwork;

namespace Ch2_3
{
    class BFSState : State
    {
        #region Enumerations

        public enum Statuses
        {
            Waiting,
            Reporting,
            Reported,
        }

        #endregion

        #region Properties

        public uint Own { get; private set; }
        public uint Parent { get; set; }

        public Statuses Status { get; set; }

        public HashSet<BFSNode> Inputs { get; private set; }

        public bool IsRoot { get; private set; }

        #endregion

        #region Constructors

        public BFSState(uint uid, bool isRoot)
        {
            Own = uid;
            Inputs = new HashSet<BFSNode>();
            IsRoot = isRoot;
        }

        #endregion
    }
}
