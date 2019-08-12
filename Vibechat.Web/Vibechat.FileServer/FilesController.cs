using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Vibechat.FileServer
{
    public class FilesController : Controller
    {
        private readonly IConfiguration configuration;

        public FilesController(IConfiguration configuration)
        {
            this.configuration = configuration;
            contentPath = configuration["ContentPath"];
        }

        private string contentPath { get; }

        [Route("api/UploadFile")]
        public bool UploadFile([FromForm] UploadFileRequest request)
        {
            try
            {
                request.Path = request.Path.Replace('\\', '/');

                using (var stream = new MemoryStream())
                {
                    Directory.CreateDirectory(contentPath + Path.GetDirectoryName(request.Path));

                    using (var fs = new FileStream(contentPath + request.Path, FileMode.Create))
                    {
                        request.file.CopyTo(fs);
                    }
                }

                return true;
            }
            catch
            {
                if (System.IO.File.Exists(contentPath + request.Path))
                {
                    System.IO.File.Delete(contentPath + request.Path);
                }

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