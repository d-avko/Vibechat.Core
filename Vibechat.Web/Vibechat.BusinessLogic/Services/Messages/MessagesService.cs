using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.BusinessLogic.Extensions;
using Vibechat.DataLayer.DataModels;
using Vibechat.DataLayer.Repositories;
using Vibechat.DataLayer.Repositories.Specifications;
using Vibechat.DataLayer.Repositories.Specifications.DeletedMessages;
using Vibechat.DataLayer.Repositories.Specifications.Messages;
using Vibechat.DataLayer.Repositories.Specifications.UsersChats;
using Vibechat.Shared.DTO.Messages;

namespace Vibechat.BusinessLogic.Services.Messages
{
    public class MessagesService
    {
        private readonly IConversationRepository conversationRepository;
        private readonly IMessagesRepository messagesRepository;
        private readonly IUsersConversationsRepository usersConversationsRepository;
        private readonly ILastMessagesRepository lastMessagesRepository;
        private readonly UnitOfWork unitOfWork;
        private readonly IUsersRepository usersRepository;
        private readonly IAttachmentKindsRepository attachmentKindsRepository;
        private readonly IAttachmentsRepository attachmentRepository;
        private readonly IChatEventsRepository chatEventsRepository;
        private readonly IDeletedMessagesRepository deletedMessages;

        public MessagesService(IConversationRepository conversationRepository,
            IMessagesRepository messagesRepository,
            IUsersConversationsRepository usersConversationsRepository,
            ILastMessagesRepository lastMessagesRepository,
            UnitOfWork unitOfWork,
            IUsersRepository usersRepository,
            IAttachmentKindsRepository attachmentKindsRepository,
            IAttachmentsRepository attachmentRepository,
            IChatEventsRepository chatEventsRepository,
            IDeletedMessagesRepository deletedMessages
        )
        {
            this.conversationRepository = conversationRepository;
            this.messagesRepository = messagesRepository;
            this.usersConversationsRepository = usersConversationsRepository;
            this.lastMessagesRepository = lastMessagesRepository;
            this.unitOfWork = unitOfWork;
            this.usersRepository = usersRepository;
            this.attachmentKindsRepository = attachmentKindsRepository;
            this.attachmentRepository = attachmentRepository;
            this.chatEventsRepository = chatEventsRepository;
            this.deletedMessages = deletedMessages;
        }

        public const int MinSymbolsInMessagesSearch = 5;

        public async Task<List<Message>> GetAttachments(AttachmentKind kind, int conversationId, string whoAccessedId,
            int offset, int count)
        {
            var conversation = await conversationRepository.GetByIdAsync(conversationId);

            if (conversation == null)
            {
                throw new KeyNotFoundException("Wrong conversation to get attachments from.");
            }

            //only member of conversation could request messages of non-public conversation.

            if (!await usersConversationsRepository.Exists(whoAccessedId, conversationId) && !conversation.IsPublic)
            {
                throw new UnauthorizedAccessException("You are unauthorized to do such an action.");
            }

            var deleted = await deletedMessages.AsQuerableAsync(
                new GetDeletedMessagesOfUserSpec(whoAccessedId));

            var messages = await messagesRepository.ListAsync(
                new GetSpecificAttachmentsSpec
                (deleted,
                 conversationId,
                 kind,
                 offset,
                 count));
            
            return (from msg in messages
                select msg.ToMessage()).ToList();
        }

        public async Task<int> GetUnreadMessagesAmount(Shared.DTO.Conversations.Chat chat, string userId)
        {
            var lastMessage = await lastMessagesRepository.GetByIdAsync(userId, chat.Id);
            IQueryable<DeletedMessagesDataModel> deleted = await deletedMessages
                .AsQuerableAsync(new GetDeletedMessagesSpec(chat.Id, userId));

            return await messagesRepository.CountAsync(new UnreadMessagesCountSpec(deleted, chat.Id, lastMessage?.MessageID ?? 0));
        }

        /// <summary>
        /// Performs case-insensitive message search.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="deviceId"></param>
        /// <param name="searchString"></param>
        /// <param name="callerId"></param>
        /// <returns></returns>
        public async Task<List<Message>> SearchForMessages(string deviceId, string searchString, int offset, int count,
            string callerId)
        {
            if (searchString.Length < MinSymbolsInMessagesSearch)
            {
                throw new InvalidDataException($"Minimum symbols for search to work: {MinSymbolsInMessagesSearch}");
            }
            
            var foundMessages = messagesRepository.Search(offset, count, searchString, callerId)
                .ToList();

            return (from msg in foundMessages
                select msg.ToMessage()).ToList();
        }

