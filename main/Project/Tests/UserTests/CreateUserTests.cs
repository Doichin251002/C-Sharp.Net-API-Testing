using FinalProject.Framework.Helpers;
using FinalProject.Framework.Models;
using System.Net;
using System.Text.Json;

namespace FinalProject.Tests.UserTests
{
    public class CreateUserTests : TestBase
    {
        // Valid user creation scenarios

        [Test]
        public async Task CreateUser_ShouldReturnCreatedStatusAndUser()
        {
            // Arrange: generate a valid user
            var newUser = TestDataGenerator.GenerateUser();

            // Act: send create user request
            var response = await UserService.CreateUserAsync(newUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: response is Created and user data matches
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
        public async Task CreateUser_WithMaxLengthName_ShouldReturnCreated()
        {
            // Arrange: generate user with max allowed name length (100)
            var user = TestDataGenerator.GenerateUser();
            user.Name = new string('A', 100);

            // Act: create user
            var response = await UserService.CreateUserAsync(user);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: should create successfully and name matches
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
            createdUser!.Name.Should().Be(user.Name);
        }

        [Test]
        public async Task CreateUserWithExistingEmail_ShouldReturnUnprocessableEntity()
        {
            // Arrange: create a user first
            var user1 = TestDataGenerator.GenerateUser();
            var createResponse1 = await UserService.CreateUserAsync(user1);
            createResponse1.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdUser1 = JsonSerializer.Deserialize<UserResponse>(
                await createResponse1.Content.ReadAsStringAsync());

            // Arrange: prepare second user with same email
            var user2 = TestDataGenerator.GenerateUser();
            user2.Email = user1.Email;

            // Act: try to create second user with duplicate email
            var response = await UserService.CreateUserAsync(user2);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: should return UnprocessableEntity with email taken error
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseContent.Should().NotBeNullOrEmpty();

            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().NotBeNull();
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.field == "email" && e.message.Contains("has already been taken"));

            // Cleanup
            await UserService.DeleteUserAsync(createdUser1!.Id);
        }

        // Invalid data scenarios

        [Test]
        public async Task CreateUser_WithoutEmail_ShouldReturnUnprocessableEntity()
        {
            // Arrange: generate user with null email
            var user = TestDataGenerator.GenerateUser();
            user.Email = null;

            // Act: try to create user
            var response = await UserService.CreateUserAsync(user);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: UnprocessableEntity with email error
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseContent.Should().NotBeNullOrEmpty();
            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().Contain(e => e.field == "email");
        }

        [Test]
        public async Task CreateUser_WithoutName_ShouldReturnUnprocessableEntity()
        {
            // Arrange: user without name
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
            // Arrange: user with invalid gender value
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
            // Arrange: user with multiple invalid fields
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

        [Test]
        public async Task CreateUser_WithTooLongName_ShouldReturnUnprocessableEntity()
        {
            // Arrange: name exceeding max length (e.g. 256 chars)
            var user = TestDataGenerator.GenerateUser();
            user.Name = new string('A', 256);

            // Act
            var response = await UserService.CreateUserAsync(user);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().Contain(e => e.field == "name");
        }

        [Test]
        public async Task CreateUser_WithWhitespaceName_ShouldReturnUnprocessableEntity()
        {
            // Arrange: name consisting only of whitespace
            var user = TestDataGenerator.GenerateUser();
            user.Name = "    ";

            // Act
            var response = await UserService.CreateUserAsync(user);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().Contain(e => e.field == "name");
        }

        [Test]
        public async Task CreateUser_WithInvalidEmailFormat_ShouldReturnUnprocessableEntity()
        {
            // Arrange: invalid email format (missing '@')
            var user = TestDataGenerator.GenerateUser();
            user.Email = "no-at-symbol.com";

            // Act
            var response = await UserService.CreateUserAsync(user);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().Contain(e => e.field == "email");
        }

        [Test]
        public async Task CreateUser_WithInvalidStatus_ShouldReturnUnprocessableEntity()
        {
            // Arrange: invalid status value
            var user = TestDataGenerator.GenerateUser();
            user.Status = "unknown-status";

            // Act
            var response = await UserService.CreateUserAsync(user);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().Contain(e => e.field == "status");
        }
    }
}
