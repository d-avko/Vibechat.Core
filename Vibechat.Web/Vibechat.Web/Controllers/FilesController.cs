using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.ApiModels.Files;
using Vibechat.Web.Services.Images;

namespace Vibechat.Web.Controllers
{
    public class FilesController : Controller
    {
        private IImageCompressionService imageCompression { get; set; }
        public FilesController(IImageCompressionService imageCompression)
        {
            this.imageCompression = imageCompression;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("Files/Upload")]
        public Task<FilesUploadResponse> UploadFiles(List<IFormFile> files)
        {
            throw new NotImplementedException();
        }
    }
}
