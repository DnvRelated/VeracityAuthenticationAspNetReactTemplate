using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Veracity.Common.Authentication;
using Veracity.Common.OAuth.Providers;

namespace VeracityAuthenticationAspNetReactTemplate.Extensions;

internal static class VeracityConfigurationExtensions
{
    public static void AddVeracityConfiguration(this WebApplicationBuilder builder, ConfigurationManager configuration)
    {
        builder.Services.AddVeracity(configuration)
            .Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            })
            .AddSingleton(ConstructDataProtector)
            .AddVeracityServices(configuration["Veracity:MyServicesApi"])
            .AddSingleton(ConstructDistributedCache)
            .AddSession()
            .AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddVeracityAuthentication(configuration)
            .AddCookie();
    }

    private static IDistributedCache ConstructDistributedCache(IServiceProvider s)
        => new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));

    private static Veracity.Common.Authentication.IDataProtector ConstructDataProtector(IServiceProvider s)
    {
        return new DataProtector<IDataProtectionProvider>(s.GetDataProtectionProvider(),
                    (p, data) => p.CreateProtector("token").Protect(data),
                    (p, data) => p.CreateProtector("token").Unprotect(data));
    }
}
