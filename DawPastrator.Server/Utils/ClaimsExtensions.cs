using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DawPastrator.Server.Utils
{
    public static class ClaimsExtensions
    {
        public static bool TryGetUserId(this ClaimsPrincipal principal, out int userId)
        {
            var query =
                from claim in principal.Claims
                where claim.Type == ClaimTypes.UserData
                select claim.Value;

            var userIdStr = query.FirstOrDefault();


            if (userIdStr != null && int.TryParse(userIdStr, out var num))
            {
                userId = num;
                return true;
            }

            userId = 0;
            return false;
        }

    }
}
