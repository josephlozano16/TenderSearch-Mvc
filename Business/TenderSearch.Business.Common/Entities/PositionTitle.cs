using System.ComponentModel.DataAnnotations;
using Eml.EntityBaseClasses;

namespace TenderSearch.Business.Common.Entities
{
    public class PositionTitle : EntityBaseSoftDeleteInt
    {
        [Required]
        [StringLength(256)]
        public string Title { get; set; }

    }
}
