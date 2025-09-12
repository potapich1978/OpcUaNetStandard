using Opc.Ua.Client;
using System.Threading.Tasks;

namespace OpcSessions.Abstract
{
    /// <remarks>
    /// Provides an abstraction for managing multiple OPC UA sessions 
    /// identified by application IDs.
    /// </remarks>
    public interface IOpcSessionsManager
    {
        /// <remarks>
        /// Creates and registers a new OPC UA session for the specified application 
        /// and endpoint URL. If a session with the same <paramref name="appId"/> 
        /// already exists, it is not replaced.
        /// </remarks>
        Task AddSession(ISessionChannelParams sessionParams);

        /// <remarks>
        /// Retrieves an active OPC UA session for the given application identifier. 
        /// Returns <c>null</c> if no session is found.
        /// </remarks>
        ISession GetSession(string appId);

        /// <remarks>
        /// Removes and disposes of an existing OPC UA session associated with 
        /// the specified application identifier.
        /// </remarks>
        void RemoveSession(string appId);
        
    }
}
