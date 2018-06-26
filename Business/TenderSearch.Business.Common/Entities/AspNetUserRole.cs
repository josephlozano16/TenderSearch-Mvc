using System.ComponentModel.DataAnnotations;
using Eml.EntityBaseClasses;

namespace TenderSearch.Business.Common.Entities
{
    public class AspNetUserRole : EntityBaseSoftDeleteInt
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(128)]
        public string Role { get; set; }

        [StringLength(128)]
        public string OldRole { get; set; }
    }
}