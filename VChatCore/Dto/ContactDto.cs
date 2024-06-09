using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VChatCore.Dto
{
    public class ContactDto
    {
        public long Id { get; set; }
        public string UserCode { get; set; }
        public string ContactCode { get; set; }
        public DateTime Created { get; set; }

        public UserDto User { get; set; }
        public UserDto UserContact { get; set; }
    }
}