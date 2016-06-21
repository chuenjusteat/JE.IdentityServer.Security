using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JE.IdentityServer.Security.Tests.Infrastructure;
using NUnit.Framework;

namespace JE.IdentityServer.Security.Tests.Throttling
{
    public class ThrottledRequestsWithExcludedOsVersion
    {
        [Test]
        public async Task ThrottledRequestsWithExcludedOsVersion_WithAllowedFailures_ShouldNotThrottle()
        {
            const int numberOfAllowedLoginFailures = 3;
            const int numberOfAttemptsThatShouldTriggerThrottling = numberOfAllowedLoginFailures + 1;
            using (var identityServerWithThrottledLoginRequests = new IdentityServerWithThrottledLoginRequests()
                .WithNumberOfAllowedLoginFailures(numberOfAllowedLoginFailures)
                .WithExcludedOsVersionExpression("5\\.0")
                .WithProtectedGrantType("password"))
            {
                var server = identityServerWithThrottledLoginRequests.Build();

                for (var attempt = 0; attempt < numberOfAttemptsThatShouldTriggerThrottling; ++attempt)
                {
                    var response = await server.CreateNativeLoginRequest()
                        .WithUsername("jeuser.example.com")
                        .WithPassword("Passw0rd")
                        .WithOsVersion("5.0")
                        .Build()
                        .PostAsync();

                    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                    var tokenFailureResponse = await response.Content.ReadAsAsync<TokenFailureResponseModel>();
                    tokenFailureResponse.Error.Should().Be("invalid_grant");
                }
            }
        }

        [Test]
        public async Task ThrottledRequestsWithExcludedOsVersion_WithAllowedFailures_ShouldPublishExcludedLoginStatistics()
        {
            const int numberOfAllowedLoginFailures = 3;
            const int numberOfAttemptsThatShouldTriggerThrottling = numberOfAllowedLoginFailures + 1;
            using (var identityServerWithThrottledLoginRequests = new IdentityServerWithThrottledLoginRequests()
                .WithNumberOfAllowedLoginFailures(numberOfAllowedLoginFailures)
                .WithExcludedOsVersionExpression("5\\.0")
                .WithProtectedGrantType("password"))
            {
                var server = identityServerWithThrottledLoginRequests.Build();

                for (var attempt = 0; attempt < numberOfAttemptsThatShouldTriggerThrottling; ++attempt)
                {
                    await server.CreateNativeLoginRequest()
                        .WithUsername("jeuser.example.com")
                        .WithPassword("Passw0rd")
                        .WithOsVersion("5.0")
                        .Build()
                        .PostAsync();
                }

                identityServerWithThrottledLoginRequests.LoginStatistics.TotalNumberOfExcludedAttemptedLogins.Should()
                    .Be(numberOfAttemptsThatShouldTriggerThrottling);
            }
        }
    }
}