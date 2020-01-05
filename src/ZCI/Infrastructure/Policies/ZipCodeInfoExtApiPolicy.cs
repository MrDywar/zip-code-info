using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ZCI.Infrastructure.Policies
{
    public class ZipCodeInfoExtApiPolicy
    {
        public static AsyncPolicyWrap<HttpResponseMessage> Create(IConfiguration configuration)
        {
            var apiTimeout = configuration.GetValue("ExternalApiMillisecondsTimeout", 1000);
            var exceptionsCountToBreak = configuration.GetValue("ExternalApiExceptionsAllowedBeforeBreaking", 4);
            var breakDuration = configuration.GetValue("ExternalApiDurationOfBreakInSeconds", 30);

            var timeout = Policy
                .TimeoutAsync(
                    TimeSpan.FromMilliseconds(apiTimeout),
                    Polly.Timeout.TimeoutStrategy.Optimistic
                );

            var circuitBreaker = Policy<HttpResponseMessage>
                .Handle<Exception>() //TODO: ignore user input errors, or catch well known external server errors (403,500...).
                .CircuitBreakerAsync(
                    exceptionsCountToBreak,
                    TimeSpan.FromSeconds(breakDuration)
                );

            return circuitBreaker.WrapAsync(timeout);
        }
    }
}
