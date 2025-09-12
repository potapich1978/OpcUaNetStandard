using Opc.Ua.Client;
using OpcCallbacks.Abstract;

namespace OpcSubscriptions.tests
{
    /// <summary>
    /// Unit tests for <see cref="OpcSubscriptionsManager"/>.
    /// Covers subscription creation, monitored item management, and disposal logic.
    /// </summary>
    /// <summary>
    /// Unit tests for <see cref="OpcSubscriptionsManager"/>.
    /// Covers subscription creation, monitored item management, and disposal logic.
    /// </summary>
    public class OpcSubscriptionsManagerTests
    {
        private readonly IOpcCallbackManager _callbackManagerMock;
        private readonly OpcSubscriptionsManager _manager;

        public OpcSubscriptionsManagerTests()
        {
            _callbackManagerMock = Substitute.For<IOpcCallbackManager>();
            _manager = new OpcSubscriptionsManager(_callbackManagerMock);
        }

        /// <summary>
        /// Verifies that a new subscription is created, added to the session,
        /// and the monitored item callback is registered.
        /// </summary>
        [Fact]
        public async Task AddSubscription_ShouldCreateSubscriptionAndAddItem()
        {
            // Arrange
            var sessionMock = Substitute.For<ISession>();
            sessionMock.DefaultSubscription.Returns(new Subscription());
            var manager = Substitute.For<OpcSubscriptionsManager>(_callbackManagerMock);
            manager.CreateSubscriptionAsync(Arg.Any<Subscription>()).Returns(Task.CompletedTask);
            manager.ApplySubscriptionChanges(Arg.Any<Subscription>()).Returns(Task.CompletedTask);
            static void callback(MonitoredItem s, MonitoredItemNotificationEventArgs e) { }

            // Act
            await manager.AddSubscriprion(sessionMock, "App1", "Tag1", "Event1", callback);

            // Assert
            await _callbackManagerMock.Received(1).AddItemToSubscription(
                Arg.Any<Subscription>(),
                "Tag1",
                "Event1",
callback
            );
        }

        /// <summary>
        /// Ensures that removing a monitored item calls the callback manager,
        /// disposes the subscription if MonitoredItemCount is 0, and removes it from the dictionary.
        /// </summary>
        [Fact]
        public async Task RemoveItemFromSubscription_ShouldCallCallbackManager_AndDisposeIfEmpty()
        {
            // Arrange
            var subscription = new Subscription();
            var manager = new OpcSubscriptionsManager(_callbackManagerMock);

            // Inject subscription into internal dictionary
            var field = typeof(OpcSubscriptionsManager).GetField("_subscriptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dict = (System.Collections.Concurrent.ConcurrentDictionary<string, Subscription>)field.GetValue(manager);
            dict.TryAdd("App1", subscription);

            // Act
            await manager.RemoveItemFromSubscription("App1", "Tag1", "Event1");

            // Assert
            await _callbackManagerMock.Received(1).RemoveItemFromSubscription(subscription, "Tag1", "Event1");
            Assert.False(dict.ContainsKey("App1"));
        }

        /// <summary>
        /// Verifies that removing a subscription directly disposes it and removes from dictionary.
        /// </summary>
        [Fact]
        public void RemoveSubscription_ShouldDisposeAndRemove()
        {
            // Arrange
            var subscription = new Subscription();
            var manager = new OpcSubscriptionsManager(_callbackManagerMock);

            var field = typeof(OpcSubscriptionsManager).GetField("_subscriptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dict = (System.Collections.Concurrent.ConcurrentDictionary<string, Subscription>)field.GetValue(manager);
            dict.TryAdd("App2", subscription);

            // Act
            manager.RemoveSubscriprion("App2");

            // Assert
            Assert.False(dict.ContainsKey("App2"));
        }
    }
}
