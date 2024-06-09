using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace VChatCore.Model
{
    [Table("GroupUser")]
    public class GroupUser
    {
        public long Id { get; set; }
        public string GroupCode { get; set; }
        public string UserCode { get; set; }

        public virtual Group Group { get; set; }
        public virtual User User { get; set; }
    }
}