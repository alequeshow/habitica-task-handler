using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alequeshow.Habitica.Webhooks.Helpers
{
    public class EpochReadIsoWriteDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Convert epoch time (long) to DateTime for API deserialization
            var epochTime = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeMilliseconds(epochTime).UtcDateTime;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Write DateTime in ISO format for serialization
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }
}
