namespace VChatCore.Dto
{
    public class UserDto
    {
        public string Code { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Dob { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public string Gender { get; set; }

        public bool IsFriend { get; set; }
    }
}