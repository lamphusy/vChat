using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VChatCore.Dto;
using VChatCore.Model;

namespace VChatCore.Service
{
    public class CallService
    {
        protected readonly MyContext context;
        private IHubContext<ChatHub> chatHub;
        protected readonly IWebHostEnvironment hostEnvironment;

        public CallService(MyContext context, IWebHostEnvironment hostEnvironment, IHubContext<ChatHub> chatHub)
        {
            this.context = context;
            this.chatHub = chatHub;
            this.hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Danh sách lịch sử cuộc gọi
        /// </summary>
        /// <param name="userSession">User hiện tại đang đăng nhập</param>
        /// <returns>Danh sách lịch sử cuộc gọi</returns>
        public List<GroupCallDto> GetCallHistory(string userSession)
        {
            //danh sách cuộc gọi
            List<GroupCallDto> groupCalls = this.context.GroupCalls
                     .Where(x => x.Calls.Any(y => y.UserCode.Equals(userSession)))
                     .Select(x => new GroupCallDto()
                     {
                         Code = x.Code,
                         Name = x.Name,
                         Avatar = x.Avatar,
                         LastActive = x.LastActive,
                         Calls = x.Calls.OrderByDescending(y => y.Created)
                             .Select(y => new CallDto()
                             {
                                 UserCode = y.UserCode,
                                 User = new UserDto()
                                 {
                                     FullName = y.User.FullName,
                                     Avatar = y.User.Avatar
                                 },
                                 Created = y.Created,
                                 Status = y.Status
                             }).ToList()
                     }).ToList();

            foreach (var group in groupCalls)
            {
                /// hiển thị tên cuộc gọi là người trong cuộc thoại (không phải người đang đang nhập)
                /// VD; A gọi cho B => Màn hình A sẽ nhìn trên danh sách cuộc gọi tên B và ngược lại.

                var us = group.Calls.FirstOrDefault(x => !x.UserCode.Equals(userSession));
                group.Name = us?.User.FullName;
                group.Avatar = us?.User.Avatar;

                group.LastCall = group.Calls.FirstOrDefault();
            }

            return groupCalls.OrderByDescending(x => x.LastActive)
                     .ToList();
        }

        /// <summary>
        /// Thông tin chi tiết lịch sử cuộc gọi
        /// </summary>
        /// <param name="userSession">User hiện tại đang đăng nhập</param>
        /// <param name="groupCallCode">Mã cuộc gọi</param>
        /// <returns>Danh sách cuộc gọi</returns>
        public List<CallDto> GetHistoryById(string userSession, string groupCallCode)
        {
            string friend = this.context.Calls
                .Where(x => x.GroupCallCode.Equals(groupCallCode) && x.UserCode != userSession)
                .Select(x => x.UserCode)
                .FirstOrDefault();

            return this.context.Calls
                .Where(x => x.UserCode.Equals(userSession) && x.GroupCallCode.Equals(groupCallCode))
                .OrderByDescending(x => x.Created)
                .Select(x => new CallDto()
                {
                    Status = x.Status,
                    Created = x.Created,
                    UserCode = friend
                })
                .ToList();
        }

        /// <summary>
        /// Thực hiện cuộc gọi
        /// </summary>
        /// <param name="userSession">User hiện tại đang đăng nhập</param>
        /// <param name="callTo">Người được gọi</param>
        /// <returns>Đường link truy câp cuộc gọi</returns>
        public string Call(string userSession, string callTo)
        {
            string urlVideoCall = GetUrlVideoCall();

            // Lấy thông tin lịch sử cuộc gọi đã gọi cho user
            string grpCallCode = this.context.GroupCalls
                       .Where(x => x.Type.Equals(Constants.GroupType.SINGLE))
                       .Where(x => x.Calls.Any(y => y.UserCode.Equals(userSession) &&
                                   x.Calls.Any(y => y.UserCode.Equals(callTo))))
                       .Select(x => x.Code)
                       .FirstOrDefault();

            GroupCall groupCall = this.context.GroupCalls.FirstOrDefault(x => x.Code.Equals(grpCallCode));
            DateTime dateNow = DateTime.Now;

            User userCallTo = this.context.Users.FirstOrDefault(x => x.Code.Equals(callTo));
            User userCall = this.context.Users.FirstOrDefault(x => x.Code.Equals(userSession));

            // Kiểm tra lịch sử cuộc gọi đã tồn tại hay chưa. Nếu chưa => tạo nhóm gọi mới.
            if (groupCall == null)
            {
                groupCall = new GroupCall()
                {
                    Code = Guid.NewGuid().ToString("N"),
                    Created = dateNow,
                    CreatedBy = userSession,
                    Type = Constants.GroupType.SINGLE,
                    LastActive = dateNow
                };
                this.context.GroupCalls.Add(groupCall);
            }

            /// Thêm danh sách thành viên trong cuộc gọi. Mặc định người gọi trạng thái OUT_GOING
            /// Người được gọi trạng thái MISSED. Nếu người được gọi join vào => CHuyển trạng thái IN_COMING
            /// 

            List<Call> calls = new List<Call>(){
                new Call()
                {
                    GroupCallCode = groupCall.Code,
                    UserCode = userSession,
                    Status =Constants.CallStatus.OUT_GOING,
                    Created = dateNow,
                    Url =urlVideoCall,
                },
                new Call()
                {
                    GroupCallCode = groupCall.Code,
                    UserCode = userCallTo.Code,
                    Status =Constants.CallStatus.MISSED,
                    Created = dateNow,
                    Url =urlVideoCall,
                }
            };

            this.context.Calls.AddRange(calls);
            this.context.SaveChanges();

            ///Truyền thông tin realtime cuộc gọi. Thông tin hubConnection của user.
            if (!string.IsNullOrWhiteSpace(userCallTo.CurrentSession))
                this.chatHub.Clients.Client(userCallTo.CurrentSession).SendAsync("callHubListener", new
                {
                    Url = urlVideoCall,
                    IncomingCallFrom = new
                    {
                        UserName = userCall.UserName
                    }
                });

            return urlVideoCall;
        }

        /// <summary>
        /// Thực hiện cuộc gọi trong nhóm
        /// </summary>
        /// <param name="userSession">Code của người dùng thực hiện khởi tạo cuộc gọi trong nhóm</param>
        /// <param name="groupCode">Code của nhóm</param>
        /// <param name="usersCode">Mời các thành viên tham gia cuộc gọi. Nếu rỗng thì mời tất cả</param>
        /// <returns></returns>
        public async Task<string> CallGroup(string userSession, string groupCode)
        {
            string urlVideoCall = GetUrlVideoCall();

            var group = await this.context.Groups.Include(item => item.GroupUsers)
                .ThenInclude(item => item.User)
                .FirstOrDefaultAsync(item => item.Code == groupCode);
            if (group == null)
                throw new ArgumentException("Nhóm không tồn tại");

            // Lấy thông tin lịch sử cuộc gọi đã gọi trong nhóm

            GroupCall groupCall = this.context.GroupCalls.FirstOrDefault(x => x.Type.Equals(Constants.GroupType.MULTI) && x.Code == group.Code);
            DateTime dateNow = DateTime.Now;

            User userCall = this.context.Users.FirstOrDefault(x => x.Code.Equals(userSession));

            // Kiểm tra lịch sử cuộc gọi đã tồn tại hay chưa. Nếu chưa => tạo nhóm gọi mới.
            if (groupCall == null)
            {
                groupCall = new GroupCall()
                {
                    Code = group.Code,
                    Created = dateNow,
                    CreatedBy = userSession,
                    Type = Constants.GroupType.MULTI,
                    LastActive = dateNow
                };
                this.context.GroupCalls.Add(groupCall);
            }

            /// Thêm danh sách thành viên trong cuộc gọi. Mặc định người gọi trạng thái OUT_GOING
            /// Người được gọi trạng thái MISSED. Nếu người được gọi join vào => CHuyển trạng thái IN_COMING
            List<Call> calls = group.GroupUsers.Select(item => new Call
            {
                GroupCallCode = groupCall.Code,
                UserCode = item.UserCode,
                Status = Constants.CallStatus.MISSED,
                Created = dateNow,
                Url = urlVideoCall
            }).ToList();

            calls.Add(new Call()
            {
                GroupCallCode = groupCall.Code,
                UserCode = userSession,
                Status = Constants.CallStatus.OUT_GOING,
                Created = dateNow,
                Url = urlVideoCall,
            });

            this.context.Calls.AddRange(calls);
            this.context.SaveChanges();

            await this.chatHub.Clients.Group(group.Code).SendAsync("callHubListener", new
            {
                Url = urlVideoCall,
                IncomingCallFrom = new
                {
                    GroupName = group.Name,
                    UserName = userCall.UserName
                }
            });

            return urlVideoCall;
        }

        /// <summary>
        /// Tham gia cuộc gọi. Cập nhật trạng thái cuộc gọi thành IN_COMING
        /// </summary>
        /// <param name="userSession">User hiện tại đang đăng nhập</param>
        /// <param name="url">Đường dẫn truy cập video call</param>
        public void JoinVideoCall(string userSession, string url)
        {
            Call call = this.context.Calls
                .Where(x => x.UserCode.Equals(userSession) && x.Url.Equals(url))
                .FirstOrDefault();

            if (call != null)
            {
                call.Status = Constants.CallStatus.IN_COMMING;
                this.context.SaveChanges();
            }
        }

        /// <summary>
        /// Hủy cuộc gọi
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="url">Đường dẫn truy cập video call</param>
        public void CancelVideoCall(string userSession, string url)
        {
            string urlCall = this.context.Calls
                .Where(x => x.UserCode.Equals(userSession) && x.Url.Equals(url))
                .Select(x => x.Url)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(urlCall))
            {
                try
                {
                    #region gọi API xóa đường dẫn video call trên daily
                    var client = new RestClient($"https://api.daily.co/v1/rooms/{urlCall.Split('/').Last()}");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.DELETE);
                    request.AddHeader("Authorization", $"Bearer {EnviConfig.DailyToken}");
                    IRestResponse response = client.Execute(request);
                    #endregion
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");

                }
            }
        }

        private string GetUrlVideoCall()
        {
            #region Gọi API tạo room - daily.co
            var client = new RestClient("https://api.daily.co/v1/rooms");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", $"Bearer {EnviConfig.DailyToken}");
            IRestResponse response = client.Execute(request);
            DailyRoomResp dailyRoomResp = JsonConvert.DeserializeObject<DailyRoomResp>(response.Content);
            #endregion

            return dailyRoomResp.url;
        }
    }

    /// <summary>
    /// model room
    /// </summary>
    public class DailyRoomResp
    {
        public string id { get; set; }
        public string name { get; set; }
        public string api_created { get; set; }
        public string privacy { get; set; }
        public string url { get; set; }
        public DateTime created_at { get; set; }
    }
}
