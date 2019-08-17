using System.Collections.Generic;
using System.Threading.Tasks;
using VibeChat.Web;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Data.Repositories
{
    public interface IChatRolesRepository
    {
        void Add(int chatId, string userId, ChatRole role);

        void Add(ConversationDataModel chat, string userId, ChatRole role);
        Task<ChatRoleDataModel> GetAsync(int chatId, string userId);

        Task<List<ChatRoleDataModel>> GetAsync(string userId);

        void Remove(ChatRoleDataModel chatRole);
        void Update(ChatRole role, ChatRoleDataModel chatRole);
    }
}