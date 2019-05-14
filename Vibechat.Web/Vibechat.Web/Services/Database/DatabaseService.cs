using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.ChatData.Messages;
using Vibechat.Web.Data.ApiModels.Messages;
using Vibechat.Web.Services.ChatDataProviders;
using VibeChat.Web;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Services
{
    public class DatabaseService
    {
        public DatabaseService(ApplicationDbContext mContext, UserManager<UserInApplication> mUserManager, IChatDataProvider chatDataProvider)
        {
            this.mContext = mContext;
            this.mUserManager = mUserManager;
            this.chatDataProvider = chatDataProvider;
        }

        protected ApplicationDbContext mContext;

        protected UserManager<UserInApplication> mUserManager;

        protected IChatDataProvider chatDataProvider;

        #region Conversations

        public async Task<ConversationTemplate> CreateConversation(CreateConversationCredentialsApiModel convInfo)
        {

            var defaultError = new FormatException("Error while creating the conversation..");

            //if there was no group info or creator info

            if (string.IsNullOrWhiteSpace(convInfo.ConversationName) || string.IsNullOrWhiteSpace(convInfo.CreatorId))
            {
                throw defaultError;
            }

            var user = await mUserManager.FindByIdAsync(convInfo.CreatorId).ConfigureAwait(false);

            if (user == null)
            {
                throw defaultError;
            }

            if ((!convInfo.IsGroup) && (convInfo.DialogUserId == null))
            {
                throw new FormatException("No dialogue user was provided...");
            }

            UserInApplication SecondDialogueUser = null;

            if (convInfo.DialogUserId != null)
            {
                //if this is a dialogue , find a user with whom to create conversation
                SecondDialogueUser = await mUserManager.FindByIdAsync(convInfo.DialogUserId);

                if (SecondDialogueUser == null)
                {
                    throw defaultError;
                }
            }

            // create new conversation

            var ConversationToAdd = new ConversationDataModel()
            {
                IsGroup = convInfo.IsGroup,
                Name = convInfo.IsGroup ? convInfo.ConversationName : SecondDialogueUser.UserName,
                ImageUrl = convInfo.ImageUrl ?? chatDataProvider.GetGroupPictureUrl()
            };

            await mContext.Conversations.AddAsync(ConversationToAdd).ConfigureAwait(false);

            //To prevent duplicating, check if there is no such pair as User - conversation

            if (mContext.UsersConversations.SingleOrDefault(conv => (conv.Conversation.ConvID == ConversationToAdd.ConvID) && (conv.User == user)) != null)
            {
                mContext.Conversations.Remove(ConversationToAdd);
                throw defaultError;
            }

            await mContext.UsersConversations.AddAsync(new UsersConversationDataModel()
            {
                Conversation = ConversationToAdd,
                User = user
            });

            await mContext.SaveChangesAsync();

            //after saving changes we have Id of our 
            //created conversation in ConversationToAdd variable

            return  new ConversationTemplate()
                    {
                        ConversationID = ConversationToAdd.ConvID,
                        DialogueUser = (ConversationToAdd.IsGroup) ?
                          null :
                          new UserInfo()
                          {
                              Id = SecondDialogueUser.Id,
                              Name = SecondDialogueUser.FirstName,
                              ImageUrl = SecondDialogueUser.ProfilePicImageURL,
                              LastName = SecondDialogueUser.LastName,
                              LastSeen = SecondDialogueUser.LastSeen,
                              UserName = SecondDialogueUser.UserName,
                              ConnectionId = SecondDialogueUser.ConnectionId,
                              IsOnline = SecondDialogueUser.IsOnline
                          },

                        ImageUrl = ConversationToAdd.ImageUrl,
                        IsGroup = ConversationToAdd.IsGroup,
                        Name = ConversationToAdd.Name,
                        Participants = (await GetConversationParticipants(new GetParticipantsApiModel() { ConvId = ConversationToAdd.ConvID })).Participants,
                        Messages = null
                    };

        }

        public async Task<UserInfo> AddUserToGroup(string userId, int conversationId)
        {
            var userToAdd = await mContext.Users.FindAsync(userId);

            return new UserInfo()
            {
                Id = userToAdd.Id,
                Name = userToAdd.FirstName,
                ImageUrl = userToAdd.ProfilePicImageURL,
                LastName = userToAdd.LastName,
                LastSeen = userToAdd.LastSeen,
                UserName = userToAdd.UserName,
                ConnectionId = userToAdd.ConnectionId,
                IsOnline = userToAdd.IsOnline
            };
            //TODO
            //CHECK IF THIS IS EVEN ALLOWED
        }

        public async Task<UserInfo> RemoveUserFromGroup()
        {
            return null;
            //TODO
            //CHECK IF THIS IS EVEN ALLOWED
        }

        public async Task<MessageDataModel> AddMessage(Message message, int groupId, string SenderId)
        {
            UserInApplication whoSent = await mContext.Users.FindAsync(SenderId).ConfigureAwait(false);

            if(whoSent == null)
            {
                throw new FormatException($"Failed to retrieve user with id {SenderId} from database: no such user exists");
            }

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

        public async Task<MessageDataModel> AddAttachmentMessage(Message message, int groupId, string SenderId)
        {
            UserInApplication whoSent = await mContext.Users.FindAsync(SenderId).ConfigureAwait(false);

            if (whoSent == null)
            {
                throw new FormatException($"Failed to retrieve user with id {SenderId} from database: no such user exists");
            }

            var attachment = await mContext.Attachments.AddAsync(new MessageAttachmentDataModel()
            {
                AttachmentKind = await mContext.AttachmentKinds.FindAsync(message.AttachmentInfo.AttachmentKind),
                ContentUrl = message.AttachmentInfo.ContentUrl,
                ImageHeight = message.AttachmentInfo.ImageHeight,
                ImageWidth = message.AttachmentInfo.ImageWidth,
                AttachmentName = message.AttachmentInfo.AttachmentName
            });

            var addedMessage = mContext.Messages.Add(new MessageDataModel()
            {
                ConversationID = groupId,
                MessageContent = message.MessageContent,
                TimeReceived = DateTime.UtcNow,
                User = whoSent,
                AttachmentInfo = attachment.Entity,
                IsAttachment = true
            });

            await mContext.SaveChangesAsync();

            return addedMessage.Entity;
        }


        public async Task AddUserToConversation(AddToConversationApiModel UserProvided)
        {
            var defaultError = new FormatException("Invalid credentials were provided.");


            var FoundConversation = await mContext.Conversations.FindAsync(UserProvided.ConvId).ConfigureAwait(false); ;

            if (FoundConversation == null)
                throw defaultError;

            var FoundUser = await mUserManager.FindByIdAsync(UserProvided.UserId).ConfigureAwait(false);

            if (FoundUser == null)
                throw defaultError;

            await mContext.UsersConversations.AddAsync(new UsersConversationDataModel()
            {
                Conversation = FoundConversation,
                User = FoundUser
            }).ConfigureAwait(false);

            await mContext.SaveChangesAsync();
        }

        public async Task<ConversationInfoResultApiModel> GetConversationInformation(CredentialsForConversationInfoApiModel UserProvided, string whoAccessedId)
        {
            var defaultError = new FormatException("User info provided was not correct.");

            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            if (UserProvided.UserId != whoAccessedId)
            {
                throw unAuthorizedError;
            }

            if (UserProvided == null)
                throw defaultError;

            if (UserProvided.UserId == null)
                throw defaultError;

            var user = await mUserManager.FindByIdAsync(UserProvided.UserId).ConfigureAwait(false);

            if (user == null)
                throw defaultError;

            // list containing raw data from db

            var EntitiesList = new List<ConversationDataModel>();

            var UsersConvs = mContext.UsersConversations
                .Include(x => x.Conversation)
                .Include(y => y.User);

            EntitiesList.AddRange(
            from UserConv in UsersConvs
            where UserConv.User.Id == UserProvided.UserId
            select UserConv.Conversation);

            var returnData = new List<ConversationTemplate>();

            // UserInApplication(server) --> UserInfo(in Client app)

            foreach (ConversationDataModel conv in EntitiesList)
            {
                UserInApplication DialogueUser = conv.IsGroup ? null : GetAnotherUserInDialogue(conv.ConvID, UserProvided.UserId);

                returnData.Add
                    (
                        new ConversationTemplate()
                        {
                            Name = conv.IsGroup ? conv.Name : DialogueUser.UserName,
                            ConversationID = conv.ConvID,
                            DialogueUser = DialogueUser == null ?
                            null
                            :
                            new UserInfo()
                            {
                                Id = DialogueUser.Id,
                                Name = DialogueUser.FirstName,
                                LastName = DialogueUser.LastName,
                                LastSeen = DialogueUser.LastSeen,
                                UserName = DialogueUser.UserName,
                                ImageUrl = DialogueUser.ProfilePicImageURL,
                                IsOnline = DialogueUser.IsOnline,
                                ConnectionId = DialogueUser.ConnectionId
                            },
                            IsGroup = conv.IsGroup,
                            ImageUrl = conv.ImageUrl,
                            Participants = (await GetConversationParticipants(new GetParticipantsApiModel() { ConvId = conv.ConvID })).Participants,

                            //only get last message here, client should fetch messages after he opened the conversation.

                            Messages = (await GetConversationMessages(new GetMessagesApiModel() { ConversationID = conv.ConvID, Count = 1, MesssagesOffset = 0 }, whoAccessedId)).Messages
                        }
                    );
            }

            return new ConversationInfoResultApiModel()
            {
                Conversations = returnData
            };

        }


        public async Task<GetParticipantsResultApiModel> GetConversationParticipants(GetParticipantsApiModel convInfo)
        {
            var defaultErrorMessage = new FormatException("Wrong conversation was provided.");

            if (convInfo == null)
                throw defaultErrorMessage;

            var conversation = await mContext.Conversations.FindAsync(convInfo.ConvId).ConfigureAwait(false);

            if (conversation == null)
                throw defaultErrorMessage;

            //Update info from db
            var UsersConvs = mContext.UsersConversations
                .Include(x => x.User)
                .Include(y => y.Conversation);

            var ParticipantsToReturn =
                (from UserConv in UsersConvs
                 where UserConv.Conversation.ConvID == conversation.ConvID
                 select new UserInfo()
                 {
                     Id = UserConv.User.Id,
                     LastName = UserConv.User.LastName,
                     LastSeen = UserConv.User.LastSeen,
                     Name = UserConv.User.FirstName,
                     UserName = UserConv.User.UserName,
                     ImageUrl = UserConv.User.ProfilePicImageURL,
                     ConnectionId = UserConv.User.ConnectionId,
                     IsOnline = UserConv.User.IsOnline
                 }).ToList();

            return new GetParticipantsResultApiModel()
            {
                    Participants = ParticipantsToReturn
            };

        }


        public async Task<GetMessagesResultApiModel> GetConversationMessages(GetMessagesApiModel convInfo, string whoAccessedId)
        {
            var defaultErrorMessage = new FormatException("Wrong conversation was provided.");

            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            if (mContext.Messages.FirstOrDefault() == null)
            {
                return new GetMessagesResultApiModel()
                {
                    Messages = null
                };
            }

            if (convInfo == null)
                throw defaultErrorMessage;

            var conversation = await mContext.Conversations.FindAsync(convInfo.ConversationID).ConfigureAwait(false);

            if (conversation == null)
                throw defaultErrorMessage;

            var members = await GetConversationParticipants(new GetParticipantsApiModel() { ConvId = convInfo.ConversationID });

            //only member of conversation could request messages (there should exist other calls for non-members).

            if (members.Participants.Find(x => x.Id == whoAccessedId) == null)
                throw unAuthorizedError;

            var deletedMessages = mContext
                .DeletedMessages
                .Where(msg => msg.Message.ConversationID == convInfo.ConversationID && msg.UserId == whoAccessedId);

            var messages = mContext
                .Messages
                .Where(msg => msg.ConversationID == convInfo.ConversationID)
                .Where(msg => !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID))
                .OrderByDescending(x => x.TimeReceived)
                .Skip(convInfo.MesssagesOffset)
                .Take(convInfo.Count)
                .Include(msg => msg.User);

            return new GetMessagesResultApiModel()
            {
                Messages = (from msg in messages
                            select new Message()
                            {
                                Id = msg.MessageID,
                                ConversationID = msg.ConversationID,
                                MessageContent = msg.MessageContent,
                                TimeReceived = msg.TimeReceived,
                                User = msg.User == null ? null : new UserInfo()
                                {
                                    Id = msg.User.Id,
                                    LastName = msg.User.LastName,
                                    LastSeen = msg.User.LastSeen,
                                    Name = msg.User.FirstName,
                                    UserName = msg.User.UserName,
                                    ImageUrl = msg.User.ProfilePicImageURL,
                                    IsOnline = msg.User.IsOnline,
                                    ConnectionId = msg.User.ConnectionId
                                },
                                AttachmentInfo = msg.AttachmentInfo == null ? null : new MessageAttachment()
                                {
                                    AttachmentKind = msg.AttachmentInfo.AttachmentKind.Name,
                                    ContentUrl = msg.AttachmentInfo.ContentUrl,
                                    AttachmentName = msg.AttachmentInfo.AttachmentName,
                                    ImageHeight = msg.AttachmentInfo.ImageHeight,
                                    ImageWidth = msg.AttachmentInfo.ImageWidth
                                },
                                IsAttachment = msg.IsAttachment
                            }).ToList()
            };

        }


        public async Task<GetConversationByIdResultApiModel> GetConversationById(GetConversationByIdApiModel convInfo, string whoAccessedId)
        {

            var conversation = mContext.Conversations.FirstOrDefault(x => x.ConvID == convInfo.ConversationId);

            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            var user = new UserInApplication();

            if (conversation.IsGroup)
            {
                user = await mUserManager.FindByIdAsync(whoAccessedId).ConfigureAwait(false);
            }


            if (conversation == null)
            {
                throw new FormatException("No such conversation was found.");
            }

            var members = await GetConversationParticipants(new GetParticipantsApiModel() { ConvId = convInfo.ConversationId });

            //only member of conversation could request messages (there should exist other calls for non-members).

            if (members.Participants.Find(x => x.UserName == whoAccessedId) == null)
                throw unAuthorizedError;

            if (conversation.IsGroup && user == null)
            {
                throw new FormatException("User with such id was not found.");
            }

            user = GetAnotherUserInDialogue(convInfo.ConversationId, user.Id);

            if (user == null)
            {
                throw new FormatException("Unexpected error: no corresponding user in dialogue.");
            }

            return new GetConversationByIdResultApiModel()
            {
                Conversation = new ConversationTemplate()
                {
                    ConversationID = conversation.ConvID,
                    DialogueUser = (conversation.IsGroup) ? null : new UserInfo()
                    {
                        Id = user.Id,
                        Name = user.FirstName,
                        ImageUrl = user.ProfilePicImageURL,
                        LastName = user.LastName,
                        LastSeen = user.LastSeen,
                        UserName = user.UserName,
                        IsOnline = user.IsOnline,
                        ConnectionId = user.ConnectionId
                    },
                    ImageUrl = conversation.ImageUrl,
                    IsGroup = conversation.IsGroup,
                    Name = conversation.Name
                }

            };

        }

        public async Task DeleteConversationMessages(DeleteMessagesRequest messagesInfo, string whoAccessedId)
        {
            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            var messagesToDelete = mContext.Messages.Where(x => messagesInfo.MessagesId.Any(y => y == x.MessageID));

            if(!messagesToDelete.All(x => x.ConversationID == messagesInfo.ConversationId))
            {
                throw new ArgumentException("All messages must be from same conversation passed as ConversationId parameter.");
            }

            var conversation = await mContext.UsersConversations
                .FirstOrDefaultAsync(x => x.User.Id == whoAccessedId && x.Conversation.ConvID == messagesInfo.ConversationId);

            if(conversation == null)
            {
                throw unAuthorizedError;
            }

            await mContext.DeletedMessages.AddRangeAsync(
                messagesInfo.MessagesId
                .Select(msgId => new DeletedMessagesDataModel()
                {
                    UserId = whoAccessedId,
                    Message = mContext.Messages.First(msg => msg.MessageID == msgId)
                }));

            await mContext.SaveChangesAsync();
        } 

        /// <summary>
        /// Helper method used to find user with whom 
        /// current user have a dialogue
        /// </summary>
        /// <param name="convId"></param>
        /// <param name="FirstUserInDialogueId"></param>
        /// <returns></returns>
        public UserInApplication GetAnotherUserInDialogue(int convId, string FirstUserInDialogueId)
        {
            var UsersConvs = mContext.UsersConversations
               .Include(x => x.Conversation)
               .Include(y => y.User);

            foreach (var conv in UsersConvs)
            {
                // dialogues have only 2 users in them => find first user and then return second
                if ((conv.Conversation.ConvID == convId) && (conv.User.Id != FirstUserInDialogueId))
                {
                    return conv.User;
                }
            }

            return null;
        }

        #endregion

        #region Login / register

        public async Task<LoginResultApiModel> LogInAsync(LoginCredentialsApiModel loginCredentials)
        {
            var defaultError = new FormatException("Wrong username or password");

            if ((loginCredentials?.UserNameOrEmail == null) || (string.IsNullOrWhiteSpace(loginCredentials.UserNameOrEmail)))
            {
                throw defaultError;
            }

            UserInApplication user;

            if (loginCredentials.UserNameOrEmail.Contains("@"))
            {
                user = await mUserManager.FindByEmailAsync(loginCredentials.UserNameOrEmail);
            }
            else
            {
                user = await mUserManager.FindByNameAsync(loginCredentials.UserNameOrEmail);
            }

            if (user == null)
            {
                throw defaultError;
            }

            if (!await mUserManager.CheckPasswordAsync(user, loginCredentials.Password).ConfigureAwait(false))
            {
                throw defaultError;
            }

            //if we are here then have valid password and login of a user

            return new LoginResultApiModel()
            {
                Info = new UserInfo()
                {
                    Name = user.FirstName,
                    LastSeen = user.LastSeen,
                    ImageUrl = user.ProfilePicImageURL,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Id = user.Id
                },
                Token = user.GenerateJwtToken(),
            };
        }

        public async Task RegisterNewUserAsync(RegisterInformationApiModel userToRegister)
        {
            var defaultError = new FormatException("Check the fields and try again.");

            var EmailFormatError = new FormatException("Email is in wrong format!");

            if (userToRegister == null)
                throw defaultError;

            if (string.IsNullOrWhiteSpace(userToRegister.Email) || string.IsNullOrWhiteSpace(userToRegister.Password) || string.IsNullOrWhiteSpace(userToRegister.UserName))
            {
                throw defaultError;
            }

            if (userToRegister.UserName.Contains("@") || userToRegister.UserName.Contains("@"))
            {
                throw new FormatException("Nickname or Username cannot contain '@'");
            }

            if (!Regex.Match(userToRegister.Email, "[^@]*@[^\\.]\\.(\\w+)").Success)
            {
                throw EmailFormatError;
            }

            // if UserName and email are not unique

            if ((mUserManager.FindByNameAsync(userToRegister.UserName) == null) && (mUserManager.FindByEmailAsync(userToRegister.Email) == null))
            {
                throw new FormatException("The username or e-mail is not unique.");
            }

            var userToCreate = new UserInApplication()
            {
                UserName = userToRegister.UserName,
                Email = userToRegister.Email,
                FirstName = userToRegister.FirstName,
                LastName = userToRegister.LastName,
                ProfilePicImageURL = chatDataProvider.GetProfilePictureUrl()
            };


            var result = await mUserManager.CreateAsync(userToCreate, userToRegister.Password);

            if (!result.Succeeded)
            {
                throw new FormatException(result.Errors?.ToList()[0].Description);
            }

        }

        #endregion

        #region Users info
        public async Task<UserByIdApiResponseModel> GetUserById(UserByIdApiModel userId)
        {
            if (userId == null)
            {
                throw new FormatException("Provided user was null");
            }

            var FoundUser = await mContext.Users.FindAsync(userId.Id).ConfigureAwait(false);

            if (FoundUser == null)
            {
                throw new FormatException("User was not found");
            }


            return new UserByIdApiResponseModel()
            {
                User = new UserInfo()
                {
                    Id = FoundUser.Id,
                    ImageUrl = FoundUser.ProfilePicImageURL,
                    LastName = FoundUser.LastName,
                    LastSeen = FoundUser.LastSeen,
                    Name = FoundUser.FirstName,
                    UserName = FoundUser.UserName,
                    ConnectionId = FoundUser.ConnectionId,
                    IsOnline = FoundUser.IsOnline
                }
            };

        }

        public async Task<UserInApplication> GetUserById(string userId)
        {
            if (userId == null)
            {
                throw new FormatException("Provided user was null");
            }

            var FoundUser = await mContext.Users.FindAsync(userId).ConfigureAwait(false);

            if (FoundUser == null)
            {
                throw new FormatException("User was not found");
            }


            return FoundUser;
        }

        public async Task<UsersByNickNameResultApiModel> FindUsersByNickName(UsersByNickNameApiModel credentials)
        {
            if (credentials.UsernameToFind == null)
            {
                throw new FormatException("Nickname was null");
            }

            var result = mUserManager.Users.Where(user => user.UserName.Contains(credentials.UsernameToFind)).ToList();

            if (result.Count() == 0)
            {
                throw new FormatException("Noone was found.");
            }

            return new UsersByNickNameResultApiModel()
            {
                UsersFound = result.Select((FoundUser) => new FoundUser
                {
                    ID = FoundUser.Id,
                    Username = FoundUser.UserName,
                    FirstName = FoundUser.FirstName,
                    LastName = FoundUser.LastName
                }).ToList()
            };
        }

        public async Task MakeUserOnline(string userId, string signalRConnectionId)
        {
            UserInApplication user = await mContext.Users.FindAsync(userId);

            user.IsOnline = true;

            user.LastSeen = DateTime.UtcNow;

            user.ConnectionId = signalRConnectionId;

            await mContext.SaveChangesAsync();
        }

        public async Task MakeUserOffline(string userId)
        {
            var user = await mContext.Users.FindAsync(userId);

            user.IsOnline = false;

            user.ConnectionId = null;

            mContext.SaveChanges();
        }

        #endregion
    }
}
