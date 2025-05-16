using FinalProject.Framework.Helpers;
using FinalProject.Framework.Models;
using FinalProject.Framework.Service;
using System.Net;
using System.Text.Json;

namespace FinalProject.Tests.UserTests
{
    public class GetUserTests : TestBase
    {
        private int _createdUserId;

        [SetUp]
        public async Task SetUp()
        {
            // Arrange: create a user before each test
            var newUser = TestDataGenerator.GenerateUser();
            var response = await UserService.CreateUserAsync(newUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            var createdUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
            _createdUserId = createdUser!.Id;
        }

        [Test]
        public async Task GetAllUsers_ShouldReturnOkStatusAndUserList()
        {
            // Act: request all users
            var response = await UserService.GetAllUsersAsync();
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: verify OK status and non-empty user list
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Should().NotBeNullOrEmpty();

            var users = JsonSerializer.Deserialize<List<UserResponse>>(responseContent);
            users.Should().NotBeNull();
            users.Should().BeOfType<List<UserResponse>>();
        }

        [Test]
        public async Task GetUserById_ExistingUser_ShouldReturnOkStatusAndUser()
        {
            // Arrange: verify user exists
            _createdUserId.Should().BeGreaterThan(0, "User must be created first");

            // Act: get user by id
            var response = await UserService.GetUserByIdAsync(_createdUserId);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: verify OK status and correct user data
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Should().NotBeNullOrEmpty();

            var user = JsonSerializer.Deserialize<UserResponse>(responseContent);
            user.Should().NotBeNull();
            user!.Id.Should().Be(_createdUserId);
            user.Name.Should().NotBeNullOrEmpty();
            user.Email.Should().NotBeNullOrEmpty();
            user.Gender.Should().BeOneOf("male", "female");
            user.Status.Should().BeOneOf("active", "inactive");
        }

        [Test]
        public async Task GetUserById_NonExistingUser_ShouldReturnNotFound()
        {
            // Arrange: define non-existing user id
            var nonExistingUserId = 999999999;

            // Act: request user by non-existing id
            var response = await UserService.GetUserByIdAsync(nonExistingUserId);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: verify NotFound status and error message
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseContent.Should().NotBeNullOrEmpty();

            bool containsErrorMessage = responseContent.ToLower().Contains("resource not found") ||
                                       responseContent.ToLower().Contains("not found");
            containsErrorMessage.Should().BeTrue("Response should contain 'not found' or 'resource not found'");
        }

        [Test]
        public async Task GetUserById_AfterDeletion_ShouldReturnNotFound()
        {
            // Act: delete user, then attempt to get by id
            var deleteResponse = await UserService.DeleteUserAsync(_createdUserId);
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var response = await UserService.GetUserByIdAsync(_createdUserId);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: verify NotFound status and error message
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseContent.ToLower().Should().Contain("not found");
        }

    }
}
