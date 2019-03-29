using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenMSM.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using OpenMSM.Data;

namespace OpenMSM.Web.Controllers
{
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private AppDbContext _dbContext { get; set; }

        public AdminController(AppDbContext appDbContext, IMapper mapper) : base(mapper)
        {
            this._dbContext = appDbContext;
        }

        [HttpGet("configuration")]
        public IActionResult GetConfiguration()
        {
            var config = _dbContext.Configuration;
            if (config == null)
            {
                config = new Data.Models.Configuration();
            }
            return Ok(config);
        }

        [HttpPost("configuration")]
        public IActionResult UpdateConfiguration([FromBody]OpenMSM.Data.Models.Configuration configuration)
        {
            var config = _dbContext.Configuration;
            if (config == null)
            {
                config = new Data.Models.Configuration();
                this._dbContext.Add(config);
            }
            config.NumberOfMessagesToStore = configuration.NumberOfMessagesToStore;
            config.StoreLogMessages = configuration.StoreLogMessages;
            _dbContext.SaveChanges();
            return NoContent();
        }
    }
}
