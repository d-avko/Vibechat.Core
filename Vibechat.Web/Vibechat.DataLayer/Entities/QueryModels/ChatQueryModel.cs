using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Entities.QueryModels
{
    public class ChatQueryModel
    {
        private readonly ConversationDataModel _baseModel;

        public ChatQueryModel(ConversationDataModel baseModel)
        {
            _baseModel = baseModel;
        }

        public string DeviceId { get; set; }

        public ChatRoleDataModel UserRole { get; set; }

        public IEnumerable<AppUser> Participants { get; set; }

        public MessageDataModel LastMessage { get; set; }

        public AppUser GetDialogUser()
        {
            return Participants?.FirstOrDefault();
        }
    }
}
