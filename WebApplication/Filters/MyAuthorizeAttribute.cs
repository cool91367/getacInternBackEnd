using System;
using Microsoft.AspNetCore.Authorization;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string User = "User";
}

public class MyAuthorizeAttribute : AuthorizeAttribute
{
    public MyAuthorizeAttribute(params string[] roles)
    {
        Roles = String.Join(",", roles);
    }
}