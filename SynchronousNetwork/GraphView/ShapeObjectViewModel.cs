namespace GraphView
{
    public abstract class ShapeObjectViewModel : BaseViewModel
    {
        #region Properties

        protected object Model { get; private set; }

        #endregion

        #region Constructors

        protected ShapeObjectViewModel(INotifyModelChange modelChange, 
            ModelChangeHandler modelChangeHanlder)
            : base(modelChange, modelChangeHanlder)
        {
            Model = modelChange;
        }

        #endregion
    }
}
