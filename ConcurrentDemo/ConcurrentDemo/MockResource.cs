using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrentDemo
{
    class MockResource
    {
        #region Fields

        private readonly int _maximumAccess;
        private int _currentVisitors;

        #endregion

        #region Constructors

        public MockResource(int maximumAccess)
        {
            _maximumAccess = maximumAccess;
            _currentVisitors = 0;
        }

        #endregion

        #region Properties

        public int NumAvailable
        {
            get { return _maximumAccess - _currentVisitors; }
        }

        #endregion

        #region Methods

        public void Get()
        {
            lock (this)
            {
                _currentVisitors++;
                Assert.IsTrue(_currentVisitors <= _maximumAccess);
            }
        }

        public void Release()
        {
            lock (this)
            {
                Assert.IsTrue(_currentVisitors > 0);
                _currentVisitors--;
            }
        }

        #endregion
    }
}
