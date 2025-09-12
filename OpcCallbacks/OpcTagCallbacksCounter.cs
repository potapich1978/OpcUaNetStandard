using OpcCallbacks.Abstract;
using System.Collections.Generic;

namespace OpcCallbacks
{
    /// <summary>
    /// Thread-safe implementation of <see cref="IOpcTagCallbacksCount"/> for tracking callback
    /// registrations per OPC UA tag using a dictionary with synchronization.
    /// </summary>
    internal sealed class OpcTagCallbacksCounter : IOpcTagCallbacksCount
    {
        /// <summary>
        /// Dictionary storing callback counts per tag identifier.
        /// </summary>
        private readonly Dictionary<string, int> _tagsCallbacksCount;

        /// <summary>
        /// Synchronization object for thread-safe access to the callback count dictionary.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpcTagCallbacksCounter"/> class.
        /// </summary>
        public OpcTagCallbacksCounter()
        {
            _tagsCallbacksCount = new Dictionary<string, int>();
        }

        /// <summary>
        /// Increments the callback count for the specified tag identifier in a thread-safe manner.
        /// </summary>
        /// <param name="tagId">The tag identifier to register a callback for.</param>
        public void RegisterCallback(string tagId)
        {

            lock (_lock)
            {
                if (_tagsCallbacksCount.TryGetValue(tagId, out int count))
                {
                    _tagsCallbacksCount[tagId] = count + 1;
                }
                else
                {
                    _tagsCallbacksCount.Add(tagId, 1);
                }
            }
        }

        /// <summary>
        /// Decrements the callback count for the specified tag identifier in a thread-safe manner.
        /// Removes the tag entry from the dictionary when the count reaches zero.
        /// </summary>
        /// <param name="tagId">The tag identifier to unregister a callback from.</param>
        /// <returns>The remaining number of callbacks registered for the tag after decrementing.</returns>
        public int UnregisterCallback(string tagId)
        {
            lock (_lock)
            {
                if (_tagsCallbacksCount.TryGetValue(tagId, out int count) && count > 0)
                {
                    count--;
                    if (count == 0)
                    {
                        _tagsCallbacksCount.Remove(tagId);
                    }
                    else
                    {
                        _tagsCallbacksCount[tagId] = count;
                    }
                    return count;
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets the current number of registered callbacks for the specified tag identifier.
        /// </summary>
        /// <param name="tagId">The tag identifier to get the callback count for.</param>
        /// <returns>The number of callbacks currently registered for the tag.</returns>
        public int CallbacksCount(string tagId)
        {
            lock (_lock)
            {
                if( _tagsCallbacksCount.TryGetValue(tagId,out int count))
                    return count;

                return 0;
            }
        }
    }
}
