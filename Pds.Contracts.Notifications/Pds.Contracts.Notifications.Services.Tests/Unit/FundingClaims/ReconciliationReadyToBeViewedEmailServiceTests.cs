using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Extensions;
using Pds.Contracts.Notifications.Services.Implementations.FundingClaims;
using Pds.Contracts.Notifications.Services.Interfaces.FundingClaims;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Tests.Extensions;
using Pds.Core.ApiClient.Exceptions;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Common.Identity.Enums;
using Pds.Core.DfESignIn.Exceptions;
using Pds.Core.DfESignIn.Interfaces;
using Pds.Core.DfESignIn.Models;
using Pds.Core.Logging;
using Pds.Core.Notification.Interfaces;
using Pds.Core.Notification.Models;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit.FundingClaims
{
    [TestClass, TestCategory("Unit")]
    public class ReconciliationReadyToBeViewedEmailServiceTests
    {
        private const string TestBaseAddress = "http://test-api-endpoint";
        private const string TestFakeAccessToken = "AccessToken";

        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<ReconciliationReadyToBeViewedEmailService>> _mockLogger;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<IDfESignInPublicApi> _mockDfESignInPublicApi;
        private readonly Mock<IAuditService> _mockAuditService;

        private readonly IReconciliationReadyToBeViewedEmailService _reconciliationReadyToBeViewedEmailService;

        public ReconciliationReadyToBeViewedEmailServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);
            _mockLogger = new Mock<ILoggerAdapter<ReconciliationReadyToBeViewedEmailService>>(MockBehavior.Strict);
            _mockDfESignInPublicApi = new Mock<IDfESignInPublicApi>(MockBehavior.Strict);
            _mockAuditService = new Mock<IAuditService>(MockBehavior.Strict);
            _mockHttp = new MockHttpMessageHandler();

            _reconciliationReadyToBeViewedEmailService = new ReconciliationReadyToBeViewedEmailService(
                CreateAuthenticationService(),
                Options.Create(CreateFundingClaimsDataApiConfiguration()),
                _mockNotificationEmailQueueService.Object,
                _mockLogger.Object,
                _mockDfESignInPublicApi.Object,
                _mockAuditService.Object,
                _mockHttp.ToHttpClient());
        }

        [TestMethod]
        public async Task Process_WhenFundingClaimDataApiUnreachable_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new ReconciliationReadyToBeViewedEmailMessage()
            {
                ReconciliationId = 1
            };

            SetupServicesMock(null, serviceBusMessage.ReconciliationId, false, true);

            // Act
            Func<Task> actual = async () => { await _reconciliationReadyToBeViewedEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<ApiGeneralException>();
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task Process_WithNoMatchedReconciliation_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new ReconciliationReadyToBeViewedEmailMessage()
            {
                ReconciliationId = 1
            };

            SetupServicesMock(null, serviceBusMessage.ReconciliationId);

            // Act
            Func<Task> actual = async () => { await _reconciliationReadyToBeViewedEmailService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<Exception>();
            result.Which.Message.Should().Be("Error fetching reconciliation.");
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task Process_WithValidReconciliationAndFailedDSIApiCall_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new ReconciliationReadyToBeViewedEmailMessage()
            {
                ReconciliationId = 1
            };

            var mockFundingClaim = new Reconciliation
            {
                Title = "mockTitle",
                Type = ReconciliationType.FINAL
            };

            SetupServicesMock(mockFundingClaim, serviceBusMessage.ReconciliationId);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(), UserRole.SignFundingClaims.ToString() }, false, true);

            // Act
            Func<Task> actual = async () => { await _reconciliationReadyToBeViewedEmailService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<GetApiResultException>();
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ReconciliationType.FINAL)]
        [DataRow(ReconciliationType.YEAREND)]
        public async Task Process_WithValidReconciliationAndInvalidRoles_DoesNotSendReconciliationReadyToBeViewedEmail(ReconciliationType reconciliationType)
        {
            // Arrange
            var serviceBusMessage = new ReconciliationReadyToBeViewedEmailMessage()
            {
                ReconciliationId = 1
            };

            var mockFundingClaim = new Reconciliation
            {
                Id = 1,
                Title = "mockTitle",
                Type = reconciliationType,
                Ukprn = 12345678
            };

            var expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockFundingClaim.Type, Constants.MessageType_ReconciliationReadyToBeViewedEmail);

            SetupServicesMock(mockFundingClaim, serviceBusMessage.ReconciliationId);

            SetupDfESignInPublicApiMock(new[] { UserRole.DocumentExchangeUser.ToString() }, true);

            var expectedError = $"Reconciliation id [{mockFundingClaim.Id}], {nameof(ReconciliationReadyToBeViewedEmailMessage)} processed and no users found with roles [ViewFundingClaimsAndReconciliationStatements, SignFundingClaims] for organisation [{mockFundingClaim.Ukprn}].";

            // Act
            await _reconciliationReadyToBeViewedEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockLogger.Verify(logger => logger.LogError(expectedError), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == $"{nameof(ReconciliationReadyToBeViewedEmailMessage)} processed and no users found with roles [ViewFundingClaimsAndReconciliationStatements, SignFundingClaims] for organisation [{mockFundingClaim.Ukprn}].")), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ReconciliationType.FINAL)]
        [DataRow(ReconciliationType.YEAREND)]
        public async Task Process_WithValidReconciliationAndRequiredUserRoles_SuccessfullySendsReconciliationReadyToBeViewedEmail(ReconciliationType reconciliationType)
        {
            // Arrange
            var serviceBusMessage = new ReconciliationReadyToBeViewedEmailMessage()
            {
                ReconciliationId = 1
            };

            var mockFundingClaim = new Reconciliation
            {
                Title = "mockTitle",
                Type = reconciliationType
            };

            var expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockFundingClaim.Type, Constants.MessageType_ReconciliationReadyToBeViewedEmail);

            var expectedAuditMessage = "ReconciliationReadyToBeViewedEmailMessage processed and published to SharedEmailprocessorQueue.";

            SetupServicesMock(mockFundingClaim, serviceBusMessage.ReconciliationId);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(), UserRole.SignFundingClaims.ToString() });

            // Act
            await _reconciliationReadyToBeViewedEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ReconciliationType.FINAL)]
        [DataRow(ReconciliationType.YEAREND)]
        public async Task Process_WithValidReconciliationAndRequiredUserRolesWhenNotificationServiceReturnsError_ThrowsException(ReconciliationType reconciliationType)
        {
            // Arrange
            var serviceBusMessage = new ReconciliationReadyToBeViewedEmailMessage()
            {
                ReconciliationId = 1
            };

            var mockFundingClaim = new Reconciliation
            {
                Title = "mockTitle",
                Type = reconciliationType
            };

            var expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockFundingClaim.Type, Constants.MessageType_ReconciliationReadyToBeViewedEmail);

            SetupServicesMock(mockFundingClaim, serviceBusMessage.ReconciliationId, true);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(), UserRole.SignFundingClaims.ToString() });

            // Act
            Func<Task> actual = async () => { await _reconciliationReadyToBeViewedEmailService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<Exception>();
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        private static FundingClaimsDataApiConfiguration CreateFundingClaimsDataApiConfiguration()
        => new ()
        {
            ApiBaseAddress = TestBaseAddress
        };

        private static NotificationMessage GetNotificationMessageMock(string documentTitle, ReconciliationType reconciliationType, string messageType)
        {
            return new NotificationMessage()
            {
                EmailMessageType = messageType,
                RequestingService = Constants.RequestingService_FundingClaims,
                EmailAddresses = new[] { "testemail" }.AsEnumerable(),
                EmailPersonalisation = new GovUkNotifyPersonalisation()
                {
                    Personalisation = new Dictionary<string, object>()
                    {
                        { "ReconciliationTitle",  documentTitle },
                        { "ReconciliationType", reconciliationType.GetPropertyValue<ReconciliationType, DisplayAttribute>(o => o.Name) }
                    }
                }
            };
        }

        private static IAuthenticationService<FundingClaimsDataApiConfiguration> CreateAuthenticationService()
        {
            var mockAuthenticationService = new Mock<IAuthenticationService<FundingClaimsDataApiConfiguration>>(MockBehavior.Strict);
            mockAuthenticationService
                .Setup(x => x.GetAccessTokenForAAD())
                .Returns(Task.FromResult(TestFakeAccessToken));
            return mockAuthenticationService.Object;
        }

        private void SetupDfESignInPublicApiMock(string[] roles, bool noUsersWithGivenRoles = false, bool isDfESignInPublicApiThrowsException = false)
        {
            var users = new List<UserContact>();

            if (!noUsersWithGivenRoles)
            {
                users.Add(new UserContact()
                {
                    Email = "testemail",
                    FirstName = "testFirstName",
                    LastName = "testLastName",
                    Roles = roles
                });
            }

            var mockUserContactsResponse = new UserContactLookupResponse
            {
                Users = users
            };

            if (!isDfESignInPublicApiThrowsException)
            {
                _mockDfESignInPublicApi
                    .Setup(x => x.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()))
                    .Returns(Task.FromResult(mockUserContactsResponse));
            }
            else
            {
                _mockDfESignInPublicApi
                    .Setup(x => x.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()))
                    .Throws(new GetApiResultException());
            }
        }

        private void SetupServicesMock(Reconciliation mockReconciliation, int reconciliationClaimId, bool isNotificationEmailQueueServiceThrowException = false, bool isFundingClaimDataApiThrowException = false)
        {
            var response = new StringContent(JsonConvert.SerializeObject(mockReconciliation), System.Text.Encoding.UTF8, "application/json");

            var config = CreateFundingClaimsDataApiConfiguration();

            if (isNotificationEmailQueueServiceThrowException)
            {
                _mockNotificationEmailQueueService
                    .Setup(service => service.SendAsync(It.IsAny<NotificationMessage>()))
                    .ThrowsAsync(new Exception());
            }
            else
            {
                _mockNotificationEmailQueueService
                    .Setup(service => service.SendAsync(It.IsAny<NotificationMessage>()))
                    .Returns(Task.CompletedTask);
            }

            _mockAuditService
                .Setup(service => service.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask);

            if (isFundingClaimDataApiThrowException)
            {
                _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.ReconciliationGetByIdEndpoint + "/" + reconciliationClaimId)
                .Throw(new ApiGeneralException());
            }
            else
            {
                _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.ReconciliationGetByIdEndpoint + "/" + reconciliationClaimId)
                .Respond(HttpStatusCode.OK, response);
            }

            _mockLogger.Setup(logger => logger.LogInformation(It.IsAny<string>()));
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>()));
        }
    }
}
