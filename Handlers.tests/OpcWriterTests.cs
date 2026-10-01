using Opc.Ua;
using Opc.Ua.Client;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the OpcWriter class.
    /// </summary>
    public class OpcWriterTests
    {
        private const string TagId = "ns=2;s=Dev.Count";
        private const int Value = 42;

        private readonly ISession _session = Substitute.For<ISession>();
        private ReadValueIdCollection _read;
        private WriteValueCollection _written;

        /// <summary>
        /// Verifies that the data type of the tag is read from the server, the Value attribute is written
        /// without timestamps and status, and the cancellation token reaches the session.
        /// </summary>
        [Fact]
        public async Task WriteAsync_ReadsTagTypeAndWritesOnlyValueOfTheTag()
        {
            // Arrange
            SetupTagType(BuiltInType.Int32);
            SetupWrite(StatusCodes.Good);
            using var cancellation = new CancellationTokenSource();

            // Act
            await OpcWriter.WriteAsync(_session, TagId, Value, cancellation.Token);

            // Assert
            Assert.NotNull(_read);
            Assert.Equal(
                new[] { (TagId, Attributes.DataType), (TagId, Attributes.ValueRank) },
                _read.Select(r => (r.NodeId.ToString(), r.AttributeId)));
            Assert.NotNull(_written);
            var written = Assert.Single(_written);
            Assert.Equal(new NodeId(TagId), written.NodeId);
            Assert.Equal(Attributes.Value, written.AttributeId);
            Assert.Equal(new Variant(Value), written.Value.WrappedValue);
            Assert.Equal(DateTime.MinValue, written.Value.SourceTimestamp);
            Assert.Equal(DateTime.MinValue, written.Value.ServerTimestamp);
            Assert.Equal(StatusCodes.Good, written.Value.StatusCode.Code);
            await _session.Received(1).ReadAsync(
                Arg.Any<RequestHeader>(), Arg.Any<double>(), Arg.Any<TimestampsToReturn>(),
                Arg.Any<ReadValueIdCollection>(), cancellation.Token);
            await _session.Received(1).WriteAsync(
                Arg.Any<RequestHeader>(), Arg.Any<WriteValueCollection>(), cancellation.Token);
        }

        /// <summary>
        /// Verifies that the value is written converted to the data type of the tag.
        /// </summary>
        [Theory]
        [InlineData(42, BuiltInType.UInt16, (ushort)42)]
        [InlineData("12.5", BuiltInType.Float, 12.5f)]
        [InlineData(1, BuiltInType.Boolean, true)]
        [InlineData(42, BuiltInType.String, "42")]
        public async Task WriteAsync_ConvertsValueToTagType(object value, BuiltInType tagType, object expected)
        {
            // Arrange
            SetupTagType(tagType);
            SetupWrite(StatusCodes.Good);

            // Act
            await OpcWriter.WriteAsync(_session, TagId, value, CancellationToken.None);

            // Assert
            Assert.NotNull(_written);
            var written = Assert.Single(_written).Value;
            Assert.Equal(tagType, written.WrappedValue.TypeInfo.BuiltInType);
            Assert.Equal(expected, written.Value);
        }

        /// <summary>
        /// Verifies that a value that cannot be converted to the data type of the tag is not written.
        /// </summary>
        [Fact]
        public async Task WriteAsync_WhenValueCannotBeConverted_ThrowsAndDoesNotWrite()
        {
            // Arrange
            SetupTagType(BuiltInType.UInt16);

            // Act
            var error = await Assert.ThrowsAsync<ServiceResultException>(
                () => OpcWriter.WriteAsync(_session, TagId, 42.7, CancellationToken.None));

            // Assert
            Assert.Equal(StatusCodes.BadTypeMismatch, error.StatusCode);
            await _session.DidNotReceive().WriteAsync(
                Arg.Any<RequestHeader>(), Arg.Any<WriteValueCollection>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Verifies that the value is written as is when the data type of the tag could not be read,
        /// so the server reports what is wrong with the tag.
        /// </summary>
        [Fact]
        public async Task WriteAsync_WhenTagTypeIsUnreadable_WritesValueAsIsAndThrowsWithServerStatus()
        {
            // Arrange
            _session.ReadAsync(Arg.Any<RequestHeader>(), Arg.Any<double>(), Arg.Any<TimestampsToReturn>(),
                    Arg.Any<ReadValueIdCollection>(), Arg.Any<CancellationToken>())
                .Returns(new ReadResponse
                {
                    Results = new DataValueCollection
                    {
                        new DataValue(StatusCodes.BadNodeIdUnknown),
                        new DataValue(StatusCodes.BadNodeIdUnknown)
                    }
                });
            SetupWrite(StatusCodes.BadNodeIdUnknown);

            // Act
            var error = await Assert.ThrowsAsync<ServiceResultException>(
                () => OpcWriter.WriteAsync(_session, TagId, Value, CancellationToken.None));

            // Assert
            Assert.Equal(StatusCodes.BadNodeIdUnknown, error.StatusCode);
            Assert.NotNull(_written);
            Assert.Equal(new Variant(Value), Assert.Single(_written).Value.WrappedValue);
        }

        /// <summary>
        /// Verifies that a write rejected by the server throws with the status returned for the tag.
        /// </summary>
        [Theory]
        [InlineData(StatusCodes.BadNotWritable)]
        [InlineData(StatusCodes.BadTypeMismatch)]
        public async Task WriteAsync_WhenServerRejectsWrite_ThrowsWithServerStatus(uint status)
        {
            // Arrange
            SetupTagType(BuiltInType.Int32);
            SetupWrite(status);

            // Act
            var error = await Assert.ThrowsAsync<ServiceResultException>(
                () => OpcWriter.WriteAsync(_session, TagId, Value, CancellationToken.None));

            // Assert
            Assert.Equal(status, error.StatusCode);
        }

        /// <summary>
        /// Verifies that a good status other than plain Good is not treated as a failure.
        /// </summary>
        [Fact]
        public async Task WriteAsync_WhenWriteCompletesAsynchronously_DoesNotThrow()
        {
            // Arrange
            SetupTagType(BuiltInType.Int32);
            SetupWrite(StatusCodes.GoodCompletesAsynchronously);

            // Act
            var error = await Record.ExceptionAsync(
                () => OpcWriter.WriteAsync(_session, TagId, Value, CancellationToken.None));

            // Assert
            Assert.Null(error);
        }

        /// <summary>
        /// Verifies that a response without a result for the written tag throws.
        /// </summary>
        [Fact]
        public async Task WriteAsync_WhenResponseHasNoResult_Throws()
        {
            // Arrange
            SetupTagType(BuiltInType.Int32);
            _session.WriteAsync(Arg.Any<RequestHeader>(), Arg.Any<WriteValueCollection>(), Arg.Any<CancellationToken>())
                .Returns(new WriteResponse { Results = new StatusCodeCollection() });

            // Act
            var error = await Assert.ThrowsAsync<ServiceResultException>(
                () => OpcWriter.WriteAsync(_session, TagId, Value, CancellationToken.None));

            // Assert
            Assert.Equal(StatusCodes.BadUnexpectedError, error.StatusCode);
        }

        private void SetupTagType(BuiltInType tagType)
        {
            _session.ReadAsync(Arg.Any<RequestHeader>(), Arg.Any<double>(), Arg.Any<TimestampsToReturn>(),
                    Arg.Do<ReadValueIdCollection>(r => _read = r), Arg.Any<CancellationToken>())
                .Returns(new ReadResponse
                {
                    Results = new DataValueCollection
                    {
                        new DataValue(new Variant(new NodeId((uint)tagType))),
                        new DataValue(new Variant(ValueRanks.Scalar))
                    }
                });
        }

        private void SetupWrite(uint status)
        {
            _session.WriteAsync(Arg.Any<RequestHeader>(),
                    Arg.Do<WriteValueCollection>(w => _written = w), Arg.Any<CancellationToken>())
                .Returns(new WriteResponse { Results = new StatusCodeCollection { status } });
        }
    }
}
