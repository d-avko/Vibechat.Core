using System;

namespace Vibechat.Web.Services.ChatDataProviders
{
    public class DefaultChatDataProvider : IChatDataProvider
    {
        protected static string[] ProfilePicturesUrls =
        {
            "/en/assets/Users/pic1.png",
            "/en/assets/Users/pic2.png",
            "/en/assets/Users/pic3.png",
            "/en/assets/Users/pic4.png",
            "/en/assets/Users/pic5.png"
        };


        protected static string[] GroupPicturesUrls =
        {
            "/en/assets/Groups/pic1.png",
            "/en/assets/Groups/pic2.png",
            "/en/assets/Groups/pic3.png",
            "/en/assets/Groups/pic4.png",
            "/en/assets/Groups/pic5.png"
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