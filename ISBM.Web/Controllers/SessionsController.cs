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
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private ProviderPublicationService _providerPublicationService { get; set; }
        private ConsumerPublicationService _consumerPublicationService { get; set; }

        public SessionsController(AppDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
            _providerPublicationService = new ProviderPublicationService(dbContext, mapper);
            _consumerPublicationService = new ConsumerPublicationService(dbContext, mapper);
            this.servicesList.Add(_providerPublicationService);
            this.servicesList.Add(_consumerPublicationService);
        }

        #region Private Methods

        #endregion

        // TODO: Doing this differently where the entire message topic + expiry is in the POST body, not in the URL... Why are these split and seemingly arbitrarily?
        [HttpPost("{sessionId}/publications")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult PostPublication(string sessionId, [FromBody]Message message)
        {
            if (message == null)
            {
                return BadRequest(new { message = "Malformed message object in HTTP body." });
            }
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(message.Content);
                var messageId = _providerPublicationService.PostPublication(sessionId, doc.DocumentElement, message.Topics, message.Duration);
                return Created(new Uri(Url.Link("ExpirePublication", new { sessionId, messageId })), new Message { Id = messageId });
            }
            catch (XmlException)
            {
                return UnprocessableEntity(new { message = "Failed to format message content as XML. Please send the XML content as valid XML." });
            }
            catch (SessionFaultException e)
            {
                if (e.Message.IndexOf("is not of the correct type") >= 0)
                {
                    return UnprocessableEntity(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
        }

        [HttpPost("{sessionId}/publications/{messageId}", Name = "ExpirePublication")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult ExpirePublication(string sessionId, string messageId)
        {
            try
            {
                _providerPublicationService.ExpirePublication(sessionId, messageId);
                return NoContent();
            }
            catch (SessionFaultException e)
            {
                if (e.Message.IndexOf("is not of the correct type") >= 0)
                {
                    return UnprocessableEntity(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
        }

        [HttpDelete("{sessionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ClosePublicationSession(string sessionId, string messageId)
        {
            try
            {
                var session = this.dbContext.Set<ISBM.Data.Models.Session>().FirstOrDefault(m => m.Id == new Guid(sessionId));
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
            catch (SessionFaultException e)
            {
                return NotFound(new { message = e.Message });
            }
        }

        [HttpGet("{sessionId}/publication")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult ReadPublication(string sessionId)
        {
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
                    this.Response.Headers.Add("ISBM-Topic", retval.Topics.Aggregate((last, next) => last + ", " + next));
                    return Ok(retval);
                }
                return Ok();
            }
            catch (SessionFaultException e)
            {
                if (e.Message.IndexOf("is not of the correct type") >= 0)
                {
                    return UnprocessableEntity(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
        }


        [HttpDelete("{sessionId}/publication")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult RemovePublication(string sessionId)
        {
            try
            {
                _consumerPublicationService.RemovePublication(sessionId);
                return NoContent();
            }
            catch (SessionFaultException e)
            {
                if (e.Message.IndexOf("is not of the correct type") >= 0)
                {
                    return UnprocessableEntity(new { message = e.Message });
                }
                return NotFound(new { message = e.Message });
            }
        }
    }
}