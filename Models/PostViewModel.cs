using Swish.Areas.Identity.Data;
using System;
using System.Collections.Generic;

namespace Swish.Models
{
    public class PostViewModel
    {
        public int mylike { get; set; }
        public int PostID { get; set; }
        public int PLikes { get; set; }
        public string PostTxt { get; set; }
        public DateTime PostDt { get; set; }
        public List<CommentViewModel> Comments { get; internal set; }
        public SwishUser PosterUser { get; set; }
        public List<pImage> PostImages { get; set; }
    }
    public class pImage
    {
        public string Image { get; set; }
        public bool IsVideo { get; set; }
    }
}