using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Vibechat.FileServer
{
    public class FilesController : Controller
    {
        private readonly IConfiguration configuration;

        private string contentPath { get; set; }

        public FilesController(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.contentPath = configuration["ContentPath"];
        }

        public class UploadFileRequest
        {
            [FromForm(Name = "file")]
            public IFormFile file { get; set; }

            [FromForm(Name = "path")]
            public string Path { get; set; }
        }

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
    }
}
