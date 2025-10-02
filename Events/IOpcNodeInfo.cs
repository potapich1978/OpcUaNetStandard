using System;
using System.Collections.Generic;
using System.Text;

namespace Events
{
    /// <summary>
    /// represent information about OPC node
    /// </summary>
    public interface IOpcNodeInfo
    {
        /// <summary>
        /// short name of OPC node. use it for caption in UI
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// full qulified name of OPC node. Use it for subscribe on change value
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// node is folder
        /// </summary>
        bool IsFolder { get; }
    }
}
