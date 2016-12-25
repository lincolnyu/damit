using System;
using System.Threading;

namespace ThreadSync
{
    /// <summary>
    ///  A semaphore implemented based on locks (mutexes) and events
    /// </summary>
    public class Semaphore
    {
        #region Fields

        /// <summary>
        ///  event that is fired when some access points may become available
        /// </summary>
        private readonly AutoResetEvent _eventAvailable = new AutoResetEvent(false);

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a semaphore with the specified resource initial availibility
        /// </summary>
        /// <param name="initialCount">The number of threads allowed to access initially</param>
        public Semaphore(int initialCount)
        {
            Available = initialCount;
            Waiting = 0;
        }

        #endregion

        #region Properties

        /// <summary>
        ///  Number of resources available for threads to access
        /// </summary>
        public int Available { get; protected set; }

        /// <summary>
        ///  Number of threads waiting
        /// </summary>
        public int Waiting { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        ///  Wait for resource access
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait or -1 to wait indefinitely</param>
        /// <returns>true if resource granted</returns>
        public bool WaitOne(int millisecondsTimeout)
        {
            var start = DateTime.Now;

            lock (this)
            {
                if (Available > 0)
                {
                    Available--;
                    return true;
                }
                Waiting++;
            }

            while (true)
            {
                // get a blocking event to stop further resource access before there is new availability
                if (millisecondsTimeout >= 0)
                {
                    var now = DateTime.Now;
                    var timeSpan = now - start;
                    if (timeSpan.Milliseconds > millisecondsTimeout)
                    {
                        return false;
                    }
                    var success = _eventAvailable.WaitOne(millisecondsTimeout - timeSpan.Milliseconds);
                    if (!success) return false;
                }
                else
                {
                    _eventAvailable.WaitOne();
                }
                
                lock (this)
                {
                    if (Available <= 0) continue;

                    Available--;
                    Waiting--;
                    if (Available > 0)
                    {
                        _eventAvailable.Set();
                    }
                    break;
                }
            }
            return true;
        }

        public void WaitOne()
        {
            lock (this)
            {
                if (Available > 0)
                {
                    Available--;
                    return;
                }
                Waiting++;
            }

            while (true)
            {
                // get a blocking event to stop further resource access before there is new availability
                var success = _eventAvailable.WaitOne();
                if (!success) return;
                lock (this)
                {
                    if (Available <= 0) continue;

                    Available--;
                    Waiting--;
                    if (Available > 0)
                    {
                        _eventAvailable.Set();
                    }
                    break;
                }
            }
        }

        /// <summary>
        ///  Release resource access
        /// </summary>
        /// <returns>Previous availability count</returns>
        public int Release()
        {
            lock (this)
            {
                var prevCount = Available;
                Available++;
                _eventAvailable.Set();
                return prevCount;
            }
        }

        #endregion
    }
}
