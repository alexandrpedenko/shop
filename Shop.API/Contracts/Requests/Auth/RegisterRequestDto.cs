using System.ComponentModel.DataAnnotations;

namespace Shop.API.Contracts.Requests.Auth
{
    /// <summary>
    /// Request DTO for user registration
    /// </summary>
    public sealed record RegisterRequestDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string Name { get; init; } = default!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; init; } = default!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password must be between 6 and 100 characters.", MinimumLength = 6)]
        public string Password { get; init; } = default!;

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(Admin|Customer)$", ErrorMessage = "Role must be either 'Admin' or 'Customer'.")]
        public string Role { get; init; } = default!;
    }
}
