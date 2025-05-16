using FinalProject.Framework.Models;
using FinalProject.Framework.Helpers;
using System.Net;
using System.Text.Json;

namespace FinalProject.Tests.UserTests
{
    [TestFixture]
    public class DeleteUserTests : TestBase
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
        public async Task DeleteUser_ExistingUser_ShouldReturnNoContent()
        {
            // Arrange
            _createdUserId.Should().BeGreaterThan(0, "User must be created first");

            // Act
            var response = await UserService.DeleteUserAsync(_createdUserId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await UserService.GetUserByIdAsync(_createdUserId);
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeleteUser_Twice_ShouldReturnNotFoundOnSecondAttempt()
        {
            // Act
            var firstDeleteResponse = await UserService.DeleteUserAsync(_createdUserId);
            firstDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Act
            var secondDeleteResponse = await UserService.DeleteUserAsync(_createdUserId);
            var responseContent = await secondDeleteResponse.Content.ReadAsStringAsync();

            // Assert
            secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseContent.Should().NotBeNullOrEmpty();

            var containsErrorMessage = responseContent.ToLower().Contains("not found");
            containsErrorMessage.Should().BeTrue("Second delete should return 'not found'");
        }

        [Test]
        public async Task DeleteUser_NonExistingUser_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistingUserId = 999999999;

            // Act
            var response = await UserService.DeleteUserAsync(nonExistingUserId);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseContent.Should().NotBeNullOrEmpty();

            bool containsErrorMessage = responseContent.ToLower().Contains("resource not found") ||
                                       responseContent.ToLower().Contains("not found");
            containsErrorMessage.Should().BeTrue("Response should contain 'not found' or 'resource not found'");
        }

        [Test]
        public async Task DeleteUser_ThenRecreateUserWithSameEmail_ShouldSucceed()
        {
            // Arrange
            var user = TestDataGenerator.GenerateUser();
            var createResponse = await UserService.CreateUserAsync(user);
            var createdUser = JsonSerializer.Deserialize<UserResponse>(await createResponse.Content.ReadAsStringAsync());

            // Act
            var deleteResponse = await UserService.DeleteUserAsync(createdUser!.Id);
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Act
            var recreatedUser = TestDataGenerator.GenerateUser();
            recreatedUser.Email = user.Email;
            var recreateResponse = await UserService.CreateUserAsync(recreatedUser);

            // Assert
            recreateResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var recreated = JsonSerializer.Deserialize<UserResponse>(await recreateResponse.Content.ReadAsStringAsync());
            await UserService.DeleteUserAsync(recreated!.Id); // clean-up
        }

    }
}