using FluentAssertions;
using Moq;
using System;

namespace Pds.Contracts.Notifications.Services.Tests.Extensions
{
    public static class ItIs
    {
        public static T EquivalentTo<T>(T expected)
        {
            Func<T, bool> validate = actual =>
            {
                try
                {
                    actual.Should().BeEquivalentTo(expected);
                    return true;
                }
                catch
                {
                    return false;
                }
            };

            return Match.Create<T>(s => validate(s));
        }
    }
}