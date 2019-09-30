using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vibechat.DataLayer.DataModels;
using Vibechat.DataLayer.Extensions;

namespace Vibechat.DataLayer
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        private readonly IServiceProvider provider;

        #region Constructor

        /// <summary>
        ///     Default constructor, expecting database options passed in
        /// </summary>
        /// <param name="options">The database context options</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options
        , IServiceProvider provider) : base(options)
        {
            this.provider = provider;
        }

        #endregion

        public DbSet<SettingsDataModel> Settings { get; set; }

        public DbSet<ConversationDataModel> Conversations { get; set; }

        public DbSet<UsersConversationDataModel> UsersConversations { get; set; }

        public DbSet<MessageDataModel> Messages { get; set; }

        public DbSet<DeletedMessagesDataModel> DeletedMessages { get; set; }

        public DbSet<AttachmentKindDataModel> AttachmentKinds { get; set; }

        public DbSet<MessageAttachmentDataModel> Attachments { get; set; }

        public DbSet<ConversationsBansDataModel> ConversationsBans { get; set; }

        public DbSet<UsersBansDatamodel> UsersBans { get; set; }

        public DbSet<ContactsDataModel> Contacts { get; set; }

        public DbSet<DhPublicKeyDataModel> PublicKeys { get; set; }

        public new DbSet<RoleDataModel> Roles { get; set; }

        public DbSet<ChatRoleDataModel> ChatRoles { get; set; }
        
        public DbSet<LastMessageDataModel> LastViewedMessages { get; set; }
        
        public DbSet<ChatEventDataModel> ChatEvents { get; set; }

        public DbSet<UserConnectionDataModel> UserConnections { get; set; }
         
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SettingsDataModel>().HasIndex(a => a.Name);

            modelBuilder.Entity<MessageDataModel>()
                .HasOne(x => x.AttachmentInfo)
                .WithOne(x => x.Message)
                .HasForeignKey(typeof(MessageAttachmentDataModel));

            modelBuilder.Entity<UsersConversationDataModel>()
                .HasKey(x => new {x.UserID, x.ChatID});

            modelBuilder.Entity<UsersBansDatamodel>()
                .HasKey(x => new {x.BannedByID, x.BannedID});

            modelBuilder.Entity<ConversationsBansDataModel>()
                .HasKey(x => new {x.ChatID, x.UserID});

            modelBuilder.Entity<ContactsDataModel>()
                .HasKey(x => new {x.FirstUserID, x.SecondUserID});

            modelBuilder.Entity<ChatRoleDataModel>()
                .HasKey(x => new {x.ChatId, x.UserId});

            modelBuilder.Entity<LastMessageDataModel>()
                .HasKey(x => new {x.ChatID, x.UserID});

            modelBuilder.Entity<AppUser>()
                .HasMany(x => x.Connections)
                .WithOne(x => x.User);

            modelBuilder.Entity<ConversationDataModel>()
                .HasMany(x => x.BannedUsers)
                .WithOne(x => x.Conversation);

            modelBuilder.Entity<ConversationDataModel>()
                .HasMany(x => x.Roles)
                .WithOne(x => x.Chat);

            modelBuilder.Entity<ConversationDataModel>()
                .HasMany(x => x.Participants)
                .WithOne(x => x.Conversation);

            modelBuilder.Entity<ConversationDataModel>()
                .HasMany(x => x.LastMessages)
                .WithOne(x => x.Conversation);
            
            modelBuilder.Entity<MessageDataModel>()
                .HasMany(x => x.DeletedEntries)
                .WithOne(x => x.Message);

            modelBuilder.SeedData();
        }
    }
}