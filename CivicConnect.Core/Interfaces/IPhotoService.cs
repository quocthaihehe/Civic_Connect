using System.IO;
using System.Threading.Tasks;
using CivicConnect.Core.Entities;

namespace CivicConnect.Core.Interfaces
{
    public interface IPhotoService
    {
        Task<PhotoUploadResult> AddPhotoAsync(Stream fileStream, string fileName);
        Task<bool> DeletePhotoAsync(string publicId);
    }
}
