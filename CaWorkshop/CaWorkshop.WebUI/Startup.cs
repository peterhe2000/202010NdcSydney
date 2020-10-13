using System.Linq;
using CaWorkshop.Application.Common.Interfaces;
using CaWorkshop.Infrastructure;
using CaWorkshop.Infrastructure.Persistence;
using CaWorkshop.WebUI.Filters;
using CaWorkshop.WebUI.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace CaWorkshop.WebUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInfrastructureServices(Configuration);
            services.AddApplicationServices();

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddHttpContextAccessor(); // Asp.net identity 
  services.AddHealthChecksUI()
                .AddInMemoryStorage();

              services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>()
                .AddSmtpHealthCheck(options =>
                {
                    options.Host = "localhost";
                    options.Port = 25;
                });
            services.AddControllersWithViews(options =>
                options.Filters.Add(new ApiExceptionFilterAttribute()));

            services.AddRazorPages();

            services.AddOpenApiDocument(configure =>
            {
                configure.Title = "CaWorkshop API";
            });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddOpenApiDocument(configure =>
            {
                configure.Title = "CaWorkshop API";
                configure.AddSecurity("JWT", Enumerable.Empty<string>(),
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Type into the textbox: Bearer {your JWT token}."
                    });

                configure.OperationProcessors.Add(
                    new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });

                  }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            //app.UseOpenApi();
            //app.UseSwaggerUi3();
            app.UseSwaggerUi3(settings =>
            {
                settings.Path = "/api";
                settings.DocumentPath = "/api/specification.json";
            });
            // app.UseReDoc(); -- Useful public documentation (no try it now feature)
            // app.UseReDoc(settings => settings.Path = "/docs");
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.Map("/api", context => Task.Run(() => context.Response.Redirect("/docs")));
            //});

            app.UseRouting();          

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    //Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI();
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    //spa.UseAngularCliServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}
