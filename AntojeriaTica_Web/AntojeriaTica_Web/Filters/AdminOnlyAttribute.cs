using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace AntojeriaTica_Web.Filters
{
    // Simple session-based admin check. Adjust role value as needed.
    public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
    {
        private readonly HashSet<string> _roles;

        // Permitir pasar uno o más roles permitidos. Por defecto "Admin".
        public AdminOnlyAttribute(params string[] roles)
        {
            _roles = roles.Length > 0
                ? new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(new[] { "Admin" }, StringComparer.OrdinalIgnoreCase);
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var roleInSession = httpContext.Session.GetString("NombreRol");

            if (string.IsNullOrEmpty(roleInSession) || !_roles.Contains(roleInSession))
            {
                if (httpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.Result = new JsonResult(new { success = false, message = "Acceso denegado" }) { StatusCode = StatusCodes.Status403Forbidden };
                }
                else
                {
                    context.Result = new RedirectToActionResult("IniciarSesion", "Login", new { error = "Acceso restringido" });
                }
            }
        }
    }
}
