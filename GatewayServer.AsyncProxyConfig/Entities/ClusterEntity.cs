using SqlSugar;

namespace GatewayServer.AsyncProxyConfig.Entities
{
    [SugarTable("cluster")]
    public class ClusterEntity : EntityBase
    {
        public ClusterEntity()
        {
            this.ClusterDestinations = new List<ClusterDestinationEntity>();
        }

        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(ColumnName = "cluster_code")]
        public string ClusterCode { get; set; } = "";

        [SugarColumn(ColumnName = "cluster_name")]
        public string ClusterName { get; set; } = "";

        [SugarColumn(ColumnName = "load_balancing_policy")]
        public string LoadBalancingPolicy { get; set; } = "";

        #region 健康检查相关

        [SugarColumn(ColumnName = "enabled_helth_check")]
        public short EnabledHelthCheck { get; set; }

        [SugarColumn(ColumnName = "helth_check_interval")]
        public int HelthCheckInterval { get; set; }

        [SugarColumn(ColumnName = "helth_check_timeout")]
        public int HelthCheckTimeout { get; set; }

        [SugarColumn(ColumnName = "helth_check_policy")]
        public string HelthCheckPolicy { get; set; } = "";

        [SugarColumn(ColumnName = "helth_check_path")]
        public string HelthCheckPath { get; set; } = "";

        #endregion

        [SugarColumn(IsIgnore = true)]
        public virtual IList<ClusterDestinationEntity> ClusterDestinations { get; }

        [SugarColumn(ColumnName = "is_deleted")]
        public short IsDeleted { get; set; }

        [SugarColumn(ColumnName = "remark")]
        public string Remark { get; set; } = "";
    }
}
