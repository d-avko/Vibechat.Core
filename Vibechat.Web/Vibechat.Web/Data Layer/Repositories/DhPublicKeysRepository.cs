using System;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Data.Repositories
{
    public class DhPublicKeysRepository : IDhPublicKeysRepository
    {
        public DhPublicKeysRepository(ApplicationDbContext dbContext)
        {
            mContext = dbContext;
        }

        private ApplicationDbContext mContext { get; }

        [Obsolete("Keys are now seeded via DbContext")]
        public async Task Add(DhPublicKeyDataModel value)
        {
            mContext.PublicKeys.Add(value);
        }

        public async Task<DhPublicKeyDataModel> GetRandomKey()
        {
            var r = new Random();
            var keys = mContext.PublicKeys.ToList();
            //ids: 1 - 5, indexes : 0 - 4
            return keys[r.Next(0, keys.Count())];
        }
    }
}