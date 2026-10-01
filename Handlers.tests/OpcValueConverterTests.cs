using Opc.Ua;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the OpcValueConverter class.
    /// </summary>
    public class OpcValueConverterTests
    {
        private static readonly double[] NumbersWithFraction = { 1.0, 2.5 };
        private static readonly byte[] Bytes = { 1, 2, 3 };
        private static readonly int[] MatrixElements = { 1, 2, 3, 4 };
        private static readonly double[] NumbersBeyondFloat = { 1.0, 1e40 };
        private static readonly int[] NumbersBeyondUInt16 = { 1, 70000 };

        private static readonly object[] WholeNumberSources =
        {
            (sbyte)42, (byte)42, (short)42, (ushort)42, 42, 42u, 42L, 42ul, 42f, 42.0, "42"
        };

        private static readonly (BuiltInType TagType, object Expected)[] WholeNumberByTagType =
        {
            (BuiltInType.SByte, (sbyte)42),
            (BuiltInType.Byte, (byte)42),
            (BuiltInType.Int16, (short)42),
            (BuiltInType.UInt16, (ushort)42),
            (BuiltInType.Int32, 42),
            (BuiltInType.UInt32, 42u),
            (BuiltInType.Int64, 42L),
            (BuiltInType.UInt64, 42ul),
            (BuiltInType.Enumeration, 42),
            (BuiltInType.Float, 42f),
            (BuiltInType.Double, 42.0),
            (BuiltInType.String, "42")
        };

        /// <summary>
        /// A whole number of every numeric .NET type and as a string, for every numeric and string tag type.
        /// </summary>
        public static TheoryData<object, BuiltInType, object> WholeNumberConversions()
        {
            var conversions = new TheoryData<object, BuiltInType, object>();
            foreach (var (tagType, expected) in WholeNumberByTagType)
            {
                foreach (var source in WholeNumberSources)
                    conversions.Add(source, tagType, expected);
            }

            return conversions;
        }

        /// <summary>
        /// Arrays of another element type and the array expected for the tag.
        /// </summary>
        public static TheoryData<Array, BuiltInType, Array> ArrayConversions() => new()
        {
            { new[] { 1, 2 }, BuiltInType.UInt16, new ushort[] { 1, 2 } },
            { new[] { 1L, 2L }, BuiltInType.Int16, new short[] { 1, 2 } },
            { new sbyte[] { 1, 2 }, BuiltInType.Int16, new short[] { 1, 2 } },
            { new[] { 1.0, 2.0 }, BuiltInType.Int32, new[] { 1, 2 } },
            { new[] { "1", "2" }, BuiltInType.Int32, new[] { 1, 2 } },
            { new[] { 1.5, 2.5 }, BuiltInType.Float, new[] { 1.5f, 2.5f } },
            { new[] { 1, 2 }, BuiltInType.Double, new[] { 1.0, 2.0 } },
            { new[] { "1.5", "2.5" }, BuiltInType.Double, new[] { 1.5, 2.5 } },
            { new[] { 0, 1 }, BuiltInType.Boolean, new[] { false, true } },
            { new[] { "true", "false" }, BuiltInType.Boolean, new[] { true, false } },
            { new[] { 1, 2 }, BuiltInType.String, new[] { "1", "2" } },
            { new[] { 1.5, 2.5 }, BuiltInType.String, new[] { "1.5", "2.5" } }
        };

        /// <summary>
        /// Verifies that a whole number of any numeric .NET type or a string with it is converted
        /// to the data type of an integer, floating-point or string tag.
        /// </summary>
        [Theory]
        [MemberData(nameof(WholeNumberConversions))]
        public void ToTagType_WithWholeNumber_ConvertsToTagType(object value, BuiltInType tagType, object expected)
        {
            // Act
            var converted = OpcValueConverter.ToTagType(value, new TypeInfo(tagType, ValueRanks.Scalar));

            // Assert
            Assert.IsType(expected.GetType(), converted);
            Assert.Equal(expected, converted);
        }

        /// <summary>
        /// Verifies that fractional numbers, strings with them and logical values are converted
        /// to the data type of the tag.
        /// </summary>
        [Theory]
        [InlineData(12.5, BuiltInType.Float, 12.5f)]
        [InlineData("12.5", BuiltInType.Float, 12.5f)]
        [InlineData(12.5f, BuiltInType.Double, 12.5)]
        [InlineData("12.5", BuiltInType.Double, 12.5)]
        [InlineData(12.5, BuiltInType.String, "12.5")]
        [InlineData(12.5f, BuiltInType.String, "12.5")]
        [InlineData(0, BuiltInType.Boolean, false)]
        [InlineData(1, BuiltInType.Boolean, true)]
        [InlineData(1L, BuiltInType.Boolean, true)]
        [InlineData((byte)0, BuiltInType.Boolean, false)]
        [InlineData("true", BuiltInType.Boolean, true)]
        [InlineData("false", BuiltInType.Boolean, false)]
        [InlineData("1", BuiltInType.Boolean, true)]
        [InlineData("0", BuiltInType.Boolean, false)]
        [InlineData(true, BuiltInType.Int32, 1)]
        public void ToTagType_WithValueOfAnotherType_ConvertsToTagType(
            object value, BuiltInType tagType, object expected)
        {
            // Act
            var converted = OpcValueConverter.ToTagType(value, new TypeInfo(tagType, ValueRanks.Scalar));

            // Assert
            Assert.IsType(expected.GetType(), converted);
            Assert.Equal(expected, converted);
        }

        /// <summary>
        /// Verifies that an array is converted element by element to the data type of the tag.
        /// </summary>
        [Theory]
        [MemberData(nameof(ArrayConversions))]
        public void ToTagType_WithArrayOfAnotherType_ConvertsEachElement(
            Array value, BuiltInType tagType, Array expected)
        {
            // Act
            var converted = OpcValueConverter.ToTagType(value, new TypeInfo(tagType, ValueRanks.OneDimension));

            // Assert
            Assert.IsType(expected.GetType(), converted);
            Assert.Equal(expected.Cast<object>(), ((Array)converted).Cast<object>());
        }

        /// <summary>
        /// Verifies that a value that already has the data type of the tag is returned untouched.
        /// </summary>
        [Fact]
        public void ToTagType_WithValueOfTagType_ReturnsSameValue()
        {
            // Arrange
            object value = (ushort)42;

            // Act
            var converted = OpcValueConverter.ToTagType(value, new TypeInfo(BuiltInType.UInt16, ValueRanks.Scalar));

            // Assert
            Assert.Same(value, converted);
        }

        /// <summary>
        /// Verifies that a matrix of the data type of the tag is returned untouched.
        /// </summary>
        [Fact]
        public void ToTagType_WithMatrixOfTagType_ReturnsSameValue()
        {
            // Arrange
            var matrix = new Matrix(MatrixElements, BuiltInType.Int32, 2, 2);

            // Act
            var converted = OpcValueConverter.ToTagType(
                matrix, new TypeInfo(BuiltInType.Int32, ValueRanks.TwoDimensions));

            // Assert
            Assert.Same(matrix, converted);
        }

        /// <summary>
        /// Verifies that a value for a tag without a concrete data type is returned untouched.
        /// </summary>
        [Theory]
        [InlineData(BuiltInType.Null)]
        [InlineData(BuiltInType.Variant)]
        [InlineData(BuiltInType.Number)]
        [InlineData(BuiltInType.Integer)]
        [InlineData(BuiltInType.UInteger)]
        public void ToTagType_WithTagOfAbstractType_ReturnsSameValue(BuiltInType tagType)
        {
            // Arrange
            object value = 42;

            // Act
            var converted = OpcValueConverter.ToTagType(value, new TypeInfo(tagType, ValueRanks.Scalar));

            // Assert
            Assert.Same(value, converted);
        }

        /// <summary>
        /// Verifies that a byte array is returned untouched for a ByteString tag and for a tag with an array of Byte.
        /// </summary>
        [Theory]
        [InlineData(BuiltInType.ByteString, ValueRanks.Scalar)]
        [InlineData(BuiltInType.Byte, ValueRanks.OneDimension)]
        public void ToTagType_WithByteArray_ReturnsSameValue(BuiltInType tagType, int valueRank)
        {
            // Act
            var converted = OpcValueConverter.ToTagType(Bytes, new TypeInfo(tagType, valueRank));

            // Assert
            Assert.Same(Bytes, converted);
        }

        /// <summary>
        /// Verifies that a null value is returned as is.
        /// </summary>
        [Fact]
        public void ToTagType_WithNullValue_ReturnsNull()
        {
            // Act
            var converted = OpcValueConverter.ToTagType(null, new TypeInfo(BuiltInType.Int32, ValueRanks.Scalar));

            // Assert
            Assert.Null(converted);
        }

        /// <summary>
        /// Verifies that a value with a fractional part is rejected for an integer tag instead of being rounded.
        /// </summary>
        [Theory]
        [InlineData(42.7, BuiltInType.UInt16)]
        [InlineData(42.5f, BuiltInType.Int32)]
        [InlineData(0.5, BuiltInType.Enumeration)]
        public void ToTagType_WithFractionForIntegerTag_ThrowsTypeMismatch(object value, BuiltInType tagType)
        {
            // Act
            var error = Assert.Throws<ServiceResultException>(
                () => OpcValueConverter.ToTagType(value, new TypeInfo(tagType, ValueRanks.Scalar)));

            // Assert
            Assert.Equal(StatusCodes.BadTypeMismatch, error.StatusCode);
        }

        /// <summary>
        /// Verifies that an array with a fractional element is rejected for a tag with an array of integers.
        /// </summary>
        [Fact]
        public void ToTagType_WithFractionInArrayForIntegerTag_ThrowsTypeMismatch()
        {
            // Act
            var error = Assert.Throws<ServiceResultException>(() => OpcValueConverter.ToTagType(
                NumbersWithFraction, new TypeInfo(BuiltInType.UInt16, ValueRanks.OneDimension)));

            // Assert
            Assert.Equal(StatusCodes.BadTypeMismatch, error.StatusCode);
        }

        /// <summary>
        /// Verifies that a value outside the range of the data type of the tag is rejected with the cause kept.
        /// </summary>
        [Theory]
        [InlineData(70000, BuiltInType.UInt16)]
        [InlineData(-1, BuiltInType.UInt16)]
        [InlineData(1e10, BuiltInType.Int32)]
        public void ToTagType_WithValueOutOfRange_ThrowsOutOfRange(object value, BuiltInType tagType)
        {
            // Act
            var error = Assert.Throws<ServiceResultException>(
                () => OpcValueConverter.ToTagType(value, new TypeInfo(tagType, ValueRanks.Scalar)));

            // Assert
            Assert.Equal(StatusCodes.BadOutOfRange, error.StatusCode);
            Assert.IsType<OverflowException>(error.InnerException);
        }

        /// <summary>
        /// Verifies that an array with an element outside the range of the data type of the tag is rejected.
        /// </summary>
        [Fact]
        public void ToTagType_WithValueOutOfRangeInArray_ThrowsOutOfRange()
        {
            // Act
            var error = Assert.Throws<ServiceResultException>(() => OpcValueConverter.ToTagType(
                NumbersBeyondUInt16, new TypeInfo(BuiltInType.UInt16, ValueRanks.OneDimension)));

            // Assert
            Assert.Equal(StatusCodes.BadOutOfRange, error.StatusCode);
        }

        /// <summary>
        /// Verifies that a number outside the range of a floating-point tag is rejected instead of becoming infinity.
        /// </summary>
        [Theory]
        [InlineData(1e40, BuiltInType.Float)]
        [InlineData("1e40", BuiltInType.Float)]
        [InlineData("1e400", BuiltInType.Double)]
        public void ToTagType_WithNumberBeyondFloatingPointRange_ThrowsOutOfRange(object value, BuiltInType tagType)
        {
            // Act
            var error = Assert.Throws<ServiceResultException>(
                () => OpcValueConverter.ToTagType(value, new TypeInfo(tagType, ValueRanks.Scalar)));

            // Assert
            Assert.Equal(StatusCodes.BadOutOfRange, error.StatusCode);
        }

        /// <summary>
        /// Verifies that an array with an element outside the range of a floating-point tag is rejected.
        /// </summary>
        [Fact]
        public void ToTagType_WithNumberBeyondFloatRangeInArray_ThrowsOutOfRange()
        {
            // Act
            var error = Assert.Throws<ServiceResultException>(() => OpcValueConverter.ToTagType(
                NumbersBeyondFloat, new TypeInfo(BuiltInType.Float, ValueRanks.OneDimension)));

            // Assert
            Assert.Equal(StatusCodes.BadOutOfRange, error.StatusCode);
        }

        /// <summary>
        /// Verifies that infinity passed by the caller is converted, not taken for a number out of range.
        /// </summary>
        [Fact]
        public void ToTagType_WithInfinity_ConvertsToInfinityOfTagType()
        {
            // Act
            var converted = OpcValueConverter.ToTagType(
                double.PositiveInfinity, new TypeInfo(BuiltInType.Float, ValueRanks.Scalar));

            // Assert
            Assert.Equal(float.PositiveInfinity, Assert.IsType<float>(converted));
        }

        /// <summary>
        /// Verifies that a string that is not a value of the data type of the tag is rejected with the cause kept.
        /// </summary>
        [Theory]
        [InlineData("abc", BuiltInType.Int32)]
        [InlineData("12,5", BuiltInType.Float)]
        [InlineData("42.7", BuiltInType.UInt16)]
        [InlineData("True", BuiltInType.Boolean)]
        public void ToTagType_WithUnparsableString_ThrowsTypeMismatch(string value, BuiltInType tagType)
        {
            // Act
            var error = Assert.Throws<ServiceResultException>(
                () => OpcValueConverter.ToTagType(value, new TypeInfo(tagType, ValueRanks.Scalar)));

            // Assert
            Assert.Equal(StatusCodes.BadTypeMismatch, error.StatusCode);
            Assert.IsType<FormatException>(error.InnerException);
        }

        /// <summary>
        /// Verifies that a value of a .NET type unknown to OPC UA is rejected and the message names
        /// the value, its type and the data type of the tag.
        /// </summary>
        [Fact]
        public void ToTagType_WithUnsupportedValueType_ThrowsTypeMismatchNamingValueAndTypes()
        {
            // Act
            var error = Assert.Throws<ServiceResultException>(
                () => OpcValueConverter.ToTagType(12.5m, new TypeInfo(BuiltInType.Double, ValueRanks.Scalar)));

            // Assert
            Assert.Equal(StatusCodes.BadTypeMismatch, error.StatusCode);
            Assert.Contains("'12.5'", error.Message);
            Assert.Contains(nameof(Decimal), error.Message);
            Assert.Contains(nameof(BuiltInType.Double), error.Message);
        }
    }
}
