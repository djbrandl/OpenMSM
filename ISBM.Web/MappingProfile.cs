using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISBM.Web
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ISBM.Data.Models.Channel, ISBM.ServiceDefinitions.Channel>();
        }
    }
}
