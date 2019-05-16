﻿using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IConversationRepository
    {
        Task<ConversationDataModel> Add(bool IsGroup, string name, string imageUrl);
        ConversationDataModel GetById(int id);

        void Remove(ConversationDataModel entity);
    }
}