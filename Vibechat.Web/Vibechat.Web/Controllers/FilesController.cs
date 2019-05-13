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
using Vibechat.Web.Services.Hashing;
using Vibechat.Web.Services.Images;
using Vibechat.Web.Services.Paths;

namespace Vibechat.Web.Controllers
{
    public class FilesController : Controller
    {
        private IImageCompressionService imageCompression { get; set; }

        private UniquePathsProvider pathsProvider { get; set; }

        private static string FilesLocationRelative = "Uploads/";

        private static string FilesLocation = "ClientApp/dist/Uploads/";

        public FilesController(IImageCompressionService imageCompression, UniquePathsProvider pathsProvider)
        {
            this.imageCompression = imageCompression;
            this.pathsProvider = pathsProvider;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("Files/UploadImages")]
        public async Task<ResponseApiModel<FilesUploadResponse>> UploadImages(List<IFormFile> files)
        {
            var result = new FilesUploadResponse() { UploadedFiles = new List<MessageAttachment>() };

            if(files.Count() == 0)
            {
                return new ResponseApiModel<FilesUploadResponse>()
                {
                    ErrorMessage = "No files were provided.",
                    IsSuccessfull = false,
                    Response = null
                };
            }
            string Errors = string.Empty;

            foreach (var file in files)
            {
                try
                {
                    using (var buffer = new MemoryStream())
                    {
                        await file.CopyToAsync(buffer);

                        buffer.Seek(0, SeekOrigin.Begin);

                        ValueTuple<Stream, int, int> resultImage = imageCompression.Resize(buffer);

                        var uniquePath = pathsProvider.GetUniquePath(file.FileName);

                        Directory.CreateDirectory(FilesLocation + uniquePath);

                        using (var fStream = new FileStream(FilesLocation + uniquePath + file.FileName, FileMode.Create))
                        {
                            resultImage.Item1.CopyTo(fStream);
                        }

                        result.UploadedFiles.Add(new MessageAttachment()
                        {
                            AttachmentKind = "img",
                            AttachmentName = file.FileName,
                            ContentUrl = FilesLocationRelative + uniquePath + file.FileName,
                            ImageHeight = resultImage.Item3,
                            ImageWidth = resultImage.Item2
                        });
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
