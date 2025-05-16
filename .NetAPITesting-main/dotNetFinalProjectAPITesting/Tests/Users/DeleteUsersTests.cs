using FluentAssertions;
using Newtonsoft.Json;

namespace dotNetFinalProjectAPITesting.Tests.Users;

public class DeleteUsersTests : TestBase
{
    [Test]
    [Parallelizable]
    public async Task Delete_DeleteUser_Returns_Ok()
    {
        var newUser = new
        {
            name = "2pac",
            email = $"2pac{Guid.NewGuid()}@testovi.com",
            gender = "male",
            status = "active",
        };
        
        var requestBody = JsonConvert.SerializeObject(newUser);
        
        var postRequest = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
        
        var postResponse = await Client.PostAsync("users", postRequest);

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var responseBody = await postResponse.Content.ReadAsStringAsync();
        
        var userFromPostRequest = JsonConvert.DeserializeObject<UserResponse>(responseBody);
        
        userFromPostRequest.name.Should().Be(newUser.name);
        userFromPostRequest.email.Should().Be(newUser.email);
        userFromPostRequest.status.Should().Be(newUser.status);
        userFromPostRequest.gender.Should().Be(newUser.gender);
        
        
        
        var deleteResponse = await Client.DeleteAsync($"users/{userFromPostRequest.id}");
        
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        
        
        
        var getResponse = await Client.GetAsync($"users/{userFromPostRequest.id}");
        
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        
        var getResponseBody = await getResponse.Content.ReadAsStringAsync();
        
        var errorMessage = JsonConvert.DeserializeObject<ErrorResponse>(getResponseBody);

        errorMessage.field.Should().BeNull();
        errorMessage.message.Should().Be("Resource not found");
    }
    
    [Test]
    [Parallelizable]
    public async Task Delete_DeleteUser_NonexistentID_Returns_NotFound()
    {
        var deleteResponse = await Client.DeleteAsync($"users/0");
        
        deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        
        var deleteResponseBody = await deleteResponse.Content.ReadAsStringAsync();
        
        var errorMessage = JsonConvert.DeserializeObject<ErrorResponse>(deleteResponseBody);

        errorMessage.field.Should().BeNull();
        errorMessage.message.Should().Be("Resource not found");
    }
}