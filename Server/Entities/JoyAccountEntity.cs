using Microsoft.AspNetCore.Identity;

namespace IdentityWithCookies.Server.Entities
{
    public class AccountEntity : IdentityUser
    {
        public long AccountEntityId { get; set; }
    }
}

