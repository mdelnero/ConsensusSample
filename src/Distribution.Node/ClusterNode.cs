using Distribution.Library;
using System;
using System.ServiceModel;
using System.Threading;

namespace Distribution.NetworkNode
{
    public class ClusterNode : IClusterNode, INodePoolObserver
    {
        private object syncLock = new object();

        private NodeList nodeList;

        private Node leaderNode = null;

        public NodeHeader Header { get; private set; }

        public ClusterNode(NodeHeader header)
        {
            this.Header = header;
            nodeList = new NodeList(this);

            Console.WriteLine(header);
            Console.WriteLine();
        }

        public NodeHeader WhoIs()
        {
            return Header;
        }

        public ClusterWelcomeMessage RegisterNode(NodeHeader node)
        {
            AddNode(node);

            ClusterWelcomeMessage message = new ClusterWelcomeMessage();

            message.NodeID = Header;
            message.NodeList = nodeList.ToHeaderList();

            return message;
        }

        public void ElectionRequest(NodeHeader node)
        {
            Node caller = nodeList.GetNode(node);

            if (caller != null)
            {
                caller.ElectionResponse(Header);
            }

            lock (syncLock)
            {
                if (Header.State == NodeState.Follower)
                {
                    BroadcastElection();
                }
            }
        }

        public void ElectionResponse(NodeHeader node)
        {
            Node caller = nodeList.GetNode(node);

            if (caller != null)
            {
                caller.ResponseTickCount = Environment.TickCount;
            }
        }

        public void ElectionResult(NodeHeader node)
        {
            lock (syncLock)
            {
                Header.State = NodeState.Follower;

                nodeList.SetLeader(node);

                leaderNode = nodeList.Leader;
            }
        }

        public void Heartbeat()
        {
        }

        public void JoinCluster()
        {
            DiscoveryService discovery = new DiscoveryService(Header, ProgramSettings.Cluster.Nodes);

            NodeHeader randomNode = discovery.FindClusterNode();

            if (randomNode == null)
            {
                Header.State = NodeState.Leader;
                OnNewLeader(Header);
            }
            else
            {
                JoinCluster(randomNode);
            }

            Thread threadInfo = new Thread(WorkerThread);
            threadInfo.Name = "WorkerThread";
            threadInfo.IsBackground = true;
            threadInfo.Start();
        }

        private void JoinCluster(NodeHeader networkNode)
        {
            Node node = AddNode(networkNode);

            if (node.Status == NodeStatus.Alive)
            {
                ClusterWelcomeMessage welcome = node.RegisterNode(Header);

                foreach (var item in welcome.NodeList)
                {
                    if (item.ID == Header.ID)
                    {
                        continue;
                    }

                    Node hop = AddNode(item);

                    if (hop.Status == NodeStatus.Alive)
                    {
                        hop.RegisterNode(Header);
                    }
                }
            }

            leaderNode = nodeList.Leader;
        }

        private void BroadcastElection()
        {
            Header.State = NodeState.Candidate;

            foreach (var item in nodeList)
            {
                try
                {
                    if (item.Status == NodeStatus.Alive && item.Header.ProcessID > Header.ProcessID)
                    {
                        item.ElectionRequest(Header);
                        item.RequestTickCount = Environment.TickCount;
                    }
                }
                catch (Exception)
                {
                }
            }

            nodeList.PurgeNodes();
        }

        private void BroadcastElectionResult()
        {
            foreach (var item in nodeList)
            {
                try
                {
                    if (item.Status == NodeStatus.Alive)
                    {
                        item.ElectionResult(Header);
                    }
                }
                catch (Exception)
                {
                }
            }

            nodeList.PurgeNodes();
        }

        private void FolowerTask()
        {
            try
            {
                leaderNode.Heartbeat();
            }
            catch (Exception)
            {
                leaderNode = null;
                BroadcastElection();
            }
        }

        private void CandidateTask()
        {
            foreach (var item in nodeList)
            {
                try
                {
                    if (item.Status == NodeStatus.Alive && item.Header.ProcessID > Header.ProcessID)
                    {
                        if ((item.ResponseTickCount - item.RequestTickCount) < 300)
                        {
                            Header.State = NodeState.Listener;
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            if (Header.State == NodeState.Candidate)
            {
                Header.State = NodeState.Leader;
                OnNewLeader(Header);
                BroadcastElectionResult();
            }
        }

        private void PurgeTask()
        {
            foreach (var item in nodeList)
            {
                try
                {
                    if (item.Status == NodeStatus.Alive)
                    {
                        item.Heartbeat();
                    }
                }
                catch (Exception)
                {
                }
            }

            nodeList.PurgeNodes();
        }

        private void WorkerThread()
        {
            AutoResetEvent autoEvent = new AutoResetEvent(false);

            try
            {
                while (true)
                {
                    autoEvent.WaitOne(300, true);

                    lock (syncLock)
                    {
                        if (Header.State == NodeState.Follower)
                        {
                            FolowerTask();
                        }
                        else if (Header.State == NodeState.Candidate)
                        {
                            CandidateTask();
                        }
                    }

                    PurgeTask();
                }
            }
            catch (Exception)
            {
                throw new Exception("Falha catastrofica!");
            }
        }

        public Node AddNode(NodeHeader nodeHeader)
        {
            Node newNode = null;

            try
            {
                newNode = nodeList.Find(x => x.Header.ID == nodeHeader.ID);

                if (newNode != null)
                {
                    return newNode;
                }

                newNode = new Node(nodeHeader);
                nodeList.Add(newNode);

                newNode.Connect(nodeHeader.NodeAddress);

                OnNodeAdded(newNode.Header);
            }
            catch (Exception)
            {

            }

            return newNode;
        }

        public void OnNewLeader(NodeHeader node)
        {
            if (node.ID == Header.ID)
            {
                ConsoleColor c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("**** LEADER ****");
                Console.ForegroundColor = c;
            }
            else
            {
                Console.WriteLine("NOVO LEADER: " + node.NodeAddress);
            }
        }

        public void OnNodeAdded(NodeHeader node)
        {
            Console.WriteLine("Added: " + node.NodeAddress);
        }

        public void OnNodeRemoved(NodeHeader node)
        {
            Console.WriteLine("Removed: " + node.NodeAddress);
        }
    }

    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single,
        IncludeExceptionDetailInFaults = true,
        AutomaticSessionShutdown = true)]
    public class ServiceNode : ClusterNode
    {
        public ServiceNode(NodeHeader nodeID)
            : base(nodeID)
        {
        }
    }
}
