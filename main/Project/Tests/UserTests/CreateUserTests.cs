using FinalProject.Framework.Helpers;
using FinalProject.Framework.Models;
using System.Net;
using System.Text.Json;

namespace FinalProject.Tests.UserTests
{
    public class CreateUserTests : TestBase
    {
        [Test]
        public async Task CreateUser_ShouldReturnCreatedStatusAndUser()
        {
            // Arrange
            var newUser = TestDataGenerator.GenerateUser();

            // Act
            var response = await UserService.CreateUserAsync(newUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            responseContent.Should().NotBeNullOrEmpty();

            var createdUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
            createdUser.Should().NotBeNull();
            createdUser!.Id.Should().BeGreaterThan(0);
            createdUser.Name.Should().Be(newUser.Name);
            createdUser.Email.Should().Be(newUser.Email);
            createdUser.Gender.Should().Be(newUser.Gender);
            createdUser.Status.Should().Be(newUser.Status);
        }

        [Test]
        public async Task CreateUserWithExistingEmail_ShouldReturnUnprocessableEntity()
        {
            // Arrange - Create a user first
            var user1 = TestDataGenerator.GenerateUser();
            var createResponse1 = await UserService.CreateUserAsync(user1);
            createResponse1.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdUser1 = JsonSerializer.Deserialize<UserResponse>(
                await createResponse1.Content.ReadAsStringAsync());

            // Create another user with the same email
            var user2 = TestDataGenerator.GenerateUser();
            user2.Email = user1.Email;

            // Act
            var response = await UserService.CreateUserAsync(user2);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseContent.Should().NotBeNullOrEmpty();

            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().NotBeNull();
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.field == "email" && e.message.Contains("has already been taken"));

            await UserService.DeleteUserAsync(createdUser1!.Id);
        }

        [Test]
        public async Task CreateUser_WithoutEmail_ShouldReturnUnprocessableEntity()
        {
            // Arrange
            var user = TestDataGenerator.GenerateUser();
            user.Email = null;

            // Act
            var response = await UserService.CreateUserAsync(user);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseContent.Should().NotBeNullOrEmpty();

            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().Contain(e => e.field == "email");
        }

        [Test]
        public async Task CreateUser_WithoutName_ShouldReturnUnprocessableEntity()
        {
            // Arrange
            var user = TestDataGenerator.GenerateUser();
            user.Name = null;

            // Act
            var response = await UserService.CreateUserAsync(user);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseContent.Should().NotBeNullOrEmpty();

            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().Contain(e => e.field == "name");
        }

        [Test]
        public async Task CreateUser_WithInvalidGender_ShouldReturnUnprocessableEntity()
        {
            // Arrange
            var user = TestDataGenerator.GenerateUser();
            user.Gender = "unknown";

            // Act
            var response = await UserService.CreateUserAsync(user);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().Contain(e => e.field == "gender");
        }

        [Test]
        public async Task CreateUser_InvalidData_ShouldReturnUnprocessableEntity()
        {
            // Arrange
            var invalidUser = new CreateUserRequest
            {
                Name = "",
                Email = "invalid-email",
                Gender = "invalid",
                Status = "invalid"
            };

            // Act
            var response = await UserService.CreateUserAsync(invalidUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseContent.Should().NotBeNullOrEmpty();

            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().NotBeNull();
            errors.Should().NotBeEmpty();
        }
    }
}
