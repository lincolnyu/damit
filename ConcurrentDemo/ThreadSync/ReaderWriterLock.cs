using System;
using System.Threading;

namespace ThreadSync
{
    /// <summary>
    ///  A reader writer lock based on locks(mutexes) and events
    /// </summary>
    /// <remarks>
    ///  The lock ensures that
    ///  1. when a thread is writing, no other thread can access
    ///  2. threads can read together, but no other thread can write
    /// </remarks>
    public class ReaderWriterLock
    {
        #region Fields

        /// <summary>
        ///  The number of readers currently reading the resource (currently being in reading mode)
        /// </summary>
        private int _readerCount;

        /// <summary>
        ///  Currently is in writing mode (being written by a thread)
        /// </summary>
        private bool _beingWritten;

        /// <summary>
        ///  An event that's fired when it turns to be available for reading or writing
        /// </summary>
        private readonly AutoResetEvent _eventAvailable = new AutoResetEvent(false);

        #endregion

        #region Methods

        /// <summary>
        ///  Attempts to get a reader lock and gets blocked before it's available or timed out
        /// </summary>
        /// <param name="millisecondsTimeout">The maximum milliseconds to wait or minus one to wait indefinitely</param>
        /// <returns>True if the reader lock is successfully acquired</returns>
        public bool AcquireReaderLock(int millisecondsTimeout = -1)
        {
            var start = DateTime.Now;

            lock (this)
            {
                if (!_beingWritten)
                {
                    _readerCount++;
                    return true;
                }
            }
            while (true)
            {
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
                    if (_beingWritten) continue;
                    _readerCount++;
                }
                return true;
            }
        }

        /// <summary>
        ///  Attempts to get a writer lock and gets blocked before it's available or timed out
        /// </summary>
        /// <param name="millisecondsTimeout">The maximum milliseconds to wait or minus one to wait indefinitely</param>
        /// <returns>True if the reader lock is successfully acquired</returns>
        public bool AcquireWriterLock(int millisecondsTimeout = -1)
        {
            var start = DateTime.Now;

            lock (this)
            {
                if (_readerCount == 0 && !_beingWritten)
                {
                    _beingWritten = true;
                    return true;
                }
            }
            while (true)
            {
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
                    if (_readerCount > 0 || _beingWritten) continue;
                    _beingWritten = true;
                }
                return true;
            }
        }

        /// <summary>
        ///  Release a reader lock
        /// </summary>
        public void ReleaseReaderLock()
        {
            lock (this)
            {
                if (_readerCount <= 0) return;
                System.Diagnostics.Trace.Assert(!_beingWritten);
                _readerCount--;
                if (_readerCount == 0)
                {
                    _eventAvailable.Set();
                }
            }
        }

        /// <summary>
        ///  Release a writer lock
        /// </summary>
        public void ReleaseWriterLock()
        {
            lock (this)
            {
                if (!_beingWritten) return;
                System.Diagnostics.Trace.Assert(_readerCount == 0);
                _beingWritten = false;
                _eventAvailable.Set();
            }
        }

        #endregion
    }
}
