using System;
using System.Collections.Generic;

namespace Distribution.Library
{
    public class DiscoveryService
    {
        private NodeHeader header;

        private List<NodeConfigurationElement> nodes;

        public DiscoveryService(NodeHeader header, NodeConfigCollection addresses)
        {
            this.header = header;

            this.nodes = new List<NodeConfigurationElement>();

            for (int i = 0; i < addresses.Count; i++)
            {
                this.nodes.Add(addresses[i]);
            }
        }

        public NodeHeader FindClusterNode()
        {
            foreach (var item in nodes)
            {
                if (header.NodeAddress == item.Address)
                {
                    continue;
                }

                RemoteClusterNode conn = new RemoteClusterNode();

                try
                {
                    if (conn.Connect(item.Address))
                    {
                        NodeHeader result = conn.WhoIs();

                        return result;
                    }
                }
                catch (Exception)
                {
                }
            }

            return null;
        }
    }
}
