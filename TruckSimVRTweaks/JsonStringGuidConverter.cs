using System.Text.Json;
using System.Text.Json.Serialization;

namespace TruckSimVRTweaks
{
    public class JsonStringGuidConverter : JsonConverter<Guid>
    {
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String
                && reader.GetString() is string stringValue
                && Guid.TryParse(stringValue, out Guid guid))
            {
                return guid;
            }
            else
            {
                throw new JsonException($"Failed to parse GUID {reader.GetString()}");
            }
        }

        public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("B"));
        }
    }
}
