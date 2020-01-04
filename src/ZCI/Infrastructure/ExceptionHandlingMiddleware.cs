using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using ZCI.Common.Exceptions;

namespace ZCI.Infrastructure
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly bool _isDevelopment;

        public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _isDevelopment = env.IsDevelopment();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex) when (!httpContext.Response.HasStarted)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var exInfo = GetExceptionInfo(ex);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)exInfo.code;

            var result = _isDevelopment ?
                JsonSerializer.Serialize(new { message = exInfo.message, exception = ex.ToString() })
                : JsonSerializer.Serialize(new { message = exInfo.message });

            return context.Response.WriteAsync(result);
        }

        private (HttpStatusCode code, string message) GetExceptionInfo(Exception exception)
        {
            if (exception is ValidationException)
                return (HttpStatusCode.BadRequest, exception.Message);

            if (exception is ExternalServiceException
                || exception is ExecutionRejectedException)
                return (HttpStatusCode.FailedDependency, exception.Message);

            return (HttpStatusCode.InternalServerError, "InternalServerError");
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
