using FluentAssertions;
using Newtonsoft.Json;

namespace dotNetFinalProjectAPITesting.Tests.Users;

public class PostUsersTests : TestBase
{
    [Test]
    [Parallelizable]
    public async Task Post_CreateUser_Returns_OK()
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
    }

    [Test]
    [Parallelizable]
    public async Task Post_CreateUser_UsedEmail_Returns_UnprocessableEntity()
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
        
        var secondPostResponse = await Client.PostAsync("users", postRequest);
        
        secondPostResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
        
        var responseBody = await secondPostResponse.Content.ReadAsStringAsync();
        
        var errorMessage = JsonConvert.DeserializeObject<List<ErrorResponse>>(responseBody);
        
        errorMessage[0].field.Should().Be("email");
        errorMessage[0].message.Should().Be("has already been taken");
    }
    
    [Test]
    [Parallelizable]
    public async Task Post_CreateUser_EmptyName_Returns_UnprocessableEntity()
    {
        var newUser = new
        {
            name = "",
            email = $"2pac{Guid.NewGuid()}@testovi.com",
            gender = "male",
            status = "active",
        };
        
        var requestBody = JsonConvert.SerializeObject(newUser);
        
        var postRequest = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
        
        var postResponse = await Client.PostAsync("users", postRequest);
        
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
        
        var responseBody = await postResponse.Content.ReadAsStringAsync();
        
        var errorMessage = JsonConvert.DeserializeObject<List<ErrorResponse>>(responseBody);
        
        errorMessage[0].field.Should().Be("name");
        errorMessage[0].message.Should().Be("can't be blank");
    }
    
    [Test]
    [Parallelizable]
    public async Task Post_CreateUser_InvalidGender_Returns_UnprocessableEntity()
    {
        var newUser = new
        {
            name = "2pac",
            email = $"2pac{Guid.NewGuid()}@testovi.com",
            gender = "test",
            status = "active",
        };
        
        var requestBody = JsonConvert.SerializeObject(newUser);
        
        var postRequest = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
        
        var postResponse = await Client.PostAsync("users", postRequest);
        
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
        
        var responseBody = await postResponse.Content.ReadAsStringAsync();
        
        var errorMessage = JsonConvert.DeserializeObject<List<ErrorResponse>>(responseBody);
        
        errorMessage[0].field.Should().Be("gender");
        errorMessage[0].message.Should().Be("can't be blank, can be male of female");
    }
    
    [Test]
    [Parallelizable]
    public async Task Post_CreateUser_InvalidEmail_Returns_UnprocessableEntity()
    {
        var newUser = new
        {
            name = "2pac",
            email = "test@+",
            gender = "male",
            status = "active",
        };
        
        var requestBody = JsonConvert.SerializeObject(newUser);
        
        var postRequest = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
        
        var postResponse = await Client.PostAsync("users", postRequest);
        
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
        
        var responseBody = await postResponse.Content.ReadAsStringAsync();
        
        var errorMessage = JsonConvert.DeserializeObject<List<ErrorResponse>>(responseBody);
        
        errorMessage[0].field.Should().Be("email");
        errorMessage[0].message.Should().Be("is invalid");
    }
}