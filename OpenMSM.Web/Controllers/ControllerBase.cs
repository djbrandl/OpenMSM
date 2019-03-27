using AutoMapper;
using OpenMSM.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenMSM.ServiceDefinitions;
using OpenMSM.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using OpenMSM.Web.Models;
using Microsoft.AspNetCore.Http;

namespace OpenMSM.Web.Controllers
{
    public class ControllerBase : Controller
    {
        private IHubContext<AdminHub> HubContext { get; }
        protected IHttpContextAccessor HttpContextAccessor { get; }
        protected IMapper Mapper { get; private set; }
        protected IList<ServiceBase> ServicesList { get; set; }

        public ControllerBase(IMapper mapper, IHubContext<AdminHub> hubContext, IHttpContextAccessor httpContextAccessor)
        {
            this.Mapper = mapper;
            this.ServicesList = new List<ServiceBase>();
            this.HubContext = hubContext;
            this.HttpContextAccessor = httpContextAccessor;
        }

        protected async Task SendAdminLog(LogApiMessage logMessage)
        {
            await HubContext.Clients.All.SendAsync(AdminHub.ActionOccurred, logMessage);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authHeader = context.HttpContext.Request.Headers.FirstOrDefault(m => m.Key == "Authorization");

            // checking null on the key as the KeyValuePair<T,T> is a struct and initialized with default <null, 0>
            if (!string.IsNullOrWhiteSpace(authHeader.Key))
            {
                foreach (var service in ServicesList)
                {
                    service.SetAccessToken(authHeader.Value.ToString().ToXmlElement().OuterXml);
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
