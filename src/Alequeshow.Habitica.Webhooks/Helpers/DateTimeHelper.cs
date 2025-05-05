namespace Alequeshow.Habitica.Webhooks.Helpers;

public static class DateTimeHelper
{
    public static DateTime FromBrtToUtc(this DateTime dateTime)
    {
        // Adds the UTC offset to the dateTime so when the data is saved, it can be retrieved with the expected data conversion.
        // Since the BRT to UTC is -3, we need to add 3 hours to the dateTime to compensate this difference.
        return dateTime.AddHours(3);
    }
}