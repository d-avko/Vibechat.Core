using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Services.Hashing;
using Vibechat.Web.Services.Repositories;

namespace Vibechat.Web.Services.Users
{
    public class SessionsService
    {
        private readonly IHexHashingService hexHashingService;
        private readonly IUsersSessionsRepository usersSessions;
        private readonly IUsersRepository usersRepository;

        public SessionsService(
            IHexHashingService hexHashingService,
            IUsersSessionsRepository usersSessions,
            IUsersRepository usersRepository)
        {
            this.hexHashingService = hexHashingService;
            this.usersSessions = usersSessions;
            this.usersRepository = usersRepository;
        }

        /// <summary>
        /// Gets auth key by id
        /// </summary>
        /// <param name="keyId">id of a key is normally SHA256(key) in hex format</param>
        /// <returns></returns>
        public async Task<string> GetAuthKey(string keyId, string userId)
        {
            var user = await usersRepository.GetById(userId);

            if(user == null)
            {
                throw new ArgumentException("userId");
            }

            IQueryable<SessionDataModel> sessions = usersSessions.GetSessions(user);

            return (from session in sessions
                    where hexHashingService.Hash(session.AuthKey) == keyId
                    select session)
                    .FirstOrDefault()?
                    .AuthKey;
        }
    }
}
