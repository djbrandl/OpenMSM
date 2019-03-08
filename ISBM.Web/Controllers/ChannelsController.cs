using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using AutoMapper;
using ISBM.Data;
using ISBM.Web.Models;
using ISBM.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ISBM.Web.Controllers
{
    [Route("api/[controller]")]
    public class ChannelsController : ControllerBase
    {
        private XmlElement _accessToken { get; set; }
        private ChannelManagementService _service { get; set; }

        public ChannelsController(AppDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
            _service = new ChannelManagementService(dbContext, mapper);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authHeader = context.HttpContext.Request.Headers.FirstOrDefault(m => m.Key == "Authorization");

            // checking null on the key as the KeyValuePair<T,T> is a struct and initialized with default <null, 0>
            if (!string.IsNullOrWhiteSpace(authHeader.Key))
            {
                _service.SetAccessToken(authHeader.Value.ToString().ToXmlElement().OuterXml);
            }
            
            base.OnActionExecuting(context);
        }

        [HttpGet]
        public IEnumerable<ISBM.Web.Models.Channel> Get()
        {
            return _service.GetChannels().Select(m => mapper.Map<ISBM.Web.Models.Channel>(m));
        }

        [HttpGet("{channelUri}")]
        public ISBM.Web.Models.Channel Get(string channelUri)
        {
            var channel = _service.GetChannel(channelUri);
            return mapper.Map<ISBM.Web.Models.Channel>(channel);
        }

        [HttpPost]
        public void Post([FromBody]ISBM.Web.Models.Channel channel)
        {
            var tokens = channel.SecurityTokens == null ? new XmlElement[0] : channel.SecurityTokens.Select(m => m.Token).ToXmlElements(); 
            _service.CreateChannel(channel.Uri, channel.Type, channel.Description, tokens);
        }

        [HttpDelete("{channelUri}")]
        public void Delete(string channelUri)
        {
            _service.DeleteChannel(channelUri);
        }

        [HttpPost("{channelUri}/security-tokens")]
        public void AddSecurityTokens(string channelUri, [FromBody]SecurityToken[] securityTokens)
        {
            var tokens = securityTokens.Select(m => m.Token).ToXmlElements();
            _service.AddSecurityTokens(channelUri, tokens);            
        }

        [HttpDelete("{channelUri}/security-tokens")]
        public void DeleteSecurityTokens(string channelUri, [FromBody]SecurityToken[] securityTokens)
        {
            var tokens = securityTokens.Select(m => m.Token).ToXmlElements();
            _service.RemoveSecurityTokens(channelUri, tokens);
        }
    }
}
