using Xabe.FFmpeg;

namespace MetalfluxApi.Server.Core.Service;

public static class FFmpegUtilsService
{
    public const int SegmentTime = 10; // seconds

    private static string? _tempDir;

    public static void CreateTempDir(int id)
    {
        if (_tempDir != null)
            Directory.Delete(_tempDir, true);

        _tempDir = Path.Combine(Path.GetTempPath(), $"media_{id}_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public static string CombineTempPath(string fileName)
    {
        if (_tempDir == null)
            throw new Exception("Temp directory not created");

        return Path.Combine(_tempDir, fileName);
    }

    public static async Task<(string, FileStream)> AssembleVideoSegments(
        int id,
        string fileExtension,
        List<string> segmentPaths,
        bool reencode
    )
    {
        if (_tempDir == null)
            throw new Exception("Temp directory not created");

        // Create concat list file for FFmpeg
        var listPath = Path.Combine(_tempDir, "list.txt");
        await File.WriteAllLinesAsync(
            listPath,
            segmentPaths.Select(p => $"file '{p.Replace("\\", "/")}'")
        );

        // Final file path
        var finalPath = Path.Combine(_tempDir, $"output_{id}.{fileExtension}");

        // Two FFmpeg command if reencodage is needed or not
        var ffmpegParams = reencode
            ? $"-f concat -safe 0 -i \"{listPath}\" -c:v libx264 -preset veryfast -crf 23 -c:a aac -b:a 192k -movflags +faststart \"{finalPath}\""
            : $"-f concat -safe 0 -i \"{listPath}\" -c copy -fflags +genpts \"{finalPath}\"";

        var conversion = FFmpeg
            .Conversions.New()
            .AddParameter(ffmpegParams, ParameterPosition.PreInput);

        await conversion.Start();

        // Async file cleaning
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMinutes(2)); // Let time for ASP.NET to send response
            try
            {
                Directory.Delete(_tempDir, true);
                _tempDir = null;
            }
            catch
            { /* ignore errors here */
            }
        });

        // Create stream for file repsonse
        return (
            finalPath,
            new FileStream(finalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
        );
    }
}
