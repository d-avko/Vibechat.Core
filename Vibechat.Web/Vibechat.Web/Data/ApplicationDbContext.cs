﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web.Data;
using VibeChat.Web.Data.DataModels;

namespace VibeChat.Web
{
    public class ApplicationDbContext : IdentityDbContext<UserInApplication>
    {

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

        #region Constructor

        /// <summary>
        /// Default constructor, expecting database options passed in
        /// </summary>
        /// <param name="options">The database context options</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SettingsDataModel>().HasIndex(a => a.Name);

            modelBuilder.Entity<MessageDataModel>()
                .HasOne(x => x.AttachmentInfo)
                .WithOne(x => x.Message)
                .HasForeignKey(typeof(MessageAttachmentDataModel));

            modelBuilder.Entity<UsersConversationDataModel>()
                .HasKey(x => new { x.UserID, x.ChatID });

            modelBuilder.Entity<UsersBansDatamodel>()
                .HasKey(x => new { x.BannedByID, x.BannedID});

            modelBuilder.Entity<ConversationsBansDataModel>()
                .HasKey(x => new { x.ChatID, x.UserID});

            modelBuilder.Entity<ContactsDataModel>()
                .HasKey(x => new { x.FirstUserID, x.SecondUserID}); 
        }
    }
}
