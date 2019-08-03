using Vibechat.Web.Data_Layer.DataModels;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public interface ILastMessagesRepository
    {
        void Add(string userId, int chatId, int msgId);
        LastMessageDataModel Get(string userId, int chatId);
        void Remove(LastMessageDataModel entry);

        void Update(LastMessageDataModel entry);
    }
}