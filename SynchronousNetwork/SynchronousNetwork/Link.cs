namespace SynchronousNetwork
{
    /// <summary>
    ///  A class that represents a link for connecting processes and passing
    ///  messages from one process to another in a syncrhonous network
    /// </summary>
    public class Link
    {
        #region Properties

        /// <summary>
        ///  The upstream process the link is linked with
        /// </summary>
        public Process UpstreamProcess { get; private set; }

        /// <summary>
        ///  The downstream process the link is linked with
        /// </summary>
        public Process DownstreamProcess { get; private set; }

        /// <summary>
        ///  The index of the link in list of links the upstream process
        /// </summary>
        public int IndexInUpstream { get; private set; }

        /// <summary>
        ///  The index of the link in list of links the downstream process
        /// </summary>
        public int IndexInDownstream { get; private set; }

        /// <summary>
        ///  The message the link is currently carrying
        /// </summary>
        public Message Message { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///  Sends a message through the link
        /// </summary>
        /// <param name="msg">The message to send</param>
        public void SendMessage(Message msg)
        {
            Message = msg;
        }

        /// <summary>
        ///  Gets and removes the message from the buffer of the link
        /// </summary>
        /// <returns>The message received</returns>
        public Message ReceiveMessage()
        {
            var msg = Message;
            Message = null;
            return msg;
        }

        #endregion

        #region Static methods

        /// <summary>
        ///  Connects two processes with the given link which is to be added to the end of the 
        ///  link list in each of the two processes
        /// </summary>
        /// <param name="link">The link to connnect the two processes with</param>
        /// <param name="upstreamProcess">The upstreawm process to link with</param>
        /// <param name="downstreamProcess">The downstream process to link with</param>
        protected static void AddLink(Link link, Process upstreamProcess, Process downstreamProcess)
        {
            link.UpstreamProcess = upstreamProcess;
            link.DownstreamProcess = downstreamProcess;
            link.IndexInUpstream = upstreamProcess.OutNbrs.Count;
            link.IndexInDownstream = downstreamProcess.InNbrs.Count;
            upstreamProcess.OutNbrs.Add(link);
            downstreamProcess.InNbrs.Add(link);
        }

        /// <summary>
        ///  Connects two processes with the given link with the index of link in each of
        ///  these two processes specified
        /// </summary>
        /// <param name="link">The link to connect the two processes with</param>
        /// <param name="upstreamProcess">The upstream process to link with</param>
        /// <param name="indexUpstream">The index of the link in the upstream process</param>
        /// <param name="downstreamProcess">The downstream process to link with</param>
        /// <param name="indexDownstream">The index of the link in the downstream process</param>
        protected static void InsertLink(Link link, Process upstreamProcess, int indexUpstream,
                                         Process downstreamProcess, int indexDownstream)
        {
            link.UpstreamProcess = upstreamProcess;
            link.DownstreamProcess = downstreamProcess;
            link.IndexInUpstream = indexUpstream;
            link.IndexInDownstream = indexDownstream;
            while (upstreamProcess.OutNbrs.Count <= indexUpstream)
            {
                upstreamProcess.OutNbrs.Add(null);
            }
            upstreamProcess.OutNbrs[indexUpstream] = link;
            while (downstreamProcess.InNbrs.Count <= indexDownstream)
            {
                downstreamProcess.InNbrs.Add(null);
            }
            downstreamProcess.InNbrs[indexDownstream] = link;
        }

        /// <summary>
        ///  Connects two proceses with a unidirecitonal link which is to be added
        ///  to the end of the link list in each of the two processes
        /// </summary>
        /// <param name="upstreamProcess">The upstreawm process to link with</param>
        /// <param name="downstreamProcess">The downstream process to link with</param>
        /// <returns>The link created to connect the two processes</returns>
        public static Link Connect(Process upstreamProcess, Process downstreamProcess)
        {
            var link = new Link();
            AddLink(link, upstreamProcess, downstreamProcess);
            return link;
        }

        /// <summary>
        ///  Connects two processes with a unidirectional link with the index of link in each of
        ///  these two processes specified
        /// </summary>
        /// <param name="upstreamProcess">The upstream process to link with</param>
        /// <param name="indexUpstream">The index of the link in the upstream process</param>
        /// <param name="downstreamProcess">The downstream process to link with</param>
        /// <param name="indexDownstream">The index of the link in the downstream process</param>
        /// <returns>The link created to connect the two processes</returns>
        public static Link Connect(Process upstreamProcess, int indexUpstream, Process downstreamProcess,
                                   int indexDownstream)
        {
            var link = new Link();
            InsertLink(link, upstreamProcess, indexUpstream, downstreamProcess, indexDownstream);
            return link;
        }

        #endregion
    }
}
