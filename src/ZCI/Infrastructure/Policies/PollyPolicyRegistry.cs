using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Registry;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZCI.Infrastructure.Policies
{
    public class PollyPolicyRegistry
    {
        public static PolicyRegistry Create(IConfiguration configuration)
        {
            return new PolicyRegistry()
            {
                { "zipCodeInfoExtApiPolicy", ZipCodeInfoExtApiPolicy.Create(configuration) }
            };
        }
    }
}
