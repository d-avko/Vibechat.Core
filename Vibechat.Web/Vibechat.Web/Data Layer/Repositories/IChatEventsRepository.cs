using Vibechat.Web.Data_Layer.DataModels;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public interface IChatEventsRepository
    {
        ChatEventDataModel Add(ChatEventDataModel model);
    }
}