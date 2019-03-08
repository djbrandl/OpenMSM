using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using AutoMapper;
using ISBM.Data;
using ISBM.ServiceDefinitions;
using ISBM.Web.Models;
using ISBM.Web.Services;
using Microsoft.AspNetCore.Http;
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
        [ProducesResponseType(typeof(ISBM.Web.Models.Channel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string channelUri)
        {
            try
            {
                var channel = _service.GetChannel(System.Net.WebUtility.UrlDecode(channelUri));
                return Ok(mapper.Map<ISBM.Web.Models.Channel>(channel));
            }
            catch (ChannelFaultException e)
            {
                return NotFound(new { message = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Post([FromBody]ISBM.Web.Models.Channel channel)
        {
            try
            {
                var tokens = channel.SecurityTokens == null ? new XmlElement[0] : channel.SecurityTokens.Select(m => m.Token).ToXmlElements();
                _service.CreateChannel(channel.Uri, channel.Type, channel.Description, tokens);
                return Created(string.Empty, null);
            }
            catch (ChannelFaultException e)
            {
                return NotFound(new { message = e.Message });
            }
        }

        [HttpDelete("{channelUri}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string channelUri)
        {
            try
            {
                _service.DeleteChannel(System.Net.WebUtility.UrlDecode(channelUri));
                return NoContent();
            }
            catch (ChannelFaultException e)
            {
                return NotFound(new { message = e.Message });
            }
        }

        [HttpPost("{channelUri}/security-tokens")]

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult AddSecurityTokens(string channelUri, [FromBody]SecurityToken[] securityTokens)
        {
            try
            {
                var tokens = securityTokens.Select(m => m.Token).ToXmlElements();
                _service.AddSecurityTokens(System.Net.WebUtility.UrlDecode(channelUri), tokens);
                return Created(string.Empty, null);
            }
            catch (ChannelFaultException e)
            {
                return NotFound(new { message = e.Message });
            }
        }

        [HttpDelete("{channelUri}/security-tokens")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RemoveSecurityTokens(string channelUri, [FromBody]SecurityToken[] securityTokens)
        {
            try
            {
                var tokens = securityTokens.Select(m => m.Token).ToXmlElements();
                _service.RemoveSecurityTokens(System.Net.WebUtility.UrlDecode(channelUri), tokens);
                return NoContent();
            }
            catch (ChannelFaultException e)
            {
                return NotFound(new { message = e.Message });
            }
        }
    }
}
