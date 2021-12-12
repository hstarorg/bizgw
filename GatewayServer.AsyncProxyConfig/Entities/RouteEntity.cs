using SqlSugar;

namespace GatewayServer.AsyncProxyConfig.Entities
{
    [SugarTable("route")]
    public class RouteEntity : EntityBase
    {
        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(ColumnName = "route_name")]
        public string RouteName { get; set; } = "";

        /// <summary>
        /// 要关联到的集群
        /// </summary>
        [SugarColumn(ColumnName = "cluster_code")]
        public string ClusterCode { get; set; } = "";

        [SugarColumn(ColumnName = "match_path")]
        public string MatchPath { get; set; } = "";

        /// <summary>
        /// 允许的 Method，竖线分割
        /// </summary>
        [SugarColumn(ColumnName = "match_methods")]
        public string MatchMethods { get; set; } = "";


        [SugarColumn(ColumnName = "transforms")]
        public string Transforms { get; set; } = "";

        /// <summary>
        /// 是否删除，逻辑删
        /// </summary>
        [SugarColumn(ColumnName = "is_deleted")]
        public short IsDeleted { get; set; } = 0;

        [SugarColumn(ColumnName = "remark")]
        public string Remark { get; set; } = "";
    }
}
