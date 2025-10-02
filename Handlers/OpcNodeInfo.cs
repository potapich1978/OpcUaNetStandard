using Events;
using Opc.Ua;

namespace Handlers
{
    internal sealed class OpcNodeInfo : IOpcNodeInfo
    {
        public string DisplayName { get; private set; }
        public string FullName { get; private set; }
        public bool IsFolder { get; private set; }

        public OpcNodeInfo(ReferenceDescription description)
        {
            FullName = description.NodeId.ToString();
            DisplayName = description.DisplayName.ToString();
            IsFolder = description.NodeClass == NodeClass.Object;
        }
    }
}
