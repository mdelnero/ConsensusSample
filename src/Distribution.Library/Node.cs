
namespace Distribution.Library
{
    public class Node : RemoteClusterNode
    {
        public NodeHeader Header { get; private set; }

        public long RequestTickCount { get; set; }

        public long ResponseTickCount { get; set; }

        public Node(NodeHeader header)
        {    
            this.Header = header;
        }
    }
}
