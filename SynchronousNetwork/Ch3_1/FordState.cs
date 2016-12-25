using System.Collections.Generic;
using SynchronousNetwork;

namespace Ch3_1
{
    public class FordState : State
    {
        #region Enumerations

        public enum Statuses
        {
            IsTarget,
            TargetReported,
            Listening,
            Updating,
        }

        #endregion

        #region Properties

        public uint Own { get; private set; }
        public Statuses Status { get; set; }
        public uint Parent { get; set; }
        public double BestDist { get; set; }
        public bool IsTarget { get; private set; }

        public HashSet<FordNode> Excluded { get; private set; } 

        #endregion

        #region Constructors

        public FordState(uint uid, bool isTarget)
        {
            Own = uid;
            IsTarget = isTarget;
            Excluded = new HashSet<FordNode>();
        }

        #endregion
    }
}
