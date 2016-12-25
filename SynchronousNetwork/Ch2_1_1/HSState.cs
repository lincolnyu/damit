using SynchronousNetwork;

namespace Ch2_1_1
{
    class HSState : State
    {
        #region Enumerations

        public enum Statuses
        {
            IniMsg,
            Challenging,
            Passing,
            PassingLeft,
            PassingRight,
            Chosen,
        }

        #endregion

        #region Properties

        public uint Own { get; private set; }

        public uint Send { get; set; }

        public Statuses Status { get; set; }

        #endregion

        #region Constructors

        public HSState(uint uid)
        {
            Own = uid;
        }

        #endregion
    }
}
