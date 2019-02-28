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
        private IDictionary<string, string> _header { get; set; }
        public ServiceBase(DbContext dbContext, IMapper mapper)
        {
            this.mapper = mapper;
            this.appDbContext = dbContext;
            _header = new Dictionary<string, string>();
        }

        public void SetHeaderInformation(IDictionary<string, string> header)
        {
            this._header = header;
        }

        protected string GetHeaderSecurityToken()
        {
            return _header.ContainsKey("Security") ? _header["Security"] : string.Empty;
        }
    }
}
