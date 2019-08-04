using System.Collections.Generic;
using Vibechat.Web.ChatData.Messages;

namespace Vibechat.Web.Data.ApiModels.Files
{
    public class FilesUploadResponse
    {
        /// <summary>
        ///     Relative urls to uploaded files
        /// </summary>
        public List<MessageAttachment> UploadedFiles { get; set; }
    }
}