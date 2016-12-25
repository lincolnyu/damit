using SynchronousNetwork;

namespace LeaderElectionRing
{
    public class UidMessage : Message
    {
         #region Properties

        public uint Uid { get; private set; }

        #endregion

        #region Constructors

        public UidMessage(uint uid)
        {
            Uid = uid;
        }

        #endregion
    }
}
