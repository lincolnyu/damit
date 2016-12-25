using System.Threading;

namespace ConcurrentDemo
{
    class Program
    {
        static void Main()
        {
            //var semaTest = new SemaTest();
            //semaTest.TestMySema();
            //semaTest.TestSysSema();

            var rwTest = new RwTest();
            //rwTest.TestSysRwLock();
            rwTest.TestMyRwLock();
        }
    }
}
