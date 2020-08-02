using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Swish.Models
{
    public class PostImages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageID { get; set; }
        public string ImageFileName { get; set; }
        public bool IsVideo { get; set; }
        public PostModel postModel { get; set; }
    }
}
