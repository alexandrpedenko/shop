namespace Shop.Core.Helpers.OperationResult
{
    public sealed class OperationResult<T>
    {
        public T? Value { get; }

        public bool IsSuccess { get; }

        public string? ErrorMessage { get; }

        public OperationErrorType ErrorType { get; }

        private OperationResult(T value)
        {
            IsSuccess = true;
            ErrorType = OperationErrorType.None;
            Value = value;
        }

        private OperationResult(string errorMessage, OperationErrorType errorType)
        {
            IsSuccess = false;
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }

        public static OperationResult<T> Success(T value) => new(value);

        public static OperationResult<T> Failure(string errorMessage, OperationErrorType errorType) => new(errorMessage, errorType);
    }

}
