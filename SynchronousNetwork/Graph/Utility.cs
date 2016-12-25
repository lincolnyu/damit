using System;

namespace Graph
{
    public static class Utility
    {
        #region Delegates

        public delegate bool IgnoreNode(dynamic nodeToCheck);

        #endregion

        #region Static methods

        public static void DisplayNetwork(dynamic network, IgnoreNode ignoreNode)
        {
            Console.WriteLine("Network ...");
            foreach (var proc in network.Processes)
            {
                if (ignoreNode(proc)) continue;
                dynamic d = proc.State;
                Console.WriteLine(" Process {0}", d.Own);
            }

            for (var i = 0; i < network.Links.Count; i += 2)
            {
                var link = network.Links[i];
                var weightedLink = link as WeightedLink;
                if (weightedLink == null)
                {
                    continue;
                }
                var sUs = weightedLink.UpstreamProcess.State;
                var sDs = weightedLink.DownstreamProcess.State;
                dynamic dUs = sUs;
                dynamic dDs = sDs;
                Console.WriteLine(" Link: {0} <-- {1:0.000} --> {2}", dUs.Own, 
                    weightedLink.Weight, dDs.Own);
            }
        }

        #endregion
    }
}
