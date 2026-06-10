using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CivicConnect.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CivicConnect.Infrastructure.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary? _cloudinary;
        private readonly string _webRootPath;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            // Kiểm tra cấu hình Cloudinary thực tế (tránh dùng giá trị placeholder)
            if (!string.IsNullOrEmpty(cloudName) && 
                !string.IsNullOrEmpty(apiKey) && 
                !string.IsNullOrEmpty(apiSecret) &&
                !cloudName.Contains("your-cloud") &&
                !apiKey.Contains("your-api"))
            {
                var account = new Account(cloudName, apiKey, apiSecret);
                _cloudinary = new Cloudinary(account);
            }

            // Thiết lập thư mục fallback wwwroot
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _webRootPath = configuration["WebRootPath"] 
                ?? Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "CivicConnect.Web", "wwwroot"));

            // Nếu không tìm thấy, fallback về thư mục wwwroot cục bộ của tiến trình chạy
            if (!Directory.Exists(_webRootPath))
            {
                _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }
        }

        public async Task<(string Url, string ThumbnailUrl, string PublicId)> UploadIssueImageAsync(
            Stream fileStream, string fileName, string contentType, int issueId)
        {
            // Validate định dạng tệp ảnh
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            var isAllowed = false;
            foreach (var type in allowedTypes)
            {
                if (contentType.Equals(type, StringComparison.OrdinalIgnoreCase))
                {
                    isAllowed = true;
                    break;
                }
            }
            if (!isAllowed && !fileName.EndsWith(".jpg") && !fileName.EndsWith(".png") && !fileName.EndsWith(".webp"))
            {
                throw new InvalidOperationException("Chỉ chấp nhận JPG, PNG, WebP");
            }

            if (_cloudinary != null)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, fileStream),
                    Folder = $"civicconnect/issues/{issueId}",
                    PublicId = $"{issueId}_{Guid.NewGuid():N}",
                    Transformation = new Transformation()
                        .Width(1280).Height(960).Crop("limit")
                        .Quality("auto:good"),
                    Tags = $"issue,issue_{issueId}"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                {
                    throw new Exception($"Lỗi upload Cloudinary: {uploadResult.Error.Message}");
                }

                var url = uploadResult.SecureUrl.ToString();
                var publicId = uploadResult.PublicId;

                var thumbnailUrl = _cloudinary.Api.UrlImgUp
                    .Transform(new Transformation().Width(400).Height(300).Crop("fill"))
                    .BuildUrl(publicId);

                return (url, thumbnailUrl, publicId);
            }
            else
            {
                // Fallback lưu cục bộ tại local disk (wwwroot/uploads/issues/{issueId}/)
                var relativeFolder = Path.Combine("uploads", "issues", issueId.ToString());
                var absoluteFolder = Path.Combine(_webRootPath, relativeFolder);

                if (!Directory.Exists(absoluteFolder))
                {
                    Directory.CreateDirectory(absoluteFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid():N}_{fileName}";
                var absoluteFilePath = Path.Combine(absoluteFolder, uniqueFileName);

                using (var destStream = new FileStream(absoluteFilePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(destStream);
                }

                var relativeFilePath = Path.Combine("/", relativeFolder, uniqueFileName).Replace("\\", "/");
                return (relativeFilePath, relativeFilePath, $"local_{uniqueFileName}");
            }
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (_cloudinary != null && !publicId.StartsWith("local_"))
            {
                var deletionParams = new DeletionParams(publicId);
                await _cloudinary.DestroyAsync(deletionParams);
            }
            else
            {
                // Xử lý xóa tệp local nếu có
                try
                {
                    var fileName = publicId.Replace("local_", "");
                    // Tìm file trong tất cả thư mục con của uploads/issues
                    var searchPath = Path.Combine(_webRootPath, "uploads", "issues");
                    if (Directory.Exists(searchPath))
                    {
                        var files = Directory.GetFiles(searchPath, fileName, SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            File.Delete(file);
                        }
                    }
                }
                catch
                {
                    // Bỏ qua lỗi
                }
            }
        }

        public async Task DeleteIssueImagesAsync(int issueId)
        {
            if (_cloudinary != null)
            {
                await _cloudinary.DeleteResourcesByTagAsync($"issue_{issueId}");
            }
            else
            {
                try
                {
                    var relativeFolder = Path.Combine("uploads", "issues", issueId.ToString());
                    var absoluteFolder = Path.Combine(_webRootPath, relativeFolder);
                    if (Directory.Exists(absoluteFolder))
                    {
                        Directory.Delete(absoluteFolder, true);
                    }
                }
                catch
                {
                    // Bỏ qua lỗi
                }
            }
        }
    }
}
