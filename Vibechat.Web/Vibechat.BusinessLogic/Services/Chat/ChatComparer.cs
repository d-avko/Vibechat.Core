using System.Collections.Generic;

namespace Vibechat.BusinessLogic.Services.Chat
{
    public class ChatComparer : IComparer<Shared.DTO.Conversations.Chat>
    {
        public int Compare(Shared.DTO.Conversations.Chat x, Shared.DTO.Conversations.Chat y)
        {
            switch (x.LastMessage)
            {
                case null when y.LastMessage == null:
                    return 0;
                case null when y.LastMessage != null:
                    return 1;
            }
            
            if (x.LastMessage != null && y.LastMessage == null)
            {
                return 0;
            }
            
            if (x.LastMessage.Id == y.LastMessage.Id)
            {
                return 1;
            }

            return x.LastMessage.Id > y.LastMessage.Id ? 0 : 1;
        }
    }
}