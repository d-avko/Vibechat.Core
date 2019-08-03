using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.ChatData.Messages;
using Vibechat.Web.Data.ApiModels.Files;
using Vibechat.Web.Services.FileSystem;
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

            string Error = string.Empty;
            string thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

            var errorLock = new object();
            var resultLock = new object();

            Parallel.ForEach(request.images, file => 
            {
                try
                {
                    using (var buffer = new MemoryStream())
                    {
                        file.CopyTo(buffer);
                        buffer.Seek(0, SeekOrigin.Begin);
                        var uploadedFile = filesService.SaveMessagePicture(file, buffer, file.FileName, request.ChatId, thisUserId).GetAwaiter().GetResult();

                        lock (resultLock)
                        {
                            result.UploadedFiles.Add(uploadedFile);
                        }
                    }   
                }
                catch (Exception ex)
                {
                    lock (errorLock)
                    {
                        Error = "Some of the files failed to upload. Exception type for last file was: " + ex.GetType().ToString();
                    }
                }
            });

            return new ResponseApiModel<FilesUploadResponse>()
            {
                ErrorMessage = Error == string.Empty ? null : Error,
                IsSuccessfull = Error == string.Empty,
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

            try
            {
                using (var buffer = new MemoryStream())
                {
                    await request.file.CopyToAsync(buffer);
                    buffer.Seek(0, SeekOrigin.Begin);

                    MessageAttachment savedFile = await filesService.SaveMessageFile(request.file, buffer, request.file.FileName, request.ChatId, thisUserId);

                    return new ResponseApiModel<MessageAttachment>()
                    {
                        IsSuccessfull = true,
                        Response = savedFile
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<MessageAttachment>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = "Failed to upload. Exception type: " + ex.GetType().Name
                };
            }
        }

    }
}
