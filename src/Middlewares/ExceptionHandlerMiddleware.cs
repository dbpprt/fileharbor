using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fileharbor.Common;
using Fileharbor.Exceptions;
using Fileharbor.ViewModels.v1;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fileharbor.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ExceptionHandlerMiddleware(RequestDelegate next, IHostingEnvironment hostingEnvironment)
        {
            _next = next;
            _hostingEnvironment = hostingEnvironment;
        }

        private async Task WriteResponse(HttpContext httpContext, Exception e, IHostingEnvironment hostingEnvironment)
        {
            var model = new ExceptionResponse
            {
                Message = e.Message,
                StackTrace = hostingEnvironment.IsDevelopment() ? e.StackTrace : string.Empty,
                Type = e.GetType().Name
            };

            var result = JsonConvert.SerializeObject(
                model, 
                hostingEnvironment.IsDevelopment() ? Formatting.Indented : Formatting.None);

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = e is FileharborException exception ? exception.StatusCode : 500;
            await httpContext.Response.WriteAsync(result);
        }

        [UsedImplicitly]
        public async Task Invoke(HttpContext httpContext, ILogger<ExceptionHandlerMiddleware> logger, CurrentPrincipal principal, IHostingEnvironment hostingEnvironment)
        {
            try
            {
                await _next(httpContext);
            }
            catch (FileharborException e)
            {
                logger.LogError(e, "Unhandled FileharborException exception raised!");
                logger.LogError(e, "Exception: {0}", JsonConvert.SerializeObject(e, Formatting.Indented));
                logger.LogError(e, "CurrentPrincipal: {0}", JsonConvert.SerializeObject(principal, Formatting.Indented));

                await WriteResponse(httpContext, e, _hostingEnvironment);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Unhandled FileharborException exception raised!");
                logger.LogCritical(e, "Exception: {0}", JsonConvert.SerializeObject(e, Formatting.Indented));
                logger.LogCritical(e, "CurrentPrincipal: {0}", JsonConvert.SerializeObject(principal, Formatting.Indented));

                await WriteResponse(httpContext, e, _hostingEnvironment);
            }
        }
    }
}
