using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.Notifications.Services.Utilities;
using System;
using System.Collections.Generic;

namespace Pds.Contracts.Notifications.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void ReplaceTokensTest()
        {
            // Arrange
            var dictionaryOfTokens = new Dictionary<string, string>
            {
                { "test1", "value1" },
                { "test2", "value2" },
                { "test3", "value3" },
            };

            var tokensInType = new
            {
                Test4 = "value4",
                Test5 = 5,
                Test6 = new DateTime(2021, 1, 1)
            };

            var format = "This is test for replace tokens {test1}, {test2}, {test3}, {Test4}, {Test5}, {Test6}, {test7}";
            var expected = $"This is test for replace tokens value1, value2, value3, value4, 5, {new DateTime(2021, 1, 1)}, {{test7}}";

            // Act
            var result = format
                            .ReplaceTokens(dictionaryOfTokens)
                            .ReplaceTokens(tokensInType);

            // Assert
            result.Should().Be(expected);
        }
    }
}