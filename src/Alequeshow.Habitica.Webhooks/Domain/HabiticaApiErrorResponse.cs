namespace Alequeshow.Habitica.Webhooks.Domain;

public record HabiticaApiErrorResponse
{
    public bool Success { get; set; }    

    public string? Error { get; set; }

    public string? Message { get; set; }
}