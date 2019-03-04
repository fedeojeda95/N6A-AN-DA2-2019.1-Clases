using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Homeworks.BusinessLogic.Interface;

namespace Homeworks.WebApi.Filters {
    public class ProtectFilter : Attribute, IActionFilter
    {
        private readonly string role;

        public ProtectFilter(string role) 
        {
            this.role = role;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Obtenemos el token del header HTTP `authorization`
            string headerToken = context.HttpContext.Request.Headers["Authorization"];
            // Si el token es null, el usuario no se esta autenticado. Por eso cortamos
            // el pipeline. Si no envio un token, no es necesario seguir ejecutando el resto
            // de la aplicación.
            if (headerToken == null) {
                context.Result = new ContentResult()
                {
                    Content = "Token is required",
                };
            } else {
                try {
                    Guid token = Guid.Parse(headerToken);
                    VerifyToken(token, context);
                } catch (FormatException exception) {
                    context.Result = new ContentResult()
                    {
                        Content = "Invalid Token format",
                    };
                }
            }
        }

        private void VerifyToken(Guid token, ActionExecutingContext context)
        {
            // Usamos using asi nos aseguramos que se llame el Dispose de este `sessions` enseguida salgamos del bloque
            using (var sessions = GetSessionLogic(context)) {
                // Verificamos que el token sea valido
                if (!sessions.IsValidToken(token)) {
                    context.Result = new ContentResult()
                    {
                        Content = "Invalid Token",
                    };
                }
                // Verificamos que el rol del usuario sea correcto
                if (!sessions.HasLevel(token, role)) {
                    context.Result = new ContentResult()
                    {
                        Content = "The user isn't " + role,
                    };   
                }
            }
        }

        private ISessionsLogic GetSessionLogic(ActionExecutingContext context) {
            var typeOfSessionsLogic = typeof(ISessionsLogic);
            return context.HttpContext.RequestServices.GetService(typeOfSessionsLogic) as ISessionsLogic;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Vacio, ya que no queremos hacer nada despues de la request
        }
    }
}