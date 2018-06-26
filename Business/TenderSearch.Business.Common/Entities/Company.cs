using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Eml.EntityBaseClasses;

namespace TenderSearch.Business.Common.Entities
{
    public class Company : EntityBaseSoftDeleteInt
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [DataType(DataType.Url)]
        public string Website { get; set; }

        [Required]
        public string AbnCan { get; set; }

        public virtual IList<Contract> Contracts { get; set; }

        public virtual IList<ContactPerson> ContactPersons { get; set; }
    }
}
