using FluentAssertions;
using Newtonsoft.Json;

namespace dotNetFinalProjectAPITesting.Tests.Users;

[TestFixture]
public class GetUsersTests : TestBase
{
    [Test]
    [Parallelizable]
    public async Task Get_All_Users_Returns_OK()
    {
        var response = await Client.GetAsync("users");

        response.StatusCode.ToString().Should().Be("OK");

        string responseData = await response.Content.ReadAsStringAsync();
        var users = JsonConvert.DeserializeObject<List<UserResponse>>(responseData);

        users.Should().NotBeNull();
        users.Count().Should().Be(10);
        foreach (var user in users)
        {
            user.id.Should().BePositive();
            user.name.Should().NotBeNullOrWhiteSpace(); 
            user.email.Should().NotBeNullOrWhiteSpace();
            user.status.Should().NotBeNullOrWhiteSpace();
            user.gender.Should().NotBeNullOrWhiteSpace();
        }
    }
    
    [Test]
    [Parallelizable]
    public async Task Get_User_By_Id_Returns_OK()
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
        
        var getResponse = await Client.GetAsync("users/" + userFromPostRequest.id);
        
        var getResponseBody = await getResponse.Content.ReadAsStringAsync();

        var userFromGetRequest = JsonConvert.DeserializeObject<UserResponse>(getResponseBody);
        
        userFromGetRequest.name.Should().Be(newUser.name);
        userFromGetRequest.status.Should().Be(newUser.status);
        userFromGetRequest.gender.Should().Be(newUser.gender);
        userFromGetRequest.email.Should().Be(newUser.email);
    }
    
    [Test]
    [Parallelizable]
    public async Task Get_User_By_Nonexistent_Id_Returns_NotFound()
    {
        var response = await Client.GetAsync("users/0");

        response.StatusCode.ToString().Should().Be("NotFound");

        string responseData = await response.Content.ReadAsStringAsync();

        responseData.Should().Contain("Resource not found");
    }
}