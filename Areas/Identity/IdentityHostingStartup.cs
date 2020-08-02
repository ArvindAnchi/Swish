using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swish.Areas.Identity.Data;
using Swish.Data;

[assembly: HostingStartup(typeof(Swish.Areas.Identity.IdentityHostingStartup))]
namespace Swish.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<SwishDBContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("SwishDBContextConnection")));

                services.AddDefaultIdentity<SwishUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<SwishDBContext>();
            });
        }
    }
}