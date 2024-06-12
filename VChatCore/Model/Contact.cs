using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VChatCore.Model
{
    [Table("Contact")]
    public class Contact
    {
        public long Id { get; set; }
        public string UserCode { get; set; }
        public string ContactCode { get; set; }
        public DateTime Created { get; set; }

        public virtual User User { get; set; }

        [ForeignKey("ContactCode")]
        public virtual User UserContact { get; set; }
    }
}