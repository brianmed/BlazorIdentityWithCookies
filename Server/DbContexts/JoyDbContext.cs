using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using IdentityWithCookies.Server.Entities;

namespace IdentityWithCookies.Server.DbContexts
{
    public class JoyDbContext : IdentityDbContext<AccountEntity>
    {
        public JoyDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
                .UseSqlite("DataSource=joy-42.sqlite");
        }
    }
}
