using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.API.Contracts.Responses;
using Shop.API.IntegrationTests.ApiIntegrationTests.Product;
using Shop.API.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Shop.API.IntegrationTests.ApiIntegrationTests.Auth
{
    [Collection("Database Tests")]
    public sealed class AuthController_Tests
        (CustomWebApplicationFactory<Program> factory)
        : ApiTestsBase(factory)
    {
        [Fact]
        public async Task Register_Succeeds_WhenValidDataProvided()
        {
            // Arrange
            SeedDatabaseWithUsers();

            var registerRequest = new
            {
                Name = "John",
                Email = "john.doe@example.com",
                Password = "Password123!",
                Role = "Customer"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.EnsureSuccessStatusCode();

            // Verify database
            await VerifyUserInDatabase(registerRequest.Email, registerRequest.Name, registerRequest.Role);
        }

        [Fact]
        public async Task Register_Fails_WhenRoleIsInvalid()
        {
            // Arrange
            SeedDatabaseWithUsers();

            var registerRequest = new
            {
                Name = "John",
                Email = "john.doe@example.com",
                Password = "Password123!",
                Role = "InvalidRole"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.ShouldFail()
                .WithErrors("Role must be either 'Admin' or 'Customer'.");
        }

        [Fact]
        public async Task Register_Fails_WhenEmailAlreadyExists()
        {
            // Arrange
            SeedDatabaseWithUsers();

            var registerRequest = new
            {
                Name = "John",
                Email = "existing.user@example.com",
                Password = "Password123!",
                Role = "Customer"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.ShouldFail()
                .WithErrors("Email 'existing.user@example.com' is already taken.");
        }

        [Fact]
        public async Task Register_Fails_WhenValidationErrorsOccur()
        {
            // Arrange
            SeedDatabaseWithUsers();

            var invalidRegisterRequest = new
            {
                Name = "",
                Email = "invalid-email",
                Password = "123",
                Role = "InvalidRole"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", invalidRegisterRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var errors = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            errors.Should().NotBeNull();
            errors!.Errors.Should().ContainKey("Name").WhoseValue.Should().Contain("Name is required.");
            errors.Errors.Should().ContainKey("Email").WhoseValue.Should().Contain("Invalid email address.");
            errors.Errors.Should().ContainKey("Password").WhoseValue.Should().Contain("Password must be between 6 and 100 characters.");
            errors.Errors.Should().ContainKey("Role").WhoseValue.Should().Contain("Role must be either 'Admin' or 'Customer'.");
        }

        [Fact]
        public async Task Login_Succeeds_WithValidCredentials()
        {
            // Arrange
            SeedDatabaseWithUsers();
            var loginRequest = new
            {
                Email = "existing.user@example.com",
                Password = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            var contentResult = response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_Fails_WhenCredentialsAreInvalid()
        {
            // Arrange
            SeedDatabaseWithUsers();
            var loginRequest = new
            {
                Email = "existing.user@example.com",
                Password = "WrongPassword!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.ShouldFail()
                .WithErrors("Invalid email or password.");
        }

        private void SeedDatabaseWithUsers()
        {
            SeedUsersAndRoles((userManager, roleManager) =>
            {
                // Ensure roles exist
                var roles = new[] { "Customer", "Admin" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                    }
                }

                // Create existing user
                if (userManager.FindByEmailAsync("existing.user@example.com").GetAwaiter().GetResult() == null)
                {
                    var user = new IdentityUser
                    {
                        UserName = "existing.user@example.com",
                        Email = "existing.user@example.com",
                        EmailConfirmed = true
                    };

                    userManager.CreateAsync(user, "Password123!").GetAwaiter().GetResult();
                    userManager.AddToRoleAsync(user, "Customer").GetAwaiter().GetResult();
                }
            });
        }

        private async Task VerifyUserInDatabase(string email, string name, string role)
        {
            using var context = GetDbContext();

            var userInDb = await context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.UserName,
                    Roles = context.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                        .ToList()
                })
                .SingleOrDefaultAsync(u => u.Email == email);

            userInDb.Should().NotBeNull();
            userInDb!.UserName.Should().Be(name);

            var roleInDb = userInDb.Roles.FirstOrDefault();
            roleInDb.Should().Be(role);
        }
    }
}
