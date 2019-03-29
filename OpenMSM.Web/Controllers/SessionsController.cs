using System;
using System.Linq;
using System.Xml;
using AutoMapper;
using OpenMSM.Data;
using OpenMSM.ServiceDefinitions;
using OpenMSM.Web.Models;
using OpenMSM.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OpenMSM.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private ProviderPublicationService _providerPublicationService { get; set; }
        private ConsumerPublicationService _consumerPublicationService { get; set; }
        private ProviderRequestService _providerRequestService { get; set; }
        private ConsumerRequestService _consumerRequestService { get; set; }
        private NotificationService _notificationService { get; set; }
        private AppDbContext _dbContext { get; set; }

        public SessionsController(ProviderPublicationService providerPublicationService, 
            ConsumerPublicationService consumerPublicationService, 
            ProviderRequestService providerRequestService, 
            ConsumerRequestService consumerRequestService, 
            NotificationService notificationService, 
            AppDbContext dbContext, 
            IMapper mapper) : base(mapper)
        {
            this._dbContext = dbContext;
            this._providerPublicationService = providerPublicationService;
            this._consumerPublicationService = consumerPublicationService;
            this._providerRequestService = providerRequestService;
            this._consumerRequestService = consumerRequestService;
            this._notificationService = notificationService;
            this.ServicesList.Add(_providerPublicationService);
            this.ServicesList.Add(_consumerPublicationService);
            this.ServicesList.Add(_providerRequestService);
            this.ServicesList.Add(_consumerRequestService);
        }

        #region Private Methods

        private IActionResult HandleSessionFault(SessionFaultException e)
        {
            if (e.Message.IndexOf("Provided header security token") >= 0)
            {
                return Unauthorized(new { message = e.Message });
            }
            if (e.Message.IndexOf("is not of the correct type") >= 0)
            {
                return UnprocessableEntity(new { message = e.Message });
            }
            return NotFound(new { message = e.Message });
        }

        #endregion

        // TODO: Doing this differently where the entire message topic + expiry is in the POST body, not in the URL
        // Why are the message topics and duration split in the documentation between the URL and the body?
        [HttpPost("{sessionId}/publications")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult PostPublication(string sessionId, [FromBody]Message message)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            if (message == null)
            {
                return BadRequest(new { message = "Malformed message object in HTTP body." });
            }
            if (!message.Topics.Any())
            {
                return UnprocessableEntity(new { message = "There must be at least 1 topic for a publication message." });
            }
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(message.Content);
                var messageId = _providerPublicationService.PostPublication(sessionId, doc.DocumentElement, message.Topics, message.Duration);

                // fire and forget the call to notify all listeners
                _notificationService.NotifyAllListeners(new Guid(sessionId), new Guid(messageId));

                return Created(new Uri(Url.Link("ExpirePublication", new { sessionId, messageId })), new Message { Id = messageId, Type = MessageType.Publication });
            }
            catch (XmlException)
            {
                return UnprocessableEntity(new { message = "Failed to format message content as XML. Please send the XML content as valid XML." });
            }
            catch (SessionFaultException e)
            {
                return HandleSessionFault(e);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("{sessionId}/publications/{messageId}", Name = "ExpirePublication")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult ExpirePublication(string sessionId, string messageId)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            if (!messageId.IsGuid())
            {
                return BadRequest(new { message = "Invalid message ID format." });
            }
            try
            {
                _providerPublicationService.ExpirePublication(sessionId, messageId);
                return NoContent();
            }
            catch (SessionFaultException e)
            {
                return HandleSessionFault(e);
            }
        }

        [HttpDelete("{sessionId}", Name = "CloseSession")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult CloseSession(string sessionId)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            try
            {
                var session = this._dbContext.Set<OpenMSM.Data.Models.Session>().FirstOrDefault(m => m.Id == new Guid(sessionId));
                if (session == null)
                {
                    return NotFound(new { message = "A session with the specified ID does not exist." });
                }
                switch (session.Type)
                {
                    case Data.Models.SessionType.Publisher:
                        _providerPublicationService.ClosePublicationSession(sessionId);
                        return NoContent();
                    case Data.Models.SessionType.Subscriber:
                        _consumerPublicationService.CloseSubscriptionSession(sessionId);
                        return NoContent();
                    case Data.Models.SessionType.Requester:
                        _consumerRequestService.CloseConsumerRequestSession(sessionId);
                        return NoContent();
                    case Data.Models.SessionType.Responder:
                        _providerRequestService.CloseProviderRequestSession(sessionId);
                        return NoContent();
                    default:
                        return NotFound();
                }
            }
            catch (Exception e)
            {
                if (e.Message.IndexOf("Provided header security token") >= 0)
                {
                    return Unauthorized(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
        }

        [HttpGet("{sessionId}/publication")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult ReadPublication(string sessionId)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            try
            {
                var message = _consumerPublicationService.ReadPublication(sessionId);
                if (message != null)
                {
                    var retval = new OpenMSM.Web.Models.Message
                    {
                        Id = message.MessageID,
                        Content = message.MessageContent.OuterXml,
                        Topics = message.Topic,
                        Type = MessageType.Publication
                    };
                    this.Response.Headers.Add("OpenMSM-MessageID", retval.Id);
                    this.Response.Headers.Add("OpenMSM-Topic", retval.Topics.Any() ? retval.Topics.Aggregate((last, next) => last + ", " + next) : "");
                    return Ok(retval);
                }
                return Ok(new { });
            }
            catch (SessionFaultException e)
            {
                return HandleSessionFault(e);
            }
        }

        [HttpDelete("{sessionId}/publication")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult RemovePublication(string sessionId)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            try
            {
                _consumerPublicationService.RemovePublication(sessionId);
                return NoContent();
            }
            catch (SessionFaultException e)
            {
                return HandleSessionFault(e);
            }
        }

        [HttpGet("{sessionId}/request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult ReadRequest(string sessionId)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            try
            {
                var message = _providerRequestService.ReadRequest(sessionId);
                if (message != null)
                {
                    var retval = new OpenMSM.Web.Models.Message
                    {
                        Id = message.MessageID,
                        Content = message.MessageContent.OuterXml,
                        Topics = new[] { message.Topic },
                        Type = MessageType.Publication
                    };
                    this.Response.Headers.Add("OpenMSM-MessageID", retval.Id);
                    this.Response.Headers.Add("OpenMSM-Topic", retval.Topics.Any() ? retval.Topics.Aggregate((last, next) => last + ", " + next) : "");
                    return Ok(retval);
                }
                return Ok(new { });
            }
            catch (SessionFaultException e)
            {
                return HandleSessionFault(e);
            }
        }

        [HttpDelete("{sessionId}/request")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult RemoveRequest(string sessionId)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            try
            {
                _providerRequestService.RemoveRequest(sessionId);
                return NoContent();
            }
            catch (SessionFaultException e)
            {
                return HandleSessionFault(e);
            }
        }

        [HttpPost("{sessionId}/responses")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult PostResponse(string sessionId, string requestMessageId, [FromBody]Message message)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            if (!requestMessageId.IsGuid())
            {
                return BadRequest(new { message = "Invalid request message ID format." });
            }
            if (message == null)
            {
                return BadRequest(new { message = "Malformed message object in HTTP body." });
            }
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(message.Content);
                var messageId = _providerRequestService.PostResponse(sessionId, requestMessageId, doc.DocumentElement);

                // fire and forget the call to notify all listeners
                _notificationService.NotifyAllListeners(new Guid(sessionId), new Guid(messageId));

                return Created(new Uri(Url.Link("ExpirePublication", new { sessionId, messageId })), new Message { Id = messageId, Type = MessageType.Response });
            }
            catch (XmlException)
            {
                return UnprocessableEntity(new { message = "Failed to format message content as XML. Please send the XML content as valid XML." });
            }
            catch (SessionFaultException e)
            {
                return HandleSessionFault(e);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("{sessionId}/requests")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult PostRequest(string sessionId, [FromBody]Message message)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            if (message == null)
            {
                return BadRequest(new { message = "Malformed message object in HTTP body." });
            }
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(message.Content);
                var messageId = _consumerRequestService.PostRequest(sessionId, doc.DocumentElement, message.Topics.FirstOrDefault(), message.Duration);

                // fire and forget the call to notify all listeners
                _notificationService.NotifyAllListeners(new Guid(sessionId), new Guid(messageId));

                return Created(new Uri(Url.Link("ExpireRequest", new { sessionId, messageId })), new Message { Id = messageId, Type = MessageType.Request});
            }
            catch (XmlException)
            {
                return UnprocessableEntity(new { message = "Failed to format message content as XML. Please send the XML content as valid XML." });
            }
            catch (SessionFaultException e)
            {
                return HandleSessionFault(e);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("{sessionId}/requests/{messageId}", Name = "ExpireRequest")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult ExpireRequest(string sessionId, string messageId)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            if (!messageId.IsGuid())
            {
                return BadRequest(new { message = "Invalid message ID format." });
            }
            try
            {
                _consumerRequestService.ExpireRequest(sessionId, messageId);
                return NoContent();
            }
            catch (SessionFaultException e)
            {
                return HandleSessionFault(e);
            }
        }

        [HttpGet("{sessionId}/responses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult ReadResponse(string sessionId, string requestMessageId)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            try
            {
                var message = _consumerRequestService.ReadResponse(sessionId, requestMessageId);
                if (message != null)
                {
                    var retval = new OpenMSM.Web.Models.Message
                    {
                        Id = message.MessageID,
                        Content = message.MessageContent.OuterXml,
                        Type = MessageType.Response
                    };
                    this.Response.Headers.Add("OpenMSM-MessageID", retval.Id);
                    this.Response.Headers.Add("OpenMSM-Topic", string.Empty);
                    return Ok(retval);
                }
                return Ok(new { });
            }
            catch (SessionFaultException e)
            {
                return HandleSessionFault(e);
            }
        }

        [HttpDelete("{sessionId}/responses/{requestMessageId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult RemoveResponse(string sessionId, string requestMessageId)
        {
            if (!sessionId.IsGuid())
            {
                return BadRequest(new { message = "Invalid session ID format." });
            }
            if (!requestMessageId.IsGuid())
            {
                return BadRequest(new { message = "Invalid request message ID format." });
            }
            try
            {
                _consumerRequestService.RemoveResponse(sessionId, requestMessageId);
                return NoContent();
            }
            catch (SessionFaultException e)
            {
                return HandleSessionFault(e);
            }
        }
    }
}