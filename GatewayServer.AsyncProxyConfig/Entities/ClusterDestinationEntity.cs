using SqlSugar;

namespace GatewayServer.AsyncProxyConfig.Entities
{
    [SugarTable("destination")]
    public class ClusterDestinationEntity : EntityBase
    {
        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 关联的集群
        /// </summary>
        [SugarColumn(ColumnName = "cluster_code")]
        public string ClusterCode { get; set; } = "";

        /// <summary>
        /// 匹配地址
        /// </summary>
        [SugarColumn(ColumnName = "address")]
        public string Address { get; set; } = "";

        /// <summary>
        /// 健康检查地址
        /// </summary>
        [SugarColumn(ColumnName = "helth_check_path")]
        public string HelthCheckPath { get; set; } = "";

        [SugarColumn(ColumnName = "is_deleted")]
        public short IsDeleted { get; set; }

        [SugarColumn(ColumnName = "name")]
        public string Name { get; set; } = "";
    }
}
