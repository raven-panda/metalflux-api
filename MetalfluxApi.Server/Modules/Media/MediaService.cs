using System.Reflection.Metadata;
using Amazon.S3.Model;
using MetalfluxApi.Server.Core.Base;
using MetalfluxApi.Server.Core.Dto;
using MetalfluxApi.Server.Core.Exceptions;
using MetalfluxApi.Server.Core.Service;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.StaticFiles;
using Xabe.FFmpeg;

namespace MetalfluxApi.Server.Modules.Media;

public interface IMediaService : IService<MediaDto, MediaModel>
{
    UserSelectionResponse GetUserSelection(int userId);
    List<MediaDto> Search(CursorSearchRequestDto body, out int lastId, out bool lastItemReached);
    Task<MediaDto> UploadMedia(int id, IFormFile file);
    Task<(Stream file, string contentType, string fileName)> GetMediaStream(int id, int chunk);
    Task<(Stream file, string contentType, string fileName)> GetFullMediaStream(
        int id,
        bool reencode = true
    );
}

internal sealed class MediaService(
    IMediaRepository repository,
    S3Service s3Service,
    IConfiguration configuration
) : IMediaService
{
    public MediaDto Get(int id)
    {
        var item = repository.Get(id);
        if (item == null)
            throw new EntityNotFoundException("Media", id);

        return ToDto(item);
    }

    public UserSelectionResponse GetUserSelection(int userId)
    {
        // TODO : implement that
        var item = repository.Search(
            new CursorSearchRequestDto { NextCursor = 0, SearchQuery = "" },
            out _,
            out _
        );

        return new UserSelectionResponse(ToDto(item), ToDto(item));
    }

    public async Task<(Stream file, string contentType, string fileName)> GetMediaStream(
        int id,
        int chunk
    )
    {
        var item = repository.Get(id);
        if (item == null)
            throw new EntityNotFoundException("Media", id);

        var fileName = $"{item.Id}_segments/{chunk}.{item.FileExtension}";

        var getRequest = new GetObjectRequest
        {
            BucketName = configuration["S3:BucketName"],
            Key = fileName,
        };

        var mediaFile = await s3Service.GetObjectAsync(getRequest);
        var success = new FileExtensionContentTypeProvider().TryGetContentType(
            fileName,
            out var contentType
        );
        if (!success || contentType == null)
            throw new Exception("Could not parse content type");

        return (mediaFile, contentType, fileName);
    }

    // TODO : fix the micro-cut between segments
    public async Task<(Stream file, string contentType, string fileName)> GetFullMediaStream(
        int id,
        bool reencode = true
    )
    {
        var item = repository.Get(id);
        if (item == null)
            throw new EntityNotFoundException("Media", id);

        FFmpegUtilsService.CreateTempDir(id);

        var contentTypeProvider = new FileExtensionContentTypeProvider();
        var segmentPaths = new List<string>();
        var contentType = item.ContentType;

        // Download chunks from S3
        for (var i = 0; i < item.TotalChunks; i++)
        {
            var s3Key = $"{item.Id}_segments/{i}.{item.FileExtension}";
            var localPath = FFmpegUtilsService.CombineTempPath(
                $"segment_{i:000}.{item.FileExtension}"
            );

            var getRequest = new GetObjectRequest
            {
                BucketName = configuration["S3:BucketName"],
                Key = s3Key,
            };

            await using var mediaFile = await s3Service.GetObjectAsync(getRequest);
            await using var fs = File.Create(localPath);
            await mediaFile.CopyToAsync(fs);
            segmentPaths.Add(localPath);

            contentTypeProvider.TryGetContentType(localPath, out contentType);
        }

        var (finalPath, fileStream) = await FFmpegUtilsService.AssembleVideoSegments(
            id,
            item.FileExtension,
            segmentPaths,
            reencode
        );

        return (fileStream, contentType ?? item.ContentType, Path.GetFileName(finalPath));
    }

    public MediaDto Add(MediaDto item)
    {
        var created = ToDto(repository.Add(ToModel(item)));
        return created;
    }

    public async Task<MediaDto> UploadMedia(int id, IFormFile file)
    {
        var uploadSuccess = false;

        var item = repository.Get(id);
        if (item == null)
            throw new EntityNotFoundException("Media", id);
        if (item.HasUploadedMedia)
            throw new BadHttpRequestException(
                $"File already uploaded for media {item.Id}. Please create a new one instead."
            );

        var tempFile = Path.Combine(
            Path.GetTempPath(),
            Path.GetRandomFileName() + Path.GetExtension(file.FileName)
        );
        await using (var fs = File.Create(tempFile))
        {
            await file.CopyToAsync(fs);
        }

        var fileExtension = file.FileName.Split('.').Last();

        try
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(tempFile);
            var totalSeconds = (int)mediaInfo.Duration.TotalSeconds;
            var segmentCount = (int)
                Math.Ceiling((double)totalSeconds / FFmpegUtilsService.SegmentTime);

            for (var i = 0; i < segmentCount; i++)
            {
                var startTime = i * FFmpegUtilsService.SegmentTime;
                var segmentFile = Path.Combine(
                    Path.GetTempPath(),
                    $"segment_{i:000}.{fileExtension}"
                );

                var conversion = FFmpeg
                    .Conversions.New()
                    .AddParameter(
                        $"-i \"{tempFile}\" -ss {startTime} -t {FFmpegUtilsService.SegmentTime} -c:v libx264 -c:a aac -strict experimental \"{segmentFile}\"",
                        ParameterPosition.PreInput
                    );

                await conversion.Start();

                var fileName = $"{item.Id}_segments/{i}.{fileExtension}";
                await using var stream = File.OpenRead(segmentFile);
                var putRequest = new PutObjectRequest
                {
                    BucketName = configuration["S3:BucketName"],
                    Key = fileName,
                    InputStream = stream,
                    ContentType = file.ContentType,
                };
                await s3Service.PutObjectAsync(putRequest);

                File.Delete(segmentFile);
            }

            uploadSuccess = true;

            item.FileExtension = fileExtension;
            item.ContentType = file.ContentType;
            item.UpdatedAt = DateTime.UtcNow;
            item.TotalChunks = segmentCount;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
        item.HasUploadedMedia = uploadSuccess;
        repository.Update(item);
        return ToDto(item);
    }

    public int Remove(int id)
    {
        if (!repository.Exists(id))
            throw new EntityNotFoundException("User", id);

        return repository.Remove(id);
    }

    public MediaDto Update(MediaDto item)
    {
        var model = repository.Get(item.Id);
        if (model == null)
            throw new EntityNotFoundException("Media", item.Id);

        model.Name = item.Name;
        model.UpdatedAt = DateTime.UtcNow;
        return ToDto(repository.Update(model));
    }

    public List<MediaDto> Search(
        CursorSearchRequestDto body,
        out int lastId,
        out bool lastItemReached
    )
    {
        return ToDto(repository.Search(body, out lastId, out lastItemReached));
    }

    public MediaDto ToDto(MediaModel model)
    {
        return new MediaDto
        {
            Id = model.Id,
            Name = model.Name,
            FileExtension = model.FileExtension,
            ContentType = model.ContentType,
            HasUploadedMedia = model.HasUploadedMedia,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            TotalChunks = model.TotalChunks,
        };
    }

    private List<MediaDto> ToDto(List<MediaModel> model)
    {
        return model.Select(ToDto).ToList();
    }

    public MediaModel ToModel(MediaDto dto)
    {
        var model = repository.Get(dto.Id);

        return model
            ?? new MediaModel
            {
                Name = dto.Name,
                HasUploadedMedia = dto.HasUploadedMedia,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
            };
    }
}
