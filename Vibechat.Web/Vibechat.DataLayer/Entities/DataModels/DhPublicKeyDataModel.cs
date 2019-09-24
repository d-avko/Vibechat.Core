using System.ComponentModel.DataAnnotations;

namespace Vibechat.DataLayer.DataModels
{
    public class DhPublicKeyDataModel
    {
        [Key] public int Id { get; set; }

        public string Modulus { get; set; }

        public string Generator { get; set; }
    }
}