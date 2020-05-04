using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Core_all.Controllers.Logging;
using Microsoft.Extensions.Logging;

namespace Core_all.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private ILogger _logger;
       
        public ErrorController(ILogger<ErrorController> logger)
        {
           
            _logger = logger;
        }

       
        [Route("Error/500")]
        public IActionResult Error() {

            var exceptionhandlerpathfeatures = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            // exceptionhandlerpathfeatures.Error.StackTrace
            Exception ex = exceptionhandlerpathfeatures.Error;
           
            string requestid = HttpContext.Request.Headers["requestId"].ToString();

            _logger.LogError( ex, "Application_level_Error_Start-");

            _logger.LogError( "AppError End");

            string errorinfo = "Sorry, seems some internal exception RequestID :" + requestid;

            return new JsonResult(errorinfo);
        }

    }
}