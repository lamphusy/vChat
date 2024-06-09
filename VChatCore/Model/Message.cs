using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace VChatCore.Model
{
    [Table("Message")]
    public class Message
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string GroupCode { get; set; }
        public string Content { get; set; }
        public string Path { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }

        public virtual Group Group { get; set; }
        [ForeignKey("CreatedBy")]
        public virtual User UserCreatedBy { get; set; }
    }
}