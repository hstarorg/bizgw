using System.ComponentModel.DataAnnotations;

namespace GatewayServer.ControlPlane.Dtos
{
    /// <summary>集群响应。健康检查字段用清晰命名(实体里是 Helth 拼写)。</summary>
    public sealed class ClusterDto
    {
        public long Id { get; set; }
        public string ClusterCode { get; set; } = "";
        public string ClusterName { get; set; } = "";
        public string LoadBalancingPolicy { get; set; } = "";
        public bool EnabledHealthCheck { get; set; }
        public int HealthCheckInterval { get; set; }
        public int HealthCheckTimeout { get; set; }
        public string HealthCheckPolicy { get; set; } = "";
        public string HealthCheckPath { get; set; } = "";
        public string Remark { get; set; } = "";
        /// <summary>被多少条路由引用(供删除前预警)。</summary>
        public int UsedByRouteCount { get; set; }
        public long CreateDate { get; set; }
        public long ModifyDate { get; set; }
        public string CreatorName { get; set; } = "";
        public string ModifierName { get; set; } = "";
    }

    /// <summary>更新集群(cluster_code 不可改,故不含)。</summary>
    public class ClusterUpdateRequest
    {
        public string ClusterName { get; set; } = "";
        public string LoadBalancingPolicy { get; set; } = "RoundRobin";
        public bool EnabledHealthCheck { get; set; }
        public int HealthCheckInterval { get; set; }
        public int HealthCheckTimeout { get; set; }
        public string HealthCheckPolicy { get; set; } = "";
        public string HealthCheckPath { get; set; } = "";
        public string Remark { get; set; } = "";
    }

    /// <summary>新增集群(带稳定标识 cluster_code)。</summary>
    public sealed class ClusterCreateRequest : ClusterUpdateRequest
    {
        [Required] public string ClusterCode { get; set; } = "";
    }
}
