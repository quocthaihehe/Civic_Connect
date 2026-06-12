using CivicConnect.Web.Models;
using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Services;
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
        private readonly Cloudinary _cloudinary;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
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
            if (fileStream.Length > 0)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, fileStream),
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

            throw new ArgumentException("File stream is empty.");
        }

        public async Task<bool> DeletePhotoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }
    }
}
