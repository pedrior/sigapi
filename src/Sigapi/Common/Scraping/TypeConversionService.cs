using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;

namespace Sigapi.Common.Scraping;

public sealed class TypeConversionService : ITypeConversionService
{
    private static readonly ConcurrentDictionary<Type, TypeConverter> ConverterCache = new();

    // This dictionary remains focused on its specialty: parsing from strings.
    private static readonly ConcurrentDictionary<Type, Func<string, object?>> StringParsers = new();

    static TypeConversionService()
    {
        // Boolean.
        RegisterParser(TryParseBoolean);

        // Numbers.
        RegisterParser(input => TryParseNumber<int>(input, int.TryParse));
        RegisterParser(input => TryParseNumber<long>(input, long.TryParse));
        RegisterParser(input => TryParseNumber<float>(input, float.TryParse));
        RegisterParser(input => TryParseNumber<double>(input, double.TryParse));
        RegisterParser(input => TryParseNumber<decimal>(input, decimal.TryParse));

        // Date/Time.
        RegisterParser(input => TryParseDateTime<DateTime>(input, DateTime.TryParse));
        RegisterParser(input => TryParseDateTime<DateTimeOffset>(input, DateTimeOffset.TryParse));

        // Guid.
        RegisterParser(TryParseGuid);
    }
    
    public object? ConvertTo(object? input, Type targetType)
    {
        // Handle null.
        if (input is null)
        {
            // Return null if the target type is a reference type or a nullable value type.
            if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) is not null)
            {
                return null;
            }

            // Otherwise, we cannot assign null to a non-nullable value type.
            throw new FormatException($"Cannot convert null to non-nullable type {targetType.Name}.");
        }

        // Handle direct assignment if types are already compatible.
        if (targetType.IsInstanceOfType(input))
        {
            return input;
        }

        var nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // For string inputs, use the specialized parsing logic.
        if (input is string inputString)
        {
            return ConvertFromString(inputString, targetType, nonNullableType);
        }

        // For other types, attempt a general conversion first.
        try
        {
            return Convert.ChangeType(input, nonNullableType, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            // Fallback: Convert the object to a string and use the parsing logic.
            // This handles cases like converting an integer '1' to the boolean 'true'.
            return ConvertFromString(input.ToString(), targetType, nonNullableType);
        }
    }

    private static object? ConvertFromString(string? input, Type targetType, Type nonNullableType)
    {
        // Handle string conversion directly.
        if (targetType == typeof(string))
        {
            return input;
        }

        // Handle null/empty input.
        if (string.IsNullOrWhiteSpace(input))
        {
            return Nullable.GetUnderlyingType(targetType) is not null
                ? null
                : throw new FormatException(
                    $"Cannot convert null/empty string to non-nullable type {nonNullableType.Name}.");
        }

        // Handle enum conversion.
        if (nonNullableType.IsEnum)
        {
            return TryParseEnum(input, nonNullableType);
        }

        // Use the registered parsers.
        if (StringParsers.TryGetValue(nonNullableType, out var parser))
        {
            return parser(input);
        }

        // Fallback to TypeConverter for other types.
        return TryConvertWithTypeConverter(input, nonNullableType);
    }

    private static void RegisterParser<T>(Func<string, T> parser) where T : notnull =>
        StringParsers[typeof(T)] = input => parser(input);

    private static bool TryParseBoolean(string input) => input switch
    {
        "1" => true,
        "0" => false,
        _ => bool.TryParse(input, out var result)
            ? result
            : throw new FormatException($"Invalid boolean value: '{input}'.")
    };

    private static T TryParseNumber<T>(string input, TryParseDelegate<T> parser) where T : notnull
    {
        return parser(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : throw new FormatException($"Invalid {typeof(T).Name} value: '{input}'.");
    }

    private static T TryParseDateTime<T>(string input, TryParseDateTimeDelegate<T> parser) where T : notnull
    {
        return parser(input, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result)
            ? result
            : throw new FormatException($"Invalid {typeof(T).Name} value: '{input}'.");
    }

    private static Guid TryParseGuid(string input)
    {
        return Guid.TryParse(input, out var result)
            ? result
            : throw new FormatException($"Invalid Guid value: '{input}'.");
    }

    private static object TryParseEnum(string input, Type enumType)
    {
        try
        {
            return Enum.Parse(enumType, input, ignoreCase: true);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Invalid {enumType.Name} enum value: '{input}'.", ex);
        }
    }

    private static object? TryConvertWithTypeConverter(string input, Type targetType)
    {
        var converter = ConverterCache.GetOrAdd(targetType, TypeDescriptor.GetConverter);

        if (!converter.CanConvertFrom(typeof(string)))
        {
            throw new NotSupportedException($"Conversion not supported for type {targetType.Name}.");
        }

        try
        {
            return converter.ConvertFromString(null, CultureInfo.InvariantCulture, input);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Conversion failed for type {targetType.Name} with value '{input}'.", ex);
        }
    }

    private delegate bool TryParseDelegate<T>(string input,
        NumberStyles styles,
        IFormatProvider provider,
        out T result);

    private delegate bool TryParseDateTimeDelegate<T>(string input,
        IFormatProvider provider,
        DateTimeStyles styles,
        out T result);
}