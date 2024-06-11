using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VChatCore.Dto;
using VChatCore.Model;
using VChatCore.Util;

namespace VChatCore.Service
{
    public class UserService
    {
        protected readonly MyContext context;
        protected readonly IWebHostEnvironment hostEnvironment;

        public UserService(MyContext context, IWebHostEnvironment hostEnvironment)
        {
            this.context = context;
            this.hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Lấy thông tin cá nhân của user
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <returns>Thông tin user</returns>
        public UserDto GetProfile(string userCode)
        {
            return this.context.Users
                    .Where(x => x.Code.Equals(userCode))
                    .Select(x => new UserDto()
                    {
                        Code = x.Code,
                        FullName = x.FullName,
                        Address = x.Address,
                        Avatar = x.Avatar,
                        Email = x.Email,
                        Gender = x.Gender,
                        Phone = x.Phone,
                        Dob = x.Dob
                    }).FirstOrDefault();
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="user">Thông tin user</param>
        /// <returns></returns>
        public UserDto UpdateProfile(string userCode, UserDto user)
        {
            User us = this.context.Users
                    .FirstOrDefault(x => x.Code.Equals(userCode));

            if (us != null)
            {
                us.FullName = user.FullName;
                us.Dob = user.Dob;
                us.Email = user.Email;

                if (user.Avatar.Contains("data:image/png;base64,"))
                {
                    string pathAvatar = $"wwwroot/Resource/Avatar/{Guid.NewGuid().ToString("N")}";
                    string pathFile = Path.Combine(this.hostEnvironment.ContentRootPath, pathAvatar);
                    DataHelper.Base64ToImage(user.Avatar.Replace("data:image/png;base64,", ""), pathFile);
                    us.Avatar = user.Avatar = pathAvatar;
                }

                us.Address = user.Address;
                us.Phone = user.Phone;
                us.Gender = user.Gender;

                this.context.SaveChanges();
            }
            return user;
        }

        /// <summary>
        /// Lấy danh sách liên hệ của user
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <returns>Danh sách liên hệ</returns>
        public List<UserDto> GetContact(string userCode)
        {
            return this.context.Contacts
                     .Where(x => x.UserCode.Equals(userCode) || x.ContactCode.Equals(userCode))
                     .OrderBy(x => x.UserContact.FullName)
                     .Select(x => new UserDto()
                     {
                         Avatar = x.UserContact.Avatar,
                         Code = x.UserContact.Code,
                         FullName = x.UserContact.FullName,
                         Address = x.UserContact.Address,
                         Dob = x.UserContact.Dob,
                         Email = x.UserContact.Email,
                         Gender = x.UserContact.Gender,
                         Phone = x.UserContact.Phone
                     }).ToList();
        }

        /// <summary>
        /// Tìm kiếm liên hệ.
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="keySearch">Từ khóa tìm kiếm</param>
        /// <returns></returns>
        public List<UserDto> SearchContact(string userCode, string keySearch)
        {
            if (string.IsNullOrWhiteSpace(keySearch))
                return new List<UserDto>();

            List<ContactDto> friends = context.Contacts
                   .Where(x => x.UserCode.Equals(userCode) || x.ContactCode.Equals(userCode))
                   .Select(x => new ContactDto()
                   {
                       ContactCode = x.ContactCode,
                       UserCode = x.UserCode
                   }).ToList();

            List<UserDto> users = context.Users
                .Where(x => !x.Code.Equals(userCode))
                .Where(x => x.FullName.Contains(keySearch) || x.Phone.Contains(keySearch) || x.Email.Contains(keySearch))
                .OrderBy(x => x.FullName)
                .Select(x => new UserDto()
                {
                    Avatar = x.Avatar,
                    Code = x.Code,
                    FullName = x.FullName,
                }).ToList();

            users.ForEach(x =>
            {
                if (friends.Any(y => y.UserCode.Equals(x.Code) || y.ContactCode.Equals(x.Code)))
                    x.IsFriend = true;
            });

            // Loại bỏ liên hệ đã có
            return users.FindAll(x => !x.IsFriend);
        }

        /// <summary>
        /// Thêm mới liên hệ
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="user">Thông tin liên hệ</param>
        public void AddContact(string userCode, UserDto user)
        {
            Contact contact = new Contact()
            {
                UserCode = userCode,
                ContactCode = user.Code,
                Created = DateTime.Now
            };
            context.Contacts.Add(contact);

            context.SaveChanges();
        }
    }
}
