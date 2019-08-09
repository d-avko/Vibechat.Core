using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;
using VibeChat.Web.ChatData;
using Vibechat.Web.Data.ApiModels.Messages;
using Vibechat.Web.Data.Messages;
using Vibechat.Web.Data.Repositories;
using Vibechat.Web.Data_Layer.Repositories;
using Vibechat.Web.Extensions;

namespace Vibechat.Web.Services.Messages
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
        private readonly IAttachmentRepository attachmentRepository;

        public MessagesService(IConversationRepository conversationRepository,
            IMessagesRepository messagesRepository,
            IUsersConversationsRepository usersConversationsRepository,
            ILastMessagesRepository lastMessagesRepository,
            UnitOfWork unitOfWork,
            IUsersRepository usersRepository,
            IAttachmentKindsRepository attachmentKindsRepository,
            IAttachmentRepository attachmentRepository
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
        }

        public const int MinSymbolsInMessagesSearch = 5;
        
        public async Task<List<Message>> GetAttachments(AttachmentKind kind, int conversationId, string whoAccessedId,
            int offset, int count)
        {
            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            if (messagesRepository.Empty()) return null;

            var conversation = conversationRepository.GetById(conversationId);

            if (conversation == null) throw new FormatException("Wrong conversation to get attachments from.");

            var members = usersConversationsRepository.GetConversationParticipants(conversationId);

            //only member of conversation could request messages of non-public conversation.

            if (members.FirstOrDefault(x => x.Id == whoAccessedId) == null && !conversation.IsPublic)
                throw new UnauthorizedAccessException("You are unauthorized to do such an action.");

            var messages = messagesRepository.GetAttachments(
                whoAccessedId,
                conversationId,
                kind,
                offset,
                count);


            return (from msg in messages
                select msg.ToMessage()).ToList();
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
        public async Task<List<Message>> SearchForMessages(string deviceId, string searchString, int offset, int count, string callerId)
        {
            if (searchString.Length < MinSymbolsInMessagesSearch)
            {
                throw new InvalidDataException($"Minimum symbols for search to work: {MinSymbolsInMessagesSearch}");
            }
            
            List<ConversationDataModel> userChats = usersConversationsRepository.GetUserConversations(deviceId, callerId).ToList();
            var foundMessages = messagesRepository.Search(userChats, offset, count, searchString, callerId);
            
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
            var defaultErrorMessage = new FormatException("Wrong conversation was provided.");

            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            if (messagesRepository.Empty()) return null;

            var conversation = conversationRepository.GetById(chatId);

            if (conversation == null) throw defaultErrorMessage;

            var members = usersConversationsRepository.GetConversationParticipants(chatId);

            //only member of conversation could request messages of non-public conversation.

            if (members.FirstOrDefault(x => x.Id == whoAccessedId) == null && !conversation.IsPublic)
                throw unAuthorizedError;

            var messages = messagesRepository.Get(
                whoAccessedId,
                chatId, 
                maxMessageId, 
                history, 
                offset, 
                count);

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

        public async Task DeleteMessages(DeleteMessagesRequest messagesInfo, string whoAccessedId)
        {
            var messagesToDelete = messagesRepository.GetByIds(messagesInfo.MessagesId);

            if (!messagesToDelete.All(x => x.ConversationID == messagesInfo.ConversationId))
                throw new ArgumentException(
                    "All messages must be from same conversation passed as ConversationId parameter.");

            var conversation = await usersConversationsRepository.Get(whoAccessedId, messagesInfo.ConversationId);

            if (conversation == null) throw new UnauthorizedAccessException("You are unauthorized to do such an action.");

            try
            {
                messagesRepository.Remove(messagesInfo.MessagesId, whoAccessedId);
                await unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Failed to delete this message. Probably it was already deleted.", ex);
            }
        }
        
        public async Task SetLastMessage(string userId, int chatId, int msgId)
        {
            var lastMessage = lastMessagesRepository.Get(userId, chatId);

            if (lastMessage == null)
            {
                lastMessagesRepository.Add(userId, chatId, msgId);
            }
            else
            {
                if (lastMessage.MessageID >= msgId)
                {
                    return;
                }
                
                lastMessage.MessageID = msgId;
                lastMessagesRepository.Update(lastMessage);
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
            var message = messagesRepository.GetById(msgId);

            if (message.User.Id == whoAccessedId)
                throw new UnauthorizedAccessException("Couldn't mark this message as read because it was sent by you.");

            if (!await usersConversationsRepository.Exists(whoAccessedId,conversationId))
                throw new UnauthorizedAccessException(
                    "User was not present in conversation. Couldn't mark the message as read.");

            messagesRepository.MarkAsRead(message);
            await SetLastMessage(whoAccessedId, conversationId, msgId);
            await unitOfWork.Commit();
        }

        /// <summary>
        /// Adds a messages and updates client lastMessageId
        /// </summary>
        /// <param name="message"></param>
        /// <param name="groupId"></param>
        /// <param name="senderId"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public async Task<MessageDataModel> AddMessage(Message message, int groupId, string senderId)
        {
            var whoSent = await usersRepository.GetById(senderId);

            if (whoSent == null)
                throw new FormatException(
                    $"Failed to retrieve user with id {senderId} from database: no such user exists");

            MessageDataModel forwardedMessage = null;

            if (message.ForwardedMessage != null)
            {
                var foundMessage = messagesRepository.GetByIds(new List<int> {message.ForwardedMessage.Id}).ToList();

                if (!foundMessage.Count().Equals(1)) throw new FormatException("Forwarded message was not found.");

                forwardedMessage = foundMessage[0];
            }

            var result = messagesRepository.Add(whoSent, message, groupId, forwardedMessage);
            await unitOfWork.Commit();
            await SetLastMessage(senderId, groupId, result.MessageID);
            return result;
        }

        public async Task<MessageDataModel> AddEncryptedMessage(string message, int groupId, string senderId)
        {
            var whoSent = await usersRepository.GetById(senderId);

            if (whoSent == null)
                throw new FormatException(
                    $"Failed to retrieve user with id {senderId} from database: no such user exists");

            var result = messagesRepository.AddSecureMessage(whoSent, message, groupId);
            await unitOfWork.Commit();
            await SetLastMessage(senderId, groupId, result.MessageID);
            return result;
        }

        public async Task<MessageDataModel> AddAttachmentMessage(Message message, int groupId, string senderId)
        {
            var whoSent = await usersRepository.GetById(senderId);

            if (whoSent == null)
                throw new FormatException(
                    $"Failed to retrieve user with id {senderId} from database: no such user exists");

            var attachmentKind = await attachmentKindsRepository.GetById(message.AttachmentInfo.AttachmentKind);

            var attachment = attachmentRepository.Add(attachmentKind, message);

            var result = messagesRepository.AddAttachment(whoSent, attachment, message, groupId);
            await unitOfWork.Commit();
            await SetLastMessage(senderId, groupId, result.MessageID);
            return result;
        }
    }
}