        /// <summary>
        /// Returns messages for specified user and chat.
        /// </summary>
        /// <param name="whoAccessedId"></param>
        /// <param name="chatId"></param>
        /// <param name="maxMessageId">messageId to start from</param>
        /// <param name="history">if true, return messages which ids are lower than maxMessageId, greater otherwise.
        /// When false, maxMessageId is inclusive, and is not inclusive when true.</param>
        /// <param name="offset"></param>
        /// <param name="count">amount to return.</param>
        /// <param name="setLastMessage">Set last loaded msg of a client after querying the messages, if chatId is group.</param>
        /// <returns></returns>
        public async Task<List<Message>> GetMessages(int chatId, int offset, int count, int maxMessageId,
            bool history,
            bool setLastMessage,
            string whoAccessedId)
        {
            var conversation = await conversationRepository.GetByIdAsync(chatId);

            if (conversation == null)
            {
                throw new KeyNotFoundException("Wrong conversation.");
            }

            var existsInChat = await usersConversationsRepository.Exists(whoAccessedId, chatId);

            //only member of conversation could request messages of non-public conversation.

            if (!existsInChat && !conversation.IsPublic)
            {
                throw new UnauthorizedAccessException("You are unauthorized to do such an action.");
            }

            if(maxMessageId == -1)
            {
                throw new InvalidDataException("Wrong maxMessageId was provided.");
            }
            
            ISpecification<MessageDataModel> spec;

            if (history)
            {
                spec = new GetMessagesHistorySpec(
                    whoAccessedId,
                    chatId,
                    maxMessageId,
                    offset,
                    count);
            }
            else
            {
                spec = new GetRecentMessagesSpec(
                    whoAccessedId,
                    chatId,
                    maxMessageId,
                    offset,
                    count);
            }

            var messages = await messagesRepository.ListAsync(spec);

            //automatically set last message in group.

            if (setLastMessage && conversation.IsGroup)
            {
                if (!messages.Count().Equals(0))
                {
                    await SetLastMessage(whoAccessedId, chatId, messages.Max(msg => msg.MessageID));
                }
            }

            return (from msg in messages
                select msg.ToMessage()).ToList();
        }

        public async Task<Message> GetLastRecentMessage(int chatId, string userId)
        {
            var deleted = await deletedMessages.AsQuerableAsync(new GetDeletedMessagesOfUserSpec(userId));

            return (await messagesRepository.ListAsync(
                new GetLatestMessagesSpec(deleted, chatId, 0, 1))).FirstOrDefault()?.ToMessage();
        }

        public async Task<IEnumerable<Message>> GetAllAttachments(int chatId, string userId)
        {
            var deleted = await deletedMessages.AsQuerableAsync(new GetDeletedMessagesOfUserSpec(userId));

            return (await messagesRepository.ListAsync(
                new GetAttachmentsSpec(deleted, chatId, 0, Int32.MaxValue))).Select(x => x.ToMessage());
        }

        public async Task DeleteMessages(List<int> messagesIds,int chatId, string whoAccessedId)
        {
            var messagesToDelete = await messagesRepository
                .ListAsync(new MessagesByIdsSpec(messagesIds));

            if (messagesToDelete.Any(x => x.ConversationID != chatId))
            {
                throw new InvalidDataException(
                    "All messages must be from same conversation passed as ConversationId parameter.");
            }

            var conversation = await usersConversationsRepository.GetByIdAsync(whoAccessedId, chatId);

            if (conversation == null)
            {
                throw new UnauthorizedAccessException("You are unauthorized to do such an action.");
            }

            try
            {
                foreach (MessageDataModel msg in messagesToDelete)
                {
                    await deletedMessages.AddAsync(DeletedMessagesDataModel.Create(whoAccessedId, msg));
                }

                await unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Failed to delete this message. Probably it was already deleted.", ex);
            }
        }

        /// <summary>
        /// Sets last message id of a user.
        /// Checks if new id is greater than previous one:
        /// if less, nothing will happen.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="chatId"></param>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public async Task SetLastMessage(string userId, int chatId, int msgId)
        {
            var lastMessage = await lastMessagesRepository.GetByIdAsync(userId, chatId);

            if (lastMessage == null)
            {
                await lastMessagesRepository.AddAsync(LastMessageDataModel.Create(userId, chatId, msgId));
            }
            else
            {
                if (lastMessage.MessageID >= msgId)
                {
                    return;
                }

                lastMessage.MessageID = msgId;
                await lastMessagesRepository.UpdateAsync(lastMessage);
            }

            await unitOfWork.Commit();
        }


