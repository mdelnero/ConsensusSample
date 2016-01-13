using Distribution.Library;
using System;
using System.Configuration;
using System.Diagnostics;

namespace Distribution.NetworkNode
{
    internal static class ProgramSettings
    {
        public static ClusterConfigurationSection Cluster;
    }

    class Program
    {
        private static ServiceHostHelper wcfServiceHost = new ServiceHostHelper();

        static void Main(string[] args)
        {
            ProgramSettings.Cluster = (ClusterConfigurationSection)ConfigurationManager.GetSection("ClusterSettings/Cluster");

            try
            {
                NodeHeader nodeId = new NodeHeader(Process.GetCurrentProcess().Id, ProgramSettings.Cluster.Endpoint);

                ServiceNode instance = new ServiceNode(nodeId);

                wcfServiceHost.StartPipeHost(instance, nodeId.NodeAddress);

                instance.JoinCluster();
            }
            catch (Exception)
            {
                throw;
            }

            Console.Read();
        }
    }
}



