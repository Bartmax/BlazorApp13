using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlazorApp13.Server.Data;
using BlazorApp13.Server.Models;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OpenIddict.Validation.AspNetCore;

namespace BlazorApp13.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
            });

            services.AddAuthentication();
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                opt.DefaultForbidScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });

            services.AddOpenIddict(config =>
            {
                config.AddCore(options =>
                {
                    options.UseEntityFrameworkCore(config => { config.UseDbContext<ApplicationDbContext>(); });
                })
                .AddServer(options =>
                {
                    options.AddDevelopmentEncryptionCertificate();
                    options.AddDevelopmentSigningCertificate();

                    options.AllowAuthorizationCodeFlow();
                    options.AllowRefreshTokenFlow();

                    options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles);

                    options.SetTokenEndpointUris("/connect/token");
                    options.SetAuthorizationEndpointUris("/connect/authorize");
                    options.SetUserinfoEndpointUris("/connect/userinfo");
                    options.SetLogoutEndpointUris("/connect/logout");

                    options.UseAspNetCore(options =>
                    {
                        options.EnableAuthorizationEndpointPassthrough();
                        options.EnableLogoutEndpointPassthrough();
                        options.EnableTokenEndpointPassthrough();
                        options.EnableStatusCodePagesIntegration();

                    });
                })
                .AddValidation(options =>
                {
                    options.UseAspNetCore();
                    options.UseLocalServer();

                });
            });

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
