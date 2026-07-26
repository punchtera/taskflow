using System.Globalization;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// SQLite has no native Guid or DateTime type, so those are stored as TEXT.
/// These helpers make the conversion explicit rather than relying on implicit
/// ORM behaviour — which keeps the mapping obvious and predictable in review.
/// </summary>
internal static class SqliteValueConverter
{
    public static string ToText(Guid value) => value.ToString();

    public static string ToText(DateTime value) =>
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    public static string? ToText(DateTime? value) =>
        value.HasValue ? ToText(value.Value) : null;

    // Values are always written via ToText(DateTime) as UTC "O" strings (trailing 'Z'),
    // so RoundtripKind yields a DateTime with Kind == Utc.
    public static DateTime ToDateTime(string value) =>
        DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    public static DateTime? ToNullableDateTime(string? value) =>
        string.IsNullOrEmpty(value) ? null : ToDateTime(value);
}
