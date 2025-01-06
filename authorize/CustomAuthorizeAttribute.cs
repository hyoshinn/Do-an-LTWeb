
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace QuanLyShopDoGiaDung.authorize
{
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public string[] AllowedRole { get; set; }
        public CustomAuthorizeAttribute(params string[] roles)
        {
            AllowedRole = roles;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            ClaimsPrincipal claimUser = context.HttpContext.User;
            if (!claimUser.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Access", null);
            }

            // var role = context.HttpContext.Session.GetString("role");
            // if (!AllowedRole.Contains(role))
            // {
            //     context.Result = new RedirectToActionResult("Index", "AuthorizationError", null);
            // }
        }
    }
}