using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;

namespace Swish.Models
{
    public class CommentModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentID { get; set; }
        public int PostId { get; set; }
        public string UserID { get; set; }
        public string Comment { get; set; }
        public int CLikes { get; set; }
        public bool Deleted { get; set; }
    }
}
