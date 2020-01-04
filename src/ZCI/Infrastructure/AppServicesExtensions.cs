using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZCI.Domain;
using ZCI.Infrastructure.Policies;

namespace ZCI.Infrastructure
{
    public static class AppServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IZipInfoService, ZipInfoService>();
            services.AddSingleton<ILogger, FakeLogger>();
            services.AddSingleton<IReadOnlyPolicyRegistry<string>>(PollyPolicyRegistry.Create(configuration));
        }
    }
}
