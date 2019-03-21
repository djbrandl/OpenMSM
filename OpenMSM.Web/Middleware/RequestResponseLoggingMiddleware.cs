using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OpenMSM.Web.Hubs;
using OpenMSM.Web.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OpenMSM.Web.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IHubContext<AdminHub> _hubContext;

        public RequestResponseLoggingMiddleware(RequestDelegate next,
                                                ILoggerFactory loggerFactory,
                                                IHubContext<AdminHub> hubContext)
        {
            _next = next;
            _logger = loggerFactory
                      .CreateLogger<RequestResponseLoggingMiddleware>();
            _hubContext = hubContext;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments(new PathString("/api")))
            {
                await _next(context);
                return;
            }
            
            var originalBodyStream = context.Response.Body;

            // create a new request stream
            using (var requestStream = new MemoryStream())

            // and a new response stream
            using (var responseStream = new MemoryStream())
            {
                // handle request
                // evaluate and copy the request body our new memory stream
                await context.Request.Body.CopyToAsync(requestStream);
                
                // set the stream position to 0
                requestStream.Seek(0, SeekOrigin.Begin);

                // read the contents of our memory stream
                var body = await new StreamReader(requestStream).ReadToEndAsync();
                var content = Encoding.UTF8.GetBytes(body);

                // reset the stream position to 0 so that the next method can evaluate it 
                requestStream.Seek(0, SeekOrigin.Begin);

                // set the request body stream to our memory stream with the evaluated body at position 0
                context.Request.Body = requestStream;

                var message = new LogApiMessage
                {
                    RequestIP = context.Connection.RemoteIpAddress.ToString(),
                    RequestMethod = context.Request.Method,
                    RequestURL = context.Request.Path.ToString(),
                    RequestBody = body
                };

                // handle response
                context.Response.Body = responseStream;

                await _next(context);

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                message.ResponseStatus = context.Response.StatusCode;
                message.ResponseBody = text;
                message.RespondedOn = DateTime.UtcNow;
                await _hubContext.Clients.All.SendAsync(AdminHub.ActionOccurred, message);
                await responseStream.CopyToAsync(originalBodyStream);
            }
        }

        private async Task FormatResponse(HttpResponse response, LogApiMessage logApiMessage)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            logApiMessage.ResponseStatus = response.StatusCode;
            logApiMessage.ResponseBody = text;
        }

        private async Task<string> FormatRequestToString(HttpRequest request)
        {
            var body = request.Body;
            request.EnableRewind();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body = body;

            return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
        }

        private async Task<string> FormatResponseToString(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return $"Response {text}";
        }
    }
}
