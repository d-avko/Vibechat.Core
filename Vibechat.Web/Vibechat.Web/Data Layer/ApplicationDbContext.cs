using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VibeChat.Web.Data;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web.Data.DataModels;
using Vibechat.Web.Data_Layer.DataModels;
using Vibechat.Web.Extensions;

namespace VibeChat.Web
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        #region Constructor

        /// <summary>
        ///     Default constructor, expecting database options passed in
        /// </summary>
        /// <param name="options">The database context options</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
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
            
            modelBuilder.SeedData();
        }
    }
}