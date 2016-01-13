using Distribution.Library;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Distribution.NetworkNode
{
    public interface INodePoolObserver
    {
        void OnNewLeader(NodeHeader node);

        void OnNodeAdded(NodeHeader node);

        void OnNodeRemoved(NodeHeader node);
    }

    public class NodeList : List<Node>
    {
        private INodePoolObserver observer;

        public Node Leader
        {
            get
            {
                return this.Find(x => x.Header.State == NodeState.Leader);
            }
        }        

        public NodeList(INodePoolObserver observer)
        {
            this.observer = observer;
        }

        public List<NodeHeader> ToHeaderList()
        {
            List<NodeHeader> result;

            result = this.Select(x => x.Header).ToList();

            return result;
        }

        public Node GetNode(NodeHeader node)
        {
            return this.Find(x => x.Header.ID == node.ID);
        }  

        public void SetLeader(NodeHeader node)
        {
            Node currentLeader = Leader as Node;

            if (currentLeader != null)
            {
                if (currentLeader.Header.ID == node.ID)
                {
                    return;
                }

                currentLeader.Header.State = NodeState.Follower;
            }

            Node newLeader = this.Find(x => x.Header.ID == node.ID);

            if (newLeader == null)
            {
                throw new Exception("Tragédia!");
            }

            newLeader.Header.State = NodeState.Leader;
            observer.OnNewLeader(newLeader.Header);
        }

        public void PurgeNodes()
        {
            List<Node> deadNodes = new List<Node>();

            lock (this)
            {
                foreach (var item in this)
                {
                    if (item.Status == NodeStatus.Dead || item.Status == NodeStatus.Error)
                    {
                        deadNodes.Add(item);
                    }
                }

                if (deadNodes.Count > 0)
                {
                    foreach (var item in deadNodes)
                    {
                        this.Remove(item);
                        observer.OnNodeRemoved(item.Header);
                    }
                }
            }
        }
    }
}
