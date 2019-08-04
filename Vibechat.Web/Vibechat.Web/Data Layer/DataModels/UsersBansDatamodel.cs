using System.ComponentModel.DataAnnotations.Schema;
using VibeChat.Web;

namespace Vibechat.Web.Data.DataModels
{
    public class UsersBansDatamodel
    {
        public string BannedID { get; set; }

        public string BannedByID { get; set; }

        [ForeignKey("BannedID")] public virtual AppUser BannedUser { get; set; }

        [ForeignKey("BannedByID")] public virtual AppUser BannedBy { get; set; }
    }
}