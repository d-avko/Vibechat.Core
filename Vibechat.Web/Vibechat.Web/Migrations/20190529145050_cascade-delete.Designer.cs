﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VibeChat.Web;

namespace Vibechat.Web.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20190529145050_cascade-delete")]
    partial class cascadedelete
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.11-servicing-32099")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("VibeChat.Web.ConversationDataModel", b =>
                {
                    b.Property<int>("ConvID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("BanForeignKey");

                    b.Property<string>("CreatorId");

                    b.Property<string>("FullImageUrl");

                    b.Property<bool>("IsGroup");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("Name");

                    b.Property<string>("ThumbnailUrl");

                    b.HasKey("ConvID");

                    b.HasIndex("BanForeignKey")
                        .IsUnique();

                    b.HasIndex("CreatorId");

                    b.ToTable("Conversations");
                });

            modelBuilder.Entity("VibeChat.Web.Data.DataModels.AttachmentKindDataModel", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Name");

                    b.ToTable("AttachmentKinds");
                });

            modelBuilder.Entity("Vibechat.Web.Data.DataModels.ConversationsBansDataModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BannedUserId");

                    b.HasKey("Id");

                    b.HasIndex("BannedUserId");

                    b.ToTable("ConversationsBans");
                });

            modelBuilder.Entity("VibeChat.Web.Data.DataModels.MessageAttachmentDataModel", b =>
                {
                    b.Property<int>("AttachmentID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AttachmentKindName");

                    b.Property<string>("AttachmentName");

                    b.Property<string>("ContentUrl");

                    b.Property<int>("ImageHeight");

                    b.Property<int>("ImageWidth");

                    b.HasKey("AttachmentID");

                    b.HasIndex("AttachmentKindName");

                    b.ToTable("Attachments");
                });

            modelBuilder.Entity("Vibechat.Web.Data.DataModels.UsersBansDatamodel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BannedById");

                    b.Property<string>("BannedUserId");

                    b.HasKey("Id");

                    b.HasIndex("BannedById");

                    b.HasIndex("BannedUserId");

                    b.ToTable("UsersBans");
                });

            modelBuilder.Entity("VibeChat.Web.Data.SettingsDataModel", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(2048);

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("VibeChat.Web.DeletedMessagesDataModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("MessageID");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("MessageID");

                    b.ToTable("DeletedMessages");
                });

            modelBuilder.Entity("VibeChat.Web.MessageDataModel", b =>
                {
                    b.Property<int>("MessageID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("AttachmentInfoAttachmentID");

                    b.Property<int>("ConversationID");

                    b.Property<bool>("IsAttachment");

                    b.Property<string>("MessageContent");

                    b.Property<DateTime>("TimeReceived");

                    b.Property<string>("UserId");

                    b.HasKey("MessageID");

                    b.HasIndex("AttachmentInfoAttachmentID");

                    b.HasIndex("UserId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("VibeChat.Web.UserInApplication", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("ConnectionId");

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<bool>("IsOnline");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("LastName");

                    b.Property<DateTime>("LastSeen");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("ProfilePicImageURL");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("VibeChat.Web.UsersConversationDataModel", b =>
                {
                    b.Property<int>("UsersConvsID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("ConversationConvID");

                    b.Property<string>("UserId");

                    b.HasKey("UsersConvsID");

                    b.HasIndex("ConversationConvID");

                    b.HasIndex("UserId");

                    b.ToTable("UsersConversations");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("VibeChat.Web.UserInApplication")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("VibeChat.Web.UserInApplication")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VibeChat.Web.UserInApplication")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("VibeChat.Web.UserInApplication")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VibeChat.Web.ConversationDataModel", b =>
                {
                    b.HasOne("Vibechat.Web.Data.DataModels.ConversationsBansDataModel", "Ban")
                        .WithOne("Conversation")
                        .HasForeignKey("VibeChat.Web.ConversationDataModel", "BanForeignKey")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VibeChat.Web.UserInApplication", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId");
                });

            modelBuilder.Entity("Vibechat.Web.Data.DataModels.ConversationsBansDataModel", b =>
                {
                    b.HasOne("VibeChat.Web.UserInApplication", "BannedUser")
                        .WithMany()
                        .HasForeignKey("BannedUserId");
                });

            modelBuilder.Entity("VibeChat.Web.Data.DataModels.MessageAttachmentDataModel", b =>
                {
                    b.HasOne("VibeChat.Web.Data.DataModels.AttachmentKindDataModel", "AttachmentKind")
                        .WithMany()
                        .HasForeignKey("AttachmentKindName");
                });

            modelBuilder.Entity("Vibechat.Web.Data.DataModels.UsersBansDatamodel", b =>
                {
                    b.HasOne("VibeChat.Web.UserInApplication", "BannedBy")
                        .WithMany()
                        .HasForeignKey("BannedById");

                    b.HasOne("VibeChat.Web.UserInApplication", "BannedUser")
                        .WithMany()
                        .HasForeignKey("BannedUserId");
                });

            modelBuilder.Entity("VibeChat.Web.DeletedMessagesDataModel", b =>
                {
                    b.HasOne("VibeChat.Web.MessageDataModel", "Message")
                        .WithMany()
                        .HasForeignKey("MessageID");
                });

            modelBuilder.Entity("VibeChat.Web.MessageDataModel", b =>
                {
                    b.HasOne("VibeChat.Web.Data.DataModels.MessageAttachmentDataModel", "AttachmentInfo")
                        .WithMany()
                        .HasForeignKey("AttachmentInfoAttachmentID");

                    b.HasOne("VibeChat.Web.UserInApplication", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("VibeChat.Web.UsersConversationDataModel", b =>
                {
                    b.HasOne("VibeChat.Web.ConversationDataModel", "Conversation")
                        .WithMany()
                        .HasForeignKey("ConversationConvID");

                    b.HasOne("VibeChat.Web.UserInApplication", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });
#pragma warning restore 612, 618
        }
    }
}
