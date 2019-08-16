namespace WebApplication
{
    // Add roles to this enum and perform GET:/dev/seed to add the roles to the database
    public enum Role
    {
        Admin,
        User
    }

    public static class RoleExtensions
    {
        public static string GetRoleName(this Role role)
        {
            return role.ToString();
        }
    }
}