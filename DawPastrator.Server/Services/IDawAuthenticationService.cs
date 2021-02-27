using DawPastrator.Server.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DawPastrator.Server.Services
{
    public interface IDawAuthenticationService
    {
        public bool Verify(UserLoginModel model, out ClaimsPrincipal principal);
    }

    public class DefaultDawAuthenticationService : IDawAuthenticationService
    {
        public bool Verify(UserLoginModel model, out ClaimsPrincipal principal)
        {
            var claims = new Claim[]
            {
                new (ClaimTypes.Name, model.UserName),
                new (ClaimTypes.UserData, 0.ToString())
            };

            var identity = new ClaimsIdentity(claims, StringConstant.Cookies);
            principal = new ClaimsPrincipal(identity);

            return true;
        }
    }
}
