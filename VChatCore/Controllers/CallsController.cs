using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VChatCore.Dto;
using VChatCore.Service;

namespace VChatCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallsController : ControllerBase
    {
        private CallService _callService;
        private readonly IHttpContextAccessor _contextAccessor;

        public CallsController(CallService callService, IHttpContextAccessor contextAccessor)
        {
            this._callService = callService;
            this._contextAccessor = contextAccessor;
        }

        [Route("call/{userCode}")]
        [HttpGet]
        public IActionResult Call(string userCode)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(this._contextAccessor);
                responseAPI.Data = this._callService.Call(userSession, userCode);
                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("get-history")]
        [HttpGet]
        public IActionResult GetHistory()
        {
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(this._contextAccessor);
                responseAPI.Data = this._callService.GetCallHistory(userSession);
                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("get-history/{key}")]
        [HttpGet]
        public IActionResult GetHistoryById(string key)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(this._contextAccessor);
                responseAPI.Data = this._callService.GetHistoryById(userSession, key);
                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("join-video-call")]
        [HttpGet]
        public IActionResult JoinVideoCall(string url)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(this._contextAccessor);
                this._callService.JoinVideoCall(userSession, url);
                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

        [Route("cancel-video-call")]
        [HttpGet]
        public IActionResult CancelVideoCall(string url)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            try
            {
                string userSession = SystemAuthorization.GetCurrentUser(this._contextAccessor);
                this._callService.CancelVideoCall(userSession, url);
                return Ok(responseAPI);
            }
            catch (Exception ex)
            {
                responseAPI.Message = ex.Message;
                return BadRequest(responseAPI);
            }
        }

    }
}
