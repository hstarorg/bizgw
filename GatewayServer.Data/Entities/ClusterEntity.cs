using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Column("enabled_helth_check")]
        public short EnabledHelthCheck { get; set; }

        [Column("helth_check_interval")]
        public int HelthCheckInterval { get; set; }

        [Column("helth_check_timeout")]
        public int HelthCheckTimeout { get; set; }

        [Column("helth_check_policy")]
        public string HelthCheckPolicy { get; set; } = "";

        [Column("helth_check_path")]
        public string HelthCheckPath { get; set; } = "";

        #endregion

        /// <summary>
        /// 集群下的目标服务器，非数据库字段，由 DbProxyConfigHelper 按 cluster_code 填充
        /// </summary>
        [NotMapped]
        public virtual IList<ClusterDestinationEntity> ClusterDestinations { get; }

        [Column("is_deleted")]
        public short IsDeleted { get; set; }

        [Column("remark")]
        public string Remark { get; set; } = "";
    }
}
