using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;

namespace VibeChat.Web.Controllers
{
    public class ConversationsController : Controller
    {
        protected ApplicationDbContext mContext;

        protected UserManager<UserInApplication> mUserManager;

        public ConversationsController(ApplicationDbContext context, UserManager<UserInApplication> userManager)
        {
            mContext = context;
            mUserManager = userManager;
        }

        #region Conversations
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/CreateConversation")]
        public async Task<ResponseApiModel<CreateConversationResultApiModel>> CreateConv([FromBody] CreateConversationCredentialsApiModel convInfo)
        {

            var defaultErrorMessage = new ResponseApiModel<CreateConversationResultApiModel>()
            {
                ErrorMessage = "Error while creating the conversation..",
                IsSuccessfull = false
            };

            //if there was no group info or creator info

            if (string.IsNullOrWhiteSpace(convInfo.ConvName) || string.IsNullOrWhiteSpace(convInfo.CreatorId))
            {
                return defaultErrorMessage;
            }

            var user = await mUserManager.FindByIdAsync(convInfo.CreatorId).ConfigureAwait(false);

            if (user == null)
            {
                return defaultErrorMessage;
            }

            if ((!convInfo.IsGroup) && (convInfo.DialogueUserId == null))
            {
                return new ResponseApiModel<CreateConversationResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = "No dialogue user was provided..."
                };
            }

            UserInApplication SecondDialogueUser = null;

            if (convInfo.DialogueUserId != null)
            {
                //if this is a dialogue , find a user with whom to create conversation
                SecondDialogueUser = await mUserManager.FindByIdAsync(convInfo.DialogueUserId);

                if (SecondDialogueUser == null)
                {
                    return defaultErrorMessage;
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
                return defaultErrorMessage;
            }

            await mContext.UsersConversations.AddAsync(new UsersConversationDataModel()
            {
                Conversation = ConversationToAdd,
                User = user
            });

            await mContext.SaveChangesAsync();

            //after saving changes we have Id of our 
            //created conversation in ConversationToAdd variable

            return new ResponseApiModel<CreateConversationResultApiModel>()
            {
                IsSuccessfull = true,
                Response = new CreateConversationResultApiModel()
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
                              ImageUrl = SecondDialogueUser.ProfilePicImageURL,
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
                }
            };


        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/AddToConversation")]
        public async Task<ResponseApiModel<AddToConversationResultApiModel>> AddUserToConversation([FromBody]AddToConversationApiModel UserProvided)
        {

            var defaultErrorMessage = new ResponseApiModel<AddToConversationResultApiModel>()
            {
                IsSuccessfull = false,
                ErrorMessage = "Invalid credentials were provided."
            };


            var FoundConversation = await mContext.Conversations.FindAsync(UserProvided.ConvId).ConfigureAwait(false); ;

            if (FoundConversation == null)
                return defaultErrorMessage;

            var FoundUser = await mUserManager.FindByIdAsync(UserProvided.UserId).ConfigureAwait(false);

            if (FoundUser == null)
                return defaultErrorMessage;

            await mContext.UsersConversations.AddAsync(new UsersConversationDataModel()
            {
                Conversation = FoundConversation,
                User = FoundUser
            }).ConfigureAwait(false);

            await mContext.SaveChangesAsync().ConfigureAwait(false);

            return new ResponseApiModel<AddToConversationResultApiModel>()
            {
                IsSuccessfull = true
            };

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetConversationInfo")]
        public async Task<ResponseApiModel<ConversationInfoResultApiModel>> GetConversationInfo([FromBody] CredentialsForConversationInfoApiModel UserProvided)
        {

            var defaultError = new ResponseApiModel<ConversationInfoResultApiModel>()
            {
                ErrorMessage = "User info provided was not correct.",
                IsSuccessfull = false
            };

            if (UserProvided == null)
                return defaultError;

            if (UserProvided.UserId == null)
                return defaultError;

            var user = await mUserManager.FindByIdAsync(UserProvided.UserId).ConfigureAwait(false);

            if (user == null)
                return defaultError;

            // list containing raw data from db

            var EntitiesList = new List<ConversationDataModel>();

            var UsersConvs = mContext.UsersConversations
                .Include(x => x.Conversation)
                .Include(y => y.User);

            EntitiesList.AddRange(
            from UserConv in UsersConvs
            where UserConv.User.Id == UserProvided.UserId
            select UserConv.Conversation);

            //list to return
            var returnData = new List<ConversationTemplate>();


            // UserInApplication(server) --> UserInfo(in Client app)

            foreach (ConversationDataModel conv in EntitiesList)
            {
                UserInApplication DialogueUser = conv.IsGroup ? null : GetDialogueUser(conv.ConvID, UserProvided.UserId);

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
                                ImageUrl = DialogueUser.ProfilePicImageURL,
                                IsOnline = DialogueUser.IsOnline,
                                ConnectionId = DialogueUser.ConnectionId
                            },
                            IsGroup = conv.IsGroup,
                            PictureBackground = conv.PictureBackgroundRgb,
                            ImageUrl = conv.ImageUrl,                       
                        }
                    );               
            }

