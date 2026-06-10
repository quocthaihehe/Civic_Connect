using System.IO;
using System.Threading.Tasks;

namespace CivicConnect.Core.Interfaces
{
    public interface ICloudinaryService
    {
        Task<(string Url, string ThumbnailUrl, string PublicId)> UploadIssueImageAsync(Stream fileStream, string fileName, string contentType, int issueId);
        Task DeleteImageAsync(string publicId);
        Task DeleteIssueImagesAsync(int issueId);
    }
}
