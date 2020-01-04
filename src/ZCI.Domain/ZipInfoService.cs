using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Registry;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ZCI.Common.DTO;
using ZCI.Common.Exceptions;
using ZCI.Domain.Models.Google;
using ZCI.Domain.Models.Openweather;

namespace ZCI.Domain
{
    public class ZipInfoService : IZipInfoService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;

        private readonly AsyncPolicyWrap<HttpResponseMessage> _externalApiPolicy;
        private readonly JsonSerializerOptions _jsonSerOps = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true
        };

        public ZipInfoService(
            IConfiguration configuration,
            IHttpClientFactory clientFactory,
            IReadOnlyPolicyRegistry<string> registry,
            ILogger logger)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
            _logger = logger;

            _externalApiPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>("defaultExternalApiPolicy");
        }

        public async Task<ZipCodeInfoDTO> GetZipCodeInfoAsync(string zipCode)
        {
            var city = await GetCityInfo(zipCode);
            var timeZone = await GetTimeZoneInfo(city.Coord.Lat, city.Coord.Lon, DateTimeOffset.UtcNow);

            return new ZipCodeInfoDTO()
            {
                CityName = city.Name,
                Temperature = city.Main.Temp,
                TemperatureUnits = city.Main.TempUnits,
                TimeZone = timeZone.TimeZoneName
            };
        }

        private async Task<OpenweatherCurrent> GetCityInfo(string zipCode)
        {
            // TODO: add zip code validation (wiki -> valid formats)
            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ValidationException(nameof(zipCode));

            var queryString = QueryHelpers.AddQueryString(
                _configuration["OpenweatherUrl"],
                new Dictionary<string, string>()
                {
                    {"zip", $"{zipCode},us" },
                    {"APPID", _configuration["OpenweatherApiKey"] }
                });

            var response = await GetResponse(queryString);
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("");
                throw new ExternalServiceException("Openweather");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OpenweatherCurrent>(responseBody, _jsonSerOps);
            result.Main.TempUnits = "Kelvin";

            return result;
        }

        private async Task<GoogleTimeZone> GetTimeZoneInfo(double lat, double lon, DateTimeOffset dateTime)
        {
            var queryString = QueryHelpers.AddQueryString(
                _configuration["GoogleTimeZoneUrl"],
                new Dictionary<string, string>()
                {
                    {"location", $"{lat.ToString(CultureInfo.InvariantCulture)},{lon.ToString(CultureInfo.InvariantCulture)}" },
                    {"timestamp", dateTime.ToUnixTimeSeconds().ToString() },
                    {"key", _configuration["GoogleApiKey"] }
                });

            var response = await GetResponse(queryString);
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("");
                throw new ExternalServiceException("Google");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GoogleTimeZone>(responseBody, _jsonSerOps);
        }

        private async Task<HttpResponseMessage> GetResponse(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var client = _clientFactory.CreateClient();

            return await _externalApiPolicy.ExecuteAsync(async ct => await client.SendAsync(request, ct), CancellationToken.None);
        }
    }
}
