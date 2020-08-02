using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Swish.Models
{
    public class NotificationModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MyProperty { get; set; }
        public string SenderUserName { get; set; }
        public string RecieverUserName { get; set; }
        public int NotifType { get; set; }
        public string NotifMessage { get; set; }
        public bool Read { get; set; }
    }
}
