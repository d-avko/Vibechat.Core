using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using VibeChat.Web;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;

namespace Vibechat.Web.Services
{
    public class DatabaseService
    {
        public DatabaseService(ApplicationDbContext mContext, UserManager<UserInApplication> mUserManager)
        {
            this.mContext = mContext;
            this.mUserManager = mUserManager;
        }

        protected ApplicationDbContext mContext;

        protected UserManager<UserInApplication> mUserManager;

        #region Conversations

        public async Task<CreateConversationResultApiModel> CreateConversation(CreateConversationCredentialsApiModel convInfo)
        {

            var defaultError = new FormatException("Error while creating the conversation..");

            //if there was no group info or creator info

            if (string.IsNullOrWhiteSpace(convInfo.ConvName) || string.IsNullOrWhiteSpace(convInfo.CreatorId))
            {
                throw defaultError;
            }

            var user = await mUserManager.FindByIdAsync(convInfo.CreatorId).ConfigureAwait(false);

            if (user == null)
            {
                throw defaultError;
            }

            if ((!convInfo.IsGroup) && (convInfo.DialogueUserId == null))
            {
                throw new FormatException("No dialogue user was provided...");
            }

            UserInApplication SecondDialogueUser = null;

            if (convInfo.DialogueUserId != null)
            {
                //if this is a dialogue , find a user with whom to create conversation
                SecondDialogueUser = await mUserManager.FindByIdAsync(convInfo.DialogueUserId);

                if (SecondDialogueUser == null)
                {
                    throw defaultError;
                }
            }

            // create new conversation

            var ConversationToAdd = new ConversationDataModel()
            {
                IsGroup = convInfo.IsGroup,
                Name = convInfo.IsGroup ? convInfo.ConvName : SecondDialogueUser.UserName,
                PictureBackgroundRgb = BackgroundColors.GetGroupBackground(),
                ImageUrl = convInfo.ImageUrl
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

            return new CreateConversationResultApiModel()
            {
                    CreatedConversation = new ConversationTemplate()
                    {
                        ConversationID = ConversationToAdd.ConvID,
                        DialogueUser = (ConversationToAdd.IsGroup) ?
                          null :
                          new UserInfo()
                          {
                              Id = SecondDialogueUser.Id,
                              Name = SecondDialogueUser.FirstName,
                              ProfilePicImageUrl = SecondDialogueUser.ProfilePicImageURL,
                              LastName = SecondDialogueUser.LastName,
                              LastSeen = SecondDialogueUser.LastSeen,
                              UserName = SecondDialogueUser.UserName,
                              ProfilePicRgb = SecondDialogueUser.ProfilePicRgb,
                              ConnectionId = SecondDialogueUser.ConnectionId,
                              IsOnline = SecondDialogueUser.IsOnline
                          },

                        ImageUrl = ConversationToAdd.ImageUrl,
                        IsGroup = ConversationToAdd.IsGroup,
                        Name = ConversationToAdd.Name,
                        PictureBackground = ConversationToAdd.PictureBackgroundRgb
                    }
            };


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

            await mContext.SaveChangesAsync().ConfigureAwait(false);
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
                                ProfilePicRgb = DialogueUser.ProfilePicRgb,
                                LastName = DialogueUser.LastName,
                                LastSeen = DialogueUser.LastSeen,
                                UserName = DialogueUser.UserName,
                                ProfilePicImageUrl = DialogueUser.ProfilePicImageURL,
                                IsOnline = DialogueUser.IsOnline,
                                ConnectionId = DialogueUser.ConnectionId
                            },
                            IsGroup = conv.IsGroup,
                            PictureBackground = conv.PictureBackgroundRgb,
                            ImageUrl = conv.ImageUrl,
                            Participants = (await GetConversationParticipants(new GetParticipantsApiModel() { ConvId = conv.ConvID })).Participants,
                            Messages = (await GetConversationMessages(new GetMessagesApiModel() { ConvID = conv.ConvID }, whoAccessedId)).Messages
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
                     ProfilePicRgb = UserConv.User.ProfilePicRgb,
                     ProfilePicImageUrl = UserConv.User.ProfilePicImageURL,
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

            var conversation = await mContext.Conversations.FindAsync(convInfo.ConvID).ConfigureAwait(false);

            if (conversation == null)
                throw defaultErrorMessage;

            var members = await GetConversationParticipants(new GetParticipantsApiModel() { ConvId = convInfo.ConvID });

            //only member of conversation could request messages (there should exist other calls for non-members).

            if (members.Participants.Find(x => x.UserName == whoAccessedId) == null)
                throw unAuthorizedError;

            // get related messages
            var messages = mContext.Messages.Where(msg => msg.ConversationID == convInfo.ConvID).Include(msg => msg.User);

            var MessagesToReturn = (

            from msg in messages
            where msg.ConversationID == convInfo.ConvID
            select new Message()
            {
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
                    ProfilePicRgb = msg.User.ProfilePicRgb,
                    ProfilePicImageUrl = msg.User.ProfilePicImageURL,
                    IsOnline = msg.User.IsOnline,
                    ConnectionId = msg.User.ConnectionId
                }
            }).ToList();

            return new GetMessagesResultApiModel()
            {
                Messages = MessagesToReturn
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
                        ProfilePicImageUrl = user.ProfilePicImageURL,
                        LastName = user.LastName,
                        LastSeen = user.LastSeen,
                        UserName = user.UserName,
                        ProfilePicRgb = user.ProfilePicRgb,
                        IsOnline = user.IsOnline,
                        ConnectionId = user.ConnectionId
                    },
                    ImageUrl = conversation.ImageUrl,
                    IsGroup = conversation.IsGroup,
                    Name = conversation.Name,
                    PictureBackground = conversation.PictureBackgroundRgb
                }

            };

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
                    ProfilePicImageUrl = user.ProfilePicImageURL,
                    ProfilePicRgb = user.ProfilePicRgb,
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
                ProfilePicRgb = BackgroundColors.GetProfilePicRgb(),
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
                    ProfilePicImageUrl = FoundUser.ProfilePicImageURL,
                    LastName = FoundUser.LastName,
                    LastSeen = FoundUser.LastSeen,
                    Name = FoundUser.FirstName,
                    UserName = FoundUser.UserName,
                    ProfilePicRgb = FoundUser.ProfilePicRgb,
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
                    ProfilePicRgb = FoundUser.ProfilePicRgb,
                    FirstName = FoundUser.FirstName,
                    LastName = FoundUser.LastName
                }).ToList()
            };
        }
        #endregion
    }
}
