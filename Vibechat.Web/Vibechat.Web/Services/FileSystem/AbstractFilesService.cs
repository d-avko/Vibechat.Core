using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibechat.Web.Services.Paths;

namespace Vibechat.Web.Services.FileSystem
{
    public abstract class AbstractFilesService
    {
        protected static string FilesLocationRelative = "Uploads/";

        protected static string StaticFilesLocation = "wwwroot/";

        public AbstractFilesService(UniquePathsProvider pathsProvider)
        {
            PathsProvider = pathsProvider;
        }

        public UniquePathsProvider PathsProvider { get; }

        /// <summary>
        /// Saves file to specified / generated location
        /// </summary>
        /// <param name="file"></param>
        /// <param name="filename"></param>
        /// <param name="chatOrUserId"></param>
        /// <param name="sender"></param>
        /// <param name="additionalPathString">string to insert between filename and extension</param>
        /// <param name="folder">folder to save to. Assumes it's created</param>
        /// <returns></returns>
        public string SaveFile(MemoryStream file, string filename, string chatOrUserId, string sender, string additionalPathString = null, string folder = null)
        {
            var builder = new StringBuilder();

            string resultPath;

            var uniquePath = GetUniquePath(sender, chatOrUserId, filename);
            builder.Append(StaticFilesLocation);
            builder.Append(FilesLocationRelative);
            builder.Append(uniquePath);

            if (additionalPathString == null)
            {
                Directory.CreateDirectory(builder.ToString());

                builder.Append(filename);
                resultPath = builder.ToString();
            }
            else
            {
                //folder specified explicitly
                if(folder != null)
                {
                    builder.Clear();
                    builder.Append(folder);
                }
                else
                {
                    //folder doesn't yet exist
                    Directory.CreateDirectory(builder.ToString());
                }

                builder.Append(Path.GetFileNameWithoutExtension(filename));
                builder.Append(additionalPathString);
                builder.Append(Path.GetExtension(filename));

                resultPath = builder.ToString();
            }

            using (var fStream = new FileStream(resultPath, FileMode.Create))
            {
                file.CopyTo(fStream);
            }

            return resultPath;
        }

        public string GetUniquePath(string sender, string chatOrUserId, string name)
        {
            var first = PathsProvider.GetUniquePath(sender);
            var second = PathsProvider.GetUniquePath(chatOrUserId);
            var third = PathsProvider.GetUniquePath(name);

            return first + second + third;
        }
    }
}
