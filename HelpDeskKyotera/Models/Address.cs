using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDeskKyotera.Models
{
    public class Address
    {
        public Guid Id { get; set; }
        // FK to ApplicationUser
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
        public string Department { get; set; } = null!;
        public string Locatioin { get; set; } = null!;
        public bool IsActive { get; set; }

        //Audit Columns
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}

