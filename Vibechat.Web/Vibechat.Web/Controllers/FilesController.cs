using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.ChatData.Messages;
using Vibechat.Web.Data.ApiModels.Files;
using Vibechat.Web.Services.FileSystem;
using Vibechat.Web.Services.Hashing;
using Vibechat.Web.Services.Images;
using Vibechat.Web.Services.Paths;

namespace Vibechat.Web.Controllers
{
    public class FilesController : Controller
    {
        public ImagesService ImagesService { get; }

        public static int MaxFileLengthMB = 5;

        public FilesController(ImagesService imagesService)
        {
            ImagesService = imagesService;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("Files/UploadImages")]
        public async Task<ResponseApiModel<FilesUploadResponse>> UploadImages(List<IFormFile> images)
        {
            var result = new FilesUploadResponse() { UploadedFiles = new List<MessageAttachment>() };

            if (images.Count() == 0)
            {
                return new ResponseApiModel<FilesUploadResponse>()
                {
                    ErrorMessage = "No files were provided.",
                    IsSuccessfull = false,
                    Response = null
                };
            }

            foreach (var image in images)
            {
                if(image.Length > MaxFileLengthMB)
                {
                    return new ResponseApiModel<FilesUploadResponse>()
                    {
                        ErrorMessage = $"Some of the files was larger than {MaxFileLengthMB}",
                        IsSuccessfull = false
                    };
                }
            }

            string Errors = string.Empty;

            foreach (var file in images)
            {
                try
                {
                    using (var buffer = new MemoryStream())
                    {
                        await file.CopyToAsync(buffer);
                        buffer.Seek(0, SeekOrigin.Begin);

                        result.UploadedFiles.Add(ImagesService.GetImageAsAttachment(buffer, file.FileName));
                    }
                }
                catch(Exception ex)
                {
                    Errors += ex.Message;
                }              
            }

            return new ResponseApiModel<FilesUploadResponse>()
            {
                ErrorMessage = Errors == string.Empty ? null : Errors,
                IsSuccessfull = Errors == string.Empty,
                Response = result
            };
        }

    }
}
