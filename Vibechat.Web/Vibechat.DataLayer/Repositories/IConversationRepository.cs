using System.Collections.Generic;
using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IConversationRepository : IAsyncRepository<ConversationDataModel>
    {
        Task<List<ConversationDataModel>> GetChatsByName(string name, int maxParticipants = 100);

        Task<ConversationDataModel> GetByIdAsync(int id, string userId, int maxParticipants = 100);
    }
}