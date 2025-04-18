using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alequeshow.Habitica.Webhooks.Helpers
{
    public class EpochDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Convert epoch time (long) to DateTime
            var epochTime = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeMilliseconds(epochTime).UtcDateTime;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Convert DateTime to epoch time (long)
            var epochTime = new DateTimeOffset(value).ToUnixTimeMilliseconds();
            writer.WriteNumberValue(epochTime);
        }
    }
}