using Microsoft.EntityFrameworkCore;

namespace mockup.Model
{
    public class UserDbContext:DbContext

    {
        public UserDbContext(DbContextOptions<UserDbContext> options):base(options)
        {

        } 

        public DbSet<User> Users { get; set; }
        
    }
}
