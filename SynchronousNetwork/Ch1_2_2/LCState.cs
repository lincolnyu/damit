using SynchronousNetwork;

namespace Ch1_2_2
{
    class LCState : State
    {
        #region Enumerations

        public enum Statuses
        {
            Unknown,
            Chosen,
            Reported
        }

        #endregion

        #region Properties

        public uint Own { get; private set; }

        public uint? Send { get; set; }

        public Statuses Status { get; set; }

        #endregion

        #region Constructors

        public LCState(uint uid)
        {
            Own = uid;
        }

        #endregion
    }
}
