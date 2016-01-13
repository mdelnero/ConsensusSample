using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Distribution.Library
{
    [DataContract]
    public class NodeHeader
    {
        [DataMember]
        public string ID { get; private set; }

        [DataMember]
        public NodeState State { get; set; }

        [DataMember]
        public long ProcessID { get; set; }

        [DataMember]
        public string NodeAddress { get; set; }

        public NodeHeader(long processID, string nodeAddress)
        {
            this.ID = Guid.NewGuid().ToString();
            this.State = NodeState.Follower;

            this.ProcessID = processID;
            this.NodeAddress = nodeAddress;
        }

        public override string ToString()
        {
            string format = "ID: {0}\nProcess ID: {1}\nAddress: {2}";

            return string.Format(
                format,
                ID,
                ProcessID,
                NodeAddress);
        }
    }
}
