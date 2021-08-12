using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using IdentityWithCookies.Server.DbContexts;
using IdentityWithCookies.Server.Entities;

namespace IdentityWithCookies.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<JoyDbContext>();

            services.AddIdentity<AccountEntity, IdentityRole>()
                .AddEntityFrameworkStores<JoyDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                // options.Password.RequiredUniqueChars = 1;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            services.AddControllers();

            services.Configure<CookiePolicyOptions>(options => {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDistributedMemoryCache();

            services.AddSession();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(async (options) =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    options.Events = new JwtBearerEvents();

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = await JwtToken.JwtIssuerAsync(),
                        ValidAudience = await JwtToken.JwtIssuerAsync(),
                        ValidateIssuerSigningKey = true,
                        ValidIssuers = new List<string>()
                        {
                            await JwtToken.JwtIssuerAsync()
                        },
                        ValidAudiences = new List<string>()
                        {
                            await JwtToken.JwtIssuerAsync()
                        },
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(await JwtToken.JwtSecretAsync()))
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStarted.Register(async () => await OnStartedAsync(app));

            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();

            app.UseBlazorFrameworkFiles();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseMiddleware<JwtInHeaderMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        async private Task OnStartedAsync(IApplicationBuilder app)
        {
            using (IServiceScope serviceScope = app.ApplicationServices.CreateScope())
            {
                JoyDbContext joyDbContext = serviceScope.ServiceProvider.GetService<JoyDbContext>();

                await joyDbContext.Database.EnsureCreatedAsync();
                await joyDbContext.SaveChangesAsync();

                RoleManager<IdentityRole> roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

                if (await roleManager.FindByNameAsync("Admin") is null) {
                    IdentityRole role = new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" };

                    await roleManager.CreateAsync(role);
                }

                if (await roleManager.FindByNameAsync("User") is null) {
                    IdentityRole role = new IdentityRole { Name = "User", NormalizedName = "USER" };

                    await roleManager.CreateAsync(role);
                }
            }
        }
    }

    public class JwtInHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtInHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string cookie = context.Request.Cookies["JwtBearer"];

            if (cookie is not null) {
                if (context.Request.Headers.ContainsKey("Authorization") is false) {
                    context.Request.Headers.Append("Authorization", "Bearer " + cookie);
                }
            }

            await _next.Invoke(context);
        }
    }
}
