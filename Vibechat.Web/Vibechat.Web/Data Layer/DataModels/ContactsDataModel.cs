using System.ComponentModel.DataAnnotations.Schema;
using VibeChat.Web;

namespace Vibechat.Web.Data.DataModels
{
    public class ContactsDataModel
    {
        public string FirstUserID { get; set; }

        public string SecondUserID { get; set; }

        [ForeignKey("FirstUserID")] public virtual AppUser User { get; set; }

        [ForeignKey("SecondUserID")] public virtual AppUser Contact { get; set; }

        public static ContactsDataModel Create(string creator, string contact)
        {
            return new ContactsDataModel
            {
                FirstUserID = creator,
                SecondUserID = contact
            };
        }
    }
}