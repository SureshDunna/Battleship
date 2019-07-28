using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Battleship
{
    [Route("error")]
    public class ErrorController : Controller
    {        
        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = feature?.Error;

            return exception != null ? StatusCode((int)HttpStatusCode.InternalServerError) : Ok();
        }
    }
}