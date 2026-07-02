using System.ComponentModel.DataAnnotations;

namespace GatewayServer.ControlPlane.Dtos
{
    public sealed class DestinationDto
    {
        public long Id { get; set; }
        public string ClusterCode { get; set; } = "";
        public string Address { get; set; } = "";
        public string HealthCheckPath { get; set; } = "";
        public string Name { get; set; } = "";
        public long CreateDate { get; set; }
        public long ModifyDate { get; set; }
        public string CreatorName { get; set; } = "";
        public string ModifierName { get; set; } = "";
    }

    /// <summary>新增/编辑目标(cluster_code 来自 URL)。</summary>
    public sealed class DestinationUpsertRequest
    {
        [Required] public string Address { get; set; } = "";
        public string HealthCheckPath { get; set; } = "";
        public string Name { get; set; } = "";
    }
}
