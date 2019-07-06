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
using VibeChat.Web;

namespace Vibechat.Web.Controllers
{
    public class FilesController : Controller
    {
        public FilesService filesService { get; }

        public static int MaxImageLengthMB = 5;

        public static int MaxFileLengthMB = 25;

        public FilesController(FilesService imagesService)
        {
            filesService = imagesService;
        }

        public class UploadImagesRequest
        {
            [FromForm(Name = "images")]
            public List<IFormFile> images { get; set; }

            [FromForm(Name = "ChatId")]
            public string ChatId { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("Files/UploadImages")]
        public async Task<ResponseApiModel<FilesUploadResponse>> UploadImages([FromForm] UploadImagesRequest request)
        {
            var result = new FilesUploadResponse() { UploadedFiles = new List<MessageAttachment>() };

            if (request.images.Count() == 0)
            {
                return new ResponseApiModel<FilesUploadResponse>()
                {
                    ErrorMessage = "No files were provided.",
                    IsSuccessfull = false,
                    Response = null
                };
            }

            foreach (IFormFile image in request.images)
            {
                if(image.Length > 1024 * 1024 * MaxImageLengthMB)
                {
                    return new ResponseApiModel<FilesUploadResponse>()
                    {
                        ErrorMessage = $"Some of the files was larger than {MaxImageLengthMB} Mb",
                        IsSuccessfull = false
                    };
                }
            }

            string Errors = string.Empty;
            string thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

            foreach (var file in request.images)
            {
                try
                {
                    using (var buffer = new MemoryStream())
                    {
                        await file.CopyToAsync(buffer);
                        buffer.Seek(0, SeekOrigin.Begin);

                        result.UploadedFiles.Add(filesService.SaveMessagePicture(buffer, file.FileName, request.ChatId, thisUserId));
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

        public class UploadFileRequest
        {
            [FromForm(Name = "file")]
            public IFormFile file { get; set; }

            [FromForm(Name = "ChatId")]
            public string ChatId { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("Files/UploadFile")]
        public async Task<ResponseApiModel<MessageAttachment>> UploadFile([FromForm] UploadFileRequest request)
        {
            if (request.file.Length > 1024 * 1024 * MaxFileLengthMB)
            {
                return new ResponseApiModel<MessageAttachment>()
                {
                    ErrorMessage = $"File was larger than {MaxFileLengthMB} Mb",
                    IsSuccessfull = false
                };
            }

            string thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

            using (var buffer = new MemoryStream())
            {
                await request.file.CopyToAsync(buffer);
                buffer.Seek(0, SeekOrigin.Begin);

                var savedFile = filesService.SaveFile(buffer, request.file.FileName, request.ChatId, thisUserId);

                return new ResponseApiModel<MessageAttachment>()
                {
                    IsSuccessfull = true,
                    Response = savedFile
                };
            }
        }

    }
}
