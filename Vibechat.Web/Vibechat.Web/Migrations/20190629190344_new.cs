using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class @new : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    LastSeen = table.Column<DateTime>(nullable: false),
                    ProfilePicImageURL = table.Column<string>(nullable: true),
                    FullImageUrl = table.Column<string>(nullable: true),
                    ConnectionId = table.Column<string>(nullable: true),
                    IsOnline = table.Column<bool>(nullable: false),
                    IsPublic = table.Column<bool>(nullable: false),
                    RefreshToken = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentKinds",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentKinds", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "PublicKeys",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Modulus = table.Column<string>(nullable: true),
                    Generator = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    Value = table.Column<string>(maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    FirstUserID = table.Column<string>(nullable: false),
                    SecondUserID = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => new { x.FirstUserID, x.SecondUserID });
                    table.ForeignKey(
                        name: "FK_Contacts_AspNetUsers_FirstUserID",
                        column: x => x.FirstUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Contacts_AspNetUsers_SecondUserID",
                        column: x => x.SecondUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    State = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    MessageContent = table.Column<string>(nullable: true),
                    IsAttachment = table.Column<bool>(nullable: false),
                    ForwardedMessageMessageID = table.Column<int>(nullable: true),
                    ConversationID = table.Column<int>(nullable: false),
                    TimeReceived = table.Column<DateTime>(nullable: false),
                    EncryptedPayload = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageID);
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ForwardedMessageMessageID",
                        column: x => x.ForwardedMessageMessageID,
                        principalTable: "Messages",
                        principalColumn: "MessageID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "UsersBans",
                columns: table => new
                {
                    BannedID = table.Column<string>(nullable: false),
                    BannedByID = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersBans", x => new { x.BannedByID, x.BannedID });
                    table.ForeignKey(
                        name: "FK_UsersBans_AspNetUsers_BannedByID",
                        column: x => x.BannedByID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_UsersBans_AspNetUsers_BannedID",
                        column: x => x.BannedID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    ThumbnailUrl = table.Column<string>(nullable: true),
                    FullImageUrl = table.Column<string>(nullable: true),
                    IsGroup = table.Column<bool>(nullable: false),
                    IsPublic = table.Column<bool>(nullable: false),
                    IsSecure = table.Column<bool>(nullable: false),
                    AuthKeyId = table.Column<string>(nullable: true),
                    CreatorId = table.Column<string>(nullable: true),
                    PublicKeyId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Conversations_PublicKeys_PublicKeyId",
                        column: x => x.PublicKeyId,
                        principalTable: "PublicKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    AttachmentID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContentUrl = table.Column<string>(nullable: true),
                    AttachmentName = table.Column<string>(nullable: true),
                    ImageWidth = table.Column<int>(nullable: false),
                    ImageHeight = table.Column<int>(nullable: false),
                    AttachmentKindName = table.Column<string>(nullable: true),
                    MessageId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.AttachmentID);
                    table.ForeignKey(
                        name: "FK_Attachments_AttachmentKinds_AttachmentKindName",
                        column: x => x.AttachmentKindName,
                        principalTable: "AttachmentKinds",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attachments_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "MessageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeletedMessages",
                columns: table => new
                {
                    MessageID = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedMessages", x => x.MessageID);
                    table.ForeignKey(
                        name: "FK_DeletedMessages_Messages_MessageID",
                        column: x => x.MessageID,
                        principalTable: "Messages",
                        principalColumn: "MessageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationsBans",
                columns: table => new
                {
                    UserID = table.Column<string>(nullable: false),
                    ChatID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationsBans", x => new { x.ChatID, x.UserID });
                    table.ForeignKey(
                        name: "FK_ConversationsBans_Conversations_ChatID",
                        column: x => x.ChatID,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConversationsBans_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "UsersConversations",
                columns: table => new
                {
                    UserID = table.Column<string>(nullable: false),
                    ChatID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersConversations", x => new { x.UserID, x.ChatID });
                    table.ForeignKey(
                        name: "FK_UsersConversations_Conversations_ChatID",
                        column: x => x.ChatID,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsersConversations_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.InsertData(
                table: "AttachmentKinds",
                column: "Name",
                value: "img");

            migrationBuilder.InsertData(
                table: "PublicKeys",
                columns: new[] { "Id", "Generator", "Modulus" },
                values: new object[,]
                {
                    { 1, "5", "30445704021091515043589705032000416743065879523138206374507066714396902967826274025631618229965138647563750555154979126280906579122837150373480202328346842570774085859182345981586646516929523275488501196012664339665000259691166799681559154401985737875054416305014473301299223214280232526356243298947531967598860851745809118777180045083632452217526977904029644599066308131296163393000164130258492650715234142089972808359667482827732851432028411017393736987205804161070813960388610459820505679155787472727171508004197344905155937150738660205704682022704155225348753967503171179256418887643829768064151315264948430447643" },
                    { 2, "5", "32263366888482059323243883015844747463965633534460381695780102759488099321032708334304505667666637685300505592369993636530815881102308957048371822607863954733817431065032603081782824700230973378782698023906415337615400031203101471874900241507489668722465641666896931753727311596566091264518919851218789756402628304989296109806612534970945915938124598750842390238411160204981182640889415926127674121044185527248690460313606448767420374921388319398313924659508283461791477125909269820467245948516679916104219176576965841073678423303547601277304956836098698284063305292637873086496892392420324931515705463719457818391903" },
                    { 3, "5", "24686716193249294655435300797497114211078959859276664073733717026830954181574388585315694045547212768961613342303599456962906305799342423705644564610104313711924785307130075216907652332970869326682805755857773148743676260028590179245424907542602604826824659365364084160548368532184623254146224405505128422317198793170098375902826740530739724436141448566436832702729067122102229640982472203935696282671800474814097452256414678537282324994774580722689119929861200126328720274469821909551794989267104991749216243041288976425117086205131154327259025718994944932831286131755855732747670825247733961413482774928847145848683" },
                    { 4, "5", "20511064139841876696208229254000064895498308708985023576653435535421416417594516774924493827744623544847238987846574744361120463310357785783241148249314307516370165566636708802131190227740623365100379446244224186700951777543056890843857560451632206080364698149346737149295461623896888713887833761226282705077573490187927513477344537509706489902786608552066156656101651000143551974090549989153259917689688908445021764937236819290246070592801026626071949237468671408951883181725105134543274800285933845010517241736518635511601891749900728648705430139444855907139519170754104572729308445072787726322570780369556422739723" },
                    { 5, "5", "20850965393100772630721381827467472846769974337484934948485962234352705295662792015939788728305700806819038412100331548613067641292812785261491147385725382284428429766310618521703081552913546975030866816165893472445277829294748965421857329404134415647286470036472641032781856997861782897278603448824696922997054565547655636184219624327030090189311608152965483673354384130916454486847897115874140265827339554177255203771867546072260275858144383592611464594638476890150964299365002135998606866470198012882210416522022871618334038010682652038038493600763843805286318397247480947350931175656728334601366362069270360295443" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_AttachmentKindName",
                table: "Attachments",
                column: "AttachmentKindName");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_MessageId",
                table: "Attachments",
                column: "MessageId",
                unique: true,
                filter: "[MessageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_SecondUserID",
                table: "Contacts",
                column: "SecondUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CreatorId",
                table: "Conversations",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_PublicKeyId",
                table: "Conversations",
                column: "PublicKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationsBans_UserID",
                table: "ConversationsBans",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ForwardedMessageMessageID",
                table: "Messages",
                column: "ForwardedMessageMessageID");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UserId",
                table: "Messages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Name",
                table: "Settings",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_UsersBans_BannedID",
                table: "UsersBans",
                column: "BannedID");

            migrationBuilder.CreateIndex(
                name: "IX_UsersConversations_ChatID",
                table: "UsersConversations",
                column: "ChatID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "ConversationsBans");

            migrationBuilder.DropTable(
                name: "DeletedMessages");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "UsersBans");

            migrationBuilder.DropTable(
                name: "UsersConversations");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AttachmentKinds");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "PublicKeys");
        }
    }
}
