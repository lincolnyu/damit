using SynchronousNetwork;

namespace Ch3_3
{
    /// <summary>
    ///  State data for Maximal Indepedent Set
    /// </summary>
    public class MISState : State
    {
        #region Enumerations

        /// <summary>
        ///  The enumeration of all possible statuses
        /// </summary>
        public enum Statuses
        {
            Initial,
            JustSelected,
            JustUnselected,
            Selected,
            Unselected
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a state object with the specified ID
        /// </summary>
        /// <param name="uid">The unique ID to instantiate the object with</param>
        public MISState(uint uid)
        {
            Own = uid;
        }

        #endregion

        #region Properties

        /// <summary>
        ///  Unique id of the corresponding node (not used in the algorithm)
        /// </summary>
        public uint Own { get; set; }

        /// <summary>
        ///  The index of the current status, initially 'Initial'
        /// </summary>
        public Statuses Status { get; set; }

        /// <summary>
        ///  The current number of neighbours that have been determined to be unselected
        /// </summary>
        public int UnselectedNeighbours { get; set; }

        /// <summary>
        ///  The value used to compare with the neigbouring nodes to decide the winner of the selection
        /// </summary>
        public int Magnitude { get; set; }

        #endregion
    }
}
