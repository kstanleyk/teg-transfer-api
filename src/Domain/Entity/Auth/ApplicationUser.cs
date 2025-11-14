using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Domain.Entity.Auth;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }

    // Additional properties for all users
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}