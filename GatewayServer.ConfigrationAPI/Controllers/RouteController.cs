using GatewayServer.ConfigrationAPI.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GatewayServer.ConfigrationAPI.Controllers
{
    [ApiController]
    [Route("api/routes")]
    public class RouteController : ControllerBase
    {
        private readonly ILogger<RouteController> _logger;

        public RouteController(ILogger<RouteController> logger)
        {
            _logger = logger;
        }

        [HttpGet("")]
        public IEnumerable<RouteDto> Query(RouteQueryDto queryDto)
        {
            return new List<RouteDto>();
        }

        [HttpGet("{id}")]
        public RouteDto GetDetail()
        {
            return new RouteDto();
        }


        [HttpPost("")]
        public bool CreateRoute(RouteDto routeDto)
        {
            return true;
        }

        [HttpPut("{id}")]
        public bool UpdateRoute(RouteDto routeDto)
        {
            return true;
        }
    }
}
