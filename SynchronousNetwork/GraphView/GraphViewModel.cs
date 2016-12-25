using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace GraphView
{
    /// <summary>
    ///  The view model for the entire graph on the canvas
    /// </summary>
    public class GraphViewModel
    {
        #region Fields

        /// <summary>
        ///  The backing field for NodeRadius
        /// </summary>
        private double _nodeRadius;

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates and initialises a view model
        /// </summary>
        public GraphViewModel(double nodeRadius = 10)
        {
            ShapeObjects = new ObservableCollection<ShapeObjectViewModel>();
            _nodeRadius = nodeRadius;
        }

        #endregion

        #region Properties

        /// <summary>
        ///  All shape objects to be displayed on the canvas
        /// </summary>
        public ObservableCollection<ShapeObjectViewModel> ShapeObjects { get; private set; }

        /// <summary>
        ///  The radius of all nodes
        /// </summary>
        public double NodeRadius
        {
            get { return _nodeRadius; }
            set
            {
                if (Math.Abs(value - _nodeRadius) < double.Epsilon) return;
                _nodeRadius = value;
                foreach (var node in ShapeObjects.OfType<NodeViewModel>())
                {
                    node.R = _nodeRadius;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Adds a node
        /// </summary>
        /// <param name="node">The node to add</param>
        public void AddNode(NodeViewModel node)
        {
            ShapeObjects.Add(node);
        }

        /// <summary>
        ///  Adds a link
        /// </summary>
        /// <param name="link">The link to add</param>
        public void AddLink(LinkViewModel link)
        {
            ShapeObjects.Add(link);
        }

        #endregion
    }
}
