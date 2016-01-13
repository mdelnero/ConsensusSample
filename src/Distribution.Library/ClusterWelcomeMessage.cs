using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Distribution.Library
{
    [DataContract]
    public class ClusterWelcomeMessage
    {
        [DataMember]
        public NodeHeader NodeID { get; set; }

        [DataMember]
        public List<NodeHeader> NodeList { get; set; }
    }
}
