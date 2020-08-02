using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Swish.Models
{
    public class FriendsModel
    {
        [Key]
        public string FriendKey { get; set; }
        public string User1 { get; set; }
        public string User2 { get; set; }
        public bool Confirmed { get; set; }
    }

    public class BlockedModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string UserID { get; set; }
        public string OtherUserID { get; set; }
    }
}
