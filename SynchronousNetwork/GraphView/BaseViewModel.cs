using System.ComponentModel;

namespace GraphView
{
    /// <summary>
    ///  Base class for view models
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        #region Delegates

        public delegate void ModelChangeHandler(object model, BaseViewModel viewModel, ModelChangeEventArgs e);

        #endregion

        #region Events

        /// <summary>
        ///  The event that's fired when bound property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        protected BaseViewModel(INotifyModelChange modelChange, ModelChangeHandler modelChangeHanlder)
        {
            modelChange.OnModelChange += (sender, args) => modelChangeHanlder(modelChange, this, args);
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Invoked when certain bound property has changed
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
