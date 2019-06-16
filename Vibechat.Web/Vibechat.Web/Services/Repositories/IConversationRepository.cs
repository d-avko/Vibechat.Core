using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IConversationRepository
    {
        Task Add(ConversationDataModel conversation);

        ConversationDataModel GetById(int id);

        void Remove(ConversationDataModel entity);

        void UpdateThumbnail(string thumbnailUrl, string fullimageUrl, ConversationDataModel entity);

        void ChangeName(ConversationDataModel entity, string name);

        Task<IQueryable<ConversationDataModel>> SearchByName(string name, UserInApplication whoSearches, IUsersConversationsRepository participantsProvider);

        Task ChangePublicState(int conversationId);

        Task UpdateAuthKey(ConversationDataModel chat, string authKeyId);
    }
}