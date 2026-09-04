namespace api.Services;

public interface IAudioPlaybackService
{
    Task<AudioOpenResult?> OpenAsync(int songId, CancellationToken cancellationToken);

    Task<AudioOpenResult?> OpenAsync(int songId, int? memberId, CancellationToken cancellationToken);
}

public sealed class AudioOpenResult : IDisposable
{
    public required Stream Stream { get; init; }
    public required string ContentType { get; init; }
    public bool EnableRangeProcessing { get; init; }

    public void Dispose() => Stream.Dispose();
}
