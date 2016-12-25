using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using MySema = ThreadSync.Semaphore;
using SysSema = System.Threading.Semaphore;

namespace ConcurrentDemo
{
    [TestClass]
    class SemaTest
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

        private const int MaxAccess = 10;

        private readonly MySema _mySema = new MySema(MaxAccess);

        private readonly SysSema _sysSema = new SysSema(MaxAccess, MaxAccess);

        private readonly MockResource _mockResource = new MockResource(MaxAccess);

        #endregion

        private void MySemaSyncedThread(object o)
        {
            var iterations = (int) o;
            for (var i = 0; i < iterations; i++)
            {
                _mySema.WaitOne();

                Console.Write("G");

                _mockResource.Get();

                var waitType = _rand.Next(10);
                if (waitType < 7)
                {
                    var numIters = _rand.Next(1000);
                    Thread.SpinWait(numIters);
                }
                else
                {
                    var sleepTime = _rand.Next(1000);
                    Thread.Sleep(sleepTime);
                }

                _mockResource.Release();

                Console.Write("R");

                _mySema.Release();
            }
        }

        private void SysSemaSyncedThread(object o)
        {
            var iterations = (int)o;
            for (var i = 0; i < iterations; i++)
            {
                _sysSema.WaitOne();

                Console.Write("G");

                _mockResource.Get();

                var waitType = _rand.Next(10);
                if (waitType < 7)
                {
                    var numIters = _rand.Next(1000);
                    Thread.SpinWait(numIters);
                }
                else
                {
                    var sleepTime = _rand.Next(1000);
                    Thread.Sleep(sleepTime);
                }

                _mockResource.Release();

                Console.Write("R");

                _sysSema.Release();
            }
        }

        [TestMethod]
        public void TestMySema()
        {
            const int numThreads = 100;
            var threads = new Thread[numThreads];
            for (var i = 0; i < numThreads; i++)
            {
                var thread = threads[i] = new Thread(MySemaSyncedThread);
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
        public void TestSysSema()
        {
            const int numThreads = 100;
            var threads = new Thread[numThreads];
            for (var i = 0; i < numThreads; i++)
            {
                var thread = threads[i] = new Thread(SysSemaSyncedThread);
                var r = _rand.Next(_threadPriority.Length);
                thread.Priority = _threadPriority[r];
                thread.Start(100);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
    }
}
