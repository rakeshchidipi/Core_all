using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Core_all.Controllers.Logging;

namespace Core_all.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(ErrorHandlingFilter))]
    //[Authorize]
    public class EmpController : ControllerBase
    {
        private IHostingEnvironment _env;
        private ILogger<EmpController> _logger;

        public EmpController(IHostingEnvironment env,ILogger<EmpController> logger)
        {
            _env = env;
            _logger = logger;
        }

        [HttpGet]
        [Route("name/{name}")]
        public string name(string name) {

            _logger.LogInformation(name);

            _logger.LogDebug ("debug");

            _logger.LogError("error");

            //int i = 0;
            //int j = 10;
            //int k = j / i;

            return _env.EnvironmentName;
        }
    }
}