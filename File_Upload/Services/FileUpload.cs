using Microsoft.AspNetCore.Components.Forms;

namespace File_Upload.Services
{
    public interface IFileUpload
    {
        Task UploadFile(IBrowserFile file);
        Task<string> GeneratePreviewUrl(IBrowserFile file);
    }

    public class FileUpload : IFileUpload
    {
        private IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileUpload> _logger;

        public FileUpload(IWebHostEnvironment webHostEnvironment, ILogger<FileUpload> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task UploadFile(IBrowserFile file)
        {
            if (file is not null)
            {
                try
                {
                    var sanitizedFileName = Path.GetFileName(file.Name);
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", sanitizedFileName);

                    // Create the uploads folder if it does not exist
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    using (var stream = file.OpenReadStream())
                    {
                        await using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }

                    _logger.LogInformation($"File uploaded successfully: {file.Name}");
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }
        }

        public async Task<string> GeneratePreviewUrl(IBrowserFile file)
        {
            if (!file.ContentType.Contains("image"))
                if (!file.ContentType.Contains("pdf"))
                    return "images/pdf_log.png";

            var resizedImage = await file.RequestImageFileAsync(file.ContentType, 100, 100);
            var buffer = new byte[resizedImage.Size];
            await resizedImage.OpenReadStream().ReadExactlyAsync(buffer);
            return $"data:{file.ContentType};base64,{Convert.ToBase64String(buffer)}";
        }
    }
}
