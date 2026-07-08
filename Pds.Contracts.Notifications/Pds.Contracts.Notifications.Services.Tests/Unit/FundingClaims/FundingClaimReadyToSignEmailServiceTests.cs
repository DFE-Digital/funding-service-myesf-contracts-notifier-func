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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit.FundingClaims
{
    [TestClass, TestCategory("Unit")]
    public class FundingClaimReadyToSignEmailServiceTests
    {
        private const string TestBaseAddress = "http://test-api-endpoint";
        private const string TestFakeAccessToken = "AccessToken";

        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<FundingClaimReadyToSignEmailService>> _mockLogger;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<IDfESignInPublicApi> _mockDfESignInPublicApi;
        private readonly Mock<IAuditService> _mockAuditService;

        private readonly IFundingClaimReadyToSignEmailService _fundingClaimReadyToSignEmailService;

        public FundingClaimReadyToSignEmailServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);
            _mockLogger = new Mock<ILoggerAdapter<FundingClaimReadyToSignEmailService>>(MockBehavior.Strict);
            _mockDfESignInPublicApi = new Mock<IDfESignInPublicApi>(MockBehavior.Strict);
            _mockAuditService = new Mock<IAuditService>(MockBehavior.Strict);
            _mockHttp = new MockHttpMessageHandler();

            _fundingClaimReadyToSignEmailService = new FundingClaimReadyToSignEmailService(
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
            var serviceBusMessage = new FundingClaimReadyToSignEmailMessage()
            {
                FundingClaimId = 1
            };

            SetupServicesMock(null, serviceBusMessage.FundingClaimId, false, true);

            // Act
            await Assert.ThrowsExceptionAsync<ApiGeneralException>(async () => await _fundingClaimReadyToSignEmailService.Process(serviceBusMessage));
            Func<Task> actual = async () => { await _fundingClaimReadyToSignEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<ApiGeneralException>();
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task Process_WithValidFundingClaimAndFailedDSIApiCall_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new FundingClaimReadyToSignEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = DateTime.UtcNow,
                }
            };

            SetupServicesMock(mockFundingClaim, serviceBusMessage.FundingClaimId);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(), UserRole.SignFundingClaims.ToString() }, false, true);

            // Act
            Func<Task> actual = async () => { await _fundingClaimReadyToSignEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<GetApiResultException>();
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
        }

        [TestMethod]
        public async Task Process_WithValidFundingClaimAndInvalidRoles_DoesNotSendFundingClaimReadyToSignEmail()
        {
            // Arrange
            var serviceBusMessage = new FundingClaimReadyToSignEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Id = 1,
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = DateTime.UtcNow,
                },
                Ukprn = 12345678
            };

            SetupServicesMock(mockFundingClaim, serviceBusMessage.FundingClaimId);

            SetupDfESignInPublicApiMock(new[] { UserRole.DocumentExchangeUser.ToString() }, true);

            var expectedError_Sign = $"Funding id [{mockFundingClaim.Id}], {nameof(FundingClaimReadyToSignEmailMessage)} processed and no users found with roles [SignFundingClaims] for organisation [{mockFundingClaim.Ukprn}]";
            var expectedInformation_View = $"Funding id [{mockFundingClaim.Id}], {nameof(FundingClaimReadyToSignEmailMessage)} processed and no users found with roles [ViewFundingClaimsAndReconciliationStatements] for organisation [{mockFundingClaim.Ukprn}]";

            // Act
            await _fundingClaimReadyToSignEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockLogger.Verify(logger => logger.LogError(expectedError_Sign), Times.Once);
            _mockLogger.Verify(logger => logger.LogInformation(expectedInformation_View), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == $"{nameof(FundingClaimReadyToSignEmailMessage)} processed and no users found with roles [SignFundingClaims] for organisation [{mockFundingClaim.Ukprn}]")), Times.Once);
        }

        [TestMethod]
        public async Task Process_WithValidFundingClaimAndBothUserRoles_SuccessfullySendsFundingClaimReadyToSignEmail()
        {
            // Arrange
            var serviceBusMessage = new FundingClaimReadyToSignEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = DateTime.UtcNow,
                }
            };

            var expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockFundingClaim.FundingClaimWindow.SignatureCloseDate, Constants.MessageType_FundingClaimReadyToSignEmail);

            var expectedAuditMessage = "FundingClaimReadyToSignEmailMessage processed and published to SharedEmailprocessorQueue.";

            SetupServicesMock(mockFundingClaim, serviceBusMessage.FundingClaimId);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(), UserRole.SignFundingClaims.ToString() });

            // Act
            await _fundingClaimReadyToSignEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [TestMethod]
        public async Task Process_WithValidFundingClaimAndViewFundingClaimsAndReconciliationStatementsRole_SuccessfullySendsFundingClaimReadyToSignViewOnlyEmail()
        {
            // Arrange
            var serviceBusMessage = new FundingClaimReadyToSignEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Id = 1,
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = DateTime.UtcNow,
                }
            };

            var expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockFundingClaim.FundingClaimWindow.SignatureCloseDate, Constants.MessageType_FundingClaimReadyToSignViewOnlyEmail);

            var expectedAuditMessage = "FundingClaimReadyToSignEmailMessage view only processed and published to SharedEmailprocessorQueue.";

            var expectedError_Sign = $"Funding id [{mockFundingClaim.Id}], {nameof(FundingClaimReadyToSignEmailMessage)} processed and no users found with roles [SignFundingClaims] for organisation [{mockFundingClaim.Ukprn}]";

            SetupServicesMock(mockFundingClaim, serviceBusMessage.FundingClaimId);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewFundingClaimsAndReconciliationStatements.ToString() });

            // Act
            await _fundingClaimReadyToSignEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockLogger.Verify(logger => logger.LogError(expectedError_Sign), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == $"{nameof(FundingClaimReadyToSignEmailMessage)} processed and no users found with roles [SignFundingClaims] for organisation [{mockFundingClaim.Ukprn}]")), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [TestMethod]
        public async Task Process_WithValidFundingClaimAndSignFundingClaimsRole_SuccessfullySendsFundingClaimReadyToSignEmail()
        {
            // Arrange
            var serviceBusMessage = new FundingClaimReadyToSignEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = DateTime.UtcNow,
                }
            };

            var expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockFundingClaim.FundingClaimWindow.SignatureCloseDate, Constants.MessageType_FundingClaimReadyToSignEmail);

            var expectedAuditMessage = "FundingClaimReadyToSignEmailMessage processed and published to SharedEmailprocessorQueue.";

            SetupServicesMock(mockFundingClaim, serviceBusMessage.FundingClaimId);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignFundingClaims.ToString() });

            // Act
            await _fundingClaimReadyToSignEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [TestMethod]
        public async Task Process_WithValidFundingClaimAndSignFundingClaimsAndViewFundingClaimsRole_SuccessfullySendsFundingClaimReadyToSignEmailAndFundingClaimReadyToSignEmail_ViewOnly()
        {
            // Arrange
            var serviceBusMessage = new FundingClaimReadyToSignEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = DateTime.UtcNow,
                }
            };

            var expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockFundingClaim.FundingClaimWindow.SignatureCloseDate, Constants.MessageType_FundingClaimReadyToSignEmail);
            var expectedMessage2 = GetNotificationMessageMock(mockFundingClaim.Title, mockFundingClaim.FundingClaimWindow.SignatureCloseDate, Constants.MessageType_FundingClaimReadyToSignViewOnlyEmail, true);

            var expectedAuditMessage = "FundingClaimReadyToSignEmailMessage processed and published to SharedEmailprocessorQueue.";
            var expectedAuditMessage2 = "FundingClaimReadyToSignEmailMessage view only processed and published to SharedEmailprocessorQueue.";


            SetupServicesMock(mockFundingClaim, serviceBusMessage.FundingClaimId);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignFundingClaims.ToString(), UserRole.ViewFundingClaimsAndReconciliationStatements.ToString() }, testTwoUsers: true);

            // Act
            await _fundingClaimReadyToSignEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage2)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage2)), Times.Once);
        }

        [TestMethod]
        public async Task Process_WithValidFundingClaimRequiredUserRolesAndNotificationServiceReturnsError_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new FundingClaimReadyToSignEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = DateTime.UtcNow,
                }
            };

            var expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockFundingClaim.FundingClaimWindow.SignatureCloseDate, Constants.MessageType_FundingClaimReadyToSignEmail);

            SetupServicesMock(mockFundingClaim, serviceBusMessage.FundingClaimId, true);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(), UserRole.SignFundingClaims.ToString() });

            // Act
            await Assert.ThrowsExceptionAsync<Exception>(async () => await _fundingClaimReadyToSignEmailService.Process(serviceBusMessage));

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        [TestMethod]
        public async Task Process_WithValidFundingClaimRequiredUserRolesAndNoDueDateOnDateAvalilabe_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new FundingClaimReadyToSignEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = null,
                }
            };

            SetupServicesMock(mockFundingClaim, serviceBusMessage.FundingClaimId, true);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(), UserRole.SignFundingClaims.ToString() });

            // Act
            Func<Task> actual = async () => { await _fundingClaimReadyToSignEmailService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<Exception>();
            result.Which.Message.Should().Be("Due Date is not avaialble.");
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
        }

        private static FundingClaimsDataApiConfiguration CreateFundingClaimsDataApiConfiguration()
            => new ()
            {
                ApiBaseAddress = TestBaseAddress
            };

        private static NotificationMessage GetNotificationMessageMock(string documentTitle, DateTime? dueDate, string messageType, bool testSecondUser = false)
        {
            return new NotificationMessage()
            {
                EmailMessageType = messageType,
                RequestingService = Constants.RequestingService_FundingClaims,
                EmailAddresses = new[] { !testSecondUser ? "testemail" : "testemail2" }.AsEnumerable(),
                EmailPersonalisation = new GovUkNotifyPersonalisation()
                {
                    Personalisation = new Dictionary<string, object>()
                    {
                        { "FundingClaimNotification", documentTitle },
                        { "DueDate", dueDate.HasValue ? dueDate.Value.DisplayFormat() : null }
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

        private void SetupDfESignInPublicApiMock(string[] roles, bool noUsersWithGivenRoles = false, bool isDfESignInPublicApiThrowsException = false, bool testTwoUsers = false)
        {
            var users = new List<UserContact>();

            if (!noUsersWithGivenRoles)
            {
                if (!testTwoUsers)
                {
                    users.Add(new UserContact()
                    {
                        Email = "testemail",
                        FirstName = "testFirstName",
                        LastName = "testLastName",
                        Roles = roles
                    });
                }
                else
                {
                    users.Add(new UserContact()
                    {
                        Email = "testemail",
                        FirstName = "testFirstName",
                        LastName = "testLastName",
                        Roles = new[] { roles[0] }
                    });
                    users.Add(new UserContact()
                    {
                        Email = "testemail2",
                        FirstName = "testFirstName2",
                        LastName = "testLastName2",
                        Roles = new[] { roles[1] }
                    });
                }
            }

            var mockUserContactsResponse = new UserContactLookupResponse
            {
                Users = users
            };

            if (!isDfESignInPublicApiThrowsException)
            {
                _mockDfESignInPublicApi.Setup(x => x.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()))
                .Returns(Task.FromResult(mockUserContactsResponse));
            }
            else
            {
                _mockDfESignInPublicApi.Setup(x => x.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()))
                .Throws(new GetApiResultException());
            }
        }

        private void SetupServicesMock(FundingClaim mockFundingClaim, int fundingClaimId, bool isNotificationEmailQueueServiceThrowException = false, bool isFundingClaimDataApiThrowException = false)
        {
            var response = new StringContent(JsonConvert.SerializeObject(mockFundingClaim), System.Text.Encoding.UTF8, "application/json");

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
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.FundingClaimByIdEndpoint + "/" + fundingClaimId)
                .Throw(new ApiGeneralException());
            }
            else
            {
                _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.FundingClaimByIdEndpoint + "/" + fundingClaimId)
                .Respond(HttpStatusCode.OK, response);
            }

            _mockLogger.Setup(logger => logger.LogInformation(It.IsAny<string>()));
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>()));
        }
    }
}
