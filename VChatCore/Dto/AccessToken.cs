using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VChatCore.Dto
{
    public class AccessToken
    {
        public string User { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string Token { get; set; }
    }
}
