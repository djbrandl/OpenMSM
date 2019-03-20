using System;
using System.Collections.Generic;
using System.Linq;
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
        private ChannelManagementService _channelManagementService { get; set; }
        private ProviderPublicationService _providerPublicationService { get; set; }
        private ConsumerPublicationService _consumerPublicationService { get; set; }

        //public ChannelsController(AppDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        //{
        //    _channelManagementService = new ChannelManagementService(dbContext, mapper);
        //    _providerPublicationService = new ProviderPublicationService(dbContext, mapper);
        //    _consumerPublicationService = new ConsumerPublicationService(dbContext, mapper);
        //    this.servicesList.Add(_channelManagementService);
        //    this.servicesList.Add(_providerPublicationService);
        //    this.servicesList.Add(_consumerPublicationService);
        //}

        public ChannelsController(ChannelManagementService channelManagementService, ProviderPublicationService providerPublicationService, ConsumerPublicationService consumerPublicationService, IMapper mapper) : base(mapper)
        {
            this._channelManagementService = channelManagementService;
            this._providerPublicationService = providerPublicationService;
            this._consumerPublicationService = consumerPublicationService;
            this.ServicesList.Add(_channelManagementService);
            this.ServicesList.Add(_providerPublicationService);
            this.ServicesList.Add(_consumerPublicationService);
        }

        [HttpGet]
        public IEnumerable<ISBM.Web.Models.Channel> Get()
        {
            return _channelManagementService.GetChannels().Select(m => Mapper.Map<ISBM.Web.Models.Channel>(m));
        }

        [HttpGet("{channelUri}")]
        [ProducesResponseType(typeof(ISBM.Web.Models.Channel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string channelUri)
        {
            try
            {
                var channel = _channelManagementService.GetChannel(System.Net.WebUtility.UrlDecode(channelUri));
                return Ok(Mapper.Map<ISBM.Web.Models.Channel>(channel));
            }
            catch (ChannelFaultException e)
            {
                if (e.Message.IndexOf("Provided header security token") >= 0)
                {
                    return Unauthorized(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
        }

        // TODO: Doing the return slightly differently - not returning the full channel object as that 
        // functionality doesn't match what the ISBM specifications would return and it is overly verbose
        // returning the same object that was passed in.
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Post([FromBody]ISBM.Web.Models.Channel channel)
        {
            if (channel == null)
            {
                return BadRequest(new { message = "Malformed channel object in HTTP body." });
            }
            try
            {
                var tokens = channel.SecurityTokens == null ? new XmlElement[0] : channel.SecurityTokens.Select(m => m.Token).ToXmlElements();
                _channelManagementService.CreateChannel(channel.Uri,
                    channel.Type == Models.ChannelType.Publication ? ServiceDefinitions.ChannelType.Publication : ServiceDefinitions.ChannelType.Request,
                    channel.Description,
                    tokens);
                return Created(string.Empty, null);
            }
            catch (ChannelFaultException e)
            {
                if (e.Message.IndexOf("Provided header security token") >= 0)
                {
                    return Unauthorized(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
        }

        [HttpDelete("{channelUri}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string channelUri)
        {
            try
            {
                _channelManagementService.DeleteChannel(System.Net.WebUtility.UrlDecode(channelUri));
                return NoContent();
            }
            catch (ChannelFaultException e)
            {
                if (e.Message.IndexOf("Provided header security token") >= 0)
                {
                    return Unauthorized(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
        }

        [HttpPost("{channelUri}/security-tokens", Name = "AddSecurityTokens")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult AddSecurityTokens(string channelUri, [FromBody]SecurityToken[] securityTokens)
        {
            try
            {
                var tokens = securityTokens.Select(m => m.Token).ToXmlElements();
                _channelManagementService.AddSecurityTokens(System.Net.WebUtility.UrlDecode(channelUri), tokens);
                return Created(new Uri(Url.Link("AddSecurityTokens", new { channelUri })), null);
            }
            catch (ChannelFaultException e)
            {
                if (e.Message.IndexOf("Provided header security token") >= 0)
                {
                    return Unauthorized(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
        }

        [HttpDelete("{channelUri}/security-tokens")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RemoveSecurityTokens(string channelUri, [FromBody]SecurityToken[] securityTokens)
        {
            try
            {
                var tokens = securityTokens.Select(m => m.Token).ToXmlElements();
                _channelManagementService.RemoveSecurityTokens(System.Net.WebUtility.UrlDecode(channelUri), tokens);
                return NoContent();
            }
            catch (ChannelFaultException e)
            {
                if (e.Message.IndexOf("Provided header security token") >= 0)
                {
                    return Unauthorized(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
        }

        [HttpPost("{channelUri}/publication-sessions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult OpenPublicationSession(string channelUri)
        {
            try
            {
                var sessionId = _providerPublicationService.OpenPublicationSession(channelUri);
                return Created(string.Empty, new Session { Id = sessionId, Type = SessionType.PublicationProvider });
            }
            catch (ChannelFaultException e)
            {
                if (e.Message.IndexOf("Provided header security token") >= 0)
                {
                    return Unauthorized(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
            catch (OperationFaultException e)
            {
                return UnprocessableEntity(new { message = e.Message });
            }
        }


        [HttpPost("{channelUri}/subscription-sessions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult OpenSubscriptionSession(string channelUri, [FromBody]Session session)
        {
            if (session == null)
            {
                return BadRequest(new { message = "Malformed session object in HTTP body." });
            }
            try
            {
                if (session.XPathNamespaces == null)
                {
                    session.XPathNamespaces = new XPathNamespace[0];
                }

                if (session.XPathNamespaces.Select(m => m.Prefix).Distinct().Count() < session.XPathNamespaces.Count())
                {
                    return BadRequest(new { message = "Duplicate namespace prefixes provided." });
                }

                var sessionId = _consumerPublicationService
                    .OpenSubscriptionSession(channelUri,
                        session.Topics,
                        session.ListenerUrl,
                        session.XPathExpression,
                        session.XPathNamespaces.Select(m => new Namespace { NamespaceName = m.Namespace, NamespacePrefix = m.Prefix }).ToArray());

                session.Id = sessionId;
                session.Type = SessionType.PublicationConsumer;

                // Sending the link to the route for "ClosePublicationSession", but that requires a DELETE action to be taken.
                // This is set for semantic purposes for the "Location" header that is returned.
                return Created(new Uri(Url.Link("ClosePublicationSession", new { sessionId })), session);
            }
            catch (ChannelFaultException e)
            {
                if (e.Message.IndexOf("Provided header security token") >= 0)
                {
                    return Unauthorized(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
            catch (OperationFaultException e)
            {
                return UnprocessableEntity(new { message = e.Message });
            }
        }
    }
}
