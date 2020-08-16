using DomainObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using READUS.Crypto;
using SourceControl.InMemory;
using Storage;
using System;
using System.Text;

namespace READUS
{
    public class Startup
    {

        const string SUPER_SECURE_KEY = "this is the key that I will use for development";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // if in dev
            services.AddSingleton<IDataContext, MemoryDataContext>(service => CreateMockdataContext());

            services.Configure<CryptoConfigs>(options => Configuration.GetSection("Crypto").Bind(options));
            services.Configure<JwtConfigs>(options => Configuration.GetSection("JWT").Bind(options));

            var jwtConfigs = new JwtConfigs();
            Configuration.GetSection("JWT").Bind(jwtConfigs);

            services.AddControllersWithViews();
            services.AddAuthentication(x => 
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;

                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfigs.SymmetricKey)),
                    ValidateIssuer = false, // false for development
                    ValidateAudience = false, // false for development
                    ClockSkew = TimeSpan.Zero
                };
            });

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });
        }

        private MemoryDataContext CreateMockdataContext()
        {
            var docs = new MemoryRepository<Document>();
            var repos = new MemoryRepository<Repository>();
            var orgs = new MemoryRepository<Organization>();
            var users = new MemoryRepository<User>();

            var newOrg = new Organization()
            {
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                Name = "Test Org",
            };
            orgs.Add(newOrg);

            var repoData = new MemoryMetadata()
            {
                HasPassword = true,
                Password = "Pa$$w0rd",
                RootDirectory = @"C:\Users\alija\Documents\Programming\Test%20Project"
            };

            var newRepo = new Repository()
            {
                Name = "Test Repo",
                OrganizationId = newOrg.Id,
                SCM = SupportedSystems.Memory,
                CustomRepositoryInformation = JsonConvert.SerializeObject(repoData)
            };

            repos.Add(newRepo);

            var newUser = new User()
            {
                Username = "agreen",
                Password = "password"
            };

            users.Add(newUser);

            return new MemoryDataContext(docs, orgs, repos, users);
        }
    }
}
