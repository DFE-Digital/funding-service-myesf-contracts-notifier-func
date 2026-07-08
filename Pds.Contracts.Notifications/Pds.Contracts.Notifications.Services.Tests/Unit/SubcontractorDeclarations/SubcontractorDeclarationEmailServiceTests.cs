using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Extensions;
using Pds.Contracts.Notifications.Services.Implementations.SubcontractorDeclarations;
using Pds.Contracts.Notifications.Services.Interfaces.SubcontractorDeclarations;
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

namespace Pds.Contracts.Notifications.Services.Tests.Unit.SubcontractorDeclarations
{
    [TestClass, TestCategory("Unit")]
    public class SubcontractorDeclarationEmailServiceTests
    {
        private const string TestBaseAddress = "http://test-api-endpoint";
        private const string TestFakeAccessToken = "AccessToken";

        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<SubcontractorDeclarationEmailService>> _mockLogger;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<IDfESignInPublicApi> _mockDfESignInPublicApi;
        private readonly Mock<IAuditService> _mockAuditService;

        private readonly ISubcontractorDeclarationEmailService _subcontractorDeclarationEmailService;

        public SubcontractorDeclarationEmailServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);
            _mockLogger = new Mock<ILoggerAdapter<SubcontractorDeclarationEmailService>>(MockBehavior.Strict);
            _mockDfESignInPublicApi = new Mock<IDfESignInPublicApi>(MockBehavior.Strict);
            _mockAuditService = new Mock<IAuditService>(MockBehavior.Strict);
            _mockHttp = new MockHttpMessageHandler();

