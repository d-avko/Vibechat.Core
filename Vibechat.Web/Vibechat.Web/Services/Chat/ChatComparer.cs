using System.Collections.Generic;
using VibeChat.Web.ChatData;

namespace Vibechat.Web.Services
{
    public class ChatComparer : IComparer<Chat>
    {
        public int Compare(Chat x, Chat y)
        {
            if (x.LastMessage.Id == y.LastMessage.Id)
            {
                return 0;
            }

            return x.LastMessage.Id.CompareTo(y.LastMessage.Id);
        }
    }
}