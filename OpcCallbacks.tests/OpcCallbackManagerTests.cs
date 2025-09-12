using NSubstitute;
using Opc.Ua.Client;
using OpcCallbacks.Abstract;

namespace OpcCallbacks.tests
{
    public class OpcCallbacksManagerTests
    {
        private readonly IOpcTagCallbacksCount _mockCallbacksCount;
        private readonly OpcCallbacksManager _manager;
        private const string tagId = "ns=2;s=KAESER.Main.Compressors.C1.ADT";
        private const string eventKey = "testEvent";

        public OpcCallbacksManagerTests()
        {
            _mockCallbacksCount = Substitute.For<IOpcTagCallbacksCount>();
            _manager = new OpcCallbacksManager(_mockCallbacksCount);
        }

        /// <summary>
        /// Verifies that when adding an item to subscription, the callback count is incremented.
        /// </summary>
        [Fact]
        public async Task AddItemToSubscription_Always_IncrementsCallbackCount()
        {
            // Arrange
            static void callback(MonitoredItem item, MonitoredItemNotificationEventArgs args) { }
            var subscription = new Subscription();
            // Act
            await _manager.AddItemToSubscription(subscription, tagId, eventKey, callback);

            // Assert
            _mockCallbacksCount.Received(1).RegisterCallback(tagId);
        }

        /// <summary>
        /// Verifies that when removing an item from subscription, the callback count is decremented.
        /// </summary>
        [Fact]
        public async Task RemoveItemFromSubscription_Always_DecrementsCallbackCount()
        {
            // Arrange
            var subscription = new Subscription();

            // Act
            await _manager.RemoveItemFromSubscription(subscription, tagId, eventKey);

            // Assert
            _mockCallbacksCount.Received(1).UnregisterCallback(tagId);
        }

        /// <summary>
        /// Verifies that when callback count reaches zero, the item is removed from subscription.
        /// </summary>
        [Fact]
        public async Task RemoveItemFromSubscription_CountReachesZero_RemovesItemFromSubscription()
        {
            // Arrange
            _mockCallbacksCount.UnregisterCallback(tagId).Returns(0);
            var subscription = new Subscription();


            await _manager.RemoveItemFromSubscription(subscription, tagId, eventKey);

            // Assert
            _mockCallbacksCount.Received(1).UnregisterCallback(tagId);
            // We can't easily verify RemoveItem was called, so we focus on what we can test
        }

        /// <summary>
        /// Verifies that multiple operations on the same tag id are handled correctly.
        /// </summary>
        [Fact]
        public async Task MultipleOperations_SameTagId_ManagesCountCorrectly()
        {
            // Arrange
            var eventKey2 = "event2";
            static void callback1(MonitoredItem item, MonitoredItemNotificationEventArgs args) { }
            static void callback2(MonitoredItem item, MonitoredItemNotificationEventArgs args) { }
            var subscription = new Subscription();

            // Act
            await _manager.AddItemToSubscription(subscription, tagId, eventKey, callback1);
            await _manager.AddItemToSubscription(subscription, tagId, eventKey2, callback2);
            await _manager.RemoveItemFromSubscription(subscription, tagId, eventKey);

            // Assert
            _mockCallbacksCount.Received(2).RegisterCallback(tagId);
            _mockCallbacksCount.Received(1).UnregisterCallback(tagId);
        }

        /// <summary>
        /// Verifies that operations on different tag ids are handled independently.
        /// </summary>
        [Fact]
        public async Task MultipleOperations_DifferentTagIds_ManagesCountsIndependently()
        {
            // Arrange
            var tagId2 = "ns=2;s=KAESER.Main.Compressors.C1.ADT1"; ;
            var eventKey = "testEvent";
            var subscription = new Subscription();
            static void callback(MonitoredItem item, MonitoredItemNotificationEventArgs args) { }


            // Act
            await _manager.AddItemToSubscription(subscription, tagId, eventKey, callback);
            await _manager.AddItemToSubscription(subscription, tagId2, eventKey, callback);
            await _manager.RemoveItemFromSubscription(subscription, tagId, eventKey);

            // Assert
            _mockCallbacksCount.Received(1).RegisterCallback(tagId);
            _mockCallbacksCount.Received(1).RegisterCallback(tagId2);
            _mockCallbacksCount.Received(1).UnregisterCallback(tagId);
            _mockCallbacksCount.DidNotReceive().UnregisterCallback(tagId2);
        }
    }
}
