#if (OrganizationalAuth || IndividualB2CAuth || IndividualLocalAuth)
using Microsoft.AspNetCore.Authentication;
#endif
using Microsoft.AspNetCore.Builder;
#if (IndividualLocalAuth)
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
#endif
#if (RequiresHttps)
using Microsoft.AspNetCore.HttpsPolicy;
#endif
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
#if (IndividualLocalAuth)
using Microsoft.EntityFrameworkCore;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
#if (IndividualLocalAuth)
using BlazorWasm_CSharp.Server.Data;
using BlazorWasm_CSharp.Server.Models;
#endif

namespace BlazorWasm_CSharp.Server
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
#if (IndividualLocalAuth)
            services.AddDbContext<ApplicationDbContext>(options =>
    #if (UseLocalDB)
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
    #else
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));
    #endif

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

            services.AddAuthentication()
                .AddIdentityServerJwt();
#endif
#if (OrganizationalAuth)
            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
#elif (IndividualB2CAuth)
            services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
                .AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options));
#endif

            services.AddControllersWithViews();
#if (IndividualLocalAuth)
            services.AddRazorPages();
#endif

            services.AddBlazorStaticFilesConfiguration();

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
#if (IndividualLocalAuth)
                app.UseDatabaseErrorPage();
#endif
                app.UseBlazorDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
#if (RequiresHttps)
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
#else
            }

#endif
            app.UseStaticFiles();

            app.UseRouting();

#if (OrganizationalAuth || IndividualAuth)
            app.UseAuthentication();
#endif
#if (IndividualLocalAuth)
            app.UseIdentityServer();
#endif
#if (!NoAuth)
            app.UseAuthorization();

#endif
            app.UseEndpoints(endpoints =>
            {
#if (IndividualLocalAuth)
                endpoints.MapRazorPages();
#endif
                endpoints.MapControllers();

                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
