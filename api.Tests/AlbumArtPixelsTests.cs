using api.Services;
using Xunit;

namespace api.Tests;

public class AlbumArtPixelsTests
{
    [Fact]
    public void Reads_png_dimensions_and_warns_outside_the_recommended_range()
    {
        var small = AlbumArtPixels.TryRead(PngHeader(400, 400));
        Assert.Equal(400, small.Width);
        Assert.Equal(400, small.Height);
        Assert.Equal(
            "Cover is 400x400. Recommended size is 1600-3000 pixels on each side.",
            AlbumArtPixels.Warn(small.Width, small.Height));

        var large = AlbumArtPixels.TryRead(PngHeader(3200, 2000));
        Assert.Equal(
            "Cover is 3200x2000. Recommended size is 1600-3000 pixels on each side.",
            AlbumArtPixels.Warn(large.Width, large.Height));

        var recommended = AlbumArtPixels.TryRead(PngHeader(1600, 3000));
        Assert.Null(AlbumArtPixels.Warn(recommended.Width, recommended.Height));
        Assert.Null(AlbumArtPixels.Warn(null, null));
    }

    [Fact]
    public void Sniffs_png_bytes_when_the_content_type_is_missing()
    {
        Assert.Equal("image/png", AlbumArtPixels.SniffMime(PngHeader(16, 16), null));
        Assert.Equal("image/jpeg", AlbumArtPixels.SniffMime([0xFF, 0xD8, 0xFF, 0xE0], "image/jpeg"));
        Assert.Null(AlbumArtPixels.SniffMime([1, 2, 3], "audio/mpeg"));
    }

    [Fact]
    public void Truncated_or_garbage_bytes_do_not_throw()
    {
        Assert.Equal((null, null), AlbumArtPixels.TryRead([]));
        Assert.Equal((null, null), AlbumArtPixels.TryRead([0xFF, 0xD8]));
        Assert.Null(AlbumArtPixels.SniffMime([], "not-a-type"));
    }

    internal static byte[] PngHeader(int width, int height)
    {
        var bytes = new byte[24];
        ReadOnlySpan<byte> signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        signature.CopyTo(bytes);
        bytes[11] = 13;
        bytes[12] = (byte)'I';
        bytes[13] = (byte)'H';
        bytes[14] = (byte)'D';
        bytes[15] = (byte)'R';
        bytes[16] = (byte)(width >> 24);
        bytes[17] = (byte)(width >> 16);
        bytes[18] = (byte)(width >> 8);
        bytes[19] = (byte)width;
        bytes[20] = (byte)(height >> 24);
        bytes[21] = (byte)(height >> 16);
        bytes[22] = (byte)(height >> 8);
        bytes[23] = (byte)height;
        return bytes;
    }
}
