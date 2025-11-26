using Microsoft.AspNetCore.Identity;

namespace HelpDeskKyotera.Models
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        //property
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        //Audit Columns
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}