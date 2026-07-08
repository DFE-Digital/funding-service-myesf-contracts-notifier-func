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
    public class FundingClaimWithdrawnEmailServiceTests
    {
        private const string TestBaseAddress = "http://test-api-endpoint";
        private const string TestFakeAccessToken = "AccessToken";

        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<FundingClaimWithdrawnEmailService>> _mockLogger;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<IDfESignInPublicApi> _mockDfESignInPublicApi;
        private readonly Mock<IAuditService> _mockAuditService;

        private readonly IFundingClaimWithdrawnEmailService _fundingClaimWithdrawnEmailService;

        public FundingClaimWithdrawnEmailServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);
            _mockLogger = new Mock<ILoggerAdapter<FundingClaimWithdrawnEmailService>>(MockBehavior.Strict);
            _mockDfESignInPublicApi = new Mock<IDfESignInPublicApi>(MockBehavior.Strict);
            _mockAuditService = new Mock<IAuditService>(MockBehavior.Strict);
            _mockHttp = new MockHttpMessageHandler();

            _fundingClaimWithdrawnEmailService = new FundingClaimWithdrawnEmailService(
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
            var serviceBusMessage = new FundingClaimWithdrawnEmailMessage()
            {
                FundingClaimId = 1
            };

            SetupServicesMock(null, null, serviceBusMessage.FundingClaimId, false, true);

            // Act
            Func<Task> actual = async () => { await _fundingClaimWithdrawnEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<ApiGeneralException>();
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task Process_WithValidFundingClaimAndFailedDSIApiCall_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new FundingClaimWithdrawnEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user"
            };

            SetupServicesMock(mockFundingClaim, null, serviceBusMessage.FundingClaimId);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(), UserRole.SignFundingClaims.ToString() }, false, true);

            // Act
            Func<Task> actual = async () => { await _fundingClaimWithdrawnEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<GetApiResultException>();
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
        }

        [TestMethod]
        public async Task Process_WithValidFundingClaimAndInvalidRoles_DoesNotSendFundingClaimWithdrawnEmail()
        {
            // Arrange
            var serviceBusMessage = new FundingClaimWithdrawnEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user"
            };

            SetupServicesMock(mockFundingClaim, null, serviceBusMessage.FundingClaimId);

            SetupDfESignInPublicApiMock(new[] { UserRole.DocumentExchangeUser.ToString() }, true);

            var expectedError = $"Funding id [{mockFundingClaim.Id}], {nameof(FundingClaimWithdrawnEmailMessage)} processed and no users found with roles [ViewFundingClaimsAndReconciliationStatements, SignFundingClaims] for organisation [{mockFundingClaim.Ukprn}]";

            // Act
            await _fundingClaimWithdrawnEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockLogger.Verify(logger => logger.LogError(expectedError), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == $"{nameof(FundingClaimWithdrawnEmailMessage)} processed and no users found with roles [ViewFundingClaimsAndReconciliationStatements, SignFundingClaims] for organisation [{mockFundingClaim.Ukprn}]")), Times.Once);
        }

        [DataTestMethod]
        [DataRow(true, Constants.MessageType_FundingClaimWithdrawnPreviousVersionSignedEmail)]
        [DataRow(false, Constants.MessageType_FundingClaimWithdrawnNotSignedEmail)]
        public async Task Process_WithValidFundingClaimAndRequiredUserRoles_SuccessfullySendsFundingClaimWithdrawnEmail(bool isPreviousVerionAvailable, string messageType)
        {
            // Arrange
            var serviceBusMessage = new FundingClaimWithdrawnEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Id = serviceBusMessage.FundingClaimId,
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = DateTime.UtcNow,
                }
            };

            var mockPreviousFundingClaim = new FundingClaim
            {
                Id = serviceBusMessage.FundingClaimId,
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user1",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = DateTime.UtcNow,
                }
            };

            NotificationMessage expectedMessage;
            if (isPreviousVerionAvailable)
            {
                expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockPreviousFundingClaim.SignedOn, mockPreviousFundingClaim.SignedByDisplayName, mockFundingClaim.FundingClaimWindow.SignatureCloseDate, isPreviousVerionAvailable, messageType);
            }
            else
            {
                expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockFundingClaim.SignedOn, mockFundingClaim.SignedByDisplayName, mockFundingClaim.FundingClaimWindow.SignatureCloseDate, isPreviousVerionAvailable, messageType);
            }

            var expectedAuditMessage = "FundingClaimWithdrawnEmailMessage processed and published to SharedEmailprocessorQueue.";

            SetupServicesMock(mockFundingClaim, mockPreviousFundingClaim, serviceBusMessage.FundingClaimId, false, false, isPreviousVerionAvailable);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(), UserRole.SignFundingClaims.ToString() });

            // Act
            await _fundingClaimWithdrawnEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [DataTestMethod]
        [DataRow(true, Constants.MessageType_FundingClaimWithdrawnPreviousVersionSignedEmail)]
        [DataRow(false, Constants.MessageType_FundingClaimWithdrawnNotSignedEmail)]
        public async Task Process_WithValidFundingClaimRequiredUserRolesAndNotificationServiceReturnsError_ThrowsException(bool isPreviousVerionAvailable, string messageType)
        {
            // Arrange
            var serviceBusMessage = new FundingClaimWithdrawnEmailMessage()
            {
                FundingClaimId = 1
            };

            var mockFundingClaim = new FundingClaim
            {
                Id = serviceBusMessage.FundingClaimId,
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = DateTime.UtcNow,
                }
            };

            var mockPreviousFundingClaim = new FundingClaim
            {
                Id = serviceBusMessage.FundingClaimId,
                Title = "mockTitle",
                SignedOn = DateTime.UtcNow,
                SignedBy = "testuser",
                SignedByDisplayName = "test user1",
                FundingClaimWindow = new FundingClaimWindow
                {
                    SignatureCloseDate = DateTime.UtcNow,
                }
            };

            NotificationMessage expectedMessage;
            if (isPreviousVerionAvailable)
            {
                expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockPreviousFundingClaim.SignedOn, mockPreviousFundingClaim.SignedByDisplayName, mockFundingClaim.FundingClaimWindow.SignatureCloseDate, isPreviousVerionAvailable, messageType);
            }
            else
            {
                expectedMessage = GetNotificationMessageMock(mockFundingClaim.Title, mockFundingClaim.SignedOn, mockFundingClaim.SignedByDisplayName, mockFundingClaim.FundingClaimWindow.SignatureCloseDate, isPreviousVerionAvailable, messageType);
            }

            SetupServicesMock(mockFundingClaim, mockPreviousFundingClaim, serviceBusMessage.FundingClaimId, true, false, isPreviousVerionAvailable);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(), UserRole.SignFundingClaims.ToString() });

            // Act
            Func<Task> actual = async () => { await _fundingClaimWithdrawnEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<Exception>();
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        private static FundingClaimsDataApiConfiguration CreateFundingClaimsDataApiConfiguration()
            => new ()
            {
                ApiBaseAddress = TestBaseAddress
            };

        private static NotificationMessage GetNotificationMessageMock(string documentTitle, DateTime? signedOn, string signedByDisplayName, DateTime? deadline, bool isPreviousVersionAvailable, string messageType)
        {
            var personalisation = new Dictionary<string, object>()
            {
                { "FundingClaimTitle",  documentTitle },
                { "Deadline", deadline.HasValue ? deadline.Value.DisplayFormat() : null }
            };

            if (isPreviousVersionAvailable)
            {
                personalisation.Add("PreviousSigner", signedByDisplayName);
                personalisation.Add("PreviousSignedDateTime", signedOn.HasValue ? signedOn.Value.DisplayFormat() : null);
            }

            return new NotificationMessage()
            {
                EmailMessageType = messageType,
                RequestingService = Constants.RequestingService_FundingClaims,
                EmailAddresses = new[] { "testemail" }.AsEnumerable(),
                EmailPersonalisation = new GovUkNotifyPersonalisation()
                {
                    Personalisation = personalisation
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

        private void SetupServicesMock(FundingClaim mockFundingClaim, FundingClaim mockPreviousFundingClaim, int fundingClaimId, bool isNotificationEmailQueueServiceThrowException = false, bool isFundingClaimDataApiThrowException = false, bool isPreviousVersionAvailable = false)
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

            if (isPreviousVersionAvailable)
            {
                var previousVersionresponse = new StringContent(JsonConvert.SerializeObject(mockPreviousFundingClaim), System.Text.Encoding.UTF8, "application/json");

                _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.PreviouslySignedVersionOfFundingClaimByIdEndpoint + "/" + fundingClaimId)
                .Respond(HttpStatusCode.OK, previousVersionresponse);
            }

            if (!isPreviousVersionAvailable && mockPreviousFundingClaim != null)
            {
                _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.PreviouslySignedVersionOfFundingClaimByIdEndpoint + "/" + fundingClaimId)
                .Respond(HttpStatusCode.OK, x => null);
            }
        }
    }
}
