using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Galaxium.API.Entities;
using Galaxium.API.Repository.Interfaces;
using Galaxium.Api.Services.Interfaces;

namespace Galaxium.Api.Services.service
{
    public class ProductPhotoService : IProductPhotoService
{
    private readonly Cloudinary _cloudinary;
    private readonly IProductPhotoRepository _photoRepo;

    public ProductPhotoService(
        Cloudinary cloudinary,
        IProductPhotoRepository photoRepo)
    {
        _cloudinary = cloudinary;
        _photoRepo = photoRepo;
    }

    public async Task UploadAsync(int productId, IFormFile file, bool isPrimary)
    {
        if (file == null || file.Length == 0)
            throw new Exception("Archivo inválido");

        var count = await _photoRepo.CountByProductAsync(productId);
        if (count >= 4)
            throw new Exception("Un producto no puede tener más de 4 imágenes");

        using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = $"products/{productId}",
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception(result.Error.Message);

        var photo = new ProductPhoto
        {
            ProductId = productId,
            PhotoUrl = result.SecureUrl.ToString(),
            IsPrimary = isPrimary
        };

        await _photoRepo.AddAsync(photo);
    }
}
}