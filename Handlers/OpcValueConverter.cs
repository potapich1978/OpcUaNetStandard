using Opc.Ua;
using System;
using System.Collections.Generic;

namespace Handlers
{
    /// <summary>
    /// Converts a value to the data type of an OPC UA tag.
    /// </summary>
    internal static class OpcValueConverter
    {
        private static readonly HashSet<BuiltInType> AbstractTypes = new HashSet<BuiltInType>
        {
            BuiltInType.Null,
            BuiltInType.Variant,
            BuiltInType.Number,
            BuiltInType.Integer,
            BuiltInType.UInteger
        };

        private static readonly HashSet<BuiltInType> IntegerTypes = new HashSet<BuiltInType>
        {
            BuiltInType.SByte,
            BuiltInType.Byte,
            BuiltInType.Int16,
            BuiltInType.UInt16,
            BuiltInType.Int32,
            BuiltInType.UInt32,
            BuiltInType.Int64,
            BuiltInType.UInt64,
            BuiltInType.Enumeration
        };

        /// <summary>
        /// The value converted to the data type of the tag; the value itself when it already has that type
        /// or the tag has no concrete data type. Throws when the value cannot be converted.
        /// </summary>
        public static object ToTagType(object value, TypeInfo tagType)
        {
            var target = tagType.BuiltInType;
            if (value == null || AbstractTypes.Contains(target))
                return value;

            // byte[] is both ByteString and an array of Byte, and a CLR type test takes sbyte[] for byte[].
            if (value.GetType() == typeof(byte[]))
                return value;

            if (IntegerTypes.Contains(target) && AnyNumber(value, HasFraction))
                throw Failure(StatusCodes.BadTypeMismatch, value, target, null);

            object converted;
            try
            {
                converted = TypeInfo.Cast(value, target);
            }
            catch (OverflowException error)
            {
                throw Failure(StatusCodes.BadOutOfRange, value, target, error);
            }
            catch (Exception error)
            {
                throw Failure(StatusCodes.BadTypeMismatch, value, target, error);
            }

            if (converted == null)
                throw Failure(StatusCodes.BadTypeMismatch, value, target, null);

            // A number outside the range of Float or Double is cast to infinity instead of failing.
            if (AnyNumber(converted, double.IsInfinity) && !AnyNumber(value, double.IsInfinity))
                throw Failure(StatusCodes.BadOutOfRange, value, target, null);

            return converted;
        }

        private static bool HasFraction(double number)
            => number % 1 != 0;

        private static bool AnyNumber(object value, Func<double, bool> matches)
        {
            switch (value)
            {
                case float number:
                    return matches(number);
                case double number:
                    return matches(number);
                case Array items:
                    foreach (var item in items)
                    {
                        if (AnyNumber(item, matches))
                            return true;
                    }

                    return false;
                default:
                    return false;
            }
        }

        private static ServiceResultException Failure(uint status, object value, BuiltInType target, Exception error)
            => new ServiceResultException(
                status,
                FormattableString.Invariant(
                    $"Value '{value}' of type {value.GetType().Name} cannot be converted to tag data type {target}."),
                error);
    }
}
