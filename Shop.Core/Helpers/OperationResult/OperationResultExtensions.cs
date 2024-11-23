using Microsoft.AspNetCore.Mvc;

namespace Shop.Core.Helpers.OperationResult
{
    public static class OperationResultExtensions
    {
        public static IActionResult? CheckForAction<T>(this OperationResult<T> result)
        {
            if (!result.IsSuccess)
            {
                return new ObjectResult(result.ErrorMessage)
                {
                    StatusCode = result.ErrorType switch
                    {
                        OperationErrorType.NotFound => 404,
                        OperationErrorType.Validation => 400,
                        _ => 500
                    }
                }; ;
            }

            return null;
        }
    }
}
