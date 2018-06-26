using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eml.EntityBaseClasses;

namespace TenderSearch.Business.Common.Entities
{
    public class Contract : EntityBaseSoftDeleteInt
    {
        [Required]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required]
        public string ContractType { get; set; }

        [Display(Name = "Date Signed")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateSigned { get; set; }

        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Renewal Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? RenewalDate { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        public virtual Company Company { get; set; }
    }
}
