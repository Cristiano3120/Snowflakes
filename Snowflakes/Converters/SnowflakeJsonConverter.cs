using System.Text.Json;
using System.Text.Json.Serialization;

namespace Snowflakes.Converters;

public sealed class SnowflakeJsonConverter : JsonConverter<Snowflake>
{
    public override Snowflake Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.String
            ? new Snowflake(long.Parse(reader.GetString()!))
            : new Snowflake(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, Snowflake value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value.ToString());
}