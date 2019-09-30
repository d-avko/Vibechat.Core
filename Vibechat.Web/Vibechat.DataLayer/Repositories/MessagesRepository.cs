using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vibechat.DataLayer.DataModels;
using Vibechat.DataLayer.Repositories.Specifications.DeletedMessages;
using Vibechat.Shared.DTO.Messages;

namespace Vibechat.DataLayer.Repositories
{
    public class MessagesRepository : BaseRepository<MessageDataModel>, IMessagesRepository
    {
        private readonly IDeletedMessagesRepository deletedMessages;

        public MessagesRepository(ApplicationDbContext dbContext, IDeletedMessagesRepository deletedMessages) 
            : base(dbContext)
        {
            this.deletedMessages = deletedMessages;
        }
        
        //TODO: Refactor specifications to join deletedMessages table.

        public async Task<MessageDataModel> GetMostRecentMessage(int conversationId)
        {
            return _dbContext.Messages
                .Include(x => x.AttachmentInfo.AttachmentKind)
                .Include(x => x.User)
                .Include(x => x.ForwardedMessage.AttachmentInfo.AttachmentKind)
                .Include(x => x.ForwardedMessage.User)
                .Include(x => x.Event)
                .Include(x => x.Event.Actor)
                .Include(x => x.Event.UserInvolved)
                .Where(message => message.ConversationID == conversationId)
                .OrderByDescending(x => x.TimeReceived)
                .Take(1)
                .SingleOrDefault();
        }

        public Task<int> GetUnreadMessagesCount(int chatId, int lastMessageId, string userId)
        {
            return _dbContext.Messages
                .CountAsync(message => (message.ConversationID == chatId) 
                                       && (message.MessageID > lastMessageId));
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
                .Include(x => x.ForwardedMessage);

            var notForwarded =
                Base
                .Where(msg => msg.Type == MessageType.Text)
                .Where(msg =>
                    EF.Functions.Like(msg.MessageContent.ToLower(), $"%{searchString.ToLower()}%"))
                .Include(x => x.User)
                .Include(x => x.ForwardedMessage);

            return
                forwardedMessages.Union(notForwarded)
                    .OrderByDescending(msg => msg.TimeReceived)
                    .Skip(offset)
                    .Take(count).ToList();
        }
    }
}