using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sigapi.Common.Scraping.Networking.Sessions;

public sealed class SessionSerializer : JsonConverter<ISession>
{
    private const string IdProperty = nameof(ISession.Id);
    private const string CreatedAtProperty = nameof(ISession.CreatedAt);
    private const string ExpiresAtProperty = nameof(ISession.ExpiresAt);
    private const string CookiesProperty = "Cookies";

    public override ISession Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected '{JsonTokenType.StartObject}' token.");
        }

        string? id = null;
        DateTimeOffset? createdAt = null;
        DateTimeOffset? expiresAt = null;
        List<Cookie>? cookies = null;

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
            {
                return new Session(
                    id ?? null!,
                    createdAt ?? default,
                    expiresAt ?? default,
                    cookies ?? []);
            }

            if (reader.TokenType is not JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected {JsonTokenType.PropertyName} token.");
            }

            var property = reader.GetString();

            reader.Read(); // Move to the property value.

            switch (property)
            {
                case IdProperty:
                    id = reader.GetString();
                    break;
                case CreatedAtProperty:
                    createdAt = DateTimeOffset.Parse(reader.GetString()!);
                    break;
                case ExpiresAtProperty:
                    expiresAt = DateTimeOffset.Parse(reader.GetString()!);
                    break;
                case CookiesProperty:
                    cookies = JsonSerializer.Deserialize<List<Cookie>>(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException("Unexpected end of JSON.");
    }

    public override void Write(Utf8JsonWriter writer, ISession value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(IdProperty, value.Id);
        writer.WriteString(CreatedAtProperty, value.CreatedAt);
        writer.WriteString(ExpiresAtProperty, value.ExpiresAt);

        // We can directly serialize the list of cookies.
        writer.WritePropertyName(CookiesProperty);
        JsonSerializer.Serialize(writer, value.ListCookies(), options);

        writer.WriteEndObject();
    }
}