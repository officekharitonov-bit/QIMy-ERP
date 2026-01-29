using System.Text;

namespace QIMy.AI.Services;

/// <summary>
/// AI-enhanced encoding detection with ML and statistical analysis
/// </summary>
public class AiEncodingDetectionService : IAiEncodingDetectionService
{
    public async Task<EncodingDetectionResult> DetectEncodingAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        // Read sample for analysis
        var buffer = new byte[Math.Min(8192, stream.Length)];
        stream.Position = 0;
        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
        var sample = buffer[..bytesRead];

        stream.Position = 0; // Reset for later use

        return await DetectEncodingAsync(sample, cancellationToken);
    }

    public Task<EncodingDetectionResult> DetectEncodingAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        var result = new EncodingDetectionResult();

        // 1. BOM Detection (highest confidence)
        var bomResult = DetectByBom(data);
        if (bomResult != null)
        {
            result.Encoding = bomResult.Encoding;
            result.Confidence = bomResult.Confidence;
            result.DetectionMethod = "BOM";
            result.Details = $"BOM detected: {bomResult.Details}";
            return Task.FromResult(result);
        }

        // 2. Statistical Analysis
        var stats = AnalyzeCharacterDistribution(data);

        // 3. UTF-8 validation test
        var utf8Score = TestUtf8Validity(data);
        if (utf8Score > 0.9m)
        {
            result.Encoding = Encoding.UTF8;
            result.Confidence = utf8Score;
            result.DetectionMethod = "Statistical";
            result.Details = "High probability UTF-8 based on byte patterns";

            result.Alternatives.Add(new AlternativeEncoding
            {
                Encoding = Encoding.GetEncoding(1252),
                Confidence = 0.3m,
                Reason = "Windows-1252 fallback"
            });

            return Task.FromResult(result);
        }

        // 4. Test Windows-1252 (common for BMD exports)
        var win1252Score = TestWindows1252(data, stats);
        if (win1252Score > 0.7m)
        {
            result.Encoding = Encoding.GetEncoding(1252);
            result.Confidence = win1252Score;
            result.DetectionMethod = "Statistical";
            result.Details = "Windows-1252 detected by character frequency";

            result.Alternatives.Add(new AlternativeEncoding
            {
                Encoding = Encoding.UTF8,
                Confidence = 0.4m,
                Reason = "UTF-8 alternative"
            });

            return Task.FromResult(result);
        }

        // 5. Default fallback (BMD standard)
        result.Encoding = Encoding.GetEncoding(1252);
        result.Confidence = 0.5m;
        result.DetectionMethod = "Fallback";
        result.Details = "Using Windows-1252 as BMD default";

        result.Alternatives.Add(new AlternativeEncoding
        {
            Encoding = Encoding.UTF8,
            Confidence = 0.5m,
            Reason = "UTF-8 alternative"
        });

        return Task.FromResult(result);
    }

    private EncodingDetectionResult? DetectByBom(byte[] data)
    {
        if (data.Length < 2) return null;

        // UTF-8 BOM: EF BB BF
        if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
        {
            return new EncodingDetectionResult
            {
                Encoding = Encoding.UTF8,
                Confidence = 1.0m,
                DetectionMethod = "BOM",
                Details = "UTF-8 BOM (EF BB BF)"
            };
        }

        // UTF-16 LE BOM: FF FE
        if (data[0] == 0xFF && data[1] == 0xFE)
        {
            return new EncodingDetectionResult
            {
                Encoding = Encoding.Unicode, // UTF-16 LE
                Confidence = 1.0m,
                DetectionMethod = "BOM",
                Details = "UTF-16 LE BOM (FF FE)"
            };
        }

        // UTF-16 BE BOM: FE FF
        if (data[0] == 0xFE && data[1] == 0xFF)
        {
            return new EncodingDetectionResult
            {
                Encoding = Encoding.BigEndianUnicode, // UTF-16 BE
                Confidence = 1.0m,
                DetectionMethod = "BOM",
                Details = "UTF-16 BE BOM (FE FF)"
            };
        }

        return null;
    }

    private CharacterStatistics AnalyzeCharacterDistribution(byte[] data)
    {
        var stats = new CharacterStatistics();

        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];

            // ASCII range (0x00-0x7F)
            if (b < 0x80)
            {
                stats.AsciiCount++;

                if (b >= 0x20 && b <= 0x7E) // Printable ASCII
                    stats.PrintableAsciiCount++;
            }
            // Extended ASCII / UTF-8 continuation bytes
            else if (b >= 0x80 && b <= 0xBF)
            {
                stats.HighByteCount++;
                stats.ExtendedAsciiCount++;
            }
            // UTF-8 multi-byte start
            else if (b >= 0xC0 && b <= 0xF7)
            {
                stats.Utf8StartByteCount++;
            }
        }

        stats.TotalBytes = data.Length;
        return stats;
    }

    private decimal TestUtf8Validity(byte[] data)
    {
        try
        {
            // Try to decode as UTF-8
            var text = Encoding.UTF8.GetString(data);

            // Check for replacement characters (indicates invalid UTF-8)
            int replacementChars = text.Count(c => c == '\uFFFD');

            if (replacementChars == 0)
            {
                // Perfect UTF-8
                return 0.95m;
            }

            // Calculate score based on replacement ratio
            decimal ratio = (decimal)replacementChars / text.Length;
            if (ratio < 0.01m) return 0.85m;
            if (ratio < 0.05m) return 0.6m;

            return 0.3m;
        }
        catch
        {
            return 0.1m;
        }
    }

    private decimal TestWindows1252(byte[] data, CharacterStatistics stats)
    {
        // Windows-1252 characteristics:
        // - Много extended ASCII (0x80-0xFF)
        // - Мало UTF-8 multi-byte sequences

        if (stats.TotalBytes == 0) return 0m;

        decimal extendedRatio = (decimal)stats.ExtendedAsciiCount / stats.TotalBytes;
        decimal utf8Ratio = (decimal)stats.Utf8StartByteCount / stats.TotalBytes;

        // High extended ASCII + low UTF-8 = likely Windows-1252
        if (extendedRatio > 0.05m && utf8Ratio < 0.02m)
        {
            return 0.8m;
        }

        // Moderate extended ASCII
        if (extendedRatio > 0.01m && utf8Ratio < 0.05m)
        {
            return 0.6m;
        }

        return 0.4m;
    }

    private class CharacterStatistics
    {
        public int TotalBytes { get; set; }
        public int AsciiCount { get; set; }
        public int PrintableAsciiCount { get; set; }
        public int HighByteCount { get; set; }
        public int ExtendedAsciiCount { get; set; }
        public int Utf8StartByteCount { get; set; }
    }
}
