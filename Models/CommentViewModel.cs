namespace Swish.Models
{
    public class CommentViewModel
    {
        public int ComID { get; set; }
        public string UName { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string PPicPath { get; set; }
        public string CommentTxt { get; set; }
        public int CommentLikes { get; set; }
        public bool Deleted { get; set; }
    }
}