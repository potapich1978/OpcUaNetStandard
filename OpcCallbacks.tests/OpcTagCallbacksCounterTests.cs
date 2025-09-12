namespace OpcCallbacks.tests
{
    /// <summary>
    /// Contains unit tests for the <see cref="OpcTagCallbacksCounter"/> class.
    /// </summary>
    public class OpcTagCallbacksCounterTests
    {
        private readonly OpcTagCallbacksCounter _counter;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpcTagCallbacksCounterTests"/> class.
        /// </summary>
        public OpcTagCallbacksCounterTests()
        {
            _counter = new OpcTagCallbacksCounter();
        }

        /// <summary>
        /// Verifies that registering a callback for a new tag ID initializes the count to one.
        /// </summary>
        [Fact]
        public void RegisterCallback_NewTagId_CountShouldBeOne()
        {
            // Arrange
            var tagId = "testTag";

            // Act
            _counter.RegisterCallback(tagId);

            // Assert
            Assert.Equal(1, _counter.CallbacksCount(tagId));
        }

        /// <summary>
        /// Verifies that registering multiple callbacks for the same tag ID increments the count correctly.
        /// </summary>
        [Fact]
        public void RegisterCallback_ExistingTagId_ShouldIncrementCount()
        {
            // Arrange
            var tagId = "testTag";
            _counter.RegisterCallback(tagId);

            // Act
            _counter.RegisterCallback(tagId);

            // Assert
            Assert.Equal(2, _counter.CallbacksCount(tagId));
        }

        /// <summary>
        /// Verifies that unregistering a callback for an existing tag ID decrements the count correctly.
        /// </summary>
        [Fact]
        public void UnregisterCallback_ExistingTagId_ShouldDecrementCount()
        {
            // Arrange
            var tagId = "testTag";
            _counter.RegisterCallback(tagId);
            _counter.RegisterCallback(tagId); // Count = 2

            // Act
            var result = _counter.UnregisterCallback(tagId);

            // Assert
            Assert.Equal(1, result);
            Assert.Equal(1, _counter.CallbacksCount(tagId));
        }

        /// <summary>
        /// Verifies that when unregistering a callback reduces the count to zero, the tag entry is removed.
        /// </summary>
        [Fact]
        public void UnregisterCallback_CountReachesZero_ShouldRemoveTagEntry()
        {
            // Arrange
            var tagId = "testTag";
            _counter.RegisterCallback(tagId); // Count = 1

            // Act
            var result = _counter.UnregisterCallback(tagId);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(0, _counter.CallbacksCount(tagId));
        }

        /// <summary>
        /// Verifies that unregistering a callback for a non-existent tag ID returns zero.
        /// </summary>
        [Fact]
        public void UnregisterCallback_NonExistentTagId_ShouldReturnZero()
        {
            // Arrange
            var tagId = "nonExistentTag";

            // Act
            var result = _counter.UnregisterCallback(tagId);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Verifies that unregistering a callback for a tag ID with zero count returns zero.
        /// </summary>
        [Fact]
        public void UnregisterCallback_CountAlreadyZero_ShouldReturnZero()
        {
            // Arrange
            var tagId = "testTag";
            _counter.RegisterCallback(tagId);
            _counter.UnregisterCallback(tagId); // Count = 0

            // Act
            var result = _counter.UnregisterCallback(tagId);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Verifies that getting the callback count for a non-existent tag ID returns zero.
        /// </summary>
        [Fact]
        public void CallbacksCount_NonExistentTagId_ShouldReturnZero()
        {
            // Arrange
            var tagId = "nonExistentTag";

            // Act
            var result = _counter.CallbacksCount(tagId);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Verifies that after multiple register and unregister operations, the count is correct.
        /// </summary>
        [Fact]
        public void CallbacksCount_AfterMultipleOperations_ShouldReturnCorrectCount()
        {
            // Arrange
            var tagId = "testTag";

            // Act
            _counter.RegisterCallback(tagId); // Count = 1
            _counter.RegisterCallback(tagId); // Count = 2
            _counter.UnregisterCallback(tagId); // Count = 1
            _counter.RegisterCallback(tagId); // Count = 2
            _counter.RegisterCallback(tagId); // Count = 3

            // Assert
            Assert.Equal(3, _counter.CallbacksCount(tagId));
        }

        /// <summary>
        /// Verifies that multiple tag IDs are tracked independently.
        /// </summary>
        [Fact]
        public void MultipleTagIds_ShouldBeTrackedIndependently()
        {
            // Arrange
            var tagId1 = "tag1";
            var tagId2 = "tag2";

            // Act
            _counter.RegisterCallback(tagId1); // tag1 = 1
            _counter.RegisterCallback(tagId1); // tag1 = 2
            _counter.RegisterCallback(tagId2); // tag2 = 1
            _counter.UnregisterCallback(tagId1); // tag1 = 1

            // Assert
            Assert.Equal(1, _counter.CallbacksCount(tagId1));
            Assert.Equal(1, _counter.CallbacksCount(tagId2));
        }

        /// <summary>
        /// Verifies that multiple threads registering callbacks for the same tag ID maintains thread safety.
        /// </summary>
        [Fact]
        public void ThreadSafety_MultipleThreadsRegisteringSameTag_ShouldMaintainCorrectCount()
        {
            // Arrange
            var tagId = "testTag";
            var iterations = 1000;

            // Act
            Parallel.For(0, iterations, i => {
                _counter.RegisterCallback(tagId);
            });

            // Assert
            Assert.Equal(iterations, _counter.CallbacksCount(tagId));
        }

        /// <summary>
        /// Verifies that multiple threads registering and unregistering callbacks for the same tag ID maintains thread safety.
        /// </summary>
        [Fact]
        public void ThreadSafety_MultipleThreadsRegisteringAndUnregistering_ShouldMaintainCorrectCount()
        {
            // Arrange
            var tagId = "testTag";
            var registerIterations = 1000;
            var unregisterIterations = 500;

            // Act
            Parallel.For(0, registerIterations, i => {
                _counter.RegisterCallback(tagId);
            });

            Parallel.For(0, unregisterIterations, i => {
                _counter.UnregisterCallback(tagId);
            });

            // Assert
            Assert.Equal(registerIterations - unregisterIterations, _counter.CallbacksCount(tagId));
        }
    }
}
