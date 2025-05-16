using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dotNetFinalProjectAPITesting;

public class TestBase
{
    protected HttpClient Client;
    private ServiceProvider _serviceProvider;

    [OneTimeSetUp]
    public void GlobalSetup()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("config.json").Build();
        
        var services = new ServiceCollection();
        services.RegisterServices(configuration);
        _serviceProvider = services.BuildServiceProvider();
        
        var httpClientProvider = _serviceProvider.GetRequiredService<HttpClientProvider>();
        Client = httpClientProvider.Client;
    }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        if (Client != null)
        {
            _serviceProvider.Dispose();
        }
    }
    
}