using Authorization.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;

namespace Authorization.API.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class JwtAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private const String _authentication = "Authorization";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var Request = context.HttpContext.Request;

                if (!Request.Headers.Any(h => h.Key == _authentication))
                {
                    context.Result = new ObjectResult(new ErrorMessage() { Message = ErrorMessage.Unauthorized }) { StatusCode = (int)HttpStatusCode.Unauthorized };
                    return;
                }

                var user = context.HttpContext.User;
                //Validates token
                if (!user.Identity.IsAuthenticated)
                {
                    context.Result = new ObjectResult(new ErrorMessage() { Message = ErrorMessage.Unauthorized }) { StatusCode = (int)HttpStatusCode.Unauthorized };
                    return;
                }

                //Validates expiration time
                var hasExp = long.TryParse(user.FindFirst("exp").Value, out long expLongDate);
                var expDate = DateTimeOffset.FromUnixTimeSeconds(expLongDate).ToLocalTime();

                if (expDate < DateTime.Now)
                {
                    context.Result = new ObjectResult(new ErrorMessage() { Message = ErrorMessage.InvalidSession }) { StatusCode = (int)HttpStatusCode.Unauthorized };
                    return;
                }

                //Authorized
                return;
            }
            catch (Exception ex)
            {
                new ObjectResult(new ErrorMessage() { Message = ex.Message }) { StatusCode = (int)HttpStatusCode.InternalServerError };
                return;
            }
        }
    }
}