using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary? _cloudinary;
        private readonly bool _isMock;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
            if (string.IsNullOrWhiteSpace(config.Value.CloudName) ||
                string.IsNullOrWhiteSpace(config.Value.ApiKey) ||
                string.IsNullOrWhiteSpace(config.Value.ApiSecret))
            {
                _isMock = true;
                return;
            }

            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            var handler = new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            var httpClient = new System.Net.Http.HttpClient(handler);

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Client = httpClient;
        }

        public async Task<PhotoUploadResult> AddPhotoAsync(Stream fileStream, string fileName)
        {
            if (fileStream.Length == 0)
            {
                throw new ArgumentException("File stream is empty.");
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            bool isHeic = extension == ".heic" || extension == ".heif";

            Stream processedStream = fileStream;
            string targetFileName = fileName;
            MemoryStream? tempMs = null;

            try
            {
                if (isHeic)
                {
                    tempMs = new MemoryStream();
                    using (var image = new ImageMagick.MagickImage(fileStream))
                    {
                        image.Format = ImageMagick.MagickFormat.Jpeg;
                        await Task.Run(() => image.Write(tempMs));
                    }
                    tempMs.Position = 0;
                    processedStream = tempMs;
                    targetFileName = Path.ChangeExtension(fileName, ".jpg");
                }

                if (_isMock)
                {
                    // Lưu file cục bộ trong thư mục wwwroot/uploads
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(targetFileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var file = new FileStream(filePath, FileMode.Create))
                    {
                        await processedStream.CopyToAsync(file);
                    }

                    var relativeUrl = "/uploads/" + uniqueFileName;
                    return new PhotoUploadResult(relativeUrl, relativeUrl, uniqueFileName);
                }

                if (_cloudinary == null)
                {
                    throw new InvalidOperationException("Cloudinary is not configured.");
                }

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(targetFileName, processedStream),
                    Transformation = new Transformation().Width(800).Height(800).Crop("fill")
                        .FetchFormat("auto") // f_auto
                        .Quality("auto")     // q_auto
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
                }

                var thumbnailUrl = _cloudinary.Api.UrlImgUp
                    .Transform(new Transformation().Width(400).Height(400).Crop("fill").FetchFormat("auto").Quality("auto"))
                    .BuildUrl(uploadResult.PublicId);

                return new PhotoUploadResult(uploadResult.SecureUrl.ToString(), thumbnailUrl, uploadResult.PublicId);
            }
            finally
            {
                if (tempMs != null)
                {
                    await tempMs.DisposeAsync();
                }
            }
        }

        public async Task<bool> DeletePhotoAsync(string publicId)
        {
            if (_isMock)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", publicId);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return true;
                }
                return false;
            }

            if (_cloudinary == null)
            {
                return false;
            }

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }
    }
}
