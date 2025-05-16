using Newtonsoft.Json;
using FluentAssertions;

namespace dotNetFinalProjectAPITesting.Tests.Users;

public class PutUsersTests : TestBase
{
    [Test]
    [Parallelizable]
    public async Task Put_UpdateUser_Returns_OK()
    {
        var user = new
        {
            name = "2pac",
            email = $"2pac{Guid.NewGuid()}@testovi.com",
            gender = "male",
            status = "active",
        };
        
        var postRequestBody = JsonConvert.SerializeObject(user);
        
        var postRequest = new StringContent(postRequestBody, System.Text.Encoding.UTF8, "application/json");
        
        var postResponse = await Client.PostAsync("users", postRequest);

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var postResponseBody = await postResponse.Content.ReadAsStringAsync();
        
        var userFromPostRequest = JsonConvert.DeserializeObject<UserResponse>(postResponseBody);
        
        

        var updatedUser = new
        {
            name = "Test1234",
            email = $"2pac{Guid.NewGuid()}@testovi.com",
            gender = "male",
            status = "active",
        };
        
        var putRequestBody = JsonConvert.SerializeObject(updatedUser);
        
        var putRequest = new StringContent(putRequestBody, System.Text.Encoding.UTF8, "application/json");
        
        var putResponse = await Client.PutAsync($"users/{userFromPostRequest.id}", putRequest);
        
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var putResponseBody = await putResponse.Content.ReadAsStringAsync();
        
        var userFromPutRequest = JsonConvert.DeserializeObject<UserResponse>(putResponseBody);
        
        userFromPutRequest.name.Should().Be(updatedUser.name);
        userFromPutRequest.email.Should().Be(updatedUser.email);
        userFromPutRequest.status.Should().Be(updatedUser.status);
        userFromPutRequest.gender.Should().Be(updatedUser.gender);
        
        
        
        var getResponse = await Client.GetAsync($"users/{userFromPostRequest.id}");

        getResponse.StatusCode.ToString().Should().Be("OK");

        var getResponseBody = await getResponse.Content.ReadAsStringAsync();
        
        var userFromGetResponse = JsonConvert.DeserializeObject<UserResponse>(getResponseBody);
        
        userFromGetResponse.name.Should().Be(updatedUser.name);
        userFromGetResponse.email.Should().Be(updatedUser.email);
        userFromGetResponse.status.Should().Be(updatedUser.status);
        userFromGetResponse.gender.Should().Be(updatedUser.gender);
    }
    
    [Test]
    [Parallelizable]
    public async Task Put_UpdateUser_EmptyEmail_Returns_UnprocessableEntity()
    {
        var user = new
        {
            name = "2pac",
            email = $"2pac{Guid.NewGuid()}@testovi.com",
            gender = "male",
            status = "active",
        };
        
        var postRequestBody = JsonConvert.SerializeObject(user);
        
        var postRequest = new StringContent(postRequestBody, System.Text.Encoding.UTF8, "application/json");
        
        var postResponse = await Client.PostAsync("users", postRequest);

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var postResponseBody = await postResponse.Content.ReadAsStringAsync();
        
        var userFromPostRequest = JsonConvert.DeserializeObject<UserResponse>(postResponseBody);



        var updatedUser = new
        {
            name = "Test1234",
            email = "",
            gender = "male",
            status = "active",
        };
        
        var putRequestBody = JsonConvert.SerializeObject(updatedUser);
        
        var putRequest = new StringContent(putRequestBody, System.Text.Encoding.UTF8, "application/json");
        
        var putResponse = await Client.PutAsync($"users/{userFromPostRequest.id}", putRequest);
        
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
        
        var putResponseBody = await putResponse.Content.ReadAsStringAsync();
        
        var errorResponse = JsonConvert.DeserializeObject<List<ErrorResponse>>(putResponseBody);
        
        errorResponse[0].field.Should().Be("email");
        errorResponse[0].message.Should().Be("can't be blank");
    }
    
    [Test]
    [Parallelizable]
    public async Task Put_UpdateUser_InvalidEmail_Returns_UnprocessableEntity()
    {
        var user = new
        {
            name = "2pac",
            email = $"2pac{Guid.NewGuid()}@testovi.com",
            gender = "male",
            status = "active",
        };
        
        var postRequestBody = JsonConvert.SerializeObject(user);
        
        var postRequest = new StringContent(postRequestBody, System.Text.Encoding.UTF8, "application/json");
        
        var postResponse = await Client.PostAsync("users", postRequest);

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var postResponseBody = await postResponse.Content.ReadAsStringAsync();
        
        var userFromPostRequest = JsonConvert.DeserializeObject<UserResponse>(postResponseBody);



        var updatedUser = new
        {
            name = "Test1234",
            email = "test@+",
            gender = "male",
            status = "active",
        };
        
        var putRequestBody = JsonConvert.SerializeObject(updatedUser);
        
        var putRequest = new StringContent(putRequestBody, System.Text.Encoding.UTF8, "application/json");
        
        var putResponse = await Client.PutAsync($"users/{userFromPostRequest.id}", putRequest);
        
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
        
        var putResponseBody = await putResponse.Content.ReadAsStringAsync();
        
        var errorResponse = JsonConvert.DeserializeObject<List<ErrorResponse>>(putResponseBody);
        
        errorResponse[0].field.Should().Be("email");
        errorResponse[0].message.Should().Be("is invalid");
    }

    [Test]
    [Parallelizable]
    public async Task Put_UpdateUser_EmptyName_Returns_UnprocessableEntity()
    {
        var user = new
        {
            name = "2pac",
            email = $"2pac{Guid.NewGuid()}@testovi.com",
            gender = "male",
            status = "active",
        };

        var postRequestBody = JsonConvert.SerializeObject(user);

        var postRequest = new StringContent(postRequestBody, System.Text.Encoding.UTF8, "application/json");

        var postResponse = await Client.PostAsync("users", postRequest);

        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var postResponseBody = await postResponse.Content.ReadAsStringAsync();

        var userFromPostRequest = JsonConvert.DeserializeObject<UserResponse>(postResponseBody);



        var updatedUser = new
        {
            name = "",
            email = "test@+",
            gender = "male",
            status = "active",
        };

        var putRequestBody = JsonConvert.SerializeObject(updatedUser);

        var putRequest = new StringContent(putRequestBody, System.Text.Encoding.UTF8, "application/json");

        var putResponse = await Client.PutAsync($"users/{userFromPostRequest.id}", putRequest);

        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);

        var putResponseBody = await putResponse.Content.ReadAsStringAsync();

        var errorResponse = JsonConvert.DeserializeObject<List<ErrorResponse>>(putResponseBody);

        errorResponse[0].field.Should().Be("name");
        errorResponse[0].message.Should().Be("can't be blank");
    }
}