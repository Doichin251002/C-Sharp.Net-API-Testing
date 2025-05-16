using FinalProject.Framework.Helpers;
using FinalProject.Framework.Models;
using FinalProject.Framework.Service;
using FinalProject.Tests;
using System.Net;
using System.Text.Json;

[TestFixture]
public class DeleteUserTests : TestBase
{
    private int _createdUserId;

    [SetUp]
    public async Task SetUp()
    {
        // Arrange: create a new user before each test
        var newUser = TestDataGenerator.GenerateUser();
        var response = await UserService.CreateUserAsync(newUser);
        var responseContent = await response.Content.ReadAsStringAsync();

        var createdUser = JsonSerializer.Deserialize<UserResponse>(responseContent);
        _createdUserId = createdUser!.Id;
    }

    // VALID DATA TESTS

    [Test]
    public async Task DeleteUser_ExistingUser_ShouldReturnNoContent()
    {
        // Arrange: ensure user is created
        _createdUserId.Should().BeGreaterThan(0);

        // Act: delete the created user
        var response = await UserService.DeleteUserAsync(_createdUserId);

        // Assert: deletion returns NoContent and user is no longer found
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await UserService.GetUserByIdAsync(_createdUserId);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteUser_Twice_ShouldReturnNotFoundOnSecondAttempt()
    {
        // Act: delete the user first time
        var firstDeleteResponse = await UserService.DeleteUserAsync(_createdUserId);
        firstDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act: try deleting the same user again
        var secondDeleteResponse = await UserService.DeleteUserAsync(_createdUserId);
        var responseContent = await secondDeleteResponse.Content.ReadAsStringAsync();

        // Assert: second delete returns NotFound with proper error message
        secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        responseContent.Should().NotBeNullOrEmpty();
        responseContent.ToLower().Should().Contain("not found");
    }

    [Test]
    public async Task DeleteUser_ThenRecreateUserWithSameEmail_ShouldSucceed()
    {
        // Arrange: create a user
        var user = TestDataGenerator.GenerateUser();
        var createResponse = await UserService.CreateUserAsync(user);
        var createdUser = JsonSerializer.Deserialize<UserResponse>(await createResponse.Content.ReadAsStringAsync());

        // Act: delete the created user
        var deleteResponse = await UserService.DeleteUserAsync(createdUser!.Id);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act: recreate a new user with the same email as the deleted user
        var recreatedUser = TestDataGenerator.GenerateUser();
        recreatedUser.Email = user.Email;
        var recreateResponse = await UserService.CreateUserAsync(recreatedUser);

        // Assert: recreation should succeed and user should be created
        recreateResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var recreated = JsonSerializer.Deserialize<UserResponse>(await recreateResponse.Content.ReadAsStringAsync());
        await UserService.DeleteUserAsync(recreated!.Id); // cleanup
    }

    [Test]
    public async Task DeleteUser_ConcurrentDeleteRequests_ShouldHandleGracefully()
    {
        // Act: send two delete requests concurrently for the same user ID
        var deleteTask1 = UserService.DeleteUserAsync(_createdUserId);
        var deleteTask2 = UserService.DeleteUserAsync(_createdUserId);

        await Task.WhenAll(deleteTask1, deleteTask2);

        // Assert: one request should succeed with NoContent, the other should get NotFound
        var statuses = new[] { deleteTask1.Result.StatusCode, deleteTask2.Result.StatusCode };
        statuses.Should().Contain(HttpStatusCode.NoContent);
        statuses.Should().Contain(HttpStatusCode.NotFound);
    }

    // INVALID DATA TESTS

    [Test]
    public async Task DeleteUser_NonExistingUser_ShouldReturnNotFound()
    {
        // Arrange: define a non-existing user ID
        var nonExistingUserId = 999999999;

        // Act: attempt to delete a non-existing user
        var response = await UserService.DeleteUserAsync(nonExistingUserId);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert: should return NotFound with appropriate error message
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        responseContent.Should().NotBeNullOrEmpty();
        (responseContent.ToLower().Contains("resource not found") || responseContent.ToLower().Contains("not found")).Should().BeTrue();
    }

    [Test]
    public async Task DeleteUser_WithInvalidId_ShouldReturnNotFound()
    {
        var invalidUserId = -1;
        var response = await UserService.DeleteUserAsync(invalidUserId);
        var responseContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        responseContent.Should().NotBeNullOrEmpty();
        responseContent.ToLower().Should().Contain("resource not found");
    }

}
