using System.Buffers.Binary;
using System.Text;

namespace api.Services;

public static class AlbumArtPixels
{
    public const int MinEdge = 1600;
    public const int MaxEdge = 3000;
    public const int MaxBytes = 10 * 1024 * 1024;

    public static (int? Width, int? Height) TryRead(ReadOnlySpan<byte> bytes)
    {
        try
        {
            if (TryReadPng(bytes, out var pngWidth, out var pngHeight))
            {
                return (pngWidth, pngHeight);
            }

            if (TryReadJpeg(bytes, out var jpegWidth, out var jpegHeight))
            {
                return (jpegWidth, jpegHeight);
            }

            if (TryReadGif(bytes, out var gifWidth, out var gifHeight))
            {
                return (gifWidth, gifHeight);
            }

            if (TryReadWebp(bytes, out var webpWidth, out var webpHeight))
            {
                return (webpWidth, webpHeight);
            }
        }
        catch
        {
            return (null, null);
        }

        return (null, null);
    }

    public static string? Warn(int? width, int? height)
    {
        if (width is null or <= 0 || height is null or <= 0)
        {
            return null;
        }

        if (IsInsideRange(width.Value) && IsInsideRange(height.Value))
        {
            return null;
        }

        return $"Cover is {width.Value}x{height.Value}. Recommended size is {MinEdge}-{MaxEdge} pixels on each side.";
    }

    public static string? SniffMime(ReadOnlySpan<byte> bytes, string? contentType)
    {
        try
        {
            if (DriveFileKinds.IsImageMime(contentType))
            {
                return contentType;
            }

            if (HasPngSignature(bytes))
            {
                return "image/png";
            }

            if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            {
                return "image/jpeg";
            }

            if (HasGifSignature(bytes))
            {
                return "image/gif";
            }

            if (HasWebpSignature(bytes))
            {
                return "image/webp";
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    public static async Task<string?> CopyCappedAsync(
        Stream source,
        Stream destination,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        var rented = System.Buffers.ArrayPool<byte>.Shared.Rent(81_920);
        try
        {
            long total = 0;
            int read;
            while ((read = await source.ReadAsync(rented.AsMemory(0, rented.Length), cancellationToken)) > 0)
            {
                total += read;
                if (total > maxBytes)
                {
                    return "too_large";
                }

                await destination.WriteAsync(rented.AsMemory(0, read), cancellationToken);
            }

            return total == 0 ? "empty_file" : null;
        }
        catch
        {
            return "upload_failed";
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static bool IsInsideRange(int edge) => edge is >= MinEdge and <= MaxEdge;

    private static bool TryReadPng(ReadOnlySpan<byte> bytes, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (!HasPngSignature(bytes) || bytes.Length < 24)
        {
            return false;
        }

        if (bytes[12] != (byte)'I' || bytes[13] != (byte)'H' || bytes[14] != (byte)'D' || bytes[15] != (byte)'R')
        {
            return false;
        }

        width = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(16, 4));
        height = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(20, 4));
        return width > 0 && height > 0;
    }

    private static bool TryReadJpeg(ReadOnlySpan<byte> bytes, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (bytes.Length < 4 || bytes[0] != 0xFF || bytes[1] != 0xD8)
        {
            return false;
        }

        var offset = 2;
        while (offset + 9 < bytes.Length)
        {
            if (bytes[offset] != 0xFF)
            {
                offset++;
                continue;
            }

            var marker = bytes[offset + 1];
            if (marker is 0xD8 or 0x01 || marker is >= 0xD0 and <= 0xD9)
            {
                offset += 2;
                continue;
            }

            if (offset + 3 >= bytes.Length)
            {
                return false;
            }

            var segmentLength = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 2, 2));
            if (segmentLength < 2 || offset + 2 + segmentLength > bytes.Length)
            {
                return false;
            }

            if (marker is 0xC0 or 0xC1 or 0xC2)
            {
                height = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 5, 2));
                width = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 7, 2));
                return width > 0 && height > 0;
            }

            offset += 2 + segmentLength;
        }

        return false;
    }

    private static bool TryReadGif(ReadOnlySpan<byte> bytes, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (!HasGifSignature(bytes) || bytes.Length < 10)
        {
            return false;
        }

        width = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(6, 2));
        height = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(8, 2));
        return width > 0 && height > 0;
    }

    private static bool TryReadWebp(ReadOnlySpan<byte> bytes, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (!HasWebpSignature(bytes) || bytes.Length < 30)
        {
            return false;
        }

        var fourCc = Encoding.ASCII.GetString(bytes.Slice(12, 4));
        if (fourCc == "VP8X" && bytes.Length >= 30)
        {
            width = 1 + (bytes[24] | (bytes[25] << 8) | (bytes[26] << 16));
            height = 1 + (bytes[27] | (bytes[28] << 8) | (bytes[29] << 16));
            return width > 0 && height > 0;
        }

        if (fourCc == "VP8 " && bytes.Length >= 30)
        {
            width = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(26, 2)) & 0x3FFF;
            height = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(28, 2)) & 0x3FFF;
            return width > 0 && height > 0;
        }

        if (fourCc == "VP8L" && bytes.Length >= 25)
        {
            var packed = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(21, 4));
            width = (int)((packed & 0x3FFF) + 1);
            height = (int)(((packed >> 14) & 0x3FFF) + 1);
            return width > 0 && height > 0;
        }

        return false;
    }

    private static bool HasPngSignature(ReadOnlySpan<byte> bytes)
    {
        return bytes.Length >= 8
            && bytes[0] == 0x89
            && bytes[1] == 0x50
            && bytes[2] == 0x4E
            && bytes[3] == 0x47
            && bytes[4] == 0x0D
            && bytes[5] == 0x0A
            && bytes[6] == 0x1A
            && bytes[7] == 0x0A;
    }

    private static bool HasGifSignature(ReadOnlySpan<byte> bytes)
    {
        return bytes.Length >= 6
            && bytes[0] == (byte)'G'
            && bytes[1] == (byte)'I'
            && bytes[2] == (byte)'F'
            && bytes[3] == (byte)'8'
            && (bytes[4] == (byte)'7' || bytes[4] == (byte)'9')
            && bytes[5] == (byte)'a';
    }

    private static bool HasWebpSignature(ReadOnlySpan<byte> bytes)
    {
        return bytes.Length >= 16
            && bytes[0] == (byte)'R'
            && bytes[1] == (byte)'I'
            && bytes[2] == (byte)'F'
            && bytes[3] == (byte)'F'
            && bytes[8] == (byte)'W'
            && bytes[9] == (byte)'E'
            && bytes[10] == (byte)'B'
            && bytes[11] == (byte)'P';
    }
}
