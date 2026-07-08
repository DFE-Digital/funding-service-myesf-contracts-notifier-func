using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Implementations;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Core.ApiClient.Exceptions;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Azure.Interfaces.Messaging;
using Pds.Core.Logging;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractNotificationServiceTests
    {
        #region Fields

        private const string TestBaseAddress = "http://test-api-endpoint";
        private const string TestFakeAccessToken = "AccessToken";
        private const string TestServiceBusExceptionMessage = "Test Exception Error";

        private readonly MockHttpMessageHandler _mockHttp
            = new MockHttpMessageHandler();

        private readonly ILoggerAdapter<ContractNotificationService> _contractsLogger
            = Mock.Of<ILoggerAdapter<ContractNotificationService>>(MockBehavior.Strict);

        private readonly IAuditService _auditService
            = Mock.Of<IAuditService>(MockBehavior.Strict);

        private readonly IAzureMessagingServiceBusService _mockAzureMessagingServiceBusService
            = Mock.Of<IAzureMessagingServiceBusService>(MockBehavior.Strict);

        private readonly Contract _contract
            = new Contract() { ContractNumber = "123", ContractVersion = 234, Id = 345, Ukprn = 456 };

        #endregion Fields


        #region GetContracts

        [TestMethod]
        public async Task GetContracts_Returns_ExpectedDataAsync()
        {
            // Arrange
            var queryParams = CreateQueryParameters(2);
            var config = CreateContractsDataApiConfiguration(queryParams);

            var contracts = CreateContractReminders();
            var stringContent = new StringContent(JsonConvert.SerializeObject(contracts), System.Text.Encoding.UTF8, "application/json");

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.ContractReminderEndpoint)
                .WithQueryString(queryParams)
                .Respond(HttpStatusCode.OK, stringContent);

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            var result = await service.GetOverdueContracts();

            // Assert
            result.Should().BeEquivalentTo(contracts);
            VerifyAll();
        }

        [TestMethod]
        public async Task GetContracts_OnFailure_ThrowsException()
        {
            // Arrange
            var expectedStatusCode = HttpStatusCode.InternalServerError;
            var queryParams = CreateQueryParameters(2);
            var config = CreateContractsDataApiConfiguration(queryParams);

            var stringContent = new StringContent(string.Empty);

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.ContractReminderEndpoint)
                .WithQueryString(queryParams)
                .Respond(expectedStatusCode, stringContent);

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogError(It.IsAny<ApiGeneralException>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.GetOverdueContracts();

            // Assert
            await act.Should().ThrowAsync<ApiGeneralException>()
                .Where(p => p.ResponseStatusCode == expectedStatusCode);
            _mockHttp.VerifyNoOutstandingExpectation();
            Mock.Get(_contractsLogger).VerifyAll();
        }

        [TestMethod]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        public async Task GetContracts_Verify_QueryString_IncludesAllConfiguredValues(int numOfParameters)
        {
            // Arrange
            var queryParams = CreateQueryParameters(numOfParameters);
            var config = CreateContractsDataApiConfiguration(queryParams);

            var stringContent = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.ContractReminderEndpoint)
                .WithQueryString(queryParams)
                .Respond(HttpStatusCode.OK, stringContent);

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.GetOverdueContracts();

            // Assert
            await act.Should().NotThrowAsync();
            _mockHttp.VerifyNoOutstandingExpectation();
            Mock.Get(_contractsLogger).VerifyAll();
        }

        #endregion GetContracts


        #region Queue Reminder Email Tests

        [TestMethod]
        public async Task QueueContractEmailReminderMessage_DoesNotThrowExceptions()
        {
            // Arrange
            ContractReminderMessage actualReminder = null;
            string actualQueueName = null;
            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_mockAzureMessagingServiceBusService)
                .Setup(p => p.SendMessageAsync(It.IsAny<ContractReminderMessage>(), It.IsAny<string>(), null))
                .Returns(Task.CompletedTask)
                .Callback((ContractReminderMessage reminder, string queueName, string sessionId) =>
                {
                    actualReminder = reminder;
                    actualQueueName = queueName;
                })
                .Verifiable();

            Mock.Get(_auditService)
                .Setup(p => p.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.QueueContractEmailReminderMessage(_contract);

            // Assert
            await act.Should().NotThrowAsync();
            actualReminder.ContractId.Should().Be(_contract.Id);
            VerifyAll();
        }

        [TestMethod]
        public async Task QueueContractEmailReminderMessage_FailureToQueue_AllowsExceptionToBubble()
        {
            // Arrange
            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_mockAzureMessagingServiceBusService)
                .Setup(p => p.SendMessageAsync(It.IsAny<ContractReminderMessage>(), It.IsAny<string>(), null))
                .Throws(new ServiceBusException(TestServiceBusExceptionMessage, ServiceBusFailureReason.ServiceTimeout))
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.QueueContractEmailReminderMessage(_contract);

            // Assert
            await act.Should().ThrowAsync<ServiceBusException>().Where(ex => ex.Message.Contains(TestServiceBusExceptionMessage));

            VerifyAll();
        }

        [TestMethod]
        public async Task QueueContractEmailReminderMessage_FailureToLogAudit_AllowsExceptionToBubble()
        {
            // Arrange
            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_mockAzureMessagingServiceBusService)
                .Setup(p => p.SendMessageAsync(It.IsAny<ContractReminderMessage>(), It.IsAny<string>(), null))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock.Get(_auditService)
                .Setup(p => p.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Throws<InvalidOperationException>()
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.QueueContractEmailReminderMessage(_contract);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();

            VerifyAll();
        }

        [TestMethod]
        public async Task QueueContractEmailReminderMessage_VerifyAuditEntry()
        {
            // Arrange
            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_mockAzureMessagingServiceBusService)
                .Setup(p => p.SendMessageAsync(It.IsAny<ContractReminderMessage>(), It.IsAny<string>(), null))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock.Get(_auditService)
                .Setup(p => p.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.QueueContractEmailReminderMessage(_contract);

            // Assert
            await act.Should().NotThrowAsync();
            VerifyAll();
        }

        #endregion Queue Reminder Email Tests


        #region NotifyContractReminderSent Tests

        [TestMethod]
        public async Task NotifyContractReminderSent_DoesNotThrowExceptions()
        {
            // Arrange
            string updateContent = GetUpdateRequestContents();

            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            _mockHttp
                .Expect(HttpMethod.Patch, TestBaseAddress + Constants.ContractReminderPatchEndpoint)
                .WithContent(updateContent)
                .Respond(HttpStatusCode.OK);

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_auditService)
                .Setup(p => p.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.NotifyContractReminderSent(_contract);

            // Assert
            await act.Should().NotThrowAsync();
            VerifyAll();
        }

        [TestMethod]
        public async Task NotifyContractReminderSent_OnContractApiFailure_ExceptionIsRaised()
        {
            // Arrange
            string updateContent = GetUpdateRequestContents();

            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogError(It.IsAny<ApiGeneralException>(), It.IsAny<string>(), It.IsAny<object[]>()));

            _mockHttp
                .Expect(HttpMethod.Patch, TestBaseAddress + Constants.ContractReminderPatchEndpoint)
                .WithContent(updateContent)
                .Respond(HttpStatusCode.InternalServerError, new StringContent(string.Empty));

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.NotifyContractReminderSent(_contract);

            // Assert
            await act.Should().ThrowAsync<ApiGeneralException>();
            VerifyAll();
        }

        [TestMethod]
        public async Task NotifyContractReminderSent_OnAuditApiFailure_ExceptionIsRaised()
        {
            // Arrange
            string updateContent = GetUpdateRequestContents();

            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            _mockHttp
                .Expect(HttpMethod.Patch, TestBaseAddress + Constants.ContractReminderPatchEndpoint)
                .WithContent(updateContent)
                .Respond(HttpStatusCode.OK);

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_auditService)
                .Setup(p => p.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Throws<InvalidOperationException>()
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.NotifyContractReminderSent(_contract);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            VerifyAll();
        }

        #endregion NotifyContractReminderSent Tests


        #region Setup Helpers

        private ContractNotificationService CreateContractNotificationService(ContractsDataApiConfiguration contractsConfig)
        {
            var authService = CreateAuthenticationService();
            var httpClient = _mockHttp.ToHttpClient();

            var rtn = new ContractNotificationService(
                authService,
                httpClient,
                Options.Create(contractsConfig),
                _mockAzureMessagingServiceBusService,
                _contractsLogger,
                _auditService);

            return rtn;
        }

        private ContractNotificationService CreateMockContractNotifcationService(ContractsDataApiConfiguration contractsConfig)
        {
            var authService = CreateAuthenticationService();
            var httpClient = _mockHttp.ToHttpClient();

            var rtn = new Mock<ContractNotificationService>(
                MockBehavior.Strict,
                authService,
                httpClient,
                Options.Create(contractsConfig),
                _mockAzureMessagingServiceBusService,
                _contractsLogger,
                _auditService);

            return rtn.Object;
        }

        private ContractReminders CreateContractReminders()
        {
            ContractReminders rtn = new ContractReminders()
            {
                Contracts = new List<Contract>()
                {
                    new Contract()
                    {
                        ContractNumber = "One123",
                        ContractVersion = 234,
                        Id = 345,
                        Ukprn = 456
                    },
                    new Contract()
                    {
                        ContractNumber = "Two123",
                        ContractVersion = 234,
                        Id = 345,
                        Ukprn = 456
                    }
                }
            };

            return rtn;
        }

        private IDictionary<string, string> CreateQueryParameters(int numOfParameters)
        {
            IDictionary<string, string> rtn = new Dictionary<string, string>();

            for (int i = 0; i < numOfParameters; i++)
            {
                rtn.Add("keytest" + i, "valuetest" + i);
            }

            return rtn;
        }

        private string GetUpdateRequestContents()
        {
            var update = new ContractUpdateRequest()
            {
                Id = _contract.Id,
                ContractNumber = _contract.ContractNumber,
                ContractVersion = _contract.ContractVersion
            };

            return JsonConvert.SerializeObject(update);
        }

        private IAuthenticationService<ContractsDataApiConfiguration> CreateAuthenticationService()
        {
            var mockAuthenticationService = new Mock<IAuthenticationService<ContractsDataApiConfiguration>>(MockBehavior.Strict);
            mockAuthenticationService
                .Setup(x => x.GetAccessTokenForAAD())
                .Returns(Task.FromResult(TestFakeAccessToken));
            return mockAuthenticationService.Object;
        }

        private ContractsDataApiConfiguration CreateContractsDataApiConfiguration(IDictionary<string, string> queryParameters)
            => new ContractsDataApiConfiguration()
            {
                ApiBaseAddress = TestBaseAddress,
                ContractReminderQuerystring = new QuerystringEndpointConfiguration()
                {
                    QueryParameters = queryParameters
                }
            };

        #endregion Setup Helpers


        #region Verify Helpers

        private void VerifyAll()
        {
            _mockHttp.VerifyNoOutstandingExpectation();
            Mock.Get(_contractsLogger).VerifyAll();
            Mock.Get(_auditService).VerifyAll();
            Mock.Get(_mockAzureMessagingServiceBusService).VerifyAll();
        }

        #endregion Verify Helpers
    }
}