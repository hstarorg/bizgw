using GatewayServer.Data.Entities;

namespace GatewayServer.Data
{
    /// <summary>
    /// 配置快照文档的契约:发布期把规范化配置序列化成它,数据面反序列化它。
    /// 控制面与数据面只通过此契约相交;字段变更时升 <see cref="ConfigSnapshotDoc.CurrentSchemaVersion"/>。
    /// </summary>
    public class ConfigSnapshotDoc
    {
        /// <summary>当前文档结构版本。</summary>
        public const int CurrentSchemaVersion = 1;

        public List<RouteEntity> Routes { get; set; } = new();

        public List<ClusterEntity> Clusters { get; set; } = new();

        public List<ClusterDestinationEntity> Destinations { get; set; } = new();
    }
}
