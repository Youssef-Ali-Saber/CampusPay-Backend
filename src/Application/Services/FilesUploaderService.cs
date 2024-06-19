using Microsoft.AspNetCore.Http;
namespace Application.Services;
public class FilesUploaderService(IHttpContextAccessor httpContextAccessor)
{

    public async Task<string> UploadImageAsync(IFormFile icon, string path)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(icon.FileName);
        var localPath = "Images/Icon/" + $"{fileName}";
        var linkOfImage = string.Empty;
        using (var fileStream = new FileStream(localPath, FileMode.Create))
        {
            await icon.CopyToAsync(fileStream);
            linkOfImage = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/{path}{fileName}";
        }
        return linkOfImage;

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

