using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OpenMSM.Data;
using OpenMSM.Data.Models;
using OpenMSM.Web.Hubs;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMSM.Web.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly AppDbContext _appDbContext;
        private readonly IHubContext<AdminHub> _hubContext;

        public RequestResponseLoggingMiddleware(RequestDelegate next,
                                                ILoggerFactory loggerFactory,
                                                IHubContext<AdminHub> hubContext,
                                                AppDbContext appDbContext)
        {
            _next = next;
            _logger = loggerFactory
                      .CreateLogger<RequestResponseLoggingMiddleware>();
            _hubContext = hubContext;
            _appDbContext = appDbContext;
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
                if(_appDbContext.Configuration != null && _appDbContext.Configuration.StoreLogMessages)
                {
                    var messageCount = _appDbContext.LogApiMessages.Count();
                    if(_appDbContext.Configuration.NumberOfMessagesToStore >= 0 && messageCount >= _appDbContext.Configuration.NumberOfMessagesToStore)
                    {
                        _appDbContext.LogApiMessages.Remove(_appDbContext.LogApiMessages.OrderByDescending(m => m.RespondedOn).FirstOrDefault());
                    }
                    _appDbContext.LogApiMessages.Add(message);
                    _appDbContext.SaveChanges();
                }
                await responseStream.CopyToAsync(originalBodyStream);
            }
        }
    }
}
