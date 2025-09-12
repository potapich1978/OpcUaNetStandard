using System;
using System.Threading.Tasks;

namespace OpcCallbacks.Abstract
{
    /// <summary>
    /// Provides thread-safe methods for counting callbacks associated with OPC UA monitored items.
    /// </summary>
    internal interface IOpcTagCallbacksCount
    {
        /// <summary>
        /// Increments the callback count for the specified tag identifier.
        /// </summary>
        /// <param name="tagId">The tag identifier to register a callback for.</param>
        void RegisterCallback(string tagId);

        /// <summary>
        /// Decrements the callback count for the specified tag identifier.
        /// </summary>
        /// <param name="tagId">The tag identifier to unregister a callback from.</param>
        /// <returns>The remaining number of callbacks registered for the tag after decrementing.</returns>
        int UnregisterCallback(string tagId);

        /// <summary>
        /// Gets the current number of registered callbacks for the specified tag identifier.
        /// </summary>
        /// <param name="tagId">The tag identifier to get the callback count for.</param>
        /// <returns>The number of callbacks currently registered for the tag.</returns>
        int CallbacksCount(string tagId);
    }
}
