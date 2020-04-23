using Cw5.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Cw5.Loggining
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        readonly IStudentsDbService db;

        public BasicAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,IStudentsDbService service)
            : base(options, logger, encoder, clock){
            db = service;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Lack of authorization field");

            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credBytes = Convert.FromBase64String(authHeader.Parameter);
            var creds = Encoding.UTF8.GetString(credBytes).Split(":");

            if (creds.Length != 2)
                return AuthenticateResult.Fail("Incorrect authorization header value");


            if (db.Login(creds[0], creds[1]))
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier,"1"),
                    new Claim(ClaimTypes.Name, creds[0]),
                    new Claim(ClaimTypes.Role,"employee")
                };

                var ident = new ClaimsIdentity(claims, Scheme.Name);
                var princ = new ClaimsPrincipal(ident);
                var ticket = new AuthenticationTicket(princ, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            return AuthenticateResult.Fail("Wrong login or password");
        }
    }
}
