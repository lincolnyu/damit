using System.Collections.Generic;
using System.Text;

namespace SynchronousNetwork
{
    /// <summary>
    ///  A class that encapsulates the essence of a synchronous network
    /// </summary>
    public class Network
    {
        #region Nested types

        /// <summary>
        ///  object that can be converted to short string
        /// </summary>
        public interface IToShortString
        {
            string ToShortString();
        }
        
        #endregion

        #region Properties

        /// <summary>
        ///  The unique ID for the network
        /// </summary>
        public uint Uid { get; private set; }

        /// <summary>
        ///  All processes in the network
        /// </summary>
        public IList<Process> Processes { get; private set; }
        
        /// <summary>
        ///  All links that connect the processes in the network
        /// </summary>
        public IList<Link> Links { get; private set; } 

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates the network with the specified ID
        /// </summary>
        /// <param name="uid">The unique ID assigned to the network</param>
        public Network(uint uid)
        {
            Processes = new List<Process>();
            Links = new List<Link>();
            Uid = uid;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Initializes and invokes Init() method on all processes
        /// </summary>
        public void Initialize()
        {
            foreach (var process in Processes)
            {
                if (process.Actions.Count == 0)
                {
                    // default action list
                    process.Actions.Add(process.Msgs);
                    process.Actions.Add(process.Trans);                   
                }
                process.Init();
            }
        }

        /// <summary>
        ///  Loops through all defined actions on the processes. For each action
        ///  The processes are iterated and no interaction between the processes
        ///  that occur this round are processed
        /// </summary>
        public void RunOneRound()
        {
            var process0 = Processes[0];
            var actionCount = process0.Actions.Count;
            for (var i = 0; i < actionCount; i++)
            {
                foreach (var process in Processes)
                {
                    process.Actions[i]();
                }
            }
        }

        /// <summary>
        ///  Returns the string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Network {0}", Uid);
            sb.Append(">>> Processes >>> ");
            var firstTime = true;
            foreach (var process in Processes)
            {
                var sp = process.ToString();
                if (firstTime)
                {
                    firstTime = false;
                }
                else
                {
                    sb.Append("---\n");
                }
                sb.Append(sp);
                if (!(sp.Length > 0 && sp[sp.Length - 1] == '\n'))
                {
                    sb.Append("\r\n");
                }
            }
            sb.Append(">>> Links >>> ");
            firstTime = true;
            foreach (var link in Links)
            {
                var sl = link.ToString();
                if (firstTime)
                {
                    firstTime = false;
                }
                else
                {
                    sb.Append("---\n");
                }
                sb.Append(sl);
                if (!(sb.Length > 0 && sb[sb.Length - 1] == '\n'))
                {
                    sb.Append("\r\n");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        ///  Returns the short string representation of the network
        /// </summary>
        /// <returns>The short string that represents the network</returns>
        public virtual string ToShortString()
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var process in Processes)
            {
                var hss = process as IToShortString;
                if (hss == null) continue;
                var ss = hss.ToShortString();
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append('|');
                }
                sb.Append(ss);
            }
            return sb.ToString();
        }

        #endregion
    }
}
