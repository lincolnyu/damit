using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GraphView
{
    public class NodeViewModel : ShapeObjectViewModel
    {
        #region Fields

        /// <summary>
        ///  The X component of the location of the node
        /// </summary>
        private double _x;

        /// <summary>
        ///  The Y component of the location of the node
        /// </summary>
        private double _y;

        /// <summary>
        ///  The radius of the circle to display that represents node 
        /// </summary>
        private double _r;

        /// <summary>
        ///  The backing field for the property of Text
        /// </summary>
        private string _text;

        #endregion

        #region Properties
        
        /// <summary>
        ///  The left side of the ellipse that represents the node
        /// </summary>
        public double Left
        {
            get { return _x - _r; }
        }

        /// <summary>
        ///  The right side of the ellipse that represents the node
        /// </summary>
        public double Width
        {
            get { return _r*2; }
        }

        /// <summary>
        ///  The top of the ellipse that represents the node
        /// </summary>
        public double Top
        {
            get { return _y - _r; }
        }

        /// <summary>
        ///  The bottom of the ellipse that represents teh node
        /// </summary>
        public double Height
        {
            get { return _r*2; }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (value == _text) return;
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        /// <summary>
        ///  The X component of the circle that represents the node
        /// </summary>
        public double X
        {
            get { return _x; }
            set 
            { 
                if (Math.Abs(value - _x) < double.Epsilon) return;
                _x = value;
                OnPropertyChanged("Left");
                OnPropertyChanged("Width");
            }
        }

        /// <summary>
        ///  The Y component of the circle that represents the node 
        /// </summary>
        public double Y
        {
            get { return _y; }
            set
            {
                if (Math.Abs(value - _y) < double.Epsilon) return;
                _y = value;
                OnPropertyChanged("Top");
                OnPropertyChanged("Height");
            }
        }

        /// <summary>
        ///  The radius of the circle that represents the node
        /// </summary>
        public double R
        {
            get { return _r; }
            set
            {
                if (Math.Abs(value - _r) < double.Epsilon) return;
                _r = value;
                OnPropertyChanged("Left");
                OnPropertyChanged("Width");
                OnPropertyChanged("Top");
                OnPropertyChanged("Height");
            }
        }

        /// <summary>
        ///  A collection of links this node links to
        /// </summary>
        public ObservableCollection<LinkViewModel> Links { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a nwe NodeViewModel
        /// </summary>
        public NodeViewModel(INotifyModelChange model, ModelChangeHandler modelChangeHanlder)
            : base(model, modelChangeHanlder)
        {
            Links = new ObservableCollection<LinkViewModel>();
        }

        #endregion
    }
}
