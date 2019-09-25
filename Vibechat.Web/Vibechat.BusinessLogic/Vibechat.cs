using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.BusinessLogic.Extensions;
using Vibechat.DataLayer;
using Vibechat.DataLayer.Repositories;

namespace Vibechat.BusinessLogic
{
    public class Vibechat
    {
        private readonly UserManager<AppUser> users;
        private readonly IConnectionsRepository connections;
        private readonly IJwtTokenGenerator tokenGenerator;
        private readonly UnitOfWork unitOfWork;

        public Vibechat(
            UserManager<AppUser> users, 
            IConnectionsRepository connections, 
            IJwtTokenGenerator tokenGenerator,
            UnitOfWork unitOfWork)
        {
            this.users = users;
            this.connections = connections;
            this.tokenGenerator = tokenGenerator;
            this.unitOfWork = unitOfWork;
        }

        public async Task OnStartup()
        {
            foreach (var user in users.Users)
            {
                user.IsOnline = false;
            }
            
            await connections.ClearAsync();
            
            users.SeedAdminAccount(tokenGenerator);

            await unitOfWork.Commit();
        }
    }
}