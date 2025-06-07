using Investly.DAL.Entities;
using Investly.PL.General;
using System.Security.Claims;

namespace Investly.PL.Extentions
{
    public static class ClaimsPrincipleExtention
    {
        public static int? GetUserId(this ClaimsPrincipal  user)
        {
            var idClaim = user.FindFirst("id");
            if (idClaim == null || !int.TryParse(idClaim.Value, out int userId))
            {
                return null;
            }

            return userId;
        }
        public static int GetUserType(this ClaimsPrincipal user)
        {
            var userTypeClaim = user.FindFirst("userType");

            if (userTypeClaim==null||!int.TryParse(userTypeClaim.Value, out int userTypeId))
            {
                return 0;
             
            }

            return userTypeId;

        }
        //public static string GetUserName(this ClaimsPrincipal claimsPrincipal)
        //{
        //    return claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty;
        //}
    }
}
