using FinalProject.Framework.Helpers;
using FinalProject.Framework.Models;
using FinalProject.Framework.Service;
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
            // Arrange: create a valid user before each test
            var newUser = TestDataGenerator.GenerateUser();
            var response = await UserService.CreateUserAsync(newUser);
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
            _createdUserId = createdUser!.Id;
        }

        // ------------------- VALID DATA TESTS -------------------

        [Test]
        public async Task UpdateUserPut_ExistingUser_ShouldReturnOkStatusAndUpdatedUser()
        {
            // Arrange: prepare valid updated data for PUT
            var updateUser = TestDataGenerator.GenerateUser();

            // Act: send PUT request to update user
            var response = await UserService.UpdateUserPutAsync(_createdUserId, updateUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: verify response is OK and user is updated
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
            updatedUser!.Id.Should().Be(_createdUserId);
            updatedUser.Name.Should().Be(updateUser.Name);
            updatedUser.Email.Should().Be(updateUser.Email);
            updatedUser.Gender.Should().Be(updateUser.Gender);
            updatedUser.Status.Should().Be(updateUser.Status);
        }

        [Test]
        public async Task UpdateUserPatch_ExistingUser_ShouldReturnOkStatusAndPartiallyUpdatedUser()
        {
            // Arrange: prepare partial user update with PATCH
            var updateUser = TestDataGenerator.GenerateUpdateUser(new[] { "name", "status" });

            // Act: send PATCH request
            var response = await UserService.UpdateUserPatchAsync(_createdUserId, updateUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: verify response and update
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
            updatedUser!.Name.Should().Be(updateUser.Name);
            updatedUser.Status.Should().Be(updateUser.Status);
        }

        [Test]
        public async Task UpdateUserPut_ShouldNotChangeId()
        {
            // Arrange: generate valid update
            var updateUser = TestDataGenerator.GenerateUser();

            // Act: send PUT request
            var response = await UserService.UpdateUserPutAsync(_createdUserId, updateUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: ID should remain unchanged
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
            updatedUser!.Id.Should().Be(_createdUserId);
        }

        // ------------------- INVALID DATA TESTS -------------------

        [Test]
        public async Task UpdateUserPatch_InvalidFields_ShouldReturnUnprocessableEntity()
        {
            // Arrange: set invalid fields (email, gender)
            var invalidUpdate = new UpdateUserRequest
            {
                Email = "invalid-email",
                Gender = "unknown"
            };

            // Act: send PATCH request with invalid data
            var response = await UserService.UpdateUserPatchAsync(_createdUserId, invalidUpdate);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: should return 422 and error details
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent);
            errors.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task UpdateUserPut_NonExistingUser_ShouldReturnNotFound()
        {
            // Arrange: define non-existent user ID
            var nonExistingUserId = 999999999;
            var updateUser = TestDataGenerator.GenerateUser();

            // Act: attempt PUT update on non-existent user
            var response = await UserService.UpdateUserPutAsync(nonExistingUserId, updateUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: should return 404 with not found message
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseContent.ToLower().Should().Contain("not found");
        }

        [Test]
        public async Task UpdateUserPatch_NonExistingUser_ShouldReturnNotFound()
        {
            // Arrange: define non-existent ID
            var nonExistingUserId = 999999999;
            var updateUser = TestDataGenerator.GenerateUpdateUser(new[] { "status" });

            // Act: try to PATCH update non-existent user
            var response = await UserService.UpdateUserPatchAsync(nonExistingUserId, updateUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: should return 404
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseContent.ToLower().Should().Contain("not found");
        }

        [Test]
        public async Task UpdateUserPatch_WithEmptyBody_ShouldReturnBadRequestOrNoChange()
        {
            // Arrange: empty update (no fields)
            var emptyUpdate = new UpdateUserRequest();

            // Act: send PATCH with empty object
            var response = await UserService.UpdateUserPatchAsync(_createdUserId, emptyUpdate);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: accept empty update or return error
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.UnprocessableEntity, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdateUserPut_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange: invalid ID (zero)
            var updateUser = TestDataGenerator.GenerateUser();

            // Act: send PUT with ID = 0
            var response = await UserService.UpdateUserPutAsync(0, updateUser);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }
    }
}
