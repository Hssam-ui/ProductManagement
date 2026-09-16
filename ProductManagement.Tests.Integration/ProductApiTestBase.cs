using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductManagement.Api.Data;
using Testcontainers.PostgreSql;

namespace ProductManagement.Tests.Integration
{
    public class TestServerIntegratedAuth : IServerIntegratedAuth
    {
        public bool IsEnabled => true;
        public string AuthenticationScheme => "TestScheme";
    }

    [SetUpFixture]
    public class ProductApiTestBase
    {
        protected static PostgreSqlContainer _dbContainer;
        protected static WebApplicationFactory<Program> _factory;
        protected static HttpClient _client;

        [OneTimeSetUp]
        public async Task GlobalSetup()
        {
            _dbContainer = new PostgreSqlBuilder().WithDatabase("test_db")
                .WithUsername("test_admin")
                .WithPassword("testing_password")
                .WithCleanUp(true)
                .Build();

            await _dbContainer.StartAsync();
            _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.UseSetting("ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString());
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IServerIntegratedAuth, TestServerIntegratedAuth>();
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestScheme";
                        options.DefaultChallengeScheme = "TestScheme";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
                });
            }
            );
            _client = _factory.CreateClient();

            using (var scope = _factory.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<AppDbContext>();
                // run migration of the db
                await context.Database.MigrateAsync();
            }
        }

        [OneTimeTearDown]
        public async Task GlobalTearDown()
        {
            try
            {
                _client.Dispose();

                await _factory.DisposeAsync();

                if (_dbContainer != null)
                {
                    await _dbContainer.StopAsync();
                    await _dbContainer.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while disposing resources:{ex.Message}");
                
            }
            

        }

    }
}
