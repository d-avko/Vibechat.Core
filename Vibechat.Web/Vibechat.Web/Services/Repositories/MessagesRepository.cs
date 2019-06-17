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

        public async Task<MessageDataModel> Add(UserInApplication whoSent, Message message, int groupId, MessageDataModel forwardedMessage)
        {
            var addedMessage = mContext.Messages.Add(new MessageDataModel()
            {
                ConversationID = groupId,
                MessageContent = message.MessageContent,
                TimeReceived = DateTime.UtcNow,
                User = whoSent,
                IsAttachment = false,
                ForwardedMessage = forwardedMessage,
                State = MessageState.Delivered
            });

            await mContext.SaveChangesAsync();

            return addedMessage.Entity;
        }

        public async Task<MessageDataModel> AddSecureMessage(UserInApplication whoSent, string message, int groupId)
        {
            var addedMessage = mContext.Messages.Add(new MessageDataModel()
            {
                ConversationID = groupId,
                TimeReceived = DateTime.UtcNow,
                User = whoSent,
                IsAttachment = false,
                State = MessageState.Delivered,
                EncryptedPayload = message
            });

            await mContext.SaveChangesAsync();

            return addedMessage.Entity;
        }

        public async Task<MessageDataModel> AddAttachment(
            UserInApplication whoSent,
            MessageAttachmentDataModel attachment,
            Message message,
            int groupId)
        {
            var addedMessage = mContext.Messages.Add(new MessageDataModel()
            {
                ConversationID = groupId,
                MessageContent = message.MessageContent,
                TimeReceived = DateTime.UtcNow,
                User = whoSent,
                AttachmentInfo = attachment,
                IsAttachment = true,
                State = MessageState.Delivered
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

        public async Task RemovePermanent(List<MessageDataModel> messages)
        {
            mContext.Messages.RemoveRange(messages);
            await mContext.SaveChangesAsync();
        }

        public IQueryable<MessageDataModel> GetByIds(List<int> ids)
        {
            return mContext.Messages.Where(msg => ids.Any(id => id == msg.MessageID));
        }

        public MessageDataModel GetById(int id)
        {
            return mContext.Messages
                .Include(x => x.User)
                .FirstOrDefault(x => x.MessageID == id);
        }

        public void MarkAsRead(MessageDataModel message)
        {
            message.State = MessageState.Read;
            mContext.SaveChanges();
        }

        public IIncludableQueryable<MessageDataModel, MessageAttachmentDataModel> Get(
            string userId,
            int conversationId,
            bool AllMessages = false, 
            int offset = 0, 
            int count = 0)
        {
            var deletedMessages = mContext
            .DeletedMessages
            .Where(msg => msg.Message.ConversationID == conversationId && msg.UserId == userId);

            if (AllMessages)
            {
                return mContext
                       .Messages
                       .Where(msg => msg.ConversationID == conversationId)
                       .Where(msg => !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID))
                       .Include(msg => msg.User)
                       .Include(msg => msg.AttachmentInfo);
            }
                    
           return mContext
                .Messages
                .Where(msg => msg.ConversationID == conversationId && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID))
                .OrderByDescending(x => x.TimeReceived)
                .Skip(offset)
                .Take(count)
                .Include(msg => msg.User)
                .Include(msg => msg.AttachmentInfo);
        }

        public IIncludableQueryable<MessageDataModel, MessageAttachmentDataModel> GetAttachments(
            string userId, 
            int conversationId, 
            string attachmentKind,
            int offset,
            int count)
        {
            var deletedMessages = mContext
              .DeletedMessages
              .Where(msg => msg.Message.ConversationID == conversationId && msg.UserId == userId);

            return mContext
                .Messages
                .Where(msg => 
                msg.ConversationID == conversationId 
                && msg.IsAttachment 
                && msg.AttachmentInfo.AttachmentKind.Name == attachmentKind 
                && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID))
                .OrderByDescending(x => x.TimeReceived)
                .Skip(offset)
                .Take(count)
                .Include(msg => msg.User)
                .Include(msg => msg.AttachmentInfo);
        }
        
        public int GetUnreadAmount(int conversationId, string userId)
        {
            var deletedMessages = mContext
             .DeletedMessages
             .Where(msg => msg.Message.ConversationID == conversationId && msg.UserId == userId);

            return  mContext
                    .Messages
                    .Where(
                    msg => msg.ConversationID == conversationId 
                    && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID)
                    && msg.State == MessageState.Delivered
                    && msg.User.Id != userId).Count();
        }

        public bool Empty()
        {
            return mContext.Messages.FirstOrDefault() == null;
        }

    }
}
