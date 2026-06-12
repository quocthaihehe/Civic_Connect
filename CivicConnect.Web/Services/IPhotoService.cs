using System.IO;
using System.Threading.Tasks;
using CivicConnect.Web.Models.Entities;

namespace CivicConnect.Web.Services
{
    public interface IPhotoService
    {
        Task<PhotoUploadResult> AddPhotoAsync(Stream fileStream, string fileName);
        Task<bool> DeletePhotoAsync(string publicId);
    }
}
