using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization.API.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //JUST to make sure the tests will work with Authentication or Authorization header
            //In documentation was saying to use Authentication, but the correct one is Authorization
            if (context.Request.Headers.Keys.Contains("Authentication") && !context.Request.Headers.Keys.Contains("Authorization"))
            {
                context.Request.Headers.Add("Authorization", (context.Request.Headers["Authentication"]));
            }

            await _next.Invoke(context);
        }

    }

    public static class UserKeyValidatorsExtension
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<AuthenticationMiddleware>();
            return app;
        }
    }

}
