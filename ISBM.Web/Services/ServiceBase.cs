using AutoMapper;
using ISBM.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ISBM.Web.Services
{
    public abstract class ServiceBase
    {
        protected readonly IMapper mapper;
        protected readonly DbContext appDbContext;
        private string _accessToken { get; set; }
        public ServiceBase(DbContext dbContext, IMapper mapper)
        {
            this.mapper = mapper;
            this.appDbContext = dbContext;
        }

        public void SetAccessToken(string token)
        {
            this._accessToken = token;
        }

        protected string GetAccessToken()
        {
            return _accessToken;
        }
    }
}
