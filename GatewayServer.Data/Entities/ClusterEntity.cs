using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GatewayServer.Data.Entities
{
    [Table("cluster")]
    public class ClusterEntity : EntityBase
    {
        public ClusterEntity()
        {
            this.ClusterDestinations = new List<ClusterDestinationEntity>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("cluster_code")]
        public string ClusterCode { get; set; } = "";

        [Column("cluster_name")]
        public string ClusterName { get; set; } = "";

        [Column("load_balancing_policy")]
        public string LoadBalancingPolicy { get; set; } = "";

        #region 健康检查相关

        [Column("enabled_health_check")]
        public short EnabledHealthCheck { get; set; }

        [Column("health_check_interval")]
        public int HealthCheckInterval { get; set; }

        [Column("health_check_timeout")]
        public int HealthCheckTimeout { get; set; }

        [Column("health_check_policy")]
        public string HealthCheckPolicy { get; set; } = "";

        [Column("health_check_path")]
        public string HealthCheckPath { get; set; } = "";

        #endregion

        /// <summary>
        /// 集群下的目标服务器，非数据库字段，运行时按 cluster_code 填充(快照文档不含此字段)
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public virtual IList<ClusterDestinationEntity> ClusterDestinations { get; }

        [Column("is_deleted")]
        public short IsDeleted { get; set; }

        [Column("remark")]
        public string Remark { get; set; } = "";
    }
}
