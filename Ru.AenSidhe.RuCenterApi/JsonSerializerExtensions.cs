using System.Text.Json;

namespace Ru.AenSidhe.RuCenterApi;

public static class JsonSerializerExtensions
{
    public static T? DeserializeAnonymousType<T>(this string json, T _, JsonSerializerOptions? options = default)
        => JsonSerializer.Deserialize<T>(json, options);

    public static ValueTask<TValue?> DeserializeAnonymousTypeAsync<TValue>(this Stream stream, TValue _, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken); // Method to deserialize from a stream added for completeness
}
