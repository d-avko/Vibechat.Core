﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeChat.Web
{
    public class UsersConversationDataModel
    {
        public string UserID { get; set; }

        public int ChatID { get; set; }

        [ForeignKey("UserID")]
        public virtual AppUser User { get; set; }

        [ForeignKey("ChatID")]
        public virtual ConversationDataModel Conversation { get; set; }

    }
}
