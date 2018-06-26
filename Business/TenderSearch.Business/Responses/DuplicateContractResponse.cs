using System.Collections.Generic;
using System.Linq;
using Eml.Mediator.Contracts;
using TenderSearch.Business.Common.Entities;

namespace TenderSearch.Business.Responses
{
    public class DuplicateContractResponse : IResponse
    {
        public List<Contract> Contracts { get; private set; }

        public DuplicateContractResponse(IEnumerable<Contract> contracts)
        {
            Contracts = contracts.ToList();
        }
    }
}
