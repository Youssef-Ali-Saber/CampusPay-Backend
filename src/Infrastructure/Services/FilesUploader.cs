using Microsoft.AspNetCore.Http;
namespace Infrastructure.Services;
public class FilesUploader(IHttpContextAccessor httpContextAccessor)
{
    public async Task<string> UploadPictureAsync(IFormFile picture)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(picture.FileName);
        var localPath = "Images/Picture/" + $"{fileName}";
        var linkOfPicture = string.Empty;
        using (var fileStream = new FileStream(localPath, FileMode.Create))
        {
            await picture.CopyToAsync(fileStream);
            linkOfPicture = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Images/Picture/{fileName}";
        }
        return linkOfPicture;
    }

    public async Task<string> UploadIconAsync(IFormFile icon)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(icon.FileName);
        var localPath = "Images/Icon/" + $"{fileName}";
        var linkOfIcon = string.Empty;
        using (var fileStream = new FileStream(localPath, FileMode.Create))
        {
            await icon.CopyToAsync(fileStream);
            linkOfIcon = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Images/Icon/{fileName}";
        }
        return linkOfIcon;

    }

    public Dictionary<string, string> ValidateFile(IFormFile formFile)
    {
        var allowExtensions = new List<string> { ".jpg", ".png", ".jpeg" };
        var fileExtension = Path.GetExtension(formFile.FileName);
        var maxFileSizeInBytes = 10485760;
        Dictionary<string, string> errors = new Dictionary<string, string>();
        if (!allowExtensions.Contains(fileExtension))
        {
            errors["file"] = "Invalid file extension";
        }
        if (formFile.Length > maxFileSizeInBytes)
        {
            errors["file"] = "File size is too large";
        }
        return errors;
    }
}

