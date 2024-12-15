namespace Shop.API.Contracts.Responses
{
    /// <summary>
    /// Response DTO for user login
    /// </summary>
    public sealed record LoginResponseDto
    {
        public string Token { get; init; } = default!;
    }
}
