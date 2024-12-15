using System.ComponentModel.DataAnnotations;

namespace Shop.API.Contracts.Requests.Auth
{
    /// <summary>
    /// Request DTO for user login
    /// </summary>
    public sealed record LoginRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; init; } = default!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password must be between 6 and 100 characters.", MinimumLength = 6)]
        public string Password { get; init; } = default!;
    }
}
