using System.Collections.Generic;
using Vibechat.Shared.DTO.Messages;

namespace Vibechat.Shared.ApiModels.Files
{
    public class FilesUploadResponse
    {
        /// <summary>
        ///     Relative urls to uploaded files
        /// </summary>
        public List<MessageAttachment> UploadedFiles { get; set; }
    }
}