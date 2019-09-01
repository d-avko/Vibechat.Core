using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.BusinessLogic.Services.FileSystem;
using Vibechat.Shared.ApiModels;
using Vibechat.Shared.ApiModels.Files;
using Vibechat.Shared.DTO.Messages;

namespace Vibechat.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        public static int MaxImageLengthMB = 5;

        public static int MaxFileLengthMB = 25;

        public FilesController(FilesService imagesService)
        {
            filesService = imagesService;
        }

        public FilesService filesService { get; }

        [Authorize(Policy = "PublicApi")]
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> UploadImages([FromForm] UploadImagesRequest request)
        {
            var result = new FilesUploadResponse {UploadedFiles = new List<MessageAttachment>()};

            if (!request.images.Any())
            {
                return BadRequest(new ResponseApiModel<FilesUploadResponse>
                {
                    ErrorMessage = "No files were provided.",
                    IsSuccessfull = false,
                    Response = null
                });
            }

            foreach (var image in request.images)
            {
                if (image.Length > 1024 * 1024 * MaxImageLengthMB)
                {
                    return BadRequest(new ResponseApiModel<FilesUploadResponse>
                    {
                        ErrorMessage = $"Some of the files were larger than {MaxImageLengthMB} Mb",
                        IsSuccessfull = false
                    });
                }
            }

            var error = string.Empty;
            var thisUserId = ClaimsExtractor.GetUserIdClaim(User.Claims);

            var errorLock = new object();
            var resultLock = new object();

            Parallel.ForEach(request.images, file =>
            {
                try
                {
                    var uploadedFile = filesService
                        .SaveMessagePicture(file, file.FileName, request.ChatId, thisUserId).GetAwaiter()
                        .GetResult();

                    lock (resultLock)
                    {
                        result.UploadedFiles.Add(uploadedFile);
                    }
                }
                catch (Exception ex)
                {
                    lock (errorLock)
                    {
                        error = "Some of the files failed to upload. Exception type for last file was: " +
                                ex.GetType();
                    }
                }
            });

            return Ok(new ResponseApiModel<FilesUploadResponse>
            {
                ErrorMessage = error == string.Empty ? null : error,
                IsSuccessfull = error == string.Empty,
                Response = result
            });
        }

        [Authorize(Policy = "PublicApi")]
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
        {
            if (request.file.Length > 1024 * 1024 * MaxFileLengthMB)
            {
                return BadRequest(new ResponseApiModel<MessageAttachment>
                {
                    ErrorMessage = $"File was larger than {MaxFileLengthMB} Mb",
                    IsSuccessfull = false
                });
            }

            var thisUserId = ClaimsExtractor.GetUserIdClaim(User.Claims);

            try
            {
                var savedFile = await filesService.SaveMessageFile(request.file, request.file.FileName,
                    request.ChatId, thisUserId);

                return Ok(new ResponseApiModel<MessageAttachment>
                {
                    IsSuccessfull = true,
                    Response = savedFile
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseApiModel<MessageAttachment>
                {
                    IsSuccessfull = false,
                    ErrorMessage = "Failed to upload. Exception type: " + ex.GetType().Name
                });
            }
        }

        public class UploadImagesRequest
        {
            public List<IFormFile> images { get; set; }

            public string ChatId { get; set; }
        }

        public class UploadFileRequest
        {
            public IFormFile file { get; set; }

            public string ChatId { get; set; }
        }
    }
}