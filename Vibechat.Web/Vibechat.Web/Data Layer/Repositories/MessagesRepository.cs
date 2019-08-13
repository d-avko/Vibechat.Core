using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using VibeChat.Web;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;
using Vibechat.Web.Data.Messages;
using Vibechat.Web.DTO.Messages;

namespace Vibechat.Web.Data.Repositories
{
    public class MessagesRepository : IMessagesRepository
    {
        public MessagesRepository(ApplicationDbContext dbContext)
        {
            mContext = dbContext;
        }

        private readonly ApplicationDbContext mContext;

        public MessageDataModel Add(MessageDataModel message)
        {
            return mContext.Messages.Add(message)?.Entity;
        }

        public void Remove(List<int> messagesIds, string whoRemovedId)
        {
            mContext.DeletedMessages.AddRange(
                messagesIds
                    .Select(msgId => new DeletedMessagesDataModel
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

        public IQueryable<MessageDataModel> Get(
            string userId,
            int conversationId,
            int maxMessageId = -1,
            bool history = false,
            int offset = 0,
            int count = 0)
        {
            var deletedMessages = mContext
                .DeletedMessages
                .Where(msg => msg.Message.ConversationID == conversationId && msg.UserId == userId);

            IQueryable<MessageDataModel> query;
            //query data considering maxMessageId
            
            if (maxMessageId != -1)
            {
                if (history)
                {
                    query = mContext
                        .Messages
                        .Where(msg => msg.ConversationID == conversationId
                                      && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID)
                                      && msg.MessageID < maxMessageId)
                        .OrderByDescending(msg => msg.TimeReceived)
                        .Skip(offset)
                        .Take(count);   
                }
                else
                {
                    query = mContext
                        .Messages
                        .Where(msg => msg.ConversationID == conversationId
                                      && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID)
                                      && msg.MessageID >= maxMessageId)
                        .OrderBy(msg => msg.TimeReceived)
                        .Skip(offset)
                        .Take(count); 
                }
            }
            else
            {
                //just query latest data.
                
                query = mContext
                    .Messages
                    .Where(msg =>
                        msg.ConversationID == conversationId &&
                        !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID))
                    .OrderByDescending(x => x.TimeReceived)
                    .Skip(offset)
                    .Take(count);
            }

            return query
                .Include(msg => msg.User)
                .Include(msg => msg.AttachmentInfo)
                .Include(x => x.AttachmentInfo)
                .ThenInclude(x => x.AttachmentKind)
                .Include(x => x.User)
                .Include(x => x.ForwardedMessage)
                .ThenInclude(x => x.AttachmentInfo)
                .ThenInclude(x => x.AttachmentKind)
                .Include(x => x.ForwardedMessage)
                .ThenInclude(x => x.User)
                .Include(x => x.Event)
                .Include(x => x.Event.Actor)
                .Include(x => x.Event.UserInvolved)
                .AsNoTracking();
        }

        public IQueryable<MessageDataModel> GetAttachments(
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
                    && msg.Type == MessageType.Attachment
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
                .ThenInclude(x => x.User)
                .AsNoTracking();
        }

        public int GetUnreadAmount(int conversationId, string userId, int lastMessageId)
        {
            var deletedMessages = mContext
                .DeletedMessages
                .Where(msg => msg.Message.ConversationID == conversationId && msg.UserId == userId);

            return mContext
                .Messages.Count(msg => msg.ConversationID == conversationId
                                       && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID)
                                       && msg.MessageID > lastMessageId);
        }
        
        public IQueryable<MessageDataModel> Search
            (int offset, int count, string searchString, string userId)
        {
            var Base = mContext
                .Messages
                .Where(msg => !mContext.DeletedMessages.Any(deleted =>
                    deleted.UserId == userId && deleted.MessageID == msg.MessageID))
                .Where(msg => mContext.UsersConversations.Any(chat => 
                    chat.ChatID == msg.ConversationID && chat.UserID == userId));
            
            var forwardedMessages = 
                Base
                .Where(msg => msg.Type == MessageType.Forwarded && msg.ForwardedMessage.Type == MessageType.Text)
                .Where(msg =>
                    EF.Functions.Like(msg.ForwardedMessage.MessageContent.ToLower(), $"%{searchString.ToLower()}%"));

            var notForwarded = 
                Base
                .Where(msg => msg.Type == MessageType.Text)
                .Where(msg =>
                    EF.Functions.Like(msg.MessageContent.ToLower(), $"%{searchString.ToLower()}%"));

            return 
                forwardedMessages.Concat(notForwarded)
                    .OrderByDescending(msg => msg.TimeReceived)
                    .Skip(offset)
                    .Take(count)
                    .Include(msg => msg.ForwardedMessage)
                    .Include(x => x.User)
                    .AsNoTracking();
        }

        public bool Empty()
        {
            return mContext.Messages == null;
        }
    }
}