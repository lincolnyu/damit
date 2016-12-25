using SynchronousNetwork;

namespace Ch3_2
{
    public class MSTMessage : Message
    {
        #region Enumerations

        public enum Types
        {
            ConnectionRequest,
            LeaderAnnouncment,
            ReplyToLeader,
            MinimumOutbound,
            MinimumAnnouncement,
        }

        #endregion

        #region Properties

        public Types Type { get; private set; }

        /// <summary>
        ///  The value that is associated with the message
        /// </summary>
        public object Value { get; private set; }

        #endregion

        #region Constructors

        public MSTMessage(Types type)
        {
            Type = type;
        }

        public MSTMessage(Types type, object value)
        {
            Type = type;
            Value = value;
        }

        #endregion

    }
}
