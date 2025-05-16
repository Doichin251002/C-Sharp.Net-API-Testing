using System.Net.Http.Headers;

namespace dotNetFinalProjectAPITesting;

public class HttpClientProvider
{
    public HttpClient Client { get; }

    public HttpClientProvider(ApiConfig apiConfig)
    {
        Client = new HttpClient()
        {
            BaseAddress = new Uri(apiConfig.BaseUrl)
        };
        
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiConfig.AuthToken);
    }
}