using System;

namespace Vibechat.Web.Services.ChatDataProviders
{
    public class DefaultChatDataProvider : IChatDataProvider
    {
        protected static string[] ProfilePicturesUrls =
        {
            "/Users/pic1.png",
            "/Users/pic2.png",
            "/Users/pic3.png",
            "/Users/pic4.png",
            "/Users/pic5.png"
        };


        protected static string[] GroupPicturesUrls =
        {
            "/Groups/pic1.png",
            "/Groups/pic2.png",
            "/Groups/pic3.png",
            "/Groups/pic4.png",
            "/Groups/pic5.png"
        };

        public string GetGroupPictureUrl()
        {
            return GroupPicturesUrls[new Random().Next(0, GroupPicturesUrls.Length)];
        }

        public string GetProfilePictureUrl()
        {
            return ProfilePicturesUrls[new Random().Next(0, ProfilePicturesUrls.Length)];
        }
    }
}