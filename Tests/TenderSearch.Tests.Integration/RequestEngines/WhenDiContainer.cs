﻿using Eml.Mediator.Contracts;
using Shouldly;
using TenderSearch.Business.Requests;
using TenderSearch.Business.Responses;
using TenderSearch.Tests.Integration.BaseClasses;
using Xunit;

namespace TenderSearch.Tests.Integration.RequestEngines
{
    public class WhenDiContainer : IntegrationTestDbBase
    {
        [Fact]
        public void DuplicateContractAsyncEngine_ShouldBeDiscoverable()
        {
            var sut = ClassFactory.GetExport<IRequestAsyncEngine<DuplicateContractAsyncRequest, DuplicateContractResponse>>();

            sut.ShouldNotBeNull();
        }
    }
}