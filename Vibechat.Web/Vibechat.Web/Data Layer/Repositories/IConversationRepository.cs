using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public interface IConversationRepository
    {
        void Add(ConversationDataModel conversation);

        ConversationDataModel GetById(int id);

        void Remove(ConversationDataModel entity);

        void UpdateThumbnail(string thumbnailUrl, string fullimageUrl, ConversationDataModel entity);

        void ChangeName(ConversationDataModel entity, string name);

        IQueryable<ConversationDataModel> SearchByName(string name);

        void ChangePublicState(ConversationDataModel chat);

        void UpdateAuthKey(ConversationDataModel chat, string authKeyId);
    }
}