            return new ResponseApiModel<ConversationInfoResultApiModel>()
            {
                IsSuccessfull = true,
                Response = new ConversationInfoResultApiModel()
                {
                    Conversations = returnData
                }
            };

        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetConversationParticipants")]
        public async Task<ResponseApiModel<GetParticipantsResultApiModel>> GetConversationParticipants([FromBody] GetParticipantsApiModel convInfo)
        {
            var defaultErrorMessage =
            new ResponseApiModel<GetParticipantsResultApiModel>()
            {
                IsSuccessfull = false,
                ErrorMessage = "Wrong conversation was provided."
            };

            if (convInfo == null)
                return defaultErrorMessage;

            var conversation = await mContext.Conversations.FindAsync(convInfo.ConvId).ConfigureAwait(false);

            if (conversation == null)
                return defaultErrorMessage;

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
                    ImageUrl = UserConv.User.ProfilePicImageURL,
                    ConnectionId = UserConv.User.ConnectionId,
                    IsOnline = UserConv.User.IsOnline
                }).ToList();

            return new ResponseApiModel<GetParticipantsResultApiModel>()
            {
                IsSuccessfull = true,
                Response = new GetParticipantsResultApiModel()
                {
                    Participants = ParticipantsToReturn
                }
            };

        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetConversationMessages")]
        public async Task<ResponseApiModel<GetMessagesResultApiModel>> GetConversationMessages([FromBody] GetMessagesApiModel convInfo)
        {
            var defaultErrorMessage =
            new ResponseApiModel<GetMessagesResultApiModel>()
            {
                IsSuccessfull = false,
                ErrorMessage = "Wrong conversation was provided."
            };

            if (mContext.Messages == null)
                return new ResponseApiModel<GetMessagesResultApiModel>()
                {
                    IsSuccessfull = true,
                    Response = new GetMessagesResultApiModel()
                };

            if (mContext.Messages.FirstOrDefault() == null)
                return new ResponseApiModel<GetMessagesResultApiModel>()
                {
                    IsSuccessfull = true,
                    Response = new GetMessagesResultApiModel()
                };

            if (convInfo == null)
                return defaultErrorMessage;

            var conversation = await mContext.Conversations.FindAsync(convInfo.ConvID).ConfigureAwait(false);

            if (conversation == null)
                return defaultErrorMessage;

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
                    ImageUrl = msg.User.ProfilePicImageURL,
                    IsOnline = msg.User.IsOnline,
                    ConnectionId = msg.User.ConnectionId
                }
            }).ToList();

            return new ResponseApiModel<GetMessagesResultApiModel>()
            {
                IsSuccessfull = true,
                Response = new GetMessagesResultApiModel()
                {
                    Messages = MessagesToReturn
                }
            };

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetConversationInfoById")]
        public async Task<ResponseApiModel<GetConversationByIdResultApiModel>> GetConversationById([FromBody] GetConversationByIdApiModel convInfo)
        {

            var conversation = mContext.Conversations.FirstOrDefault(x => x.ConvID == convInfo.ConversationId);

            var user = new UserInApplication();

            if (conversation.IsGroup)
            {
                user = await mUserManager.FindByIdAsync(convInfo.UserId).ConfigureAwait(false);
            }


            if (conversation == null)
            {
                return new ResponseApiModel<GetConversationByIdResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = "No such conversation was found."
                };
            }

            if (conversation.IsGroup && user == null)
            {
                return new ResponseApiModel<GetConversationByIdResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = "User with such id was not found."
                };
            }

            user = GetDialogueUser(convInfo.ConversationId, user.Id);

            if (user == null)
            {
                return new ResponseApiModel<GetConversationByIdResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = "Unexpected error: no corresponding user in dialogue."
                };

            }

            return new ResponseApiModel<GetConversationByIdResultApiModel>()
            {
                IsSuccessfull = true,
                Response = new GetConversationByIdResultApiModel()
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
                            ProfilePicRgb = user.ProfilePicRgb,
                            IsOnline = user.IsOnline,
                            ConnectionId = user.ConnectionId
                        },
                        ImageUrl = conversation.ImageUrl,
                        IsGroup = conversation.IsGroup,
                        Name = conversation.Name,
                        PictureBackground = conversation.PictureBackgroundRgb
                    }
                }

            };

        }

        #endregion

        #region Helper methods
        /// <summary>
        /// Helper method used to find user with whom 
        /// current user have a dialogue
        /// </summary>
        /// <param name="convId"></param>
        /// <param name="FirstUserInDialogueId"></param>
        /// <returns></returns>
        private UserInApplication GetDialogueUser(int convId, string FirstUserInDialogueId)
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
    }
}
