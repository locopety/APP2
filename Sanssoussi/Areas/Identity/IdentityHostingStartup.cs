using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Sanssoussi.Areas.Identity;
using Sanssoussi.Areas.Identity.Data;
using Sanssoussi.Data;
using System;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]

namespace Sanssoussi.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(
                (context, services) =>
                {
                    services.AddDbContext<SanssoussiContext>(
                        options =>
                            options.UseSqlite(
                                context.Configuration.GetConnectionString("SanssoussiContextConnection")));

                    services.AddDefaultIdentity<SanssoussiUser>(options => {
                        options.Password.RequireDigit = true;
                        options.Password.RequireLowercase = true;
                        options.Password.RequireUppercase = true;
                        options.Password.RequireNonAlphanumeric = true;
                        options.Password.RequiredLength = 10;
                        options.SignIn.RequireConfirmedAccount = true;
                        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                        options.Lockout.MaxFailedAccessAttempts = 5;
                        options.Lockout.AllowedForNewUsers = true;
                    })
                    .AddEntityFrameworkStores<SanssoussiContext>();
                });
        }
    }
}