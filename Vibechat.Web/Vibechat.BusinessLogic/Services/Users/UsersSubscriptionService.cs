using System.Collections.Generic;
using System.Linq;

namespace Vibechat.BusinessLogic.Services.Users
{
    public class UsersSubscriptionService
    {
        public UsersSubscriptionService()
        {
            UsersSubscriptions = new Dictionary<string, List<string>>();
        }

        private Dictionary<string, List<string>> UsersSubscriptions { get; }

        /// <summary>
        /// Returns subscribers of specified userId,
        /// or null if there are no any.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<string> GetSubscribers(string userId)
        {
            if (!UsersSubscriptions.ContainsKey(userId))
            {
                return null;
            }

            return UsersSubscriptions[userId];
        }

        /// <summary>
        /// Removes subscriber of specified userId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="subscriber"></param>
        public void RemoveSubscriber(string userId, string subscriber)
        {
            if (!UsersSubscriptions.ContainsKey(userId))
            {
                return;
            }
        
            var subs = UsersSubscriptions[userId];

            if (subs?.FirstOrDefault(x => x == subscriber) != null)
            {
                subs.Remove(subscriber);
            }
        }

        /// <summary>
        /// Subscribes specified subscriber to userId. 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="subscriber"></param>
        public void AddSubscriber(string userId, string subscriber)
        {
            if (!UsersSubscriptions.ContainsKey(userId))
            {
                UsersSubscriptions.Add(userId, new List<string>());
            }

            var subs = UsersSubscriptions[userId];

            if (subs.FirstOrDefault(x => x == subscriber) != default)
            {
                return;
            }

            UsersSubscriptions[userId].Add(subscriber);
        }
    }
}