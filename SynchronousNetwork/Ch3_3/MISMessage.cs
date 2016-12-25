using SynchronousNetwork;

namespace Ch3_3
{
    public class MISMessage : Message
    {
        #region Enumerations

        /// <summary>
        ///  All possible types of a message
        /// </summary>
        public enum Types
        {
            Magnitude,
            Selected,
            UnSelected
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates the message with the specified type
        /// </summary>
        /// <param name="type">The type of the message</param>
        public MISMessage(Types type)
            : this(type, 0)
        {
        }

        /// <summary>
        ///  Instantiates the message with the specified type and the specified magnitude
        /// </summary>
        /// <param name="type">The type of the message (should always be 'Magnitude')</param>
        /// <param name="magnitude">The magnitude value associated with the message</param>
        public MISMessage(Types type, int magnitude)
        {
            Type = type;
            Magnitude = magnitude;
        }

        #endregion

        #region Properties

        /// <summary>
        ///  The type of the message
        /// </summary>
        public Types Type { get; private set; }


        public int Magnitude { get; private set; }

        #endregion
    }
}
