using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Vibechat.BusinessLogic.Auth;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.BusinessLogic.Middleware;
using Vibechat.BusinessLogic.Services.Bans;
using Vibechat.BusinessLogic.Services.Chat;
using Vibechat.BusinessLogic.Services.ChatDataProviders;
using Vibechat.BusinessLogic.Services.Crypto;
using Vibechat.BusinessLogic.Services.FileSystem;
using Vibechat.BusinessLogic.Services.Hashing;
using Vibechat.BusinessLogic.Services.Images;
using Vibechat.BusinessLogic.Services.Login;
using Vibechat.BusinessLogic.Services.Messages;
using Vibechat.BusinessLogic.Services.Paths;
using Vibechat.BusinessLogic.Services.Users;
using Vibechat.BusinessLogic.UserProviders;
using Vibechat.DataLayer.Repositories;
using Vibechat.Shared.DTO.Conversations;

namespace Vibechat.BusinessLogic.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddDefaultServices(this IServiceCollection services)
        {
            services.AddScoped<ChatService, ChatService>();
            services.AddScoped<UsersService, UsersService>();
            services.AddScoped<LoginService, LoginService>();
            services.AddScoped<FilesService, FilesService>();
            services.AddScoped<BansService, BansService>();
            services.AddScoped<MessagesService, MessagesService>();
        }

        public static void AddDefaultMiddleware(this IServiceCollection services)
        {
            services.AddScoped<UserStatusMiddleware, UserStatusMiddleware>();
        }

        public static void AddDefaultRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IMessagesRepository, MessagesRepository>();
            services.AddScoped<IAttachmentKindsRepository, AttachmentKindsRepository>();
            services.AddScoped<IUsersConversationsRepository, UsersConversationsRepository>();
            services.AddScoped<IConversationRepository, ConversationsRepository>();
            services.AddScoped<IConversationsBansRepository, ConversationsBansRepository>();
            services.AddScoped<IUsersBansRepository, UsersBansRepository>();
            services.AddScoped<IContactsRepository, ContactsRepository>();
            services.AddScoped<IDhPublicKeysRepository, DhPublicKeysRepository>();
            services.AddScoped<IChatRolesRepository, ChatRolesRepository>();
            services.AddScoped<ILastMessagesRepository, LastMessagesRepository>();
            services.AddScoped<IChatEventsRepository, ChatEventsRepository>();
            services.AddScoped<IAttachmentsRepository ,AttachmentsRepository>();
            services.AddScoped<IRolesRepository, RolesRepository>();
            services.AddScoped<IDeletedMessagesRepository, DeletedMessagesRepository>();
        }

        public static void AddBusinessLogic(this IServiceCollection services)
        {
            services.AddSingleton<ICustomHubUserIdProvider, DefaultUserIdProvider>();
            services.AddScoped<ITokenValidator, JwtTokenValidator>();
            services.AddScoped<JwtSecurityTokenHandler, JwtSecurityTokenHandler>();
            services.AddSingleton<IChatDataProvider, DefaultChatDataProvider>();
            services.AddSingleton<IImageCompressionService, ImageCompressionService>();
            services.AddSingleton<IImageScalingService, ImageCompressionService>();
            services.AddSingleton<IHexHashingService, Sha1Service>();
            services.AddSingleton<UniquePathsProvider, UniquePathsProvider>();
            services.AddScoped<CryptoService, CryptoService>();
            services.AddSingleton<UsersSubscriptionService, UsersSubscriptionService>();
            services.AddScoped<UnitOfWork, UnitOfWork>();
            services.AddScoped<IComparer<Chat>, ChatComparer>();
            services.AddSingleton<UserCultureService, UserCultureService>();
        }

        public static void AddAuthServices(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("PublicApi", policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.Requirements.Add(new PublicApiRequirement());
                });

                options.AddPolicy("PrivateApi", policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.Requirements.Add(new PrivateApiRequirement());
                });
            });

            services.AddScoped<IAuthorizationHandler, PublicApiAuthHandler>();
            services.AddScoped<IAuthorizationHandler, PrivateApiAuthHandler>();
        }
    }
}