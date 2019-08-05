using System.Collections.Generic;
using VibeChat.Web.ChatData;

namespace Vibechat.Web.Services
{
    public class ChatComparer : IComparer<Chat>
    {
        public int Compare(Chat x, Chat y)
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