using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vibechat.Web.Services.Paths;

namespace Vibechat.Web.Services.FileSystem
{
    public abstract class AbstractFilesService
    {
        private const string FilesLocationRelative = "Uploads/";

        protected AbstractFilesService(UniquePathsProvider pathsProvider)
        {
            PathsProvider = pathsProvider;
        }

        private UniquePathsProvider PathsProvider { get; }

        /// <summary>
        ///     Saves file to specified / generated location
        /// </summary>
        /// <param name="file"></param>
        /// <param name="filename"></param>
        /// <param name="chatOrUserId"></param>
        /// <param name="sender"></param>
        /// <param name="additionalPathString">string to insert between filename and extension</param>
        /// <param name="folder">folder to save to. Assumes it's created</param>
        /// <returns></returns>
        protected async Task<string> SaveFile(IFormFile formFile, MemoryStream file, string filename, string chatOrUserId,
            string sender, string additionalPathString = null, string folder = null)
        {
            var builder = new StringBuilder();

            string resultPath;

            var uniquePath = GetUniquePath(sender, chatOrUserId, filename);
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
                if (folder != null)
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


            await SaveToStorage(formFile, file, resultPath);

            return resultPath;
        }

        protected  virtual Task DeleteFile(string path)
        {
            File.Delete(path);
            return Task.CompletedTask;
        }

        protected virtual async Task SaveToStorage(IFormFile formFile, MemoryStream file, string path)
        {
            using (var fStream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(fStream);
            }
        }

        private string GetUniquePath(string sender, string chatOrUserId, string name)
        {
            var first = PathsProvider.GetUniquePath(sender);
            first = first.Substring(first.Length / 2);
            var second = PathsProvider.GetUniquePath(chatOrUserId);
            second = second.Substring(second.Length / 2);
            var third = PathsProvider.GetUniquePath(name);
            third = third.Substring(third.Length / 2);

            return first + second + third;
        }
    }
}