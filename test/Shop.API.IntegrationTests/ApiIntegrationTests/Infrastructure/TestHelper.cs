using FluentAssertions;
using System.Net;

namespace Shop.API.IntegrationTests.ApiIntegrationTests.Product
{
    public static class TestHelper
    {
        public static HttpResponseMessage ShouldFail(this HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
