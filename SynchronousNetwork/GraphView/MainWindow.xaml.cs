namespace GraphView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly GraphViewModel _gvm;

        public MainWindow()
        {
            InitializeComponent();

            MainCanvas.DataContext = _gvm = new GraphViewModel();
        }
    }
}
