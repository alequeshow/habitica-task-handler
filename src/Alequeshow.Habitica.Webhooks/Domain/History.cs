using System.Text.Json.Serialization;
using Alequeshow.Habitica.Webhooks.Helpers;

namespace Alequeshow.Habitica.Webhooks.Domain;

public record History
{
    [JsonConverter(typeof(EpochDateTimeConverter))]
    public required DateTime Date { get; set; }

    public double? Value { get; set; }

    public bool? IsDue { get; set; }

    public bool? Completed { get; set; }

}