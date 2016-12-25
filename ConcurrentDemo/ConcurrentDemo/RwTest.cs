using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SysRwLock = System.Threading.ReaderWriterLock;
using MyRwLock = ThreadSync.ReaderWriterLock;


namespace ConcurrentDemo
{
    [TestClass]
    class RwTest
    {
        #region Fields

        private readonly ThreadPriority[] _threadPriority = new[]
            {
                ThreadPriority.Lowest,
                ThreadPriority.BelowNormal,
                ThreadPriority.Normal,
                ThreadPriority.AboveNormal,
                ThreadPriority.Highest
            };

        private readonly Random _rand = new Random(123);

        private readonly SysRwLock _sysRwLock = new SysRwLock();

        private readonly MyRwLock _myRwLock = new MyRwLock();

        private readonly MockRwResource _rwResource = new MockRwResource();

        #endregion

        #region Methods

        void SysRwLockSyncedThread(object o)
        {
            var iterations = (int)o;
            for (var i = 0; i < iterations; i++)
            {
                var m = _rand.Next(10);
                
                var sleep = _rand.Next(10) < 7;
                if (m < 6)
                {
                    // read
                    _sysRwLock.AcquireReaderLock(-1);
                    lock(this)
                    {
                        Console.Write("R");
                    }

                    if (sleep)
                    {
                        var t = _rand.Next(100);
                        _rwResource.ReadSleep(t);
                    }
                    else
                    {
                        var w = _rand.Next(1000000);
                        _rwResource.Read(w);    
                    }
                    
                    _sysRwLock.ReleaseReaderLock();
                }
                else
                {
                    // write
                    _sysRwLock.AcquireWriterLock(-1);
                    lock (this)
                    {
                        Console.Write("W");
                    }

                    if (sleep)
                    {
                        var t = _rand.Next(100);
                        _rwResource.WriteSleep(t);
                    }
                    else
                    {
                        var w = _rand.Next(1000000);
                        _rwResource.Write(w);    
                    }

                    _sysRwLock.ReleaseWriterLock();
                }
            }
        }

        void MyRwLockSyncedThread(object o)
        {
            var iterations = (int)o;
            for (var i = 0; i < iterations; i++)
            {
                var m = _rand.Next(10);

                var sleep = _rand.Next(10) < 7;
                if (m < 6)
                {
                    // read
                    _myRwLock.AcquireReaderLock();
                    lock (this)
                    {
                        Console.Write("R");
                    }

                    if (sleep)
                    {
                        var t = _rand.Next(100);
                        _rwResource.ReadSleep(t);
                    }
                    else
                    {
                        var w = _rand.Next(1000000);
                        _rwResource.Read(w);
                    }

                    _myRwLock.ReleaseReaderLock();
                }
                else
                {
                    // write
                    _myRwLock.AcquireWriterLock();
                    lock (this)
                    {
                        Console.Write("W");
                    }

                    if (sleep)
                    {
                        var t = _rand.Next(100);
                        _rwResource.WriteSleep(t);
                    }
                    else
                    {
                        var w = _rand.Next(1000000);
                        _rwResource.Write(w);
                    }

                    _myRwLock.ReleaseWriterLock();
                }
            }
        }


        [TestMethod]
        public void TestSysRwLock()
        {
            const int numThreads = 100;
            var threads = new Thread[numThreads];
            for (var i = 0; i < numThreads; i++)
            {
                var thread = threads[i] = new Thread(SysRwLockSyncedThread);
                var r = _rand.Next(_threadPriority.Length);
                thread.Priority = _threadPriority[r];
                thread.Start(100);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        [TestMethod]
        public void TestMyRwLock()
        {
            const int numThreads = 100;
            var threads = new Thread[numThreads];
            for (var i = 0; i < numThreads; i++)
            {
                var thread = threads[i] = new Thread(MyRwLockSyncedThread);
                var r = _rand.Next(_threadPriority.Length);
                thread.Priority = _threadPriority[r];
                thread.Start(100);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        #endregion
    }
}
