using System.Net;
using dotNetAPITesting.POCOs;
using Newtonsoft.Json;

namespace dotNetAPITesting.Tests;

public class BasicTestGet
{
    private String _url = "https://gorest.co.in/public/v2/users";
    private HttpClient _httpClient;

    [SetUp]
    public void Setup()
    {
        _httpClient = new HttpClient();
    }


    [Test]
    [Category("GET")]
    public async Task GetAllUsers()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(_url);

        Assert.That(response.StatusCode.Equals(HttpStatusCode.OK), "Expected status code is 200 OK");
        Assert.That(response.Content, Is.Not.Null, "Response content is not null");
    }

    [Test]
    [Category("GET")]
    public async Task GetAllUsersResponse()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(_url);

        Assert.That(response.IsSuccessStatusCode, Is.True, "Expected status code is 200 OK");
        Assert.That(response.Content, Is.Not.Null, "Response content is not null");

        string responseData = await response.Content.ReadAsStringAsync();

        List<UserResponse> users = JsonConvert.DeserializeObject<List<UserResponse>>(responseData);

        Assert.That(users.Count, Is.Not.Zero, "Expected number of users returned is not 0");
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }
}