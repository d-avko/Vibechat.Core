using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public class DhPublicKeysRepository : IDhPublicKeysRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public DhPublicKeysRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public async Task Add(DhPublicKeyDataModel value)
        {
            mContext.PublicKeys.Add(value);
            await mContext.SaveChangesAsync();
        }

        public async Task<DhPublicKeyDataModel> GetRandomKey()
        {
            var r = new Random();
            var keys = mContext.PublicKeys.ToList();
            return keys[r.Next(0, keys.Count() + 1)];
        }
    }
}
