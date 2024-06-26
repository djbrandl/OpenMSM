﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AutoMapper;
using OpenMSM.ServiceDefinitions;
using OpenMSM.Web.Models;
using OpenMSM.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenMSM.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace OpenMSM.Web.Controllers
{
    [Route("api/[controller]")]
    public class ChannelsController : ControllerBase
    {
        private XmlElement _accessToken { get; set; }
        private ChannelManagementService _channelManagementService { get; set; }
        private ProviderPublicationService _providerPublicationService { get; set; }
        private ConsumerPublicationService _consumerPublicationService { get; set; }
        private ProviderRequestService _providerRequestService { get; set; }
        private ConsumerRequestService _consumerRequestService { get; set; }

        public ChannelsController(ChannelManagementService channelManagementService, 
            ProviderPublicationService providerPublicationService, 
            ConsumerPublicationService consumerPublicationService, 
            ProviderRequestService providerRequestService, 
            ConsumerRequestService consumerRequestService, 
            IMapper mapper, 
            IHubContext<AdminHub> hubContext, 
            IHttpContextAccessor httpContextAccessor) : base(mapper, hubContext, httpContextAccessor)
        {
            this._channelManagementService = channelManagementService;
            this._providerPublicationService = providerPublicationService;
            this._consumerPublicationService = consumerPublicationService;
            this._providerRequestService = providerRequestService;
            this._consumerRequestService = consumerRequestService;
            this.ServicesList.Add(_channelManagementService);
            this.ServicesList.Add(_providerPublicationService);
            this.ServicesList.Add(_consumerPublicationService);
            this.ServicesList.Add(_providerRequestService);
            this.ServicesList.Add(_consumerRequestService);
        }

        #region Private Methods

        private IActionResult HandleChannelFault(ChannelFaultException e)
        {
            if (e.Message.IndexOf("Provided header security token") >= 0)
            {
                return Unauthorized(new { message = e.Message });
            }
            return NotFound(new { message = e.Message });
        }

        private IActionResult GenericOpenSession(Func<Session> action)
        {
            try
            {
                var session = action.Invoke();

                // Sending the link to the route for "CloseSession", but that requires a DELETE action to be taken.
                // This is set for semantic purposes for the "Location" header that is returned.
                return Created(new Uri(Url.Link("CloseSession", new { sessionId = session.Id })), session);
            }
            catch (ChannelFaultException e)
            {
                return HandleChannelFault(e);
            }
            catch (OperationFaultException e)
            {
                return UnprocessableEntity(new { message = e.Message });
            }
        }

        #endregion

        [HttpGet]
        public IEnumerable<OpenMSM.Web.Models.Channel> Get()
        {
            return _channelManagementService.GetChannels().Select(m => Mapper.Map<OpenMSM.Web.Models.Channel>(m));
        }

        [HttpGet("{channelUri}")]
        [ProducesResponseType(typeof(OpenMSM.Web.Models.Channel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string channelUri)
        {
            try
            {
                var channel = _channelManagementService.GetChannel(System.Net.WebUtility.UrlDecode(channelUri).Trim());
                return Ok(Mapper.Map<OpenMSM.Web.Models.Channel>(channel));
            }
            catch (ChannelFaultException e)
            {
                return HandleChannelFault(e);
            }
        }

        // TODO: Doing the return slightly differently - not returning the full channel object as that 
        // functionality doesn't match what the OpenMSM specifications would return and it is overly verbose
        // returning the same object that was passed in.
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Post([FromBody]OpenMSM.Web.Models.Channel channel)
        {
            if (channel == null)
            {
                return BadRequest(new { message = "Malformed channel object in HTTP body." });
            }
            try
            {
                var tokens = channel.SecurityTokens == null ? new XmlElement[0] : channel.SecurityTokens.Select(m => m.Token).Where(m => !string.IsNullOrWhiteSpace(m)).ToXmlElements();
                _channelManagementService.CreateChannel(channel.Uri.Trim(),
                    channel.Type == Models.ChannelType.Publication ? ServiceDefinitions.ChannelType.Publication : ServiceDefinitions.ChannelType.Request,
                    channel.Description,
                    tokens);
                return Created(string.Empty, null);
            }
            catch (ChannelFaultException e)
            {
                return HandleChannelFault(e);
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
                _channelManagementService.DeleteChannel(System.Net.WebUtility.UrlDecode(channelUri).Trim());
                return NoContent();
            }
            catch (ChannelFaultException e)
            {
                return HandleChannelFault(e);
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
                _channelManagementService.AddSecurityTokens(System.Net.WebUtility.UrlDecode(channelUri).Trim(), tokens);
                return Created(new Uri(Url.Link("AddSecurityTokens", new { channelUri })), null);
            }
            catch (ChannelFaultException e)
            {
                return HandleChannelFault(e);
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
                _channelManagementService.RemoveSecurityTokens(System.Net.WebUtility.UrlDecode(channelUri).Trim(), tokens);
                return NoContent();
            }
            catch (ChannelFaultException e)
            {
                return HandleChannelFault(e);
            }
        }

        [HttpPost("{channelUri}/publication-sessions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult OpenPublicationSession(string channelUri)
        {
            return GenericOpenSession(() =>
            {
                var sessionId = _providerPublicationService.OpenPublicationSession(channelUri.Trim());
                return new Session { Id = sessionId, Type = SessionType.PublicationProvider };
            });
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
            if (!session.Topics.Any())
            {
                return UnprocessableEntity(new { message = "There must be at least 1 topic for a session." });
            }
            if (session.XPathNamespaces == null)
            {
                session.XPathNamespaces = new XPathNamespace[0];
            }
            if (session.XPathNamespaces.Select(m => m.Prefix).Distinct().Count() < session.XPathNamespaces.Count())
            {
                return BadRequest(new { message = "Duplicate namespace prefixes provided." });
            }
            return GenericOpenSession(() =>
            {
                var sessionId = _consumerPublicationService
                    .OpenSubscriptionSession(channelUri.Trim(),
                        session.Topics,
                        session.ListenerUrl,
                        session.XPathExpression,
                        session.XPathNamespaces.Select(m => new Namespace { NamespaceName = m.Namespace, NamespacePrefix = m.Prefix }).ToArray());

                session.Id = sessionId;
                session.Type = SessionType.PublicationConsumer;
                return session;
            });
        }

        [HttpPost("{channelUri}/consumer-request-sessions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult OpenConsumerRequestSession(string channelUri, [FromBody]string ListenerURL)
        {
            return GenericOpenSession(() =>
            {
                var sessionId = _consumerRequestService.OpenConsumerRequestSession(channelUri.Trim(), ListenerURL);
                return new Session { Id = sessionId, Type = SessionType.RequestConsumer };
            });
        }

        [HttpPost("{channelUri}/provider-request-sessions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult OpenProviderRequestSession(string channelUri, [FromBody]Session session)
        {
            if (session == null)
            {
                return BadRequest(new { message = "Malformed session object in HTTP body." });
            }
            if (session.XPathNamespaces == null)
            {
                session.XPathNamespaces = new XPathNamespace[0];
            }
            if (session.XPathNamespaces.Select(m => m.Prefix).Distinct().Count() < session.XPathNamespaces.Count())
            {
                return BadRequest(new { message = "Duplicate namespace prefixes provided." });
            }
            return GenericOpenSession(() =>
            {

                var sessionId = _providerRequestService.OpenProviderRequestSession(
                        channelUri.Trim(),
                        session.Topics,
                        session.ListenerUrl,
                        session.XPathExpression,
                        session.XPathNamespaces.Select(m => new Namespace { NamespaceName = m.Namespace, NamespacePrefix = m.Prefix }).ToArray());

                session.Id = sessionId;
                session.Type = SessionType.RequestProvider;

                return session;
            });
        }
    }
}
