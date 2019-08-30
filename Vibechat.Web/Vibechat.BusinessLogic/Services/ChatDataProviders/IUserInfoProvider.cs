namespace Vibechat.BusinessLogic.Services.ChatDataProviders
{
    public interface IChatDataProvider
    {
        string GetProfilePictureUrl();

        string GetGroupPictureUrl();
    }
}