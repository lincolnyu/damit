namespace GraphView
{
    /// <summary>
    ///  The interface for model entity that notifies its view model on change
    /// </summary>
    public interface INotifyModelChange
    {
        event NotifyModelChange OnModelChange;
    }
}
