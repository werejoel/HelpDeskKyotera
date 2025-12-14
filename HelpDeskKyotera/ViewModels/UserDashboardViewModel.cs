using HelpDeskKyotera.Models;

namespace HelpDeskKyotera.ViewModels
{
    public class UserDashboardViewModel
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Department Department { get; set; }
        public List<string> UserRoles { get; set; } = new List<string>();
        public int TotalRequestedTickets { get; set; }
        public int TotalAssignedTickets { get; set; }
        public List<Ticket> RecentRequestedTickets { get; set; } = new List<Ticket>();
        public List<Ticket> RecentAssignedTickets { get; set; } = new List<Ticket>();
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
    }

    public class UserProfileViewModel
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string JobTitle { get; set; }
        public string EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastLogin { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
