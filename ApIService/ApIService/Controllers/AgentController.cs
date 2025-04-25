using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private INumberService _numberService;
        
        public AgentController(INumberService numberService)
        {
            _numberService = numberService;
        }
        
        [HttpGet]
        public ActionResult<int> Get()
        {
            var number = _numberService.GetNumber();
            return Ok(number);
        }
    }
}
