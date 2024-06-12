using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using VChatCore.Dto;

namespace VChatCore.Controllers
{
    public class SystemAuthorization : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string token = context.HttpContext.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(token))
            {
                ResponseAPI responseAPI = new ResponseAPI();
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                responseAPI.Message = "Lỗi xác thực";
                context.Result = new JsonResult(responseAPI);
            }
            else
            {
                try
                {
                    string tokenValue = token.Replace("Bearer", string.Empty).Trim();
                    ClaimsPrincipal claimsPrincipal = DecodeJWTToken(tokenValue, EnviConfig.SecretKey);
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                catch (SecurityTokenExpiredException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");

                    ResponseAPI responseAPI = new ResponseAPI();
                    context.HttpContext.Response.StatusCode = responseAPI.Status = (int)HttpStatusCode.NotAcceptable;
                    responseAPI.Message = "Hết phiên đăng nhập";
                    context.Result = new JsonResult(responseAPI);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");

                    ResponseAPI responseAPI = new ResponseAPI();
                    context.HttpContext.Response.StatusCode = responseAPI.Status = (int)HttpStatusCode.Unauthorized;
                    responseAPI.Message = "Lỗi xác thực";
                    context.Result = new JsonResult(responseAPI);
                }
            }
        }

        public static string GetCurrentUser(IHttpContextAccessor context)
        {
            try
            {
                string token = context.HttpContext.Request.Headers["Authorization"].ToString();
                string tokenValue = token.Replace("Bearer", string.Empty).Trim();
                ClaimsPrincipal claimsPrincipal = DecodeJWTToken(tokenValue, EnviConfig.SecretKey);
                string userSession = claimsPrincipal.FindFirstValue(ClaimTypes.Sid);
                return userSession;
            }
            catch
            {
                throw new ArgumentException("Lỗi xác thực");
            }
        }

        public static ClaimsPrincipal DecodeJWTToken(string token, string secretAuthKey)
        {
            var key = Encoding.ASCII.GetBytes(secretAuthKey);
            var handler = new JwtSecurityTokenHandler();
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            var claims = handler.ValidateToken(token, validations, out var tokenSecure);
            return claims;
        }
    }
    public class SystemAuthorizeAttribute : TypeFilterAttribute
    {
        public SystemAuthorizeAttribute() : base(typeof(SystemAuthorization))
        {
        }
    }
}