        /// <summary>
        ///     Marks message as read and updates lastMessageId of a client.
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="conversationId"></param>
        /// <param name="whoAccessedId"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task MarkMessageAsRead(int msgId, int conversationId, string whoAccessedId)
        {
            var message = await messagesRepository.GetByIdAsync(msgId);

            if (!await usersConversationsRepository.Exists(whoAccessedId, conversationId))
            {
                throw new UnauthorizedAccessException(
                    "User was not present in conversation. Couldn't mark the message as read.");
            }
            message.State = MessageState.Read;
            await messagesRepository.UpdateAsync(message);
            await SetLastMessage(whoAccessedId, conversationId, msgId);
            await unitOfWork.Commit();
        }

        /// <summary>
        /// Adds a messages and updates client lastMessageId
        /// </summary>
        /// <param name="message"></param>
        /// <param name="chatId"></param>
        /// <param name="senderId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public async Task<MessageDataModel> AddMessage(Message message, int chatId, string senderId)
        {
            var whoSent = await usersRepository.GetByIdAsync(senderId);

            if (whoSent == null)
            {
                throw new InvalidDataException(
                    $"Failed to retrieve user with id {senderId} from database: no such user exists");
            }

            MessageDataModel forwardedMessage = null;

            if (message.Type == MessageType.Forwarded)
            {
                if (!await usersConversationsRepository.Exists(senderId, message.ForwardedMessage.ConversationID))
                {
                    throw new UnauthorizedAccessException("Forwarded message id is incorrect.");
                }

                var foundMessage = await messagesRepository.GetByIdAsync(message.ForwardedMessage.Id);

                forwardedMessage = foundMessage ?? throw new InvalidDataException("Forwarded message was not found.");
            }

            MessageDataModel model = MessageDataModel.Create(whoSent, chatId);

            if (forwardedMessage != null)
            {
                model.AsForwarded(forwardedMessage);
            }
            else
            {
                model.AsText(message.MessageContent);
            }

            var result = await messagesRepository.AddAsync(model);
            await unitOfWork.Commit();
            await SetLastMessage(senderId, chatId, result.MessageID);
            return result;
        }

        public async Task<MessageDataModel> AddEncryptedMessage(string message, int groupId, string senderId)
        {
            var whoSent = await usersRepository.GetByIdAsync(senderId);

            if (whoSent == null)
            {
                throw new InvalidDataException(
                    $"Failed to retrieve user with id {senderId} from database: no such user exists");
            }

            var model = MessageDataModel
                .Create(whoSent, groupId)
                .AsSecure(message);
            var result = await messagesRepository.AddAsync(model);
            await unitOfWork.Commit();
            await SetLastMessage(senderId, groupId, result.MessageID);
            return result;
        }

        public async Task<MessageDataModel> AddAttachmentMessage(Message message, int groupId, string senderId)
        {
            var whoSent = await usersRepository.GetByIdAsync(senderId);

            if (whoSent == null)
            {
                throw new InvalidDataException(
                    $"Failed to retrieve user with id {senderId} from database: no such user exists");
            }

            var attachmentKind = await attachmentKindsRepository.GetById(message.AttachmentInfo.AttachmentKind);

            var attachment = await attachmentRepository.AddAsync(
                MessageAttachmentDataModel.Create(attachmentKind, message));

            var model = MessageDataModel
                .Create(whoSent, groupId)
                .AsAttachment(attachment);

            var result = await messagesRepository.AddAsync(model);
            await unitOfWork.Commit();
            await SetLastMessage(senderId, groupId, result.MessageID);
            return result;
        }

        public async Task<MessageDataModel> AddChatEvent(ChatEventType type,
            string actorId, string userInvolvedId, int chatId)
        {
            try
            {
                var newEvent = await chatEventsRepository.AddAsync(ChatEventDataModel.Create(actorId, userInvolvedId, type));

                var model = MessageDataModel
                    .Create(null, chatId)
                    .AsEvent(newEvent);

                var result = await messagesRepository.AddAsync(model);
                await unitOfWork.Commit();
                return result;
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Wrong actorId or userId", e);
            }
        }
    }
}