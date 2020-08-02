using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Swish.Models
{
    public class PostModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostID { get; set; }
        public DateTime PDate { get; set; } = DateTime.Now;
        public string UserID { get; set; }
        public string PText { get; set; }
        public int PLikes { get; set; }
        public List<PostImages> postImages { get; set; }
    }
}