using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Implementations.Contracts;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.ApiClient.Exceptions;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Logging;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit.Contracts
{
    [TestClass, TestCategory("Unit")]
    public class ContractContentToBeSignedServiceTests
    {
        private const string TestBaseAddress = "http://test-api-endpoint";
        private const string TestFakeAccessToken = "AccessToken";

        private readonly ILoggerAdapter<ContractContentToBeSignedService> _mockLogger;
        private readonly MockHttpMessageHandler _mockHttp;

        private readonly IContractContentToBeSignedService _contractContentToBeSignedService;

        public ContractContentToBeSignedServiceTests()
        {
            _mockLogger = Mock.Of<ILoggerAdapter<ContractContentToBeSignedService>>(MockBehavior.Strict);
            _mockHttp = new MockHttpMessageHandler();

            _contractContentToBeSignedService = new ContractContentToBeSignedService(
                CreateAuthenticationService(),
                Options.Create(CreateContractsDataApiConfiguration()),
                _mockLogger,
                _mockHttp.ToHttpClient());
        }

        [TestMethod]
        public async Task Process_WhenContractDataApiCallFails_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new ContractContentToBeSignedMessage()
            {
                ContractId = 1
            };

            SetupServicesMock(true);

            // Act
            Func<Task> actual = async () => { await _contractContentToBeSignedService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<Exception>();
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task Process_WhenContractDataApiCallIsSuccessful_DoesNotThrowException()
        {
            // Arrange
            var serviceBusMessage = new ContractContentToBeSignedMessage()
            {
                ContractId = 1
            };

            SetupServicesMock();

            // Act
            await _contractContentToBeSignedService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        private static ContractsDataApiConfiguration CreateContractsDataApiConfiguration()
            => new ()
            {
                ApiBaseAddress = TestBaseAddress
            };

        private static IAuthenticationService<ContractsDataApiConfiguration> CreateAuthenticationService()
        {
            var mockAuthenticationService = new Mock<IAuthenticationService<ContractsDataApiConfiguration>>(MockBehavior.Strict);
            mockAuthenticationService
                .Setup(x => x.GetAccessTokenForAAD())
                .Returns(Task.FromResult(TestFakeAccessToken));
            return mockAuthenticationService.Object;
        }

        private void SetupServicesMock(bool isContractDataApiThrowException = false)
        {
            var config = CreateContractsDataApiConfiguration();

            if (isContractDataApiThrowException)
            {
                _mockHttp
                .Expect(HttpMethod.Patch, config.ApiBaseAddress + Constants.PrependSignedPageToDocumentEndpoint)
                .Throw(new ApiGeneralException());
            }
            else
            {
                _mockHttp
                .Expect(HttpMethod.Patch, config.ApiBaseAddress + Constants.PrependSignedPageToDocumentEndpoint)
                .Respond(HttpStatusCode.OK);
            }

            Mock.Get(_mockLogger)
                .Setup(logger => logger.LogInformation(It.IsAny<string>()));
        }
    }
}
