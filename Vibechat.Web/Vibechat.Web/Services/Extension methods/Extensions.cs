﻿using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VibeChat.Web;
using VibeChat.Web.ChatData;
using Vibechat.Web.ChatData.Messages;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web.Data.DataModels;
using Vibechat.Web.Data.Messages;
using Vibechat.Web.Data_Layer.DataModels;
using Vibechat.Web.Services.Extension_methods;

namespace Vibechat.Web.Extensions
{
    public static class Extensions
    {
        public static UserInfo ToUserInfo(this AppUser user)
        {
            return new UserInfo
            {
                Id = user.Id,
                Name = user.FirstName,
                ImageUrl = user.ProfilePicImageURL,
                FullImageUrl = user.FullImageUrl,
                LastName = user.LastName,
                LastSeen = user.LastSeen.ToUTCString(),
                UserName = user.UserName,
                ConnectionId = user.ConnectionId,
                IsOnline = user.IsOnline,
                IsPublic = user.IsPublic
            };
        }

        public static MessageAttachment ToMessageAttachment(this MessageAttachmentDataModel value)
        {
            return new MessageAttachment
            {
                AttachmentKind = value.AttachmentKind.Kind,
                ContentUrl = value.ContentUrl,
                AttachmentName = value.AttachmentName,
                ImageHeight = value.ImageHeight,
                ImageWidth = value.ImageWidth,
                FileSize = value.FileSize
            };
        }

        public static DhPublicKey ToDhPublicKey(this DhPublicKeyDataModel value)
        {
            return new DhPublicKey
            {
                Generator = value.Generator,
                Modulus = value.Modulus
            };
        }

        public static Chat ToChatDto(
            this ConversationDataModel value,
            List<UserInfo> participants,
            AppUser dialogUser,
            DhPublicKeyDataModel key,
            ChatRoleDataModel chatRole,
            string deviceId,
            int lastMessageId,
            Message lastMessage)
        {
            return new Chat
            {
                Name = value.Name,
                Id = value.Id,
                DialogueUser = dialogUser?.ToUserInfo(),
                IsGroup = value.IsGroup,
                ThumbnailUrl = value.ThumbnailUrl,
                FullImageUrl = value.FullImageUrl,
                Participants = participants,
                AuthKeyId = value.AuthKeyId,
                IsSecure = value.IsSecure,
                PublicKey = key?.ToDhPublicKey(),
                DeviceId = deviceId,
                ChatRole = chatRole.ToChatRole(),
                ClientLastMessageId = lastMessageId,
                LastMessage = lastMessage
            };
        }

        public static Message ToMessage(this MessageDataModel value)
        {
            return new Message
            {
                Id = value.MessageID,
                ConversationID = value.ConversationID,
                MessageContent = value.MessageContent,
                TimeReceived = value.TimeReceived.ToUTCString(),
                User = value.User?.ToUserInfo(),
                AttachmentInfo = value.AttachmentInfo?.ToMessageAttachment(),
                Type = value.Type,
                ForwardedMessage = value.ForwardedMessage?.ToMessage(),
                State = value.State,
                EncryptedPayload = value.EncryptedPayload,
                Event = value.Event?.ToChatEvent()
            };
        }

        public static ChatEvent ToChatEvent(this ChatEventDataModel value)
        {
            return new ChatEvent()
            {
                Actor = value.ActorId,
                Type = value.EventType,
                UserInvolved = value.UserInvolvedId,
                ActorName = value.Actor?.UserName,
                UserInvolvedName = value.UserInvolved?.UserName
            };
        }

        public static ChatRoleDto ToChatRole(this ChatRoleDataModel value)
        {
            return new ChatRoleDto
            {
                ChatId = value.ChatId,
                Role = value.RoleId
            };
        }

        public static void SeedData(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttachmentKindDataModel>().HasData(new AttachmentKindDataModel
            {
                Kind = AttachmentKind.Image
            }, new AttachmentKindDataModel
            {
                Kind = AttachmentKind.File
            });

            modelBuilder.Entity<RoleDataModel>().HasData(new RoleDataModel
            {
                Id = ChatRole.Creator
            }, new RoleDataModel
            {
                Id = ChatRole.Moderator
            }, new RoleDataModel
            {
                Id = ChatRole.NoRole
            });

            modelBuilder.Entity<DhPublicKeyDataModel>().HasData(new DhPublicKeyDataModel
            {
                Id = 1,
                Generator = "5",
                Modulus =
                    "30445704021091515043589705032000416743065879523138206374507066714396902967826274025631618229965138647563750555154979126280906579122837150373480202328346842570774085859182345981586646516929523275488501196012664339665000259691166799681559154401985737875054416305014473301299223214280232526356243298947531967598860851745809118777180045083632452217526977904029644599066308131296163393000164130258492650715234142089972808359667482827732851432028411017393736987205804161070813960388610459820505679155787472727171508004197344905155937150738660205704682022704155225348753967503171179256418887643829768064151315264948430447643"
            }, new DhPublicKeyDataModel
            {
                Id = 2,
                Generator = "5",
                Modulus =
                    "32263366888482059323243883015844747463965633534460381695780102759488099321032708334304505667666637685300505592369993636530815881102308957048371822607863954733817431065032603081782824700230973378782698023906415337615400031203101471874900241507489668722465641666896931753727311596566091264518919851218789756402628304989296109806612534970945915938124598750842390238411160204981182640889415926127674121044185527248690460313606448767420374921388319398313924659508283461791477125909269820467245948516679916104219176576965841073678423303547601277304956836098698284063305292637873086496892392420324931515705463719457818391903"
            }, new DhPublicKeyDataModel
            {
                Id = 3,
                Generator = "5",
                Modulus =
                    "24686716193249294655435300797497114211078959859276664073733717026830954181574388585315694045547212768961613342303599456962906305799342423705644564610104313711924785307130075216907652332970869326682805755857773148743676260028590179245424907542602604826824659365364084160548368532184623254146224405505128422317198793170098375902826740530739724436141448566436832702729067122102229640982472203935696282671800474814097452256414678537282324994774580722689119929861200126328720274469821909551794989267104991749216243041288976425117086205131154327259025718994944932831286131755855732747670825247733961413482774928847145848683"
            }, new DhPublicKeyDataModel
            {
                Id = 4,
                Generator = "5",
                Modulus =
                    "20511064139841876696208229254000064895498308708985023576653435535421416417594516774924493827744623544847238987846574744361120463310357785783241148249314307516370165566636708802131190227740623365100379446244224186700951777543056890843857560451632206080364698149346737149295461623896888713887833761226282705077573490187927513477344537509706489902786608552066156656101651000143551974090549989153259917689688908445021764937236819290246070592801026626071949237468671408951883181725105134543274800285933845010517241736518635511601891749900728648705430139444855907139519170754104572729308445072787726322570780369556422739723"
            }, new DhPublicKeyDataModel
            {
                Id = 5,
                Generator = "5",
                Modulus =
                    "20850965393100772630721381827467472846769974337484934948485962234352705295662792015939788728305700806819038412100331548613067641292812785261491147385725382284428429766310618521703081552913546975030866816165893472445277829294748965421857329404134415647286470036472641032781856997861782897278603448824696922997054565547655636184219624327030090189311608152965483673354384130916454486847897115874140265827339554177255203771867546072260275858144383592611464594638476890150964299365002135998606866470198012882210416522022871618334038010682652038038493600763843805286318397247480947350931175656728334601366362069270360295443"
            });
        }
    }
}