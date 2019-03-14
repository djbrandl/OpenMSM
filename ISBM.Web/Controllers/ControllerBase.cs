using AutoMapper;
using ISBM.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISBM.ServiceDefinitions;

namespace ISBM.Web.Controllers
{
    public class ControllerBase : Controller
    {
        protected DbContext dbContext { get; private set; }
        protected IMapper mapper { get; private set; }
        protected IList<ServiceBase> servicesList { get; set; }

        public ControllerBase(DbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.servicesList = new List<ServiceBase>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authHeader = context.HttpContext.Request.Headers.FirstOrDefault(m => m.Key == "Authorization");

            // checking null on the key as the KeyValuePair<T,T> is a struct and initialized with default <null, 0>
            if (!string.IsNullOrWhiteSpace(authHeader.Key))
            {
                foreach (var service in servicesList)
                {
                    service.SetAccessToken(authHeader.Value.ToString().ToXmlElement().OuterXml);
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
