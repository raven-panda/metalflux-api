using System.Net;
using Amazon.S3;
using Amazon.S3.Model;

namespace MetalfluxApi.Server.Core.Service;

internal sealed class S3Service(IConfiguration configuration)
{
    private readonly IAmazonS3 _s3Client = new AmazonS3Client(
        configuration["S3:Username"],
        configuration["S3:Password"],
        new AmazonS3Config { ServiceURL = configuration["S3:Url"], ForcePathStyle = true }
    );

    public async Task PutObjectAsync(PutObjectRequest request)
    {
        await _s3Client.PutObjectAsync(request);
    }

    public async Task<Stream> GetObjectAsync(GetObjectRequest request)
    {
        try
        {
            var response = await _s3Client.GetObjectAsync(request);
            var stream = response.ResponseStream;
            return stream;
        }
        catch (AmazonS3Exception e)
        {
            throw new HttpRequestException(e.Message, e.InnerException, e.StatusCode);
        }
    }
}
