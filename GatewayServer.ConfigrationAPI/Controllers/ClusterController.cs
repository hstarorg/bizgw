using GatewayServer.ConfigrationAPI.BLL;
using GatewayServer.ConfigrationAPI.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GatewayServer.ConfigrationAPI.Controllers
{
    [ApiController]
    [Route("api/clusters")]
    public class ClusterController : ControllerBase
    {

        private readonly ILogger<ClusterController> _logger;
        private readonly ClusterBll clusterBll;

        public ClusterController(ILogger<ClusterController> logger, ClusterBll clusterBll)
        {
            _logger = logger;
            this.clusterBll = clusterBll;
        }

        [HttpGet("")]
        public IEnumerable<ClusterDto> Query(ClusterQueryDto queryDto)
        {
            return new List<ClusterDto>();
        }

        [HttpGet("{id}")]
        public ClusterDto GetDetail()
        {
            return new ClusterDto();
        }


        [HttpPost("")]
        public async Task<bool> CreateCluster(ClusterDto clusterDto)
        {
            return await this.clusterBll.DoCreateCluster();
        }

        [HttpPut("{id}")]
        public bool UpdateCluster(ClusterDto clusterDto)
        {
            return true;
        }
    }
}