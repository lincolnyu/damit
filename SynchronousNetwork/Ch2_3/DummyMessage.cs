
using SynchronousNetwork;

namespace Ch2_3
{
    class DummyMessage : Message
    {
        #region Properties

        public uint ChildUid { get; private set; }
        public uint ParentUid { get; private set; }

        #endregion

        #region Constructors

        public DummyMessage(uint childUid, uint parentUid)
        {
            ChildUid = childUid;
            ParentUid = parentUid;
        }

        #endregion
    }
}
