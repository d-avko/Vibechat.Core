using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using VibeChat.Web;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Services.Repositories
{
    public class MessagesRepository : IMessagesRepository
    {
        private ApplicationDbContext mContext { get; set; }


        public MessagesRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public async Task<MessageDataModel> Add(UserInApplication whoSent, Message message, int groupId)
        {
            var addedMessage = mContext.Messages.Add(new MessageDataModel()
            {
                ConversationID = groupId,
                MessageContent = message.MessageContent,
                TimeReceived = DateTime.UtcNow,
                User = whoSent,
                AttachmentInfo = null,
                IsAttachment = false
            });

            await mContext.SaveChangesAsync();

            return addedMessage.Entity;
        }


        public async Task<MessageDataModel> AddAttachment(
            UserInApplication whoSent,
            MessageAttachmentDataModel attachment,
            Message message,
            int groupId,
            string SenderId)
        {
            var addedMessage = mContext.Messages.Add(new MessageDataModel()
            {
                ConversationID = groupId,
                MessageContent = message.MessageContent,
                TimeReceived = DateTime.UtcNow,
                User = whoSent,
                AttachmentInfo = attachment,
                IsAttachment = true
            });

            await mContext.SaveChangesAsync();

            return addedMessage.Entity;
        }

        public async Task Remove(List<int> messagesIds, string whoRemovedId)
        {
            await mContext.DeletedMessages.AddRangeAsync(
                messagesIds
                .Select(msgId => new DeletedMessagesDataModel()
                {
                    UserId = whoRemovedId,
                    Message = mContext.Messages.First(msg => msg.MessageID == msgId)
                }));

            await mContext.SaveChangesAsync();
        }

        public IQueryable<MessageDataModel> GetMessagesByIds(List<int> ids)
        {
            return mContext.Messages.Where(msg => ids.Any(id => id == msg.MessageID));
        }

        public IIncludableQueryable<MessageDataModel, MessageAttachmentDataModel> GetMessagesForConversation(
            string userId,
            int conversationId,
            int offset,
            int count)
        {
            var deletedMessages = mContext
            .DeletedMessages
            .Where(msg => msg.Message.ConversationID == conversationId && msg.UserId == userId);

            return mContext
                .Messages
                .Where(msg => msg.ConversationID == conversationId)
                .Where(msg => !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID))
                .OrderByDescending(x => x.TimeReceived)
                .Skip(offset)
                .Take(count)
                .Include(msg => msg.User)
                .Include(msg => msg.AttachmentInfo);
        }

        public bool Empty()
        {
            return mContext.Messages.FirstOrDefault() == null;
        }

    }
}
