using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrentDemo
{
    class MockRwResource
    {
        #region Fields

        private int _readerCount;
        private bool _written;

        #endregion

        #region Methods

        public void Read(int ticks)
        {
            lock (this)
            {
                Assert.IsFalse(_written);
                ++_readerCount;
            }
            Thread.SpinWait(ticks);
            lock(this)
            {
                --_readerCount;
            }
        }

        public void ReadSleep(int milliseconds)
        {
            lock (this)
            {
                Assert.IsFalse(_written);
                ++_readerCount;
            }
            Thread.Sleep(milliseconds);
            lock (this)
            {
                --_readerCount;
            }
        }

        public void Write(int ticks)
        {
            lock (this)
            {
                Assert.IsFalse(_written);
                Assert.IsTrue(_readerCount==0);
                _written = true;
            }
            Thread.SpinWait(ticks);
            lock(this)
            {
                _written = false;
            }
        }

        public void WriteSleep(int milliseconds)
        {
            lock (this)
            {
                Assert.IsFalse(_written);
                Assert.IsTrue(_readerCount == 0);
                _written = true;
            }
            Thread.Sleep(milliseconds);
            lock (this)
            {
                _written = false;
            }
        }

        #endregion
    }
}
