using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DemoCRUD.API;

public class CustomAuthFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
    }
}
