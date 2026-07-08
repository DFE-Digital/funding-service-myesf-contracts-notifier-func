using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Func.SubcontractorDeclarations;
using Pds.Contracts.Notifications.Services.Interfaces.SubcontractorDeclarations;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit.SubcontractorDeclarations
{
    [TestClass, TestCategory("Unit")]
    public class SubcontractorDeclarationEmailFunctionTests
    {
        [TestMethod]
        public async Task Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<ISubcontractorDeclarationEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<SubcontractorDeclarationEmailMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable(Times.Once);

            var function = new SubcontractorDeclarationEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new SubcontractorDeclarationEmailMessage() { SubcontractorDeclarationId = 1 }); };

            // Assert
            await act.Should().NotThrowAsync();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_ThrowException()
        {
            // Arrange
            var mockService = new Mock<ISubcontractorDeclarationEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<SubcontractorDeclarationEmailMessage>()))
                .ThrowsAsync(It.IsAny<Exception>())
                .Verifiable(Times.Once);

            var function = new SubcontractorDeclarationEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new SubcontractorDeclarationEmailMessage() { SubcontractorDeclarationId = 1 }); };

            // Assert
            await act.Should().ThrowAsync<Exception>();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_WhenValidationFailure_ThrowsArgumentException()
        {
            var mockService = new Mock<ISubcontractorDeclarationEmailService>(MockBehavior.Strict);

            var function = new SubcontractorDeclarationEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new SubcontractorDeclarationEmailMessage()); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
