using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibechat.Web.Services.Users
{
    public class UsersSubsriptionService
    {
        public UsersSubsriptionService()
        {
            UsersSubsriptions = new Dictionary<string, List<string>>();
        }

        private Dictionary<string, List<string>> UsersSubsriptions { get; set; }
        
        public List<string> GetSubscribers(string userId)
        {
            if (!UsersSubsriptions.ContainsKey(userId))
            {
                return null;
            }

            return UsersSubsriptions[userId];
        }

        public void RemoveSubsriber(string userId, string subsriber)
        {
            if (!UsersSubsriptions.ContainsKey(userId))
            {
                return;
            }

            var subs = UsersSubsriptions[userId];

            if(subs != null && subs.Count().Equals(0))
            {
                if(subs.FirstOrDefault(x => x == subsriber) != default)
                {
                    subs.Remove(subsriber);
                }
            }
        }

        public void AddSubsriber(string userId, string subsriber)
        {
            if (!UsersSubsriptions.ContainsKey(userId))
            {
                UsersSubsriptions.Add(userId, new List<string>());
            }
            var subs = UsersSubsriptions[userId];

            if (subs.FirstOrDefault(x => x == subsriber) != default)
            {
                UsersSubsriptions[userId].Remove(subsriber);
            }

            UsersSubsriptions[userId].Add(subsriber);
        }
    }
}
