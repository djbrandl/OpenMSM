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
using Microsoft.EntityFrameworkCore;

namespace ISBM.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private ProviderPublicationService _providerPublicationService { get; set; }
        private ConsumerPublicationService _consumerPublicationService { get; set; }
        private NotificationService _notificationService { get; set; }
        private AppDbContext _dbContext { get; set; }

        //public SessionsController(AppDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        //{
        //    _providerPublicationService = new ProviderPublicationService(dbContext, mapper);
        //    _consumerPublicationService = new ConsumerPublicationService(dbContext, mapper);
        //    this.servicesList.Add(_providerPublicationService);
        //    this.servicesList.Add(_consumerPublicationService);
        //}

        public SessionsController(ProviderPublicationService providerPublicationService, ConsumerPublicationService consumerPublicationService, NotificationService notificationService, AppDbContext dbContext, IMapper mapper) : base(mapper)
        {
            this._dbContext = dbContext;
            this._providerPublicationService = providerPublicationService;
            this._consumerPublicationService = consumerPublicationService;
            this._notificationService = notificationService;
            this.ServicesList.Add(_providerPublicationService);
            this.ServicesList.Add(_consumerPublicationService);
        }

    #region Private Methods

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
        }

        [HttpDelete("{sessionId}", Name = "ClosePublicationSession")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ClosePublicationSession(string sessionId, string messageId)
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
                var session = this._dbContext.Set<ISBM.Data.Models.Session>().FirstOrDefault(m => m.Id == new Guid(sessionId));
                if (session == null)
                {
                    return NotFound(new { message = "A session with the specified ID does not exist." });
                }
                if (session.Type == Data.Models.SessionType.Publisher)
                {
                    _providerPublicationService.ClosePublicationSession(sessionId);
                    return NoContent();
                }
                else if (session.Type == Data.Models.SessionType.Subscriber)
                {
                    _consumerPublicationService.CloseSubscriptionSession(sessionId);
                    return NoContent();
                }
                return NotFound();
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
                    var retval = new ISBM.Web.Models.Message
                    {
                        Id = message.MessageID,
                        Content = message.MessageContent.OuterXml,
                        Topics = message.Topic,
                        Type = MessageType.Publication
                    };
                    this.Response.Headers.Add("ISBM-MessageID", retval.Id);
                    this.Response.Headers.Add("ISBM-Topic", retval.Topics.Any() ? retval.Topics.Aggregate((last, next) => last + ", " + next) : "");
                    return Ok(retval);
                }
                return Ok();
            }
            catch (SessionFaultException e)
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
        }
    }
}