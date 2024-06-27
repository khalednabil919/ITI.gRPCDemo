using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace ITI.gRPCDemo.Server
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //Basic username:password
            if (Request.Headers.ContainsKey("Authorization"))
            {
                var headerValue = Request.Headers["Authorization"].ToString().Split(" ");
                if (headerValue[0] != "Basic")
                    return Task.FromResult(AuthenticateResult.Fail("!UnAuthorized"));

                //username:password
                var token = headerValue[1];
                var bytes = Convert.FromBase64String(token);    
                var plainText = Encoding.UTF8.GetString(bytes);

                int index = plainText.IndexOf(':');
                var userName = plainText.Substring(0, index);
                int i = plainText.Length;
                var password = plainText.Substring(index+1,plainText.Length-index-1);

                if (userName.Equals("device") && password == "P@ssw0rd")
                {
                    var claimPrinciple = new ClaimsPrincipal(
                        new ClaimsIdentity(new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, userName),
                            new Claim(ClaimTypes.Role,"Device")
                        }));

                    return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimPrinciple, Scheme.Name)));

                }
            }
            return Task.FromResult(AuthenticateResult.Fail("!UnAuthorized"));
        }
    }
}
