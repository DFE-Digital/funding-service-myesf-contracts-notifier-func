using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.Notifications.Services.Extensions;
using System;

namespace Pds.Contracts.Notifications.Services.Tests.Extensions
{
    [TestClass, TestCategory("Unit")]
    public class DateExtensionsTests
    {
        [TestMethod]
        public void DateExtensions_GMTWhenDisplayFormatCalled_ReturnsDateStringAsExpected()
        {
            // Arrange
            var input = new DateTime(2024, 1, 5, 22, 6, 45);
            var expected = "5 January 2024 at 10:06pm";

            // Act
            var result = input.DisplayFormat();

            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void DateExtensions_BSTWhenDisplayFormatCalled_ReturnsDateStringAsExpected()
        {
            // Arrange
            var input = new DateTime(2024, 10, 5, 22, 6, 45);
            var expected = "5 October 2024 at 11:06pm";

            // Act
            var result = input.DisplayFormat();

            // Assert
            result.Should().Be(expected);
        }
    }
}
