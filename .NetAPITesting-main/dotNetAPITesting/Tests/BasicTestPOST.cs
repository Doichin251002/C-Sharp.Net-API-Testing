using System.Net.Http.Headers;
using System.Net.Mime;
using dotNetAPITesting.POCOs;
using Newtonsoft.Json;

namespace dotNetAPITesting.Tests;

[TestFixture]
public class BasicTestPOST
{
    private String _token = "7b4127fc405869b43de152f82a27b388046adac7d5c0727b6b8bbeeb15ccd4b6";
    private String _url = "https://gorest.co.in/";
    private HttpClient _httpClient;

    [SetUp]
    public void Setup()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_url);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    [Test]
    [Category("POST")]
    public async Task CreateNewUser()
    {
        var newUser = new UserRequest("teestt", "male", $"testoviii.{Guid.NewGuid()}@testi.com", "active");

        var requestBody = System.Text.Json.JsonSerializer.Serialize(newUser);

        var requestContent = new StringContent(requestBody, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json);

        HttpResponseMessage responseContent = await _httpClient.PostAsync("public/v2/users", requestContent);

        Assert.That(responseContent.StatusCode.Equals(System.Net.HttpStatusCode.Created), Is.True,
            "Expected status code is 201 Created");

        var responseBody = await responseContent.Content.ReadAsStringAsync();

        var userResponse = JsonConvert.DeserializeObject<UserResponse>(responseBody);

        Assert.That(userResponse.name, Is.EqualTo(newUser.name));
        Assert.That(userResponse.email, Is.EqualTo(newUser.email));
        Assert.That(userResponse.gender, Is.EqualTo(newUser.gender));
        Assert.That(userResponse.status, Is.EqualTo(newUser.status));
        Assert.That(userResponse.Id, Is.Not.Zero, "Expected user id returned");
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }
}