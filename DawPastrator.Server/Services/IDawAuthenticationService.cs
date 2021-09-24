using DawPastrator.Server.Models;
using DawPastrator.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DawPastrator.Server.Services
{
    public interface IDawAuthenticationService
    {
        public Task<ClaimsPrincipal?> VerifyAsync(UserLoginModel model);
    }

    public class DefaultDawAuthenticationService : IDawAuthenticationService
    {
 
        private readonly IDatabaseServices databaseServices;

        public DefaultDawAuthenticationService(IDatabaseServices databaseServices)
        {
            this.databaseServices = databaseServices;
        }

        public async Task<ClaimsPrincipal?> VerifyAsync(UserLoginModel model)
        {
            var userId = databaseServices.GetUserIdAsync(model.UserName);

            if (await databaseServices.VerifyMasterPasswordAsync(model.UserName, model.Password))
            {
                var claims = new Claim[]
                {
                    new (ClaimTypes.Name, model.UserName),
                    new (ClaimTypes.UserData, userId.ToString())
                };

                var identity = new ClaimsIdentity(claims, StringConstant.Cookies);

                return new ClaimsPrincipal(identity);
            }
            else
            {
                return null;
            }
        }
    }
}
