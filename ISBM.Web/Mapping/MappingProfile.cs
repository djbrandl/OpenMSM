using AutoMapper;
using ISBM.ServiceDefinitions;

namespace ISBM.Web.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ISBM.Data.Models.Channel, ISBM.ServiceDefinitions.Channel>()
                .IgnoreAllVirtual()
                .ForMember(m => m.ChannelDescription, opt => opt.MapFrom(m => m.Description))
                .ForMember(m => m.ChannelType, opt => opt.MapFrom(m => (ChannelType)m.Type))
                .ForMember(m => m.ChannelURI, opt => opt.MapFrom(m => m.URI));
        }
    }
}
