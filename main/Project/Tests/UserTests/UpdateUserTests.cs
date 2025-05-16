using FinalProject.Framework.Helpers;
using FinalProject.Framework.Models;
using System.Net;
using System.Text.Json;

namespace FinalProject.Tests.UserTests
{
    public class UpdateUserTests : TestBase
    {
        private int _createdUserId;

        [SetUp]
        public async Task SetUp()
        {
            var newUser = TestDataGenerator.GenerateUser();
            var response = await UserService.CreateUserAsync(newUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            var createdUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
            _createdUserId = createdUser!.Id;
        }

        [Test]
        public async Task UpdateUserPut_ExistingUser_ShouldReturnOkStatusAndUpdatedUser()
        {
            // Arrange
            _createdUserId.Should().BeGreaterThan(0, "User must be created first");
            var updateUser = TestDataGenerator.GenerateUser();

            // Act
            var response = await UserService.UpdateUserPutAsync(_createdUserId, updateUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Should().NotBeNullOrEmpty();

            var updatedUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
            updatedUser.Should().NotBeNull();
            updatedUser!.Id.Should().Be(_createdUserId);
            updatedUser.Name.Should().Be(updateUser.Name);
            updatedUser.Email.Should().Be(updateUser.Email);
            updatedUser.Gender.Should().Be(updateUser.Gender);
            updatedUser.Status.Should().Be(updateUser.Status);
        }

        [Test]
        public async Task UpdateUserPatch_ExistingUser_ShouldReturnOkStatusAndPartiallyUpdatedUser()
        {
            // Arrange
            _createdUserId.Should().BeGreaterThan(0, "User must be created first");
            var updateUser = TestDataGenerator.GenerateUpdateUser(new[] { "name", "status" });

            // Act
            var response = await UserService.UpdateUserPatchAsync(_createdUserId, updateUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"PATCH failed with status: {response.StatusCode}");
                Console.WriteLine($"Response: {responseContent}");
            }

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Should().NotBeNullOrEmpty();

            var updatedUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
            updatedUser.Should().NotBeNull();
            updatedUser!.Id.Should().Be(_createdUserId);
            updatedUser.Name.Should().Be(updateUser.Name);
            updatedUser.Status.Should().Be(updateUser.Status);
            updatedUser.Email.Should().NotBeNullOrEmpty();
            updatedUser.Gender.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task UpdateUserPatch_NonExistingUser_ShouldReturnNotFound()
        {
            var nonExistingUserId = 999999999;
            var updateUser = TestDataGenerator.GenerateUpdateUser(new[] { "name" });

            var response = await UserService.UpdateUserPatchAsync(nonExistingUserId, updateUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseContent.ToLower().Should().Contain("not found");
        }


        [Test]
        public async Task UpdateUserPut_NonExistingUser_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistingUserId = 999999999;
            var updateUser = TestDataGenerator.GenerateUser();

            // Act
            var response = await UserService.UpdateUserPutAsync(nonExistingUserId, updateUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseContent.Should().NotBeNullOrEmpty();

            bool containsErrorMessage = responseContent.ToLower().Contains("resource not found") ||
                                       responseContent.ToLower().Contains("not found");
            containsErrorMessage.Should().BeTrue("Response should contain 'not found' or 'resource not found'");
        }

        [Test]
        public async Task UpdateUserPatch_InvalidFields_ShouldReturnUnprocessableEntity()
        {
            var invalidUpdate = new UpdateUserRequest
            {
                Email = "invalid-email",
                Gender = "unknown"
            };

            var response = await UserService.UpdateUserPatchAsync(_createdUserId, invalidUpdate);
            var responseContent = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseContent.Should().NotBeNullOrEmpty();

            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().NotBeNull();
            errors.Should().NotBeEmpty();
        }

        [Test]
        public async Task UpdateUserPut_ShouldNotChangeId()
        {
            var updateUser = TestDataGenerator.GenerateUser();

            var response = await UserService.UpdateUserPutAsync(_createdUserId, updateUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
            updatedUser.Should().NotBeNull();
            updatedUser!.Id.Should().Be(_createdUserId);
        }

    }
}
