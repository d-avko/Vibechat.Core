using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.ApiModels.Bans;
using Vibechat.Web.Services.Repositories;
using VibeChat.Web;

namespace Vibechat.Web.Services.Bans
{
    public class BansService
    {
        public BansService(
            IUsersBansRepository usersBansRepository, 
            IConversationsBansRepository conversationsBansRepository,
            IUsersRepository usersRepository,
            IConversationRepository conversationRepository)
        {
            UsersBansRepository = usersBansRepository;
            ConversationsBansRepository = conversationsBansRepository;
            UsersRepository = usersRepository;
            ConversationRepository = conversationRepository;
        }

        public IUsersBansRepository UsersBansRepository { get; }
        public IConversationsBansRepository ConversationsBansRepository { get; }
        public IUsersRepository UsersRepository { get; }
        public IConversationRepository ConversationRepository { get; }

        public async Task BanUserFromConversation(int conversationId, string userToBanId, string whoAccessedId)
        {
            if (userToBanId == whoAccessedId)
            {
                throw new FormatException("Can't ban yourself.");
            }

            ConversationDataModel conversation = ConversationRepository.GetById(conversationId);
            UserInApplication banned = await UsersRepository.GetById(userToBanId);

            if (conversation.Creator.Id != whoAccessedId)
            {
                throw new FormatException("Only creator can ban users.");
            }

            if(banned == null)
            {
                throw new FormatException("Wrong user to ban id was provided.");
            }

            if (conversation == null)
            {
                throw new FormatException("Wrong conversation id was provided.");
            }

            try
            {
                ConversationsBansRepository.BanUserInGroup(banned, conversation);
            }
            catch
            {
                throw new FormatException("Wrong conversation id was provided.");
            }
        }

        public async Task BanUser(string UserToBanId, int? conversationId, string whoAccessedId)
        {
            if (UserToBanId == whoAccessedId)
            {
                throw new FormatException("Can't ban yourself.");
            }

            UserInApplication bannedBy = await UsersRepository.GetById(whoAccessedId);
            UserInApplication banned = await UsersRepository.GetById(UserToBanId);

            if (banned == null || bannedBy == null)
            {
                throw new FormatException("Wrong id of person to ban.");
            }

            try
            {
                UsersBansRepository.BanUser(banned, bannedBy);
            }
            catch
            {
                throw new FormatException("User is already banned.");
            }

            if(conversationId != null)
            {
                ConversationDataModel conversation = ConversationRepository.GetById(conversationId.Value);

                if (conversation == null)
                {
                    throw new FormatException("Something really bad happened: conversation to ban was provided, but it was incorrect.");
                }

                ConversationsBansRepository.BanUserInGroup(banned, conversation);
            }
        }

        public async Task<bool> IsBannedFromConversation(int conversationId, string Who)
        {
            var who = await UsersRepository.GetById(Who);

            if(who == null)
            {
                throw new FormatException("Wrong id of a person to check.");
            }

            return ConversationsBansRepository.IsBanned(who, conversationId);
        }

        public bool IsBannedFromMessagingWith(string who, string byWho)
        {
            // check if allowed
            return UsersBansRepository.IsBanned(who, byWho);
        }

        public async Task Unban(string userId, string whoUnbans)
        {
            try
            {
                UsersBansRepository.UnbanUser(userId, whoUnbans);
            }
            catch
            {
                throw new FormatException("Wrong id of a person to unban.");
            }
        }
    }
}
