using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Swish.Models
{
    public class ChatModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string Sender { get; set; }
        public string Reciever { get; set; }
        public string Message { get; set; }
        public string Image { get; set; }
        public DateTime dateTime { get; set; }

    }
}