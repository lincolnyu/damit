using SynchronousNetwork;

namespace Ch2_3
{
    class BFSMessage : Message
    {
        public const string Message = "Report";

        public static readonly BFSMessage Instance = new BFSMessage();
    }
}
