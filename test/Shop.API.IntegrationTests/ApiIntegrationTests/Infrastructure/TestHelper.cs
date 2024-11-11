using FluentAssertions;
using System.Net;

namespace Shop.API.IntegrationTests.ApiIntegrationTests.Product
{
    public static class TestHelper
    {
        public static HttpResponseMessage ShouldFail(this HttpResponseMessage response)
        {
            response.StatusCode.Should().Match(code =>
                 code == HttpStatusCode.BadRequest || code == HttpStatusCode.NotFound,
                 "Expected either 400 BadRequest or 404 NotFound, but received {0}", response.StatusCode);

            return response;
        }

        public static HttpResponseMessage WithErrors(this HttpResponseMessage response, params string[] errors)
        {
            var content = response.Content;
            var jsonString = content.ReadAsStringAsync().GetAwaiter().GetResult();
            jsonString.Should().ContainAny(errors);
            return response;
        }
    }
}
