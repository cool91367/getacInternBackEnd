using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApplication.Models
{
    public class UserDbContext : IdentityDbContext<User>
    {
        public UserDbContext()
        {

        }

        public UserDbContext(DbContextOptions options)
            : base(options)
        {

        }
    }
}