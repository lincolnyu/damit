using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace GraphView
{
    /// <summary>
    ///  View model for links
    /// </summary>
    public class LinkViewModel : ShapeObjectViewModel
    {
        #region Fields

        /// <summary>
        ///  Backing field for X1
        /// </summary>
        private double _x1;

        /// <summary>
        ///  Backing field for Y1
        /// </summary>
        private double _y1;

        /// <summary>
        ///  Backing field for property X2
        /// </summary>
        private double _x2;

        /// <summary>
        ///  Backing field for property Y2
        /// </summary>
        private double _y2;

        /// <summary>
        ///  The first node
        /// </summary>
        private NodeViewModel _node1;

        /// <summary>
        ///  The second node
        /// </summary>
        private NodeViewModel _node2;

        /// <summary>
        ///  The backing field for Text
        /// </summary>
        private string _text;

        /// <summary>
        ///  The backing field for Arrow1Visibility
        /// </summary>
        private Visibility _arrow1Visibility;

        /// <summary>
        ///  The backing field for Arrow2Visibility
        /// </summary>
        private Visibility _arrow2Visibility;

        #endregion

        #region Properties

        /// <summary>
        ///  determines the value for the property X1 of the shaft in the target view
        /// </summary>
        /// <remarks>
        ///  Since all links are connected with two nodes both ends, their positions are totally
        ///  decided by the nodes, therefore these properties are not publicly settable, same as below
        /// </remarks>
        public double X1
        {
            get { return _x1; }
            private set
            {
                if (Math.Abs(_x1 - value) < double.Epsilon) return;
                _x1 = value;
                OnPropertyChanged("X1");
            }
        }

        /// <summary>
        ///  determines the value for the property X2 of the shaft in the target view
        /// </summary>
        public double X2
        {
            get { return _x2; }
            private set
            {
                if (Math.Abs(_x2 - value) < double.Epsilon) return;
                _x2 = value;
                OnPropertyChanged("X2");
            }
        }

        /// <summary>
        ///  determines the value for the property Y1 of the shaft in the target view
        /// </summary>
        public double Y1
        {
            get { return _y1; }
            private set
            {
                if (Math.Abs(_y1 - value) < double.Epsilon) return;
                _y1 = value;
                OnPropertyChanged("Y1");
            }
        }

        /// <summary>
        ///  determines the value for the property Y2 of the shaft in the target view
        /// </summary>
        public double Y2
        {
            get { return _y2; }
            private set
            {
                if (Math.Abs(_y2 - value) < double.Epsilon) return;
                _y2 = value;
                OnPropertyChanged("Y2");
            }
        }

        /// <summary>
        ///  The points of the polygon that draws the first arrow
        /// </summary>
        public PointCollection Arrow1Points { get; private set; }

        /// <summary>
        ///  The points of the polyline that draws the second arrow
        /// </summary>
        public PointCollection Arrow2Points { get; private set; }

        /// <summary>
        ///  The visibility of arrow 1
        /// </summary>
        public Visibility Arrow1Visibility
        {
            get { return _arrow1Visibility; }
            set
            {
                if (value == _arrow1Visibility) return;
                OnPropertyChanged("Arrow1Visibility");
                _arrow1Visibility = value;
            }
        }

        /// <summary>
        ///  The visibility of arrow 2
        /// </summary>
        public Visibility Arrow2Visibliity
        {
            get { return _arrow2Visibility; }
            set
            {
                if (value == _arrow2Visibility) return;
                OnPropertyChanged("Arrow2Visibility");
                _arrow2Visibility = value;
            }
        }

        /// <summary>
        ///  The first node the link links to
        /// </summary>
        public NodeViewModel Node1
        {
            get { return _node1; }
            set
            {
                if (value == _node1) return;
                if (_node1 != null) _node1.PropertyChanged -= NodePropertyChanged;
                _node1 = value;
                _node1.PropertyChanged += NodePropertyChanged;
                UpdateParametersDueToNode();
            }
        }

        /// <summary>
        ///  The second node the link links to
        /// </summary>
        public NodeViewModel Node2
        {
            get { return _node2; }
            set
            {
                if (value == _node2) return;
                if (_node2 != null) _node2.PropertyChanged -= NodePropertyChanged;
                _node2 = value;
                _node2.PropertyChanged += NodePropertyChanged;
                UpdateParametersDueToNode();
            }
        }

        /// <summary>
        ///  The text associated with the link
        /// </summary>
        public String Text
        {
            get { return _text; }
            set
            {
                if (value != _text) return;
                OnPropertyChanged("Text");
                _text = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates the view model and initialises it
        /// </summary>
        public LinkViewModel(INotifyModelChange model, ModelChangeHandler modelChangeHanlder)
            : base(model, modelChangeHanlder)
        {
            Arrow1Points = new PointCollection();
            Arrow2Points = new PointCollection();
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Updates all link properties according to node changes
        /// </summary>
        public void UpdateParametersDueToNode()
        {
            if (Node1 == null || Node2 == null) return;

            var node1X = Node1.X;
            var node1Y = Node1.Y;
            var node1R = Node1.R;
            var node2X = Node2.X;
            var node2Y = Node2.Y;
            var node2R = Node2.R;

            var dx21 = node2X - node1X;
            var dy21 = node2Y - node1Y;
            var dd21 = Math.Sqrt(dx21*dx21 + dy21*dy21);
            var x1 = node1X + node1R*dx21/dd21;
            var y1 = node1Y + node1R*dy21/dd21;

            var dx12 = -dx21;
            var dy12 = -dy21;
            var x2 = node2X + node2R*dx12/dd21;
            var y2 = node2Y + node2R*dy12/dd21;

            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;

            // arrow1
            // not implemented

            // arrow2
            // not implemented
        }

        private void NodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // current all properties will affect the link
            UpdateParametersDueToNode();
        }

        #endregion
    }
}
