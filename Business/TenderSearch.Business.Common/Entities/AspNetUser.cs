using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eml.EntityBaseClasses;

namespace TenderSearch.Business.Common.Entities
{
    public class AspNetUser : EntityBaseSoftDeleteInt
    {
        [Key]
        [StringLength(256)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [NotMapped]
        public bool HasRole => Roles.Count > 0;

        public List<string> Roles { get; set; } = new List<string>();
    }
}