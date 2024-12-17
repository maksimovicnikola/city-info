using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers
{
    [Route("api/v{version:apiVersion}/files")]
    [ApiController]
    [ApiVersion(0.1)]
    public class FilesController : ControllerBase
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;
        // We are injecting the FileExtensionContentTypeProvider to get the content type of the file
        // This will work since we injected in Program.cs FileExtensionContentTypeProvider as a Singletone
        public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider ?? throw new System.ArgumentNullException(nameof(fileExtensionContentTypeProvider));
        }

        [HttpGet("{fileId}")]
        public ActionResult GetFile(string fileId)
        {
            var pathToFile = "certificate.pdf";

            if (!System.IO.File.Exists(pathToFile)) 
            {
                return NotFound();
            }

            // Get the content type of the file
            if (!_fileExtensionContentTypeProvider.TryGetContentType(pathToFile, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var bytes = System.IO.File.ReadAllBytes(pathToFile);
            return File(bytes, contentType, Path.GetFileName(pathToFile));
        }

        [HttpPost]
        public async Task<ActionResult> PostFile(IFormFile file)
        {
            // Validate the input; Put a limit on filesize to avoid large uploads attacks
            // Only accept .pdf files (check content-type)
            if (file.Length == 0 || file.Length < 20971520 || file.ContentType != "application/pdf")
            {
                return BadRequest("No file or an invalid one has been inputed");
            }
            
            // Create the file path; Avoud using file.FileName, as an attacker can provide malicious one, including full paths or relative paths
            var path = Path.Combine(Directory.GetCurrentDirectory(), $"uploaded_file_{Guid.NewGuid()}.pdf");

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            
            return Ok("Your file has been uploaded successfully");
        }
    }
}
