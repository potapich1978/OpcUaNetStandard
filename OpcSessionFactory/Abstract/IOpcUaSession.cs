using Opc.Ua.Client;
using System;

namespace OpcSessionFactory.Abstract
{
    /// <remarks>
    /// Represents an abstraction of an OPC UA session 
    /// with automatic resource disposal support.
    /// </remarks>
    public interface IOpcUaSession: IDisposable
    {
        /// <remarks>
        /// Provides access to the underlying OPC UA session object.
        /// </remarks>
        ISession Session { get; }
    }
}
