using api.Handlers.Hello;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("member")]
    public class MemberController(ILogger<MemberController> logger, ISender mediator) : ControllerBase
    {
        private readonly ILogger<MemberController> _logger = logger;
    
        [HttpGet(Name = "SayHello")]
        public async Task<string> Get() => 
            await mediator.Send(new HelloHandler.HelloCommand());
    }
}