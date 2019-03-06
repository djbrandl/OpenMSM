using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISBM.Web.Controllers
{
    public class ControllerBase : Controller
    {
        protected DbContext dbContext { get; private set; }
        protected IMapper mapper { get; private set; }
        public ControllerBase(DbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }
    }
}
