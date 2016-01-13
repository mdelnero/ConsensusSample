using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Distribution.Library
{
    public class RemoteClusterNode :
        IClusterNode,
        IDisposable
    {
        private IClusterNode channel;

        private ChannelFactory<IClusterNode> channelFactory;

        public NodeStatus Status { get; private set; }

        /// <summary>
        /// Class Constructor.</summary>
        public RemoteClusterNode()
        {
            Status = NodeStatus.Starting;
        }

        public bool Connect(string endpoint)
        {
            if (Status == NodeStatus.Alive)
                return true;

            try
            {
                string pipeName = "net.pipe://localhost/" + endpoint;

                pipeName = "net.tcp://" + endpoint + "/ServiceNode";

                NetTcpBinding binding = new NetTcpBinding();

                binding.ReceiveTimeout = TimeSpan.MaxValue;
                binding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
                binding.ReliableSession.Enabled = true;

                if (channelFactory == null)
                {
                    channelFactory = new ChannelFactory<IClusterNode>(
                        binding, ///new NetNamedPipeBinding(),
                        new EndpointAddress(pipeName));
                }

                channel = channelFactory.CreateChannel();

                ((IClientChannel)channel).Open(TimeSpan.FromMilliseconds(150));
                ((IClientChannel)channel).Faulted += new EventHandler(ClientChannel_Falted);
                ((IClientChannel)channel).Closed += new EventHandler(ClientChannel_Closed);

                Status = NodeStatus.Alive;
            }
            catch (Exception)
            {
                Status = NodeStatus.Error;
                return false;
            }

            return true;
        }

        private void ClientChannel_Closed(object sender, EventArgs e)
        {
            Status = NodeStatus.Dead;
        }

        private void ClientChannel_Falted(object sender, EventArgs e)
        {
            Status = NodeStatus.Error;
        }

        private void ChannelCheck()
        {
            if (Status != NodeStatus.Alive || ((IClientChannel)channel).State != CommunicationState.Opened)
            {
                throw new CommunicationException();
            }
        }

        public void Dispose()
        {
            if (channel != null)
            {
                IClientChannel cc = (IClientChannel)channel;
                cc.Faulted -= ClientChannel_Closed;
            }

            if (channelFactory != null)
            {
                channelFactory.Close();
            }
        }

        public NodeHeader WhoIs()
        {
            ChannelCheck();

            return channel.WhoIs();
        }

        public ClusterWelcomeMessage RegisterNode(NodeHeader node)
        {
            ChannelCheck();

            return channel.RegisterNode(node);
        }

        public void ElectionRequest(NodeHeader node)
        {
            ChannelCheck();

            channel.ElectionRequest(node);
        }

        public void ElectionResult(NodeHeader node)
        {
            ChannelCheck();

            channel.ElectionResult(node);
        }

        public void Heartbeat()
        {
            ChannelCheck();

            channel.Heartbeat();
        }

        public void ElectionResponse(NodeHeader node)
        {
            ChannelCheck();

            channel.ElectionResponse(node);
        }
    }
}
