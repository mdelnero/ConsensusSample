using System.Collections.Generic;
using System.ServiceModel;

namespace Distribution.Library
{ 
    [ServiceContract]
    public interface IClusterNode
    {
        [OperationContract]
        NodeHeader WhoIs();

        [OperationContract]
        ClusterWelcomeMessage RegisterNode(NodeHeader node);

        [OperationContract]
        void ElectionRequest(NodeHeader node);

        [OperationContract]
        void ElectionResponse(NodeHeader node);

        [OperationContract]
        void ElectionResult(NodeHeader node);

        [OperationContract]
        void Heartbeat();
    }
}
