using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Vibechat.Web.Migrations
{
    public partial class postgreDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "AspNetRoles",
                table => new
                {
                    Id = table.Column<string>(),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_AspNetRoles", x => x.Id); });

            migrationBuilder.CreateTable(
                "AspNetUsers",
                table => new
                {
                    Id = table.Column<string>(),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(),
                    TwoFactorEnabled = table.Column<bool>(),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(),
                    AccessFailedCount = table.Column<int>(),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    LastSeen = table.Column<DateTime>(),
                    ProfilePicImageURL = table.Column<string>(nullable: true),
                    FullImageUrl = table.Column<string>(nullable: true),
                    ConnectionId = table.Column<string>(nullable: true),
                    IsOnline = table.Column<bool>(),
                    IsPublic = table.Column<bool>(),
                    RefreshToken = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_AspNetUsers", x => x.Id); });

            migrationBuilder.CreateTable(
                "AttachmentKinds",
                table => new
                {
                    Name = table.Column<string>()
                },
                constraints: table => { table.PrimaryKey("PK_AttachmentKinds", x => x.Name); });

            migrationBuilder.CreateTable(
                "PublicKeys",
                table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Modulus = table.Column<string>(nullable: true),
                    Generator = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_PublicKeys", x => x.Id); });

            migrationBuilder.CreateTable(
                "Settings",
                table => new
                {
                    Id = table.Column<string>(),
                    Name = table.Column<string>(maxLength: 256),
                    Value = table.Column<string>(maxLength: 2048)
                },
                constraints: table => { table.PrimaryKey("PK_Settings", x => x.Id); });

            migrationBuilder.CreateTable(
                "AspNetRoleClaims",
                table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    RoleId = table.Column<string>(),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        x => x.RoleId,
                        "AspNetRoles",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "AspNetUserClaims",
                table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<string>(),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        "FK_AspNetUserClaims_AspNetUsers_UserId",
                        x => x.UserId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "AspNetUserLogins",
                table => new
                {
                    LoginProvider = table.Column<string>(),
                    ProviderKey = table.Column<string>(),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new {x.LoginProvider, x.ProviderKey});
                    table.ForeignKey(
                        "FK_AspNetUserLogins_AspNetUsers_UserId",
                        x => x.UserId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "AspNetUserRoles",
                table => new
                {
                    UserId = table.Column<string>(),
                    RoleId = table.Column<string>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new {x.UserId, x.RoleId});
                    table.ForeignKey(
                        "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        x => x.RoleId,
                        "AspNetRoles",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_AspNetUserRoles_AspNetUsers_UserId",
                        x => x.UserId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "AspNetUserTokens",
                table => new
                {
                    UserId = table.Column<string>(),
                    LoginProvider = table.Column<string>(),
                    Name = table.Column<string>(),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new {x.UserId, x.LoginProvider, x.Name});
                    table.ForeignKey(
                        "FK_AspNetUserTokens_AspNetUsers_UserId",
                        x => x.UserId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "Contacts",
                table => new
                {
                    FirstUserID = table.Column<string>(),
                    SecondUserID = table.Column<string>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => new {x.FirstUserID, x.SecondUserID});
                    table.ForeignKey(
                        "FK_Contacts_AspNetUsers_FirstUserID",
                        x => x.FirstUserID,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_Contacts_AspNetUsers_SecondUserID",
                        x => x.SecondUserID,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "UsersBans",
                table => new
                {
                    BannedID = table.Column<string>(),
                    BannedByID = table.Column<string>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersBans", x => new {x.BannedByID, x.BannedID});
                    table.ForeignKey(
                        "FK_UsersBans_AspNetUsers_BannedByID",
                        x => x.BannedByID,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_UsersBans_AspNetUsers_BannedID",
                        x => x.BannedID,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "Conversations",
                table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    ThumbnailUrl = table.Column<string>(nullable: true),
                    FullImageUrl = table.Column<string>(nullable: true),
                    IsGroup = table.Column<bool>(),
                    IsPublic = table.Column<bool>(),
                    IsSecure = table.Column<bool>(),
                    AuthKeyId = table.Column<string>(nullable: true),
                    CreatorId = table.Column<string>(nullable: true),
                    PublicKeyId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        "FK_Conversations_AspNetUsers_CreatorId",
                        x => x.CreatorId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Conversations_PublicKeys_PublicKeyId",
                        x => x.PublicKeyId,
                        "PublicKeys",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "ConversationsBans",
                table => new
                {
                    UserID = table.Column<string>(),
                    ChatID = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationsBans", x => new {x.ChatID, x.UserID});
                    table.ForeignKey(
                        "FK_ConversationsBans_Conversations_ChatID",
                        x => x.ChatID,
                        "Conversations",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_ConversationsBans_AspNetUsers_UserID",
                        x => x.UserID,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "Messages",
                table => new
                {
                    MessageID = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    State = table.Column<int>(),
                    UserId = table.Column<string>(nullable: true),
                    MessageContent = table.Column<string>(nullable: true),
                    IsAttachment = table.Column<bool>(),
                    ForwardedMessageMessageID = table.Column<int>(nullable: true),
                    ConversationID = table.Column<int>(),
                    TimeReceived = table.Column<DateTime>(),
                    EncryptedPayload = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageID);
                    table.ForeignKey(
                        "FK_Messages_Conversations_ConversationID",
                        x => x.ConversationID,
                        "Conversations",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_Messages_Messages_ForwardedMessageMessageID",
                        x => x.ForwardedMessageMessageID,
                        "Messages",
                        "MessageID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_Messages_AspNetUsers_UserId",
                        x => x.UserId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "UsersConversations",
                table => new
                {
                    UserID = table.Column<string>(),
                    ChatID = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersConversations", x => new {x.UserID, x.ChatID});
                    table.ForeignKey(
                        "FK_UsersConversations_Conversations_ChatID",
                        x => x.ChatID,
                        "Conversations",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_UsersConversations_AspNetUsers_UserID",
                        x => x.UserID,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "Attachments",
                table => new
                {
                    AttachmentID = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ContentUrl = table.Column<string>(nullable: true),
                    AttachmentName = table.Column<string>(nullable: true),
                    ImageWidth = table.Column<int>(),
                    ImageHeight = table.Column<int>(),
                    AttachmentKindName = table.Column<string>(nullable: true),
                    MessageId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.AttachmentID);
                    table.ForeignKey(
                        "FK_Attachments_AttachmentKinds_AttachmentKindName",
                        x => x.AttachmentKindName,
                        "AttachmentKinds",
                        "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Attachments_Messages_MessageId",
                        x => x.MessageId,
                        "Messages",
                        "MessageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "DeletedMessages",
                table => new
                {
                    MessageID = table.Column<int>(),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedMessages", x => x.MessageID);
                    table.ForeignKey(
                        "FK_DeletedMessages_Messages_MessageID",
                        x => x.MessageID,
                        "Messages",
                        "MessageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                "AttachmentKinds",
                "Name",
                "img");

            migrationBuilder.InsertData(
                "PublicKeys",
                new[] {"Id", "Generator", "Modulus"},
                new object[,]
                {
                    {
                        1, "5",
                        "30445704021091515043589705032000416743065879523138206374507066714396902967826274025631618229965138647563750555154979126280906579122837150373480202328346842570774085859182345981586646516929523275488501196012664339665000259691166799681559154401985737875054416305014473301299223214280232526356243298947531967598860851745809118777180045083632452217526977904029644599066308131296163393000164130258492650715234142089972808359667482827732851432028411017393736987205804161070813960388610459820505679155787472727171508004197344905155937150738660205704682022704155225348753967503171179256418887643829768064151315264948430447643"
                    },
                    {
                        2, "5",
                        "32263366888482059323243883015844747463965633534460381695780102759488099321032708334304505667666637685300505592369993636530815881102308957048371822607863954733817431065032603081782824700230973378782698023906415337615400031203101471874900241507489668722465641666896931753727311596566091264518919851218789756402628304989296109806612534970945915938124598750842390238411160204981182640889415926127674121044185527248690460313606448767420374921388319398313924659508283461791477125909269820467245948516679916104219176576965841073678423303547601277304956836098698284063305292637873086496892392420324931515705463719457818391903"
                    },
                    {
                        3, "5",
                        "24686716193249294655435300797497114211078959859276664073733717026830954181574388585315694045547212768961613342303599456962906305799342423705644564610104313711924785307130075216907652332970869326682805755857773148743676260028590179245424907542602604826824659365364084160548368532184623254146224405505128422317198793170098375902826740530739724436141448566436832702729067122102229640982472203935696282671800474814097452256414678537282324994774580722689119929861200126328720274469821909551794989267104991749216243041288976425117086205131154327259025718994944932831286131755855732747670825247733961413482774928847145848683"
                    },
                    {
                        4, "5",
                        "20511064139841876696208229254000064895498308708985023576653435535421416417594516774924493827744623544847238987846574744361120463310357785783241148249314307516370165566636708802131190227740623365100379446244224186700951777543056890843857560451632206080364698149346737149295461623896888713887833761226282705077573490187927513477344537509706489902786608552066156656101651000143551974090549989153259917689688908445021764937236819290246070592801026626071949237468671408951883181725105134543274800285933845010517241736518635511601891749900728648705430139444855907139519170754104572729308445072787726322570780369556422739723"
                    },
                    {
                        5, "5",
                        "20850965393100772630721381827467472846769974337484934948485962234352705295662792015939788728305700806819038412100331548613067641292812785261491147385725382284428429766310618521703081552913546975030866816165893472445277829294748965421857329404134415647286470036472641032781856997861782897278603448824696922997054565547655636184219624327030090189311608152965483673354384130916454486847897115874140265827339554177255203771867546072260275858144383592611464594638476890150964299365002135998606866470198012882210416522022871618334038010682652038038493600763843805286318397247480947350931175656728334601366362069270360295443"
                    }
                });

            migrationBuilder.CreateIndex(
                "IX_AspNetRoleClaims_RoleId",
                "AspNetRoleClaims",
                "RoleId");

            migrationBuilder.CreateIndex(
                "RoleNameIndex",
                "AspNetRoles",
                "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_AspNetUserClaims_UserId",
                "AspNetUserClaims",
                "UserId");

            migrationBuilder.CreateIndex(
                "IX_AspNetUserLogins_UserId",
                "AspNetUserLogins",
                "UserId");

            migrationBuilder.CreateIndex(
                "IX_AspNetUserRoles_RoleId",
                "AspNetUserRoles",
                "RoleId");

            migrationBuilder.CreateIndex(
                "EmailIndex",
                "AspNetUsers",
                "NormalizedEmail");

            migrationBuilder.CreateIndex(
                "UserNameIndex",
                "AspNetUsers",
                "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_Attachments_AttachmentKindName",
                "Attachments",
                "AttachmentKindName");

            migrationBuilder.CreateIndex(
                "IX_Attachments_MessageId",
                "Attachments",
                "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_Contacts_SecondUserID",
                "Contacts",
                "SecondUserID");

            migrationBuilder.CreateIndex(
                "IX_Conversations_CreatorId",
                "Conversations",
                "CreatorId");

            migrationBuilder.CreateIndex(
                "IX_Conversations_PublicKeyId",
                "Conversations",
                "PublicKeyId");

            migrationBuilder.CreateIndex(
                "IX_ConversationsBans_UserID",
                "ConversationsBans",
                "UserID");

            migrationBuilder.CreateIndex(
                "IX_Messages_ConversationID",
                "Messages",
                "ConversationID");

            migrationBuilder.CreateIndex(
                "IX_Messages_ForwardedMessageMessageID",
                "Messages",
                "ForwardedMessageMessageID");

            migrationBuilder.CreateIndex(
                "IX_Messages_UserId",
                "Messages",
                "UserId");

            migrationBuilder.CreateIndex(
                "IX_Settings_Name",
                "Settings",
                "Name");

            migrationBuilder.CreateIndex(
                "IX_UsersBans_BannedID",
                "UsersBans",
                "BannedID");

            migrationBuilder.CreateIndex(
                "IX_UsersConversations_ChatID",
                "UsersConversations",
                "ChatID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "AspNetRoleClaims");

            migrationBuilder.DropTable(
                "AspNetUserClaims");

            migrationBuilder.DropTable(
                "AspNetUserLogins");

            migrationBuilder.DropTable(
                "AspNetUserRoles");

            migrationBuilder.DropTable(
                "AspNetUserTokens");

            migrationBuilder.DropTable(
                "Attachments");

            migrationBuilder.DropTable(
                "Contacts");

            migrationBuilder.DropTable(
                "ConversationsBans");

            migrationBuilder.DropTable(
                "DeletedMessages");

            migrationBuilder.DropTable(
                "Settings");

            migrationBuilder.DropTable(
                "UsersBans");

            migrationBuilder.DropTable(
                "UsersConversations");

            migrationBuilder.DropTable(
                "AspNetRoles");

            migrationBuilder.DropTable(
                "AttachmentKinds");

            migrationBuilder.DropTable(
                "Messages");

            migrationBuilder.DropTable(
                "Conversations");

            migrationBuilder.DropTable(
                "AspNetUsers");

            migrationBuilder.DropTable(
                "PublicKeys");
        }
    }
}