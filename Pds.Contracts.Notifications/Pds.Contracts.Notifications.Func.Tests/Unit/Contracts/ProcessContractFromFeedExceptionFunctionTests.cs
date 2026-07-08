using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Func.Contracts;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit.Contracts
{
    [TestClass, TestCategory("Unit")]
    public class ProcessContractFromFeedExceptionFunctionTests
    {
        [TestMethod]
        public async Task Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IProcessContractFromFeedExceptionService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ProcessContractFromFeedExceptionMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable(Times.Once);

            var function = new ProcessContractFromFeedExceptionFunction(mockService.Object);

            // Act
            Func<Task> act = async () =>
            {
                await function.Run(new ProcessContractFromFeedExceptionMessage()
                {
                    ParentFeedStatus = "Approved",
                    FeedStatus = "teststatus",
                    ExistingContractStatus = "testexistingstatus",
                    ParentContractNumber = "testcontractNumber",
                    ContractNumber = "testcontract1",
                    ContractVersionNumber = 1,
                    ContractTitle = "testtitle",
                    ExceptionTime = DateTime.UtcNow,
                    ProviderName = "testname",
                    Ukprn = 12345678
                });
            };

            // Assert
            await act.Should().NotThrowAsync();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_ThrowException()
        {
            // Arrange
            var mockService = new Mock<IProcessContractFromFeedExceptionService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ProcessContractFromFeedExceptionMessage>()))
                .ThrowsAsync(It.IsAny<Exception>())
                .Verifiable(Times.Once);

            var function = new ProcessContractFromFeedExceptionFunction(mockService.Object);

            // Act
            Func<Task> act = async () =>
            {
                await function.Run(new ProcessContractFromFeedExceptionMessage()
                {
                    ParentFeedStatus = "Approved",
                    FeedStatus = "teststatus",
                    ExistingContractStatus = "testexistingstatus",
                    ParentContractNumber = "testcontractNumber",
                    ContractNumber = "testcontract1",
                    ContractVersionNumber = 1,
                    ContractTitle = "testtitle",
                    ExceptionTime = DateTime.UtcNow,
                    ProviderName = "testname",
                    Ukprn = 12345678
                });
            };

            // Assert
            await act.Should().ThrowAsync<Exception>();
            mockService.Verify();
        }

        [DataTestMethod]
        [DataRow("", 1, false, true)]
        [DataRow("provider1", 0, false)]
        [DataRow("provider1", 1, true)]
        public async Task Run_WhenValidationFailure_ThrowsArgumentException(string providerName, int contractVersionNumber, bool isExceptionTimeNullOrDefault, bool isArgumentNullException = false)
        {
            // Arrange
            var mockService = new Mock<IProcessContractFromFeedExceptionService>(MockBehavior.Strict);

            var function = new ProcessContractFromFeedExceptionFunction(mockService.Object);

            // Act
            Func<Task> act = async () =>
            {
                await function.Run(new ProcessContractFromFeedExceptionMessage()
                {
                    ParentFeedStatus = "Approved",
                    FeedStatus = "teststatus",
                    ExistingContractStatus = "testexistingstatus",
                    ParentContractNumber = "testcontractNumber",
                    ContractNumber = "testcontract1",
                    ContractVersionNumber = contractVersionNumber,
                    ContractTitle = "testtitle",
                    ExceptionTime = isExceptionTimeNullOrDefault ? default : DateTime.UtcNow,
                    ProviderName = providerName,
                    Ukprn = 12345678
                });
            };

            // Assert
            if (isArgumentNullException)
            {
                await act.Should().ThrowAsync<ArgumentNullException>();
            }
            else
            {
                await act.Should().ThrowAsync<ArgumentException>();
            }
        }
    }
}
