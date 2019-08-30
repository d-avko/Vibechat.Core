using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Vibechat.DataLayer.DataModels;
using Vibechat.Shared.DTO.Messages;

namespace Vibechat.DataLayer.Repositories
{
    public class MessagesRepository : BaseRepository<MessageDataModel>, IMessagesRepository
    {
        public MessagesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
           
        }

        public List<MessageDataModel> Search
            (int offset, int count, string searchString, string userId)
        {
            var Base = _dbContext
               .Messages
               .Where(msg => !_dbContext.DeletedMessages.Any(deleted =>
                   deleted.UserId == userId && deleted.MessageID == msg.MessageID))
               .Where(msg => _dbContext.UsersConversations.Any(chat =>
                   chat.ChatID == msg.ConversationID && chat.UserID == userId));

            var forwardedMessages =
                Base
                .Where(msg => msg.Type == MessageType.Forwarded && msg.ForwardedMessage.Type == MessageType.Text)
                .Where(msg =>
                    EF.Functions.Like(msg.ForwardedMessage.MessageContent.ToLower(), $"%{searchString.ToLower()}%"))
                .Include(x => x.User)
                .Include(x => x.ForwardedMessage).ToList();

            var notForwarded =
                Base
                .Where(msg => msg.Type == MessageType.Text)
                .Where(msg =>
                    EF.Functions.Like(msg.MessageContent.ToLower(), $"%{searchString.ToLower()}%"))
                .Include(x => x.User)
                .Include(x => x.ForwardedMessage).ToList();

            return
                forwardedMessages.Concat(notForwarded)
                    .OrderByDescending(msg => msg.TimeReceived)
                    .Skip(offset)
                    .Take(count).ToList();
        }
    }
}