using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Vibechat.FileServer
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<FilesController> logger;
        private readonly string thisServerUrl;
            
        public FilesController(IConfiguration configuration, ILogger<FilesController> logger)
        {
            this.logger = logger;
            contentPath = configuration["ContentPath"];
            thisServerUrl = configuration["ServerUrl"];
        }

        private string contentPath { get; }

        [HttpPost]
        [Route("[action]")]
        public bool Upload([FromForm] UploadFileRequest request)
        {
            try
            {
                logger.LogInformation($"Got request to upload file, {request.Path}, saving to {contentPath + Path.GetDirectoryName(request.Path)}");
                
                request.Path = request.Path.Replace('\\', '/');

                Directory.CreateDirectory(contentPath + Path.GetDirectoryName(request.Path));

                using (var fs = new FileStream(contentPath + request.Path, FileMode.Create))
                {
                    request.file.CopyTo(fs);
                }

                return true;
            }
            catch
            {
                logger.LogError($"Someone failed to upload a file. Path was: {request.Path}");
                
                if (System.IO.File.Exists(contentPath + request.Path))
                {
                    System.IO.File.Delete(contentPath + request.Path);
                }

                return false;
            }
        }

        [HttpDelete]
        [Route("[action]/{path}")]
        public bool Delete(string path)
        {
            try
            {
                logger.LogInformation($"Got request to delete file: {path}");

                if (path.StartsWith(thisServerUrl))
                {
                    path = path.Replace(thisServerUrl, string.Empty);
                }
                
                System.IO.File.Delete(path);
                return true;
            }
            catch
            {
                logger.LogError($"Someone failed to delete a file. Path was: {path}");
                
                return false;
            }
        }
        

        public class UploadFileRequest
        {
            [FromForm(Name = "file")] public IFormFile file { get; set; }

            [FromForm(Name = "path")] public string Path { get; set; }
        }
    }
}