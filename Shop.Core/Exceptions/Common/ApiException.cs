namespace Shop.Core.Exceptions.Common
{
    // It's HTTP related stuff, move to API

    public class ApiException(string message, int statusCode) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }

    public /*sealed*/ class NotFoundException(string message) : ApiException(message, 404) // mark it BASE then?
    {
    }

    public sealed class BadRequestException(string message) : ApiException(message, 400)
    {
    }

    public sealed class UnauthorizedException(string message) : ApiException(message, 401)
    {
    }

    public sealed class InternalServerErrorException(string message) : ApiException(message, 500)
    {
    }
}
