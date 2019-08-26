using System;

namespace Vibechat.Web.Services.ChatDataProviders
{
    public class DefaultChatDataProvider : IChatDataProvider
    {
        protected static string[] ProfilePicturesUrls =
        {
            "assets/Users/pic1.png",
            "assets/Users/pic2.png",
            "assets/Users/pic3.png",
            "assets/Users/pic4.png",
            "assets/Users/pic5.png"
        };


        protected static string[] GroupPicturesUrls =
        {
            "assets/Groups/pic1.png",
            "assets/Groups/pic2.png",
            "assets/Groups/pic3.png",
            "assets/Groups/pic4.png",
            "assets/Groups/pic5.png"
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