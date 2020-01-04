using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ZCI.Common.DTO;
using ZCI.Common.Exceptions;

namespace ZCI.Domain.Test
{
    [TestClass]
    public class ZipInfoServiceTests
    {
        private readonly string OpenweatherUrl = "https://api.openweather.test";
        private readonly string OpenweatherApiKey = "OPENWEATHER_KEY";
        private readonly string GoogleTimeZoneUrl = "https://api.google.test";
        private readonly string GoogleApiKey = "GOOGLE_KEY";
        private readonly int ExternalApiMillisecondsTimeout = 100;

        [TestMethod]
        [DataRow("123")]
        [DataRow("123456")]
        public async Task GetZipCodeInfoAsync_ValidZipCode_ReturnsZipCodeInfo(string zipCode)
        {
            var httpFactory = CreateHttpClientFactory(async (req, ct) =>
            {
                if (IsValidOpenweatherRequest(req, zipCode))
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(GetOpenweatherSampleJson())
                    };

                if (IsValidGoogleTimeZoneRequest(req, "37.39,-122.08"))
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(GetTimeZoneSampleJson())
                    };

                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            });

            var expectedResult = new ZipCodeInfoDTO()
            {
                CityName = "Mountain View",
                Temperature = 282.55D,
                TemperatureUnits = "Kelvin",
                TimeZone = "Eastern Daylight Time"
            };

            var service = new ZipInfoService(CreateConfiguration(), httpFactory, CreatePolicyRegistry(), CreateLogger());

            //Act
            var result = await service.GetZipCodeInfoAsync(zipCode);

            Assert.AreEqual(JsonSerializer.Serialize(result), JsonSerializer.Serialize(expectedResult));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public async Task GetZipCodeInfoAsync_InvalidZipCode_ThrowValidationException(string zipCode)
        {
            var httpFactory = CreateHttpClientFactory(async (req, ct) =>
            {
                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            });

            var service = new ZipInfoService(CreateConfiguration(), httpFactory, CreatePolicyRegistry(), CreateLogger());

            //Act
            Func<Task<ZipCodeInfoDTO>> act = () => service.GetZipCodeInfoAsync(zipCode);

            await Assert.ThrowsExceptionAsync<ValidationException>(act);
        }

        [TestMethod]
        [DataRow("openweather", HttpStatusCode.BadRequest)]
        [DataRow("openweather", HttpStatusCode.InternalServerError)]
        [DataRow("google", HttpStatusCode.BadRequest)]
        [DataRow("google", HttpStatusCode.InternalServerError)]
        public async Task GetZipCodeInfoAsync_ExternalServiceError_ThrowExternalServiceException(
            string serviceName,
            HttpStatusCode statusCode)
        {
            var zipCode = "123";
            var httpFactory = CreateHttpClientFactory(async (req, ct) =>
            {
                if (serviceName == "openweather")
                    return new HttpResponseMessage(statusCode);

                if (IsValidOpenweatherRequest(req, zipCode))
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(GetOpenweatherSampleJson())
                    };

                if (serviceName == "google")
                    return new HttpResponseMessage(statusCode);

                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            });

            var service = new ZipInfoService(CreateConfiguration(), httpFactory, CreatePolicyRegistry(), CreateLogger());

            //Act
            Func<Task<ZipCodeInfoDTO>> act = () => service.GetZipCodeInfoAsync(zipCode);

            await Assert.ThrowsExceptionAsync<ExternalServiceException>(act);
        }

        private IConfiguration CreateConfiguration()
        {
            var config = new Mock<IConfiguration>();

            config.Setup(x => x[nameof(OpenweatherUrl)]).Returns(OpenweatherUrl);
            config.Setup(x => x[nameof(OpenweatherApiKey)]).Returns(OpenweatherApiKey);
            config.Setup(x => x[nameof(GoogleTimeZoneUrl)]).Returns(GoogleTimeZoneUrl);
            config.Setup(x => x[nameof(GoogleApiKey)]).Returns(GoogleApiKey);

            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(a => a.Value).Returns(ExternalApiMillisecondsTimeout.ToString());

            config.Setup(c => c.GetSection(nameof(ExternalApiMillisecondsTimeout))).Returns(configSection.Object);

            return config.Object;
        }

        private IHttpClientFactory CreateHttpClientFactory(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            var client = new HttpClient(new MockMessageHandler((req, ct) => handler(req, ct)));

            var httpFactory = new Mock<IHttpClientFactory>();
            httpFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            return httpFactory.Object;
        }

        private IReadOnlyPolicyRegistry<string> CreatePolicyRegistry()
        {
            var result = new PolicyRegistry
            {
                { "defaultExternalApiPolicy", Policy.NoOpAsync().WrapAsync(Policy.NoOpAsync<HttpResponseMessage>()) }
            };

            return result;
        }

        private ILogger CreateLogger()
        {
            return new Mock<ILogger>().Object;
        }

        private class MockMessageHandler : HttpMessageHandler
        {
            private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

            public MockMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
            {
                _handler = handler;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _handler(request, cancellationToken);
            }
        }

        private bool IsValidOpenweatherRequest(HttpRequestMessage req, string zipCode)
        {
            return req.Method == HttpMethod.Get
                    && $"{req.RequestUri.Scheme}://{req.RequestUri.Host}" == OpenweatherUrl
                    && req.RequestUri.Query.Contains($"zip={zipCode}")
                    && req.RequestUri.Query.Contains($"APPID={OpenweatherApiKey}");
        }

        private bool IsValidGoogleTimeZoneRequest(HttpRequestMessage req, string locationValue)
        {
            return req.Method == HttpMethod.Get
                    && $"{req.RequestUri.Scheme}://{req.RequestUri.Host}" == GoogleTimeZoneUrl
                    && req.RequestUri.Query.Contains($"location={locationValue}")
                    && req.RequestUri.Query.Contains($"key={GoogleApiKey}")
                    && req.RequestUri.Query.Contains("timestamp=");
        }

        private string GetOpenweatherSampleJson()
        {
            return @"{
              'coord': {'lon': -122.08,'lat': 37.39},
              'weather': [
                {
                  'id': 800,
                  'main': 'Clear',
                  'description': 'clear sky',
                  'icon': '01d'
                }
              ],
              'base': 'stations',
              'main': {
                'temp': 282.55,
                'feels_like': 281.86,
                'temp_min': 280.37,
                'temp_max': 284.26,
                'pressure': 1023,
                'humidity': 100
              },
              'visibility': 16093,
              'wind': {
                'speed': 1.5,
                'deg': 350
              },
              'clouds': {
                'all': 1
              },
              'dt': 1560350645,
              'sys': {
                'type': 1,
                'id': 5122,
                'message': 0.0139,
                'country': 'US',
                'sunrise': 1560343627,
                'sunset': 1560396563
              },
              'timezone': -25200,
              'id': 420006353,
              'name': 'Mountain View',
              'cod': 200
            }".Replace("'", "\"");
        }

        private string GetTimeZoneSampleJson()
        {
            return @"{
               'dstOffset' : 3600,
               'rawOffset' : -18000,
               'status' : 'OK',
               'timeZoneId' : 'America/New_York',
               'timeZoneName' : 'Eastern Daylight Time'
            }".Replace("'", "\"");
        }
    }
}
