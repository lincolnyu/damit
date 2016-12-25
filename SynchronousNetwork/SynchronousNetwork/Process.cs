using System.Collections.Generic;

namespace SynchronousNetwork
{
    /// <summary>
    ///  An abstract class that represents a process in a synchronouse network
    /// </summary>
    public abstract class Process
    {
        #region Delegates

        public delegate void ActionDelegate(); 

        #endregion

        #region Properties

        /// <summary>
        ///  State of the process (normally includes all the data for the process to work)
        /// </summary>
        public State State { get; set; }

        /// <summary>
        ///  Outbound links
        /// </summary>
        public readonly IList<Link> OutNbrs = new List<Link>();

        /// <summary>
        ///  Inbound links
        /// </summary>
        public readonly IList<Link> InNbrs = new List<Link>();

        /// <summary>
        ///  Actions to go through in order in each process step
        /// </summary>
        public readonly IList<ActionDelegate> Actions = new List<ActionDelegate>();

        #endregion

        #region Methods

        /// <summary>
        ///  Initializes the process to its start state
        /// </summary>
        public virtual void Init()
        {
            foreach (var inblink in InNbrs)
            {
                inblink.ReceiveMessage();   // clear message buffer
            }
        }

        /// <summary>
        ///  Produces the output messages according to the state
        /// </summary>
        public abstract void Msgs();

        /// <summary>
        ///  Transfers the state of the process according to the input messages
        /// </summary>
        public abstract void Trans();

        /// <summary>
        ///  Clears messages in the buffers of output links
        /// </summary>
        protected void ClearAllOutputMessages()
        {
            foreach (var outNbr in OutNbrs)
            {
                outNbr.Message = null;
            }
        }

        #endregion
    }

    /// <summary>
    ///  A class that represents a process in a synchronous network with the type
    ///  of its state specified
    /// </summary>
    /// <typeparam name="TState">The specified type of its state</typeparam>
    public abstract class Process<TState> : Process where TState : State
    {
        #region Properties

        /// <summary>
        ///  Overwritten State property of more specific type
        /// </summary>
        public new TState State
        {
            get { return (TState)base.State; }
            set { base.State = value; }
        }

        #endregion
    }
}
