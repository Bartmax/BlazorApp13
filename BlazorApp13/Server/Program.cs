using System;
using System.Threading.Tasks;
using BlazorApp13.Server.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace BlazorApp13.Server
{
    public class Program
    {
        private const string HOSTNAME = "https://localhost:44308";

        public async static Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();

            var dbcontext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            EnsureDatabaseCreated(dbcontext);

            var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            await CreateIfNotExistAndSeedDatabase(applicationManager);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


        private static void EnsureDatabaseCreated(ApplicationDbContext context) => context.Database.EnsureCreated();

        private async static Task CreateIfNotExistAndSeedDatabase(IOpenIddictApplicationManager applicationManager)
        {
            var clientId = "BlazorApp13";
            var app = await applicationManager.FindByClientIdAsync(clientId);
            if (app is null)
            {
                if (await applicationManager.FindByClientIdAsync(clientId) is null)
                {
                    var descriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = clientId,
                        DisplayName = "Blazor App 13",
                        RedirectUris = { new Uri($"{HOSTNAME}/authentication/login-callback") },
                        PostLogoutRedirectUris = { new Uri($"{HOSTNAME}/authentication/logout-callback") },
                        //ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Authorization,
                            OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.Endpoints.Logout,
                            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                            OpenIddictConstants.Permissions.Scopes.Email,
                            OpenIddictConstants.Permissions.Scopes.Profile,
                            OpenIddictConstants.Permissions.Scopes.Roles,
                            OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                        }
                    };

                    await applicationManager.CreateAsync(descriptor);
                }
            }
        }
    }
}
