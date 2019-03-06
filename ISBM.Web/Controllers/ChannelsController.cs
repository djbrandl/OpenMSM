using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using AutoMapper;
using ISBM.Data;
using ISBM.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ISBM.Web.Controllers
{
    [Route("api/[controller]")]
    public class ChannelsController : ControllerBase
    {
        private XmlElement _accessToken { get; set; }

        public ChannelsController(AppDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
            var t = new XmlDocument();
            t.LoadXml("<Root><Username>Foo</Username><Password>Bar</Password></Root>");
            _accessToken = t.DocumentElement;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<ServiceDefinitions.Channel> Get()
        {
            var service = new ChannelManagementService(dbContext, mapper);
            service.SetAccessToken(_accessToken.OuterXml);
            //service.CreateChannel("Foo", ServiceDefinitions.ChannelType.Publication, "Bar", new[] { _accessToken });
            return service.GetChannels();
        }

        // get channel
        [HttpGet("{id}")]
        public ServiceDefinitions.Channel Get(int id)
        {
            var header = this.Request.Headers.FirstOrDefault(m => m.Key == "Authorization");

            var dto = new Data.Models.Channel
            {
                Id = Guid.NewGuid(),
                URI = "Foo 123"
            };
            var mapped = mapper.Map<ServiceDefinitions.Channel>(dto);
            return mapped;
        }

        // create channel
        [HttpPost]
        public void Post([FromBody]string value)
        {

        }

        // delete channel
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }

        [HttpPost]
        public void SecurityTokens(string id, [FromBody]string[] securityTokens)
        {

        }

        [HttpDelete("{id}")]
        public void SecurityTokens(string id)
        {

        }
    }
}
