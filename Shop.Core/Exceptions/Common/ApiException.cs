namespace Shop.Core.Exceptions.Common
{
    public class ApiException(string message, int statusCode) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }

    public class NotFoundException(string message) : ApiException(message, 404)
    {
    }

    public class BadRequestException(string message) : ApiException(message, 400)
    {
    }

    public class UnauthorizedException(string message) : ApiException(message, 401)
    {
    }

    public class InternalServerErrorException(string message) : ApiException(message, 500)
    {
    }
}
