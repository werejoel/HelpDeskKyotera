using System.ComponentModel.DataAnnotations;

namespace HelpDeskKyotera.ViewModels.Roles
{
    public class RoleListFilterViewModel
    {
        public string? Search { get; set; }      // name/description
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
    }
}