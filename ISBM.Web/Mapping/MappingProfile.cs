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

            CreateMap<ISBM.ServiceDefinitions.Channel, ISBM.Web.Models.Channel>()
                .ForMember(m => m.Description, opt => opt.MapFrom(m => m.ChannelDescription))
                .ForMember(m => m.Uri, opt => opt.MapFrom(m => m.ChannelURI))
                .ForMember(m => m.Type, opt => opt.MapFrom(m => m.ChannelType));

            CreateMap<ISBM.Data.Models.Channel, ISBM.Web.Models.Channel>().ForMember(m => m.Type, opt => opt.MapFrom(m => (ChannelType)m.Type));
        }
    }
}
