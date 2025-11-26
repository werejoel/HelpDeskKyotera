using Microsoft.AspNetCore.Identity;
using System.Net;

namespace HelpDeskKyotera.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        //properties
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }

        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }

        //Audit Columns
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }

        // Navigation property for one-to-many relationsip
        public virtual List<Address>? Addresses { get; set; }
    }
}