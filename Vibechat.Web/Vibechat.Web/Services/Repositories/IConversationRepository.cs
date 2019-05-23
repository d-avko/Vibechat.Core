using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IConversationRepository
    {
        Task<ConversationDataModel> Add(bool IsGroup, string name, string imageUrl, UserInApplication user,bool IsPublic);
        ConversationDataModel GetById(int id);

        void Remove(ConversationDataModel entity);

        void UpdateThumbnail(string thumbnailUrl, string fullimageUrl, ConversationDataModel entity);

        void ChangeName(ConversationDataModel entity, string name);

        Task<IQueryable<ConversationDataModel>> SearchByName(string name);
    }
}