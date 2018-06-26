using System.ComponentModel.DataAnnotations;
using Eml.EntityBaseClasses;

namespace TenderSearch.Business.Common.Entities
{
    public class ContactPerson : EntityBaseSoftDeleteInt
    {
        [Required]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required]
        [Display(Name = "Position Title")]
        public int PositionTitleId { get; set; }

        [Required]
        [StringLength(256)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(256)]
        public string LastName { get; set; }

        [StringLength(50)]
        public string ContractType { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Contact Phone")]
        [StringLength(50)]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public string Department { get; set; }

        [Display(Name = "IsActive")]
        public bool IsActive { get; set; }

        public virtual PositionTitle PositionTitle { get; set; }

        public virtual Company Company { get; set; }
    }
}
