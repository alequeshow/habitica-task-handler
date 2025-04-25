namespace Alequeshow.Habitica.Webhooks.Domain;

public record HabiticaApiResponse<T>
{
    public bool Success { get; set; }

    public T? Data { get; set; }
}