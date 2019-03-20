using AutoMapper;
using OpenMSM.ServiceDefinitions;

namespace OpenMSM.Web.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<OpenMSM.Data.Models.Channel, OpenMSM.ServiceDefinitions.Channel>()
                .IgnoreAllVirtual()
                .ForMember(m => m.ChannelDescription, opt => opt.MapFrom(m => m.Description))
                .ForMember(m => m.ChannelType, opt => opt.MapFrom(m => (ChannelType)m.Type))
                .ForMember(m => m.ChannelURI, opt => opt.MapFrom(m => m.URI));

            CreateMap<OpenMSM.ServiceDefinitions.Channel, OpenMSM.Web.Models.Channel>()
                .ForMember(m => m.Description, opt => opt.MapFrom(m => m.ChannelDescription))
                .ForMember(m => m.Uri, opt => opt.MapFrom(m => m.ChannelURI))
                .ForMember(m => m.Type, opt => opt.MapFrom(m => m.ChannelType));

            CreateMap<OpenMSM.Data.Models.Channel, OpenMSM.Web.Models.Channel>().ForMember(m => m.Type, opt => opt.MapFrom(m => (ChannelType)m.Type));
        }
    }
}
