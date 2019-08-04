namespace Vibechat.Web.Services.ChatDataProviders
{
    public interface IChatDataProvider
    {
        string GetProfilePictureUrl();

        string GetGroupPictureUrl();
    }
}