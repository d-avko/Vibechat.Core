using System;
using System.Collections.Generic;
using System.Text;

namespace Vibechat.Web.Data.ApiModels.Files
{
    public class FilesUploadResponse
    {
        /// <summary>
        /// Relative urls to uploaded files
        /// </summary>
        public List<string> UploadedFilesUrls { get; set; }
    }
}
