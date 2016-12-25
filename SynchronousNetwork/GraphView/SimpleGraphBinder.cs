using System.Collections.Generic;
using Ch3_2;
using SynchronousNetwork;
using SyncLink = SynchronousNetwork.Link;

namespace GraphView
{
    class SimpleGraphBinder
    {

        private class LinkWrapper : INotifyModelChange
        {
            #region Events

            public event NotifyModelChange OnModelChange;

            #endregion

            #region Properties

            private SyncLink Link { get; set; }

            #endregion

            #region Constructors

            public LinkWrapper(SyncLink link)
            {
                Link = link;
            }

            #endregion
        }


        private class NodeWrapper : INotifyModelChange
        {
            #region Events

            public event NotifyModelChange OnModelChange;

            #endregion

            #region Properties

            private Process Process { get; set; }

            #endregion

            #region Constructors

            public NodeWrapper(Process process)
            {
                Process = process;
            }

            #endregion
        }

        private readonly Network _network;

        private readonly GraphViewModel _graphViewModel;

        public SimpleGraphBinder(Network network, GraphViewModel graphViewModel)
        {
            _network = network;
            _graphViewModel = graphViewModel;
            Bind();
        }

        void Bind()
        {
            var nodes = _network.Processes;
            var links = _network.Links;
            var nodeToNodeViewModel = new Dictionary<Process, NodeViewModel>();

            foreach (var node in nodes)
            {
                var nodeViewModel = new NodeViewModel(new NodeWrapper(node), NodeChangeHandler);
                _graphViewModel.AddNode(nodeViewModel);
                nodeToNodeViewModel[node] = nodeViewModel;
            }

            foreach (var link in links)
            {
                var node1 = link.UpstreamProcess;
                var node2 = link.DownstreamProcess;
                var node1ViewModel = nodeToNodeViewModel[node1];
                var node2ViewModel = nodeToNodeViewModel[node2];
                var linkViewModel = new LinkViewModel(new LinkWrapper(link), LinkChangeHandler)
                    {
                        Node1 = node1ViewModel,
                        Node2 = node2ViewModel
                    };
                _graphViewModel.AddLink(linkViewModel);
            }
        }

        private void NodeChangeHandler(object model, BaseViewModel viewmodel, ModelChangeEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void LinkChangeHandler(object model, BaseViewModel viewmodel, ModelChangeEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