            _subcontractorDeclarationEmailService = new SubcontractorDeclarationEmailService(
                CreateAuthenticationService(),
                Options.Create(CreateSubcontractorDeclarationDataApiConfiguration()),
                _mockNotificationEmailQueueService.Object,
                _mockLogger.Object,
                _mockDfESignInPublicApi.Object,
                _mockAuditService.Object,
                _mockHttp.ToHttpClient());
        }

        [TestMethod]
        public async Task Process_WhenSubcontractorDeclarationDataApiUnreachable_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new SubcontractorDeclarationEmailMessage()
            {
                SubcontractorDeclarationId = 1
            };

            SetupServicesMock(null, serviceBusMessage.SubcontractorDeclarationId, false, true);

            // Act
            Func<Task> actual = async () => { await _subcontractorDeclarationEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<ApiGeneralException>();
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task Process_WithNoMatchedSubcontractorDeclaration_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new SubcontractorDeclarationEmailMessage()
            {
                SubcontractorDeclarationId = 1
            };

            SetupServicesMock(null, serviceBusMessage.SubcontractorDeclarationId);

            // Act
            Func<Task> actual = async () => { await _subcontractorDeclarationEmailService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<Exception>();
            result.Which.Message.Should().Be("Error fetching full subcontractor declaration.");
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task Process_WithValidSubcontractorDeclarationAndFailedDSIApiCall_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new SubcontractorDeclarationEmailMessage()
            {
                SubcontractorDeclarationId = 1
            };

            var mockFullSubcontractorDeclaration = new FullSubcontractorDeclaration
            {
                Ukprn = 12345678,
                SubmittedBy = "test",
                SubmittedByDisplayName = "test user",
                SubmittedAt = DateTime.UtcNow,
                SubcontractorDeclarationType = SubcontractorDeclarationSubmissionType.Nil
            };

            var expectedMessage = GetNotificationMessageMock(mockFullSubcontractorDeclaration.SubmittedByDisplayName, mockFullSubcontractorDeclaration.SubmittedAt, Constants.MessageType_SubcontractorReturnSubmissionNilReturnEmail);

            SetupServicesMock(mockFullSubcontractorDeclaration, serviceBusMessage.SubcontractorDeclarationId);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewPreviousSubcontractorDeclarations.ToString(), UserRole.SubmitSubcontractorDeclarations.ToString() }, false, true);

            // Act
            Func<Task> actual = async () => { await _subcontractorDeclarationEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<GetApiResultException>();
            _mockHttp.VerifyNoOutstandingExpectation();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
        }

        [DataTestMethod]
        [DataRow(SubcontractorDeclarationSubmissionType.Nil, Constants.MessageType_SubcontractorReturnSubmissionNilReturnEmail)]
        [DataRow(SubcontractorDeclarationSubmissionType.Full, Constants.MessageType_SubcontractorReturnSubmissionFullReturnEmail)]
        public async Task Process_WithValidSubcontractorDeclarationAndInvalidRoles_DoesNotSendSubcontractorDeclarationEmail(SubcontractorDeclarationSubmissionType subcontractorDeclarationSubmissionType, string messageType)
        {
            // Arrange
            var serviceBusMessage = new SubcontractorDeclarationEmailMessage()
            {
                SubcontractorDeclarationId = 1
            };

            var mockFullSubcontractorDeclaration = new FullSubcontractorDeclaration
            {
                Ukprn = 12345678,
                SubmittedBy = "test",
                SubmittedByDisplayName = "test user",
                SubmittedAt = DateTime.UtcNow,
                SubcontractorDeclarationType = subcontractorDeclarationSubmissionType
            };

            var expectedError = $"SubcontractorDeclaration id [{mockFullSubcontractorDeclaration.Id}], {nameof(SubcontractorDeclarationEmailMessage)} processed and no users found with roles [ViewPreviousSubcontractorDeclarations, SubmitSubcontractorDeclarations] for organisation [{mockFullSubcontractorDeclaration.Ukprn}]";

            SetupServicesMock(mockFullSubcontractorDeclaration, serviceBusMessage.SubcontractorDeclarationId);

            SetupDfESignInPublicApiMock(new[] { UserRole.DocumentExchangeUser.ToString() }, true);

            // Act
            await _subcontractorDeclarationEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockLogger.Verify(logger => logger.LogError(expectedError), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == $"{nameof(SubcontractorDeclarationEmailMessage)} processed and no users found with roles [ViewPreviousSubcontractorDeclarations, SubmitSubcontractorDeclarations] for organisation [{mockFullSubcontractorDeclaration.Ukprn}]")), Times.Once);
        }

        [DataTestMethod]
        [DataRow(SubcontractorDeclarationSubmissionType.Nil, Constants.MessageType_SubcontractorReturnSubmissionNilReturnEmail)]
        [DataRow(SubcontractorDeclarationSubmissionType.Full, Constants.MessageType_SubcontractorReturnSubmissionFullReturnEmail)]
        public async Task Process_WithValidSubcontractorDeclarationAndRequiredUserRoles_SuccessfullySendsSubcontractorDeclarationEmail(SubcontractorDeclarationSubmissionType subcontractorDeclarationSubmissionType, string messageType)
        {
            // Arrange
            var serviceBusMessage = new SubcontractorDeclarationEmailMessage()
            {
                SubcontractorDeclarationId = 1
            };

            var mockFullSubcontractorDeclaration = new FullSubcontractorDeclaration
            {
                Ukprn = 12345678,
                SubmittedBy = "test",
                SubmittedByDisplayName = "test user",
                SubmittedAt = DateTime.UtcNow,
                SubcontractorDeclarationType = subcontractorDeclarationSubmissionType
            };

            var expectedMessage = GetNotificationMessageMock(mockFullSubcontractorDeclaration.SubmittedByDisplayName, mockFullSubcontractorDeclaration.SubmittedAt, messageType);

            var expectedAuditMessage = "SubcontractorDeclarationEmailMessage processed and published to SharedEmailprocessorQueue.";

            SetupServicesMock(mockFullSubcontractorDeclaration, serviceBusMessage.SubcontractorDeclarationId);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewPreviousSubcontractorDeclarations.ToString(), UserRole.SubmitSubcontractorDeclarations.ToString() });

            // Act
            await _subcontractorDeclarationEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [DataTestMethod]
        [DataRow(SubcontractorDeclarationSubmissionType.Nil, Constants.MessageType_SubcontractorReturnSubmissionNilReturnEmail)]
        [DataRow(SubcontractorDeclarationSubmissionType.Full, Constants.MessageType_SubcontractorReturnSubmissionFullReturnEmail)]
        public async Task Process_WithValidSubcontractorDeclarationAndRequiredUserRolesWhenNotificationServiceReturnsError_ThrowsException(SubcontractorDeclarationSubmissionType subcontractorDeclarationSubmissionType, string messageType)
        {
            // Arrange
            var serviceBusMessage = new SubcontractorDeclarationEmailMessage()
            {
                SubcontractorDeclarationId = 1
            };

            var mockFullSubcontractorDeclaration = new FullSubcontractorDeclaration
            {
                Ukprn = 12345678,
                SubmittedBy = "test",
                SubmittedByDisplayName = "test user",
                SubmittedAt = DateTime.UtcNow,
                SubcontractorDeclarationType = subcontractorDeclarationSubmissionType
            };

            var expectedMessage = GetNotificationMessageMock(mockFullSubcontractorDeclaration.SubmittedByDisplayName, mockFullSubcontractorDeclaration.SubmittedAt, messageType);

            SetupServicesMock(mockFullSubcontractorDeclaration, serviceBusMessage.SubcontractorDeclarationId, true);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewPreviousSubcontractorDeclarations.ToString(), UserRole.SubmitSubcontractorDeclarations.ToString() });

            // Act
            Func<Task> actual = async () => { await _subcontractorDeclarationEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<Exception>();
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        private static SubcontractorDeclarationDataApiConfiguration CreateSubcontractorDeclarationDataApiConfiguration()
        => new ()
        {
            ApiBaseAddress = TestBaseAddress
        };

        private static NotificationMessage GetNotificationMessageMock(string submittedByDisplayName, DateTime? submittedOn, string messageType)
        {
            return new NotificationMessage()
            {
                EmailMessageType = messageType,
                RequestingService = Constants.RequestingService_SubcontractorDeclarations,
                EmailAddresses = new[] { "testemail" }.AsEnumerable(),
                EmailPersonalisation = new GovUkNotifyPersonalisation()
                {
                    Personalisation = new Dictionary<string, object>()
                    {
                        { "SubmittedBy",  submittedByDisplayName },
                        { "SubmittedOn", submittedOn.HasValue ? submittedOn.Value.DisplayFormat() : null }
                    }
                }
            };
        }

        private static IAuthenticationService<SubcontractorDeclarationDataApiConfiguration> CreateAuthenticationService()
        {
            var mockAuthenticationService = new Mock<IAuthenticationService<SubcontractorDeclarationDataApiConfiguration>>(MockBehavior.Strict);
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

        private void SetupServicesMock(FullSubcontractorDeclaration mockFullSubcontractorDeclaration, int subcontractorDeclarationId, bool isNotificationEmailQueueServiceThrowException = false, bool isSubcontractorDeclarationDataApiThrowException = false)
        {
            var response = new StringContent(JsonConvert.SerializeObject(mockFullSubcontractorDeclaration), System.Text.Encoding.UTF8, "application/json");

            var config = CreateSubcontractorDeclarationDataApiConfiguration();

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

            if (isSubcontractorDeclarationDataApiThrowException)
            {
                _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.FullSubcontractorDeclarationGetByIdEndpoint + "/" + subcontractorDeclarationId)
                .Throw(new ApiGeneralException());
            }
            else
            {
                _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.FullSubcontractorDeclarationGetByIdEndpoint + "/" + subcontractorDeclarationId)
                .Respond(HttpStatusCode.OK, response);
            }

            _mockLogger.Setup(logger => logger.LogInformation(It.IsAny<string>()));
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>()));
        }
    }
}
