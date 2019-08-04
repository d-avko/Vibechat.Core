using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using VibeChat.Web;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;
using Vibechat.Web.Data.Messages;

namespace Vibechat.Web.Data.Repositories
{
    public class MessagesRepository : IMessagesRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public MessagesRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public MessageDataModel Add(AppUser whoSent, Message message, int groupId, MessageDataModel forwardedMessage)
        {
            return mContext.Messages.Add(new MessageDataModel()
            {
                ConversationID = groupId,
                MessageContent = message.MessageContent,
                TimeReceived = DateTime.UtcNow,
                User = whoSent,
                IsAttachment = false,
                ForwardedMessage = forwardedMessage,
                State = MessageState.Delivered
            })?.Entity;
        }

        public MessageDataModel AddSecureMessage(AppUser whoSent, string message, int groupId)
        {
            return mContext.Messages.Add(new MessageDataModel()
            {
                ConversationID = groupId,
                TimeReceived = DateTime.UtcNow,
                User = whoSent,
                IsAttachment = false,
                State = MessageState.Delivered,
                EncryptedPayload = message
            })?.Entity;
        }

        public MessageDataModel AddAttachment(
            AppUser whoSent,
            MessageAttachmentDataModel attachment,
            Message message,
            int groupId)
        {
            return mContext.Messages.Add(new MessageDataModel()
            {
                ConversationID = groupId,
                MessageContent = message.MessageContent,
                TimeReceived = DateTime.UtcNow,
                User = whoSent,
                AttachmentInfo = attachment,
                IsAttachment = true,
                State = MessageState.Delivered
            })?.Entity;
        }

        public void Remove(List<int> messagesIds, string whoRemovedId)
        {
            mContext.DeletedMessages.AddRange(
                messagesIds
                .Select(msgId => new DeletedMessagesDataModel()
                {
                    UserId = whoRemovedId,
                    Message = mContext.Messages.First(msg => msg.MessageID == msgId)
                }));
        }

        public void RemovePermanent(List<MessageDataModel> messages)
        {
            mContext.Messages.RemoveRange(messages);
        }

        public IQueryable<MessageDataModel> GetByIds(List<int> ids)
        {
            return mContext.Messages.Where(msg => ids.Any(id => id == msg.MessageID));
        }

        public MessageDataModel GetById(int id)
        {
            return mContext
                .Messages
                .Include(x => x.User)
                .SingleOrDefault(x => x.MessageID == id);
        }

        public void MarkAsRead(MessageDataModel message)
        {
            message.State = MessageState.Read;
        }

        public IIncludableQueryable<MessageDataModel, AppUser> Get(
            string userId,
            int conversationId,
            int maxMessageId = -1,
            bool allMessages = false, 
            int offset = 0, 
            int count = 0)
        {
            var deletedMessages = mContext
            .DeletedMessages
            .Where(msg => msg.Message.ConversationID == conversationId && msg.UserId == userId);

            IQueryable<MessageDataModel> query;
            
            if (allMessages)
            {
                query = mContext
                    .Messages
                    .Where(msg => msg.ConversationID == conversationId)
                    .Where(msg => !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID));
            }
            else
            {
                if (maxMessageId != -1)
                {
                    query = mContext
                        .Messages
                        .Where(msg => msg.ConversationID == conversationId
                                      && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID)
                                      && msg.MessageID > maxMessageId)
                        .OrderBy(msg => msg.TimeReceived)
                        .Skip(offset)
                        .Take(count);
                }
                else
                {
                    query = mContext
                        .Messages
                        .Where(msg =>
                            msg.ConversationID == conversationId &&
                            !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID))
                        .OrderByDescending(x => x.TimeReceived)
                        .Skip(offset)
                        .Take(count);
                }   
            }

            return  query
                .Include(msg => msg.User)
                .Include(msg => msg.AttachmentInfo)
                .Include(x => x.AttachmentInfo)
                .ThenInclude(x => x.AttachmentKind)
                .Include(x => x.User)
                .Include(x => x.ForwardedMessage)
                .ThenInclude(x => x.AttachmentInfo)
                .ThenInclude(x => x.AttachmentKind)
                .Include(x => x.ForwardedMessage)
                .ThenInclude(x => x.User);
        }

        public IIncludableQueryable<MessageDataModel, AppUser> GetAttachments(
            string userId, 
            int conversationId, 
            AttachmentKind attachmentKind,
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
                && msg.AttachmentInfo.AttachmentKind.Kind == attachmentKind 
                && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID))
                .OrderByDescending(x => x.TimeReceived)
                .Skip(offset)
                .Take(count)
                .Include(x => x.AttachmentInfo)
                .ThenInclude(x => x.AttachmentKind)
                .Include(x => x.User)
                .Include(x => x.ForwardedMessage)
                .ThenInclude(x => x.AttachmentInfo)
                .ThenInclude(x => x.AttachmentKind)
                .Include(x => x.ForwardedMessage)
                .ThenInclude(x => x.User);
        }
        
        public int GetUnreadAmount(int conversationId, string userId, int lastMessageId)
        {
            var deletedMessages = mContext
             .DeletedMessages
             .Where(msg => msg.Message.ConversationID == conversationId && msg.UserId == userId);
            
            return  mContext
                .Messages.Count(msg => msg.ConversationID == conversationId 
                                       && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID)
                                       && msg.MessageID > lastMessageId);
        }

        public bool Empty()
        {
            return mContext.Messages == null;
        }

    }
}
