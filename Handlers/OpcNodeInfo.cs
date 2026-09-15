using Events;
using Opc.Ua;
using System;

namespace Handlers
{
    internal sealed class OpcNodeInfo : IOpcNodeInfo
    {
        public string DisplayName { get; private set; }
        public string FullName { get; private set; }
        public bool IsFolder { get; private set; }
        public bool Writable {  get; private set; }
        public Type DataType { get; private set; }

        public OpcNodeInfo(ReferenceDescription description, Type dataType)
        {
            FullName = description.NodeId.ToString();
            DisplayName = description.DisplayName.ToString();
            IsFolder = description.NodeClass == NodeClass.Object;
            Writable = description.NodeClass == NodeClass.Variable;
            DataType = dataType;
        }
    }
}
