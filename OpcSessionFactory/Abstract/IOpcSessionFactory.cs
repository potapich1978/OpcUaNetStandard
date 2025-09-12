using System.Threading.Tasks;

namespace OpcSessionFactory.Abstract
{
    /// <remarks>
    /// Provides a contract for creating and retrieving OPC UA sessions 
    /// for specific applications and endpoints.
    /// </remarks>
    public interface IOpcSessionFactory
    {
        /// <remarks>
        /// Retrieves or creates a new OPC UA session for the given 
        /// application identifier and server endpoint.
        /// </remarks>
        Task<IOpcUaSession> GetSession(ISessionParams sessionParams);
    }
}
